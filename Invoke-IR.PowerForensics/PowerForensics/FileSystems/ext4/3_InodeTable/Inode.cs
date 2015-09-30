using System;
using System.IO;
using System.Text;
using InvokeIR.Win32;

namespace PowerForensics.Ext3
{
    public class Inode
    {
        #region Constants

        public const uint EXT4_INODE_SIZE = 0x100;

        #endregion Constants

        #region Enums

        [FlagsAttribute]
        public enum I_MODE
        {
            // Access Rights
            EXT2_S_IXOTH = 0x0001, //Others Execute
            EXT2_S_IWOTH = 0x0002, //Others Write
            EXT2_S_IROTH = 0x0004, //Others Read
            EXT2_S_IXGRP = 0x0008, //Group Execute
            EXT2_S_IWGRP = 0x0010, //Group Write
            EXT2_S_IRGRP = 0x0020, //Group Read
            EXT2_S_IXUSR = 0x0040, //User Execute
            EXT2_S_IWUSR = 0x0080, //User Write
            EXT2_S_IRUSR = 0x0100, //User Read

            // Process execution user/group override
            EXT2_S_ISVTX = 0x0200, //Sticky Bit
            EXT2_S_ISGID = 0x0400, //Set Group Id
            EXT2_S_ISUID = 0x0800, //Set User Id

            // File format
            EXT2_S_IFIFO = 0x1000, //fifo
            EXT2_S_IFCHR = 0x2000, //Character Device
            EXT2_S_IFDIR = 0x4000, //Directory
            EXT2_S_IFBLK = 0x6000, //Block Device
            EXT2_S_IFREG = 0x8000, //Regular File
            EXT2_S_IFLNK = 0xA000, //Symbolic Link
            EXT2_S_IFSOCK = 0xC000, //Socket
        }

        [FlagsAttribute]
        public enum I_FLAGS : uint
        {
            EXT4_SECRM_FL = 0x00000001, //Secure Deletion
            EXT4_UNRM_FL = 0x00000002, //Record for Undelete
            EXT4_COMPR_FL = 0x00000004, //Compressed File
            EXT4_SYNC_FL = 0x00000008, //Synchronous Updates
            EXT4_IMMUTABLE_FL = 0x00000010, //Immutable File
            EXT4_APPEND_FL = 0x00000020, //Apend Only
            EXT4_NODUMP_FL = 0x00000040, //Do not Dump/Delete File
            EXT4_NOATIME_FL = 0x00000080, //Do not Update AccessTime
            EXT4_DIRTY_FL = 0x00000100, //Dirty (modified)
            EXT4_COMPRBLK_FL = 0x00000200, //Compressed Blocks
            EXT4_NOCOMPR_FL = 0x00000400, //Access Raw Compressed Data
            EXT4_ENCRYPT_FL = 0x00000800, //Compression Error
            EXT4_INDEX_FL = 0x00001000, //Hash Indexed Directory
            EXT4_IMAGIC_FL = 0x00002000, //AFS Directory
            EXT4_JOURNAL_DATA_FL = 0x00004000, //Journal File Data
            EXT4_NOTAIL_FL = 0x00008000,
            EXT4_DIRSYNC_FL = 0x00010000,
            EXT4_TOPDIR_FL = 0x00020000,
            EXT4_HUGE_FILE_FL = 0x00040000,
            EXT4_EXTENTS_FL = 0x00080000,
            EXT4_EA_INODE_FL = 0x00200000,
            EXT4_EOFBLOCKS_FL = 0x00400000,
            EXT4_SNAPFILE_FL = 0x01000000,
            EXT4_SNAPFILE_DELETED_FL = 0x04000000,
            EXT4_SNAPFILE_SHRUNK_FL = 0x08000000,
            EXT4_INLINE_DATA_FL = 0x10000000,
            EXT4_RESERVED_FL = 0x80000000 //Reserved
        }

        #endregion Enums

        #region Properties

        public readonly uint InodeNumber;
        public readonly string Mode;
        public readonly ushort UserId;
        public readonly int Size;
        public readonly DateTime AccessTime;
        public readonly DateTime ChangedTime;
        public readonly DateTime ModifiedTime;
        public readonly DateTime DeletedTime;
        public readonly ushort GroupId;
        public readonly ushort LinksCount;
        public readonly uint BlockCount;
        public readonly I_FLAGS Flags;
        public readonly Extent[] Blocks;
        public readonly uint Generation;
        public readonly uint FileACL;
        public readonly uint DirectoryACL;
        public readonly byte[] OSD2;
        public readonly ushort ExtraSize;
        public readonly ushort Checksum;
        public readonly uint ChangeTimeExtra;
        public readonly uint ModifiedTimeExtra;
        public readonly uint AccessTimeExtra;
        public readonly DateTime BornTime;
        public readonly uint BornTimeExtra;
        public readonly uint Version;

        #endregion Properties

        #region Constructors

        internal Inode(byte[] bytes, uint inode)
        {
            InodeNumber = inode;
            Mode = GetMode(BitConverter.ToUInt16(bytes, 0x00));
            UserId = BitConverter.ToUInt16(bytes, 0x02);
            Size = BitConverter.ToInt32(bytes, 0x04);
            AccessTime = NativeMethods.FromUnixTime(BitConverter.ToUInt32(bytes, 0x08));
            ChangedTime = NativeMethods.FromUnixTime(BitConverter.ToUInt32(bytes, 0x0C));
            ModifiedTime = NativeMethods.FromUnixTime(BitConverter.ToUInt32(bytes, 0x10));
            DeletedTime = NativeMethods.FromUnixTime(BitConverter.ToUInt32(bytes, 0x14));
            GroupId = BitConverter.ToUInt16(bytes, 0x18);
            LinksCount = BitConverter.ToUInt16(bytes, 0x1A);
            BlockCount = BitConverter.ToUInt32(bytes, 0x1C);
            Flags = (I_FLAGS)BitConverter.ToUInt32(bytes, 0x20);
            Blocks = Extent.GetInstances(NativeMethods.GetSubArray(bytes, 0x28, 0x3C));
            Generation = BitConverter.ToUInt32(bytes, 0x64);
            FileACL = BitConverter.ToUInt32(bytes, 0x68);
            DirectoryACL = BitConverter.ToUInt32(bytes, 0x6C);
            // Need to work on this
            OSD2 = NativeMethods.GetSubArray(bytes, 0x74, 0x0C);
            ExtraSize = BitConverter.ToUInt16(bytes, 0x80);
            Checksum = BitConverter.ToUInt16(bytes, 0x82);
            ChangeTimeExtra = BitConverter.ToUInt32(bytes, 0x84);
            ModifiedTimeExtra = BitConverter.ToUInt32(bytes, 0x88);
            AccessTimeExtra = BitConverter.ToUInt32(bytes, 0x8C);
            BornTime = NativeMethods.FromUnixTime(BitConverter.ToUInt32(bytes, 0x90));
            BornTimeExtra = BitConverter.ToUInt32(bytes, 0x94);
            Version = BitConverter.ToUInt32(bytes, 0x98);

        }

        #endregion Constructors

        private static string GetMode(uint mode)
        {
            StringBuilder sb = new StringBuilder();

            #region FileType
            if ((mode & 0x1000) == 0x1000)
            {
                // FIFO
                sb.Append('p');
            }
            else if ((mode & 0x2000) == 0x2000)
            {
                // Character Device
                sb.Append('c');
            }
            else if ((mode & 0x4000) == 0x4000)
            {
                // Directory
                sb.Append('d');
            }
            else if ((mode & 0x6000) == 0x6000)
            {
                // Block Device
                sb.Append('b');
            }
            else if ((mode & 0xA000) == 0xA000)
            {
                // Symbolic Link
                sb.Append('l');
            }
            else if ((mode & 0xC000) == 0xC000)
            {
                // Socket
                sb.Append('s');
            }
            else
            {
                // Regular File
                sb.Append('-');
            }
            #endregion FileType

            #region User
            // User Read
            if ((mode & 0x0040) == 0x0040)
            {
                sb.Append('r');
            }
            else
            {
                sb.Append('-');
            }
            // User Write
            if ((mode & 0x0080) == 0x0080)
            {
                sb.Append('w');
            }
            else
            {
                sb.Append('-');
            }
            // User Execute
            if ((mode & 0x0100) == 0x0100)
            {
                if ((mode & 0x0800) == 0x0800)
                {
                    sb.Append('s');
                }
                else
                {
                    sb.Append('x');
                }
            }
            else
            {
                if ((mode & 0x0800) == 0x0800)
                {
                    sb.Append('S');
                }
                else
                {
                    sb.Append('-');
                }
            }
            #endregion User

            #region Group
            // Group Read
            if ((mode & 0x0020) == 0x0020)
            {
                sb.Append('r');
            }
            else
            {
                sb.Append('-');
            }
            // Group Write
            if ((mode & 0x0010) == 0x0010)
            {
                sb.Append('w');
            }
            else
            {
                sb.Append('-');
            }
            // Group Execute
            if ((mode & 0x0008) == 0x0008)
            {
                if ((mode & 0x0400) == 0x0400)
                {
                    // This needs to be reviewed
                    sb.Append('s');
                }
                else
                {
                    sb.Append('x');
                }
            }
            else
            {
                if ((mode & 0x0400) == 0x0400)
                {
                    // This needs to be reviewed
                    sb.Append('S');
                }
                else
                {
                    sb.Append('-');
                }
            }
            #endregion Group

            #region Global
            // Global Read
            if ((mode & 0x0004) == 0x0004)
            {
                sb.Append('r');
            }
            else
            {
                sb.Append('-');
            }
            // Global Write
            if ((mode & 0x0002) == 0x0002)
            {
                sb.Append('w');
            }
            else
            {
                sb.Append('-');
            }
            // Global Execute
            if ((mode & 0x0001) == 0x0001)
            {
                if ((mode & 0x0200) == 0x0200)
                {
                    sb.Append('t');
                }
                else
                {
                    sb.Append('x');
                }
            }
            else
            {
                if ((mode & 0x0200) == 0x0200)
                {
                    sb.Append('T');
                }
                else
                {
                    sb.Append('-');
                }
            }
            #endregion Global

            return sb.ToString();
        }

        public byte[] GetBytes(string volume)
        {
            IntPtr hDevice = NativeMethods.getHandle(volume);

            using (FileStream streamToRead = NativeMethods.getFileStream(hDevice))
            {
                Superblock sb = new Superblock(Superblock.GetBytes(streamToRead, 0));
                //put this in a foreach block in this.Blocks loop
                return NativeMethods.readDrive(streamToRead, (sb.BlockSize * this.Blocks[0].StartBlock), (sb.BlockSize * this.Blocks[0].BlockCount));
            }
        }
    }
}
