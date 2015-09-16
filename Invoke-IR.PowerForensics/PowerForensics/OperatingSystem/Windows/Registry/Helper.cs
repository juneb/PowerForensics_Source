using System;
using InvokeIR.PowerForensics.NTFS;

namespace InvokeIR.PowerForensics.Registry
{
    public class Helper
    {
        public static byte[] GetHiveBytes(string path)
        {
            // Get bytes for the specific record
            string volume = "\\\\.\\" + path.Split('\\')[0];
            IndexEntry entry = IndexEntry.Get(path);
            FileRecord record = new FileRecord(FileRecord.GetRecordBytes(volume, (int)entry.RecordNumber), volume, true);
            return record.GetBytes(volume);
        }

        public static NamedKey GetRootKey(string path)
        {
            byte[] bytes = GetHiveBytes(path);

            #region RegistryHeader
            
            byte[] headerBytes = new byte[0x200];
            try
            {
                Array.Copy(bytes, 0, headerBytes, 0, headerBytes.Length);
            }
            catch
            {
                throw new Exception();
            }
            RegistryHeader header = new RegistryHeader(headerBytes);

            #endregion RegistryHeader

            int offset = (int)header.RootKeyOffset + RegistryHeader.HBINOFFSET;
            int size = Math.Abs(BitConverter.ToInt32(bytes, offset));

            byte[] cellBytes = new byte[size];
            Array.Copy(bytes, offset, cellBytes, 0, cellBytes.Length);
            return new NamedKey(cellBytes, path);
        }

        internal static NamedKey GetRootKey(byte[] bytes, string path)
        {
            #region RegistryHeader

            byte[] headerBytes = new byte[0x200];
            try
            {
                Array.Copy(bytes, 0, headerBytes, 0, headerBytes.Length);
            }
            catch
            {
                throw new Exception();
            }
            RegistryHeader header = new RegistryHeader(headerBytes);

            #endregion RegistryHeader

            int offset = (int)header.RootKeyOffset + RegistryHeader.HBINOFFSET;
            int size = Math.Abs(BitConverter.ToInt32(bytes, offset));

            byte[] cellBytes = new byte[size];
            Array.Copy(bytes, offset, cellBytes, 0, cellBytes.Length);
            return new NamedKey(cellBytes, path);
        }
    }
}
