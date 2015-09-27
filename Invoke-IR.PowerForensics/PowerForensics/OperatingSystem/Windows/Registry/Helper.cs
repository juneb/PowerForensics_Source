using System;
using InvokeIR.PowerForensics.Ntfs;
using InvokeIR.Win32;

namespace InvokeIR.PowerForensics.Registry
{
    #region HelperClass

    public class Helper
    {
        #region StaticMethods

        public static byte[] GetHiveBytes(string path)
        {
            // Get bytes for the specific record
            string volume = "\\\\.\\" + path.Split('\\')[0];
            IndexEntry entry = IndexEntry.Get(path);
            FileRecord record = new FileRecord(FileRecord.GetRecordBytes(volume, (int)entry.RecordNumber), volume, true);
            return record.GetBytes();
        }

        public static NamedKey GetRootKey(string path)
        {
            byte[] bytes = GetHiveBytes(path);

            RegistryHeader header = new RegistryHeader(NativeMethods.GetSubArray(bytes, 0x00, 0x200));
            int offset = (int)header.RootKeyOffset + RegistryHeader.HBINOFFSET;
            int size = Math.Abs(BitConverter.ToInt32(bytes, offset));

            return new NamedKey(NativeMethods.GetSubArray(bytes, (uint)offset, (uint)size), path);
        }

        internal static NamedKey GetRootKey(byte[] bytes, string path)
        {
            #region RegistryHeader

            RegistryHeader header = new RegistryHeader(NativeMethods.GetSubArray(bytes, 0x00, 0x200));

            #endregion RegistryHeader

            int offset = (int)header.RootKeyOffset + RegistryHeader.HBINOFFSET;
            int size = Math.Abs(BitConverter.ToInt32(bytes, offset));

            return new NamedKey(NativeMethods.GetSubArray(bytes, (uint)offset, (uint)size), path);
        }
        
        #endregion StaticMethods
    }

    #endregion HelperClass
}
