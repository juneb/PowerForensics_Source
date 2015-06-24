using System;
using System.Text;


namespace InvokeIR.PowerForensics.NTFS
{
    public class VolumeName : Attr
    {
        #region Properties

        public readonly string VolumeNameString;

        #endregion Properties

        #region Constructors

        internal VolumeName(ResidentHeader header, byte[] attrBytes, string attrName)
        {
            Name = Enum.GetName(typeof(ATTR_TYPE), header.commonHeader.ATTRType);
            NameString = attrName;
            NonResident = header.commonHeader.NonResident;
            AttributeId = header.commonHeader.Id;

            byte[] VolumeNameStringBytes = new byte[header.AttrSize];
            Array.Copy(attrBytes, 0, VolumeNameStringBytes, 0, VolumeNameStringBytes.Length);
            VolumeNameString = Encoding.Unicode.GetString(VolumeNameStringBytes);
        }

        #endregion Constructors
    }
}
