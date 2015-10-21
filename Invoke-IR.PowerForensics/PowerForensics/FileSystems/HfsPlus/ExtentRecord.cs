using System;

namespace PowerForensics.HfsPlus
{
    public class ExtentRecord
    {
        #region Properties

        public readonly ulong LogicalSize;
        public readonly uint ClumpSize;
        public readonly uint TotalBlocks;
        public readonly uint StartBlock1;
        public readonly uint BlockCount1;
        public readonly uint StartBlock2;
        public readonly uint BlockCount2;
        public readonly uint StartBlock3;
        public readonly uint BlockCount3;
        public readonly uint StartBlock4;
        public readonly uint BlockCount4;
        public readonly uint StartBlock5;
        public readonly uint BlockCount5;
        public readonly uint StartBlock6;
        public readonly uint BlockCount6;
        public readonly uint StartBlock7;
        public readonly uint BlockCount7;
        public readonly uint StartBlock8;
        public readonly uint BlockCount8;

        #endregion Properties

        #region Constructors

        internal ExtentRecord(byte[] bytes)
        {
            LogicalSize = BitConverter.ToUInt64(bytes, 0x00);
            ClumpSize = BitConverter.ToUInt32(bytes, 0x08);
            TotalBlocks = BitConverter.ToUInt32(bytes, 0x0C);
            StartBlock1 = BitConverter.ToUInt32(bytes, 0x10);
            BlockCount1 = BitConverter.ToUInt32(bytes, 0x14);
            StartBlock2 = BitConverter.ToUInt32(bytes, 0x18);
            BlockCount2 = BitConverter.ToUInt32(bytes, 0x1C);
            StartBlock3 = BitConverter.ToUInt32(bytes, 0x20);
            BlockCount3 = BitConverter.ToUInt32(bytes, 0x24);
            StartBlock4 = BitConverter.ToUInt32(bytes, 0x28);
            BlockCount4 = BitConverter.ToUInt32(bytes, 0x2C);
            StartBlock5 = BitConverter.ToUInt32(bytes, 0x30);
            BlockCount5 = BitConverter.ToUInt32(bytes, 0x34);
            StartBlock6 = BitConverter.ToUInt32(bytes, 0x38);
            BlockCount6 = BitConverter.ToUInt32(bytes, 0x3C);
            StartBlock7 = BitConverter.ToUInt32(bytes, 0x40);
            BlockCount7 = BitConverter.ToUInt32(bytes, 0x44);
            StartBlock8 = BitConverter.ToUInt32(bytes, 0x48);
            BlockCount8 = BitConverter.ToUInt32(bytes, 0x4C);
        }

        #endregion Constructors
    }
}
