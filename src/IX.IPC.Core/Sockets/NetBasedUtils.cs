using System.Linq;
using System.Net;
using IX.StandardExtensions.Contracts;
using JetBrains.Annotations;

namespace IX.IPC.Core.Sockets
{
    internal static class NetBasedUtils
    {
        [CanBeNull]
        internal static IPAddress GetAddress(string addressOrHost)
        {
            Contract.RequiresNotNullOrWhitespace(
                addressOrHost,
                nameof(addressOrHost));

            return IPAddress.TryParse(
                addressOrHost,
                out IPAddress possibleAddress)
                ? possibleAddress
                : Dns.GetHostAddresses(addressOrHost).FirstOrDefault();
        }

        internal static IPEndPoint MakeEndpoint(
            IPAddress address,
            int port)
        {
            Contract.RequiresNotNull(
                in address,
                nameof(address));
            Contract.RequiresPositive(
                in port,
                nameof(port));

            return new IPEndPoint(
                address,
                port);
        }
    }
}