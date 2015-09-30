using System;
using System.IO;
using System.Text;
using InvokeIR.Win32;

namespace PowerForensics.Ext3
{
    #region SuperblockClass

    public class Superblock
    {
        #region Constants

        private const uint EXT2_SUPER_MAGIC = 0xEF53;
        private const uint SUPERBLOCK_OFFSET = 0x400;
        private const uint SUPERBLOCK_SIZE = 0x400;

        #endregion Constants

        #region Enums

        [FlagsAttribute]
        public enum S_STATE
        {
            Cleanly_Unmounted = 0x0001,
            Errors_Detected = 0x0002,
            Orphans_Being_Recovered = 0x0004
        }

        public enum S_ERRORS
        {
            Continue  = 1,
            Remount_ReadOnly = 2,
            Panic = 3,
        }

        public enum S_CREATOR_OS
        {
            Linux = 0,
            Hurd = 1,
            Masix = 2,
            FreeBSD = 3,
            Lites = 4
        }

        public enum S_REV_LEVEL
        {
            OriginalFormat = 1,
            v2Format = 2
        }

        [FlagsAttribute]
        public enum S_FEATURE_COMPAT
        {
            COMPAT_DIR_PREALLOC = 0x0001,
            COMPAT_IMAGIC_INODES = 0x0002,
            COMPAT_HAS_JOURNAL = 0x0004,
            COMPAT_EXT_ATTR = 0x0008,
            COMPAT_RESIZE_INODE = 0x0010,
            COMPAT_DIR_INDEX = 0x0020,
            COMPAT_LAZY_BG = 0x0040,
            COMPAT_EXCLUDE_INODE = 0x0080,
            COMPAT_EXCLUDE_BITMAP = 0x0100,
            COMPAT_SPARSE_SUPER2 = 0x0200,
        }

        [FlagsAttribute]
        public enum S_FEATURE_INCOMPAT
        {
            INCOMPAT_COMPRESSION = 0x00001,
            INCOMPAT_FILETYPE = 0x00002,
            INCOMPAT_RECOVER = 0x00004,
            INCOMPAT_JOURNAL_DEV = 0x00008,
            INCOMPAT_META_BG = 0x00010,
            INCOMPAT_EXTENTS = 0x00040,
            INCOMPAT_64BIT = 0x00080,
            INCOMPAT_MMP = 0x00100,
            INCOMPAT_FLEX_BG = 0x00200,
            INCOMPAT_EA_INODE = 0x00400,
            INCOMPAT_DIRDATA = 0x01000,
            INCOMPAT_BG_USE_META_CSUM = 0x02000,
            INCOMPAT_LARGEDIR = 0x04000,
            INCOMPAT_INLINE_DATA = 0x08000,
            INCOMPAT_ENCRYPT = 0x10000

        }

        [FlagsAttribute]
        public enum S_FEATURE_RO_COMPAT
        {
            RO_COMPAT_SPARSE_SUPER = 0x0001,
            RO_COMPAT_LARGE_FILE = 0x0002,
            RO_COMPAT_BTREE_DIR = 0x0004,
            RO_COMPAT_HUGE_FILE = 0x0008,
            RO_COMPAT_GST_CSUM = 0x0010,
            RO_COMPAT_DIR_NLINK = 0x0020,
            RO_COMPAT_EXTRA_ISIZE = 0x0040,
            RO_COMPAT_HAS_SNAPSHOT = 0x0080,
            RO_COMPAT_QUOTA = 0x0100,
            RO_COMPAT_BIGALLOC = 0x0200,
            RO_COMPAT_METADATA_CSUM = 0x0400,
            RO_COMPAT_REPLICA = 0x0800,
            RO_COMPAT_READONLY = 0x1000
        }

        [FlagsAttribute]
        public enum S_ALGO_BITMAP
        {
            EXT2_LZV1_ALG = 0x01,
            EXT2_LZRW3A_ALG = 0x02,
            EXT2_GZIP_ALG = 0x04,
            EXT2_BZIP2_ALG = 0x08,
            EXT2_LZO_ALG = 0x10
        }

        public enum S_DEF_HASH_VERSION
        {
            Legacy = 0,
            Half_MD4 = 1,
            Tea = 2,
            Legacy_Unsigned = 3,
            Half_MD4_Unsigned = 4,
            Tea_Unsigned = 5
        }

        [FlagsAttribute]
        public enum S_DEFAULT_MOUNT_OPTS
        {
            EXT4_DEFM_DEBUG = 0x0001,
            EXT4_DEFM_BSDGROUPS = 0x0002,
            EXT4_DEFM_XATTR_USER = 0x0004,
            EXT4_DEFM_ACL = 0x0008,
            EXT4_DEFM_UID16 = 0x0010,
            EXT4_DEFM_JMODE_DATA = 0x0020,
            EXT4_DEFM_JMODE_ORDERED = 0x0040,
            EXT4_DEFM_JMODE_WBACK = 0x0060,
            EXT4_DEFM_NOBARRIER = 0x0100,
            EXT4_DEFM_BLOCK_VALIDITY = 0x0200,
            EXT4_DEFM_DISCARD = 0x0400,
            EXT4_DEFM_NODELALLOC = 0x0800
        }

        #endregion Enums

        #region Properties

        public readonly uint TotalInodeCount;
        public readonly uint TotalBlockCount;
        public readonly uint SuperUserReservedBlockCount;
        public readonly uint FreeBlockCount;
        public readonly uint FreeInodeCount;
        public readonly uint FirstDataBlock;
        public readonly uint BlockSize;
        public readonly uint ClusterSize;
        public readonly uint BlocksPerGroup;
        public readonly uint ClustersPerGroup;
        public readonly uint InodesPerGroup;
        public readonly DateTime MountTime;
        public readonly DateTime WriteTime;
        public readonly ushort MountCount;
        public readonly ushort MaxMountCount;
        public readonly ushort Signature;
        public readonly S_STATE State;
        public readonly S_ERRORS Errors;
        public readonly ushort MinorRevisionLevel;
        public readonly DateTime LastCheckTime;
        public readonly uint CheckInterval;
        public readonly S_CREATOR_OS CreatorOS;
        public readonly uint RevisionLevel;
        public readonly ushort DefaultUID;
        public readonly ushort DefaultGID;
        public readonly uint FirstNonReservedInode;
        public readonly ushort InodeSize;
        public readonly ushort BlockGroupNumber;
        public readonly S_FEATURE_COMPAT FeatureCompat;
        public readonly S_FEATURE_INCOMPAT FeatureIncompat;
        public readonly S_FEATURE_RO_COMPAT FeatureROCompat;
        public readonly Guid VolumeUUID;
        public readonly string VolumeName;
        public readonly string LastMountedDirectory;
        public readonly S_ALGO_BITMAP AlgorithmBitmap;
        public readonly byte PreAllocatedBlocks;
        public readonly byte PreAllocatedDirectoryBlocks;
        public readonly Guid JournalSuperblockUUID;
        public readonly uint JournalInode;
        public readonly uint JournalDeviceNumber;
        public readonly uint OrphanListOffset;
        // This should be an array of 4 byte seeds
        public readonly uint HTREEHashSeed;
        public readonly S_DEF_HASH_VERSION HashVersion;
        public readonly S_DEFAULT_MOUNT_OPTS DefaultMountOptions;
        public readonly uint FirstMetablockBlockGroup;
        
        #endregion Properties

        #region Constructors

        internal Superblock(byte[] bytes)
        {
            Signature = BitConverter.ToUInt16(bytes, 0x38);

            if (Signature != EXT2_SUPER_MAGIC)
            {
                TotalInodeCount = BitConverter.ToUInt32(bytes, 0x00);
                TotalBlockCount = BitConverter.ToUInt32(bytes, 0x04);
                SuperUserReservedBlockCount = BitConverter.ToUInt32(bytes, 0x08);
                FreeBlockCount = BitConverter.ToUInt32(bytes, 0x0C);
                FreeInodeCount = BitConverter.ToUInt32(bytes, 0x10);
                FirstDataBlock = BitConverter.ToUInt32(bytes, 0x14);
                #region BlockSize

                uint s_log_block_size = BitConverter.ToUInt32(bytes, 0x18);
                BlockSize = (uint)1024 << (ushort)s_log_block_size;

                #endregion BlockSize
                #region ClusterSize

                uint s_log_frag_size = BitConverter.ToUInt32(bytes, 0x1C);
                ClusterSize = (uint)1024 << (ushort)s_log_frag_size;

                #endregion ClusterSize
                BlocksPerGroup = BitConverter.ToUInt32(bytes, 0x20);
                ClustersPerGroup = BitConverter.ToUInt32(bytes, 0x24);
                InodesPerGroup = BitConverter.ToUInt32(bytes, 0x28);
                MountTime = NativeMethods.FromUnixTime(BitConverter.ToUInt32(bytes, 0x2C));
                WriteTime = NativeMethods.FromUnixTime(BitConverter.ToUInt32(bytes, 0x30));
                MountCount = BitConverter.ToUInt16(bytes, 0x34);
                MaxMountCount = BitConverter.ToUInt16(bytes, 0x36);
                State = (S_STATE)BitConverter.ToUInt16(bytes, 0x3A);
                Errors = (S_ERRORS)BitConverter.ToUInt32(bytes, 0x3C);
                MinorRevisionLevel = BitConverter.ToUInt16(bytes, 0x3E);
                LastCheckTime = NativeMethods.FromUnixTime(BitConverter.ToUInt32(bytes, 0x40));
                CheckInterval = BitConverter.ToUInt32(bytes, 0x44);
                CreatorOS = (S_CREATOR_OS)BitConverter.ToUInt32(bytes, 0x48);
                RevisionLevel = BitConverter.ToUInt32(bytes, 0x4C);
                DefaultUID = BitConverter.ToUInt16(bytes, 0x50);
                DefaultGID = BitConverter.ToUInt16(bytes, 0x52);
                FirstNonReservedInode = BitConverter.ToUInt32(bytes, 0x54);
                InodeSize = BitConverter.ToUInt16(bytes, 0x58);
                BlockGroupNumber = BitConverter.ToUInt16(bytes, 0x5A);
                FeatureCompat = (S_FEATURE_COMPAT)BitConverter.ToUInt32(bytes, 0x5C);
                FeatureIncompat = (S_FEATURE_INCOMPAT)BitConverter.ToUInt32(bytes, 0x60);
                FeatureROCompat = (S_FEATURE_RO_COMPAT)BitConverter.ToUInt32(bytes, 0x64);
                VolumeUUID = new Guid(NativeMethods.GetSubArray(bytes, 0x68, 0x10));
                VolumeName = Encoding.ASCII.GetString(bytes, 0x78, 0x10).TrimEnd('\0');
                LastMountedDirectory = Encoding.ASCII.GetString(bytes, 0x88, 0x40).Split('\0')[0];
                AlgorithmBitmap = (S_ALGO_BITMAP)BitConverter.ToUInt32(bytes, 0xC8);
                PreAllocatedBlocks = bytes[0xCC];
                PreAllocatedDirectoryBlocks = bytes[0xCD];
                JournalSuperblockUUID = new Guid(NativeMethods.GetSubArray(bytes, 0xD0, 0x10));
                JournalInode = BitConverter.ToUInt32(bytes, 0xE0);
                JournalDeviceNumber = BitConverter.ToUInt32(bytes, 0xE4);
                OrphanListOffset = BitConverter.ToUInt32(bytes, 0xE8);
                HTREEHashSeed = BitConverter.ToUInt32(bytes, 0xEC);
                HashVersion = (S_DEF_HASH_VERSION)bytes[0xFC];
                DefaultMountOptions = (S_DEFAULT_MOUNT_OPTS)BitConverter.ToUInt32(bytes, 0x100);
                FirstMetablockBlockGroup = BitConverter.ToUInt32(bytes, 0x104);
            }
            else
            {
                throw new Exception("Invalid Superblock Signature.");
            }
        }

        #endregion Constructors

        #region StaticMethods

        internal static byte[] GetBytes(FileStream streamToRead, ulong offset)
        {
            // Get Superblock bytes
            return NativeMethods.readDrive(streamToRead, (offset * 512) + SUPERBLOCK_OFFSET, SUPERBLOCK_SIZE);
        }
        
        #endregion StaticMethods
    }
    
    #endregion SuperblockClass
}
