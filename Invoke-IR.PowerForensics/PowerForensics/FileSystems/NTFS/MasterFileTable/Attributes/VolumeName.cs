using System;
using System.Text;
using InvokeIR.Win32;

namespace InvokeIR.PowerForensics.Ntfs
{
    #region VolumeNameClass

    public class VolumeName : Attr
    {
        #region Properties

        public readonly string VolumeNameString;

        #endregion Properties

        #region Constructors

        internal VolumeName(ResidentHeader header, byte[] attrBytes, string attrName)
        {
            Name = (ATTR_TYPE)header.commonHeader.ATTRType;
            NameString = attrName;
            NonResident = header.commonHeader.NonResident;
            AttributeId = header.commonHeader.Id;

            VolumeNameString = Encoding.Unicode.GetString(attrBytes, 0x00, (int)header.AttrSize);
        }

        #endregion Constructors
    }
    
    #endregion VolumeNameClass
}
