using SDG.NetTransport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarnSystem.Extensions
{
    public static class ITransportConnectionExtensions
    {
        public static uint GetIPv4AddressOrZero(this ITransportConnection transportConnection)
        {
            if (transportConnection.TryGetIPv4Address(out uint address))
                return address;

            return 0u;
        }
    }
}
