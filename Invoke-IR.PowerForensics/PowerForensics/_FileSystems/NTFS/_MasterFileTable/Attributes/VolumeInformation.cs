using System;
using System.Linq;
using System.Text;

namespace InvokeIR.PowerForensics.NTFS
{

    #region VolumeInformationClass

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

        internal VolumeInformation(byte[] attrBytes, string attrName)
        {
            AttrHeader.ATTR_HEADER_RESIDENT header = new AttrHeader.ATTR_HEADER_RESIDENT(attrBytes.Take(24).ToArray());
            Name = Enum.GetName(typeof(ATTR_TYPE), header.commonHeader.ATTRType);
            NameString = attrName;
            NonResident = header.commonHeader.NonResident;
            AttributeId = header.commonHeader.Id;

            uint MajorVersion = attrBytes[32];
            uint MinorVersion = attrBytes[33];
            Version = new Version((int)MajorVersion, (int)MinorVersion);

            Flags = ((ATTR_VOLINFO)BitConverter.ToInt16(attrBytes, 34)).ToString();
        }

        #endregion Constructors
    }

    #endregion VolumeInformationClass

}
