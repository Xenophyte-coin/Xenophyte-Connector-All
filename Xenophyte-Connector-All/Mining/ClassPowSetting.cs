namespace Xenophyte_Connector_All.Mining
{
    public class ClassPowSetting
    {
        #region Mandatory share size.

        public const int MergedPowShareSize = 64;
        public const int NonceShareSize = 4;

        #endregion

        #region Computation properties size on Merged Share.

        public const int AmountByteShareSize = 32;
        public const int AmountByteShareIndex = 0;

        public const int AmountByteShareTargetSize = 32;
        public const int AmountByteShareTargetIndex = 32;

        #endregion

        #region Offset computation properties on target share.

        public const int OffsetTargetShareNonceByteIndex1 = 0;
        public const int OffsetTargetShareNonceByteIndex2 = 8;
        public const int OffsetTargetShareNonceByteIndex3 = 16;
        public const int OffsetTargetShareNonceByteIndex4 = 31;
        public const int TargetShareNonceValueShift1 = 8;
        public const int TargetShareNonceValueShift2 = 16;
        public const int TargetShareNonceValueShift3 = 24;

        #endregion


        #region Offset computation properties on the share.

        public const int OffsetShareNonceByteIndex1 = 0;
        public const int OffsetShareNonceByteIndex2 = 8;
        public const int OffsetShareNonceByteIndex3 = 16;
        public const int OffsetShareNonceByteIndex4 = 31;
        public const int ShareNonceValueShift1 = 8;
        public const int ShareNonceValueShift2 = 16;
        public const int ShareNonceValueShift3 = 24;

        #endregion

        #region Offset computation properties on nonce of the share.

        public const int OffsetNonceByteIndex1 = 0;
        public const int OffsetNonceByteIndex2 = 1;
        public const int OffsetNonceByteIndex3 = 2;
        public const int OffsetNonceByteIndex4 = 3;

        public const int NonceValueShift1 = 8;
        public const int NonceValueShift2 = 16;
        public const int NonceValueShift3 = 24;

        #endregion

        #region PoW Difficulty Share properties.

        public const int PowDifficultyShareFromResultStartIndex = 0;
        public const int PowDifficultyShareFromResultEndIndex = 8;

        public const int PowDifficultyShareFromFirstNumberStartIndex = 8;
        public const int PowDifficultyShareFromFirstNumberEndIndex = 16;

        public const int PowDifficultyShareFromSecondNumberStartIndex = 16;
        public const int PowDifficultyShareFromSecondtNumberEndIndex = 32;

        #endregion

        #region Mandatory PoW values properties.

        public const int MaxNonceValue = int.MaxValue - 1;
        public const decimal MaxPercentBlockPowValueTarget = 100.00000001m;

        #endregion
    }
}
