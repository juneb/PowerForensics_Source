﻿using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using InvokeIR.Win32;

namespace PowerForensics.Ntfs
{
    #region UsnJrnlClass

    public class UsnJrnl
    {
        #region Enums

        [FlagsAttribute]
        public enum USN_REASON : uint
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
        public enum USN_SOURCE : uint
        {
            DATA_MANAGEMENT = 0x00000001,
            AUXILIARY_DATA = 0x00000002,
            REPLICATION_MANAGEMENT = 0x00000004
        }

        #endregion Enums

        #region Properties

        private readonly static Version USN40Version = new Version(4, 0);
        
        public readonly string VolumePath;
        public readonly Version Version;
        public readonly ulong RecordNumber;
        public readonly ushort FileSequenceNumber;
        public readonly ulong ParentFileRecordNumber;
        public readonly ushort ParentFileSequenceNumber;
        public readonly ulong Usn;
        public readonly DateTime TimeStamp;
        public readonly USN_REASON Reason;
        public readonly USN_SOURCE SourceInfo;
        public readonly uint SecurityId;
        public readonly StandardInformation.ATTR_STDINFO_PERMISSION FileAttributes;
        public readonly string FileName;

        #endregion Properties

        #region Constructors

        private UsnJrnl(byte[] bytes, string volume, ref int offset)
        {
            uint RecordLength = RecordLength = BitConverter.ToUInt32(bytes, (0x00 + offset));
            VolumePath = volume;
            Version = new System.Version(BitConverter.ToUInt16(bytes, (0x04 + offset)), BitConverter.ToUInt16(bytes, (0x06 + offset)));
            RecordNumber = (BitConverter.ToUInt64(bytes, (0x08 + offset)) & 0x0000FFFFFFFFFFFF);
            FileSequenceNumber = ParentFileSequenceNumber = BitConverter.ToUInt16(bytes, (0x0E + offset));
            ParentFileRecordNumber = (BitConverter.ToUInt64(bytes, (0x10 + offset)) & 0x0000FFFFFFFFFFFF);
            ParentFileSequenceNumber = BitConverter.ToUInt16(bytes, (0x16 + offset));
            Usn = BitConverter.ToUInt64(bytes, (0x18 + offset));
            TimeStamp = DateTime.FromFileTimeUtc(BitConverter.ToInt64(bytes, (0x20 + offset)));
            Reason = ((USN_REASON)BitConverter.ToUInt32(bytes, (0x28 + offset)));
            SourceInfo = ((USN_SOURCE)BitConverter.ToUInt32(bytes, (0x2C + offset)));
            SecurityId = BitConverter.ToUInt32(bytes, (0x30 + offset));
            FileAttributes = ((StandardInformation.ATTR_STDINFO_PERMISSION)BitConverter.ToUInt32(bytes, (0x34 + offset)));
            ushort fileNameLength = BitConverter.ToUInt16(bytes, (0x38 + offset));
            ushort fileNameOffset = BitConverter.ToUInt16(bytes, (0x3A + offset));
            FileName = Encoding.Unicode.GetString(bytes, 0x3C + offset, fileNameLength); 
            offset += (int)RecordLength;
        }

        #endregion Constructors

        #region StaticMethods

        #region GetMethods

        public static UsnJrnl Get(string path, ulong usn)
        {
            string volume = NativeMethods.GetVolumeFromPath(path);
            IndexEntry entry = IndexEntry.Get(path);
            return Get(volume, (int)entry.RecordNumber, usn);
        }

        private static UsnJrnl Get(string volume, int recordnumber, ulong usn)
        {
            // Check for valid Volume name
            NativeMethods.getVolumeName(ref volume);

            // Set up FileStream to read volume
            IntPtr hVolume = NativeMethods.getHandle(volume);
            FileStream streamToRead = NativeMethods.getFileStream(hVolume);

            // Get VolumeBootRecord object for logical addressing
            VolumeBootRecord VBR = VolumeBootRecord.Get(streamToRead);

            FileRecord record = FileRecord.Get(volume, recordnumber, true);

            // Get the $J Data attribute (contains UsnJrnl details
            NonResident J = UsnJrnl.GetJStream(record);

            // Determine the length of the initial sparse pages
            ulong SparseLength = (ulong)J.DataRun[0].ClusterLength * VBR.BytesPerCluster;

            if (usn > SparseLength)
            {
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

                                // Parse desired UsnJrnl entry from cluster
                                int offset = (int)usnOffset;
                                return new UsnJrnl(clusterBytes, volume, ref offset);
                            }
                        }
                    }
                }
                return null;
            }
            else
            {
                throw new Exception("UsnJrnl entry has has been overwritten");
            }
        }

        #endregion GetMethods

        #region GetInstancesMethods

        public static UsnJrnl[] GetInstances(string volume)
        {
            IndexEntry entry = IndexEntry.Get(volume.Split('\\')[3] + "\\$Extend\\$UsnJrnl");
            return GetInstances(volume, (int)entry.RecordNumber);
        }

        public static UsnJrnl[] GetInstancesByPath(string path)
        {
            string volume = NativeMethods.GetVolumeFromPath(path);
            IndexEntry entry = IndexEntry.Get(path);
            return GetInstances(volume, (int)entry.RecordNumber);
        }

        private static UsnJrnl[] GetInstances(string volume, int recordnumber)
        {
            // Check for valid Volume name
            NativeMethods.getVolumeName(ref volume);

            // Set up FileStream to read volume
            IntPtr hVolume = NativeMethods.getHandle(volume);
            FileStream streamToRead = NativeMethods.getFileStream(hVolume);

            // Get VolumeBootRecord object for logical addressing
            VolumeBootRecord VBR = VolumeBootRecord.Get(streamToRead);

            FileRecord record = FileRecord.Get(volume, recordnumber, true);

            // Get the $J Data attribute (contains UsnJrnl details
            NonResident J = UsnJrnl.GetJStream(record);

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
                                UsnJrnl usn = new UsnJrnl(clusterBytes, volume, ref offset);
                                if (usn.Version > USN40Version)
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

        #endregion GetInstancesMethods

        internal static NonResident GetJStream(FileRecord fileRecord)
        {
            foreach (Attr attr in fileRecord.Attribute)
            {
                if (attr.NameString == "$J")
                {
                    return attr as NonResident;
                }
                
                AttributeList attrList = attr as AttributeList;
                if (attrList != null)
                {
                    foreach (AttrRef ar in attrList.AttributeReference)
                    {
                        if (ar.NameString == "$J")
                        {
                            FileRecord record = new FileRecord(FileRecord.GetRecordBytes(fileRecord.VolumePath, (int)ar.RecordNumber), fileRecord.VolumePath, true);
                            return GetJStream(record);
                        }
                    }
                }
            }
            throw new Exception("No $J attribute found.");
        }

        #endregion StaticMethods

        #region InstanceMethods

        public FileRecord GetFileRecord()
        {
            FileRecord record = FileRecord.Get(this.VolumePath, (int)this.RecordNumber, false);

            if (record.SequenceNumber == this.FileSequenceNumber)
            {
                return record;
            }
            else
            {
                throw new Exception("Desired FileRecord has been overwritten");
            }
        }

        public FileRecord GetParentFileRecord()
        {
            FileRecord record = FileRecord.Get(this.VolumePath, (int)this.ParentFileRecordNumber, false);

            if (record.SequenceNumber == this.ParentFileSequenceNumber)
            {
                return record;
            }
            else
            {
                throw new Exception("Desired FileRecord has been overwritten");
            }
        }

        #region ToStringOverride

        public override string ToString()
        {
            return String.Format("UsnJrnl for {0} ({1}) Reason: {2}", this.FileName, this.RecordNumber, this.Reason); ;
        }

        #endregion ToStringOverride

        #endregion InstanceMethods
    }

    #endregion USNJrnlClass

    #region UsnJrnlDetailClass

    public class UsnJrnlDetail
    {
        #region Properties

        public ulong MaxSize;
        public ulong AllocationDelta;
        public ulong UsnId;
        public ulong LowestUsn;

        #endregion Properties

        #region Constructors

        private UsnJrnlDetail(byte[] maxBytes)
        {
            MaxSize = BitConverter.ToUInt64(maxBytes, 0x00);
            AllocationDelta = BitConverter.ToUInt64(maxBytes, 0x08);
            UsnId = BitConverter.ToUInt64(maxBytes, 0x10);
            LowestUsn = BitConverter.ToUInt64(maxBytes, 0x18);
        }

        #endregion Constructors

        #region StaticMethods

        #region GetMethod

        public static UsnJrnlDetail Get(string path)
        {
            FileRecord record = FileRecord.Get(path, true);
            return new UsnJrnlDetail(GetMaxStream(record).RawData);
        }

        #endregion GetMethod

        #region GetBytesMethod

        public static byte[] GetBytes(string path)
        {
            FileRecord record = FileRecord.Get(path, true);
            return GetMaxStream(record).RawData;
        }

        #endregion GetBytesMethod

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

        #endregion StaticMethods
    }

    #endregion UsnJrnlDetailClass
}
