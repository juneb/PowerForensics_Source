using System;
using System.Text;
using PowerForensics.Ntfs;
using InvokeIR.Win32;

namespace PowerForensics.Registry
{
    #region HiveBinHeaderClass

    public class HiveBinHeader
    {
        #region Properties

        public readonly string Signature;
        public readonly uint hBinOffset;
        public readonly uint hBinSize;

        #endregion Properties

        #region Constructors

        internal HiveBinHeader(byte[] bytes)
        {
            Signature = Encoding.ASCII.GetString(bytes, 0x00, 0x04);

            if (Signature == "hbin")
            {
                hBinOffset = BitConverter.ToUInt32(bytes, 0x04);
                hBinSize = BitConverter.ToUInt32(bytes, 0x08);
            }
            else
            {
                throw new Exception("Invalid HiveBinHeader found.");
            }
        }

        #endregion Constructors

        #region StaticMethods
        
        public static HiveBinHeader[] GetInstances(string path)
        {
            FileRecord record = FileRecord.Get(path, true);
            byte[] bytes = record.GetBytes();

            // Registry Header
            RegistryHeader header = new RegistryHeader(NativeMethods.GetSubArray(bytes, 0x00, 0x200));

            // Hive Bin Headers
            HiveBinHeader[] headerArray = new HiveBinHeader[header.HiveBinsDataSize / 0x1000];
            byte[] hbinHeaderBytes = new byte[0x20];
            uint i = 0x1000;
            while (i < header.HiveBinsDataSize + 0x1000)
            {
                HiveBinHeader hbinHeader = new HiveBinHeader(NativeMethods.GetSubArray(bytes, i, 0x20));
                headerArray[((i / 0x1000) - 1)] = hbinHeader;
                i += hbinHeader.hBinSize;
            }

            return headerArray;
        }

        #endregion StaticMethods
    }

    #endregion HiveBinHeaderClass
}
