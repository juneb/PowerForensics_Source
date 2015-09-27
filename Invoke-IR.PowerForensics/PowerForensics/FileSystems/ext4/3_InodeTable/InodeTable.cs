using System;
using System.IO;
using InvokeIR.Win32;

namespace InvokeIR.PowerForensics.Ext3
{
    public class InodeTable
    {
        internal static byte[] GetBytes(FileStream streamToRead, Superblock superblock, uint offset)
        {
            // Derive Length of Inode Table from superblock values
            uint length = superblock.InodesPerGroup * Inode.EXT4_INODE_SIZE;

            // Get Inode Table bytes
            return NativeMethods.readDrive(streamToRead, offset, length);
        }
    }
}