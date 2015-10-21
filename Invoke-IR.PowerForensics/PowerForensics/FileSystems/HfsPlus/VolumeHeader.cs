using System;
using System.IO;
using System.Text;
using InvokeIR.Win32;

namespace PowerForensics.HfsPlus
{
    public class VolumeHeader
    {
        #region Properties

        public readonly string Signature;
        public readonly ushort Version;
        public readonly uint Attributes;
        public readonly uint LastMountedVersion;
        public readonly uint JournalInfoBlock;

        public readonly uint CreateDate;
        public readonly uint ModifyDate;
        public readonly uint BackupDate;
        public readonly uint CheckedDate;

        public readonly uint FileCount;
        public readonly uint FolderCount;

        public readonly uint BlockSize;
        public readonly uint TotalBlocks;
        public readonly uint FreeBlocks;

        public readonly uint NextAllocation;
        public readonly uint RsrcClumpSize;
        public readonly uint DataClumpSize;
        public readonly CatalogNodeId.CATALOG_ID NextCatalogId;

        public readonly uint WriteCount;
        public readonly ulong EncodingsBitmap;
        
        //public readonly uint[] FinderInfo;

        public readonly ForkData AllocationFile;
        public readonly ForkData ExtentsFile;
        public readonly ForkData CatalogFile;
        public readonly ForkData AttributesFile;
        public readonly ForkData StartupFile;
 
        #endregion Properties

        #region Constructors

        internal VolumeHeader(byte[] bytes)
        {
            Signature = Encoding.ASCII.GetString(bytes, 0x00, 0x02);

            if (Signature == "H+")
            {
                Version = BitConverter.ToUInt16(bytes, 0x02);
                Attributes = BitConverter.ToUInt32(bytes, 0x04);
                LastMountedVersion = BitConverter.ToUInt32(bytes, 0x08);
                JournalInfoBlock = BitConverter.ToUInt32(bytes, 0x0C);

                CreateDate = BitConverter.ToUInt32(bytes, 0x10);
                ModifyDate = BitConverter.ToUInt32(bytes, 0x14);
                BackupDate = BitConverter.ToUInt32(bytes, 0x18);
                CheckedDate = BitConverter.ToUInt32(bytes, 0x1C);

                FileCount = BitConverter.ToUInt32(bytes, 0x20);
                FolderCount = BitConverter.ToUInt32(bytes, 0x24);

                BlockSize = BitConverter.ToUInt32(bytes, 0x28);
                TotalBlocks = BitConverter.ToUInt32(bytes, 0x2C);
                FreeBlocks = BitConverter.ToUInt32(bytes, 0x30);

                NextAllocation = BitConverter.ToUInt32(bytes, 0x34);
                RsrcClumpSize = BitConverter.ToUInt32(bytes, 0x38);
                DataClumpSize = BitConverter.ToUInt32(bytes, 0x3C);
                NextCatalogId = (CatalogNodeId.CATALOG_ID)BitConverter.ToUInt32(bytes, 0x40);

                WriteCount = BitConverter.ToUInt32(bytes, 0x44);
                EncodingsBitmap = BitConverter.ToUInt64(bytes, 0x48);

                //FinderInfo = BitConverter.ToUInt32(bytes, 0x50);

                AllocationFile = new ForkData(NativeMethods.GetSubArray(bytes, 0x70, 0x50));
                ExtentsFile = new ForkData(NativeMethods.GetSubArray(bytes, 0xC0, 0x50));
                CatalogFile = new ForkData(NativeMethods.GetSubArray(bytes, 0x110, 0x50));
                AttributesFile = new ForkData(NativeMethods.GetSubArray(bytes, 0x160, 0x50));
                StartupFile = new ForkData(NativeMethods.GetSubArray(bytes, 0x1B0, 0x50));
            }
            else
            {
                throw new Exception("Not a valid HFS+ volume");
            }

        }

        #endregion Constructors

        #region StaticMethods
        public static VolumeHeader Get(byte[] bytes)
        {
            return new VolumeHeader(NativeMethods.GetSubArray(bytes, 1024, 512));
        }

        public static VolumeHeader Get(string volume, ulong offset)
        {
            NativeMethods.getVolumeName(ref volume);
            IntPtr hVolume = NativeMethods.getHandle(volume);
            using (FileStream streamToRead = NativeMethods.getFileStream(hVolume))
            {
                return new VolumeHeader(NativeMethods.readDrive(streamToRead, offset, 1024));
            }
        }

        #endregion StaticMethods
    }
}
