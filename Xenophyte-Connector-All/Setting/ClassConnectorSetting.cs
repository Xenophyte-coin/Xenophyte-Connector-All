using System;
using System.Collections.Generic;

namespace Xenophyte_Connector_All.Setting
{
    public class ClassConnectorSettingEnumeration
    {
        /// <summary>
        /// List of login types.
        /// </summary>
        public const string WalletLoginType = "WALLET";
        public const string WalletCreateType = "WALLET-CREATE";
        public const string WalletRestoreType = "WALLET-ASK";
        public const string WalletTokenType = "WALLET-TOKEN"; // accepted only on seed nodes token port.
        public const string RemoteLoginType = "REMOTE";
        public const string MinerLoginType = "MINER";
        public const string WalletLoginProxy = "WALLET-PROXY";
    }

    public class ClassConnectorSetting
    {
        public const int SeedNodePort = 18000;
        public const int RemoteNodeHttpPort = 18001;
        public const int RemoteNodePort = 18002;
        public const int SeedNodeTokenPort = 18003;

        /// <summary>
        ///     UPDATES - First Major Update done at 10/08/2018
        /// </summary>
        public const bool MAJOR_UPDATE_1 = true; // Implementation of: Timestamp recv transaction (for wallet), link blockchain height for transaction, last block found.

        public const bool MAJOR_UPDATE_1_SECURITY = true; // Implementation of: Certificate for include a new layer of protection.

        public const int MAJOR_UPDATE_1_SECURITY_CERTIFICATE_SIZE = 256; // Size of Certificate encryption [Between tools and seed nodes]

        public const int MAJOR_UPDATE_1_SECURITY_CERTIFICATE_SIZE_ITEM = 256;

        public const string NETWORK_GENESIS_DEFAULT_KEY = "XENOPHYTEKEY"; // DEFAULT GENESIS KEY [Small static part of dynamic key encryption between tools and blockchain.]

        public const string NETWORK_GENESIS_DEFAULT_KEY_XIRO = "XIROPHTKEY"; // DEFAULT GENESIS KEY [Small static part of dynamic key encryption between tools and blockchain.]


        /// <summary>
        ///     UPDATES - Update done at 17/10/2018
        /// </summary>
        public static string NETWORK_GENESIS_KEY = "XENOPHYTEKEY"; // GENESIS KEY [Small static part included on dynamic key encryption between tools and blockchain, updated by the blockchain in real time.]

        public const string NETWORK_GENESIS_SECONDARY_KEY = "XENOPHYTESEED"; // GENESIS SECONDARY KEY [Layer encryption key included on dynamic certificate key between tools and seed nodes]

        /// <summary>
        ///     UPDATES - Update done at 17/10/2018
        /// </summary>
        public static string NETWORK_GENESIS_KEY_XIRO = "XIROPHTKEY"; // GENESIS KEY [Small static part included on dynamic key encryption between tools and blockchain, updated by the blockchain in real time.]

        public const string NETWORK_GENESIS_SECONDARY_KEY_XIRO = "XIROPHTSEED"; // GENESIS SECONDARY KEY [Layer encryption key included on dynamic certificate key between tools and seed nodes]

        public const string PacketSplitSeperator = "*";

        public const string PacketMiningSplitSeperator = "#";

        public const string PacketContentSeperator = "|";

        public static Dictionary<string, Tuple<string, bool>> SeedNodeIp = new Dictionary<string, Tuple<string, bool>>
        {
            {"87.98.156.228", new Tuple<string, bool>("FR", true) },
        };

        public static Dictionary<string, Tuple<int, long>> SeedNodeDisconnectScore = new Dictionary<string, Tuple<int, long>>
        {
            {"87.98.156.228", new Tuple<int, long>(0, 0)},
        };

        public const decimal MinimumWalletTransactionFee = 0.000010000m;
        public const decimal MinimumWalletTransactionAnonymousFee = 0.000010000m;
        public const decimal ConstantBlockReward = 10.00000000m;
        public const int MaxTimeoutConnect = 5000;
        public const int MaxSeedNodeTimeoutConnect = 1000;
        public const int MaxPingDelay = 2000;
        public const int MaxTimeoutConnectRemoteNode = 1000;
        public const int MaxTimeoutSendPacket = 2000;
        public const int MaxTimeoutConnectLocalhostRemoteNode = 1000;
        public const int MaxNetworkPacketSize = 16384;
        public const int MaxRemoteNodeInvalidPacket = 10;
        public const int MaxRemoteNodeBanTime = 60; // 60 seconds.
        public const int MaxDelayRemoteNodeTrust = 30; // 30 seconds.
        public const int MaxDelayRemoteNodeSyncResponse = 10; // 10 seconds.
        public const int MaxDelayRemoteNodeWaitResponse = 5; // 5 seconds.
        public const int MaxDecimalPlace = 8;
        public const int MinWalletAddressSize = 48;
        public const int MaxWalletAddressSize = 96;
        public const int SeedNodeMaxDisconnection = 10; // Max disconnection before received.
        public const int SeedNodeMaxRetry = 2; // Max attempt to connect.
        public const int SeedNodeMaxKeepAliveDisconnection = 15; // Keep alive total disconnection pending 15 seconds.
        public const int WalletMinPasswordLength = 8; // Minimum password length
        public const string CoinName = "Xenophyte";
        public const string CoinNameMin = "XENOP";
    }
}
