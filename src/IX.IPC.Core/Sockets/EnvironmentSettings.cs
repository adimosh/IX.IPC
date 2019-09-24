// <copyright file="EnvironmentSettings.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using JetBrains.Annotations;

namespace IX.IPC.Core.Sockets
{
    /// <summary>
    /// Environment settings for the IPC Sockets.
    /// </summary>
    [PublicAPI]
    public static class EnvironmentSettings
    {
        /// <summary>
        /// Gets or sets the default size of the socket buffer.
        /// </summary>
        /// <value>
        /// The default size of the socket buffer.
        /// </value>
        public static int DefaultSocketBufferSize { get; set; } = 1024;

        /// <summary>
        /// Gets or sets the default socket close timeout, in milliseconds.
        /// </summary>
        /// <value>
        /// The default socket close timeout, in milliseconds.
        /// </value>
        public static int DefaultSocketCloseTimeout { get; set; } = 100;
    }
}