using System;
using System.Text;
using InvokeIR.PowerForensics.NTFS;

namespace InvokeIR.PowerForensics.Registry
{
    public class RegistryHeader
    {
        #region Constants

        internal const int HBINOFFSET = 0x1000;

        #endregion Constants

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
                throw new Exception("Invalid Registry Header");
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

        #region PublicMethods
        
        public static byte[] GetBytes(string path)
        {
            // Get bytes for the specific record
            string volume = "\\\\.\\" + path.Split('\\')[0];
            IndexEntry entry = IndexEntry.Get(path);
            FileRecord record = new FileRecord(FileRecord.GetRecordBytes(volume, (int)entry.RecordNumber), volume);
            byte[] bytes = record.GetBytes(volume);

            // Registry Header
            byte[] headerBytes = new byte[0x200];
            Array.Copy(bytes, 0, headerBytes, 0, headerBytes.Length);
            return headerBytes;
        }

        public static RegistryHeader Get(string path)
        {
            return new RegistryHeader(RegistryHeader.GetBytes(path));
        }

        #endregion PublicMethods
    }
}
