using System;
using System.IO;
using InvokeIR.Win32;

namespace InvokeIR.PowerForensics.NTFS
{
    #region MasterFileTableClass

    public class MasterFileTable
    {
        // Get Master File Table Bytes for "Get" Methods
        // GetBytes will clean up IntPtr (Volume Handle) and FileStream objects
        public static byte[] GetBytes(string volume)
        {
            // Get a handle to the specified volume
            IntPtr hVolume = NativeMethods.getHandle(volume);

            // Instatiate null byte array
            byte[] mftBytes = null;

            // Create FileStream to read from the Volume file handle
            using (FileStream streamToRead = NativeMethods.getFileStream(hVolume))
            {
                // Instantiate VolumeBootRecord object
                VolumeBootRecord VBR = new VolumeBootRecord(streamToRead);

                // Calculate byte offset to the Master File Table (MFT)
                ulong mftOffset = ((ulong)VBR.BytesPerCluster * VBR.MFTStartIndex);

                // Read bytes belonging to specified MFT Record and store in byte array
                FileRecord mftRecord = new FileRecord(NativeMethods.readDrive(streamToRead, mftOffset, (ulong)VBR.BytesPerFileRecord), volume);

                foreach (Attr attr in mftRecord.Attribute)
                {
                    if (attr.Name == "DATA")
                    {
                        return (attr as NonResident).GetBytes(volume);
                    }
                }
                throw new Exception("Error reading MFT bytes.");
            }
            //NativeMethods.CloseHandle(hVolume);
        }

        public static byte[] GetBytes(FileStream streamToRead, string volume)
        {

            // Instantiate VolumeBootRecord object
            VolumeBootRecord VBR = new VolumeBootRecord(streamToRead);

            // Calculate byte offset to the Master File Table (MFT)
            ulong mftOffset = ((ulong)VBR.BytesPerCluster * VBR.MFTStartIndex);

            // Read bytes belonging to specified MFT Record and store in byte array
            FileRecord mftRecord = new FileRecord(NativeMethods.readDrive(streamToRead, mftOffset, (ulong)VBR.BytesPerFileRecord), volume);

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
