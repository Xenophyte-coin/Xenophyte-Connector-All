using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xenophyte_Connector_All.Remote;
using Xenophyte_Connector_All.Setting;
using Xenophyte_Connector_All.Utils;

namespace Xenophyte_Connector_All.Wallet
{

    public class ClassWalletConnectToRemoteNodeObjectSendPacket : IDisposable
    {
        public byte[] packetByte;
        private bool disposed;

        public ClassWalletConnectToRemoteNodeObjectSendPacket(string packet)
        {
            packetByte = ClassUtils.GetByteArrayFromString(packet);
        }

        ~ClassWalletConnectToRemoteNodeObjectSendPacket()
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

            disposed = true;
        }
    }

    public class ClassWalletConnectToRemoteNodeObjectPacket : IDisposable
    {
        public byte[] buffer;
        public string packet;
        private bool disposed;

        public ClassWalletConnectToRemoteNodeObjectPacket()
        {
            buffer = new byte[ClassConnectorSetting.MaxNetworkPacketSize];
            packet = string.Empty;
        }

        ~ClassWalletConnectToRemoteNodeObjectPacket()
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

            disposed = true;
        }
    }

    public class ClassWalletConnectToRemoteNodeObjectError
    {
        public const string ObjectError = "ERROR";
        public const string ObjectNone = "NONE";
    }

    public class ClassWalletConnectToRemoteNodeObject
    {
        public const string ObjectAskBlock = "BLOCK";
        public const string ObjectTransaction = "TRANSACTION";
        public const string ObjectSupply = "SUPPLY";
        public const string ObjectCirculating = "CIRCULATING";
        public const string ObjectFee = "FEE";
        public const string ObjectBlockMined = "MINED";
        public const string ObjectDifficulty = "DIFFICULTY";
        public const string ObjectRate = "RATE";
        public const string ObjectPendingTransaction = "PENDING-TRANSACTION";
        public const string ObjectAskWalletTransaction = "ASK-TRANSACTION";
        public const string ObjectAskLastBlockFound = "ASK-LAST-BLOCK-FOUND";
        public const string ObjectAskWalletAnonymityTransaction = "ASK-ANONYMITY-TRANSACTION";
    }

    public class ClassWalletConnectToRemoteNode : IDisposable
    {
        private Socket _remoteNodeClient;
        private string _remoteNodeClientType;
        public IPAddress RemoteNodeHost;
        public bool RemoteNodeStatus;
        public int TotalInvalidPacket;
        public bool Disposed;
        public long LastTrustDate;

        ~ClassWalletConnectToRemoteNode()
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
            if (Disposed)
                return;

            if (disposing)
            {
                _remoteNodeClient = null;
            }
            Disposed = true;
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="remoteNodeType"></param>
        public ClassWalletConnectToRemoteNode(string remoteNodeType)
        {
            _remoteNodeClientType = remoteNodeType;
            RemoteNodeStatus = true;
        }

        /// <summary>
        /// Return the connection status opened to a remote node.
        /// </summary>
        /// <returns></returns>
        public bool CheckRemoteNode()
        {
            return RemoteNodeStatus;
        }

        /// <summary>
        ///     Connect the wallet to a remote node.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="isLinux"></param>
        /// <returns></returns>
        public async Task<bool> ConnectToRemoteNodeAsync(IPAddress host, int port, bool isLinux = false)
        {
            MalformedPacket = string.Empty;

            try
            {
                _remoteNodeClient?.Close();
            }
            catch
            {

            }
            RemoteNodeHost = host;
            TotalInvalidPacket = 0;
            LastTrustDate = 0;
            try
            {
                RemoteNodeStatus = true;
                if(!await ConnectToTarget(host, port))
                {
                    RemoteNodeStatus = false;
                    return false;
                }

                _remoteNodeClient.SetSocketKeepAliveValues(24 * 60, 60);
            }
            catch (Exception error)
            {
#if DEBUG
                Console.WriteLine("Error to connect wallet on remote nodes: " + error.Message);
#endif
                RemoteNodeStatus = false;
                return false;
            }

            RemoteNodeHost = host;

            await Task.Factory.StartNew(EnableCheckConnection, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Current).ConfigureAwait(false);
            
            return true;
        }

        /// <summary>
        /// Check the connection opened to the remote node.
        /// </summary>
        private async void EnableCheckConnection()
        {
            while (RemoteNodeStatus)
            {
                try
                {
                    if (!ClassUtils.SocketIsConnected(_remoteNodeClient))
                    {
                        RemoteNodeStatus = false;
                        break;
                    }

                    if (!await SendPacketRemoteNodeAsync(ClassRemoteNodeCommandForWallet.RemoteNodeSendPacketEnumeration.KeepAlive + "|0"))
                    {
                        RemoteNodeStatus = false;
                        break;
                    }

                }
                catch
                {
                    RemoteNodeStatus = false;
                    break;
                }
                await Task.Delay(1000);
            }
        }


        private async Task<bool> ConnectToTarget(IPAddress host, int port)
        {

            _remoteNodeClient = new Socket(host.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            Task clientTask = _remoteNodeClient.ConnectAsync(host, port);
            var delayTask = Task.Delay(ClassConnectorSetting.MaxTimeoutConnectRemoteNode);

            var completedTask = await Task.WhenAny(new[] { clientTask, delayTask });
            return completedTask == clientTask;

        }


        private string MalformedPacket;

        /// <summary>
        ///     Listen network of remote node.
        /// </summary>
        public async Task<string> ListenRemoteNodeNetworkAsync()
        {
            try
            {
                if (_remoteNodeClient != null)
                {
                    using (var remoteNodeStream = new NetworkStream(_remoteNodeClient))
                    {
                        using (var bufferedStreamNetwork = new BufferedStream(remoteNodeStream,
                            ClassConnectorSetting.MaxNetworkPacketSize))
                        {
                            using (var bufferPacket = new ClassWalletConnectToRemoteNodeObjectPacket())
                            {
                                int received = await bufferedStreamNetwork.ReadAsync(bufferPacket.buffer, 0,
                                    bufferPacket.buffer.Length);
                                if (received > 0)
                                {
                                    string packet = Encoding.UTF8.GetString(bufferPacket.buffer, 0, received);
                                    if (packet.Contains(ClassConnectorSetting.PacketSplitSeperator))
                                    {
                                        if (!string.IsNullOrEmpty(MalformedPacket))
                                        {
                                            packet = MalformedPacket + packet;
                                            MalformedPacket = string.Empty;
                                        }

                                        return packet;
                                    }
                                    else
                                    {
                                        if (MalformedPacket.Length - 1 >= int.MaxValue ||
                                            MalformedPacket.Length + packet.Length >= int.MaxValue)
                                        {
                                            MalformedPacket = string.Empty;
                                        }

                                        MalformedPacket += packet;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception error)
            {
                DisconnectRemoteNodeClient();
#if DEBUG
                Console.WriteLine("Error to listen remote node network: " + error.Message);
#endif
                return ClassWalletConnectToRemoteNodeObjectError.ObjectError;
            }

            return ClassWalletConnectToRemoteNodeObjectError.ObjectNone;
        }

        /// <summary>
        ///     Disconnect wallet of remote node.
        /// </summary>
        public void DisconnectRemoteNodeClient()
        {
            MalformedPacket = string.Empty;
            _remoteNodeClient?.Close();
            _remoteNodeClientType = string.Empty;
            RemoteNodeStatus = false;
            TotalInvalidPacket = 0;
            LastTrustDate = 0;
            Dispose();
        }

        /// <summary>
        ///     Send a selected command to remote node.
        /// </summary>
        /// <param name="command"></param>
        public async Task<bool> SendPacketRemoteNodeAsync(string command)
        {
            var clientTask = TaskSendPacketRemoteNode(command);
            var delayTask = Task.Delay(ClassConnectorSetting.MaxTimeoutSendPacket);

            var completedTask = await Task.WhenAny(new[] { clientTask, delayTask });
            return completedTask == clientTask;
        }

        private async Task<bool> TaskSendPacketRemoteNode(string command)
        {
            if (!RemoteNodeStatus)
            {
                return false;
            }
            try
            {
                if (_remoteNodeClient != null)
                {
                    using (var _remoteNodeStream = new NetworkStream(_remoteNodeClient))
                    {
                        using (var bufferedStream = new BufferedStream(_remoteNodeStream,
                            ClassConnectorSetting.MaxNetworkPacketSize))
                        {
                            using (var packetObject = new ClassWalletConnectToRemoteNodeObjectSendPacket(command + ClassConnectorSetting.PacketSplitSeperator))
                            {
                                await bufferedStream.WriteAsync(packetObject.packetByte, 0,
                                    packetObject.packetByte.Length);
                                await bufferedStream.FlushAsync();
                            }
                        }
                    }
                }

            }
            catch
            {
                RemoteNodeStatus = false;
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Send the right packet type to remote node.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SendPacketTypeRemoteNode(string walletId)
        {
           ClassWalletConnectToRemoteNodeObjectSendPacket packet;

            switch (_remoteNodeClientType)
            {
                case ClassWalletConnectToRemoteNodeObject.ObjectTransaction:
                    packet = new ClassWalletConnectToRemoteNodeObjectSendPacket(
                        ClassRemoteNodeCommandForWallet.RemoteNodeSendPacketEnumeration.WalletAskHisNumberTransaction +
                        ClassConnectorSetting.PacketContentSeperator + walletId+ClassConnectorSetting.PacketSplitSeperator);
                    break;
                case ClassWalletConnectToRemoteNodeObject.ObjectAskWalletAnonymityTransaction:
                    packet = new ClassWalletConnectToRemoteNodeObjectSendPacket(
                        ClassRemoteNodeCommandForWallet.RemoteNodeSendPacketEnumeration.WalletAskHisAnonymityNumberTransaction +
                        ClassConnectorSetting.PacketContentSeperator + walletId + ClassConnectorSetting.PacketSplitSeperator);
                    break;
                case ClassWalletConnectToRemoteNodeObject.ObjectSupply:
                    packet = new ClassWalletConnectToRemoteNodeObjectSendPacket(
                        ClassRemoteNodeCommandForWallet.RemoteNodeSendPacketEnumeration.AskCoinMaxSupply + ClassConnectorSetting.PacketContentSeperator +
                        walletId+ClassConnectorSetting.PacketSplitSeperator);
                    break;
                case ClassWalletConnectToRemoteNodeObject.ObjectCirculating:
                    packet = new ClassWalletConnectToRemoteNodeObjectSendPacket(
                        ClassRemoteNodeCommandForWallet.RemoteNodeSendPacketEnumeration.AskCoinCirculating + ClassConnectorSetting.PacketContentSeperator +
                        walletId+ClassConnectorSetting.PacketSplitSeperator);
                    break;
                case ClassWalletConnectToRemoteNodeObject.ObjectFee:
                    packet = new ClassWalletConnectToRemoteNodeObjectSendPacket(
                        ClassRemoteNodeCommandForWallet.RemoteNodeSendPacketEnumeration.AskTotalFee + ClassConnectorSetting.PacketContentSeperator + walletId+ClassConnectorSetting.PacketSplitSeperator);
                    break;
                case ClassWalletConnectToRemoteNodeObject.ObjectBlockMined:
                    packet = new ClassWalletConnectToRemoteNodeObjectSendPacket(
                        ClassRemoteNodeCommandForWallet.RemoteNodeSendPacketEnumeration.AskTotalBlockMined + ClassConnectorSetting.PacketContentSeperator +
                        walletId+ClassConnectorSetting.PacketSplitSeperator);
                    break;
                case ClassWalletConnectToRemoteNodeObject.ObjectDifficulty:
                    packet = new ClassWalletConnectToRemoteNodeObjectSendPacket(
                        ClassRemoteNodeCommandForWallet.RemoteNodeSendPacketEnumeration.AskCurrentDifficulty + ClassConnectorSetting.PacketContentSeperator +
                        walletId+ClassConnectorSetting.PacketSplitSeperator);
                    break;
                case ClassWalletConnectToRemoteNodeObject.ObjectRate:
                    packet = new ClassWalletConnectToRemoteNodeObjectSendPacket(
                        ClassRemoteNodeCommandForWallet.RemoteNodeSendPacketEnumeration.AskCurrentRate + ClassConnectorSetting.PacketContentSeperator +
                        walletId+ClassConnectorSetting.PacketSplitSeperator);
                    break;
                case ClassWalletConnectToRemoteNodeObject.ObjectPendingTransaction:
                    packet = new ClassWalletConnectToRemoteNodeObjectSendPacket(
                        ClassRemoteNodeCommandForWallet.RemoteNodeSendPacketEnumeration.AskTotalPendingTransaction +
                        ClassConnectorSetting.PacketContentSeperator + walletId+ClassConnectorSetting.PacketSplitSeperator);
                    break;
                case ClassWalletConnectToRemoteNodeObject.ObjectAskLastBlockFound:
                    packet = new ClassWalletConnectToRemoteNodeObjectSendPacket(
                        ClassRemoteNodeCommandForWallet.RemoteNodeSendPacketEnumeration.AskLastBlockFoundTimestamp +
                        ClassConnectorSetting.PacketContentSeperator + walletId+ClassConnectorSetting.PacketSplitSeperator);
                    break;
                case ClassWalletConnectToRemoteNodeObject.ObjectAskWalletTransaction:
                    return true;
                case ClassWalletConnectToRemoteNodeObject.ObjectAskBlock:
                    return true;
                default:
                    return false;
            }


            try
            {
                if (_remoteNodeClient != null)
                {
                    using (var remoteNodeStream = new NetworkStream(_remoteNodeClient))
                    {
                        using (var bufferedStreamNetwork = new BufferedStream(remoteNodeStream,
                            ClassConnectorSetting.MaxNetworkPacketSize))
                        {
                            await bufferedStreamNetwork.WriteAsync(packet.packetByte, 0, packet.packetByte.Length);
                            await bufferedStreamNetwork.FlushAsync();
                        }
                    }
                }
            }
            catch (Exception error)
            {
                RemoteNodeStatus = false;
#if DEBUG
                Console.WriteLine("Error to send packet on remote node network: " + error.Message);
#endif
                return false;
            }


            return true;
        }
    }
}