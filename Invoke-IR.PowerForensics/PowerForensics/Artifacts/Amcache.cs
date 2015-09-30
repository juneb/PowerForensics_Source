using System;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
using PowerForensics.Ntfs;
using PowerForensics.Registry;

namespace PowerForensics.Artifacts
{
    #region AmcacheClass

    // https://msdn.microsoft.com/en-us/library/cc248285.aspx
    public class Amcache
    {
        #region Properties

        public readonly string ProductName;
        public readonly string CompanyName;
        public readonly uint FileSize;
        public readonly string Description;
        public readonly DateTime CompileTime;
        public readonly DateTime ModifiedTime;
        public readonly DateTime BornTime;
        public readonly string Path;
        public readonly DateTime ModifiedTime2;
        public readonly string Hash;

        #endregion Properties

        #region Constructors

        internal Amcache(NamedKey nk, byte[] bytes)
        {
            foreach(ValueKey vk in nk.GetValues(bytes))
            {
                switch(vk.Name)
                {
                    case "0":
                        ProductName = Encoding.Unicode.GetString(vk.GetData(bytes));
                        break;
                    case "1":
                        CompanyName = Encoding.Unicode.GetString(vk.GetData(bytes));
                        break;
                    case "6":
                        FileSize = BitConverter.ToUInt32(vk.GetData(bytes), 0x00);
                        break;
                    case "c":
                        Description = Encoding.Unicode.GetString(vk.GetData(bytes));
                        break;
                    case "f":
                        CompileTime = new DateTime(1970, 1, 1).AddSeconds(BitConverter.ToInt32(vk.GetData(bytes), 0x00));
                        break;
                    case "11":
                        ModifiedTime = DateTime.FromFileTimeUtc(BitConverter.ToInt64(vk.GetData(bytes), 0x00));
                        break;
                    case "12":
                        BornTime = DateTime.FromFileTimeUtc(BitConverter.ToInt64(vk.GetData(bytes), 0x00));
                        break;
                    case "15":
                        Path = Encoding.Unicode.GetString(vk.GetData(bytes));
                        break;
                    case "17":
                        ModifiedTime2 = DateTime.FromFileTimeUtc(BitConverter.ToInt64(vk.GetData(bytes), 0x00));
                        break;
                    case "101":
                        Hash = Encoding.Unicode.GetString(vk.GetData(bytes)).TrimStart('0');
                        break;
                    default:
                        break;
                }
            }
        }

        #endregion Constructors

        #region StaticMethods

        #region GetInstancesMethods

        public static Amcache[] GetInstances(string volume)
        {
            string volLetter = volume.Split('\\')[3];
            string hivePath = volLetter + @"\Windows\AppCompat\Programs\Amcache.hve";
            return GetInstancesByPath(hivePath);
        }

        public static Amcache[] GetInstancesByPath(string hivePath)
        {
            string Key = @"Root\File";

            byte[] bytes = Registry.Helper.GetHiveBytes(hivePath);

            NamedKey[] FileSubKey = NamedKey.GetInstances(bytes, hivePath, Key);

            List<Amcache> amcacheList = new List<Amcache>();

            foreach (NamedKey key in FileSubKey)
            {
                foreach (NamedKey nk in key.GetSubKeys(bytes))
                {
                    amcacheList.Add(new Amcache(nk, bytes));
                }
            }
            return amcacheList.ToArray();
        }

        #endregion GetInstancesMethods

        #endregion StaticMethods
    }

    #endregion AmcacheClass
}
