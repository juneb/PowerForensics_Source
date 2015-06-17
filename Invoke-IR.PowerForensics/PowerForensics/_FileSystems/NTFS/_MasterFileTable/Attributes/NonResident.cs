using System;
using System.IO;
using System.Collections.Generic;
using InvokeIR.Win32;

namespace InvokeIR.PowerForensics.NTFS
{
    public class NonResident : Attr
    {
        #region Structs

        struct ATTR_HEADER_NON_RESIDENT
        {
            internal AttrHeader.ATTR_HEADER_COMMON commonHeader;	// Common data structure
            internal ulong StartVCN;		                        // Starting VCN
            internal ulong LastVCN;		                            // Last VCN
            internal ushort DataRunOffset;	                        // Offset to the Data Runs
            internal ushort CompUnitSize;	                        // Compression unit size
            internal uint Padding;		                            // Padding
            internal ulong AllocSize;		                        // Allocated size of the attribute
            internal ulong RealSize;		                        // Real size of the attribute
            internal ulong IniSize;		                            // Initialized data size of the stream 

            internal ATTR_HEADER_NON_RESIDENT(byte[] bytes)
            {
                byte[] headerBytes = new byte[16];
                Array.Copy(bytes, 0, headerBytes, 0, headerBytes.Length);
                commonHeader = new AttrHeader.ATTR_HEADER_COMMON(headerBytes);
                StartVCN = BitConverter.ToUInt64(bytes, 16);
                LastVCN = BitConverter.ToUInt64(bytes, 24);
                DataRunOffset = BitConverter.ToUInt16(bytes, 32);
                CompUnitSize = BitConverter.ToUInt16(bytes, 34);
                Padding = BitConverter.ToUInt32(bytes, 36);
                AllocSize = BitConverter.ToUInt64(bytes, 40);
                RealSize = BitConverter.ToUInt64(bytes, 48);
                IniSize = BitConverter.ToUInt64(bytes, 56);
            }
        }

        #endregion Structs

        #region Properties

        public readonly ulong AllocatedSize;
        public readonly ulong RealSize;
        public readonly ulong InitializedSize;
        public readonly DataRun[] DataRun;

        #endregion Properties

        #region Constructors

        internal NonResident(uint AttrType, string name, bool nonResident, ushort attributeId, ulong allocatedSize, ulong realSize, ulong iniSize, DataRun[] dataRunList)
        {
            Name = Enum.GetName(typeof(ATTR_TYPE), AttrType);
            NameString = name;
            NonResident = nonResident;
            AttributeId = attributeId;
            AllocatedSize = allocatedSize;
            RealSize = realSize;
            InitializedSize = iniSize;
            DataRun = dataRunList;
        }

        #endregion Constructors

        internal static byte[] GetContent(FileStream streamToRead, NonResident nonResAttr)
        {
            List<byte> DataBytes = new List<byte>();

            for (int i = 0; i < nonResAttr.DataRun.Length; i++)
            {
                ulong offset = (ulong)nonResAttr.DataRun[i].StartCluster * 4096;
                ulong length = (ulong)nonResAttr.DataRun[i].ClusterLength * 4096;
                DataBytes.AddRange(NativeMethods.readDrive(streamToRead, offset, length));
            }

            byte[] contentBytes = new byte[nonResAttr.RealSize];
            Array.Copy(DataBytes.ToArray(), 0, contentBytes, 0, contentBytes.Length);

            return contentBytes;
        }

        public static byte[] GetContent(string volume, NonResident nonResAttr)
        {

            List<byte> DataBytes = new List<byte>();

            IntPtr hVolume = NativeMethods.getHandle(volume);
            FileStream streamToRead = NativeMethods.getFileStream(hVolume);

            for (int i = 0; i < nonResAttr.DataRun.Length; i++)
            {
                ulong offset = (ulong)nonResAttr.DataRun[i].StartCluster * 4096;
                ulong length = (ulong)nonResAttr.DataRun[i].ClusterLength * 4096;
                DataBytes.AddRange(NativeMethods.readDrive(streamToRead, offset, length));
            }

            byte[] dataBytes = new byte[nonResAttr.RealSize];
            Array.Copy(DataBytes.ToArray(), 0, dataBytes, 0, dataBytes.Length);
            return dataBytes;

        }

        internal static NonResident Get(byte[] AttrBytes, string AttrName)
        {
            ATTR_HEADER_NON_RESIDENT nonResAttrHeader = new ATTR_HEADER_NON_RESIDENT(AttrBytes);

            int DataRunStart = nonResAttrHeader.DataRunOffset;
            int DataRunSize = (int)nonResAttrHeader.commonHeader.TotalSize - nonResAttrHeader.DataRunOffset;
            byte[] DataRunBytes = new byte[DataRunSize];
            Array.Copy(AttrBytes, DataRunStart, DataRunBytes, 0, DataRunBytes.Length);

            int offset = 0;
            int DataRunLengthByteCount = DataRunBytes[0] & 0x0F;
            int DataRunOffsetByteCount = ((DataRunBytes[0] & 0xF0) >> 4);

            long startCluster = 0;
            List<DataRun> dataRunList = new List<DataRun>();

            do
            {
                // Get bytes for DataRun
                byte[] runBytes = new byte[(DataRunLengthByteCount + DataRunOffsetByteCount)];
                Array.Copy(DataRunBytes, (offset + 1), runBytes, 0, runBytes.Length);

                // Instantiate a DataRun object
                DataRun dataRun = new DataRun(runBytes, DataRunLengthByteCount, DataRunOffsetByteCount, ref startCluster);

                dataRunList.Add(dataRun);
                
                offset = offset + 1 + DataRunLengthByteCount + DataRunOffsetByteCount;

                DataRunLengthByteCount = DataRunBytes[offset] & 0x0F;
                DataRunOffsetByteCount = ((DataRunBytes[offset] & 0xF0) >> 4);
            }
            while (((offset + DataRunLengthByteCount + DataRunOffsetByteCount + 1) < DataRunSize) && (DataRunOffsetByteCount <= 3) && (DataRunLengthByteCount != 0));

            return new NonResident(
                nonResAttrHeader.commonHeader.ATTRType,
                AttrName,
                nonResAttrHeader.commonHeader.NonResident,
                nonResAttrHeader.commonHeader.Id,
                nonResAttrHeader.AllocSize,
                nonResAttrHeader.RealSize,
                nonResAttrHeader.IniSize,
                dataRunList.ToArray()
            );
        }
    }

    public class DataRun
    {
        #region Properties

        public readonly long StartCluster;
        public readonly long ClusterLength;
        public readonly bool Sparse;
        public readonly bool Compressed;

        #endregion Properties

        #region Constructors

        public DataRun(byte[] bytes, int lengthByteCount, int offsetByteCount, ref long previousStartCluster)
        {
            if (offsetByteCount == 0)
            {
                Sparse = true;
            }

            byte[] DataRunLengthBytes = new byte[8];
            Array.Copy(bytes, 0, DataRunLengthBytes, 0, lengthByteCount);
            long DataRunLength = BitConverter.ToInt64(DataRunLengthBytes, 0);

            byte[] DataRunOffsetBytes = new byte[8];
            Array.Copy(bytes, lengthByteCount, DataRunOffsetBytes, 0, offsetByteCount);
            long DataRunOffset = BitConverter.ToInt64(DataRunOffsetBytes, 0);

            if ((DataRunOffset & (1 << ((offsetByteCount * 8) - 1))) != 0)
            {
                DataRunOffset = (DataRunOffset | (~0) << (offsetByteCount * 8));
            }

            previousStartCluster += DataRunOffset;
            StartCluster = previousStartCluster;
            ClusterLength = DataRunLength;
        }

        #endregion Constructors
    }

}
