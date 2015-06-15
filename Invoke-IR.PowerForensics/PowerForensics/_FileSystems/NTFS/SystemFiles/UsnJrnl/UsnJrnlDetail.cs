using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InvokeIR.PowerForensics.NTFS
{
    #region UsnJrnlDetailClass
    
    public class UsnJrnlDetail
    {
        #region Properties

        public ulong MaxSize;
        public ulong AllocationDelta;
        public ulong UsnId;
        public ulong LowestUsn;

        #endregion Properties

        #region Constructors

        internal UsnJrnlDetail(byte[] maxBytes)
        {
            //Get offset to attribute data (skip headers)
            ushort attrOffset = BitConverter.ToUInt16(maxBytes, 0x14);
            MaxSize = BitConverter.ToUInt64(maxBytes, (attrOffset + 0x00));
            AllocationDelta = BitConverter.ToUInt64(maxBytes, (attrOffset + 0x08));
            UsnId = BitConverter.ToUInt64(maxBytes, (attrOffset + 0x10));
            LowestUsn = BitConverter.ToUInt64(maxBytes, (attrOffset + 0x18));
        }

        #endregion Constructors
    }

    #endregion UsnJrnlDetailClass
}
