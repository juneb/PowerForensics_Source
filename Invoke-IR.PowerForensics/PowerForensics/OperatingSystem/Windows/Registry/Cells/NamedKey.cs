using System;
using System.Text;
using System.Collections.Generic;
using InvokeIR.PowerForensics.NTFS;

namespace InvokeIR.PowerForensics.OS.Windows.Registry
{
    public class NamedKey : Cell
    {
        #region Enums

        enum NAMED_KEY_FLAGS
        {
            VolatileKey = 0x0001,
            MountPoint = 0x0002,
            RootKey = 0x0004,
            Immutable = 0x0008,
            SymbolicLink = 0x0010,
            NameIsASCII = 0x0020,
            PredefinedHandle = 0x0040
        }

        #endregion Enums

        #region Properties

        private readonly uint CellSize;
        public readonly string Signature;
        public readonly string Flags;
        public readonly DateTime WriteTime;
        public readonly uint ParentKeyOffset;
        public readonly uint NumberOfSubKeys;
        public readonly uint NumberOfVolatileSubKeys;
        public readonly uint SubKeysListOffset;
        public readonly uint VolatileSubKeysListOffset;
        public readonly uint NumberOfValues;
        public readonly uint ValuesListOffset;
        public readonly uint SecurityKeyOffset;
        public readonly uint ClassNameOffset;
        public readonly uint LargestSubKeyNameSize;
        public readonly uint LargestSubKeyClassNameSize;
        public readonly uint LargestValueNameSize;
        public readonly uint LargestValueDataSize;
        public readonly ushort KeyNameSize;
        public readonly ushort ClassNameSize;
        public readonly string KeyNameString;

        #endregion Properties

        #region Constructors

        internal NamedKey(byte[] bytes, string signature)
        {
            #region Signature

            Signature = signature;
            if (Signature != "nk")
            {
                throw new Exception("Cell is not a valid Named Key");
            }

            #endregion Signature
          
            Flags = ((NAMED_KEY_FLAGS)BitConverter.ToUInt16(bytes, 0x02)).ToString();
            WriteTime = DateTime.FromFileTimeUtc(BitConverter.ToInt64(bytes, 0x04));
            ParentKeyOffset = BitConverter.ToUInt32(bytes, 0x10);
            NumberOfSubKeys = BitConverter.ToUInt32(bytes, 0x14);
            NumberOfVolatileSubKeys = BitConverter.ToUInt32(bytes, 0x18);
            SubKeysListOffset = BitConverter.ToUInt32(bytes, 0x1C);
            VolatileSubKeysListOffset = BitConverter.ToUInt32(bytes, 0x20);
            NumberOfValues = BitConverter.ToUInt32(bytes, 0x24);
            ValuesListOffset = BitConverter.ToUInt32(bytes, 0x28);
            SecurityKeyOffset = BitConverter.ToUInt32(bytes, 0x2C);
            ClassNameOffset = BitConverter.ToUInt32(bytes, 0x30);
            LargestSubKeyNameSize = BitConverter.ToUInt32(bytes, 0x34);
            LargestSubKeyClassNameSize = BitConverter.ToUInt32(bytes, 0x38);
            LargestValueNameSize = BitConverter.ToUInt32(bytes, 0x3C);
            LargestValueDataSize = BitConverter.ToUInt32(bytes, 0x40);
            KeyNameSize = BitConverter.ToUInt16(bytes, 0x48);
            ClassNameSize = BitConverter.ToUInt16(bytes, 0x4A);

            #region KeyNameString

            // Needs work!

            #endregion KeyNameString
        }

        #endregion Constructors

        public static NamedKey GetInstances(string path)
        {
            // Get bytes for the specific record
            string volume = "\\\\.\\" + path.Split('\\')[0];
            IndexEntry entry = IndexEntry.Get(path);
            FileRecord record = new FileRecord(FileRecord.GetRecordBytes(volume, (int)entry.RecordNumber), volume, true);
            byte[] bytes = record.GetBytes(volume);

            #region RegistryHeader

            byte[] headerBytes = new byte[0x200];
            Array.Copy(bytes, 0, headerBytes, 0, headerBytes.Length);
            RegistryHeader header = new RegistryHeader(headerBytes);

            #endregion RegistryHeader

            // Hive Bin Headers
            byte[] hbinHeaderBytes = new byte[0x20];
            uint i = 0x1000;
            List<Cell> cellList = new List<Cell>();

            while (i < header.HiveBinsDataSize + 0x1000)
            {
                Array.Copy(bytes, i, hbinHeaderBytes, 0, hbinHeaderBytes.Length);
                HiveBinHeader hbinHeader = new HiveBinHeader(hbinHeaderBytes);
                int j = 0x20;
                /*while(j < hbinHeader.hBinSize)
                {
                    int size = BitConverter.ToInt32(bytes, j);
                    byte[] cellBytes = new byte[size];
                    Array.Copy(bytes, j, cellBytes, 0, cellBytes.Length);
                    Cell cell = CellFactory.Get(cellBytes);
                    cellList.Add(cell);
                    j += size;
                }
                i += hbinHeader.hBinSize;*/
                int size = Math.Abs(BitConverter.ToInt32(bytes, j));
                Console.WriteLine(size);
                byte[] cellBytes = new byte[size];
                Array.Copy(bytes, j + 0x04, cellBytes, 0, cellBytes.Length);
                return new NamedKey(cellBytes, "nk");
            }
            //return cellList.ToArray();
            return null;
        }
    }
}
