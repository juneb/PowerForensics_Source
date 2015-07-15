using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using InvokeIR.Win32;

namespace InvokeIR.PowerForensics.NTFS
{
    #region FileRecordClass
    
    public class FileRecord
    {
        #region Enums

        enum FILE_RECORD_FLAG
        {
            INUSE = 0x01,	// File record is in use
            DIR = 0x02	    // File record is a directory
        }
        
        #endregion Enums

        #region Properties

        // Signature
        private readonly string Signature;              // "FILE" 

        // File Record Header
        private readonly ushort OffsetOfUS;             // Offset of Update Sequence
        private readonly  ushort SizeOfUS;		        // Size in words of Update Sequence Number & Array
        public readonly ushort UpdateSequenceNumber;
        public readonly byte[] UpdateSequenceArray;
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
        public readonly string Permission;

        // $FILE_NAME
        public readonly string FullName;
        public readonly string Name;
        public readonly ulong ParentRecordNumber;

        #endregion Properties

        #region Constructors
        
        public FileRecord(byte[] recordBytes, string volume)
        {
            #region Signature
            
            // Get File Record Signature
            byte[] sigBytes = new byte[4];
            Array.Copy(recordBytes, 0, sigBytes, 0, sigBytes.Length);
            Signature = Encoding.ASCII.GetString(sigBytes);
            if (Signature != "FILE")
            {
                throw new Exception("Invalid FileRecord Signature.");
            }

            #endregion Signature

            // Parse File Record Header
            OffsetOfUS = BitConverter.ToUInt16(recordBytes, 4);
            SizeOfUS = BitConverter.ToUInt16(recordBytes, 6);
            
            #region UpdateSequenceNumber
            byte[] usnBytes = new byte[2];
            Array.Copy(recordBytes, OffsetOfUS, usnBytes, 0, usnBytes.Length);
            UpdateSequenceNumber = BitConverter.ToUInt16(usnBytes, 0);
            #endregion UpdateSequenceNumber

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
                    if (attr.Name == "STANDARD_INFORMATION")
                    {
                        StandardInformation stdInfo = attr as StandardInformation;
                        ModifiedTime = stdInfo.ModifiedTime;
                        AccessedTime = stdInfo.AccessedTime;
                        ChangedTime = stdInfo.ChangedTime;
                        BornTime = stdInfo.BornTime;
                        Permission = stdInfo.Permission;
                    }
                    else if (attr.Name == "FILE_NAME")
                    {
                        FileName fN = attr as FileName;
                        if (!(fN.Namespace == 2))
                        {
                            Name = fN.Filename;
                            ParentRecordNumber = fN.ParentRecordNumber;
                        }
                    }

                    AttributeList.Add(attr);
                }
            } while (currentOffset < (attrArrayBytes.Length - 8));

            Attribute = AttributeList.ToArray();
            #endregion Attribute

            #region FullName

            StringBuilder sb = new StringBuilder();

            if (RecordNumber == 0)
            {
                sb.Append(volume.Split('\\')[3]);
                sb.Append('\\');
                sb.Append(Name);
                FullName = sb.ToString();
            }
            else if(RecordNumber == 5)
            {
                FullName = volume.Split('\\')[3];
            }
            else 
            {   
                sb.Append((new FileRecord(FileRecord.GetRecordBytes(volume, (int)ParentRecordNumber), volume)).FullName);
                sb.Append('\\');
                FullName = sb.Append(Name).ToString();
            }

            #endregion FullName
        }

        public FileRecord(ref FileRecord[] array, byte[] recordBytes, string volume)
        {
            #region Signature

            // Get File Record Signature
            byte[] sigBytes = new byte[4];
            Array.Copy(recordBytes, 0, sigBytes, 0, sigBytes.Length);
            Signature = Encoding.ASCII.GetString(sigBytes);
            if (Signature != "FILE")
            {
                throw new Exception("Invalid FileRecord Signature.");
            }

            #endregion Signature

            // Parse File Record Header
            OffsetOfUS = BitConverter.ToUInt16(recordBytes, 4);
            SizeOfUS = BitConverter.ToUInt16(recordBytes, 6);
            
            #region UpdateSequenceNumber
            byte[] usnBytes = new byte[2];
            Array.Copy(recordBytes, OffsetOfUS, usnBytes, 0, usnBytes.Length);
            UpdateSequenceNumber = BitConverter.ToUInt16(usnBytes, 0);
            #endregion UpdateSequenceNumber
            
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
                    if (attr.Name == "STANDARD_INFORMATION")
                    {
                        StandardInformation stdInfo = attr as StandardInformation;
                        ModifiedTime = stdInfo.ModifiedTime;
                        AccessedTime = stdInfo.AccessedTime;
                        ChangedTime = stdInfo.ChangedTime;
                        BornTime = stdInfo.BornTime;
                        Permission = stdInfo.Permission;
                    }
                    else if (attr.Name == "FILE_NAME")
                    {
                        FileName fN = attr as FileName;
                        if (!(fN.Namespace == 2))
                        {
                            Name = fN.Filename;
                            ParentRecordNumber = fN.ParentRecordNumber;
                        }
                    }

                    AttributeList.Add(attr);
                }
            } while (currentOffset < (attrArrayBytes.Length - 8));

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
                    sb.Append(array[(int)ParentRecordNumber].FullName);
                }
                // If record for Parent does not already exist then instantiate it and add it to the array
                else
                {
                    //Console.WriteLine("RecordIndex: {0}, ParentIndex: {1}", RecordNumber, ParentRecordNumber);
                    // This is where the recursive call should live...
                    //array[(int)ParentRecordNumber] = new FileRecord(ref array, FileRecord.GetRecordBytes(volume, (int)ParentRecordNumber), volume);
                    //sb.Append(array[(int)ParentRecordNumber].FullName);
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

        public FileRecord(byte[] recordBytes, string volume, string filePath)
        {
            #region Signature

            // Get File Record Signature
            byte[] sigBytes = new byte[4];
            Array.Copy(recordBytes, 0, sigBytes, 0, sigBytes.Length);
            Signature = Encoding.ASCII.GetString(sigBytes);
            if (Signature != "FILE")
            {
                throw new Exception("Invalid FileRecord Signature.");
            }

            #endregion Signature

            FullName = filePath;

            // Parse File Record Header
            OffsetOfUS = BitConverter.ToUInt16(recordBytes, 4);
            SizeOfUS = BitConverter.ToUInt16(recordBytes, 6);

            #region UpdateSequenceNumber
            byte[] usnBytes = new byte[2];
            Array.Copy(recordBytes, OffsetOfUS, usnBytes, 0, usnBytes.Length);
            UpdateSequenceNumber = BitConverter.ToUInt16(usnBytes, 0);
            #endregion UpdateSequenceNumber
            
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
                    if (attr.Name == "STANDARD_INFORMATION")
                    {
                        StandardInformation stdInfo = attr as StandardInformation;
                        ModifiedTime = stdInfo.ModifiedTime;
                        AccessedTime = stdInfo.AccessedTime;
                        ChangedTime = stdInfo.ChangedTime;
                        BornTime = stdInfo.BornTime;
                        Permission = stdInfo.Permission;
                    }
                    else if (attr.Name == "FILE_NAME")
                    {
                        FileName fN = attr as FileName;
                        if (!(fN.Namespace == 2))
                        {
                            Name = fN.Filename;
                            ParentRecordNumber = fN.ParentRecordNumber;
                        }
                    }

                    AttributeList.Add(attr);
                }
            } while (currentOffset < (attrArrayBytes.Length - 8));

            Attribute = AttributeList.ToArray();
            #endregion Attribute
        }

        #endregion Constructors

        #region Methods
        public static FileRecord[] GetInstances(string volume)
        {
            IntPtr hVolume = NativeMethods.getHandle(volume);

            using (FileStream streamToRead = NativeMethods.getFileStream(hVolume))
            {
                // Instantiate a null NonResident Object
                NonResident Data = null;

                // Get the FileRecord for the $MFT file
                //FileRecord mftRecord = new FileRecord(FileRecord.GetRecordBytes(volume, 0), volume);

                byte[] mftBytes = MasterFileTable.GetBytes(streamToRead, volume);

                // Determine the size of an MFT File Record
                int bytesPerFileRecord = (int)(new NTFS.VolumeBootRecord(streamToRead)).BytesPerFileRecord;

                // Calulate the number of entries in the MFT
                int fileCount = mftBytes.Length / bytesPerFileRecord;

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
                        Array.Copy(mftBytes, index * bytesPerFileRecord, recordBytes, 0, recordBytes.Length);

                        // Take UpdateSequence into account
                        ApplyFixup(ref recordBytes);

                        // Instantiate FileRecord object
                        recordArray[index] = new FileRecord(ref recordArray, recordBytes, volume);
                    }
                }
                return recordArray;
            }
        }

        internal static void ApplyFixup(ref byte[] bytes)
        {
            // Take UpdateSequence into account
            ushort usoffset = BitConverter.ToUInt16(bytes, 4);
            ushort ussize = BitConverter.ToUInt16(bytes, 6);

            if (ussize != 0)
            {
                byte[] usnBytes = new byte[2];
                Array.Copy(bytes, usoffset, usnBytes, 0, usnBytes.Length);
                ushort UpdateSequenceNumber = BitConverter.ToUInt16(usnBytes, 0);

                byte[] UpdateSequenceArray = new byte[(2 * ussize)];
                Array.Copy(bytes, (usoffset + 2), UpdateSequenceArray, 0, UpdateSequenceArray.Length);

                bytes[0x1FE] = UpdateSequenceArray[0];
                bytes[0x1FF] = UpdateSequenceArray[1];
                bytes[0x3FE] = UpdateSequenceArray[2];
                bytes[0x3FF] = UpdateSequenceArray[3];
            }
        }

        public static byte[] GetRecordBytes(string volume, int index)
        {
            // Get handle for volume
            IntPtr hVolume = NativeMethods.getHandle(volume);

            // Get filestream based on hVolume
            using (FileStream streamToRead = NativeMethods.getFileStream(hVolume))
            {
                // Get Volume Boot Record
                NTFS.VolumeBootRecord VBR = new NTFS.VolumeBootRecord(streamToRead);

                // Determine start of MFT
                ulong mftStartOffset = VBR.MFTStartIndex * VBR.BytesPerCluster;

                // Get FileRecord for $MFT
                FileRecord mftRecord = MasterFileTable.GetRecord(streamToRead, volume);

                // Get $MFT Data Attribute
                NonResident data = null;

                foreach(Attr attr in mftRecord.Attribute)
                {
                    if (attr.Name == "DATA")
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

        // GetBytes Methods return a byte array representing a specific FileRecord
        public byte[] GetBytes(string volume)
        {
            foreach (Attr attr in this.Attribute)
            {
                if (attr.Name == "DATA")
                {
                    if (attr.NonResident)
                    {
                        return (attr as NonResident).GetBytes(volume);
                    }
                    else
                    {
                        return (attr as Data).RawData;
                    }
                }
            }
            return null;
        }
        #endregion Methods 
    }

    #endregion FileRecordClass
}
