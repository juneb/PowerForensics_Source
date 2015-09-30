using System;
using System.Text;
using InvokeIR.Win32;

namespace PowerForensics.Ntfs
{
    #region VolumeNameClass

    public class VolumeName : Attr
    {
        #region Properties

        public readonly string VolumeNameString;

        #endregion Properties

        #region Constructors

        internal VolumeName(ResidentHeader header, byte[] bytes, string attrName)
        {
            Name = (ATTR_TYPE)header.commonHeader.ATTRType;
            NameString = attrName;
            NonResident = header.commonHeader.NonResident;
            AttributeId = header.commonHeader.Id;

            VolumeNameString = Encoding.Unicode.GetString(bytes, 0x00, (int)header.AttrSize);
        }

        internal VolumeName(ResidentHeader header, byte[] bytes, int offset, string attrName)
        {
            Name = (ATTR_TYPE)header.commonHeader.ATTRType;
            NameString = attrName;
            NonResident = header.commonHeader.NonResident;
            AttributeId = header.commonHeader.Id;

            VolumeNameString = Encoding.Unicode.GetString(bytes, 0x00 + offset, (int)header.AttrSize);
        }

        #endregion Constructors
    }
    
    #endregion VolumeNameClass
}
