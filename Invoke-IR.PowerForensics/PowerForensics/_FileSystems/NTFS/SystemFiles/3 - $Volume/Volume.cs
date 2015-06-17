using System;
using System.IO;
using System.Text;
using InvokeIR.Win32;

namespace InvokeIR.PowerForensics.NTFS
{
    public class Volume
    {
        internal static FileRecord GetFileRecord(string volume)
        {
            return new FileRecord(FileRecord.GetBytes(volume, 3));
        }

        internal static VolumeName GetVolumeNameAttr(FileRecord fileRecord)
        {
            foreach (Attr attr in fileRecord.Attribute)
            {
                if (attr.Name == "VOLUME_NAME")
                {
                    return attr as VolumeName;
                }
            }
            throw new Exception("No VOLUME_NAME attribute found.");
        }

        internal static VolumeInformation GetVolumeInformationAttr(FileRecord fileRecord)
        {
            foreach (Attr attr in fileRecord.Attribute)
            {
                if (attr.Name == "VOLUME_INFORMATION")
                {
                    return attr as VolumeInformation;
                }
            }
            throw new Exception("No VOLUME_INFORMATION attribute found.");
        }
    }

    public class VolumeName : Attr
    {
        #region Properties

        public readonly string VolumeNameString;

        #endregion Properties

        #region Constructors

        internal VolumeName(byte[] attrBytes, string attrName)
        {
            byte[] attrHeader = new byte[24];
            Array.Copy(attrBytes, 0, attrHeader, 0, attrHeader.Length);
            AttrHeader.ATTR_HEADER_RESIDENT header = new AttrHeader.ATTR_HEADER_RESIDENT(attrHeader);

            Name = Enum.GetName(typeof(ATTR_TYPE), header.commonHeader.ATTRType);
            NameString = attrName;
            NonResident = header.commonHeader.NonResident;
            AttributeId = header.commonHeader.Id;

            byte[] VolumeNameStringBytes = new byte[header.AttrSize];
            Array.Copy(attrBytes, 24, VolumeNameStringBytes, 0, VolumeNameStringBytes.Length);
            VolumeNameString = Encoding.Unicode.GetString(VolumeNameStringBytes);
        }

        #endregion Constructors
    }

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
            byte[] attrHeader = new byte[24];
            Array.Copy(attrBytes, 0, attrHeader, 0, attrHeader.Length);
            AttrHeader.ATTR_HEADER_RESIDENT header = new AttrHeader.ATTR_HEADER_RESIDENT(attrHeader);

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

}
