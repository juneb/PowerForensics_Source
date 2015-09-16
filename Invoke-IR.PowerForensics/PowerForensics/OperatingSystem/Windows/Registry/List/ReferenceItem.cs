using System;
using System.Text;

namespace InvokeIR.PowerForensics.Registry
{
    public class ReferenceItem : List
    {
        #region Constructors

        internal ReferenceItem(byte[] bytes)
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
            if (Signature != "ri")
            {
                Console.WriteLine(Signature);
                throw new Exception("List is not a valid Reference Item");
            }

            #endregion Signature

            Count = BitConverter.ToUInt16(bytes, 0x06);

            #endregion ListHeader

            uint[] offsetArray = new uint[Count];

            for (int i = 0; i < Count; i++)
            {
                offsetArray[i] = (BitConverter.ToUInt32(bytes, (i * 0x04) + 0x08) + RegistryHeader.HBINOFFSET);
            }

            Offset = offsetArray;
        }

        internal ReferenceItem(ushort count, uint[] offset)
        {
            Count = count;
            Offset = offset;
        }

        #endregion Constructors
    }
}
