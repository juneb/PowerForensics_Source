using System;
using System.IO;
using System.Text;
using InvokeIR.Win32;

namespace InvokeIR.PowerForensics.NTFS
{
    #region VolumeBootRecordClass

    public class VolumeBootRecord
    {

        #region Enums

        enum MEDIA_DESCRIPTOR
        {
            FloppyDisk = 0xF0,
            HardDriveDisk = 0xF8
        }

        #endregion Enums

        #region Properties

        public readonly string Signature;
        public readonly string MediaDescriptor;
        public readonly ushort BytesPerSector;
        public readonly uint BytesPerCluster;
        public readonly double BytesPerFileRecord;
        public readonly double BytesPerIndexBlock;
        public readonly ushort ReservedSectors;
        public readonly ushort SectorsPerTrack;
        public readonly ushort NumberOfHeads;
        public readonly uint HiddenSectors;
        public readonly ulong TotalSectors;
        public readonly ulong MFTStartIndex;
        public readonly ulong MFTMirrStartIndex;
        public readonly string VolumeSerialNumber;
        public readonly byte[] CodeSection;

        #endregion Properties

        #region Constructors

        internal VolumeBootRecord(FileStream streamToRead)
        {
            // Get VolumeBootRecord Bytes
            byte[] bytes = getBytes(streamToRead);

            // Get VolumeBootRecord Signature to determine File System Type
            byte[] sigBytes = new byte[8];
            Array.Copy(bytes, 3, sigBytes, 0, sigBytes.Length);
            Signature = Encoding.ASCII.GetString(sigBytes);

            // Check if NTFS Partition
            if (Signature == "NTFS    ")
            {
                BytesPerSector = BitConverter.ToUInt16(bytes, 11);
                BytesPerCluster = (uint)(bytes[13] * BytesPerSector);
                ReservedSectors = BitConverter.ToUInt16(bytes, 14);
                MediaDescriptor = Enum.GetName(typeof(MEDIA_DESCRIPTOR), bytes[21]).ToString();
                SectorsPerTrack = BitConverter.ToUInt16(bytes, 24);
                NumberOfHeads = BitConverter.ToUInt16(bytes, 26);
                HiddenSectors = BitConverter.ToUInt32(bytes, 28);
                TotalSectors = BitConverter.ToUInt64(bytes, 40);
                MFTStartIndex = BitConverter.ToUInt64(bytes, 48);
                MFTMirrStartIndex = BitConverter.ToUInt64(bytes, 56);

                sbyte clustersPerFileRecord = (sbyte)bytes[64];
                if (clustersPerFileRecord < 0)
                {
                    BytesPerFileRecord = Math.Pow(2, Math.Abs(clustersPerFileRecord));
                }
                else
                {
                    BytesPerFileRecord = clustersPerFileRecord * BytesPerCluster;
                }

                sbyte clustersPerIndexBlock = (sbyte)bytes[68];
                if (clustersPerIndexBlock < 0)
                {
                    BytesPerIndexBlock = Math.Pow(2, Math.Abs(clustersPerIndexBlock));
                }
                else
                {
                    BytesPerIndexBlock = clustersPerIndexBlock * BytesPerCluster;
                }

                byte[] snBytes = new byte[4];
                Array.Copy(bytes, 72, snBytes, 0, snBytes.Length);
                Array.Reverse(snBytes);
                VolumeSerialNumber = BitConverter.ToString(snBytes).Remove(2, 1).Remove(7, 1);

                byte[] codeBytes = new byte[430];
                Array.Copy(bytes, 80, codeBytes, 0, codeBytes.Length);
                CodeSection = codeBytes;
            }
            else
            {
                throw new Exception("Volume is not NTFS formatted.");
            }
        }

        #endregion Constructors

        #region InternalMethods

        internal static byte[] getBytes(FileStream streamToRead)
        {
            return NativeMethods.readDrive(streamToRead, 0, 512);
        }

        #endregion InternalMethods

    }

    #endregion VolumeBootRecordClass
}
