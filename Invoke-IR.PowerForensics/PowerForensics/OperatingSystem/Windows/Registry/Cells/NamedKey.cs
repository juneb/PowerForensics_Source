using System;
using System.Text;

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
    }
}
