using System;
using System.Text;
using PowerForensics.Ntfs;
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

        public readonly uint Checksum;
        public readonly uint Signature;
        public readonly uint FileFormatVersion;
        public readonly JET_FILETYPE FileType;
        public readonly JetDbTime DatabaseTime;
        public readonly JetSignature DatabaseSignature;
        public readonly JET_DBSTATE DatabaseState;
        public readonly JetLgPos ConsistentPostition;
        public readonly DateTime ConsistentTime;
        public readonly DateTime AttachTime;
        public readonly JetLgPos AttachPosition;
        public readonly DateTime DetachTime;
        public readonly JetLgPos DetachPosition;
        public readonly JetSignature LogSignature;
        public readonly JetBkInfo PreviousFullBackup;
        public readonly JetBkInfo PreviousIncrementalBackup;
        public readonly JetBkInfo CurrentFullBackup;
        public readonly uint ShadowingDisabled;
        public readonly uint LastObjectIdentifier;
        public readonly uint MajorVersion;
        public readonly uint MinorVersion;
        public readonly uint BuildNumber;
        public readonly uint ServicePackNumber;
        public readonly uint FileFormatRevision;
        public readonly uint PageSize;
        public readonly uint RepairCount;
        public readonly DateTime RepairTime;
        public readonly JetDbTime ScrubDatabaseTime;
        public readonly DateTime ScrubTime;
        public readonly uint RequiredLog;
        public readonly uint UpgradeExchangeFormat;
        public readonly uint UpgradeFreePages;
        public readonly uint UpgradeSpaceMapPages;
        public readonly JetBkInfo CurrentShadowCopyBackup;
        public readonly uint CreationFileFormatVersion;
        public readonly uint CreationFileFormatRevision;
        public readonly uint OldRepairCount;
        public readonly uint EccFixSuccessCount;
        public readonly DateTime LastEccFixTime;
        public readonly uint OldEccFixSuccessCount;
        public readonly uint EccFixErrorCount;
        public readonly DateTime LastEccFixErrorTime;
        public readonly uint OldEccFixErrorCount;
        public readonly uint BadChecksumErrorCount;
        public readonly DateTime LastBadChecksumTime;
        public readonly uint OldBadChecksumErrorCount;
        public readonly uint CommittedLog;
        public readonly JetBkInfo PreviousShadowBackup;
        public readonly JetBkInfo PreviousDifferentialBackup;
        public readonly uint NlsMajorVersion;
        public readonly uint NlsMinorVersion;

        #endregion Properties

        #region Constructors

        internal JetHeader(byte[] bytes)
        {
            Checksum = BitConverter.ToUInt32(bytes, 0x00);
            Signature = BitConverter.ToUInt32(bytes, 0x04);
            FileFormatVersion = BitConverter.ToUInt32(bytes, 0x08);
            FileType = (JET_FILETYPE)BitConverter.ToUInt32(bytes, 0x0C);
            DatabaseTime = new JetDbTime(bytes, 0x10);
            DatabaseSignature = new JetSignature(bytes, 0x18);
            DatabaseState = (JET_DBSTATE)BitConverter.ToUInt32(bytes, 0x34);
            ConsistentPostition = new JetLgPos(bytes, 0x38);
            ConsistentTime = JetLogTime.Get(bytes, 0x40);
            AttachTime = JetLogTime.Get(bytes, 0x48);
            AttachPosition = new JetLgPos(bytes, 0x50);
            DetachTime = JetLogTime.Get(bytes, 0x58);
            DetachPosition = new JetLgPos(bytes, 0x60);
            LogSignature = new JetSignature(bytes, 0x68);
            PreviousFullBackup = new JetBkInfo(bytes, 0x88);
            PreviousIncrementalBackup = new JetBkInfo(bytes, 0xA0);
            CurrentFullBackup = new JetBkInfo(bytes, 0xB8);
            ShadowingDisabled = BitConverter.ToUInt32(bytes, 0xD0);
            LastObjectIdentifier = BitConverter.ToUInt32(bytes, 0xD4);
            MajorVersion = BitConverter.ToUInt32(bytes, 0xD8);
            MinorVersion = BitConverter.ToUInt32(bytes, 0xDC);
            BuildNumber = BitConverter.ToUInt32(bytes, 0xE0);
            ServicePackNumber = BitConverter.ToUInt32(bytes, 0xE4);
            FileFormatRevision = BitConverter.ToUInt32(bytes, 0xE8);
            PageSize = BitConverter.ToUInt32(bytes, 0xEC);
            RepairCount = BitConverter.ToUInt32(bytes, 0xF0);
            RepairTime = JetLogTime.Get(bytes, 0xF4);
            ScrubDatabaseTime = new JetDbTime(bytes, 0x118);
            ScrubTime = JetLogTime.Get(bytes, 0x120);
            RequiredLog = BitConverter.ToUInt32(bytes, 0x128);
            UpgradeExchangeFormat = BitConverter.ToUInt32(bytes, 0x130);
            UpgradeFreePages = BitConverter.ToUInt32(bytes, 0x134);
            UpgradeSpaceMapPages = BitConverter.ToUInt32(bytes, 0x138);
            CurrentShadowCopyBackup = new JetBkInfo(bytes, 0x13C);
            CreationFileFormatVersion = BitConverter.ToUInt32(bytes, 0x154);
            CreationFileFormatRevision = BitConverter.ToUInt32(bytes, 0x158);
            OldRepairCount = BitConverter.ToUInt32(bytes, 0x16C);            
            EccFixSuccessCount = BitConverter.ToUInt32(bytes, 0x170);
            LastEccFixTime = JetLogTime.Get(bytes, 0x174);
            OldEccFixSuccessCount = BitConverter.ToUInt32(bytes, 0x17C);
            EccFixErrorCount = BitConverter.ToUInt32(bytes, 0x180);
            LastEccFixErrorTime = JetLogTime.Get(bytes, 0x184);
            OldEccFixErrorCount = BitConverter.ToUInt32(bytes, 0x18C);
            BadChecksumErrorCount = BitConverter.ToUInt32(bytes, 0x190);
            LastBadChecksumTime = JetLogTime.Get(bytes, 0x194);
            OldBadChecksumErrorCount = BitConverter.ToUInt32(bytes, 0x19C);
            CommittedLog = BitConverter.ToUInt32(bytes, 0x1A0);
            PreviousShadowBackup = new JetBkInfo(bytes, 0x1A4);
            PreviousDifferentialBackup = new JetBkInfo(bytes, 0x1BC);
            NlsMajorVersion = BitConverter.ToUInt32(bytes, 0x1FC);
            NlsMinorVersion = BitConverter.ToUInt32(bytes, 0x200);
        }

        #endregion Constructors

        #region StaticMethods

        public static JetHeader Get(string path)
        {
            FileRecord record = FileRecord.Get(path, true);
            byte[] bytes = record.GetContent();
            return new JetHeader(bytes);
        }

        #endregion StaticMethods
    }

    #endregion HeaderClass

    #region JET_LOGTIMEClass

    internal class JetLogTime
    {
        internal static DateTime Get(byte[] bytes, int offset)
        {
            try
            {
                return new DateTime((bytes[offset + 0x05] + 1900), bytes[offset + 0x04], bytes[offset + 0x03], bytes[offset + 0x02], bytes[offset + 0x01], bytes[offset + 0x00]);
            }
            catch
            {
                return new DateTime(1);
            }
        }
    }

    #endregion JET_LOGTIMEClass

    #region JET_DBTIMEClass

    public class JetDbTime
    {
        #region Properties

        public readonly ushort Hours;
        public readonly ushort Minutes;
        public readonly ushort Seconds;

        #endregion Properties

        #region Constructors

        internal JetDbTime(byte[] bytes, int offset)
        {
            Hours = BitConverter.ToUInt16(bytes, offset + 0x00);
            Minutes = BitConverter.ToUInt16(bytes, offset + 0x02);
            Seconds = BitConverter.ToUInt16(bytes, offset + 0x04);
        }

        #endregion Constructors

        public override string ToString()
        {
            return String.Format("{0}:{1}:{2}", this.Hours, this.Minutes, this.Seconds);
        }
    }

    #endregion JET_DBTIMEClass

    #region JET_SIGNATUREClass

    public class JetSignature
    {
        #region Properties

        public readonly uint rand;
        public readonly DateTime CreationTime;
        public readonly string NetBIOSName;

        #endregion Properties

        #region Constructors

        internal JetSignature(byte[] bytes, int offset)
        {
            rand = BitConverter.ToUInt32(bytes, offset + 0x00);
            CreationTime = JetLogTime.Get(bytes, offset + 0x04);
            NetBIOSName = Encoding.ASCII.GetString(bytes, offset + 0x0C, 0x10);
        }

        #endregion Constructors

        public override string ToString()
        {
            return this.NetBIOSName;
        }
    }

    #endregion JET_SIGNATUREClass

    #region JET_BKINFOClass

    public class JetBkInfo
    {
        #region Properties

        public readonly JetLgPos BackupPosition;
        public readonly DateTime BackupTime;
        public readonly byte[] GenerationLow;
        public readonly byte[] GenerationHi;

        #endregion Properties

        #region Constructors

        internal JetBkInfo(byte[] bytes, int offset)
        {
            BackupPosition = new JetLgPos(bytes, offset + 0x00);
            BackupTime = JetLogTime.Get(bytes, offset + 0x08);
        }

        #endregion Constructors
    }

    #endregion JET_BKINFOClass

    #region JET_LGPOSClass

    public class JetLgPos
    {
        #region Properties

        public readonly ushort Block;
        public readonly ushort Sector;
        public readonly uint Generation;

        #endregion Properties

        #region Constructors

        internal JetLgPos(byte[] bytes, int offset)
        {
            Block = BitConverter.ToUInt16(bytes, offset + 0x00);
            Sector = BitConverter.ToUInt16(bytes, offset + 0x02);
            Generation = BitConverter.ToUInt32(bytes, offset + 0x04);
        }

        #endregion Constructors
    }
    
    #endregion JET_LGPOSClass
}
