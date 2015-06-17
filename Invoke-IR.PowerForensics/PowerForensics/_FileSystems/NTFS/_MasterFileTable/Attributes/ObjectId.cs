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

        internal ObjectId(byte[] attrBytes, string attrName)
        {
            byte[] headerBytes = new byte[24];
            Array.Copy(attrBytes, 0, headerBytes, 0, headerBytes.Length);
            AttrHeader.ATTR_HEADER_RESIDENT header = new AttrHeader.ATTR_HEADER_RESIDENT(headerBytes);
            Name = Enum.GetName(typeof(ATTR_TYPE), header.commonHeader.ATTRType);
            NameString = attrName;
            NonResident = header.commonHeader.NonResident;
            AttributeId = header.commonHeader.Id;
            
            // Get ObjectId Guid
            byte[] objectIdBytes = new byte[16];
            Array.Copy(attrBytes, 24, objectIdBytes, 0, objectIdBytes.Length);
            ObjectIdGuid = new Guid(objectIdBytes);

            if (attrBytes.Length > 40)
            {
                // Get BirthVolumeId Guid
                byte[] BirthVolumeIdBytes = new byte[16];
                Array.Copy(attrBytes, 40, BirthVolumeIdBytes, 0, objectIdBytes.Length);
                BirthVolumeId = new Guid(BirthVolumeIdBytes);

                // Get BirthObjectId Guid
                byte[] BirthObjectIdBytes = new byte[16];
                Array.Copy(attrBytes, 56, BirthObjectIdBytes, 0, objectIdBytes.Length);
                BirthObjectId = new Guid(BirthObjectIdBytes);

                // Get BirthDomainId Guid
                byte[] BirthDomainIdBytes = new byte[16];
                Array.Copy(attrBytes, 72, BirthDomainIdBytes, 0, objectIdBytes.Length);
                BirthDomainId = new Guid(BirthDomainIdBytes);
            }
        }

        #endregion Constructors
    }

    #endregion ObjectIdClass
}
