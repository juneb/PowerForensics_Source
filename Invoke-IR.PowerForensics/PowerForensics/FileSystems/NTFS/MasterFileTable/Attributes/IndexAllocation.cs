using System;
using System.Text;
using System.Collections.Generic;

namespace InvokeIR.PowerForensics.NTFS
{
    public class IndexAllocation : Attr
    {
        #region Properties

        public readonly IndexEntry[] Entries;

        #endregion Properties

        #region Constructors

        internal IndexAllocation(NonResident header, string volume)
        {
            // Headers
            Name = Enum.GetName(typeof(ATTR_TYPE), header.commonHeader.ATTRType);
            NameString = header.NameString;
            NonResident = header.commonHeader.NonResident;
            AttributeId = header.commonHeader.Id;

            // Get IndexAllocation Bytes
            byte[] bytes = header.GetBytes(volume);

            // Instantiate empty IndexEntry List
            List<IndexEntry> indexEntryList = new List<IndexEntry>();

            // Iterate through IndexBlocks (4096 bytes in size)
            for (int offset = 0; offset < bytes.Length; offset += 4096)
            {
                // Detemine size of Update Sequence
                ushort usOffset = BitConverter.ToUInt16(bytes, offset + 0x04);
                ushort usSize = BitConverter.ToUInt16(bytes, offset + 0x06);
                int indexBlockSize = usOffset + (usSize * 2);

                if (indexBlockSize == 0)
                {
                    break;
                }

                // Instantiate IndexBlock Object (Header)
                byte[] indexBlockBytes = new byte[indexBlockSize];
                Array.Copy(bytes, offset, indexBlockBytes, 0, indexBlockBytes.Length);
                IndexBlock indexBlock = new IndexBlock(indexBlockBytes);

                // Create byte array for IndexEntry object
                // 0x18 represents the offset of the EntryOffset value, so it must be added on
                byte[] indexEntryBytes = new byte[indexBlock.TotalEntrySize];
                Array.Copy(bytes, offset + indexBlock.EntryOffset + 0x18, indexEntryBytes, 0, indexEntryBytes.Length);
                
                int entryOffset = 0;

                do
                {
                    // Create byte array representing current entry using the IndexEntry.Size value
                    byte[] entryBytes = new byte[BitConverter.ToUInt16(indexEntryBytes, entryOffset + 0x08)];
                    Array.Copy(indexEntryBytes, entryOffset, entryBytes, 0, entryBytes.Length);

                    // Instantiate an IndexEntry Object
                    IndexEntry indexEntry = new IndexEntry(entryBytes);
                    entryOffset += indexEntry.Size;

                    // Check if entry is the last in the Entry array
                    if (indexEntry.Flags == 0x02 || indexEntry.Flags == 0x03)
                    {
                        break;
                    }

                    // Add IndexEntry Object to list
                    indexEntryList.Add(indexEntry);
                
                }while(entryOffset < indexEntryBytes.Length);
            }
            Entries = indexEntryList.ToArray();
        }

        #endregion Constructors
    }
    
    internal class IndexBlock
    {
        #region Properties

        // Index Block Header
        internal readonly string Signature;         // "INDX"
        internal ushort OffsetOfUS;                 // Offset of Update Sequence
        internal ushort SizeOfUS;                   // Size in words of Update Sequence Number & Array
        internal readonly ushort UpdateSequenceNumber;
        internal readonly byte[] UpdateSequenceArray;
        internal readonly ulong LSN;                // $LogFile Sequence Number
        internal readonly ulong VCN;                // VCN of this index block in the index allocation

        // Index Header
        internal readonly uint EntryOffset;         // Offset of the index entries, relative to this address(0x18)
        internal readonly uint TotalEntrySize;      // Total size of the index entries
        internal readonly uint AllocEntrySize;      // Allocated size of index entries
        internal readonly byte NotLeaf;             // 1 if not leaf node (has children)
    
        #endregion Properties

        #region Constructors

        internal IndexBlock(byte[] bytes)
        {
            #region Signature
            byte[] sigBytes = new byte[0x04];
            Array.Copy(bytes, 0, sigBytes, 0, sigBytes.Length);
            Signature = Encoding.ASCII.GetString(sigBytes);
            #endregion Signature
            OffsetOfUS = BitConverter.ToUInt16(bytes, 0x04);
            SizeOfUS = BitConverter.ToUInt16(bytes, 0x06);
            #region UpdateSequenceNumber
            byte[] usnBytes = new byte[2];
            Array.Copy(bytes, OffsetOfUS, usnBytes, 0, usnBytes.Length);
            UpdateSequenceNumber = BitConverter.ToUInt16(usnBytes, 0);
            #endregion UpdateSequenceNumber
            #region UpdateSequenceArray
            UpdateSequenceArray = new byte[(2 * SizeOfUS) - 2];
            Array.Copy(bytes, (OffsetOfUS + 2), UpdateSequenceArray, 0, UpdateSequenceArray.Length);
            #endregion UpdateSequenceArray
            LSN = BitConverter.ToUInt64(bytes, 0x08);
            VCN = BitConverter.ToUInt64(bytes, 0x10);

            // Index Header
            EntryOffset = BitConverter.ToUInt32(bytes, 0x18);
            TotalEntrySize = BitConverter.ToUInt32(bytes, 0x1C);
            AllocEntrySize = BitConverter.ToUInt32(bytes, 0x20);
            NotLeaf = bytes[0x24];
        }

        #endregion Constructors
    }
}
