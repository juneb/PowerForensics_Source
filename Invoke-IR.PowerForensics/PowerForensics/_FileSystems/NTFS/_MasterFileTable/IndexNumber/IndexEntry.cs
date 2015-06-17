using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using InvokeIR.Win32;

namespace InvokeIR.PowerForensics.NTFS
{
    public class IndexEntry
    {

        #region Enums

        [FlagsAttribute]
        enum INDEX_ENTRY_FLAG
        {
            SUBNODE = 0x01,     // Index entry points to a sub-node
            LAST = 0x02         // Last index entry in the node, no Stream
        }

        #endregion Enums

        #region Structs

        struct INDEX_BLOCK
        {
            // Index Block Header
            internal uint Magic;                 // "INDX"
            internal ushort OffsetOfUS;          // Offset of Update Sequence
            internal ushort SizeOfUS;            // Size in words of Update Sequence Number & Array
            internal ulong LSN;                  // $LogFile Sequence Number
            internal ulong VCN;                  // VCN of this index block in the index allocation
            // Index Header
            internal uint EntryOffset;           // Offset of the index entries, relative to this address(0x18)
            internal uint TotalEntrySize;        // Total size of the index entries
            internal uint AllocEntrySize;        // Allocated size of index entries
            internal byte NotLeaf;               // 1 if not leaf node (has children)

            internal INDEX_BLOCK(byte[] bytes)
            {
                Magic = BitConverter.ToUInt32(bytes, 0);
                OffsetOfUS = BitConverter.ToUInt16(bytes, 4);
                SizeOfUS = BitConverter.ToUInt16(bytes, 6);
                LSN = BitConverter.ToUInt64(bytes, 8);
                VCN = BitConverter.ToUInt64(bytes, 16);
                EntryOffset = BitConverter.ToUInt32(bytes, 24);
                TotalEntrySize = BitConverter.ToUInt32(bytes, 28);
                AllocEntrySize = BitConverter.ToUInt32(bytes, 32);
                NotLeaf = bytes[36];
            }
        }
        
        internal struct INDEX_ENTRY
        {
            internal ulong FileReference;    // Low 6B: MFT record index, High 2B: MFT record sequence number
            internal ushort Size;            // Length of the index entry
            internal ushort StreamSize;      // Length of the stream
            internal byte Flags;             // Flags
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 1)]
            internal byte[] Stream;          // Stream
            // VCN of the sub node in Index Allocation, Offset = Size - 8

            internal INDEX_ENTRY(byte[] bytes)
            {
                FileReference = BitConverter.ToUInt64(bytes, 0);
                Size = BitConverter.ToUInt16(bytes, 8);
                StreamSize = BitConverter.ToUInt16(bytes, 10);
                Flags = bytes[12];
                byte[] streamBytes = new byte[StreamSize];
                Array.Copy(bytes, 16, streamBytes, 0, streamBytes.Length);
                Stream = streamBytes;
            }
        }

        #endregion Structs

        #region Properties

        public ulong FileIndex;
        public string Name;

        #endregion Properties

        #region Constructors

        internal IndexEntry(INDEX_ENTRY indxEntry, string name)
        {
            FileIndex = (indxEntry.FileReference & 0x0000FFFFFFFFFFFF);
            Name = name;
        }

        #endregion Constructors

        internal static IndexEntry[] Get(FileStream streamToRead, byte[] MFT, int index)
        {

            #region GetAttr
            VolumeBootRecord VBR = new VolumeBootRecord(streamToRead);

            ulong recordOffset = (ulong)(VBR.MFTStartIndex * VBR.BytesPerCluster) + (ulong)(VBR.BytesPerFileRecord * index);

            byte[] recordBytes = NativeMethods.readDrive(streamToRead, recordOffset, (ulong)VBR.BytesPerFileRecord);

            FileRecord fileRecord = new FileRecord(recordBytes);

            //MFTRecord fileRecord = MFTRecord.Get(MFT, index, null, null);

            IndexRoot indxRoot = null;
            NonResident INDX = null;

            foreach (Attr attr in fileRecord.Attribute)
            {
                if (attr.Name == "INDEX_ROOT")
                {
                    indxRoot = (IndexRoot)attr;
                }
                if (attr.Name == "INDEX_ALLOCATION")
                {
                    if (attr.NonResident)
                    {
                        INDX = (NonResident)attr;
                    }
                }
            }

            #endregion GetAttr

            List<IndexEntry> indxEntryList = new List<IndexEntry>();

            // Get entries from INDEX_ROOT Attribute (0x90)

            int indexrootOffset = 0;

            do{
                INDEX_ENTRY indxEntryStruct = new INDEX_ENTRY(indxRoot.EntryBytes.Skip(indexrootOffset).ToArray());

                if ((indxEntryStruct.Flags == 2) || (indxEntryStruct.Flags == 3))
                {
                    break;
                }

                indexrootOffset += indxEntryStruct.Size;

                FileName fN = new FileName(indxEntryStruct.Stream);
            
                IndexEntry indxEntry = new IndexEntry(indxEntryStruct, fN.Filename);
                
                indxEntryList.Add(indxEntry);

            } while(indexrootOffset < (indxRoot.EntryBytes.Length - 40));

            // Get entries from INDEX_ALLOCATION Attribute (0xA0)
            if (INDX != null)
            {
                byte[] nonResBytes = NonResident.GetContent(streamToRead, INDX);

                for (long offset = 0; offset < nonResBytes.Length; offset += 4096)
                {
                    byte[] indxBytes = new byte[4096];
                    Array.Copy(nonResBytes, offset, indxBytes, 0, indxBytes.Length);

                    INDEX_BLOCK indxBlock = new INDEX_BLOCK(indxBytes.Take(40).ToArray());

                    byte[] IndexAllocEntryBytes = new byte[indxBlock.TotalEntrySize];
                    Array.Copy(indxBytes, (indxBlock.EntryOffset + 24), IndexAllocEntryBytes, 0, IndexAllocEntryBytes.Length);

                    int indexallocOffset = 0;

                    do
                    {
                        if (IndexAllocEntryBytes.Length == 0)
                        {
                            break;
                        }

                        INDEX_ENTRY indxEntryStruct = new INDEX_ENTRY(IndexAllocEntryBytes.Skip(indexallocOffset).ToArray());
                        indexallocOffset += indxEntryStruct.Size;

                        if ((indxEntryStruct.Flags == 2) || (indxEntryStruct.Flags == 3))
                        {
                            break;
                        }

                        FileName fN = new FileName(indxEntryStruct.Stream);
                        IndexEntry indxEntry = new IndexEntry(indxEntryStruct, fN.Filename);
                        indxEntryList.Add(indxEntry);
                    } while (indexallocOffset < IndexAllocEntryBytes.Length);                
                }
            }
            return indxEntryList.ToArray();
        }

        public static IndexEntry[] Get(string volume, int index)
        {
            #region GetAttr
            IntPtr hVolume = NativeMethods.getHandle(volume);

            using (FileStream streamToRead = NativeMethods.getFileStream(hVolume))
            {
                VolumeBootRecord VBR = new VolumeBootRecord(streamToRead);

                ulong recordOffset = (ulong)(VBR.MFTStartIndex * VBR.BytesPerCluster) + (ulong)(VBR.BytesPerFileRecord * index);

                byte[] recordBytes = NativeMethods.readDrive(streamToRead, recordOffset, (ulong)VBR.BytesPerFileRecord);

                FileRecord fileRecord = new FileRecord(recordBytes);

                //MFTRecord fileRecord = MFTRecord.Get(MasterFileTable.GetBytes(volume), index, null, null);

                IndexRoot indxRoot = null;
                NonResident INDX = null;

                foreach (Attr attr in fileRecord.Attribute)
                {
                    if (attr.Name == "INDEX_ROOT")
                    {
                        indxRoot = (IndexRoot)attr;
                    }
                    if (attr.Name == "INDEX_ALLOCATION")
                    {
                        if (attr.NonResident)
                        {
                            INDX = (NonResident)attr;
                        }
                    }
                }

            #endregion GetAttr

                List<IndexEntry> indxEntryList = new List<IndexEntry>();

                // Get entries from INDEX_ROOT Attribute (0x90)

                int indexrootOffset = 0;

                do
                {
                    INDEX_ENTRY indxEntryStruct = new INDEX_ENTRY(indxRoot.EntryBytes.Skip(indexrootOffset).ToArray());

                    if ((indxEntryStruct.Flags == 2) || (indxEntryStruct.Flags == 3))
                    {
                        break;
                    }

                    indexrootOffset += indxEntryStruct.Size;

                    FileName fN = new FileName(indxEntryStruct.Stream);

                    IndexEntry indxEntry = new IndexEntry(indxEntryStruct, fN.Filename);

                    indxEntryList.Add(indxEntry);

                } while (indexrootOffset < (indxRoot.EntryBytes.Length - 40));

                // Get entries from INDEX_ALLOCATION Attribute (0xA0)
                if (INDX != null)
                {
                    byte[] nonResBytes = NonResident.GetContent(volume, INDX);

                    for (long offset = 0; offset < nonResBytes.Length; offset += 4096)
                    //for (long offset = 880640; offset < nonResBytes.Count; offset += 4096)
                    {
                        byte[] indxBytes = new byte[4096];
                        Array.Copy(nonResBytes, offset, indxBytes, 0, indxBytes.Length);

                        INDEX_BLOCK indxBlock = new INDEX_BLOCK(indxBytes.Take(40).ToArray());

                        byte[] IndexAllocEntryBytes = new byte[indxBlock.TotalEntrySize];
                        Array.Copy(indxBytes, (indxBlock.EntryOffset + 24), IndexAllocEntryBytes, 0, IndexAllocEntryBytes.Length);

                        int indexallocOffset = 0;

                        do
                        {
                            if (IndexAllocEntryBytes.Length == 0)
                            {
                                break;
                            }

                            INDEX_ENTRY indxEntryStruct = new INDEX_ENTRY(IndexAllocEntryBytes.Skip(indexallocOffset).ToArray());
                            indexallocOffset += indxEntryStruct.Size;

                            if ((indxEntryStruct.Flags == 2) || (indxEntryStruct.Flags == 3))
                            {
                                break;
                            }

                            FileName fN = new FileName(indxEntryStruct.Stream);
                            IndexEntry indxEntry = new IndexEntry(indxEntryStruct, fN.Filename);
                            indxEntryList.Add(indxEntry);
                        } while (indexallocOffset < IndexAllocEntryBytes.Length);
                    }
                }
                return indxEntryList.ToArray();
            }
        }
    }
}
