using System;
using System.IO;
using System.Text;
using InvokeIR.Win32;

namespace InvokeIR.PowerForensics.Ext3
{
    #region ExtentHeaderClass

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

    #endregion ExtentHeaderClass

    #region ExtentClass

    public class Extent
    {
        #region Properties

        public readonly uint LogicalBlockOffset;
        public readonly ushort BlockCount;
        public readonly ulong StartBlock;

        #endregion Properties

        #region Constructors

        internal Extent(byte[] bytes)
        {
            LogicalBlockOffset = BitConverter.ToUInt32(bytes, 0x00);
            BlockCount = BitConverter.ToUInt16(bytes, 0x04);
            // Need to add upper bytes (0x06 & 0x07)
            StartBlock = BitConverter.ToUInt32(bytes, 0x08);
        }

        #endregion Constructors

        #region StaticMethods

        public static Extent[] GetInstances(byte[] bytes)
        {
            ExtentHeader header = new ExtentHeader(NativeMethods.GetSubArray(bytes, 0x00, 0x0C));

            Extent[] extentArray = new Extent[header.NumberOfExtents];

            uint offset = 0x0C;
            uint size = 0x0C;
            
            for (int i = 0; (i < header.NumberOfExtents) && (i < 4); i++)
            {
                extentArray[i] = new Extent(NativeMethods.GetSubArray(bytes, offset, size));
                
                offset += size;
            }
                
            return extentArray;
        }

        #endregion StaticMethods

        #region InstanceMethods

        public byte[] GetBytes(string volume)
        {
            IntPtr hDevice = NativeMethods.getHandle(volume);

            using (FileStream streamToRead = NativeMethods.getFileStream(hDevice))
            {
                Superblock sb = new Superblock(Superblock.GetBytes(streamToRead, 0));
                return Win32.NativeMethods.readDrive(streamToRead, (sb.BlockSize * this.StartBlock), (sb.BlockSize * this.BlockCount));
            }
        }

        #endregion InstanceMethods
    }
    
    #endregion ExtentClass
}
