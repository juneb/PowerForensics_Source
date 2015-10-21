using System;
using System.IO;
using System.Collections.Generic;
using InvokeIR.Win32;

namespace PowerForensics.Ntfs
{
    #region MasterFileTableClass

    public class MasterFileTable
    {
        #region StaticMethods

        #region GetRecordMethods
        
        public static FileRecord GetRecord(string volume)
        {
            NativeMethods.getVolumeName(ref volume);
            IntPtr hVolume = NativeMethods.getHandle(volume);
            using (FileStream streamToRead = NativeMethods.getFileStream(hVolume))
            {
                return GetRecord(streamToRead, volume);
            }
        }

        /*public static FileRecord GetRecord(string path)
        {

        }*/

        internal static FileRecord GetRecord(FileStream streamToRead, string volume)
        {
            // Instantiate VolumeBootRecord object
            VolumeBootRecord VBR = VolumeBootRecord.Get(streamToRead);

            // Calculate byte offset to the Master File Table (MFT)
            ulong mftOffset = ((ulong)VBR.BytesPerCluster * VBR.MFTStartIndex);

            // Read bytes belonging to specified MFT Record and store in byte array
            return new FileRecord(NativeMethods.readDrive(streamToRead, mftOffset, (ulong)VBR.BytesPerFileRecord), volume, true);
        }

        #endregion GetRecordMethods

        #region GetBytesMethods

        public static byte[] GetBytes(string volume)
        {
            NativeMethods.getVolumeName(ref volume);
            IntPtr hVolume = NativeMethods.getHandle(volume);
            using (FileStream streamToRead = NativeMethods.getFileStream(hVolume))
            {
                return GetBytes(streamToRead, volume);
            }
        }

        internal static byte[] GetBytes(FileStream streamToRead, string volume)
        {
            FileRecord mftRecord = GetRecord(streamToRead, volume);

            foreach (Attr attr in mftRecord.Attribute)
            {
                if (attr.Name == Attr.ATTR_TYPE.DATA)
                {
                    return (attr as NonResident).GetBytes(volume);
                }
            }
            throw new Exception("Error reading MFT bytes.");
        }

        #endregion GetBytesMethods

        #region GetSlackMethods

        public static byte[] GetSlack(string volume)
        {
            byte[] bytes = GetBytes(volume);
            return GetSlack(bytes);
        }

        public static byte[] GetSlackByPath(string path)
        {
            FileRecord record = FileRecord.Get(path, true);
            byte[] bytes = record.GetContent();
            return GetSlack(bytes);
        }

        private static byte[] GetSlack(byte[] bytes)
        {
            int i = 0;
            uint size = 0;

            while (i < bytes.Length)
            {
                uint realsize = BitConverter.ToUInt32(bytes, i + 0x18);
                uint allocatedsize = BitConverter.ToUInt32(bytes, i + 0x1C);
                size += allocatedsize - realsize;
                i += 1024;
            }

            byte[] slackbytes = new byte[size];

            i = 0;
            size = 0;

            while (i < bytes.Length)
            {
                uint realsize = BitConverter.ToUInt32(bytes, i + 0x18);
                uint allocatedsize = BitConverter.ToUInt32(bytes, i + 0x1C);
                uint slacksize = allocatedsize - realsize;
                Array.Copy(bytes, i + realsize, slackbytes, size, slacksize);
                size += slacksize;
                i += 1024;
            }

            return slackbytes;
        }

        #endregion GetSlackMethods

        #endregion StaticMethods
    }

    #endregion MasterFileTableClass
}
