using System;
using System.IO;
using InvokeIR.Win32;

namespace InvokeIR.PowerForensics.ext4
{
    public class BlockGroupDescriptor
    {
        #region Enums

        [FlagsAttribute]
        internal enum BG_FLAGS
        {
            EXT4_BG_INODE_UNINIT = 0x1,
            EXT4_BG_BLOCK_UNINIT = 0x2,
            EXT4_BG_INODE_ZEROED = 0x4
        }

        #endregion Enums

        #region Properties

        public readonly ulong BlockBitmapLocation;
        public readonly ulong InodeBitmapLocation;
        public readonly ulong InodeTableLocation;
        public readonly uint FreeBlockCount;
        public readonly uint FreeInodeCount;
        public readonly uint DirectoryCount;
        public readonly ulong ExclusionBitmapLocation;
        public readonly uint BlockBitmapChecksum;
        public readonly uint InodeBitmapChecksum;
        public readonly uint UnusedInodeCount;
        public readonly string Flags;
        public readonly ushort Checksum;
        
        #endregion Properties

        #region Constructors

        internal BlockGroupDescriptor(byte[] bytes)
        {
            #region BlockBitmapLocation
            uint BlockBitmapLocation_Low = BitConverter.ToUInt32(bytes, 0x00);
            Console.WriteLine("Low: {0}", BlockBitmapLocation_Low);
            uint BlockBitmapLocation_High = BitConverter.ToUInt32(bytes, 0x20);
            Console.WriteLine("High: {0}", BlockBitmapLocation_High);
            BlockBitmapLocation = BlockBitmapLocation_High;
            BlockBitmapLocation = (BlockBitmapLocation << 32) + BlockBitmapLocation_Low;
            #endregion BlockBitmapLocation            
            #region InodeBitmapLocation
            uint InodeBitmapLocation_Low = BitConverter.ToUInt32(bytes, 0x04);
            uint InodeBitmapLocation_High = BitConverter.ToUInt32(bytes, 0x24);
            InodeBitmapLocation = InodeBitmapLocation_High;
            InodeBitmapLocation = (InodeBitmapLocation << 32) + InodeBitmapLocation_Low;
            #endregion InodeBitmapLocation            
            #region InodeTableLocation
            uint InodeTableLocation_Low = BitConverter.ToUInt32(bytes, 0x08);
            uint InodeTableLocation_High = BitConverter.ToUInt32(bytes, 0x28);
            InodeTableLocation = InodeTableLocation_High;
            InodeTableLocation = (InodeTableLocation << 32) + InodeTableLocation_Low;
            #endregion InodeTableLocation            
            #region FreeBlockCount
            ushort FreeBlockCount_Low = BitConverter.ToUInt16(bytes, 0x0C);
            ushort FreeBlockCount_High = BitConverter.ToUInt16(bytes, 0x2C);
            FreeBlockCount = FreeBlockCount_High;
            FreeBlockCount = (FreeBlockCount << 16) + FreeBlockCount_Low;
            #endregion FreeBlockCount             
            #region FreeInodeCount
            ushort FreeInodeCount_Low = BitConverter.ToUInt16(bytes, 0x0E);
            ushort FreeInodeCount_High = BitConverter.ToUInt16(bytes, 0x2E);
            FreeInodeCount = FreeInodeCount_High;
            FreeInodeCount = (FreeInodeCount << 16) + FreeInodeCount_Low;
            #endregion FreeInodeCount            
            #region DirectoryCount
            ushort DirectoryCount_Low = BitConverter.ToUInt16(bytes, 0x10);
            ushort DirectoryCount_High = BitConverter.ToUInt16(bytes, 0x30);
            DirectoryCount = DirectoryCount_High;
            DirectoryCount = (DirectoryCount << 16) + DirectoryCount_Low;
            #endregion DirectoryCount            
            #region ExclusionBitmapLocation
            uint ExclusionBitmapLocation_Low = BitConverter.ToUInt32(bytes, 0x14);
            uint ExclusionBitmapLocation_High = BitConverter.ToUInt32(bytes, 0x34);
            ExclusionBitmapLocation = ExclusionBitmapLocation_High;
            ExclusionBitmapLocation = (ExclusionBitmapLocation << 32) + ExclusionBitmapLocation_Low;
            #endregion ExclusionBitmapLocation            
            #region BlockBitmapChecksum
            ushort BlockBitmapChecksum_Low = BitConverter.ToUInt16(bytes, 0x18);
            ushort BlockBitmapChecksum_High = BitConverter.ToUInt16(bytes, 0x38);
            BlockBitmapChecksum = BlockBitmapChecksum_High;
            BlockBitmapChecksum = (BlockBitmapChecksum << 16) + BlockBitmapChecksum_Low;
            #endregion BlockBitmapChecksum
            #region InodeBitmapChecksum
            ushort InodeBitmapChecksum_Low = BitConverter.ToUInt16(bytes, 0x1A);
            ushort InodeBitmapChecksum_High = BitConverter.ToUInt16(bytes, 0x3A);
            InodeBitmapChecksum = InodeBitmapChecksum_High;
            InodeBitmapChecksum = (InodeBitmapChecksum << 16) + InodeBitmapChecksum_Low;
            #endregion InodeBitmapChecksum
            #region UnusedInodeCount
            ushort UnusedInodeCount_Low = BitConverter.ToUInt16(bytes, 0x1C);
            ushort UnusedInodeCount_High = BitConverter.ToUInt16(bytes, 0x32);
            UnusedInodeCount = UnusedInodeCount_High;
            UnusedInodeCount = (UnusedInodeCount << 16) + UnusedInodeCount_Low;
            #endregion UnusedInodeCount
            Flags = ((BG_FLAGS)BitConverter.ToUInt16(bytes, 0x12)).ToString();
            Checksum = BitConverter.ToUInt16(bytes, 0x1E);
        }

        #endregion Constructors

        public static BlockGroupDescriptor GetBytes(string devicePath, ulong offset)
        {
            IntPtr hDevice = NativeMethods.getHandle(devicePath);
            using (FileStream streamToRead = NativeMethods.getFileStream(hDevice))
            {
                // Get BlockGroupDescriptor bytes
                byte[] bytes = NativeMethods.readDrive(streamToRead, (offset * 512) + 0x1000, 0x200);
                return new BlockGroupDescriptor(bytes);
            }
        }
    }
}
