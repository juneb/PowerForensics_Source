using System;
using System.Text;
using System.Collections.Generic;
using InvokeIR.PowerForensics.NTFS;

namespace InvokeIR.PowerForensics.OS.Windows.Registry
{
    public class RegistryHeader
    {
        #region Enums

        enum FILE_TYPE
        {
            Normal = 0x00,
            TransactionLog = 0x01
        }

        #endregion Enums

        #region Properties

        public readonly string Signature;
        public readonly uint PrimarySequenceNumber;
        public readonly uint SecondarySequenceNumber;
        public readonly DateTime ModificationTime;
        public readonly Version Version;
        public readonly string FileType;
        public readonly uint RootKeyOffset;
        public readonly uint HiveBinsDataSize;
        public readonly uint Checksum;

        #endregion Properties

        #region Constructors

        internal RegistryHeader(byte[] bytes)
        {
            #region Signature
            
            byte[] sigBytes = new byte[4];
            Array.Copy(bytes, 0x00, sigBytes, 0, sigBytes.Length);
            Signature = Encoding.ASCII.GetString(sigBytes);

            if(Signature != "regf")
            {
                throw new Exception("Invalid Registry Header Signature");
            }
            
            #endregion Signature

            PrimarySequenceNumber = BitConverter.ToUInt32(bytes, 0x04);
            SecondarySequenceNumber = BitConverter.ToUInt32(bytes, 0x08);
            ModificationTime = DateTime.FromFileTimeUtc(BitConverter.ToInt64(bytes, 0x0C));
            Version = new Version(BitConverter.ToInt32(bytes, 0x14), BitConverter.ToInt32(bytes, 0x18));
            FileType = Enum.GetName(typeof(FILE_TYPE), 0x1C);
            RootKeyOffset = BitConverter.ToUInt32(bytes, 0x24);
            HiveBinsDataSize = BitConverter.ToUInt32(bytes, 0x28);
            Checksum = BitConverter.ToUInt32(bytes, 0x1FC);
        }

        #endregion Constructors
    }

    public class Registry
    {
        /*
        public static HiveBinHeader[] GetInstances(string path)
        {
            // Get bytes for the specific record
            string volume = "\\\\.\\" + path.Split('\\')[0];
            IndexEntry entry = IndexEntry.Get(path);
            FileRecord record = new FileRecord(FileRecord.GetRecordBytes(volume, (int)entry.RecordNumber), volume);
            byte[] bytes = record.GetBytes(volume);

            // Registry Header
            byte[] headerBytes = new byte[0x200];
            Array.Copy(bytes, 0, headerBytes, 0, headerBytes.Length);
            RegistryHeader header = new RegistryHeader(headerBytes);

            // Hive Bin Headers
            HiveBinHeader[] headerArray = new HiveBinHeader[header.HiveBinsDataSize / 0x1000];
            byte[] hbinHeaderBytes = new byte[0x20];
            uint i = 0x1000;
            while (i < header.HiveBinsDataSize + 0x1000)
            {
                Array.Copy(bytes, i, hbinHeaderBytes, 0, hbinHeaderBytes.Length);
                HiveBinHeader hbinHeader = new HiveBinHeader(hbinHeaderBytes);
                headerArray[((i / 0x1000) - 1)] = hbinHeader;
                i += hbinHeader.hBinSize;
            }

            return headerArray;
        }
        */

        public static NamedKey GetInstances(string path)
        {
            // Get bytes for the specific record
            string volume = "\\\\.\\" + path.Split('\\')[0];
            IndexEntry entry = IndexEntry.Get(path);
            FileRecord record = new FileRecord(FileRecord.GetRecordBytes(volume, (int)entry.RecordNumber), volume, true);
            byte[] bytes = record.GetBytes(volume);

            #region RegistryHeader

            byte[] headerBytes = new byte[0x200];
            Array.Copy(bytes, 0, headerBytes, 0, headerBytes.Length);
            RegistryHeader header = new RegistryHeader(headerBytes);

            #endregion RegistryHeader

            // Hive Bin Headers
            byte[] hbinHeaderBytes = new byte[0x20];
            uint i = 0x1000;
            List<Cell> cellList = new List<Cell>();

            while (i < header.HiveBinsDataSize + 0x1000)
            {
                Array.Copy(bytes, i, hbinHeaderBytes, 0, hbinHeaderBytes.Length);
                HiveBinHeader hbinHeader = new HiveBinHeader(hbinHeaderBytes);
                int j = 0x20;
                /*while(j < hbinHeader.hBinSize)
                {
                    int size = BitConverter.ToInt32(bytes, j);
                    byte[] cellBytes = new byte[size];
                    Array.Copy(bytes, j, cellBytes, 0, cellBytes.Length);
                    Cell cell = CellFactory.Get(cellBytes);
                    cellList.Add(cell);
                    j += size;
                }
                i += hbinHeader.hBinSize;*/
                int size = Math.Abs(BitConverter.ToInt32(bytes, j));
                Console.WriteLine(size);
                byte[] cellBytes = new byte[size];
                Array.Copy(bytes, j + 0x04, cellBytes, 0, cellBytes.Length);
                return new NamedKey(cellBytes, "nk");
            }
            //return cellList.ToArray();
            return null;
        }
    }
}
