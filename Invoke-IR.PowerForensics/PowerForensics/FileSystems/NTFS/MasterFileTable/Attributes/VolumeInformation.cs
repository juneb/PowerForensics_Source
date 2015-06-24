using System;

namespace InvokeIR.PowerForensics.NTFS
{
    public class VolumeInformation : Attr
    {
        #region Enums

        [FlagsAttribute]
        enum ATTR_VOLINFO
        {
            FLAG_DIRTY = 0x0001,	// Dirty
            FLAG_RLF = 0x0002,	    // Resize logfile
            FLAG_UOM = 0x0004,	    // Upgrade on mount
            FLAG_MONT = 0x0008,	    // Mounted on NT4
            FLAG_DUSN = 0x0010,	    // Delete USN underway
            FLAG_ROI = 0x0020,	    // Repair object Ids
            FLAG_MBC = 0x8000	    // Modified by chkdsk
        }

        #endregion Enums

        #region Properties

        public readonly Version Version;
        public readonly string Flags;

        #endregion Properties

        #region Constructors

        internal VolumeInformation(ResidentHeader header, byte[] attrBytes, string attrName)
        {
            Name = Enum.GetName(typeof(ATTR_TYPE), header.commonHeader.ATTRType);
            NameString = attrName;
            NonResident = header.commonHeader.NonResident;
            AttributeId = header.commonHeader.Id;

            uint MajorVersion = attrBytes[0x08];
            uint MinorVersion = attrBytes[0x09];
            Version = new Version((int)MajorVersion, (int)MinorVersion);

            Flags = ((ATTR_VOLINFO)BitConverter.ToInt16(attrBytes, 0x0A)).ToString();
        }

        #endregion Constructors
    }
}
