using System;
using PowerForensics.Ntfs;

namespace PowerForensics.EseDb
{
    public class Page
    {
        #region Enums

        [FlagsAttribute]
        public enum JET_PAGEFLAGS : uint
        {
            IsRoot = 0x0001,
            IsLeaf = 0x0002,
            IsParent = 0x0004,
            IsEmpty = 0x0008,
            IsSpaceTree = 0x0020,
            IsIndex = 0x0040,
            IsLongValue = 0x0080,
            Unknown1 = 0x0400,
            Unknown2 = 0x0800,
            NewRecordFormat = 0x2000,
            NewChecksumFormat = 0x2000,
            IsScrubbed = 0x4000,
            Unknown3 = 0x8000,
        }

        #endregion Enums

        #region Properties

        public uint PageNumber;
        public readonly ulong Checksum;
        public readonly JetDbTime LastModificationTime;
        public readonly uint PreviousPageNumber;
        public readonly uint NextPageNumber;
        public readonly uint FatherDataPage;
        public readonly ushort AvailableDataSize;
        public readonly ushort AvailableUncommittedDataSize;
        public readonly ushort FirstAvailableDataOffset;
        public readonly ushort FirstAvailablePageTag;
        public readonly Page.JET_PAGEFLAGS PageFlags;
        public DataTag[] Tags;

        #endregion Properties

        #region Constructors

        internal Page(byte[] bytes, int offset)
        {
            JetHeader h = new JetHeader(bytes);
            PageNumber = GetPageNumber((uint)offset, h.PageSize);
            Checksum = BitConverter.ToUInt64(bytes, 0x00 + offset);
            LastModificationTime = new JetDbTime(bytes, offset + 0x08);
            PreviousPageNumber = BitConverter.ToUInt32(bytes, 0x10 + offset);
            NextPageNumber = BitConverter.ToUInt32(bytes, 0x14 + offset);
            FatherDataPage = BitConverter.ToUInt32(bytes, 0x18 + offset);
            AvailableDataSize = BitConverter.ToUInt16(bytes, 0x1C + offset);
            AvailableUncommittedDataSize = BitConverter.ToUInt16(bytes, 0x1E + offset);
            FirstAvailableDataOffset = BitConverter.ToUInt16(bytes, 0x20 + offset);
            FirstAvailablePageTag = BitConverter.ToUInt16(bytes, 0x22 + offset);
            PageFlags = ((Page.JET_PAGEFLAGS)BitConverter.ToUInt32(bytes, 0x24 + offset));
            Tags = DataTag.GetInstances(bytes, offset);
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

        public static Page Get(string path, uint pagenumber)
        {
            FileRecord record = FileRecord.Get(path, true);
            byte[] bytes = record.GetContent();
            JetHeader header = new JetHeader(bytes);

            uint offset = GetPageOffset(pagenumber, header.PageSize);
            return Get(bytes, (int)offset);
        }

        private static Page Get(byte[] bytes, int offset)
        {
            return new Page(bytes, offset);
            
            //JET_PAGEFLAGS flags = (JET_PAGEFLAGS)BitConverter.ToUInt32(bytes, offset + 0x24);

            /*if ((flags & JET_PAGEFLAGS.IsSpaceTree) == JET_PAGEFLAGS.IsSpaceTree)
            {
                return new SpaceTreePage(bytes, offset);
            }
            else if ((flags & JET_PAGEFLAGS.IsRoot) == JET_PAGEFLAGS.IsRoot)
            {
                return new RootPage(bytes, offset);
            }
            else if ((flags & JET_PAGEFLAGS.IsLeaf) == JET_PAGEFLAGS.IsLeaf)
            {
                return new LeafPage(bytes, offset);
            }
            else if ((flags & JET_PAGEFLAGS.IsEmpty) == JET_PAGEFLAGS.IsEmpty)
            {
                return null;
            }
            else
            {
                return new BranchPage(bytes, offset);
            }*/
        }

        public static Page[] GetInstances(string path)
        {
            FileRecord record = FileRecord.Get(path, true);
            byte[] bytes = record.GetContent();
            
            return GetInstances(bytes);
        }

        private static Page[] GetInstances(byte[] bytes)
        {
            JetHeader header = new JetHeader(bytes);

            uint PageCount = ((uint)bytes.Length / header.PageSize) - 2;

            Page[] pageArray = new Page[PageCount];

            for (uint i = 1; i < PageCount; i++)
            {
                pageArray[i - 1] = Get(bytes, (int)GetPageOffset(i, header.PageSize));
            }
            return pageArray; 
        }

        #endregion StaticMethods
    }
}
