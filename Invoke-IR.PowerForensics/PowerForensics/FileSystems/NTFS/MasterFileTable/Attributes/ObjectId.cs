using System;
using InvokeIR.Win32;

namespace InvokeIR.PowerForensics.Ntfs
{
    #region ObjectIdClass

    public class ObjectId : Attr
    {
        #region Properties

        public readonly Guid ObjectIdGuid;
        public readonly Guid BirthVolumeId;
        public readonly Guid BirthObjectId;
        public readonly Guid BirthDomainId;

        #endregion Properties

        #region Constructors

        internal ObjectId(ResidentHeader header, byte[] attrBytes, string attrName)
        {
            Name = (ATTR_TYPE)header.commonHeader.ATTRType;
            NameString = attrName;
            NonResident = header.commonHeader.NonResident;
            AttributeId = header.commonHeader.Id;


            ObjectIdGuid = new Guid(NativeMethods.GetSubArray(attrBytes, 0x00, 0x10));
            
            if (!(attrBytes.Length < 0x20))
            {
                BirthVolumeId = new Guid(NativeMethods.GetSubArray(attrBytes, 0x10, 0x10));
                
                if (!(attrBytes.Length < 0x30))
                {
                    BirthObjectId = new Guid(NativeMethods.GetSubArray(attrBytes, 0x20, 0x10));

                    if(attrBytes.Length == 0x40)
                    {
                        BirthDomainId = new Guid(NativeMethods.GetSubArray(attrBytes, 0x30, 0x10));
                    }
                }
            }
        }

        #endregion Constructors
    }

    #endregion ObjectIdClass
}
