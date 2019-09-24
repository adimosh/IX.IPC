// <copyright file="TcpHighPerformanceSimpleMessageSocket{TMessage}.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System.Net;
using System.Net.Sockets;
using System.Threading;
using IX.StandardExtensions.Contracts;
using JetBrains.Annotations;

namespace IX.IPC.Core.Sockets
{
    /// <summary>
    /// The TCP implementation of a high-performance simple message socket.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message.</typeparam>
    /// <seealso cref="IX.IPC.Core.Sockets.HighPerformanceSimpleMessageSocket{TMessage}" />
    [PublicAPI]
    public abstract class TcpHighPerformanceSimpleMessageSocket<TMessage> : HighPerformanceSimpleMessageSocket<TMessage>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TcpHighPerformanceSimpleMessageSocket{TMessage}"/> class.
        /// </summary>
        /// <param name="hostNameOrAddress">The host name or address.</param>
        /// <param name="port">The port.</param>
        protected TcpHighPerformanceSimpleMessageSocket(
            [NotNull] string hostNameOrAddress,
            int port)
            : base(
                InitializeSocketPure(
                    NetBasedUtils.GetAddress(hostNameOrAddress),
                    port), default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpHighPerformanceSimpleMessageSocket{TMessage}"/> class.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        protected TcpHighPerformanceSimpleMessageSocket(
            [NotNull] IPAddress address,
            int port)
            : base(
                InitializeSocketPure(
                    address,
                    port), default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpHighPerformanceSimpleMessageSocket{TMessage}"/> class.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        protected TcpHighPerformanceSimpleMessageSocket([NotNull] IPEndPoint endpoint)
            : base(
                InitializeSocketPure(endpoint),
                default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpHighPerformanceSimpleMessageSocket{TMessage}"/> class.
        /// </summary>
        /// <param name="hostNameOrAddress">The host name or address.</param>
        /// <param name="port">The port.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        protected TcpHighPerformanceSimpleMessageSocket(
            string hostNameOrAddress,
            int port,
            CancellationToken cancellationToken)
            : base(
                InitializeSocketPure(
                    NetBasedUtils.GetAddress(hostNameOrAddress),
                    port), cancellationToken)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpHighPerformanceSimpleMessageSocket{TMessage}"/> class.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        protected TcpHighPerformanceSimpleMessageSocket(
            [NotNull] IPAddress address,
            int port,
            CancellationToken cancellationToken)
            : base(
                InitializeSocketPure(
                    address,
                    port), cancellationToken)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpHighPerformanceSimpleMessageSocket{TMessage}"/> class.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        protected TcpHighPerformanceSimpleMessageSocket(
            [NotNull] IPEndPoint endpoint,
            CancellationToken cancellationToken)
            : base(
                InitializeSocketPure(endpoint),
                cancellationToken)
        {
        }

        [NotNull]
        private static Socket InitializeSocketPure(
            IPAddress address,
            int port) =>
            InitializeSocketPure(
                NetBasedUtils.MakeEndpoint(
                    address,
                    port));

        [NotNull]
        private static Socket InitializeSocketPure(IPEndPoint endpoint)
        {
            Contract.RequiresNotNull(
                in endpoint,
                nameof(endpoint));

            var socket = new Socket(
                endpoint.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            socket.Connect(endpoint);

            return socket;
        }
    }
}