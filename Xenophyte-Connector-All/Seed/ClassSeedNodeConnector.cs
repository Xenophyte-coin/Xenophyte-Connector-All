using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xenophyte_Connector_All.Remote;
using Xenophyte_Connector_All.Setting;
using Xenophyte_Connector_All.Utils;

namespace Xenophyte_Connector_All.Seed
{
    public class ClassSeedNodeConnectorObjectSendPacket : IDisposable
    {
        public byte[] packetByte;
        private bool disposed;

        public ClassSeedNodeConnectorObjectSendPacket(string packet)
        {
            packetByte = ClassUtils.GetByteArrayFromString(packet);
        }

        ~ClassSeedNodeConnectorObjectSendPacket()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                packetByte = null;
            }

            disposed = true;
        }
    }

    public class ClassSeedNodeConnectorObjectPacket : IDisposable
    {
        public byte[] buffer;
        public string packet;
        private bool disposed;

        public ClassSeedNodeConnectorObjectPacket()
        {
            buffer = new byte[ClassConnectorSetting.MaxNetworkPacketSize];
            packet = string.Empty;
        }

        ~ClassSeedNodeConnectorObjectPacket()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                buffer = null;
                packet = null;
            }

            disposed = true;
        }
    }
    public class ClassSeedNodeConnector : IDisposable
    {
        private TcpClient _connector;
        private bool _isConnected;
        private bool disposed;
        private string _currentSeedNodeHost;
        public byte[] AesIvCertificate;
        public byte[] AesSaltCertificate;
        private string _malformedPacket;



        ~ClassSeedNodeConnector()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                _connector = null;
            }
            disposed = true;
        }

        /// <summary>
        ///     Start to connect on the seed node.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> StartConnectToSeedAsync(string host, int port = ClassConnectorSetting.SeedNodePort, bool isLinux = false, int customTimeConnect = ClassConnectorSetting.MaxTimeoutConnect)
        {
            _malformedPacket = string.Empty;

            if (!string.IsNullOrEmpty(host))
            {
#if DEBUG
                Console.WriteLine("Host target: " + host);
#endif
                try
                {
                    _connector = new TcpClient();
                    var connectTask = _connector.ConnectAsync(host, port);
                    var connectTaskDelay = Task.Delay(customTimeConnect);

                    var completedConnectTask = await Task.WhenAny(connectTask, connectTaskDelay);
                    if (completedConnectTask != connectTask)
                    {
                        return false;
                    }
                }

                catch (Exception error)
                {
#if DEBUG
                    Console.WriteLine("Error to connect on manual host node: " + error.Message);
#endif
                    _isConnected = false;
                    return false;
                }

                _isConnected = true;
                _currentSeedNodeHost = host;
                try
                {
                    _connector.SetSocketKeepAliveValues(20 * 60 * 1000, 30 * 1000);
                }
                catch
                {
                    return false;
                }
                return true;
            }

            Dictionary<string, int> listOfSeedNodesSpeed = new Dictionary<string, int>();
            foreach (var seedNode in ClassConnectorSetting.SeedNodeIp)
            {

                try
                {
                    int seedNodeResponseTime = -1;
                    Task taskCheckSeedNode = Task.Run(() => seedNodeResponseTime = CheckPing.CheckPingHost(seedNode.Key, true));
                    taskCheckSeedNode.Wait(ClassConnectorSetting.MaxPingDelay);
                    if (seedNodeResponseTime == -1)
                    {
                        seedNodeResponseTime = ClassConnectorSetting.MaxSeedNodeTimeoutConnect;
                    }
#if DEBUG
                    Console.WriteLine(seedNode.Key + " response time: " + seedNodeResponseTime + " ms.");
#endif
                    listOfSeedNodesSpeed.Add(seedNode.Key, seedNodeResponseTime);

                }
                catch
                {
                    listOfSeedNodesSpeed.Add(seedNode.Key, ClassConnectorSetting.MaxSeedNodeTimeoutConnect); // Max delay.
                }

            }

            listOfSeedNodesSpeed = listOfSeedNodesSpeed.OrderBy(u => u.Value).ToDictionary(z => z.Key, y => y.Value);

            var success = false;
            var seedNodeTested = false;
            foreach (var seedNode in listOfSeedNodesSpeed)
            {
#if DEBUG
                    Console.WriteLine("Seed Node Host target: " + seedNode.Key);
#endif
                try
                {
                    if (ClassConnectorSetting.SeedNodeDisconnectScore.ContainsKey(seedNode.Key))
                    {
                        int totalDisconnection = ClassConnectorSetting.SeedNodeDisconnectScore[seedNode.Key].Item1;
                        long lastDisconnection = ClassConnectorSetting.SeedNodeDisconnectScore[seedNode.Key].Item2;
                        if (lastDisconnection + ClassConnectorSetting.SeedNodeMaxKeepAliveDisconnection < ClassUtils.DateUnixTimeNowSecond())
                        {
                            totalDisconnection = 0;
                            ClassConnectorSetting.SeedNodeDisconnectScore[seedNode.Key] = new Tuple<int, long>(totalDisconnection, lastDisconnection);
                        }
                        if (totalDisconnection < ClassConnectorSetting.SeedNodeMaxDisconnection)
                        {
                            seedNodeTested = true;
                            int maxRetry = 0;
                            while (maxRetry < ClassConnectorSetting.SeedNodeMaxRetry || success)
                            {
                                _connector = new TcpClient();
                                var connectTask = _connector.ConnectAsync(seedNode.Key, port);
                                var connectTaskDelay = Task.Delay(ClassConnectorSetting.MaxSeedNodeTimeoutConnect);

                                var completedConnectTask = await Task.WhenAny(connectTask, connectTaskDelay);
                                if (completedConnectTask == connectTask)
                                {
#if DEBUG
                                        Console.WriteLine("Successfully connected to Seed Node: " + seedNode.Key);
#endif
                                    success = true;
                                    _isConnected = true;
                                    _currentSeedNodeHost = seedNode.Key;
                                    maxRetry = ClassConnectorSetting.SeedNodeMaxRetry;
                                    try
                                    {
                                        _connector.SetSocketKeepAliveValues(20 * 60 * 1000, 30 * 1000);
                                    }
                                    catch
                                    {
                                        return false;
                                    }
                                    await Task.Factory.StartNew(EnableCheckConnectionAsync, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Current).ConfigureAwait(false);

                                    return true;
                                }
                                else
                                {
#if DEBUG
                                        Console.WriteLine("Failed to connect to Seed Node: " + seedNode.Key);
#endif
                                    if (maxRetry >= ClassConnectorSetting.SeedNodeMaxRetry)
                                    {
                                        ClassConnectorSetting.SeedNodeDisconnectScore[seedNode.Key] = new Tuple<int, long>(totalDisconnection + 1, ClassUtils.DateUnixTimeNowSecond());
                                    }
                                }
                                try
                                {
                                    _connector?.Close();
                                    _connector?.Dispose();
                                }
                                catch
                                {

                                }
                                maxRetry++;
                            }
                        }
                        else
                        {
#if DEBUG
                            
                                Console.WriteLine("Max disconnection is reach for seed node: " + seedNode.Key);
                            
#endif
                        }
                    }

                }
                catch (Exception error)
                {
#if DEBUG
                        Console.WriteLine("Error to connect on seed node: " + error.Message);
#endif
                }

            }
            if (!seedNodeTested) // Clean up just in case if every seed node return too much disconnection saved in their counter.
            {
                foreach (var seednode in ClassConnectorSetting.SeedNodeIp)
                {
                    if (ClassConnectorSetting.SeedNodeDisconnectScore.ContainsKey(seednode.Key))
                    {
                        ClassConnectorSetting.SeedNodeDisconnectScore[seednode.Key] = new Tuple<int, long>(0, 0);
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Check the connection opened to the network.
        /// </summary>
        private async Task EnableCheckConnectionAsync()
        {
            while (_isConnected)
            {
                try
                {
                    if (!ClassUtils.SocketIsConnected(_connector))
                    {
                        _isConnected = false;
                        break;
                    }
                }
                catch
                {
                    _isConnected = false;
                    break;
                }
                await Task.Delay(1000);
            }
        }



        /// <summary>
        ///     Send packet to seed node.
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="certificate"></param>
        /// <param name="isSeedNode"></param>
        /// <param name="isEncrypted"></param>
        /// <returns></returns>
        public async Task<bool> SendPacketToSeedNodeAsync(string packet, string certificate, bool isSeedNode = false,  bool isEncrypted = false)
        {
            if (!ReturnStatus())
            {
                return false;
            }
            try
            {

                using (var bufferedNetworkStream = new NetworkStream(_connector.Client))
                {

                    // 10/08/2018 - MAJOR_UPDATE_1_SECURITY
                    if (ClassConnectorSetting.MAJOR_UPDATE_1_SECURITY) // SSL Layer for Send packet.
                    {
                        if (isEncrypted)
                        {
                            if (AesIvCertificate == null)
                            {
                                using (PasswordDeriveBytes password = new PasswordDeriveBytes(certificate, ClassUtils.GetByteArrayFromString(ClassUtils.FromHex(certificate.Substring(0, 8)))))
                                {
                                    AesIvCertificate = password.GetBytes(ClassConnectorSetting.MAJOR_UPDATE_1_SECURITY_CERTIFICATE_SIZE / 8);
                                    AesSaltCertificate = password.GetBytes(16);
                                }
                            }
                            using (ClassSeedNodeConnectorObjectSendPacket packetObject = new ClassSeedNodeConnectorObjectSendPacket(ClassAlgo.GetEncryptedResult(ClassAlgoEnumeration.Rijndael, packet, ClassConnectorSetting.MAJOR_UPDATE_1_SECURITY_CERTIFICATE_SIZE, AesIvCertificate, AesSaltCertificate) + ClassConnectorSetting.PacketSplitSeperator))
                            {
                                await bufferedNetworkStream.WriteAsync(packetObject.packetByte, 0, packetObject.packetByte.Length);
                                await bufferedNetworkStream.FlushAsync();
                            }

                        }
                        else
                        {
                            if (isSeedNode)
                            {
                                using (ClassSeedNodeConnectorObjectSendPacket packetObject = new ClassSeedNodeConnectorObjectSendPacket(packet + ClassConnectorSetting.PacketSplitSeperator))
                                {
                                    await bufferedNetworkStream.WriteAsync(packetObject.packetByte, 0, packetObject.packetByte.Length);
                                    await bufferedNetworkStream.FlushAsync();
                                }
                            }
                            else
                            {
                                using (ClassSeedNodeConnectorObjectSendPacket packetObject = new ClassSeedNodeConnectorObjectSendPacket(packet))
                                {
                                    await bufferedNetworkStream.WriteAsync(packetObject.packetByte, 0, packetObject.packetByte.Length);
                                    await bufferedNetworkStream.FlushAsync();
                                }
                            }

                        }
                    }

                }
            }
            catch (Exception error)
            {
#if DEBUG
                Console.WriteLine("Error to send packet on seed node: " + error.Message);
#endif
                _isConnected = false;
                return false;
            }

            return true;
        }




        /// <summary>
        ///     Listen and return packet from Seed Node.
        /// </summary>
        /// <returns></returns>
        [HostProtection(ExternalThreading = true)]
        public async Task<string> ReceivePacketFromSeedNodeAsync(string certificate, bool isSeedNode = false, bool isEncrypted = false)
        {
            try
            {

                if (ClassConnectorSetting.MAJOR_UPDATE_1_SECURITY) // New Layer for receive packet.
                {
                    using (var bufferPacket = new ClassSeedNodeConnectorObjectPacket())
                    {
                        using (var bufferedNetworkStream = new NetworkStream(_connector.Client))
                        {

                            int received = 0;

                            while ((received = await bufferedNetworkStream.ReadAsync(bufferPacket.buffer, 0, bufferPacket.buffer.Length)) > 0)
                            {
                                if (received > 0)
                                {
                                    
                                    bufferPacket.packet = Encoding.UTF8.GetString(bufferPacket.buffer, 0, received);
                                    
                                    if (bufferPacket.packet != ClassSeedNodeStatus.SeedError && bufferPacket.packet != ClassSeedNodeStatus.SeedNone)
                                    {
                                        if (isEncrypted)
                                        {
                                            if (AesIvCertificate == null)
                                            {
                                                using (PasswordDeriveBytes password = new PasswordDeriveBytes(certificate, ClassUtils.GetByteArrayFromString(ClassUtils.FromHex(certificate.Substring(0, 8)))))
                                                {
                                                    AesIvCertificate = password.GetBytes(ClassConnectorSetting.MAJOR_UPDATE_1_SECURITY_CERTIFICATE_SIZE / 8);
                                                    AesSaltCertificate = password.GetBytes(16);
                                                }
                                            }
                                            if (bufferPacket.packet.Contains(ClassConnectorSetting.PacketSplitSeperator))
                                            {
                                                if (!string.IsNullOrEmpty(_malformedPacket))
                                                {
                                                    bufferPacket.packet = _malformedPacket + bufferPacket.packet;
                                                    _malformedPacket = string.Empty;
                                                }
                                                var splitPacket = bufferPacket.packet.Split(new[] { ClassConnectorSetting.PacketSplitSeperator }, StringSplitOptions.None);
                                                bufferPacket.packet = string.Empty;
                                                foreach (var packetEach in splitPacket)
                                                {
                                                    if (!string.IsNullOrEmpty(packetEach))
                                                    {
                                                        if (packetEach.Length > 1)
                                                        {
                                                            if (packetEach.Contains(ClassRemoteNodeCommand.ClassRemoteNodeRecvFromSeedEnumeration.RemoteSendTransactionPerId))
                                                            {
                                                                bufferPacket.packet += packetEach.Replace(ClassConnectorSetting.PacketSplitSeperator, "") + ClassConnectorSetting.PacketSplitSeperator;
                                                            }
                                                            else
                                                            {

                                                                string packetDecrypt = ClassAlgo.GetDecryptedResult(ClassAlgoEnumeration.Rijndael, packetEach.Replace(ClassConnectorSetting.PacketSplitSeperator, ""), ClassConnectorSetting.MAJOR_UPDATE_1_SECURITY_CERTIFICATE_SIZE, AesIvCertificate, AesSaltCertificate);

                                                                if (packetDecrypt.Contains(ClassSeedNodeCommand.ClassReceiveSeedEnumeration.WalletSendSeedNode))
                                                                {
                                                                    var packetNewSeedNode = packetDecrypt.Replace(ClassSeedNodeCommand.ClassReceiveSeedEnumeration.WalletSendSeedNode, "");
                                                                    packetNewSeedNode = packetNewSeedNode.Replace(ClassConnectorSetting.PacketSplitSeperator, "");
                                                                    var splitPacketNewSeedNode = packetNewSeedNode.Split(new[] { ";" }, StringSplitOptions.None);
                                                                    var newSeedNodeHost = splitPacketNewSeedNode[0];
                                                                    var newSeedNodeCountry = splitPacketNewSeedNode[1];

                                                                    if (!ClassConnectorSetting.SeedNodeIp.ContainsKey(newSeedNodeHost))
                                                                    {
                                                                        ClassConnectorSetting.SeedNodeIp.Add(newSeedNodeHost, new Tuple<string, bool>(newSeedNodeCountry, false));
                                                                    }
                                                                    if (!ClassConnectorSetting.SeedNodeDisconnectScore.ContainsKey(newSeedNodeHost))
                                                                    {
                                                                        ClassConnectorSetting.SeedNodeDisconnectScore.Add(newSeedNodeHost, new Tuple<int, long>(0, 0));
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    bufferPacket.packet += packetDecrypt + ClassConnectorSetting.PacketSplitSeperator;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (!bufferPacket.packet.Contains(ClassRemoteNodeCommand.ClassRemoteNodeRecvFromSeedEnumeration.RemoteSendTransactionPerId))
                                                {
                                                    try
                                                    {
                                                        string packetDecrypt = ClassAlgo.GetDecryptedResult(ClassAlgoEnumeration.Rijndael, bufferPacket.packet, ClassConnectorSetting.MAJOR_UPDATE_1_SECURITY_CERTIFICATE_SIZE, AesIvCertificate, AesSaltCertificate);
 

                                                        if (bufferPacket.packet.Contains(ClassSeedNodeCommand.ClassReceiveSeedEnumeration.WalletSendSeedNode))
                                                        {
                                                            var packetNewSeedNode = packetDecrypt.Replace(ClassSeedNodeCommand.ClassReceiveSeedEnumeration.WalletSendSeedNode, "");
                                                            var splitPacketNewSeedNode = packetNewSeedNode.Split(new[] { ";" }, StringSplitOptions.None);
                                                            var newSeedNodeHost = splitPacketNewSeedNode[0];
                                                            var newSeedNodeCountry = splitPacketNewSeedNode[1];
                                                            if (!ClassConnectorSetting.SeedNodeIp.ContainsKey(newSeedNodeHost))
                                                            {
                                                                ClassConnectorSetting.SeedNodeIp.Add(newSeedNodeHost, new Tuple<string, bool>(newSeedNodeCountry, false));
                                                            }
                                                            if (!ClassConnectorSetting.SeedNodeDisconnectScore.ContainsKey(newSeedNodeHost))
                                                            {
                                                                ClassConnectorSetting.SeedNodeDisconnectScore.Add(newSeedNodeHost, new Tuple<int, long>(0, 0));
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (packetDecrypt != ClassAlgoErrorEnumeration.AlgoError)
                                                            {
                                                                bufferPacket.packet = packetDecrypt + ClassConnectorSetting.PacketSplitSeperator;
                                                            }
                                                            else
                                                            {
                                                                if (_malformedPacket.Length - 1 >= int.MaxValue || (long)(_malformedPacket.Length + bufferPacket.packet.Length) >= int.MaxValue)
                                                                {
                                                                    _malformedPacket = string.Empty;
                                                                }
                                                                _malformedPacket += bufferPacket.packet;
                                                            }
                                                        }

                                                    }
                                                    catch
                                                    {

                                                    }
                                                }
                                            }
                                        }
                                    }
                                    if (bufferPacket.packet == ClassSeedNodeCommand.ClassReceiveSeedEnumeration.DisconnectPacket)
                                    {
                                        _isConnected = false;
                                        return ClassSeedNodeStatus.SeedError;
                                    }

                                    return bufferPacket.packet;
                                }
                            }

                        }
                    }
                }
            }
            catch (Exception)
            {
                _isConnected = false;
                return ClassSeedNodeStatus.SeedError;
            }

            return ClassSeedNodeStatus.SeedNone;
        }

        /// <summary>
        ///     Return the status of connection.
        /// </summary>
        /// <returns></returns>
        public bool GetStatusConnectToSeed(bool isLinux = false)
        {

            if (!ClassUtils.SocketIsConnected(_connector))
            {
                _isConnected = false;
            }

            return _isConnected;
        }

        /// <summary>
        /// Return directly status without to proceed check.
        /// </summary>
        /// <returns></returns>
        public bool ReturnStatus()
        {
            return _isConnected;
        }

        /// <summary>
        /// Return the current seed node host used.
        /// </summary>
        /// <returns></returns>
        public string ReturnCurrentSeedNodeHost()
        {
            return _currentSeedNodeHost;
        }

        /// <summary>
        ///     Disconnect to Seed Node.
        /// </summary>
        public void DisconnectToSeed()
        {
            if (!string.IsNullOrEmpty(_currentSeedNodeHost))
            {
                if (ClassConnectorSetting.SeedNodeDisconnectScore.ContainsKey(_currentSeedNodeHost))
                {
                    int totalDisconnection = ClassConnectorSetting.SeedNodeDisconnectScore[_currentSeedNodeHost].Item1 + 1;
                    ClassConnectorSetting.SeedNodeDisconnectScore[_currentSeedNodeHost] = new Tuple<int, long>(totalDisconnection, ClassUtils.DateUnixTimeNowSecond());
                }
            }
            ClassConnectorSetting.NETWORK_GENESIS_KEY = ClassConnectorSetting.NETWORK_GENESIS_DEFAULT_KEY;
            _isConnected = false;
            _currentSeedNodeHost = string.Empty;
            _malformedPacket = string.Empty;
            AesIvCertificate = null;
            AesSaltCertificate = null;
            _connector?.Close();
            _connector?.Dispose();
            Dispose();
        }
    }
}