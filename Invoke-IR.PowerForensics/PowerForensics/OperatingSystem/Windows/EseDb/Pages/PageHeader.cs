using System;
using System.Text;
using InvokeIR.Win32;

namespace InvokeIR.PowerForensics.EseDb
{
    #region PageHeaderClass

    public class PageHeader
    {
        #region Enums

        [FlagsAttribute]
        public enum JET_PAGEFLAGS
        {
            Root = 0x0001,
            Leaf = 0x0002,
            Parent = 0x0004,
            Empty = 0x0008,
            SpaceTreePage = 0x0020,
            IndexPage = 0x0040,
            LongValuePage = 0x0080,
            NewRecordFormat = 0x2000,
            Scrubbed = 0x4000
        }

        #endregion Enums

        #region Properties

        public readonly ulong Checksum;
        public readonly byte[] LastModificationTime;
        public readonly uint PreviousPageNumber;
        public readonly uint NextPageNumber;
        public readonly uint FatherDataPage;
        public readonly ushort AvailableDataSize;
        public readonly ushort AvailableUncommittedDataSize;
        public readonly ushort FirstAvailableDataOffset;
        public readonly ushort FirstAvailablePageTag;
        public readonly JET_PAGEFLAGS PageFlags;
        public readonly ulong PageNumber;

        #endregion Properties

        #region Constructors

        internal PageHeader(byte[] bytes)
        {
            Checksum = BitConverter.ToUInt64(bytes, 0x00);
            LastModificationTime = NativeMethods.GetSubArray(bytes, 0x08, 0x08);
            PreviousPageNumber = BitConverter.ToUInt32(bytes, 0x10);
            NextPageNumber = BitConverter.ToUInt32(bytes, 0x14);
            FatherDataPage = BitConverter.ToUInt32(bytes, 0x18);
            AvailableDataSize = BitConverter.ToUInt16(bytes, 0x1C);
            AvailableUncommittedDataSize = BitConverter.ToUInt16(bytes, 0x1E);
            FirstAvailableDataOffset = BitConverter.ToUInt16(bytes, 0x20);
            FirstAvailablePageTag = BitConverter.ToUInt16(bytes, 0x22);
            PageFlags = (JET_PAGEFLAGS)BitConverter.ToUInt32(bytes, 0x24);
            PageNumber = BitConverter.ToUInt64(bytes, 0x40);
        }

        #endregion Constructors

        #region StaticMethods

        internal static uint GetPageOffset(uint PageNumber, uint PageSize)
        {
            return (PageNumber + 1) * PageSize;
        }

        internal static uint GetPageNumber(uint PageOffset, uint PageSize)
        {
            return (PageOffset / PageSize) - 1;
        }

        #endregion StaticMethods
    }
    
    #endregion PageHeaderClass
}
