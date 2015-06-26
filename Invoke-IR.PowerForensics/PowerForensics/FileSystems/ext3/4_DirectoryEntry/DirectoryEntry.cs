using System;
using System.Text;

namespace InvokeIR.PowerForensics.ext3
{
    public class DirectoryEntry
    {
        #region Enums

        internal enum FILE_TYPE
        {
            UnknownFileType = 0,
            RegularFile = 1,
            DirectoryFile = 2,
            CharacterDevice = 3,
            BlockDevice = 4,
            BufferFile = 5,
            SocketFile = 6,
            SymbolicLink = 7
        }

        #endregion Enums

        #region Properties

        public readonly uint Inode;
        public readonly ushort RecordLength;
        public readonly byte NameLength;
        public readonly string FileType;
        public readonly string Name;

        #endregion Properties

        #region Constructors

        internal DirectoryEntry(byte[] bytes)
        {
            Inode = BitConverter.ToUInt32(bytes, 0x00);
            RecordLength = BitConverter.ToUInt16(bytes, 0x04);
            NameLength = bytes[0x06];
            FileType = Enum.GetName(typeof(FILE_TYPE), bytes[0x07]).ToString();

            #region Name
            byte[] nameBytes = new byte[NameLength];
            Array.Copy(bytes, 0x08, nameBytes, 0, nameBytes.Length);
            Name = Encoding.ASCII.GetString(nameBytes);
            #endregion Name
        }

        #endregion Constructors
    }
}
