using System;
using System.Text;

namespace InvokeIR.PowerForensics.NTFS
{
    #region FileNameClass

    public class FileName : Attr
    {
        #region Constants

        private const byte ATTR_FILENAME_NAMESPACE_POSIX = 0x00;
        private const byte ATTR_FILENAME_NAMESPACE_WIN32 = 0x01;
        private const byte ATTR_FILENAME_NAMESPACE_DOS = 0x02;

        #endregion Constants

        #region Enums

        enum ATTR_FILENAME_FLAG
        {
            READONLY = 0x00000001,
            HIDDEN = 0x00000002,
            SYSTEM = 0x00000004,
            ARCHIVE = 0x00000020,
            DEVICE = 0x00000040,
            NORMAL = 0x00000080,
            TEMP = 0x00000100,
            SPARSE = 0x00000200,
            REPARSE = 0x00000400,
            COMPRESSED = 0x00000800,
            OFFLINE = 0x00001000,
            NCI = 0x00002000,
            ENCRYPTED = 0x00004000,
            DIRECTORY = 0x10000000,
            INDEXVIEW = 0x20000000
        }

        #endregion Enums
           
        #region Properties

        public readonly string Filename;
        public readonly ulong ParentRecordNumber;
        public readonly int Namespace;
        public readonly ulong AllocatedSize;
        public readonly ulong RealSize;
        public readonly uint Flags;
        public readonly uint ER;
        private readonly byte NameLength;
        public readonly DateTime ModifiedTime;
        public readonly DateTime AccessedTime;
        public readonly DateTime ChangedTime;
        public readonly DateTime BornTime;

        #endregion Properties

        #region Constructors

        public FileName(byte[] bytes)
        {
            Name = "FILE_NAME";

            try
            {
                // FILE_NAME Attribute
                ParentRecordNumber = (BitConverter.ToUInt64(bytes, 0x00) & 0x0000FFFFFFFFFFFF);
                BornTime = DateTime.FromFileTimeUtc(BitConverter.ToInt64(bytes, 0x08));
                ChangedTime = DateTime.FromFileTimeUtc(BitConverter.ToInt64(bytes, 0x10));
                ModifiedTime = DateTime.FromFileTimeUtc(BitConverter.ToInt64(bytes, 0x18));
                AccessedTime = DateTime.FromFileTimeUtc(BitConverter.ToInt64(bytes, 0x20));
                AllocatedSize = BitConverter.ToUInt64(bytes, 0x28);
                RealSize = BitConverter.ToUInt64(bytes, 0x30);
                Flags = BitConverter.ToUInt32(bytes, 0x38);
                ER = BitConverter.ToUInt32(bytes, 0x3C);
                NameLength = bytes[0x40];
                Namespace = Convert.ToInt32(bytes[0x41]);

                // Get FileName
                byte[] nameBytes = new byte[NameLength * 2];
                try
                {
                    Array.Copy(bytes, 0x42, nameBytes, 0, nameBytes.Length);
                    Filename = Encoding.Unicode.GetString(nameBytes).TrimEnd('\0');
                }
                catch
                {
                    throw new Exception("Could not get File Name");
                }
            }
            catch
            {

            }
        }

        internal FileName(ResidentHeader header, byte[] attrBytes, string attrName)
        {
            // Headers
            Name = Enum.GetName(typeof(ATTR_TYPE), header.commonHeader.ATTRType);
            NameString = attrName;
            NonResident = header.commonHeader.NonResident;
            AttributeId = header.commonHeader.Id;
            
            // FILE_NAME Attribute
            ParentRecordNumber = (BitConverter.ToUInt64(attrBytes, 0x00) & 0x0000FFFFFFFFFFFF);
            BornTime = DateTime.FromFileTimeUtc(BitConverter.ToInt64(attrBytes, 0x08));
            ChangedTime = DateTime.FromFileTimeUtc(BitConverter.ToInt64(attrBytes, 0x10));
            ModifiedTime = DateTime.FromFileTimeUtc(BitConverter.ToInt64(attrBytes, 0x18));
            AccessedTime = DateTime.FromFileTimeUtc(BitConverter.ToInt64(attrBytes, 0x20));
            AllocatedSize = BitConverter.ToUInt64(attrBytes, 0x28);
            RealSize = BitConverter.ToUInt64(attrBytes, 0x30);
            Flags = BitConverter.ToUInt32(attrBytes, 0x38);
            ER = BitConverter.ToUInt32(attrBytes, 0x3C);
            NameLength = attrBytes[0x40];
            Namespace = Convert.ToInt32(attrBytes[0x41]);

            // Get FileName
            byte[] nameBytes = new byte[NameLength * 2];
            Array.Copy(attrBytes, 0x42, nameBytes, 0, nameBytes.Length);
            Filename = Encoding.Unicode.GetString(nameBytes).TrimEnd('\0');
        }

        #endregion Constructors
    }

    #endregion FileNameClass
}
