using System;
using System.IO;
using InvokeIR.Win32;

namespace InvokeIR.PowerForensics.NTFS
{
    public class Bitmap
    {
        #region Properties

        public ulong Cluster;
        public bool InUse;
        
        #endregion Properties

        #region Constructors

        internal Bitmap(ulong cluster, bool inUse)
        {
            Cluster = cluster;
            InUse = inUse;
        }

        #endregion Constructors

        #region GetMethods

        internal static Bitmap Get(string volume, ulong cluster)
        {
            ulong sectorOffset = cluster / 4096;
            ulong byteOffset = (cluster % 4096) / 8;

            // Check for valid Volume name
            NativeMethods.getVolumeName(ref volume);

            IntPtr hVolume = NativeMethods.getHandle(volume);

            // Set up FileStream to read volume
            FileStream streamToRead = NativeMethods.getFileStream(hVolume);

            // Get VolumeBootRecord object for logical addressing
            VolumeBootRecord VBR = new VolumeBootRecord(streamToRead);

            // Get the Data attribute
            NonResident dataStream = Bitmap.GetDataStream(Bitmap.GetFileRecord(volume));

            // Calulate the offset of the Bitmap file's data
            ulong dataRunOffset = (ulong)dataStream.DataRun[0].StartCluster * VBR.BytesPerCluster;

            // Calculate the offset of the sector that contains the entry for the specific cluster
            ulong offset = dataRunOffset + (VBR.BytesPerSector * sectorOffset); 

            // Read appropriate sector
            byte[] bytes = NativeMethods.readDrive(streamToRead, offset, VBR.BytesPerSector);

            byte b = bytes[byteOffset];

            bool inUse = false;

            switch (cluster % 8)
            {
                case 0:
                    if ((b & 0x01) > 0) { inUse = true; }
                    break;
                case 1:
                    if ((b & 0x02) > 0) { inUse = true; }
                    break;
                case 2:
                    if ((b & 0x04) > 0) { inUse = true; }
                    break;
                case 3:
                    if ((b & 0x08) > 0) { inUse = true; }
                    break;
                case 4:
                    if ((b & 0x10) > 0) { inUse = true; }
                    break;
                case 5:
                    if ((b & 0x20) > 0) { inUse = true; }
                    break;
                case 6:
                    if ((b & 0x40) > 0) { inUse = true; }
                    break;
                case 7:
                    if ((b & 0x80) > 0) { inUse = true; }
                    break;
            }
            return new Bitmap(cluster, inUse);
        }

        #endregion GetMethods

        #region GetInstancesMethods

        internal static Bitmap[] GetInstances(string volume)
        {
            // Check for valid Volume name
            NativeMethods.getVolumeName(ref volume);

            // Set up FileStream to read volume
            IntPtr hVolume = NativeMethods.getHandle(volume);
            FileStream streamToRead = NativeMethods.getFileStream(hVolume);

            // Get VolumeBootRecord object for logical addressing
            VolumeBootRecord VBR = new VolumeBootRecord(streamToRead);

            // Get the Data attribute
            NonResident dataStream = Bitmap.GetDataStream(Bitmap.GetFileRecord(volume));

            byte[] bytes = NativeMethods.readDrive(streamToRead, ((ulong)dataStream.DataRun[0].StartCluster * VBR.BytesPerCluster), ((ulong)dataStream.DataRun[0].ClusterLength * VBR.BytesPerCluster));

            Bitmap[] bitmapArray = new Bitmap[bytes.Length * 8];

            for (int j = 0; j < bytes.Length; j++)
            {
                for (int k = 0; k < 8; k++)
                {
                    bool inUse = false;
                    int index = ((j * 8) + k);

                    switch (k)
                    {
                        case 0:
                            if ((bytes[j] & 0x01) > 0) { inUse = true; }
                            break;
                        case 1:
                            if ((bytes[j] & 0x02) > 0) { inUse = true; }
                            break;
                        case 2:
                            if ((bytes[j] & 0x04) > 0) { inUse = true; }
                            break;
                        case 3:
                            if ((bytes[j] & 0x08) > 0) { inUse = true; }
                            break;
                        case 4:
                            if ((bytes[j] & 0x10) > 0) { inUse = true; }
                            break;
                        case 5:
                            if ((bytes[j] & 0x20) > 0) { inUse = true; }
                            break;
                        case 6:
                            if ((bytes[j] & 0x40) > 0) { inUse = true; }
                            break;
                        case 7:
                            if ((bytes[j] & 0x80) > 0) { inUse = true; }
                            break;
                    }
                    bitmapArray[index] = new Bitmap((ulong)index, inUse);
                }
            }

            return bitmapArray;
        }

        #endregion GetInstancesMethods

        internal static FileRecord GetFileRecord(string volume)
        {
            int index = IndexNumber.Get(volume, "\\$Bitmap");
            return new FileRecord(FileRecord.GetBytes(volume, index));
        }

        internal static NonResident GetDataStream(FileRecord fileRecord)
        {
            foreach (Attr attr in fileRecord.Attribute)
            {
                if (attr.Name == "DATA")
                {
                    return attr as NonResident;
                }
            }
            throw new Exception("No DATA attribute found.");
        }

    }
}
