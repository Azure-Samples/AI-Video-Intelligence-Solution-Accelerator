using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace VideoProcessorGrpc
{
    public class IpHelper
    {
        /// <summary>
        /// Returns an IPv4 address in the 172 subnet if found
        /// </summary>
        /// <param name="hostName"></param>
        /// <returns>null if not found</returns>
        public static string Get172SubnetIpV4(string hostName)
        {
            IPAddress[] addresses = Dns.GetHostAddresses(hostName);
            foreach (IPAddress addr in addresses)
            {
                if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    string result = addr.ToString();
                    if (result.StartsWith("172."))
                    {
                        return result;
                    }
                }
            }
            return null;
        }
    }
}
