using System;
using System.Linq;
using System.Collections.Generic;

namespace InvokeIR.PowerForensics.NTFS
{
    #region IndexRootClass

    public class IndexRoot : Attr
    {
        // ATTR_HEADER_RESIDENT
        // IndexRoot
        // IndexHeader
        // IndexEntry[]

        #region Enums

        [FlagsAttribute]
        enum INDEX_ROOT_FLAGS
        {
            INDEX_ROOT_ONLY = 0x00,
            INDEX_ALLOCATION = 0x01
        }

        #endregion Enums

        #region Properties

        // Index Root
        public readonly string AttributeType;
        public readonly uint CollationSortingRule;
        public readonly uint IndexSize;
        public readonly byte ClustersPerIndexRecord;

        // IndexHeader
        private readonly uint StartOffset;
        private readonly uint TotalSize;
        private readonly uint AllocatedSize;
        public readonly string Flags;
        
        // IndexEntry[]
        private readonly byte[] EntryBytes;
        public readonly IndexEntry[] Entries;

        #endregion Properties

        #region Constructors
        internal IndexRoot(ResidentHeader header, byte[] attrBytes, string attrName)
        {
            #region ResidentHeader

            // Get ResidentHeader (includes Common Header)
            Name = Enum.GetName(typeof(ATTR_TYPE), header.commonHeader.ATTRType);
            NameString = attrName;
            NonResident = header.commonHeader.NonResident;
            AttributeId = header.commonHeader.Id;
            
            #endregion ResidentHeader

            #region IndexRoot

            // IndexRoot
            AttributeType = Enum.GetName(typeof(ATTR_TYPE), BitConverter.ToUInt32(attrBytes, 0x00));
            CollationSortingRule = BitConverter.ToUInt32(attrBytes, 0x04);
            IndexSize = BitConverter.ToUInt32(attrBytes, 0x08);
            ClustersPerIndexRecord = attrBytes[0x0C];

            #endregion IndexRoot

            #region IndexHeader

            // IndexHeader
            StartOffset = (BitConverter.ToUInt32(attrBytes, 0x10) + 0x10);  // Add 0x10 bytes to start offset to account for its offset
            TotalSize = BitConverter.ToUInt32(attrBytes, 0x14);
            AllocatedSize = BitConverter.ToUInt32(attrBytes, 0x18);
            Flags = ((INDEX_ROOT_FLAGS)BitConverter.ToUInt32(attrBytes, 0x1C)).ToString();
            
            #endregion IndexHeader

            #region IndexEntryArray

            if(TotalSize > StartOffset){
                // IndexEntry[]
                byte[] EntryBytes = new byte[TotalSize - StartOffset];
                try
                {
                    Array.Copy(attrBytes, StartOffset, EntryBytes, 0, EntryBytes.Length);
                }
                catch
                {   
                    throw new Exception("Error copying EntryBytes");
                }

                // Iterate through IndexEntry object
                int indexEntryOffset = 0;

                if (AttributeType == "FILE_NAME")
                {
                    // Instantiate empty IndexEntry List
                    List<IndexEntry> entryList = new List<IndexEntry>();

                    while (indexEntryOffset < (EntryBytes.Length - 0x10))
                    {                
                        // Creat byte array representing IndexEntry Object
                        int indexEntrySizeOffset = indexEntryOffset + 0x08;

                        ushort indexEntrySize = BitConverter.ToUInt16(EntryBytes, indexEntrySizeOffset);
                        byte[] indexEntryBytes = new byte[indexEntrySize];
                        Array.Copy(EntryBytes, indexEntryOffset, indexEntryBytes, 0, indexEntryBytes.Length);
                
                        // Instantiate an IndexEntry Object
                        IndexEntry indexEntry = new IndexEntry(indexEntryBytes);

                        // Add IndexEntry Object to FileName List
                        entryList.Add(indexEntry);

                        // Increment indexEntryOffset
                        indexEntryOffset += indexEntry.Size;
                    } 
                
                    Entries = entryList.ToArray();
                }
            }
            #endregion IndexEntryArray
        }
        #endregion Constuctors
    }
    #endregion IndexRootClass
}
