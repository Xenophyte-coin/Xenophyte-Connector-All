using System.Diagnostics;
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



                    if (host.AddressFamily != AddressFamily.InterNetworkV6)
                    {
                        PingReply replyNode = pingTestNode.Send(host);

                        if (replyNode.Status == IPStatus.Success)
                        {
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
                    else
                    {
                        int result = 0;
                        bool connectSuccess = false;
                        Stopwatch stopwatch = new Stopwatch();

                        try
                        {
                            using(Socket tcpClient = new Socket(host.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
                            {
                                tcpClient.Connect(host, checkSeed ? ClassConnectorSetting.SeedNodePort : ClassConnectorSetting.RemoteNodePort);
                                connectSuccess = true;
                            }
                        }
                        catch
                        {
                            if (checkSeed)
                                result = ClassConnectorSetting.MaxTimeoutConnect;
                            else
                                result = ClassConnectorSetting.MaxTimeoutConnectRemoteNode;

                        }

                        stopwatch.Stop();

                        if (connectSuccess)
                        {
                            return ClassConnectorSetting.PriorityToIpV6 ?
                                (int)stopwatch.ElapsedMilliseconds - ClassConnectorSetting.PriorityIpV6ElapsedMillisecond :
                                (int)stopwatch.ElapsedMilliseconds;
                        }

                        return result;
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