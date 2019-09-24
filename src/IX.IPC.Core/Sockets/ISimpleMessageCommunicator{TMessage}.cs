// <copyright file="ISimpleMessageCommunicator.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using System;
using JetBrains.Annotations;

namespace IX.IPC.Core.Sockets
{
    /// <summary>
    /// A service contract for a simple message communicator.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message.</typeparam>
    /// <seealso cref="System.IDisposable" />
    [PublicAPI]
    public interface ISimpleMessageCommunicator<in TMessage> : IDisposable
    {
        /// <summary>
        /// Attempts to send a message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns><see langword="true"/> if sending was successful, <see langword="false"/> otherwise.</returns>
        bool TrySendMessage([NotNull] TMessage message);
    }
}