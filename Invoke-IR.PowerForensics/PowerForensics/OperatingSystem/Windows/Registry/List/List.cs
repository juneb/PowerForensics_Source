using System;
using System.Text;

namespace InvokeIR.PowerForensics.Registry
{
    public class List
    {
        public int Size;
        public string Signature;
        public bool Allocated;
        public ushort Count;
        public uint[] Offset;

        internal static List Factory(byte[] bytes, byte[] subKeyListBytes, string type)
        {
            List list = null;

            if (type == "lf")
            {
                return new Leaf(subKeyListBytes);
            }
            else if (type == "lh")
            {
                return new HashedLeaf(subKeyListBytes);
            }
            else if (type == "li")
            {
                return new LeafItem(subKeyListBytes);
            }
            else if (type == "ri")
            {
                List ri = new ReferenceItem(subKeyListBytes);

                List[] listArray = new List[ri.Count];

                for (int i = 0; i < ri.Offset.Length; i++)
                {
                    byte[] sublistBytes = new byte[Math.Abs(BitConverter.ToInt32(bytes, (int)ri.Offset[i]))];
                    
                    Array.Copy(bytes, ri.Offset[i], sublistBytes, 0, sublistBytes.Length);

                    byte[] sigBytes = new byte[0x02];
                    Array.Copy(sublistBytes, 0x04, sigBytes, 0, sigBytes.Length);
                    string subtype = Encoding.ASCII.GetString(sigBytes);

                    listArray[i] = List.Factory(bytes, sublistBytes, subtype);
                }

                ushort aggCount = 0;
                foreach (List l in listArray)
                {
                    aggCount += l.Count;
                }

                uint[] aggOffset = new uint[aggCount];
                int j = 0;
                foreach (List l in listArray)
                {
                    for (int k = 0; (k < l.Count) && (j < aggCount); k++)
                    {
                        aggOffset[j] = l.Offset[k];
                        j++;
                    }
                }

                return new ReferenceItem(aggCount, aggOffset);
            }
            else
            {
                return null;
            }
        }
    }
}
