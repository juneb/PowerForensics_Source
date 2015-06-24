using System;
using System.IO;
using InvokeIR.Win32;

namespace InvokeIR.PowerForensics.ext3
{
    public class BlockGroupDescriptor
    {
        #region Properties

        public readonly uint bg_block_bitmap;
        public readonly uint bg_inode_bitmap;
        public readonly uint bg_inode_table;
        public readonly ushort bg_free_blocks_count;
        public readonly ushort bg_free_inodes_count;
        public readonly ushort bg_used_dirs_count;
        public readonly ushort bg_pad;
        //public readonly byte[] bg_reserved;

        #endregion Properties

        #region Constructors

        public BlockGroupDescriptor(byte[] bytes)
        {
            bg_block_bitmap = BitConverter.ToUInt32(bytes, 0x00);
            bg_inode_bitmap = BitConverter.ToUInt32(bytes, 0x04);
            bg_inode_table = BitConverter.ToUInt32(bytes, 0x08);
            bg_free_blocks_count = BitConverter.ToUInt16(bytes, 0x0C);
            bg_free_inodes_count = BitConverter.ToUInt16(bytes, 0x0E);
            bg_used_dirs_count = BitConverter.ToUInt16(bytes, 0x10);
            bg_pad = BitConverter.ToUInt16(bytes, 0x12);
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
