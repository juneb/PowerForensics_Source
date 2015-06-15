using System;
using System.Text;

//Reference https://www.sqlite.org/fileformat.html

namespace InvokeIR
{
    class DatabaseHeader
    {
        #region Enums

        private enum FILE_FORMAT
        {
            Legacy = 0x01,
            WAL = 0x02
        }
        
        private enum ENCODING
        {
            SQLITE_UTF8 = 0x01,
            SQLITE_UTF16LE = 0x02,
            SQLITE_UTF16BE = 0x03
        }
        
        #endregion Enums
        
        #region Properties
        
        public readonly string Signature;                 //The header string: "SQLite format 3" with nul terminator character at the end.
        public readonly ushort PageSize;                  //The database page size in bytes. Must be a power of two between 512 and 32768 inclusive, or the value 1 representing a page size of 65536
        public readonly byte FormatWriteVersion;          //File format write version. 1 for legacy; 2 for WAL
        public readonly byte FormatReadVersion;           //File format read version. 1 for legacy; 2 for WAL
        public readonly byte ReservedSpace;               //Bytes of unused "reserved" space at the end of each page. Usually 0.
        public readonly byte MaximumPayloadFraction;      //Maximum embedded payload fraction. Must be 64.
        public readonly byte MinimumPayloadFraction;      //Minimum embedded payload fraction. Must be 32.
        public readonly byte LeafPayloadFraction;         //Leaf payload fraction. Must be 32.
        public readonly uint FileChangeCounter;           //File change counter.
        public readonly uint SizeinPages;                 //Size of database file in pages. The "in-header database size".
        public readonly uint FirstFreelistPage;           //Page number of the first freelist trunk page.
        public readonly uint NumberofFreelistPages;       //Total number of freelist trunk pages
        public readonly uint SchemaCookie;                //The schema cookie
        public readonly uint SchemaFormatNumber;          //The schema format number. Supported schema formats are 1, 2, 3, and 4.
        public readonly uint PageCacheSize;               //Default page cache size.
        public readonly uint BTreePageNumber;             //The page number of the largest root b-tree page when in auto-vacuum or incremental-vacuum modes, or zero otherwise.
        public readonly uint TextEncoding;                //The database text encoding. A value of 1 means UTF-8. A value of 2 means UTF-16le. A value of 3 means UTF-16be.
        public readonly uint UserVersionNumber;           //The "user version" as read and set by the user_version pragme.
        public readonly uint IncrementalVacuumMode;       //True (non-zero) for incremental-vacuum mode. False (zero) otherwise.
        public readonly uint ApplicationID;               //The "Application ID" set by PRAGMA application id.
        public readonly uint VersionValidForNumber;       //The version-valid-for-number.
        public readonly uint SQLiteVersionNumber;         //SQLITE VERSION NUMBER

        #endregion Properties

        #region Constructors

        internal DatabaseHeader(byte[] bytes)
        {
            byte[] sigBytes = new byte[16];
            Array.Copy(bytes, 0, sigBytes, 0, sigBytes.Length);
            Signature = Encoding.ASCII.GetString(sigBytes);
            PageSize = BitConverter.ToUInt16(bytes, 16);
            FormatWriteVersion = bytes[18];
            FormatReadVersion = bytes[19];
            ReservedSpace = bytes[20];
            MaximumPayloadFraction = bytes[21];
            MinimumPayloadFraction = bytes[22];
            LeafPayloadFraction = bytes[23];
            FileChangeCounter = BitConverter.ToUInt32(bytes, 24);
            SizeinPages = BitConverter.ToUInt32(bytes, 28);
            FirstFreelistPage = BitConverter.ToUInt32(bytes, 32);
            NumberofFreelistPages = BitConverter.ToUInt32(bytes, 36);
            SchemaCookie = BitConverter.ToUInt32(bytes, 40);
            SchemaFormatNumber = BitConverter.ToUInt32(bytes, 44);
            PageCacheSize = BitConverter.ToUInt32(bytes, 48);
            BTreePageNumber = BitConverter.ToUInt32(bytes, 52);
            TextEncoding = BitConverter.ToUInt32(bytes, 56);
            UserVersionNumber = BitConverter.ToUInt32(bytes, 60);
            IncrementalVacuumMode = BitConverter.ToUInt32(bytes, 64);
            ApplicationID = BitConverter.ToUInt32(bytes, 68);
            VersionValidForNumber = BitConverter.ToUInt32(bytes, 92);
            SQLiteVersionNumber = BitConverter.ToUInt32(bytes, 96);
        }

        #endregion Constructors
    }
}
