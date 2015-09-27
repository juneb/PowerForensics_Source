using System;
using InvokeIR.Win32;

namespace InvokeIR.PowerForensics.Ntfs
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
            Name = (ATTR_TYPE)header.commonHeader.ATTRType;
            NameString = attrName;
            NonResident = header.commonHeader.NonResident;
            AttributeId = header.commonHeader.Id;
            RawData = NativeMethods.GetSubArray(attrBytes, 0x00, (uint)attrBytes.Length);
        }

        #endregion Constructors
    }

    #endregion DataClass
}
