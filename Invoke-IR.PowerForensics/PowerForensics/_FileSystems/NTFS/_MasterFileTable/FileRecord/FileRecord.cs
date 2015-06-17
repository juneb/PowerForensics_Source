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

        #region Structs

        internal struct FILE_RECORD_HEADER
        {
            internal byte[] Magic;			// "FILE"
            internal ushort OffsetOfUS;		// Offset of Update Sequence
            internal ushort SizeOfUS;		// Size in words of Update Sequence Number & Array
            internal ulong LSN;			    // $LogFile Sequence Number
            internal ushort SeqNo;			// Sequence number
            internal ushort Hardlinks;		// Hard link count
            internal ushort OffsetOfAttr;	// Offset of the first Attribute
            internal ushort Flags;			// Flags
            internal uint RealSize;		    // Real size of the FILE record
            internal uint AllocSize;		// Allocated size of the FILE record
            internal ulong RefToBase;		// File reference to the base FILE record
            internal ushort NextAttrId;		// Next Attribute Id
            internal ushort Align;			// Align to 4 byte boundary
            internal uint RecordNo;		    // Number of this MFT Record

            internal FILE_RECORD_HEADER(byte[] bytes)
            {
                Magic = new byte[4];
                Array.Copy(bytes, 0, Magic, 0, Magic.Length);
                OffsetOfUS = BitConverter.ToUInt16(bytes, 4);
                SizeOfUS = BitConverter.ToUInt16(bytes, 6);
                LSN = BitConverter.ToUInt64(bytes, 8);
                SeqNo = BitConverter.ToUInt16(bytes, 16);
                Hardlinks = BitConverter.ToUInt16(bytes, 18);
                OffsetOfAttr = BitConverter.ToUInt16(bytes, 20);
                Flags = BitConverter.ToUInt16(bytes, 22);
                RealSize = BitConverter.ToUInt32(bytes, 24);
                AllocSize = BitConverter.ToUInt32(bytes, 28);
                RefToBase = BitConverter.ToUInt64(bytes, 32);
                NextAttrId = BitConverter.ToUInt16(bytes, 40);
                Align = BitConverter.ToUInt16(bytes, 42);
                RecordNo = BitConverter.ToUInt32(bytes, 44);
            }
        }

        #endregion Structs

        #region Properties

        public readonly string FullPath;
        public readonly string Name;
        public readonly ulong ParentIndex;
        public readonly uint RecordNumber;
        public readonly ulong Size;
        public readonly DateTime ModifiedTime;
        public readonly DateTime AccessedTime;
        public readonly DateTime ChangedTime;
        public readonly DateTime BornTime;
        public readonly string Permission;
        public readonly ushort SequenceNumber;
        public readonly ulong LogFileSequenceNumber;
        public readonly ushort Links;
        public readonly bool Deleted;
        public readonly bool Directory;
        public readonly Attr[] Attribute;
        public readonly UInt16 UpdateSequenceNumber;
        public readonly byte[] UpdateSequenceArray;

        #endregion Properties

        #region Constructors
        
        public FileRecord(byte[] recordBytes)
        {
            // Instantiate a FILE_RECORD_HEADER struct from raw MFT Record bytes
            FILE_RECORD_HEADER RecordHeader = new FILE_RECORD_HEADER(recordBytes);

            // Check MFT Signature (FILE) to ensure bytes actually represent an MFT Record
            if (Encoding.ASCII.GetString(RecordHeader.Magic) == "FILE")
            {
                RecordNumber = RecordHeader.RecordNo;
                Size = RecordHeader.RealSize;
                SequenceNumber = RecordHeader.SeqNo;
                LogFileSequenceNumber = RecordHeader.LSN;
                Links = RecordHeader.Hardlinks;

                // Unmask Header Flags
                #region HeaderFlags

                if ((RecordHeader.Flags & (ushort)FILE_RECORD_FLAG.INUSE) == (ushort)FILE_RECORD_FLAG.INUSE)
                {
                    Deleted = false;
                }
                else
                {
                    Deleted = true;
                }
                if ((RecordHeader.Flags & (ushort)FILE_RECORD_FLAG.DIR) == (ushort)FILE_RECORD_FLAG.DIR)
                {
                    Directory = true;
                }
                else
                {
                    Directory = false;
                }

                #endregion HeaderFlags

                List<Attr> AttributeList = new List<Attr>();
                int offsetToATTR = RecordHeader.OffsetOfAttr;

                while (offsetToATTR < (RecordHeader.RealSize - 8))
                {
                    int offset = offsetToATTR;
                    Attr attr = AttributeFactory.Get(recordBytes, offset, out offsetToATTR);

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
                                ParentIndex = fN.ParentIndex;
                            }

                        }
                        AttributeList.Add(attr);
                    }
                }

                Attribute = AttributeList.ToArray();

                byte[] usnBytes = new byte[2];
                Array.Copy(recordBytes, RecordHeader.OffsetOfUS, usnBytes, 0, usnBytes.Length);
                UpdateSequenceNumber = BitConverter.ToUInt16(usnBytes, 0);

                UpdateSequenceArray = new byte[(2 * RecordHeader.SizeOfUS) - 2];
                Array.Copy(recordBytes, (RecordHeader.OffsetOfUS + 2), UpdateSequenceArray, 0, UpdateSequenceArray.Length);

            }
        }

        public FileRecord(byte[] recordBytes, string path)
        {
            FullPath = path;

            // Instantiate a FILE_RECORD_HEADER struct from raw MFT Record bytes
            FILE_RECORD_HEADER RecordHeader = new FILE_RECORD_HEADER(recordBytes);

            // Check MFT Signature (FILE) to ensure bytes actually represent an MFT Record
            if (Encoding.ASCII.GetString(RecordHeader.Magic) == "FILE")
            {
                RecordNumber = RecordHeader.RecordNo;
                Size = RecordHeader.RealSize;
                SequenceNumber = RecordHeader.SeqNo;
                LogFileSequenceNumber = RecordHeader.LSN;
                Links = RecordHeader.Hardlinks;

                // Unmask Header Flags
                #region HeaderFlags

                if ((RecordHeader.Flags & (ushort)FILE_RECORD_FLAG.INUSE) == (ushort)FILE_RECORD_FLAG.INUSE)
                {
                    Deleted = false;
                }
                else
                {
                    Deleted = true;
                }
                if ((RecordHeader.Flags & (ushort)FILE_RECORD_FLAG.DIR) == (ushort)FILE_RECORD_FLAG.DIR)
                {
                    Directory = true;
                }
                else
                {
                    Directory = false;
                }

                #endregion HeaderFlags

                List<Attr> AttributeList = new List<Attr>();
                int offsetToATTR = RecordHeader.OffsetOfAttr;

                while (offsetToATTR < (RecordHeader.RealSize - 8))
                {
                    int offset = offsetToATTR;
                    Attr attr = AttributeFactory.Get(recordBytes, offset, out offsetToATTR);

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
                                ParentIndex = fN.ParentIndex;
                            }

                        }
                        AttributeList.Add(attr);
                    }
                }

                Attribute = AttributeList.ToArray();

                byte[] usnBytes = new byte[2];
                Array.Copy(recordBytes, RecordHeader.OffsetOfUS, usnBytes, 0, usnBytes.Length);
                UpdateSequenceNumber = BitConverter.ToUInt16(usnBytes, 0);

                UpdateSequenceArray = new byte[(2 * RecordHeader.SizeOfUS) - 2];
                Array.Copy(recordBytes, (RecordHeader.OffsetOfUS + 2), UpdateSequenceArray, 0, UpdateSequenceArray.Length);
            }
        }

        internal FileRecord(byte[] mftBytes, int index, ref FileRecord[] recordArray, string volLetter)
        {
            // Get byte array representing current record
            byte[] recordBytes = GetBytes(mftBytes, index);

            // Instantiate a FILE_RECORD_HEADER struct from raw MFT Record bytes
            FILE_RECORD_HEADER RecordHeader = new FILE_RECORD_HEADER(recordBytes);

            // Check MFT Signature (FILE) to ensure bytes actually represent an MFT Record
            if (Encoding.ASCII.GetString(RecordHeader.Magic) == "FILE")
            {
                RecordNumber = RecordHeader.RecordNo;
                Size = RecordHeader.RealSize;
                SequenceNumber = RecordHeader.SeqNo;
                LogFileSequenceNumber = RecordHeader.LSN;
                Links = RecordHeader.Hardlinks;

                // Unmask Header Flags
                #region HeaderFlags

                if ((RecordHeader.Flags & (ushort)FILE_RECORD_FLAG.INUSE) == (ushort)FILE_RECORD_FLAG.INUSE)
                {
                    Deleted = false;
                }
                else
                {
                    Deleted = true;
                }
                if ((RecordHeader.Flags & (ushort)FILE_RECORD_FLAG.DIR) == (ushort)FILE_RECORD_FLAG.DIR)
                {
                    Directory = true;
                }
                else
                {
                    Directory = false;
                }

                #endregion HeaderFlags

                List<Attr> AttributeList = new List<Attr>();
                int offsetToATTR = RecordHeader.OffsetOfAttr;

                while (offsetToATTR < (RecordHeader.RealSize - 8))
                {
                    //sw.Start();
                    int offset = offsetToATTR;
                    Attr attr = AttributeFactory.Get(recordBytes, offset, out offsetToATTR);
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
                            if(!(fN.Filename.Contains("~")))
                            {
                                Name = fN.Filename;
                                ParentIndex = fN.ParentIndex;
                            }
                        }
                        AttributeList.Add(attr);
                    }
                }
                // Check if MFT Record is for the root directory (should be Record Index 5)
                // If index and ParentIndex are not the same then get FullPath
                if((ulong)index != ParentIndex)
                {
                    // Check if ParentIndex Record has already been constructed and added to array
                    if (recordArray[ParentIndex] == null)
                    {
                        recordArray[ParentIndex] = new FileRecord(mftBytes, (int)ParentIndex, ref recordArray, volLetter);   
                    }
                    // FullPath equals the ParentIndex FullPath + the current Index Name
                    // Make more efficient with String Builder
                    FullPath = recordArray[ParentIndex].FullPath + Name;
                    if(Directory)
                    {
                        FullPath += "\\";
                    }
                }
                else
                {
                    FullPath = volLetter;
                }
                Attribute = AttributeList.ToArray();
            }
            else
            {

            }
        }

        #endregion Constructors

        // GetBytes Methods return a byte array representing a specific FileRecord
        #region GetBytesMethods

        public static byte[] GetBytes(string volume, int index)
        {
            // Get handle for volume
            IntPtr hVolume = NativeMethods.getHandle(volume);

            // Get filestream based on hVolume
            using (FileStream streamToRead = NativeMethods.getFileStream(hVolume))
            {
                VolumeBootRecord VBR = new VolumeBootRecord(streamToRead);

                ulong mftStartOffset = VBR.MFTStartIndex * VBR.BytesPerCluster;

                ulong recordOffset = mftStartOffset + ((ulong)index * 1024);
                byte[] recordBytesRaw = NativeMethods.readDrive(streamToRead, recordOffset, 1024);

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

        internal static byte[] GetBytes(byte[] mftBytes, int index)
        {
            // Determine byte offset of MFT Record
            int recordOffset = index * 1024;

            // Create a byte array the size of an MFT Record (1024 bytes)
            byte[] mftRecordBytes = new byte[1024];

            // Create a subarray representing the MFT Record from the MFT byte array
            Array.Copy(mftBytes, recordOffset, mftRecordBytes, 0, 1024);

            ushort usoffset = BitConverter.ToUInt16(mftRecordBytes, 4);; 
            ushort ussize = BitConverter.ToUInt16(mftRecordBytes, 6);

            if (ussize != 0)
            {
                byte[] usnBytes = new byte[2];
                Array.Copy(mftRecordBytes, usoffset, usnBytes, 0, usnBytes.Length);
                ushort UpdateSequenceNumber = BitConverter.ToUInt16(usnBytes, 0);

                byte[] UpdateSequenceArray = new byte[(2 * ussize)];
                Array.Copy(mftRecordBytes, (usoffset + 2), UpdateSequenceArray, 0, UpdateSequenceArray.Length);

                mftRecordBytes[0x1FE] = UpdateSequenceArray[0];
                mftRecordBytes[0x1FF] = UpdateSequenceArray[1];
                mftRecordBytes[0x3FE] = UpdateSequenceArray[2];
                mftRecordBytes[0x3FF] = UpdateSequenceArray[3];
            }

            // Return the MFT Record byte array
            return mftRecordBytes;
        }

        #endregion GetBytesMethods 

        // GetInstances Methods are used to get all FileRecords for a given Volume
        #region GetInstancesMethods

        // Get all MFT Records from the MFT byte array
        internal static FileRecord[] GetInstances(byte[] mftBytes, string volLetter)
        {
            // Determine number of MFT Records (each record is 1024 bytes)
            // Create an array large enough to hold each MFT Record
            int recordCount = mftBytes.Length / 1024;
            FileRecord[] recordArray = new FileRecord[recordCount];

            // Iterate through each index number and add MFTRecord to MFTRecord[]
            for (int i = 0; i < mftBytes.Length; i += 1024)
            {
                int index = i / 1024;
                if (recordArray[index] == null)
                {
                    recordArray[index] = new FileRecord(mftBytes, index, ref recordArray, volLetter);
                }
            }

            // Return MFTRecord[]
            return recordArray;
        }


        #endregion GetInstancesMethods
    }

    #endregion FileRecordClass
}
