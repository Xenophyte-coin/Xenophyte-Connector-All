﻿using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Xenophyte_Connector_All.Seed;
using Xenophyte_Connector_All.Setting;
using Xenophyte_Connector_All.Utils;


namespace Xenophyte_Connector_All.Wallet
{
    public class ClassWalletNetworkSetting
    {
        public const int KeySize = 256;
    }

    public class ClassWalletPhase
    {
        public const string Create = "CREATE";
        public const string Restore = "RESTORE";
        public const string Login = "LOGIN";
        public const string Password = "PASSWORD";
        public const string Key = "KEY";
        public const string Pin = "PIN";
        public const string Accepted = "ACCEPTED";
    }

    public class ClassWalletConnect
    {
        private readonly ClassSeedNodeConnector _seedNodeConnector;


        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="seedNodeConnector"></param>
        public ClassWalletConnect(ClassSeedNodeConnector seedNodeConnector)
        {
            AesIvCertificate = null;
            AesSaltCertificate = null;
            _seedNodeConnector = seedNodeConnector;
        }


        public string WalletId { get; set; }
        public string WalletIdAnonymity { get; set; }
        public string WalletAddress { get; set; }
        public string WalletPassword { get; set; }
        public string WalletKey { get; set; }
        public string WalletAmount { get; set; }
        public string WalletPhase { get; set; }
        private byte[] AesIvCertificate;
        private byte[] AesSaltCertificate;
        private bool _oldVersion = false;

        /// <summary>
        ///     Can select the wallet phase for network (login, create).
        /// </summary>
        /// <param name="walletPhase"></param>
        public void SelectWalletPhase(string walletPhase)
        {
            WalletPhase = walletPhase;
            AesIvCertificate = null;
            AesSaltCertificate = null;
        }

        /// <summary>
        ///     Send packet from wallet to seed nodes.
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="certificate"></param>
        /// <param name="isEncrypted"></param>
        public async Task<bool> SendPacketWallet(string packet, string certificate, bool isEncrypted)
        {
            if (WalletPhase == ClassConnectorSettingEnumeration.WalletCreateType || WalletPhase == string.Empty) // Not allow to create a wallet on non-permanent seed nodes.
            {
                if (ClassConnectorSetting.SeedNodeIp.ContainsKey(_seedNodeConnector.ReturnCurrentSeedNodeHost()))
                {
                    if (!ClassConnectorSetting.SeedNodeIp[_seedNodeConnector.ReturnCurrentSeedNodeHost()].Item2)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            return await _seedNodeConnector.SendPacketToSeedNodeAsync(EncryptPacketWallet(packet), certificate, false, isEncrypted);
        }

        /// <summary>
        ///     Receive packet from seed nodes for wallet.
        /// </summary>
        /// <returns></returns>
        public async Task<string> ListenPacketWalletAsync(string certificate, bool isEncrypted)
        {
            string packet = string.Empty;
            try
            {
                if (WalletPhase == ClassWalletPhase.Create || WalletPhase == string.Empty) // Not allow to create a wallet on non-permanent seed nodes.
                {
                    if (ClassConnectorSetting.SeedNodeIp.ContainsKey(_seedNodeConnector.ReturnCurrentSeedNodeHost()))
                    {
                        if (!ClassConnectorSetting.SeedNodeIp[_seedNodeConnector.ReturnCurrentSeedNodeHost()].Item2)
                        {
                            return ClassSeedNodeStatus.SeedError;
                        }
                    }
                    else
                    {
                        return ClassSeedNodeStatus.SeedError;
                    }
                }
                packet = await _seedNodeConnector.ReceivePacketFromSeedNodeAsync(certificate, false, isEncrypted);
                if (packet.Contains(ClassSeedNodeCommand.ClassReceiveSeedEnumeration.WalletSendRemoteNode) || packet.Contains(ClassSeedNodeCommand.ClassReceiveSeedEnumeration.WalletResultMaxSupply) || packet.Contains(ClassSeedNodeCommand.ClassReceiveSeedEnumeration.WalletResultCoinCirculating) || packet.Contains(ClassSeedNodeCommand.ClassReceiveSeedEnumeration.WalletResultNetworkDifficulty) || packet.Contains(ClassSeedNodeCommand.ClassReceiveSeedEnumeration.WalletResultNetworkHashrate) || packet.Contains(ClassSeedNodeCommand.ClassReceiveSeedEnumeration.WalletResultTotalBlockMined) || packet.Contains(ClassSeedNodeCommand.ClassReceiveSeedEnumeration.WalletResultTotalTransactionFee) || packet.Contains(ClassSeedNodeCommand.ClassReceiveSeedEnumeration.WalletResultTotalPendingTransaction) || packet.Contains(ClassSeedNodeCommand.ClassReceiveSeedEnumeration.WalletResultBlockPerId)) return packet;

                if (WalletPhase != ClassWalletPhase.Create)
                {
                    if (packet != ClassSeedNodeStatus.SeedNone && packet != ClassSeedNodeStatus.SeedError)
                    {
                        if (packet.Contains(ClassConnectorSetting.PacketSplitSeperator))
                        {
                            var splitPacket = packet.Split(new[] { ClassConnectorSetting.PacketSplitSeperator }, StringSplitOptions.None);
                            var packetCompleted = string.Empty;
                            foreach (var packetEach in splitPacket)
                            {
                                if (!string.IsNullOrEmpty(packetEach))
                                {
                                    if (packetEach.Length > 1)
                                    {
                                        if (packetEach.Replace(ClassConnectorSetting.PacketSplitSeperator, "") != ClassWalletCommand.ClassWalletReceiveEnumeration.WalletInvalidPacket)
                                        {
                                            packetCompleted += DecryptPacketWallet(packetEach.Replace(ClassConnectorSetting.PacketSplitSeperator, "")) + ClassConnectorSetting.PacketSplitSeperator;
                                        }
                                        else
                                        {
                                            packetCompleted += packetEach.Replace(ClassConnectorSetting.PacketSplitSeperator, "") + ClassConnectorSetting.PacketSplitSeperator;
                                        }
                                    }
                                }
                            }
                            return packetCompleted;
                        }
                        if (packet.Replace(ClassConnectorSetting.PacketSplitSeperator, "") != ClassWalletCommand.ClassWalletReceiveEnumeration.WalletInvalidPacket)
                            return DecryptPacketWallet(packet);
                        

                        return ClassWalletCommand.ClassWalletReceiveEnumeration.WalletInvalidPacket;
                    }
                }
            }
            catch
            {
                return ClassSeedNodeStatus.SeedError;
            }
            return packet;
        }


        /// <summary>
        ///     Encrypt a packet according to wallet phase.
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        private string EncryptPacketWallet(string packet)
        {
            switch (WalletPhase)
            {
                case "":
                    return packet;
                case ClassWalletPhase.Restore:
                case ClassWalletPhase.Create:
                    return packet;
                case ClassWalletPhase.Login:
                    return packet;
                default:
                    if (AesIvCertificate == null)
                    {
                        using (PasswordDeriveBytes password = new PasswordDeriveBytes(WalletAddress + WalletPassword + WalletKey + (_oldVersion ? ClassConnectorSetting.NETWORK_GENESIS_KEY_XIRO : ClassConnectorSetting.NETWORK_GENESIS_KEY), ClassUtils.GetByteArrayFromString(ClassUtils.FromHex((WalletAddress + WalletPassword + WalletKey + ClassConnectorSetting.NETWORK_GENESIS_KEY).Substring(0, 8)))))
                        {
                            AesIvCertificate = password.GetBytes(ClassConnectorSetting.MAJOR_UPDATE_1_SECURITY_CERTIFICATE_SIZE / 8);
                            AesSaltCertificate = password.GetBytes(16);
                        }
                    }
                    return ClassAlgo.GetEncryptedResult(ClassAlgoEnumeration.Rijndael, packet + "|" + ClassUtils.DateUnixTimeNowSecond(), ClassWalletNetworkSetting.KeySize, AesIvCertificate, AesSaltCertificate); // AES
            }
        }

        /// <summary>
        ///     Decrypt a packet according to wallet phase.
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        private string DecryptPacketWallet(string packet)
        {
            switch (WalletPhase)
            {
                case "":
                    return packet;
                case ClassWalletPhase.Restore:
                case ClassWalletPhase.Create:
                    return packet;
                case ClassWalletPhase.Login:
                    return packet;
                default:
                    {
                        if (AesIvCertificate == null)
                        {
                            using (PasswordDeriveBytes password = new PasswordDeriveBytes(WalletAddress + WalletPassword + WalletKey + (_oldVersion ? ClassConnectorSetting.NETWORK_GENESIS_KEY_XIRO : ClassConnectorSetting.NETWORK_GENESIS_KEY), ClassUtils.GetByteArrayFromString(ClassUtils.FromHex((WalletAddress + WalletPassword + WalletKey + ClassConnectorSetting.NETWORK_GENESIS_KEY).Substring(0, 8)))))
                            {
                                AesIvCertificate = password.GetBytes(ClassConnectorSetting.MAJOR_UPDATE_1_SECURITY_CERTIFICATE_SIZE / 8);
                                AesSaltCertificate = password.GetBytes(16);
                            }
                        }
                        string packetResult = ClassAlgo.GetDecryptedResult(ClassAlgoEnumeration.Rijndael, packet, ClassWalletNetworkSetting.KeySize, AesIvCertificate, AesSaltCertificate); // AES

                        if (packetResult == ClassAlgoErrorEnumeration.AlgoError)
                        {
                            using (PasswordDeriveBytes password = new PasswordDeriveBytes(WalletAddress + WalletPassword + WalletKey + ClassConnectorSetting.NETWORK_GENESIS_KEY_XIRO, ClassUtils.GetByteArrayFromString(ClassUtils.FromHex((WalletAddress + WalletPassword + WalletKey + ClassConnectorSetting.NETWORK_GENESIS_KEY).Substring(0, 8)))))
                            {
                                AesIvCertificate = password.GetBytes(ClassConnectorSetting.MAJOR_UPDATE_1_SECURITY_CERTIFICATE_SIZE / 8);
                                AesSaltCertificate = password.GetBytes(16);
                            }

                            packetResult = ClassAlgo.GetDecryptedResult(ClassAlgoEnumeration.Rijndael, packet, ClassWalletNetworkSetting.KeySize, AesIvCertificate, AesSaltCertificate); // AES

                            _oldVersion = packetResult != ClassAlgoErrorEnumeration.AlgoError;
                        }

                        return packetResult;
                    }
            }
        }

        public void UpdateWalletIv()
        {
            using (PasswordDeriveBytes password = new PasswordDeriveBytes(WalletAddress + WalletPassword + WalletKey + (_oldVersion ? ClassConnectorSetting.NETWORK_GENESIS_KEY_XIRO : ClassConnectorSetting.NETWORK_GENESIS_KEY), ClassUtils.GetByteArrayFromString(ClassUtils.FromHex((WalletAddress + WalletPassword + WalletKey + ClassConnectorSetting.NETWORK_GENESIS_KEY).Substring(0, 8)))))
            {
                AesIvCertificate = password.GetBytes(ClassConnectorSetting.MAJOR_UPDATE_1_SECURITY_CERTIFICATE_SIZE / 8);
                AesSaltCertificate = password.GetBytes(16);
            }
        }
    }
}