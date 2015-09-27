using System;
using System.Text;
using InvokeIR.Win32;

namespace InvokeIR.PowerForensics.Ext3
{
    #region DirectoryEntry

    public class DirectoryEntry
    {
        #region Enums

        public enum FILE_TYPE
        {
            UnknownFileType = 0x00,
            RegularFile = 0x01,
            DirectoryFile = 0x02,
            CharacterDevice = 0x03,
            BlockDevice = 0x04,
            BufferFile = 0x05,
            SocketFile = 0x06,
            SymbolicLink = 0x07
        }

        #endregion Enums

        #region Properties

        public readonly uint Inode;
        public readonly ushort RecordLength;
        public readonly byte NameLength;
        public readonly FILE_TYPE FileType;
        public readonly string Name;

        #endregion Properties

        #region Constructors

        internal DirectoryEntry(byte[] bytes)
        {
            Inode = BitConverter.ToUInt32(bytes, 0x00);
            RecordLength = BitConverter.ToUInt16(bytes, 0x04);
            NameLength = bytes[0x06];
            FileType = (FILE_TYPE)bytes[0x07];
            Name = Encoding.ASCII.GetString(bytes, 0x08, NameLength);
        }

        #endregion Constructors
    }
    
    #endregion DirectoryEntry
}
