using System;
using System.IO;
using InvokeIR.Win32;

namespace InvokeIR.PowerForensics.NTFS
{
    #region MasterFileTableClass

    public class MasterFileTable
    {
        public static FileRecord GetRecord(FileStream streamToRead, string volume)
        {
            // Instantiate VolumeBootRecord object
            VolumeBootRecord VBR = new VolumeBootRecord(streamToRead);

            // Calculate byte offset to the Master File Table (MFT)
            ulong mftOffset = ((ulong)VBR.BytesPerCluster * VBR.MFTStartIndex);

            // Read bytes belonging to specified MFT Record and store in byte array
            return new FileRecord(NativeMethods.readDrive(streamToRead, mftOffset, (ulong)VBR.BytesPerFileRecord), volume, true);
        }

        public static byte[] GetBytes(FileStream streamToRead, string volume)
        {
            FileRecord mftRecord = GetRecord(streamToRead, volume);

            foreach (Attr attr in mftRecord.Attribute)
            {
                if (attr.Name == "DATA")
                {
                    return (attr as NonResident).GetBytes(volume);
                }
            }
            throw new Exception("Error reading MFT bytes.");
        }
    }
    #endregion MasterFileTableClass
}
