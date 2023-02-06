using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Xenophyte_Connector_All.Setting;

namespace Xenophyte_Connector_All.Utils
{
    public class CheckPing
    {
        /// <summary>
        ///     Check the ping from host and return the ping time.
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public static int CheckPingHost(IPAddress host, bool checkSeed = false)
        {
            try
            {
                using (var pingTestNode = new Ping())
                {


            

                    PingReply replyNode = pingTestNode.Send(host);

                    if (replyNode.Status == IPStatus.Success)
                    {
                        if (ClassConnectorSetting.PriorityToIpV6)
                        {
                            if (host.AddressFamily == AddressFamily.InterNetworkV6)
                               return (int)replyNode.RoundtripTime - ClassConnectorSetting.PriorityIpV6ElapsedMillisecond;
                        }
                        return (int)replyNode.RoundtripTime;
                    }
                    else
                    {
                        if (checkSeed)
                        {
                            return ClassConnectorSetting.MaxTimeoutConnect;
                        }
                        else
                        {
                            return ClassConnectorSetting.MaxTimeoutConnectRemoteNode;
                        }
                    }
                }
            }
            catch
            {
                if (checkSeed)
                {
                    return ClassConnectorSetting.MaxTimeoutConnect;
                }
                else
                {
                    return ClassConnectorSetting.MaxTimeoutConnectRemoteNode;
                }
            }
        }
    }
}