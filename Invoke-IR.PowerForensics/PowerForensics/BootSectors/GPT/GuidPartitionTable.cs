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
            MasterBootRecord mbr = MasterBootRecord.Get(devicePath);
            if (mbr.PartitionTable[0].SystemID == "EFI_GPT_DISK")
            {
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

    public class GuidPartitionTableHeader
    {
        #region Constants

        private const string SIGNATURE_STRING = "EFI PART";

        #endregion Constants

        #region Properties

        internal readonly string Signature;
        internal readonly Version Revision;
        internal readonly uint HeaderSize;
        internal readonly uint HeaderCRC32;
        internal readonly ulong MyLBA;
        internal readonly ulong AlternateLBA;
        internal readonly ulong FirstUsableLBA;
        internal readonly ulong LastUsableLBA;
        internal readonly Guid DiskGUID;
        internal readonly ulong PartitionEntryLBA;
        internal readonly uint NumberOfPartitionEntries;
        internal readonly uint SizeOfPartitionEntry;
        internal readonly byte[] PartitionEntryArrayCRC32;

        #endregion Properties

        #region Constructors

        internal GuidPartitionTableHeader(byte[] bytes)
        {
            // Test GPT Signature
            byte[] SignatureArray = new byte[8];
            Array.Copy(bytes, SignatureArray, 8);
            Signature = Encoding.ASCII.GetString(SignatureArray);
            if (Signature != SIGNATURE_STRING)
            {
                throw new Exception("Invalid GPT Signature");
            }

            // Get Revision Number
            ushort revMinor = BitConverter.ToUInt16(bytes, 8);
            ushort revMajor = BitConverter.ToUInt16(bytes, 10);
            Revision = new Version(revMajor, revMinor);

            // Get Header Size
            HeaderSize = BitConverter.ToUInt32(bytes, 12);

            // Get HeaderCRC32 Value

            // Get/Test MyLBA
            MyLBA = BitConverter.ToUInt64(bytes, 24);
            if (MyLBA != 1)
            {
                throw new Exception("Invalid MyLBA property value");
            }

            AlternateLBA = BitConverter.ToUInt64(bytes, 32);
            FirstUsableLBA = BitConverter.ToUInt64(bytes, 40);
            LastUsableLBA = BitConverter.ToUInt64(bytes, 48);

            // Get DiskGUID Value
            byte[] diskGUIDBytes = new byte[16];
            Array.Copy(bytes, 56, diskGUIDBytes, 0, diskGUIDBytes.Length);
            DiskGUID = new Guid(diskGUIDBytes);

            PartitionEntryLBA = BitConverter.ToUInt64(bytes, 72);
            NumberOfPartitionEntries = BitConverter.ToUInt32(bytes, 80);
            SizeOfPartitionEntry = BitConverter.ToUInt32(bytes, 84);

            // Get PartitionEntryArrayCRC32 Value
        }

        #endregion Constructors
    }

    public class GuidPartitionTableEntry
    {

        #region Enums

        /*enum PARTITION_TYPE_GUID : string
        {
            UNUSED_ENTRY = "00000000-0000-0000-0000-000000000000",
            EFI_SYSTEM_PARTITION = "C12A7328-F81F-11D2-BA4B-00A0C93EC93B",
            LEGACY_MBR = "024DEE41-33E7-11D3-9D69-0008C781F39F"
        }*/

        [FlagsAttribute]
        enum PARTITION_ATTRIBUTE
        {
            RequirePartition = 0x01,
            NoBlockIOProtocol = 0x02,
            LegacyBIOSBootable = 0x04
        }

        #endregion Enums

        #region Properties

        public readonly Guid PartitionTypeGUID;
        public readonly Guid UniquePartitionGUID;
        public readonly ulong StartingLBA;
        public readonly ulong EndingLBA;
        public readonly string Attributes;
        public readonly string PartitionName;

        #endregion Properties

        #region Constructors

        internal GuidPartitionTableEntry(byte[] bytes)
        {
            // Get PartitionTypeGuid Value
            byte[] partitionTypeGuidBytes = new byte[16];
            Array.Copy(bytes, 0, partitionTypeGuidBytes, 0, partitionTypeGuidBytes.Length);
            PartitionTypeGUID = new Guid(partitionTypeGuidBytes);

            // Get UniquePartitionGuid Value
            byte[] uniquePartitionGuidBytes = new byte[16];
            Array.Copy(bytes, 16, uniquePartitionGuidBytes, 0, uniquePartitionGuidBytes.Length);
            UniquePartitionGUID = new Guid(uniquePartitionGuidBytes);

            StartingLBA = BitConverter.ToUInt64(bytes, 32);
            EndingLBA = BitConverter.ToUInt64(bytes, 40);

            // Get Attributes Value
            Attributes = ((PARTITION_ATTRIBUTE)(BitConverter.ToUInt64(bytes, 48))).ToString();

            // Get the name of the Partition
            byte[] nameBytes = new byte[72];
            Array.Copy(bytes, 56, nameBytes, 0, nameBytes.Length);
            PartitionName = Encoding.Unicode.GetString(nameBytes).Split('\0')[0];
        }

        #endregion Constructors

    }

    #endregion GuidPartitionTableClass
}
