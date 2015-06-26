using System;
using System.IO;
using System.Text;
using InvokeIR.Win32;

namespace InvokeIR.PowerForensics.ext4
{
    public class Superblock
    {
        #region Constants

        private const uint SUPERBLOCK_OFFSET = 0x400;
        private const uint SUPERBLOCK_SIZE = 0x400;

        #endregion Constants

        #region Enums

        [FlagsAttribute]
        internal enum S_STATE
        {
            Cleanly_Unmounted = 0x0001,
            Errors_Detected = 0x0002,
            Orphans_Being_Recovered = 0x0004
        }

        internal enum S_ERRORS
        {
            Continue  = 1,
            Remount_ReadOnly = 2,
            Panic = 3,
        }

        internal enum S_CREATOR_OS
        {
            Linux = 0,
            Hurd = 1,
            Masix = 2,
            FreeBSD = 3,
            Lites = 4
        }

        internal enum S_REV_LEVEL
        {
            OriginalFormat = 1,
            v2Format = 2
        }

        [FlagsAttribute]
        internal enum S_FEATURE_COMPAT
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
        internal enum S_FEATURE_INCOMPAT
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
        internal enum S_FEATURE_RO_COMPAT
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

        internal enum S_DEF_HASH_VERSION
        {
            Legacy = 0,
            Half_MD4 = 1,
            Tea = 2,
            Legacy_Unsigned = 3,
            Half_MD4_Unsigned = 4,
            Tea_Unsigned = 5
        }

        [FlagsAttribute]
        internal enum S_DEFAULT_MOUNT_OPTS
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
        internal enum S_FLAGS
        {
            Signed_directory_hash_in_use = 0x0001,
            Unsigned_directory_hash_in_use = 0x0002,
            To_test_development_code = 0x0004
        }

        internal enum S_CHECKSUM_TYPE
        {
            CRC32C = 1
        }

        internal enum S_ENCRYPT_ALGOS
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
        public readonly string State;
        public readonly string Errors;
        public readonly ushort MinorRevisionLevel;
        public readonly DateTime LastCheckTime;
        public readonly uint CheckInterval;
        public readonly string CreatorOS;
        public readonly uint RevisionLevel;
        public readonly ushort s_def_resuid;
        public readonly ushort s_def_resgid;
        public readonly uint FirstNonReservedInode;
        public readonly ushort InodeSize;
        public readonly ushort BlockGroupNumber;
        public readonly string FeatureCompat;
        public readonly string FeatureIncompat;
        public readonly string FeatureROCompat;
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
        public readonly string HashVersion;
        public readonly byte s_jnl_backup_type;
        public readonly ushort GroupDescriptorSize;
        public readonly string DefaultMountOptions;
        public readonly uint FirstMetablockBlockGroup;
        public readonly DateTime FileSystemCreationTime;
        public readonly uint s_jnl_blocks;
        public readonly uint BlockCountHi;
        public readonly uint ReservedBlockCountHi;
        public readonly uint FreeBlockCountHi;
        public readonly ushort s_min_extra_isize;
        public readonly ushort s_want_extra_isize;
        public readonly string Flags;
        public readonly ushort RAIDStride;
        public readonly ushort MMPInterval;
        public readonly ulong MMPBlockNumber;
        public readonly uint RAIDStripeWidth;
        public readonly byte FlexibleBlockGroupSize;
        public readonly string ChecksumType;
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
        public readonly string LastErrorFunctionName;
        public readonly string MountOptions;
        public readonly uint UserQuotaInode;
        public readonly uint GroupQuotaInode;
        public readonly uint OverheadBlocks;
        public readonly uint s_backup_bgs;
        public readonly string EncryptionAlgorithm;
        public readonly uint Checksum;
        
        #endregion Properties

        #region Constructors

        internal Superblock(byte[] bytes)
        {
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
            Signature = BitConverter.ToUInt16(bytes, 0x38);
            State = ((S_STATE)BitConverter.ToUInt16(bytes, 0x3A)).ToString();
            Errors = (Enum.GetName(typeof(S_ERRORS), BitConverter.ToUInt32(bytes, 0x3C)));
            MinorRevisionLevel = BitConverter.ToUInt16(bytes, 0x3E);
            LastCheckTime = NativeMethods.FromUnixTime(BitConverter.ToUInt32(bytes, 0x40));
            CheckInterval = BitConverter.ToUInt32(bytes, 0x44);
            CreatorOS = (Enum.GetName(typeof(S_CREATOR_OS), BitConverter.ToUInt32(bytes, 0x48)));
            RevisionLevel = BitConverter.ToUInt32(bytes, 0x4C);
            s_def_resuid = BitConverter.ToUInt16(bytes, 0x50);
            s_def_resgid = BitConverter.ToUInt16(bytes, 0x52);
            FirstNonReservedInode = BitConverter.ToUInt32(bytes, 0x54);
            InodeSize = BitConverter.ToUInt16(bytes, 0x58);
            BlockGroupNumber = BitConverter.ToUInt16(bytes, 0x5A);
            FeatureCompat = ((S_FEATURE_COMPAT)BitConverter.ToUInt32(bytes, 0x5C)).ToString();
            FeatureIncompat = ((S_FEATURE_INCOMPAT)BitConverter.ToUInt32(bytes, 0x60)).ToString();
            FeatureROCompat = ((S_FEATURE_RO_COMPAT)BitConverter.ToUInt32(bytes, 0x64)).ToString();
            #region VolumeUUID
            byte[] VolumeUUIDBytes = new byte[16];
            Array.Copy(bytes, 0x68, VolumeUUIDBytes, 0, VolumeUUIDBytes.Length);
            VolumeUUID = new Guid(VolumeUUIDBytes);
            #endregion VolumeUUID
            #region VolumeLabel
            byte[] VolumeLabelBytes = new byte[16];
            Array.Copy(bytes, 0x78, VolumeLabelBytes, 0, VolumeLabelBytes.Length);
            VolumeLabel = Encoding.ASCII.GetString(VolumeLabelBytes).TrimEnd('\0');
            #endregion VolumeLabel
            #region LastMountedDirectory
            byte[] LastMountedDirectoryBytes = new byte[64];
            Array.Copy(bytes, 0x88, LastMountedDirectoryBytes, 0, LastMountedDirectoryBytes.Length);
            LastMountedDirectory = Encoding.ASCII.GetString(LastMountedDirectoryBytes).TrimEnd('\0');
            #endregion LastMountedDirectory
            s_algorithm_usage_bitmap = BitConverter.ToUInt32(bytes, 0xC8);
            PreAllocatedBlocks = bytes[0xCC];
            PreAllocatedDirectoryBlocks = bytes[0xCD];
            ReservedGDTBlocks = BitConverter.ToUInt16(bytes, 0xCE);
            #region JournalSuperblockUUID
            byte[] JournalSuperBlockUUIDBytes = new byte[16];
            Array.Copy(bytes, 0xD0, JournalSuperBlockUUIDBytes, 0, JournalSuperBlockUUIDBytes.Length);
            JournalSuperblockUUID = new Guid(JournalSuperBlockUUIDBytes);
            #endregion JournalSuperblockUUID
            JournalInode = BitConverter.ToUInt32(bytes, 0xE0);
            JournalDeviceNumber = BitConverter.ToUInt32(bytes, 0xE4);
            s_last_orphan = BitConverter.ToUInt32(bytes, 0xE8);
            HTREEHashSeed = BitConverter.ToUInt32(bytes, 0xEC);
            HashVersion = (Enum.GetName(typeof(S_DEF_HASH_VERSION), bytes[0xFC]));
            s_jnl_backup_type = bytes[0xFD];
            GroupDescriptorSize = BitConverter.ToUInt16(bytes, 0xFE);
            DefaultMountOptions = ((S_DEFAULT_MOUNT_OPTS)BitConverter.ToUInt32(bytes, 0x100)).ToString();
            FirstMetablockBlockGroup = BitConverter.ToUInt32(bytes, 0x104);
            FileSystemCreationTime = NativeMethods.FromUnixTime(BitConverter.ToUInt32(bytes, 0x108));
            s_jnl_blocks = BitConverter.ToUInt32(bytes, 0x10C);
            BlockCountHi = BitConverter.ToUInt32(bytes, 0x150);
            ReservedBlockCountHi = BitConverter.ToUInt32(bytes, 0x154);
            FreeBlockCount = BitConverter.ToUInt32(bytes, 0x158);
            s_min_extra_isize = BitConverter.ToUInt16(bytes, 0x15C);
            s_want_extra_isize = BitConverter.ToUInt16(bytes, 0x15E);
            Flags = ((S_FLAGS)BitConverter.ToUInt32(bytes, 0x160)).ToString();
            RAIDStride = BitConverter.ToUInt16(bytes, 0x164);
            MMPInterval = BitConverter.ToUInt16(bytes, 0x166);
            MMPBlockNumber = BitConverter.ToUInt64(bytes, 0x168);
            RAIDStripeWidth = BitConverter.ToUInt32(bytes, 0x170);
            FlexibleBlockGroupSize = bytes[0x174];
            ChecksumType = (Enum.GetName(typeof(S_CHECKSUM_TYPE), bytes[0x175]));
            KBWrittenLifetime = BitConverter.ToUInt64(bytes, 0x178);
            ActiveSnapshotInode = BitConverter.ToUInt32(bytes, 0x180);
            ActiveSnapshotId = BitConverter.ToUInt32(bytes, 0x184);
            s_snapshot_r_blocks_count = BitConverter.ToUInt64(bytes, 0x188);
            SnapshotListInode = BitConverter.ToUInt32(bytes, 0x190);
            ErrorCount = BitConverter.ToUInt32(bytes, 0x194);
            FirstErrorTime = NativeMethods.FromUnixTime(BitConverter.ToUInt32(bytes, 0x198));
            FirstErrorInode = BitConverter.ToUInt32(bytes, 0x19C);
            FirstErrorBlockNumber = BitConverter.ToUInt64(bytes, 0x1A0);
            #region FirstErrorFunctionName
            byte[] FirstErrorFunctionNameBytes = new byte[32];
            Array.Copy(bytes, 0x1A8, FirstErrorFunctionNameBytes, 0, FirstErrorFunctionNameBytes.Length);
            FirstErrorFunctionName = Encoding.ASCII.GetString(FirstErrorFunctionNameBytes).TrimEnd('\0');
            #endregion FirstErrorFunctionName
            FirstErrorLineNumber = BitConverter.ToUInt32(bytes, 0x1C8);
            LastErrorTime = NativeMethods.FromUnixTime(BitConverter.ToUInt32(bytes, 0x1CC));
            LastErrorInode = BitConverter.ToUInt32(bytes, 0x1D0);
            LastErrorLineNumber = BitConverter.ToUInt32(bytes, 0x1D4);
            LastErrorBlockNumber = BitConverter.ToUInt64(bytes, 0x1D8);
            #region LastErrorFunctionName
            byte[] LastErrorFunctionNameBytes = new byte[32];
            Array.Copy(bytes, 0x1A8, LastErrorFunctionNameBytes, 0, LastErrorFunctionNameBytes.Length);
            LastErrorFunctionName = Encoding.ASCII.GetString(LastErrorFunctionNameBytes).TrimEnd('\0');
            #endregion LastErrorFunctionName
            #region MountOptions
            byte[] MountOptionsBytes = new byte[64];
            Array.Copy(bytes, 0x200, MountOptionsBytes, 0, MountOptionsBytes.Length);
            MountOptions = Encoding.ASCII.GetString(MountOptionsBytes).TrimEnd('\0');
            #endregion MountOptions
            UserQuotaInode = BitConverter.ToUInt32(bytes, 0x240);
            GroupQuotaInode = BitConverter.ToUInt32(bytes, 0x244);
            OverheadBlocks = BitConverter.ToUInt32(bytes, 0x248);
            s_backup_bgs = BitConverter.ToUInt32(bytes, 0x24C);
            EncryptionAlgorithm = (Enum.GetName(typeof(S_ENCRYPT_ALGOS), BitConverter.ToUInt32(bytes, 0x24E)));
            Checksum = BitConverter.ToUInt32(bytes, 0x3FC);
        }

        #endregion Constructors

        internal static byte[] GetBytes(FileStream streamToRead, ulong offset)
        {
            // Get Superblock bytes
            return NativeMethods.readDrive(streamToRead, (offset * 512) + SUPERBLOCK_OFFSET, SUPERBLOCK_SIZE);
        }
    }
}
