using System;
using System.Text;
using System.Runtime.InteropServices;

namespace InvokeIR.PowerForensics.NTFS
{
    class PageHeader
    {
        #region Properties

        public readonly string Signature;
        public readonly ushort USOffset;
        public readonly ushort USCount;
        public readonly ulong LastLSN;
        public readonly uint Flags;
        public readonly ushort PageCount;
        public readonly ushort PagePosition;
        public readonly ushort NextRecordOffset;
        public readonly ulong LastEndLSN;
        public readonly ushort USN;
        public readonly byte[] USArray;

        #endregion Properties

        #region Constructors

        internal PageHeader(byte[] bytes)
        {
            byte[] sigBytes = new byte[4];
            Array.Copy(bytes, 0, sigBytes, 0, sigBytes.Length);
            Signature = Encoding.ASCII.GetString(sigBytes);
            USOffset = BitConverter.ToUInt16(bytes, 4);
            USCount = BitConverter.ToUInt16(bytes, 6);
            LastLSN = BitConverter.ToUInt64(bytes, 8);
            Flags = BitConverter.ToUInt32(bytes, 16);
            PageCount = BitConverter.ToUInt16(bytes, 20);
            PagePosition = BitConverter.ToUInt16(bytes, 22);
            NextRecordOffset = BitConverter.ToUInt16(bytes, 24);
            LastLSN = BitConverter.ToUInt64(bytes, 32);
            //USN = ;
            //USArray = ;
        }

        #endregion Constructors
    }
}
