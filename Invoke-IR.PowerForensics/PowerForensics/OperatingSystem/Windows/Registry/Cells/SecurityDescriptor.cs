using System;

namespace InvokeIR.PowerForensics.Registry
{
    public class SecurityDescriptor
    {
        #region Enums

        [FlagsAttribute]
        enum SECURITY_KEY_CONTROLS
        {
            SeOwnerDefaulted = 0x0001,
            SeGroupDefaulted = 0x0002,
            SeDaclPresent = 0x0004,
            SeDaclDefaulted = 0x0008,
            SeSaclPresent = 0x0010,
            SeSaclDefaulted = 0x0020,
            SeDaclAutoInheritReq = 0x0100,
            SeSaclAutoInheritReq = 0x0200,
            SeDaclAutoInherited = 0x0400,
            SeSaclAutoInherited = 0x0800,
            SeDaclProtected = 0x1000,
            SeSaclProtected = 0x2000,
            SeRmControlValid = 0x4000,
            SeSelfRelative = 0x8000,
        }

        #endregion Enums
        
        #region Properties

        public readonly string Control;
        internal readonly uint OwnerOffset;
        internal readonly uint GroupOffset;
        internal readonly uint SACLOffset;
        internal readonly uint DACLOffset;
        public readonly byte[] Owner;
        public readonly byte[] Group;
        public readonly byte[] SACL;
        public readonly byte[] DACL;

        #endregion Properties

        #region Constructors

        internal SecurityDescriptor(byte[] bytes)
        {
            Control = ((SECURITY_KEY_CONTROLS)BitConverter.ToUInt16(bytes, 0x02)).ToString();
            OwnerOffset = BitConverter.ToUInt32(bytes, 0x04);
            GroupOffset = BitConverter.ToUInt32(bytes, 0x08);
            SACLOffset = BitConverter.ToUInt32(bytes, 0x0C);
            DACLOffset = BitConverter.ToUInt32(bytes, 0x10);
            
            byte[] ownerBytes = new byte[0x10];
            Array.Copy(bytes, OwnerOffset, ownerBytes, 0, ownerBytes.Length);
            Owner = ownerBytes;

            byte[] groupBytes = new byte[0x0C];
            Array.Copy(bytes, GroupOffset, groupBytes, 0, groupBytes.Length);
            Group = groupBytes;

            byte[] saclBytes = new byte[0x08];
            Array.Copy(bytes, SACLOffset, saclBytes, 0, saclBytes.Length);
            SACL = saclBytes;

            byte[] daclBytes = new byte[0x84];
            Array.Copy(bytes, DACLOffset, daclBytes, 0, daclBytes.Length);
            DACL = daclBytes;
        }

        #endregion Constructors
    }
}
