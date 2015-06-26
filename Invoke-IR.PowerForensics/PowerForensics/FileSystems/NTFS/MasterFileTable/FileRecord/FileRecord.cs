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
            
            #endregion Signature

            // Check MFT Signature (FILE) to ensure bytes actually represent an MFT Record
            if (Signature == "FILE")
            {
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
        }

        public FileRecord(byte[] recordBytes, string volume, string filePath)
        {
            #region Signature

            // Get File Record Signature
            byte[] sigBytes = new byte[4];
            Array.Copy(recordBytes, 0, sigBytes, 0, sigBytes.Length);
            Signature = Encoding.ASCII.GetString(sigBytes);

            #endregion Signature

            // Check MFT Signature (FILE) to ensure bytes actually represent an MFT Record
            if (Signature == "FILE")
            {
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
        }

        #endregion Constructors

        // GetBytes Methods return a byte array representing a specific FileRecord
        #region GetBytesMethods

        public static byte[] GetRecordBytes(string volume, int index)
        {
            // Get handle for volume
            IntPtr hVolume = NativeMethods.getHandle(volume);

            // Get filestream based on hVolume
            using (FileStream streamToRead = NativeMethods.getFileStream(hVolume))
            {
                NTFS.VolumeBootRecord VBR = new NTFS.VolumeBootRecord(streamToRead);

                ulong mftStartOffset = VBR.MFTStartIndex * VBR.BytesPerCluster;

                ulong recordOffset = mftStartOffset + ((ulong)index * (ulong)VBR.BytesPerFileRecord);
                byte[] recordBytesRaw = NativeMethods.readDrive(streamToRead, recordOffset, (ulong)VBR.BytesPerFileRecord);

                ushort usoffset = BitConverter.ToUInt16(recordBytesRaw, 4); ;
                ushort ussize = BitConverter.ToUInt16(recordBytesRaw, 6);

                if (ussize != 0)
                {
                    byte[] usnBytes = new byte[2];
                    Array.Copy(recordBytesRaw, usoffset, usnBytes, 0, usnBytes.Length);
                    ushort UpdateSequenceNumber = BitConverter.ToUInt16(usnBytes, 0);

                    byte[] UpdateSequenceArray = new byte[(2 * ussize)];
                    Array.Copy(recordBytesRaw, (usoffset + 2), UpdateSequenceArray, 0, UpdateSequenceArray.Length);

                    recordBytesRaw[0x1FE] = UpdateSequenceArray[0];
                    recordBytesRaw[0x1FF] = UpdateSequenceArray[1];
                    recordBytesRaw[0x3FE] = UpdateSequenceArray[2];
                    recordBytesRaw[0x3FF] = UpdateSequenceArray[3];
                }

                return recordBytesRaw;
            }
        }

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

        #endregion GetBytesMethods 
    }

    #endregion FileRecordClass
}
