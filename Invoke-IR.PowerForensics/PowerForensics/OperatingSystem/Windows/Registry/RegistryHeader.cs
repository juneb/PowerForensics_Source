using System;
using System.Text;
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

        public static RegistryHeader Get(byte[] bytes)
        {
            return new RegistryHeader(bytes);
        }
    }
}
