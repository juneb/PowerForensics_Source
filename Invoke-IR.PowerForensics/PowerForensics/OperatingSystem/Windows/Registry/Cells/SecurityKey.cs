using System;
using System.Text;

namespace InvokeIR.PowerForensics.Registry
{
    public class SecurityKey : Cell
    {
        #region Properties

        internal readonly uint Flink;
        internal readonly uint Blink;
        public readonly uint ReferenceCount;
        internal readonly uint DescriptorLength;
        public readonly SecurityDescriptor Descriptor;

        #endregion Properties

        #region Constructors

        internal SecurityKey(byte[] bytes)
        {
            #region CellHeader

            Size = BitConverter.ToInt32(bytes, 0x00);

            if (Size >= 0)
            {
                Allocated = false;
            }
            else
            {
                Allocated = true;
            }

            #region Signature

            byte[] sigBytes = new byte[0x02];
            Array.Copy(bytes, 0x04, sigBytes, 0, sigBytes.Length);
            Signature = Encoding.ASCII.GetString(sigBytes);
            if (Signature != "sk")
            {
                throw new Exception("Cell is not a valid Security Key");
            }

            #endregion Signature

            #endregion CellHeader

            Flink = BitConverter.ToUInt32(bytes, 0x08);
            Blink = BitConverter.ToUInt32(bytes, 0x0C);
            ReferenceCount = BitConverter.ToUInt32(bytes, 0x10);
            DescriptorLength = BitConverter.ToUInt32(bytes, 0x14);

            #region Descriptor

            byte[] descriptorBytes = new byte[DescriptorLength];
            Array.Copy(bytes, 0x18, descriptorBytes, 0, descriptorBytes.Length);

            Descriptor = new SecurityDescriptor(descriptorBytes);

            #endregion Descriptor
        }

        #endregion Constructors
    }
}
