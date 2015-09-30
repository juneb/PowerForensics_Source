using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using InvokeIR.Win32;

namespace PowerForensics.Ntfs
{
    #region FileRecordClass
    
    public class FileRecord
    {
        #region Enums

        private enum FILE_RECORD_FLAG
        {
            INUSE = 0x01,	// File record is in use
            DIR = 0x02	    // File record is a directory
        }
        
        #endregion Enums

        #region Properties

        public readonly string VolumePath;              // Path to Volume

        // Signature
        private readonly string Signature;              // "FILE" 

        // File Record Header
        private readonly ushort OffsetOfUS;             // Offset of Update Sequence
        private readonly  ushort SizeOfUS;		        // Size in words of Update Sequence Number & Array
        private readonly ushort UpdateSequenceNumber;
        private readonly byte[] UpdateSequenceArray;
        public readonly ulong LogFileSequenceNumber;    // $LogFile Sequence Number
        public readonly ushort SequenceNumber;          // Sequence number
        public readonly ushort Hardlinks;               // Hard link count
        private ushort OffsetOfAttribute;               // Offset of the first Attribute
        private readonly ushort Flags;                  // Flags
        public readonly bool Deleted;
        public readonly bool Directory;
        public readonly uint RealSize;                  // Real size of the FILE record
        public readonly uint AllocatedSize;             // Allocated size of the FILE record
        public readonly ulong ReferenceToBase;          // File reference to the base FILE record
        private readonly ushort NextAttrId;             // Next Attribute Id
        public readonly uint RecordNumber;              // Index number of this MFT Record

        // Attribute Array
        public readonly Attr[] Attribute;

        // $STANDARD_INFORMATION
        public readonly DateTime ModifiedTime;
        public readonly DateTime AccessedTime;
        public readonly DateTime ChangedTime;
        public readonly DateTime BornTime;
        public readonly StandardInformation.ATTR_STDINFO_PERMISSION Permission;

        // $FILE_NAME
        public readonly string FullName;
        public readonly string Name;
        public readonly ushort ParentSequenceNumber;
        public readonly ulong ParentRecordNumber;
        public readonly DateTime FNModifiedTime;
        public readonly DateTime FNAccessedTime;
        public readonly DateTime FNChangedTime;
        public readonly DateTime FNBornTime;

        #endregion Properties

        #region Constructors

        internal FileRecord(byte[] recordBytes, string volume, bool fast)
        {
            VolumePath = volume;

            Signature = Encoding.ASCII.GetString(recordBytes, 0x00, 0x04);

            if (Signature == "FILE")
            {
                // Parse File Record Header
                OffsetOfUS = BitConverter.ToUInt16(recordBytes, 4);
                SizeOfUS = BitConverter.ToUInt16(recordBytes, 6);
                UpdateSequenceNumber = BitConverter.ToUInt16(recordBytes, OffsetOfUS);
                #region UpdateSequenceArray

                UpdateSequenceArray = new byte[(2 * SizeOfUS) - 2];
                Array.Copy(recordBytes, (OffsetOfUS + 2), UpdateSequenceArray, 0, UpdateSequenceArray.Length);
                
                #endregion UpdateSequenceArray
                LogFileSequenceNumber = BitConverter.ToUInt64(recordBytes, 8);
                SequenceNumber = BitConverter.ToUInt16(recordBytes, 16);
                Hardlinks = BitConverter.ToUInt16(recordBytes, 18);
                OffsetOfAttribute = BitConverter.ToUInt16(recordBytes, 20);
                Flags = BitConverter.ToUInt16(recordBytes, 22);
                #region Deleted

                if ((Flags & (ushort)FILE_RECORD_FLAG.INUSE) == (ushort)FILE_RECORD_FLAG.INUSE)
                {
                    Deleted = false;
                }
                else
                {
                    Deleted = true;
                }

                #endregion Deleted
                #region Directory

                if ((Flags & (ushort)FILE_RECORD_FLAG.DIR) == (ushort)FILE_RECORD_FLAG.DIR)
                {
                    Directory = true;
                }
                else
                {
                    Directory = false;
                }

                #endregion Directory
                RealSize = BitConverter.ToUInt32(recordBytes, 24);
                AllocatedSize = BitConverter.ToUInt32(recordBytes, 28);
                ReferenceToBase = BitConverter.ToUInt64(recordBytes, 32);
                NextAttrId = BitConverter.ToUInt16(recordBytes, 40);
                RecordNumber = BitConverter.ToUInt32(recordBytes, 44);
                #region Attribute

                // Create a byte array representing the attribute array
                byte[] attrArrayBytes = new byte[RealSize - OffsetOfAttribute];
                Array.Copy(recordBytes, OffsetOfAttribute, attrArrayBytes, 0, attrArrayBytes.Length);

                // Instantiate an empty list of Attr Objects (We don't know how many attributes the record contains)
                List<Attr> AttributeList = new List<Attr>();

                // Initialize the offset value to 0
                int currentOffset = 0;

                if (currentOffset < (attrArrayBytes.Length - 8))
                {
                    do
                    {
                        // Get attribute size
                        int attrSizeOffset = currentOffset + 4;
                        int attrSize = BitConverter.ToInt32(attrArrayBytes, attrSizeOffset);

                        // Create new byte array with just current attribute's bytes
                        byte[] currentAttrBytes = new byte[attrSize];
                        Array.Copy(attrArrayBytes, currentOffset, currentAttrBytes, 0, currentAttrBytes.Length);

                        // Increment currentOffset
                        currentOffset += attrSize;

                        Attr attr = AttributeFactory.Get(currentAttrBytes, volume);

                        if (attr != null)
                        {
                            if (attr.Name == Attr.ATTR_TYPE.STANDARD_INFORMATION)
                            {
                                StandardInformation stdInfo = attr as StandardInformation;
                                ModifiedTime = stdInfo.ModifiedTime;
                                AccessedTime = stdInfo.AccessedTime;
                                ChangedTime = stdInfo.ChangedTime;
                                BornTime = stdInfo.BornTime;
                                Permission = stdInfo.Permission;
                            }
                            else if (attr.Name == Attr.ATTR_TYPE.FILE_NAME)
                            {
                                FileName fN = attr as FileName;
                                if (!(fN.Namespace == 2))
                                {
                                    Name = fN.Filename;
                                    ParentSequenceNumber = fN.ParentSequenceNumber;
                                    ParentRecordNumber = fN.ParentRecordNumber;
                                    FNModifiedTime = fN.ModifiedTime;
                                    FNAccessedTime = fN.AccessedTime;
                                    FNChangedTime = fN.ChangedTime;
                                    FNBornTime = fN.BornTime;
                                }
                            }

                            AttributeList.Add(attr);
                        }
                    } while (currentOffset < (attrArrayBytes.Length - 8));
                }

                Attribute = AttributeList.ToArray();

                #endregion Attribute
                #region FullName

                if (fast)
                {
                    FullName = Name;
                }
                else
                {
                    StringBuilder sb = new StringBuilder();

                    if (RecordNumber == 0)
                    {
                        sb.Append(volume.Split('\\')[3]);
                        sb.Append('\\');
                        sb.Append(Name);
                        FullName = sb.ToString();
                    }
                    else if (RecordNumber == 5)
                    {
                        FullName = volume.Split('\\')[3];
                    }
                    else
                    {
                        FileRecord parent = new FileRecord(FileRecord.GetRecordBytes(volume, (int)ParentRecordNumber), volume, false);
                        if (parent.SequenceNumber == this.ParentSequenceNumber)
                        {
                            sb.Append(parent.FullName);
                        }
                        else
                        {
                            sb.Append(@"$OrphanFiles");
                        }

                        if (Name != null)
                        {
                            sb.Append('\\');
                            FullName = sb.Append(Name).ToString();
                        }
                        else
                        {
                            FullName = sb.ToString();
                        }

                    }
                }

                #endregion FullName
            }
        }

        private FileRecord(ref FileRecord[] array, byte[] mftBytes, byte[] recordBytes, int bytesPerFileRecord, string volume)
        {
            VolumePath = volume;

            Signature = Encoding.ASCII.GetString(recordBytes, 0x00, 0x04);

            if (Signature == "FILE")
            {
                // Parse File Record Header
                OffsetOfUS = BitConverter.ToUInt16(recordBytes, 4);
                SizeOfUS = BitConverter.ToUInt16(recordBytes, 6);
                UpdateSequenceNumber = BitConverter.ToUInt16(recordBytes, OffsetOfUS);
                #region UpdateSequenceArray
                
                UpdateSequenceArray = new byte[(2 * SizeOfUS) - 2];
                Array.Copy(recordBytes, (OffsetOfUS + 2), UpdateSequenceArray, 0, UpdateSequenceArray.Length);
                
                #endregion UpdateSequenceArray
                LogFileSequenceNumber = BitConverter.ToUInt64(recordBytes, 8);
                SequenceNumber = BitConverter.ToUInt16(recordBytes, 16);
                Hardlinks = BitConverter.ToUInt16(recordBytes, 18);
                OffsetOfAttribute = BitConverter.ToUInt16(recordBytes, 20);
                Flags = BitConverter.ToUInt16(recordBytes, 22);
                #region Deleted
                if ((Flags & (ushort)FILE_RECORD_FLAG.INUSE) == (ushort)FILE_RECORD_FLAG.INUSE)
                {
                    Deleted = false;
                }
                else
                {
                    Deleted = true;
                }
                #endregion Deleted
                #region Directory
                if ((Flags & (ushort)FILE_RECORD_FLAG.DIR) == (ushort)FILE_RECORD_FLAG.DIR)
                {
                    Directory = true;
                }
                else
                {
                    Directory = false;
                }
                #endregion Directory
                RealSize = BitConverter.ToUInt32(recordBytes, 24);
                AllocatedSize = BitConverter.ToUInt32(recordBytes, 28);
                ReferenceToBase = BitConverter.ToUInt64(recordBytes, 32);
                NextAttrId = BitConverter.ToUInt16(recordBytes, 40);
                RecordNumber = BitConverter.ToUInt32(recordBytes, 44);
                #region Attribute

                // Create a byte array representing the attribute array
                byte[] attrArrayBytes = new byte[RealSize - OffsetOfAttribute];

                ApplyFixup(ref recordBytes);

                Array.Copy(recordBytes, OffsetOfAttribute, attrArrayBytes, 0, attrArrayBytes.Length);

                // Instantiate an empty list of Attr Objects (We don't know how many attributes the record contains)
                List<Attr> AttributeList = new List<Attr>();

                // Initialize the offset value to 0
                int currentOffset = 0;

                if (currentOffset < (attrArrayBytes.Length - 8))
                {
                    do
                    {
                        // Get attribute size
                        int attrSizeOffset = currentOffset + 4;
                        int attrSize = BitConverter.ToInt32(attrArrayBytes, attrSizeOffset);

                        // Create new byte array with just current attribute's bytes
                        byte[] currentAttrBytes = new byte[attrSize];
                        Array.Copy(attrArrayBytes, currentOffset, currentAttrBytes, 0, currentAttrBytes.Length);

                        // Increment currentOffset
                        currentOffset += attrSize;

                        Attr attr = AttributeFactory.Get(currentAttrBytes, volume);

                        if (attr != null)
                        {
                            if (attr.Name == Attr.ATTR_TYPE.STANDARD_INFORMATION)
                            {
                                StandardInformation stdInfo = attr as StandardInformation;
                                ModifiedTime = stdInfo.ModifiedTime;
                                AccessedTime = stdInfo.AccessedTime;
                                ChangedTime = stdInfo.ChangedTime;
                                BornTime = stdInfo.BornTime;
                                Permission = stdInfo.Permission;
                            }
                            else if (attr.Name == Attr.ATTR_TYPE.FILE_NAME)
                            {
                                FileName fN = attr as FileName;
                                if (!(fN.Namespace == 2))
                                {
                                    Name = fN.Filename;
                                    ParentSequenceNumber = fN.ParentSequenceNumber;
                                    ParentRecordNumber = fN.ParentRecordNumber;
                                    FNModifiedTime = fN.ModifiedTime;
                                    FNAccessedTime = fN.AccessedTime;
                                    FNChangedTime = fN.ChangedTime;
                                    FNBornTime = fN.BornTime;
                                }
                            }

                            AttributeList.Add(attr);
                        }
                    } while (currentOffset < (attrArrayBytes.Length - 8));
                }

                Attribute = AttributeList.ToArray();

                #endregion Attribute
                #region FullName

                StringBuilder sb = new StringBuilder();

                // Record 5 is the root of the drive
                if (RecordNumber == 5)
                {
                    sb.Append(volume.Split('\\')[3]);
                }
                else
                {
                    // Derive Path by looking at ParentRecord's FullName
                    if (array[(int)ParentRecordNumber] != null)
                    {
                        if (ParentSequenceNumber == array[(int)ParentRecordNumber].SequenceNumber)
                        {
                            sb.Append(array[(int)ParentRecordNumber].FullName);
                        }
                        else
                        {
                            sb.Append(@"$OrphanFiles\");
                        }
                    }

                    // If record for Parent does not already exist then instantiate it and add it to the array
                    else
                    {
                        byte[] parentBytes = NativeMethods.GetSubArray(mftBytes, (uint)bytesPerFileRecord * (uint)ParentRecordNumber, (uint)bytesPerFileRecord);
                        array[(int)ParentRecordNumber] = new FileRecord(ref array, mftBytes, parentBytes, bytesPerFileRecord, volume);

                        if (ParentSequenceNumber == array[(int)ParentRecordNumber].SequenceNumber)
                        {
                            sb.Append(array[(int)ParentRecordNumber].FullName);
                        }
                        else
                        {
                            sb.Append(@"$OrphanFiles\");
                        }
                    }

                    // Add file name to end of path
                    sb.Append(Name);
                }

                // Add trailing \ to any file that is a directory
                if (Directory)
                {
                    sb.Append('\\');
                }

                // Figure out a way to have record 15 not have a name of $MFT...

                FullName = sb.ToString();

                #endregion FullName
            }
        }

        #endregion Constructors

        #region StaticMethods

        #region GetMethod

        public static FileRecord Get(string path, bool fast)
        {
            string volume = NativeMethods.GetVolumeFromPath(path);
            IndexEntry entry = IndexEntry.Get(path);
            return new FileRecord(FileRecord.GetRecordBytes(volume, (int)entry.RecordNumber), volume, fast);
        }

        public static FileRecord Get(string volume, int index, bool fast)
        {
            return new FileRecord(FileRecord.GetRecordBytes(volume, index), volume, fast);
        }

        #endregion GetMethod

        #region GetInstancesMethod

        public static FileRecord[] GetInstances(string volume)
        {
            FileRecord record = new FileRecord(FileRecord.GetRecordBytes(volume, 0), volume, true);
            byte[] mftBytes = record.GetBytes();
            return GetInstances(mftBytes, volume);
        }

        public static FileRecord[] GetInstancesByPath(string path)
        {
            string volume = NativeMethods.GetVolumeFromPath(path);
            FileRecord record = new FileRecord(FileRecord.GetRecordBytes(path), volume, true);
            byte[] mftBytes = record.GetBytes();
            return GetInstances(mftBytes, volume);
        }

        private static FileRecord[] GetInstances(byte[] bytes, string volume)
        {
            // Determine the size of an MFT File Record
            int bytesPerFileRecord = (int)(VolumeBootRecord.Get(volume)).BytesPerFileRecord;

            // Calulate the number of entries in the MFT
            int fileCount = bytes.Length / bytesPerFileRecord;

            // Instantiate an array of FileRecord objects
            FileRecord[] recordArray = new FileRecord[fileCount];

            // Instantiate a byte array large enough to store the bytes belonging to a file record
            byte[] recordBytes = new byte[bytesPerFileRecord];

            // Now we need to iterate through all possible index values
            for (int index = 0; index < fileCount; index++)
            {
                // Check if current record has been instantiated
                if (recordArray[index] == null)
                {
                    // Copy filerecord bytes into the recordBytes byte[]
                    Array.Copy(bytes, index * bytesPerFileRecord, recordBytes, 0, recordBytes.Length);

                    // Take UpdateSequence into account
                    ApplyFixup(ref recordBytes);

                    // Instantiate FileRecord object
                    recordArray[index] = new FileRecord(ref recordArray, bytes, recordBytes, bytesPerFileRecord, volume);
                }
            }
            return recordArray;
        }

        #endregion GetInstancesMethod

        #region GetRecordBytesMethod

        public static byte[] GetRecordBytes(string path)
        {
            string volume = NativeMethods.GetVolumeFromPath(path);
            IndexEntry entry = IndexEntry.Get(path);
            return GetRecordBytesPrivate(volume, (int)entry.RecordNumber);
        }

        public static byte[] GetRecordBytes(string volume, int index)
        {
            return GetRecordBytesPrivate(volume, index);
        }

        private static byte[] GetRecordBytesPrivate(string volume, int index)
        {
            // Get handle for volume
            IntPtr hVolume = NativeMethods.getHandle(volume);

            // Get filestream based on hVolume
            using (FileStream streamToRead = NativeMethods.getFileStream(hVolume))
            {
                // Get Volume Boot Record
                VolumeBootRecord VBR = VolumeBootRecord.Get(streamToRead);

                // Determine start of MFT
                ulong mftStartOffset = VBR.MFTStartIndex * VBR.BytesPerCluster;

                // Get FileRecord for $MFT
                FileRecord mftRecord = MasterFileTable.GetRecord(streamToRead, volume);

                // Get $MFT Data Attribute
                NonResident data = null;

                foreach (Attr attr in mftRecord.Attribute)
                {
                    if (attr.Name == Attr.ATTR_TYPE.DATA)
                    {
                        data = attr as NonResident;
                    }
                }

                // Iterate through fragments of the MFT
                foreach (DataRun dr in data.DataRun)
                {
                    ulong DataRunRecords = ((ulong)dr.ClusterLength * (ulong)VBR.BytesPerCluster) / (ulong)VBR.BytesPerFileRecord;

                    // Check if index can be found in current DataRun
                    if (index < (int)DataRunRecords)
                    {
                        ulong recordOffset = ((ulong)dr.StartCluster * (ulong)VBR.BytesPerCluster) + ((ulong)index * (ulong)VBR.BytesPerFileRecord);
                        byte[] recordBytesRaw = NativeMethods.readDrive(streamToRead, recordOffset, (ulong)VBR.BytesPerFileRecord);

                        ApplyFixup(ref recordBytesRaw);

                        return recordBytesRaw;
                    }

                    // Decrement index for the number of FileRecords in the current DataRun
                    else
                    {
                        index -= ((int)dr.ClusterLength * (int)VBR.BytesPerCluster) / (int)VBR.BytesPerFileRecord;
                    }
                }
                throw new Exception("Could not find the FileRecord requested...");
            }
        }

        #endregion GetRecordBytesMethod

        private static void ApplyFixup(ref byte[] bytes)
        {
            // Take UpdateSequence into account
            ushort usoffset = BitConverter.ToUInt16(bytes, 4);
            ushort ussize = BitConverter.ToUInt16(bytes, 6);

            if (ussize != 0)
            {
                ushort UpdateSequenceNumber = BitConverter.ToUInt16(NativeMethods.GetSubArray(bytes, usoffset, 0x02), 0);
                byte[] UpdateSequenceArray = NativeMethods.GetSubArray(bytes, (uint)(usoffset + 2), (uint)(2 * ussize));

                bytes[0x1FE] = UpdateSequenceArray[0];
                bytes[0x1FF] = UpdateSequenceArray[1];
                bytes[0x3FE] = UpdateSequenceArray[2];
                bytes[0x3FF] = UpdateSequenceArray[3];
            }
        }

        #endregion StaticMethods

        #region InstanceMethods

        public byte[] GetBytes()
        {
            foreach (Attr attr in this.Attribute)
            {
                if (attr.Name == Attr.ATTR_TYPE.DATA)
                {
                    if (attr.NonResident)
                    {
                        return (attr as NonResident).GetBytes(this.VolumePath);
                    }
                    else
                    {
                        return (attr as Data).RawData;
                    }
                }
                else if (attr.Name == Attr.ATTR_TYPE.ATTRIBUTE_LIST)
                {
                    AttributeList attrlist = attr as AttributeList;
                    foreach (AttrRef ar in attrlist.AttributeReference)
                    {
                        if (ar.Name == "DATA")
                        {
                            FileRecord record = new FileRecord(FileRecord.GetRecordBytes(this.VolumePath, (int)ar.RecordNumber), this.VolumePath, true);
                            return record.GetBytes();
                        }
                    }
                }
            }
            throw new Exception("Could not locate file contents");
        }

        internal byte[] GetBytes(VolumeBootRecord VBR)
        {
            foreach (Attr attr in this.Attribute)
            {
                if (attr.Name == Attr.ATTR_TYPE.DATA)
                {
                    if (attr.NonResident)
                    {
                        return (attr as NonResident).GetBytes(this.VolumePath, VBR);
                    }
                    else
                    {
                        return (attr as Data).RawData;
                    }
                }
                else if (attr.Name == Attr.ATTR_TYPE.ATTRIBUTE_LIST)
                {
                    AttributeList attrlist = attr as AttributeList;
                    foreach (AttrRef ar in attrlist.AttributeReference)
                    {
                        if (ar.Name == "DATA")
                        {
                            FileRecord record = new FileRecord(FileRecord.GetRecordBytes(this.VolumePath, (int)ar.RecordNumber), this.VolumePath, true);
                            return record.GetBytes();
                        }
                    }
                }
            }
            throw new Exception("Could not locate file contents");
        }

        public FileRecord GetParent()
        {
            return FileRecord.Get(this.VolumePath, (int)this.ParentRecordNumber, false);
        }

        public UsnJrnl GetUsnJrnl()
        {
            foreach (Attr attr in this.Attribute)
            {
                if (attr.Name == Attr.ATTR_TYPE.STANDARD_INFORMATION)
                {
                    StandardInformation stdInfo = attr as StandardInformation;
                    return UsnJrnl.Get(this.VolumePath, stdInfo.UpdateSequenceNumber);
                }
            }
            throw new Exception("No $STANDARD_INFORMATION Attirbute found");
        }

        #endregion InstanceMethods
    }

    #endregion FileRecordClass
}