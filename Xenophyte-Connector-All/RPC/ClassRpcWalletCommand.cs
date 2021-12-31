namespace Xenophyte_Connector_All.RPC
{
    public class ClassRpcWalletCommand
    {
        /// <summary>
        /// Request.
        /// </summary>
        public const string TokenAsk = "TOKEN-ASK";
        public const string TokenAskBalance = "TOKEN-ASK-BALANCE";
        public const string TokenAskWalletId = "TOKEN-ASK-WALLET-ID";
        public const string TokenAskWalletAnonymousId = "TOKEN-ASK-WALLET-ANONYMOUS-ID";
        public const string TokenAskWalletSendTransaction = "TOKEN-ASK-WALLET-SEND-TRANSACTION";
        public const string TokenCheckWalletAddressExist = "TOKEN-CHECK-WALLET-ADDRESS-EXIST";
        public const string TokenAskWalletTransfer = "TOKEN-ASK-WALLET-TRANSFER";
        public const string TokenAskRemoteNode = "TOKEN-ASK-REMOTE-NODE";
        public const string TokenAskWalletQuestion = "TOKEN-ASK-WALLET-QUESTION";
        public const string TokenAskBlocktemplate = "TOKEN-ASK-BLOCKTEMPLATE";


        /// <summary>
        /// Reponse.
        /// </summary>
        public const string SendToken = "SEND-TOKEN";
        public const string SendTokenBalance = "SEND-TOKEN-BALANCE";
        public const string SendTokenWalletId = "SEND-TOKEN-WALLET-ID";
        public const string SendTokenWalletAnonymousId = "SEND-TOKEN-WALLET-ANONYMOUS-ID";
        public const string SendTokenExpired = "SEND-TOKEN-EXPIRED";
        public const string SendTokenCheckWalletAddressValid = "SEND-TOKEN-CHECK-WALLET-ADDRESS-VALID";
        public const string SendTokenCheckWalletAddressInvalid = "SEND-TOKEN-CHECK-WALLET-ADDRESS-INVALID";
        public const string SendTokenValidInformation = "SEND-TOKEN-VALID-INFORMATION";
        public const string SendTokenInvalidInformation = "SEND-TOKEN-INVALID-INFORMATION";
        public const string SendTokenWalletQuestion = "SEND-TOKEN-WALLET-QUESTION";


        // Response of transaction.
        public const string SendTokenTransactionConfirmed = "SEND-TOKEN-TRANSACTION-CONFIRMED";
        public const string SendTokenTransactionRefused = "SEND-TOKEN-TRANSACTION-REFUSED";
        public const string SendTokenTransactionBusy = "SEND-TOKEN-TRANSACTION-BUSY";
        public const string SendTokenTransactionInvalidTarget = "SEND-TOKEN-TRANSACTION-INVALID-TARGET";

        // Response of transfer.
        public const string SendTokenTransferConfirmed = "SEND-TOKEN-TRANSFER-CONFIRMED";
        public const string SendTokenTransferRefused = "SEND-TOKEN-TRANSFER-REFUSED";
        public const string SendTokenTransferBusy = "SEND-TOKEN-TRANSFER-BUSY";

        /// <summary>
        /// Request of checking.
        /// </summary>
        public const string TokenCheckMaxSupply = "TOKEN-CHECK-MAX-SUPPLY";
        public const string TokenCheckCurrentCirculating = "TOKEN-CHECK-CURRENT-CIRCULATING";
        public const string TokenCheckTotalTransactionFee = "TOKEN-CHECK-TOTAL-TRANSACTION-FEE";
        public const string TokenCheckTotalBlockMined = "TOKEN-CHECK-TOTAL-BLOCK-MINED";
        public const string TokenCheckNetworkHashrate = "TOKEN-CHECK-NETWORK-HASHRATE";
        public const string TokenCheckNetworkDifficulty = "TOKEN-CHECK-NETWORK-DIFFICULTY";
        public const string TokenCheckTotalPendingTransaction = "TOKEN-CHECK-TOTAL-PENDING-TRANSACTION";
        public const string TokenCheckWalletTotalTransaction = "TOKEN-CHECK-WALLET-TOTAL-TRANSACTION";
        public const string TokenCheckLastBlockFoundDate = "TOKEN-CHECK-LAST-BLOCK-FOUND-DATE";
        public const string TokenCheckBlock = "TOKEN-CHECK-BLOCK";
        public const string TokenCheckTransaction = "TOKEN-CHECK-TRANSACTION";
        public const string WalletTokenNetworkIsAndroidType = "IS-ANDROID";

    }
}
