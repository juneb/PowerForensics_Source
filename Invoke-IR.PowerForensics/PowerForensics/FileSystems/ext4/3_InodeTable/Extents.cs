using System;
using System.IO;
using System.Text;
using InvokeIR.Win32;

namespace InvokeIR.PowerForensics.ext3
{
    internal class ExtentHeader
    {
        #region Properties

        public readonly ushort Signature;
        public readonly ushort NumberOfExtents;
        public readonly ushort MaxExtents;
        public readonly ushort DepthOfTree;
        public readonly uint GenerationId;

        #endregion Properties

        #region Constructors
        internal ExtentHeader(byte[] bytes)
        {
            Signature = BitConverter.ToUInt16(bytes, 0x00);
            if (Signature == 62218)
            {
                NumberOfExtents = BitConverter.ToUInt16(bytes, 0x02);
                MaxExtents = BitConverter.ToUInt16(bytes, 0x04);
                DepthOfTree = BitConverter.ToUInt16(bytes, 0x06);
                GenerationId = BitConverter.ToUInt32(bytes, 0x08);
            }
            else
            {
                throw new Exception("Invalid ExtentHeader");
            }
        }
        #endregion Constructors
    }

    public class Extent
    {
        #region Properties

        private readonly ExtentHeader header;
        public readonly uint LogicalBlockOffset;
        public readonly ushort BlockCount;
        public readonly ulong StartBlock;

        #endregion Properties

        #region Constructors

        internal Extent(byte[] bytes)
        {
            byte[] headerBytes = new byte[0x0C];
            Array.Copy(bytes, 0x00, headerBytes, 0x00, headerBytes.Length);
            header = new ExtentHeader(headerBytes);
            LogicalBlockOffset = BitConverter.ToUInt32(bytes, 0x0C);
            BlockCount = BitConverter.ToUInt16(bytes, 0x10);
            StartBlock = BitConverter.ToUInt32(bytes, 0x14);
        }

        #endregion Constructors
    }
}
