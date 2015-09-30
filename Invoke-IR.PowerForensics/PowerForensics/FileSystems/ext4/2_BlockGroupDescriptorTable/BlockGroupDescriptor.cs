using System;
using System.IO;
using InvokeIR.Win32;

namespace PowerForensics.Ext3
{
    public class BlockGroupDescriptor
    {
        #region Constants

        private const uint BLOCK_GROUPS_PER_SECTOR = 0x10;
        public const uint BLOCK_GROUP_DESCRIPTOR_LENGTH = 0x20;

        #endregion Constants

        #region Properties

        public readonly uint BlockBitmapOffset;
        public readonly uint InodeBitmapOffset;
        public readonly uint InodeTableOffset;
        public readonly ushort FreeBlockCount;
        public readonly ushort FreeInodeCount;
        public readonly ushort UsedDirectoryCount;

        #endregion Properties

        #region Constructors

        internal BlockGroupDescriptor(byte[] bytes)
        {
            BlockBitmapOffset = BitConverter.ToUInt32(bytes, 0x00);
            InodeBitmapOffset = BitConverter.ToUInt32(bytes, 0x04);
            InodeTableOffset = BitConverter.ToUInt32(bytes, 0x08);
            FreeBlockCount = BitConverter.ToUInt16(bytes, 0x0C);
            FreeInodeCount = BitConverter.ToUInt16(bytes, 0x0E);
            UsedDirectoryCount = BitConverter.ToUInt16(bytes, 0x10);
        }

        #endregion Constructors

        internal static byte[] GetBytes(FileStream streamToRead, uint superblockOffset, Superblock superBlock, uint group)
        {
            // Derive the location and length of the Block Group Descriptor Table
            uint bgdtOffset = (superblockOffset * NativeMethods.BYTES_PER_SECTOR) + ((superBlock.FirstDataBlock + 1) * superBlock.BlockSize);
            uint bgdtEntries = (superBlock.TotalBlockCount / superBlock.BlocksPerGroup) + 1;
            uint bgdtLength = bgdtEntries * BlockGroupDescriptor.BLOCK_GROUP_DESCRIPTOR_LENGTH;

            // Determine the offset from the beginning of the BGDT to the desired group
            uint groupOffset = group * BLOCK_GROUP_DESCRIPTOR_LENGTH;

            // Determine what Sector contains the raw bytes
            uint groupSectorOffset = groupOffset / NativeMethods.BYTES_PER_SECTOR;

            // Read the sector that the desired block resides within
            byte[] SectorBytes = NativeMethods.readDrive(streamToRead, bgdtOffset + (groupSectorOffset * NativeMethods.BYTES_PER_SECTOR), NativeMethods.BYTES_PER_SECTOR);

            // Get Block Group Descriptor offset into Sector
            uint sectorOffset = (group % BLOCK_GROUPS_PER_SECTOR) * BLOCK_GROUP_DESCRIPTOR_LENGTH;

            // Create byte[] containing only bytes for the requested Block Group Descriptor
            return NativeMethods.GetSubArray(SectorBytes, sectorOffset, BLOCK_GROUP_DESCRIPTOR_LENGTH);
        }
    }
}
