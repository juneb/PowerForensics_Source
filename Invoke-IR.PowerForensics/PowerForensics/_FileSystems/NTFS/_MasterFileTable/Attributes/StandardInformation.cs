using System;
using System.Linq;
using System.Text;
using InvokeIR.PowerForensics.NTFS;

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

        internal StandardInformation(byte[] attrBytes, string attrName)
        {
            AttrHeader.ATTR_HEADER_RESIDENT header = new AttrHeader.ATTR_HEADER_RESIDENT(attrBytes.Take(24).ToArray());
            Name = Enum.GetName(typeof(ATTR_TYPE), header.commonHeader.ATTRType);
            NameString = attrName;
            NonResident = header.commonHeader.NonResident;
            AttributeId = header.commonHeader.Id;

            BornTime = DateTime.FromFileTime(BitConverter.ToInt64(attrBytes, 24));
            ModifiedTime = DateTime.FromFileTime(BitConverter.ToInt64(attrBytes, 32));
            ChangedTime = DateTime.FromFileTime(BitConverter.ToInt64(attrBytes, 40));
            AccessedTime = DateTime.FromFileTime(BitConverter.ToInt64(attrBytes, 48));
            Permission = ((ATTR_STDINFO_PERMISSION)BitConverter.ToUInt32(attrBytes, 56)).ToString();
            if (attrBytes.Length > 72)
            {
                MaxVersionNumber = BitConverter.ToUInt32(attrBytes, 60);
                VersionNumber = BitConverter.ToUInt32(attrBytes, 64);
                ClassId = BitConverter.ToUInt32(attrBytes, 68);
                OwnerId = BitConverter.ToUInt32(attrBytes, 72);
                SecurityId = BitConverter.ToUInt32(attrBytes, 76);
                QuotaCharged = BitConverter.ToUInt64(attrBytes, 80);
                UpdateSequenceNumber = BitConverter.ToUInt64(attrBytes, 88);
            }
        }

        #endregion Constructors
    }

    #endregion StandardInformationClass
}
