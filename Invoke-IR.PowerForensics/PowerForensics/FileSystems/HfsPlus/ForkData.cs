using System;
using InvokeIR.Win32;

namespace PowerForensics.HfsPlus
{
    public class ForkData
    {
        #region Properties

        public readonly ulong LogicalSize;
        public readonly uint ClumpSize;
        public readonly uint TotalBlocks;
        ExtentRecord Extents;

        #endregion Properties

        #region Constructors

        public ForkData(byte[] bytes)
        {
            LogicalSize = BitConverter.ToUInt64(bytes, 0x00);
            ClumpSize = BitConverter.ToUInt32(bytes, 0x08);
            TotalBlocks = BitConverter.ToUInt32(bytes, 0x0C);
            Extents = new ExtentRecord(NativeMethods.GetSubArray(bytes, 0x10, 0x08));
        }

        #endregion Constructors
    }
}
