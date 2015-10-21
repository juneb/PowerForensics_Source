using System;

namespace PowerForensics.HfsPlus
{
    public class ExtentKey
    {
        #region Properties

        public readonly ushort KeyLength;
        public readonly byte ForkType;
        public readonly byte pad;
        CatalogNodeId.CATALOG_ID FileId;
        public readonly uint StartBlock;

        #endregion Properties

        #region Constructors

        internal ExtentKey(byte[] bytes)
        {
            KeyLength = BitConverter.ToUInt16(bytes, 0x00);
            ForkType = bytes[0x02];
            pad = bytes[0x03];
            FileId = (CatalogNodeId.CATALOG_ID)BitConverter.ToUInt32(bytes, 0x04);
            StartBlock = BitConverter.ToUInt32(bytes, 0x08);
        }

        #endregion Constructors
    }
}
