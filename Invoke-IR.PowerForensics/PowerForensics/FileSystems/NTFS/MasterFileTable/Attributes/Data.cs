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

        internal Data(ResidentHeader header, byte[] attrBytes, string attrName)
        {
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
