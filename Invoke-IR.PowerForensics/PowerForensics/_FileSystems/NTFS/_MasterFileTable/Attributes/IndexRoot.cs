using System;
using System.Linq;

namespace InvokeIR.PowerForensics.NTFS
{
    #region IndexRootClass

    public class IndexRoot : Attr
    {

        #region Enums

        [FlagsAttribute]
        enum INDEX_ROOT_FLAGS
        {
            INDEX_ALLOCATION = 0x01
        }

        #endregion Enums

        #region Properties

        public readonly string AttributeType;
        public readonly uint CollationSortingRule;
        public readonly uint IndexSize;
        public readonly uint StartOffset;
        public readonly uint EndOffset;
        public readonly string Flags;
        public readonly uint IndexSizeinBytes;
        public readonly byte[] EntryBytes;

        #endregion Properties

        #region Constructors

        internal IndexRoot(byte[] attrBytes, string attrName)
        {
            AttrHeader.ATTR_HEADER_RESIDENT header = new AttrHeader.ATTR_HEADER_RESIDENT(attrBytes.Take(24).ToArray());
            Name = Enum.GetName(typeof(ATTR_TYPE), header.commonHeader.ATTRType);
            NameString = attrName;
            NonResident = header.commonHeader.NonResident;
            AttributeId = header.commonHeader.Id;

            AttributeType = Enum.GetName(typeof(ATTR_TYPE), BitConverter.ToUInt32(attrBytes, 32));
            CollationSortingRule = BitConverter.ToUInt32(attrBytes, 36);
            IndexSize = BitConverter.ToUInt32(attrBytes, 40);
            StartOffset = BitConverter.ToUInt32(attrBytes, 48);
            EndOffset = BitConverter.ToUInt32(attrBytes, 52);
            Flags = ((INDEX_ROOT_FLAGS)BitConverter.ToUInt32(attrBytes, 60)).ToString();
            EntryBytes = attrBytes.Skip((int)StartOffset + 48).Take((int)EndOffset).ToArray();
        }

        #endregion Constuctors

    }

    #endregion IndexRootClass
}
