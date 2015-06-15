using System;
using System.Linq;
using System.Text;

namespace InvokeIR.PowerForensics.NTFS
{

    #region VolumeNameClass

    public class VolumeName : Attr
    {
        #region Properties

        public readonly string VolumeNameString;

        #endregion Properties

        #region Constructors

        internal VolumeName(byte[] attrBytes, string attrName)
        {
            AttrHeader.ATTR_HEADER_RESIDENT header = new AttrHeader.ATTR_HEADER_RESIDENT(attrBytes.Take(24).ToArray());
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

    #endregion VolumeNameClass

}
