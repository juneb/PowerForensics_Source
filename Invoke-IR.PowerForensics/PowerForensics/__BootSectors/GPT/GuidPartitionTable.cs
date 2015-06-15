using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using InvokeIR.Win32;

namespace InvokeIR.PowerForensics
{
    #region GuidPartitionTableClass

    public class GuidPartitionTable
    {

        #region Constants

        private const ulong GPT_OFFSET = 512;
        private const ulong SECTOR_SIZE = 512;

        #endregion Constants

        #region Properties

        public readonly Version Revision;
        public readonly uint HeaderSize;
        public readonly ulong MyLBA;
        public readonly ulong AlternateLBA;
        public readonly ulong FirstUsableLBA;
        public readonly ulong LastUsableLBA;
        public readonly Guid DiskGUID;
        public readonly ulong PartitionEntryLBA;
        public readonly uint NumberOfPartitionEntries;
        public readonly uint SizeOfPartitionEntry;
        public readonly GuidPartitionTableEntry[] PartitionTable;

        #endregion Properties

        #region Constructors

        public GuidPartitionTable(string devicePath)
        {
            NativeMethods.getDriveName(devicePath);

            MasterBootRecord mbr = MasterBootRecord.Get(devicePath);
            if (mbr.PartitionTable[0].SystemID == "EFI_GPT_DISK")
            {
                NativeMethods.getDriveName(devicePath);
                IntPtr hDevice = NativeMethods.getHandle(devicePath);
                using (FileStream streamToRead = NativeMethods.getFileStream(hDevice))
                {
                    // Get Header
                    byte[] headerBytes = NativeMethods.readDrive(streamToRead, GPT_OFFSET, SECTOR_SIZE);
                    GuidPartitionTableHeader GPTHeader = new GuidPartitionTableHeader(headerBytes);
                    Revision = GPTHeader.Revision;
                    HeaderSize = GPTHeader.HeaderSize;
                    MyLBA = GPTHeader.MyLBA;
                    AlternateLBA = GPTHeader.AlternateLBA;
                    FirstUsableLBA = GPTHeader.FirstUsableLBA;
                    LastUsableLBA = GPTHeader.LastUsableLBA;
                    DiskGUID = GPTHeader.DiskGUID;
                    PartitionEntryLBA = GPTHeader.PartitionEntryLBA;
                    NumberOfPartitionEntries = GPTHeader.NumberOfPartitionEntries;
                    SizeOfPartitionEntry = GPTHeader.SizeOfPartitionEntry;

                    // Get PartitionTable
                    List<GuidPartitionTableEntry> partitionList = new List<GuidPartitionTableEntry>();

                    bool Continue = true;

                    // Iterate through sectors that contain the GPT Entry Array
                    for(ulong j = GPTHeader.PartitionEntryLBA; (j < GPTHeader.PartitionEntryLBA + (GPTHeader.NumberOfPartitionEntries / (SECTOR_SIZE / GPTHeader.SizeOfPartitionEntry))); j++)
                    {
                        // Read one sector
                        byte[] partitionSectorBytes = NativeMethods.readDrive(streamToRead, (SECTOR_SIZE * j), SECTOR_SIZE);
                        
                        // Iterate through Partition Entries in Sector
                        // Sectors (512 bytes) / Partitions (128 bytes) = 4 partitions per sector 
                        for (long i = 0; i < 512; i += GPTHeader.SizeOfPartitionEntry)
                        {
                            // Instantiate byte array of size GPTHeader.SizeOfPartitionEntry (typically 128 bytes)
                            byte[] partitionBytes = new byte[GPTHeader.SizeOfPartitionEntry];
                            // Copy appropriate sector bytes to partitionBytes array
                            Array.Copy(partitionSectorBytes, i, partitionBytes, 0, partitionBytes.Length);
                            // Instantiate a GuidPartitionTableEntry object
                            GuidPartitionTableEntry entry = new GuidPartitionTableEntry(partitionBytes);
                            // If entry's PartitionTypeGUID is 00000000-0000-0000-0000-000000000000 then it is not a partition
                            if (entry.PartitionTypeGUID == new Guid("00000000-0000-0000-0000-000000000000"))
                            {
                                Continue = false;
                                break;
                            }
                            partitionList.Add(entry);
                        }

                        if (!Continue)
                        {
                            break;
                        }
                    }

                    PartitionTable = partitionList.ToArray();

                }
            }
            else
            {
                throw new Exception("No GPT found. Please use Get-MBR cmdlet");
            }
        }

        #endregion Constructors

        #region Methods

        public static byte[] GetBytes(string devicePath)
        {
            MasterBootRecord mbr = MasterBootRecord.Get(devicePath);
            if (mbr.PartitionTable[0].SystemID == "EFI_GPT_DISK")
            {
                Win32.NativeMethods.getDriveName(devicePath);
                IntPtr hDevice = Win32.NativeMethods.getHandle(devicePath);
                using (FileStream streamToRead = NativeMethods.getFileStream(hDevice))
                {
                    return Win32.NativeMethods.readDrive(streamToRead, GPT_OFFSET, SECTOR_SIZE);
                }
            }
            else
            {
                throw new Exception("No GPT found. Please use Get-MBR cmdlet");
            }
        }

        #endregion Methods

    }

    #endregion GuidPartitionTableClass
}
