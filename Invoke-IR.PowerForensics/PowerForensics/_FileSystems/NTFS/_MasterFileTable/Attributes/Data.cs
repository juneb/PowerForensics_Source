using System;

namespace InvokeIR.PowerForensics.NTFS
{
    #region DataClass

    public class Data : Attr
    {
        #region Properties

        public readonly byte[] RawData;

        #endregion Properties

        #region Constructors

        internal Data(byte[] attrBytes, string attrName)
        {
            byte[] headerBytes = new byte[24];
            Array.Copy(attrBytes, 0, headerBytes, 0, headerBytes.Length);
            AttrHeader.ATTR_HEADER_RESIDENT header = new AttrHeader.ATTR_HEADER_RESIDENT(headerBytes);
            Name = Enum.GetName(typeof(ATTR_TYPE), header.commonHeader.ATTRType);
            NameString = attrName;
            NonResident = header.commonHeader.NonResident;
            AttributeId = header.commonHeader.Id;
            RawData = new byte[(attrBytes.Length)];
            Array.Copy(attrBytes, 0, RawData, 0, RawData.Length);
        }

        #endregion Constructors
    }

    #endregion DataClass
}
