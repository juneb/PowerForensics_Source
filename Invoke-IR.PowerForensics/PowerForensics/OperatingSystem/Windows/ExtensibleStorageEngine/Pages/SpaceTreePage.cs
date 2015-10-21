using System;
using InvokeIR.Win32;

namespace PowerForensics.EseDb
{
    #region SpaceTreePageClass
    
    /*public class SpaceTreePage : Page
    {
        #region Properties

        private readonly ushort PageKeySize;
        public readonly byte[] PageKey;
        public readonly uint PageCount;

        #endregion Properties

        #region Constructors

        internal SpaceTreePage(byte[] bytes, int offset)
        {
            Header = new PageHeader(bytes, offset);
            Tags = DataTag.GetInstances(bytes, offset);
            //PageKeySize = BitConverter.ToUInt16(bytes, offset + Tags[1].Offset);
            //PageKey = NativeMethods.GetSubArray(bytes, (uint)offset + (uint)Tags[1].Offset + 0x02, (uint)PageKeySize);
            //PageCount = BitConverter.ToUInt32(bytes, offset + Tags[1].Offset + 0x02 + PageKeySize);
        }

        #endregion Constructors

        #region StaticMethods

        #endregion StaticMethods
    }*/

    #endregion SpaceTreePageClass
}
