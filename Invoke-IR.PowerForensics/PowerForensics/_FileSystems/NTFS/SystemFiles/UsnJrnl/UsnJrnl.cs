using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using InvokeIR.Win32;

namespace InvokeIR.PowerForensics.NTFS
{
    #region UsnJrnlClass

    public class UsnJrnl
    {
        #region Enums

        [FlagsAttribute]
        internal enum USN_REASON : uint
        {
            DATA_OVERWRITE = 0x00000001,
            DATA_EXTEND = 0x00000002,
            DATA_TRUNCATION = 0x00000004,
            NAMED_DATA_OVERWRITE = 0x00000010,
            NAMED_DATA_EXTEND = 0x00000020,
            NAMED_DATA_TRUNCATION = 0x00000040,
            FILE_CREATE = 0x00000100,
            FILE_DELETE = 0x00000200,
            EA_CHANGE = 0x00000400,
            SECURITY_CHANGE = 0x00000800,
            RENAME_OLD_NAME = 0x00001000,
            RENAME_NEW_NAME = 0x00002000,
            INDEXABLE_CHANGE = 0x00004000,
            BASIC_INFO_CHANGE = 0x00008000,
            HARD_LINK_CHANGE = 0x00010000,
            COMPRESSION_CHANGE = 0x00020000,
            ENCRYPTION_CHANGE = 0x00040000,
            OBJECT_ID_CHANGE = 0x00080000,
            REPARSE_POINT_CHANGE = 0x00100000,
            STREAM_CHANGE = 0x00200000,
            CLOSE = 0x80000000
        }

        [FlagsAttribute]
        private enum USN_SOURCE : uint
        {
            DATA_MANAGEMENT = 0x00000001,
            AUXILIARY_DATA = 0x00000002,
            REPLICATION_MANAGEMENT = 0x00000004
        }

        #endregion Enums

        #region Properties

        public readonly Version Version;
        public readonly ulong FileReferenceNumber;
        //public readonly ushort FileSequenceNumber;
        public readonly ulong ParentFileReferenceNumber;
        //public readonly ushort ParentFileSequenceNumber;
        public readonly ulong Usn;
        public readonly DateTime TimeStamp;
        public readonly string Reason;
        public readonly string SourceInfo;
        public readonly uint SecurityId;
        public readonly string FileAttributes;
        public readonly string FileName;

        #endregion Properties

        #region Constructors

        internal UsnJrnl(byte[] bytes, ref int offset)
        {
            uint RecordLength = RecordLength = BitConverter.ToUInt32(bytes, (0x00 + offset));
            ushort MajorVersion = BitConverter.ToUInt16(bytes, (0x04 + offset));
            ushort MinorVersion = BitConverter.ToUInt16(bytes, (0x06 + offset));
            Version = new System.Version(MajorVersion, MinorVersion);
            FileReferenceNumber = (BitConverter.ToUInt64(bytes, (0x08 + offset)) & 0x0000FFFFFFFFFFFF);
            //FileSequenceNumber = (ushort)(BitConverter.ToUInt64(bytes, (0x08 + offset)) & 0xFFFF000000000000);
            ParentFileReferenceNumber = (BitConverter.ToUInt64(bytes, (0x10 + offset)) & 0x0000FFFFFFFFFFFF);
            //ParentFileSequenceNumber = (ushort)(BitConverter.ToUInt64(bytes, (0x10 + offset)) & 0xFFFF000000000000);
            Usn = BitConverter.ToUInt64(bytes, (0x18 + offset));
            TimeStamp = DateTime.FromFileTime(BitConverter.ToInt64(bytes, (0x20 + offset)));
            Reason = ((USN_REASON)BitConverter.ToUInt32(bytes, (0x28 + offset))).ToString();
            SourceInfo = ((USN_SOURCE)BitConverter.ToUInt32(bytes, (0x2C + offset))).ToString();
            SecurityId = BitConverter.ToUInt32(bytes, (0x30 + offset));
            FileAttributes = ((StandardInformation.ATTR_STDINFO_PERMISSION)BitConverter.ToUInt32(bytes, (0x34 + offset))).ToString();
            ushort fileNameLength = BitConverter.ToUInt16(bytes, (0x38 + offset));
            ushort fileNameOffset = BitConverter.ToUInt16(bytes, (0x3A + offset));
            byte[] fileNameBytes = new byte[fileNameLength];
            Array.Copy(bytes, (0x3C + offset), fileNameBytes, 0, fileNameBytes.Length);
            FileName = Encoding.Unicode.GetString(fileNameBytes);
            offset += (int)RecordLength;
        }

        #endregion Constructors

        public static UsnJrnl Get(string volume, ulong usn)
        {
            // Check for valid Volume name
            NativeMethods.getVolumeName(ref volume);

            // Set up FileStream to read volume
            IntPtr hVolume = NativeMethods.getHandle(volume);
            FileStream streamToRead = NativeMethods.getFileStream(hVolume);

            // Get VolumeBootRecord object for logical addressing
            VolumeBootRecord VBR = new VolumeBootRecord(streamToRead);

            // Get the $J Data attribute (contains UsnJrnl details
            NonResident J = UsnJrnl.GetJStream(UsnJrnl.GetFileRecord(volume));

            // Determine the length of the initial sparse pages
            ulong SparseLength = (ulong)J.DataRun[0].ClusterLength * VBR.BytesPerCluster;
            // Subtract length of sparse data from desired usn offset
            ulong usnOffset = usn - SparseLength;

            // Iterate through each data run
            for (int i = 1; i < J.DataRun.Length; i++)
            {
                // Determine length of current DataRun
                ulong dataRunLength = (ulong)J.DataRun[i].ClusterLength * VBR.BytesPerCluster;

                // Check if usnOffset resides in current DataRun
                if (dataRunLength <= usnOffset)
                {
                    // If not, subtract length of DataRun from usnOffset
                    usnOffset -= dataRunLength;
                }

                // If usnOffset resides within DataRun, parse associated UsnJrnl Entry
                else
                {
                    // Read DataRun from disk
                    byte[] fragmentBytes = NativeMethods.readDrive(streamToRead, ((ulong)J.DataRun[i].StartCluster * VBR.BytesPerCluster), ((ulong)J.DataRun[i].ClusterLength * VBR.BytesPerCluster));

                    // Instatiate a byte array that is the size of a single cluster
                    byte[] clusterBytes = new byte[VBR.BytesPerCluster];

                    // Iterate through the clusters in the DataRun
                    for (long j = 0; j < J.DataRun[i].ClusterLength; j++)
                    {
                        // If usnOffset is not in current cluster, then subtract cluster size from offset and iterate
                        if (VBR.BytesPerCluster <= usnOffset)
                        {
                            usnOffset -= VBR.BytesPerCluster;
                        }
                        // Else if usnOffset is in current cluster
                        else
                        {
                            // Copy current cluster bytes to clusterBytes variable
                            Array.Copy(fragmentBytes, ((long)j * VBR.BytesPerCluster), clusterBytes, 0, clusterBytes.Length);
                            //Console.WriteLine("{0}", usnOffset);
                            // Parse desired UsnJrnl entry from cluster
                            int offset = (int)usnOffset;
                            return new UsnJrnl(clusterBytes, ref offset);
                        }
                    }
                }
            }
            return null;
        }

        public static UsnJrnl[] GetInstances(string volume)
        {
            // Check for valid Volume name
            NativeMethods.getVolumeName(ref volume);

            // Set up FileStream to read volume
            IntPtr hVolume = NativeMethods.getHandle(volume);
            FileStream streamToRead = NativeMethods.getFileStream(hVolume);

            // Get VolumeBootRecord object for logical addressing
            VolumeBootRecord VBR = new VolumeBootRecord(streamToRead);

            // Get the $J Data attribute (contains UsnJrnl details
            NonResident J = UsnJrnl.GetJStream(UsnJrnl.GetFileRecord(volume));

            List<UsnJrnl> usnList = new List<UsnJrnl>();

            for (int i = 0; i < J.DataRun.Length; i++)
            {
                if (!(J.DataRun[i].Sparse))
                {
                    long clusterCount = J.DataRun[i].ClusterLength;

                    byte[] fragmentBytes = NativeMethods.readDrive(streamToRead, ((ulong)J.DataRun[i].StartCluster * VBR.BytesPerCluster), ((ulong)clusterCount * VBR.BytesPerCluster));
                    byte[] clusterBytes = new byte[VBR.BytesPerCluster];

                    for (long j = 0; j < clusterCount; j++)
                    {
                        Array.Copy(fragmentBytes, ((long)j * VBR.BytesPerCluster), clusterBytes, 0, clusterBytes.Length);

                        int offset = 0;

                        do
                        {
                            if (clusterBytes[offset] == 0)
                            {
                                break;
                            }

                            try
                            {
                                UsnJrnl usn = new UsnJrnl(clusterBytes, ref offset);
                                if (usn.Version > new Version(4, 0))
                                {
                                    break;
                                }
                                usnList.Add(usn);
                            }
                            catch
                            {
                                break;
                            }

                        } while (offset >= 0 && offset < clusterBytes.Length);
                    }
                }
            }
            return usnList.ToArray();
        }

        internal static FileRecord GetFileRecord(string volume)
        {
            int index = IndexNumber.Get(volume, "\\$Extend\\$UsnJrnl");
            return new FileRecord(FileRecord.GetBytes(volume, index));
        }

        internal static NonResident GetJStream(FileRecord fileRecord)
        {
            foreach (Attr attr in fileRecord.Attribute)
            {
                if (attr.NameString == "$J")
                {
                    return attr as NonResident;
                }
            }
            throw new Exception("No $J attribute found.");
        }

        internal static Data GetMaxStream(FileRecord fileRecord)
        {
            foreach (Attr attr in fileRecord.Attribute)
            {
                if (attr.NameString == "$Max")
                {
                    return attr as Data;
                }
            }
            throw new Exception("No $MAX attribute found.");
        }
    }

    #endregion USNJrnlClass
}
