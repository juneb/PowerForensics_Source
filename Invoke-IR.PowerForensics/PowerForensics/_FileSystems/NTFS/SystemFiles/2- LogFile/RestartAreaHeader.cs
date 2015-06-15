using System;
using System.Text;
using System.Runtime.InteropServices;

namespace InvokeIR.PowerForensics.NTFS
{
    public class RestartAreaHeader
    {
        #region Properties
        
        public readonly string Signature;
        public readonly ushort USOffset;
        public readonly ushort USCount;
        public readonly ulong CheckDiskLSN;
        public readonly uint SystemPageSize;
        public readonly uint LogPageSize;
        public readonly ushort RestartOffset;
        public readonly ushort MinorVersion;
        public readonly ushort MajorVersion;
        public readonly byte[] USArray;
        public readonly ulong CurrentLSN;
        public readonly uint LogClient;
        public readonly uint ClientList;
        public readonly ulong Flags;

        #endregion Properties

        #region Constructors

        internal RestartAreaHeader(byte[] bytes)
        {
            byte[] sigBytes = new byte[4];
            Array.Copy(bytes, 0, sigBytes, 0, sigBytes.Length);
            Signature = Encoding.ASCII.GetString(sigBytes);;
            USOffset = BitConverter.ToUInt16(bytes, 4);
            USCount = BitConverter.ToUInt16(bytes, 6);
            CheckDiskLSN = BitConverter.ToUInt64(bytes, 8);
            SystemPageSize = BitConverter.ToUInt32(bytes, 16);
            LogPageSize = BitConverter.ToUInt32(bytes, 20);
            RestartOffset = BitConverter.ToUInt16(bytes, 24);
            MinorVersion = BitConverter.ToUInt16(bytes, 26);
            MajorVersion = BitConverter.ToUInt16(bytes, 28);
            //USArray = bytes.Skip(30).Take(18).ToArray();
            CurrentLSN = BitConverter.ToUInt64(bytes, 48);
            LogClient = BitConverter.ToUInt32(bytes, 56);
            ClientList = BitConverter.ToUInt32(bytes, 60);
            Flags = BitConverter.ToUInt64(bytes, 64);
        }

        #endregion Constructors
    }
}
