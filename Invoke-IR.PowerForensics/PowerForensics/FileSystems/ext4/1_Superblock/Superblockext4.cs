using System;
using System.IO;
using System.Text;
using InvokeIR.Win32;

namespace InvokeIR.PowerForensics.Ext4
{
    #region SuperblockClass

    public class Superblock
    {
        #region Constants

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

        [FlagsAttribute]
        public enum S_FLAGS
        {
            Signed_directory_hash_in_use = 0x0001,
            Unsigned_directory_hash_in_use = 0x0002,
            To_test_development_code = 0x0004
        }

        public enum S_CHECKSUM_TYPE
        {
            CRC32C = 1
        }

        public enum S_ENCRYPT_ALGOS
        {
            ENCRYPTION_MODE_INVALID = 0,
            ENCRYPTION_MOD_AES_256_XTS = 1,
            ENCRYPTION_MOD_AES_256_GCM = 2,
            ENCRYPTION_MOD_AES_256_CBC = 3
        }

        #endregion Enums

        #region Properties

        public readonly uint TotalInodeCount;
        public readonly uint TotalBlockCount;
        public readonly uint s_r_blocks_count_lo;
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
        public readonly ushort s_def_resuid;
        public readonly ushort s_def_resgid;
        public readonly uint FirstNonReservedInode;
        public readonly ushort InodeSize;
        public readonly ushort BlockGroupNumber;
        public readonly S_FEATURE_COMPAT FeatureCompat;
        public readonly S_FEATURE_INCOMPAT FeatureIncompat;
        public readonly S_FEATURE_RO_COMPAT FeatureROCompat;
        public readonly Guid VolumeUUID;
        public readonly string VolumeLabel;
        public readonly string LastMountedDirectory;
        public readonly uint s_algorithm_usage_bitmap;
        public readonly byte PreAllocatedBlocks;
        public readonly byte PreAllocatedDirectoryBlocks;
        public readonly ushort ReservedGDTBlocks;
        public readonly Guid JournalSuperblockUUID;
        public readonly uint JournalInode;
        public readonly uint JournalDeviceNumber;
        public readonly uint s_last_orphan;
        public readonly uint HTREEHashSeed;
        public readonly S_DEF_HASH_VERSION HashVersion;
        public readonly byte s_jnl_backup_type;
        public readonly ushort GroupDescriptorSize;
        public readonly S_DEFAULT_MOUNT_OPTS DefaultMountOptions;
        public readonly uint FirstMetablockBlockGroup;
        public readonly DateTime FileSystemCreationTime;
        public readonly uint s_jnl_blocks;
        public readonly uint BlockCountHi;
        public readonly uint ReservedBlockCountHi;
        public readonly uint FreeBlockCountHi;
        public readonly ushort s_min_extra_isize;
        public readonly ushort s_want_extra_isize;
        public readonly S_FLAGS Flags;
        public readonly ushort RAIDStride;
        public readonly ushort MMPInterval;
        public readonly ulong MMPBlockNumber;
        public readonly uint RAIDStripeWidth;
        public readonly byte FlexibleBlockGroupSize;
        public readonly S_CHECKSUM_TYPE ChecksumType;
        public readonly ulong KBWrittenLifetime;
        public readonly uint ActiveSnapshotInode;
        public readonly uint ActiveSnapshotId;
        public readonly ulong s_snapshot_r_blocks_count;
        public readonly uint SnapshotListInode;
        public readonly uint ErrorCount;
        public readonly DateTime FirstErrorTime;
        public readonly uint FirstErrorInode;
        public readonly ulong FirstErrorBlockNumber;
        public readonly string FirstErrorFunctionName;
        public readonly uint FirstErrorLineNumber;
        public readonly DateTime LastErrorTime;
        public readonly uint LastErrorInode;
        public readonly uint LastErrorLineNumber;
        public readonly ulong LastErrorBlockNumber;
        //public readonly string LastErrorFunctionName;
        public readonly string MountOptions;
        public readonly uint UserQuotaInode;
        public readonly uint GroupQuotaInode;
        public readonly uint OverheadBlocks;
        public readonly uint s_backup_bgs;
        public readonly S_ENCRYPT_ALGOS EncryptionAlgorithm;
        public readonly uint Checksum;
        
        #endregion Properties

        #region Constructors

        internal Superblock(byte[] bytes)
        {
            Signature = BitConverter.ToUInt16(bytes, 0x38);

            TotalInodeCount = BitConverter.ToUInt32(bytes, 0x00);
            TotalBlockCount = BitConverter.ToUInt32(bytes, 0x04);
            s_r_blocks_count_lo = BitConverter.ToUInt32(bytes, 0x08);
            FreeBlockCount = BitConverter.ToUInt32(bytes, 0x0C);
            FreeInodeCount = BitConverter.ToUInt32(bytes, 0x10);
            FirstDataBlock = BitConverter.ToUInt32(bytes, 0x14);
            BlockSize = BitConverter.ToUInt32(bytes, 0x18);
            ClusterSize = BitConverter.ToUInt32(bytes, 0x1C);
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
            s_def_resuid = BitConverter.ToUInt16(bytes, 0x50);
            s_def_resgid = BitConverter.ToUInt16(bytes, 0x52);
            FirstNonReservedInode = BitConverter.ToUInt32(bytes, 0x54);
            InodeSize = BitConverter.ToUInt16(bytes, 0x58);
            BlockGroupNumber = BitConverter.ToUInt16(bytes, 0x5A);
            FeatureCompat = (S_FEATURE_COMPAT)BitConverter.ToUInt32(bytes, 0x5C);
            FeatureIncompat = (S_FEATURE_INCOMPAT)BitConverter.ToUInt32(bytes, 0x60);
            FeatureROCompat = (S_FEATURE_RO_COMPAT)BitConverter.ToUInt32(bytes, 0x64);
            VolumeUUID = new Guid(NativeMethods.GetSubArray(bytes, 0x68, 0x10));
            VolumeLabel = Encoding.ASCII.GetString(bytes, 0x78, 0x10).TrimEnd('\0');
            LastMountedDirectory = Encoding.ASCII.GetString(bytes, 0x88, 0x40).TrimEnd('\0');
            s_algorithm_usage_bitmap = BitConverter.ToUInt32(bytes, 0xC8);
            PreAllocatedBlocks = bytes[0xCC];
            PreAllocatedDirectoryBlocks = bytes[0xCD];
            ReservedGDTBlocks = BitConverter.ToUInt16(bytes, 0xCE);
            JournalSuperblockUUID = new Guid(NativeMethods.GetSubArray(bytes, 0xD0, 0x10));
            JournalInode = BitConverter.ToUInt32(bytes, 0xE0);
            JournalDeviceNumber = BitConverter.ToUInt32(bytes, 0xE4);
            s_last_orphan = BitConverter.ToUInt32(bytes, 0xE8);
            HTREEHashSeed = BitConverter.ToUInt32(bytes, 0xEC);
            HashVersion = (S_DEF_HASH_VERSION)bytes[0xFC];
            s_jnl_backup_type = bytes[0xFD];
            GroupDescriptorSize = BitConverter.ToUInt16(bytes, 0xFE);
            DefaultMountOptions = (S_DEFAULT_MOUNT_OPTS)BitConverter.ToUInt32(bytes, 0x100);
            FirstMetablockBlockGroup = BitConverter.ToUInt32(bytes, 0x104);
            FileSystemCreationTime = NativeMethods.FromUnixTime(BitConverter.ToUInt32(bytes, 0x108));
            s_jnl_blocks = BitConverter.ToUInt32(bytes, 0x10C);
            BlockCountHi = BitConverter.ToUInt32(bytes, 0x150);
            ReservedBlockCountHi = BitConverter.ToUInt32(bytes, 0x154);
            FreeBlockCount = BitConverter.ToUInt32(bytes, 0x158);
            s_min_extra_isize = BitConverter.ToUInt16(bytes, 0x15C);
            s_want_extra_isize = BitConverter.ToUInt16(bytes, 0x15E);
            Flags = (S_FLAGS)BitConverter.ToUInt32(bytes, 0x160);
            RAIDStride = BitConverter.ToUInt16(bytes, 0x164);
            MMPInterval = BitConverter.ToUInt16(bytes, 0x166);
            MMPBlockNumber = BitConverter.ToUInt64(bytes, 0x168);
            RAIDStripeWidth = BitConverter.ToUInt32(bytes, 0x170);
            FlexibleBlockGroupSize = bytes[0x174];
            ChecksumType = (S_CHECKSUM_TYPE)bytes[0x175];
            KBWrittenLifetime = BitConverter.ToUInt64(bytes, 0x178);
            ActiveSnapshotInode = BitConverter.ToUInt32(bytes, 0x180);
            ActiveSnapshotId = BitConverter.ToUInt32(bytes, 0x184);
            s_snapshot_r_blocks_count = BitConverter.ToUInt64(bytes, 0x188);
            SnapshotListInode = BitConverter.ToUInt32(bytes, 0x190);
            ErrorCount = BitConverter.ToUInt32(bytes, 0x194);
            FirstErrorTime = NativeMethods.FromUnixTime(BitConverter.ToUInt32(bytes, 0x198));
            FirstErrorInode = BitConverter.ToUInt32(bytes, 0x19C);
            FirstErrorBlockNumber = BitConverter.ToUInt64(bytes, 0x1A0);
            FirstErrorFunctionName = Encoding.ASCII.GetString(bytes, 0x1A8, 0x20).TrimEnd('\0');
            FirstErrorLineNumber = BitConverter.ToUInt32(bytes, 0x1C8);
            LastErrorTime = NativeMethods.FromUnixTime(BitConverter.ToUInt32(bytes, 0x1CC));
            LastErrorInode = BitConverter.ToUInt32(bytes, 0x1D0);
            LastErrorLineNumber = BitConverter.ToUInt32(bytes, 0x1D4);
            LastErrorBlockNumber = BitConverter.ToUInt64(bytes, 0x1D8);
            //LastErrorFunctionName = Encoding.ASCII.GetString(bytes, 0x1A8, 0x20).TrimEnd('\0');
            MountOptions = Encoding.ASCII.GetString(bytes, 0x200, 0x64).TrimEnd('\0');
            UserQuotaInode = BitConverter.ToUInt32(bytes, 0x240);
            GroupQuotaInode = BitConverter.ToUInt32(bytes, 0x244);
            OverheadBlocks = BitConverter.ToUInt32(bytes, 0x248);
            s_backup_bgs = BitConverter.ToUInt32(bytes, 0x24C);
            EncryptionAlgorithm = (S_ENCRYPT_ALGOS)BitConverter.ToUInt32(bytes, 0x24E);
            Checksum = BitConverter.ToUInt32(bytes, 0x3FC);
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
