using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using IX.StandardExtensions.ComponentModel;
using IX.StandardExtensions.Contracts;
using IX.StandardExtensions.EventModel;
using IX.StandardExtensions.Threading;
using JetBrains.Annotations;

namespace IX.IPC.Core.Sockets
{
    /// <summary>
    /// A socket that can send and receive messages and that is built with high performance in mind.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message.</typeparam>
    /// <seealso cref="IX.StandardExtensions.ComponentModel.DisposableBase" />
    /// <seealso cref="IX.IPC.Core.Sockets.ISimpleMessageCommunicator{TMessage}" />
    [PublicAPI]
    public abstract class HighPerformanceSimpleMessageSocket<TMessage> : DisposableBase, ISimpleMessageCommunicator<TMessage>
    {
        private readonly Socket remoteParty;
        private readonly CancellationToken cancellationToken;

        private readonly DataContractSerializer dcs;

        private int closedSwitch;
        private int lastError;

        protected private HighPerformanceSimpleMessageSocket([NotNull] Socket remoteParty, CancellationToken cancellationToken)
        {
            Contract.RequiresNotNull(
                ref this.remoteParty,
                remoteParty,
                nameof(remoteParty));

            this.cancellationToken = cancellationToken;

            this.dcs = new DataContractSerializer(typeof(TMessage));

            Fire.AndForget(
                this.ReceiveThread,
                cancellationToken);
        }

        /// <summary>
        /// Occurs when a message is received.
        /// </summary>
        public event EventHandler<ContextObjectEventArgs<TMessage>> MessageReceived;

        /// <summary>
        /// Occurs when the remote party has disconnected.
        /// </summary>
        public event EventHandler Disconnected;

        /// <summary>
        /// Occurs when there is a communication error.
        /// </summary>
        public event EventHandler<ContextObjectEventArgs<int>> CommunicationError;

        /// <summary>
        /// Gets the remote end point.
        /// </summary>
        /// <value>
        /// The remote end point.
        /// </value>
        public EndPoint RemoteEndPoint => this.remoteParty.RemoteEndPoint;

        /// <summary>
        /// Attempts to send a message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>
        ///   <see langword="true" /> if sending was successful, <see langword="false" /> otherwise.
        /// </returns>
        public bool TrySendMessage(TMessage message)
        {
            this.RequiresNotDisposed();

            if (this.closedSwitch != 0)
            {
                return false;
            }

            byte[] buffer = this.Serialize(message);
            byte[] length = BitConverter.GetBytes(buffer.Length);

            try
            {
                lock (this)
                {
                    this.remoteParty.Send(length);
                    this.remoteParty.Send(buffer);
                }
            }
            catch (SocketException ex)
            {
                this.CloseSocket();
                this.TriggerCommunicationError(ex.ErrorCode);

                return false;
            }

            return true;
        }

        /// <summary>Disposes in the managed context.</summary>
        protected override void DisposeManagedContext()
        {
            base.DisposeManagedContext();

            this.CloseSocket();
        }

        /// <summary>
        /// Gets the size of the buffer.
        /// </summary>
        /// <returns>The size of the buffer.</returns>
        protected private int GetBufferSize() => EnvironmentSettings.DefaultSocketBufferSize;

        /// <summary>
        /// Triggers the message received event.
        /// </summary>
        /// <param name="message">The message to trigger with.</param>
        protected private void TriggerMessageReceived(TMessage message) =>
            Fire.AndForget(
                (
                    thisL1,
                    messageL1) => thisL1.MessageReceived?.Invoke(
                    thisL1,
                    new ContextObjectEventArgs<TMessage>(messageL1)),
                this,
                message);

        /// <summary>
        /// Triggers the communication error event.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        protected private void TriggerCommunicationError(int errorCode)
        {
            if (errorCode == Interlocked.CompareExchange(
                    ref this.lastError,
                    errorCode,
                    errorCode))
            {
                return;
            }

            // ReSharper disable once MethodSupportsCancellation - We do not want this to cancel
            Fire.AndForget(
                (
                    thisL1,
                    messageL1) => thisL1.CommunicationError?.Invoke(
                    thisL1,
                    new ContextObjectEventArgs<int>(messageL1)), this,
                errorCode);
        }

        private void ReceiveThread()
        {
            while (!this.cancellationToken.IsCancellationRequested && this.closedSwitch == 0)
            {
                byte[] dataBuffer;
                try
                {
                    // Receive length
                    byte[] lengthBuffer = this.ReceiveFixedLength(4);
                    var messageLength = BitConverter.ToInt32(
                        lengthBuffer,
                        0);

                    if (messageLength <= 0)
                    {
                        // Protocol error - message length problem
                        this.CloseSocket();
                        this.TriggerCommunicationError((int)SocketError.ConnectionAborted);
                    }

                    dataBuffer = this.ReceiveFixedLength(messageLength);
                }
                catch (SocketException ex)
                {
                    this.CloseSocket();
                    this.TriggerCommunicationError(ex.ErrorCode);
                    return;
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                this.TriggerMessageReceived(this.Deserialize(dataBuffer));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte[] Serialize([NotNull] TMessage message)
        {
            using (var ms = new MemoryStream())
            {
                this.dcs.WriteObject(ms, message);

                return ms.ToArray();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TMessage Deserialize([NotNull] byte[] buffer)
        {
            using (var ms = new MemoryStream(buffer))
            {
                return (TMessage)this.dcs.ReadObject(ms);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte[] ReceiveFixedLength(int desiredLength)
        {
            var read = 0;
            var buffer = new byte[desiredLength];

            while (read < desiredLength)
            {
                // ReSharper disable once ImpureMethodCallOnReadonlyValueField - MS-recommended way ?
                this.cancellationToken.ThrowIfCancellationRequested();

                read += this.remoteParty.Receive(
                    buffer,
                    read,
                    desiredLength - read,
                    SocketFlags.None);
            }

            return buffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CloseSocket()
        {
            if (Interlocked.CompareExchange(
                    ref this.closedSwitch,
                    1,
                    0) != 0)
            {
                return;
            }

            try
            {
                this.remoteParty.Shutdown(SocketShutdown.Both);
            }
            catch (SocketException ex)
            {
                this.TriggerCommunicationError(ex.ErrorCode);
            }

            try
            {
                this.remoteParty.Close(EnvironmentSettings.DefaultSocketCloseTimeout);
            }
            catch (SocketException ex)
            {
                this.TriggerCommunicationError(ex.ErrorCode);
            }

            // ReSharper disable once MethodSupportsCancellation - We don't really want this cancelled
            Fire.AndForget(
                (thisL1) => thisL1.Disconnected?.Invoke(thisL1, EventArgs.Empty),
                this);
        }
    }
}