using System;

namespace InvokeIR.PowerForensics.NTFS
{
    public class Attr
    {
        internal enum ATTR_TYPE
        {
            STANDARD_INFORMATION = 0x10,
            ATTRIBUTE_LIST = 0x20,
            FILE_NAME = 0x30,
            OBJECT_ID = 0x40,
            SECURITY_DESCRIPTOR = 0x50,
            VOLUME_NAME = 0x60,
            VOLUME_INFORMATION = 0x70,
            DATA = 0x80,
            INDEX_ROOT = 0x90,
            INDEX_ALLOCATION = 0xA0,
            BITMAP = 0xB0,
            REPARSE_POINT = 0xC0,
            EA_INFORMATION = 0xD0,
            EA = 0xE0,
            LOGGED_UTILITY_STREAM = 0x100,

            ATTR_FLAG_COMPRESSED = 0x0001,
            ATTR_FLAG_ENCRYPTED = 0x4000,
            ATTR_FLAG_SPARSE = 0x8000
        }

        public string Name;
        public string NameString;
        public bool NonResident;
        public ushort AttributeId;

        public static byte[] GetBytes(byte[] recordBytes, uint attribute)
        {

            // Instantiate a FILE_RECORD_HEADER struct from raw MFT Record bytes
            FileRecord.FILE_RECORD_HEADER RecordHeader = new FileRecord.FILE_RECORD_HEADER(recordBytes);

            int offsetToATTR = RecordHeader.OffsetOfAttr;

            while (offsetToATTR < (RecordHeader.RealSize - 8))
            { 
                byte[] commonHeaderBytes = new byte[16];
                Array.Copy(recordBytes, offsetToATTR, commonHeaderBytes, 0, commonHeaderBytes.Length);
                AttrHeader.ATTR_HEADER_COMMON commonAttributeHeader = new AttrHeader.ATTR_HEADER_COMMON(commonHeaderBytes);
                
                if (commonAttributeHeader.ATTRType == attribute)
                {
                    byte[] attrBytes = new byte[commonAttributeHeader.TotalSize];
                    Array.Copy(recordBytes, offsetToATTR, attrBytes, 0, attrBytes.Length);
                    // Return bytes for Attr
                    return attrBytes;;    
                }

                else
                {
                    // Change offsetToATTR to next Attr
                    offsetToATTR += (int)commonAttributeHeader.TotalSize;
                }
            }
            
            // Add some sort of exception handling here...
            return null;
        }

        public static Attr Get(byte[] recordBytes, uint attribute)
        {
            byte[] attrBytes = Attr.GetBytes(recordBytes, attribute);
            int offsetToAttr = 0;
            return AttributeFactory.Get(attrBytes, 0, out offsetToAttr);   
        }
    }

}
