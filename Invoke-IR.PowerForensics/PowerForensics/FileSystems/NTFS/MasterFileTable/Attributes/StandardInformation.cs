using System;

namespace InvokeIR.PowerForensics.NTFS
{
    #region StandardInformationClass

    public class StandardInformation : Attr
    {
        #region Enums
        
        [FlagsAttribute]
        internal enum ATTR_STDINFO_PERMISSION : uint
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
            ENCRYPTED = 0x00004000
        }

        #endregion Enums

        #region Properties

        public readonly DateTime BornTime;
        public readonly DateTime ModifiedTime;
        public readonly DateTime ChangedTime;
        public readonly DateTime AccessedTime;
        public readonly string Permission;
        public readonly uint MaxVersionNumber;
        public readonly uint VersionNumber;
        public readonly uint ClassId;
        public readonly uint OwnerId;
        public readonly uint SecurityId;
        public readonly ulong QuotaCharged;
        public readonly ulong UpdateSequenceNumber;

        #endregion Properties

        #region Constructors

        internal StandardInformation(ResidentHeader header, byte[] attrBytes, string attrName)
        {
            Name = Enum.GetName(typeof(ATTR_TYPE), header.commonHeader.ATTRType);
            NameString = attrName;
            NonResident = header.commonHeader.NonResident;
            AttributeId = header.commonHeader.Id;

            BornTime = DateTime.FromFileTimeUtc(BitConverter.ToInt64(attrBytes, 0x00));
            ModifiedTime = DateTime.FromFileTimeUtc(BitConverter.ToInt64(attrBytes, 0x08));
            ChangedTime = DateTime.FromFileTimeUtc(BitConverter.ToInt64(attrBytes, 0x10));
            AccessedTime = DateTime.FromFileTimeUtc(BitConverter.ToInt64(attrBytes, 0x18));
            Permission = ((ATTR_STDINFO_PERMISSION)BitConverter.ToUInt32(attrBytes, 0x20)).ToString();
            MaxVersionNumber = BitConverter.ToUInt32(attrBytes, 0x24);
            VersionNumber = BitConverter.ToUInt32(attrBytes, 0x28);
            ClassId = BitConverter.ToUInt32(attrBytes, 0x2C);
            
            if (attrBytes.Length == 0x48)
            {
                OwnerId = BitConverter.ToUInt32(attrBytes, 0x30);
                SecurityId = BitConverter.ToUInt32(attrBytes, 0x34);
                QuotaCharged = BitConverter.ToUInt64(attrBytes, 0x38);
                UpdateSequenceNumber = BitConverter.ToUInt64(attrBytes, 0x40);
            }
        }

        #endregion Constructors
    }

    #endregion StandardInformationClass
}
