using System;
using System.Text;
using InvokeIR.Win32;

namespace PowerForensics.EseDb
{
    #region JetHeaderClass
    public class JetHeader
    {
        #region Enums

        public enum JET_FILETYPE
        {
            Database = 0x00,
            StreamingFile = 0x01
        }

        public enum JET_DBSTATE
        {
            JustCreated = 0x01,
            DirtyShutdown = 0x02,
            CleanShutdown = 0x03,
            BeingConverted = 0x04,
            ForceDetach = 0x05
        }

        #endregion Enums

        #region Properties

        internal readonly uint Checksum;
        internal readonly uint Signature;
        internal readonly uint FileFormatVersion;
        internal readonly JET_FILETYPE FileType;
        //internal readonly byte[] DatabaseTime;
        internal readonly JET_SIGNATURE DatabaseSignature;
        internal readonly JET_DBSTATE DatabaseState;
        internal readonly JET_LGPOS ConsistentPostition;
        internal readonly DateTime ConsistentTime;
        internal readonly DateTime AttachTime;
        internal readonly JET_LGPOS AttachPosition;
        internal readonly DateTime DetachTime;
        internal readonly JET_LGPOS DetachPosition;
        internal readonly JET_SIGNATURE LogSignature;
        internal readonly JET_BKINFO PreviousFullBackup;
        internal readonly JET_BKINFO PreviousIncrementalBackup;
        internal readonly JET_BKINFO CurrentFullBackup;
        internal readonly uint ShadowingDisabled;
        internal readonly uint LastObjectIdentifier;
        internal readonly uint MajorVersion;
        internal readonly uint MinorVersion;
        internal readonly uint BuildNumber;
        internal readonly uint ServicePackNumber;
        internal readonly uint FileFormatRevision;
        internal readonly uint PageSize;
        internal readonly uint RepairCount;
        internal readonly DateTime RepairTime;
        //internal readonly byte[] ScrubDatabaseTime;
        internal readonly DateTime ScrubTime;
        internal readonly uint RequiredLog;
        internal readonly uint UpgradeExchangeFormat;
        internal readonly uint UpgradeFreePages;
        internal readonly uint UpgradeSpaceMapPages;
        internal readonly JET_BKINFO CurrentShadowCopyBackup;
        internal readonly uint CreationFileFormatVersion;
        internal readonly uint CreationFileFormatRevision;
        internal readonly uint OldRepairCount;
        internal readonly uint EccFixSuccessCount;
        internal readonly DateTime LastEccFixTime;
        internal readonly uint OldEccFixSuccessCount;
        internal readonly uint EccFixErrorCount;
        internal readonly DateTime LastEccFixErrorTime;
        internal readonly uint OldEccFixErrorCount;
        internal readonly uint BadChecksumErrorCount;
        internal readonly DateTime LastBadChecksumTime;
        internal readonly uint OldBadChecksumErrorCount;
        internal readonly uint CommittedLog;
        internal readonly JET_BKINFO PreviousShadowBackup;
        internal readonly JET_BKINFO PreviousDifferentialBackup;
        internal readonly uint NlsMajorVersion;
        internal readonly uint NlsMinorVersion;

        #endregion Properties

        #region Constructors

        internal JetHeader(byte[] bytes)
        {
            Checksum = BitConverter.ToUInt32(bytes, 0x00);
            Signature = BitConverter.ToUInt32(bytes, 0x04);
            FileFormatVersion = BitConverter.ToUInt32(bytes, 0x08);
            FileType = (JET_FILETYPE)BitConverter.ToUInt32(bytes, 0x0C);
            //DatabaseTime = NativeMethods.GetSubArray(bytes, 0x10, 0x08);
            DatabaseSignature = new JET_SIGNATURE(NativeMethods.GetSubArray(bytes, 0x18, 0x1C));
            DatabaseState = (JET_DBSTATE)BitConverter.ToUInt32(bytes, 0x34);
            ConsistentPostition = new JET_LGPOS(NativeMethods.GetSubArray(bytes, 0x38, 0x08));
            ConsistentTime = JET_LOGTIME.Get(bytes[0x45], bytes[0x44], bytes[0x43], bytes[0x42], bytes[0x41], bytes[0x40]);
            AttachTime = JET_LOGTIME.Get(bytes[0x4D], bytes[0x4C], bytes[0x4B], bytes[0x4A], bytes[0x49], bytes[0x48]);
            AttachPosition = new JET_LGPOS(NativeMethods.GetSubArray(bytes, 0x50, 0x08));
            DetachTime = JET_LOGTIME.Get(bytes[0x5D], bytes[0x5C], bytes[0x5B], bytes[0x5A], bytes[0x59], bytes[0x58]);
            DetachPosition = new JET_LGPOS(NativeMethods.GetSubArray(bytes, 0x60, 0x08));
            LogSignature = new JET_SIGNATURE(NativeMethods.GetSubArray(bytes, 0x68, 0x1C));
            PreviousFullBackup = new JET_BKINFO(NativeMethods.GetSubArray(bytes, 0x88, 0x18));
            PreviousIncrementalBackup = new JET_BKINFO(NativeMethods.GetSubArray(bytes, 0xA0, 0x18));
            CurrentFullBackup = new JET_BKINFO(NativeMethods.GetSubArray(bytes, 0xB8, 0x18));
            ShadowingDisabled = BitConverter.ToUInt32(bytes, 0xBC);
            LastObjectIdentifier = BitConverter.ToUInt32(bytes, 0xC0);
            MajorVersion = BitConverter.ToUInt32(bytes, 0xC4);
            MinorVersion = BitConverter.ToUInt32(bytes, 0xC8);
            BuildNumber = BitConverter.ToUInt32(bytes, 0xCC);
            ServicePackNumber = BitConverter.ToUInt32(bytes, 0xD0);
            FileFormatRevision = BitConverter.ToUInt32(bytes, 0xD4);
            PageSize = BitConverter.ToUInt32(bytes, 0xD8);
            RepairCount = BitConverter.ToUInt32(bytes, 0xDC);
            RepairTime = JET_LOGTIME.Get(bytes[0xE5], bytes[0xE4], bytes[0xE3], bytes[0xE2], bytes[0xE1], bytes[0xE0]);
            //ScrubDatabaseTime = NativeMethods.GetSubArray(bytes, 0x118, 0x08);
            ScrubTime = JET_LOGTIME.Get(NativeMethods.GetSubArray(bytes, 0x120, 0x08));
            RequiredLog = BitConverter.ToUInt32(bytes, 0x128);
            UpgradeExchangeFormat = BitConverter.ToUInt32(bytes, 0x130);
            UpgradeFreePages = BitConverter.ToUInt32(bytes, 0x134);
            UpgradeSpaceMapPages = BitConverter.ToUInt32(bytes, 0x138);
            CurrentShadowCopyBackup = new JET_BKINFO(NativeMethods.GetSubArray(bytes, 0x13C, 0x18));
            CreationFileFormatVersion = BitConverter.ToUInt32(bytes, 0x154);
            CreationFileFormatRevision = BitConverter.ToUInt32(bytes, 0x158);
            OldRepairCount = BitConverter.ToUInt32(bytes, 0x16C);            
            EccFixSuccessCount = BitConverter.ToUInt32(bytes, 0x170);
            LastEccFixTime = JET_LOGTIME.Get(bytes[0x179], bytes[0x178], bytes[0x177], bytes[0x176], bytes[0x175], bytes[0x174]);
            OldEccFixSuccessCount = BitConverter.ToUInt32(bytes, 0x17C);
            EccFixErrorCount = BitConverter.ToUInt32(bytes, 0x190);
            LastEccFixErrorTime = JET_LOGTIME.Get(bytes[0x189], bytes[0x188], bytes[0x187], bytes[0x186], bytes[0x185], bytes[0x184]);
            OldEccFixErrorCount = BitConverter.ToUInt32(bytes, 0x18C);
            BadChecksumErrorCount = BitConverter.ToUInt32(bytes, 0x190);
            LastBadChecksumTime = JET_LOGTIME.Get(bytes[0x199], bytes[0x198], bytes[0x197], bytes[0x196], bytes[0x195], bytes[0x194]);
            OldBadChecksumErrorCount = BitConverter.ToUInt32(bytes, 0x19C);
            CommittedLog = BitConverter.ToUInt32(bytes, 0x1A0);
            PreviousShadowBackup = new JET_BKINFO(NativeMethods.GetSubArray(bytes, 0x1A4, 0x18));
            PreviousDifferentialBackup = new JET_BKINFO(NativeMethods.GetSubArray(bytes, 0x1BC, 0x18));
            NlsMajorVersion = BitConverter.ToUInt32(bytes, 0x1FC);
            NlsMinorVersion = BitConverter.ToUInt32(bytes, 0x200);
        }

        #endregion Constructors
    }

    #endregion HeaderClass

    #region JET_LOGTIMEClass

    internal class JET_LOGTIME
    {
        internal static DateTime Get(byte[] bytes)
        {
            return new DateTime((bytes[5] + 1900), bytes[4], bytes[3], bytes[2], bytes[1], bytes[0]);
        }

        internal static DateTime Get(byte year, byte month, byte day, byte hour, byte min, byte sec)
        {
            return new DateTime((year + 1900), month, day, hour, min, sec);
        }
    }

    #endregion JET_LOGTIMEClass

    #region JET_SIGNATUREClass

    public class JET_SIGNATURE
    {
        #region Properties

        internal readonly uint rand;
        internal readonly DateTime CreationTime;
        internal readonly string NetBIOSName;

        #endregion Properties

        #region Constructors

        internal JET_SIGNATURE(byte[] bytes)
        {
            rand = BitConverter.ToUInt32(bytes, 0x00);
            CreationTime = JET_LOGTIME.Get(bytes[0x09], bytes[0x08], bytes[0x07], bytes[0x06], bytes[0x05], bytes[0x04]);
            NetBIOSName = Encoding.ASCII.GetString(bytes, 0x0C, 0x10);
        }

        #endregion Constructors
    }

    #endregion JET_SIGNATUREClass

    #region JET_BKINFOClass

    public class JET_BKINFO
    {
        #region Properties

        public readonly JET_LGPOS BackupPosition;
        public readonly DateTime BackupTime;
        public readonly byte[] GenerationLow;
        public readonly byte[] GenerationHi;

        #endregion Properties

        #region Constructors

        internal JET_BKINFO(byte[] bytes)
        {
            BackupPosition = new JET_LGPOS(NativeMethods.GetSubArray(bytes, 0x00, 0x08));
            BackupTime = JET_LOGTIME.Get(bytes[0x0D], bytes[0x0C], bytes[0x0B], bytes[0x0A], bytes[0x09], bytes[0x08]);
        }

        #endregion Constructors
    }

    #endregion JET_BKINFOClass

    #region JET_LGPOSClass

    public class JET_LGPOS
    {
        #region Properties

        public readonly ushort Block;
        public readonly ushort Sector;
        public readonly uint Generation;

        #endregion Properties

        #region Constructors

        internal JET_LGPOS(byte[] bytes)
        {
            Block = BitConverter.ToUInt16(bytes, 0x00);
            Sector = BitConverter.ToUInt16(bytes, 0x02);
            Generation = BitConverter.ToUInt32(bytes, 0x04);
        }

        #endregion Constructors
    }
    
    #endregion JET_LGPOSClass
}
