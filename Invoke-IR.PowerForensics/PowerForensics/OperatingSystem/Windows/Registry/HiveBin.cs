using System;
using System.Text;
using InvokeIR.PowerForensics.NTFS;

namespace InvokeIR.PowerForensics.OS.Windows.Registry
{
    public class HiveBinHeader
    {
        #region Properties

        public readonly string Signature;
        public readonly uint hBinOffset;
        public readonly uint hBinSize;

        #endregion Properties

        #region Constructors

        internal HiveBinHeader(byte[] bytes)
        {
            #region Signature

            byte[] sigBytes = new byte[4];
            Array.Copy(bytes, 0, sigBytes, 0, sigBytes.Length);
            Signature = Encoding.ASCII.GetString(sigBytes);

            if (Signature != "hbin")
            {
                throw new Exception("Invalid HiveBinHeader found.");
            }

            #endregion Signature

            hBinOffset = BitConverter.ToUInt32(bytes, 0x04);
            hBinSize = BitConverter.ToUInt32(bytes, 0x08);
        }

        #endregion Constructors

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
    }
}
