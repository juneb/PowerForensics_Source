using System;

namespace InvokeIR.PowerForensics.NTFS
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
            Name = Enum.GetName(typeof(ATTR_TYPE), header.commonHeader.ATTRType);
            NameString = attrName;
            NonResident = header.commonHeader.NonResident;
            AttributeId = header.commonHeader.Id;
            
            // Get ObjectId Guid
            byte[] objectIdBytes = new byte[16];
            Array.Copy(attrBytes, 0x00, objectIdBytes, 0, objectIdBytes.Length);
            ObjectIdGuid = new Guid(objectIdBytes);

            if (!(attrBytes.Length < 0x20))
            {
                // Get BirthVolumeId Guid
                byte[] BirthVolumeIdBytes = new byte[16];
                Array.Copy(attrBytes, 0x10, BirthVolumeIdBytes, 0, objectIdBytes.Length);
                BirthVolumeId = new Guid(BirthVolumeIdBytes);

                if (!(attrBytes.Length < 0x30))
                {
                    // Get BirthObjectId Guid
                    byte[] BirthObjectIdBytes = new byte[16];
                    Array.Copy(attrBytes, 0x20, BirthObjectIdBytes, 0, objectIdBytes.Length);
                    BirthObjectId = new Guid(BirthObjectIdBytes);

                    if(attrBytes.Length == 0x40)
                    {
                        // Get BirthDomainId Guids
                        byte[] BirthDomainIdBytes = new byte[16];
                        Array.Copy(attrBytes, 0x30, BirthDomainIdBytes, 0, objectIdBytes.Length);
                        BirthDomainId = new Guid(BirthDomainIdBytes);
                    }
                }
            }
        }

        #endregion Constructors
    }

    #endregion ObjectIdClass
}
