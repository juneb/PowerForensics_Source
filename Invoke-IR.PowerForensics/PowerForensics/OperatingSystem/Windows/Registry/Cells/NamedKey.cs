using System;
using System.Text;
using InvokeIR.Win32;

namespace InvokeIR.PowerForensics.Registry
{
    #region NamedKeyClass
    public class NamedKey : Cell
    {
        #region Enums

        [FlagsAttribute]
        public enum NAMED_KEY_FLAGS
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
        internal readonly NAMED_KEY_FLAGS Flags;
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
            Signature = Encoding.ASCII.GetString(bytes, 0x04, 0x02);
            
            if (Signature == "nk")
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

                #endregion CellHeader

                Flags = (NAMED_KEY_FLAGS)BitConverter.ToUInt16(bytes, 0x06);
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
                    Name = Encoding.ASCII.GetString(bytes, 0x50, Math.Abs(KeyNameSize));
                }

                #endregion KeyNameString
            }
            else
            {
                throw new Exception("Cell is not a valid Named Key");
            }
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

        #region InstanceMethods

        public ValueKey[] GetValues()
        {
            if (this.NumberOfValues > 0)
            {
                byte[] bytes = Helper.GetHiveBytes(this.HivePath);
                
                return this.GetValues(bytes);
            }

            return null;
        }

        internal ValueKey[] GetValues(byte[] bytes)
        {
            if (this.NumberOfValues > 0)
            {
                ValuesList list = new ValuesList(NativeMethods.GetSubArray(bytes, (uint)this.ValuesListOffset, (uint)Math.Abs(BitConverter.ToInt32(bytes, this.ValuesListOffset))), this.NumberOfValues);

                ValueKey[] vkArray = new ValueKey[list.Offset.Length];

                for (int i = 0; i < list.Offset.Length; i++)
                {
                    int size = Math.Abs(BitConverter.ToInt32(bytes, (int)list.Offset[i]));
                    vkArray[i] = new ValueKey(NativeMethods.GetSubArray(bytes, list.Offset[i], (uint)size), this.HivePath, this.Name);
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
                return this.GetSubKeys(bytes);
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
                byte[] subKeyListBytes = NativeMethods.GetSubArray(bytes, (uint)this.SubKeysListOffset, (uint)Math.Abs(BitConverter.ToInt32(bytes, this.SubKeysListOffset)));
                string type = Encoding.ASCII.GetString(subKeyListBytes, 0x04, 0x02);

                List list = List.Factory(bytes, subKeyListBytes, type);

                NamedKey[] nkArray = new NamedKey[list.Count];

                for (int i = 0; i < list.Count; i++)
                {
                    int size = Math.Abs(BitConverter.ToInt32(bytes, (int)list.Offset[i]));
                    nkArray[i] = new NamedKey(NativeMethods.GetSubArray(bytes, (uint)list.Offset[i], (uint)size), this.HivePath);
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
            return this.GetSecurityKey(bytes);
        }

        internal SecurityDescriptor GetSecurityKey(byte[] bytes)
        {
            return (new SecurityKey(NativeMethods.GetSubArray(bytes, (uint)this.SecurityKeyOffset, (uint)Math.Abs(BitConverter.ToInt32(bytes, this.SecurityKeyOffset))))).Descriptor;
        }

        #endregion InstanceMethods
    }

    #endregion NamedKeyClass
}
