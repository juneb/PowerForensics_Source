using System;
using System.Text;

namespace InvokeIR.PowerForensics.Registry
{
    public class NamedKey : Cell
    {
        #region Enums

        [FlagsAttribute]
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

        public readonly string HivePath;
        internal readonly string Flags;
        public readonly DateTime WriteTime;
        internal readonly uint ParentKeyOffset;
        public readonly uint NumberOfSubKeys;
        public readonly uint NumberOfVolatileSubKeys;
        internal readonly int SubKeysListOffset;
        internal readonly int VolatileSubKeysListOffset;
        public readonly uint NumberOfValues;
        internal readonly int ValuesListOffset;
        internal readonly int SecurityKeyOffset;
        internal readonly int ClassNameOffset;
        internal readonly uint LargestSubKeyNameSize;
        internal readonly uint LargestSubKeyClassNameSize;
        internal readonly uint LargestValueNameSize;
        internal readonly uint LargestValueDataSize;
        internal readonly ushort KeyNameSize;
        internal readonly ushort ClassNameSize;
        public readonly string Name;


        #endregion Properties

        #region Constructors

        internal NamedKey(byte[] bytes, string hivePath)
        {
            HivePath = hivePath;

            #region CellHeader

            Size = BitConverter.ToInt32(bytes, 0x00);

            if (Size >= 0)
            {
                Allocated = false;
            }
            else
            {
                Allocated = true;
            }

            #region Signature

            byte[] sigBytes = new byte[0x02];
            Array.Copy(bytes, 0x04, sigBytes, 0, sigBytes.Length);
            Signature = Encoding.ASCII.GetString(sigBytes);
            if (Signature != "nk")
            {
                throw new Exception("Cell is not a valid Named Key");
            }

            #endregion Signature

            #endregion CellHeader

            Flags = ((NAMED_KEY_FLAGS)BitConverter.ToUInt16(bytes, 0x06)).ToString();
            WriteTime = DateTime.FromFileTimeUtc(BitConverter.ToInt64(bytes, 0x08));
            ParentKeyOffset = BitConverter.ToUInt32(bytes, 0x14) + RegistryHeader.HBINOFFSET;
            NumberOfSubKeys = BitConverter.ToUInt32(bytes, 0x18);
            NumberOfVolatileSubKeys = BitConverter.ToUInt32(bytes, 0x1C);
            SubKeysListOffset = BitConverter.ToInt32(bytes, 0x20) + RegistryHeader.HBINOFFSET;
            VolatileSubKeysListOffset = BitConverter.ToInt32(bytes, 0x24) + RegistryHeader.HBINOFFSET;
            NumberOfValues = BitConverter.ToUInt32(bytes, 0x28);
            ValuesListOffset = BitConverter.ToInt32(bytes, 0x2C) + RegistryHeader.HBINOFFSET;
            SecurityKeyOffset = BitConverter.ToInt32(bytes, 0x30) + RegistryHeader.HBINOFFSET;
            ClassNameOffset = BitConverter.ToInt32(bytes, 0x34) + RegistryHeader.HBINOFFSET;
            LargestSubKeyNameSize = BitConverter.ToUInt32(bytes, 0x38);
            LargestSubKeyClassNameSize = BitConverter.ToUInt32(bytes, 0x3C);
            LargestValueNameSize = BitConverter.ToUInt32(bytes, 0x40);
            LargestValueDataSize = BitConverter.ToUInt32(bytes, 0x44);
            KeyNameSize = BitConverter.ToUInt16(bytes, 0x4C);
            ClassNameSize = BitConverter.ToUInt16(bytes, 0x4E);

            #region KeyNameString

            if ((0x50 + KeyNameSize) <= bytes.Length)
            {
                byte[] nameBytes = new byte[Math.Abs(KeyNameSize)];
                Array.Copy(bytes, 0x50, nameBytes, 0, nameBytes.Length);
                Name = Encoding.ASCII.GetString(nameBytes);
            }

            #endregion KeyNameString
        }

        #endregion Constructors

        #region StaticMethods

        public static NamedKey[] GetInstances(string path, string key)
        {
            byte[] bytes = Helper.GetHiveBytes(path);

            NamedKey hiveroot = Helper.GetRootKey(bytes, path);

            NamedKey nk = hiveroot;

            if (key != null)
            {
                foreach (string k in key.Split('\\'))
                {
                    foreach (NamedKey n in nk.GetSubKeys(bytes))
                    {
                        if (n.Name.ToUpper() == k.ToUpper())
                        {
                            nk = n;
                        }
                    }
                }
            }

            return nk.GetSubKeys(bytes);
        }

        internal static NamedKey[] GetInstances(byte[] bytes, string path, string key)
        {
            NamedKey hiveroot = Helper.GetRootKey(bytes, path);

            NamedKey nk = hiveroot;

            if (key != null)
            {
                foreach (string k in key.Split('\\'))
                {
                    foreach (NamedKey n in nk.GetSubKeys(bytes))
                    {
                        if (n.Name.ToUpper() == k.ToUpper())
                        {
                            nk = n;
                        }
                    }
                }
            }

            return nk.GetSubKeys(bytes);
        }

        #endregion StaticMethods

        #region ClassMethods

        public ValueKey[] GetValues()
        {
            if (this.NumberOfValues > 0)
            {
                byte[] bytes = Helper.GetHiveBytes(this.HivePath);

                byte[] valueListBytes = new byte[Math.Abs(BitConverter.ToInt32(bytes, this.ValuesListOffset))];
                Array.Copy(bytes, this.ValuesListOffset, valueListBytes, 0, valueListBytes.Length);

                ValuesList list = new ValuesList(valueListBytes, this.NumberOfValues);

                ValueKey[] vkArray = new ValueKey[list.Offset.Length];

                for (int i = 0; i < list.Offset.Length; i++)
                {
                    int size = Math.Abs(BitConverter.ToInt32(bytes, (int)list.Offset[i]));

                    byte[] valueBytes = new byte[size];
                    Array.Copy(bytes, list.Offset[i], valueBytes, 0, valueBytes.Length);
                    vkArray[i] = new ValueKey(valueBytes, this.HivePath);
                }

                return vkArray;
            }

            return null;
        }

        internal ValueKey[] GetValues(byte[] bytes)
        {
            if (this.NumberOfValues > 0)
            {
                byte[] valueListBytes = new byte[Math.Abs(BitConverter.ToInt32(bytes, this.ValuesListOffset))];
                Array.Copy(bytes, this.ValuesListOffset, valueListBytes, 0, valueListBytes.Length);

                ValuesList list = new ValuesList(valueListBytes, this.NumberOfValues);

                ValueKey[] vkArray = new ValueKey[list.Offset.Length];

                for (int i = 0; i < list.Offset.Length; i++)
                {
                    int size = Math.Abs(BitConverter.ToInt32(bytes, (int)list.Offset[i]));

                    byte[] valueBytes = new byte[size];
                    Array.Copy(bytes, list.Offset[i], valueBytes, 0, valueBytes.Length);
                    vkArray[i] = new ValueKey(valueBytes, this.HivePath);
                }

                return vkArray;
            }

            return null;
        }

        public NamedKey[] GetSubKeys()
        {
            if (this.NumberOfSubKeys > 0)
            {
                byte[] bytes = Helper.GetHiveBytes(this.HivePath);

                byte[] subKeyListBytes = new byte[Math.Abs(BitConverter.ToInt32(bytes, this.SubKeysListOffset))];
                Array.Copy(bytes, this.SubKeysListOffset, subKeyListBytes, 0, subKeyListBytes.Length);

                byte[] sigBytes = new byte[0x02];
                Array.Copy(subKeyListBytes, 0x04, sigBytes, 0, sigBytes.Length);
                string type = Encoding.ASCII.GetString(sigBytes);

                List list = null;

                if (type == "lf")
                {
                    list = new Leaf(subKeyListBytes);
                }
                else if(type == "lh")
                {
                    list = new HashedLeaf(subKeyListBytes);
                }
                else
                {
                    return null;
                }

                NamedKey[] nkArray = new NamedKey[list.Count];

                for (int i = 0; i < list.Count; i++)
                {
                    int size = Math.Abs(BitConverter.ToInt32(bytes, (int)list.Offset[i]));

                    byte[] keyBytes = new byte[size];
                    Array.Copy(bytes, list.Offset[i], keyBytes, 0, keyBytes.Length);
                    nkArray[i] = new NamedKey(keyBytes, this.HivePath);
                }

                return nkArray;
            }
            else
            {
                return null;
            }
        }

        internal NamedKey[] GetSubKeys(byte[] bytes)
        {
            if (this.NumberOfSubKeys > 0)
            {
                byte[] subKeyListBytes = new byte[Math.Abs(BitConverter.ToInt32(bytes, this.SubKeysListOffset))];
                Array.Copy(bytes, this.SubKeysListOffset, subKeyListBytes, 0, subKeyListBytes.Length);

                byte[] sigBytes = new byte[0x02];
                Array.Copy(subKeyListBytes, 0x04, sigBytes, 0, sigBytes.Length);
                string type = Encoding.ASCII.GetString(sigBytes);

                List list = List.Factory(bytes, subKeyListBytes, type);

                NamedKey[] nkArray = new NamedKey[list.Count];

                for (int i = 0; i < list.Count; i++)
                {
                    int size = Math.Abs(BitConverter.ToInt32(bytes, (int)list.Offset[i]));

                    byte[] keyBytes = new byte[size];
                    Array.Copy(bytes, list.Offset[i], keyBytes, 0, keyBytes.Length);
                    nkArray[i] = new NamedKey(keyBytes, this.HivePath);
                }

                return nkArray;
            }
            else
            {
                return null;
            }
        }

        public SecurityDescriptor GetSecurityKey()
        {
            byte[] bytes = Helper.GetHiveBytes(this.HivePath);

            byte[] securityKeyBytes = new byte[Math.Abs(BitConverter.ToInt32(bytes, this.SecurityKeyOffset))];
            Array.Copy(bytes, this.SecurityKeyOffset, securityKeyBytes, 0, securityKeyBytes.Length);
            return (new SecurityKey(securityKeyBytes)).Descriptor;
        }

        internal SecurityDescriptor GetSecurityKey(byte[] bytes)
        {
            byte[] securityKeyBytes = new byte[Math.Abs(BitConverter.ToInt32(bytes, this.SecurityKeyOffset))];
            Array.Copy(bytes, this.SecurityKeyOffset, securityKeyBytes, 0, securityKeyBytes.Length);

            return (new SecurityKey(securityKeyBytes)).Descriptor;
        }

        #endregion ClassMethods
    }
}
