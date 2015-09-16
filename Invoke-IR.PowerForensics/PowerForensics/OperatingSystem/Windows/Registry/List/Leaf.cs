using System;
using System.Text;

namespace InvokeIR.PowerForensics.Registry
{
    public class Leaf : List
    {
        #region Properties

        public readonly string[] HashValue;

        #endregion Properties

        #region Constructors

        internal Leaf(byte[] bytes)
        {
            #region ListHeader

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
            if (Signature != "lf")
            {
                Console.WriteLine(Signature);
                throw new Exception("List is not a valid Leaf");
            }

            #endregion Signature

            Count = BitConverter.ToUInt16(bytes, 0x06);

            #endregion ListHeader

            uint[] offsetArray = new uint[Count];
            string[] hashArray = new string[Count];

            for (int i = 0; i < Count; i++)
            {
                offsetArray[i] = (BitConverter.ToUInt32(bytes, (i * 0x08) + 0x08) + RegistryHeader.HBINOFFSET);

                byte[] hashBytes = new byte[0x04];
                Array.Copy(bytes, (i * 0x08) + 0x0C, hashBytes, 0, hashBytes.Length);
                hashArray[i] = Encoding.ASCII.GetString(hashBytes);
            }

            Offset = offsetArray;
            HashValue = hashArray;
        }

        #endregion Constructors
    }
}
