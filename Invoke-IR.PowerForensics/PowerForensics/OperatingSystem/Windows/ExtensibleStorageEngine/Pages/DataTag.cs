using System;
using System.Collections.Generic;
using InvokeIR.Win32;

namespace PowerForensics.EseDb
{
    #region DataTagClass
    
    public class DataTag
    {
        #region Enums
        
        [FlagsAttribute]
        public enum JET_TAGFLAGS : uint
        {
            v = 0x0001,
            d = 0x0002,
            c = 0x0004,
        }

        #endregion Enums

        #region Properties

        public readonly int Tag; 
        public readonly ushort Offset;
        public readonly ushort Length;
        public readonly JET_TAGFLAGS Flags;
        public readonly byte[] Contents;

        #endregion Properties

        #region Constructors

        internal DataTag(byte[] bytes, int tag, uint pageoffset, int offset)
        {
            Tag = tag;
            Offset = (ushort)((BitConverter.ToUInt16(bytes, offset + 0x02) & 0xFFF) + 0x28);
            Length = BitConverter.ToUInt16(bytes, offset);
            Flags = (JET_TAGFLAGS)((BitConverter.ToInt16(bytes, offset + 0x02) & 0xF000) >> 13);
            Contents = NativeMethods.GetSubArray(bytes, pageoffset + Offset, Length);
        }

        #endregion Constructors

        #region StaticMethods

        internal static DataTag[] GetInstances(byte[] bytes, int offset)
        {
            List<DataTag> dtList = new List<DataTag>();

            bool c = true;

            for (int i = 0; c; i++)
            {
                int o = (offset + 0x1000) - ((i + 1) * 0x04);
                DataTag dt = new DataTag(bytes, i, (uint)offset, o);
                if (dt.Length != 0)
                {
                    dtList.Add(dt);
                }
                else
                {
                    c = false;
                }
            }

            return dtList.ToArray();
        }

        #endregion StaticMethods

        #region InstanceMethods

        

        #endregion InstanceMethods
    }

    #endregion DataTagClass
}
