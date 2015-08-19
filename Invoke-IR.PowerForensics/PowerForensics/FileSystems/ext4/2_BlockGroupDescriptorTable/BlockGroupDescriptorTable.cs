using System;
using System.IO;
using InvokeIR.Win32;

namespace InvokeIR.PowerForensics.ext3
{
    public class BlockGroupDescriptorTable
    {
        internal static byte[] GetBytes(FileStream streamToRead, uint superblockOffset, Superblock superBlock)
        {
            // Derive the location and length of the Block Group Descriptor Table
            uint offset = (superblockOffset * NativeMethods.BYTES_PER_SECTOR) + ((superBlock.FirstDataBlock + 1) * superBlock.BlockSize);
            uint bgdtEntries = (superBlock.TotalBlockCount / superBlock.BlocksPerGroup) + 1;
            uint length = bgdtEntries * BlockGroupDescriptor.BLOCK_GROUP_DESCRIPTOR_LENGTH;
            
            // Ensure the bgdtLength value is a multiple of 512 (minimum value for reading bytes from disk)
            if ((length % NativeMethods.BYTES_PER_SECTOR) != 0)
            {
                Console.WriteLine("length: {0}", length);
                length += (NativeMethods.BYTES_PER_SECTOR - (length % NativeMethods.BYTES_PER_SECTOR));
            }

            // Get BlockGroupDescriptor bytes
            return NativeMethods.readDrive(streamToRead, offset, length);
        }
    }
}
