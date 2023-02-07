using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Xenophyte_Connector_All.Setting;

namespace Xenophyte_Connector_All.Utils
{
    public class CheckTcp
    {
        /// <summary>
        ///     check tcp port.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static async Task<bool> CheckTcpClientAsync(IPAddress host, int port)
        {
            try
            {    

                return await ConnectToTarget(host, port);
            }
            catch
            {
                return false;
            }
        }


        private static async Task<bool> ConnectToTarget(IPAddress host, int port)
        {

            using (var client = new Socket(host.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {

                Console.WriteLine("Try to connect to: " + host.ToString());

                Task clientTask = client.ConnectAsync(host, port);

                var timeoutConnect = ClassConnectorSetting.MaxTimeoutConnectRemoteNode;
                if (host.ToString() == "127.0.0.1" || host.ToString() == "localhost")
                {
                    timeoutConnect = ClassConnectorSetting.MaxTimeoutConnectLocalhostRemoteNode;
                }
                var delayTask = Task.Delay(timeoutConnect);

                var completedTask = await Task.WhenAny(new[] { clientTask, delayTask });
                return completedTask == clientTask;
            }
        }
    }
}