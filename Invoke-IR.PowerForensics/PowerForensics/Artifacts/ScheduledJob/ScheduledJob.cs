using System;
using System.Text;
using InvokeIR.PowerForensics.NTFS;

namespace InvokeIR.PowerForensics.Artifacts
{
    // https://msdn.microsoft.com/en-us/library/cc248285.aspx
    public class ScheduledJob
    {
        #region Enums

        enum PRODUCT_VERSION
        {
            WindowsNT4 = 0x0400,
            Windows2000 = 0x0500,
            WindowsXP = 0x0501,
            WindowsVista = 0x0600,
            Windows7 = 0x0601,
            Windows8 = 0x0602,
            Windows8_1 = 0x0603
        }

        [FlagsAttribute]
        enum PRIORITY_CLASS
        {
            NORMAL = 0x4000000,
            IDLE = 0x2000000,
            HIGH = 0x1000000,
            REALTIME = 0x800000
        }

        enum STATUS
        {
            SCHED_S_TASK_READY = 0x00041300,
            SCHED_S_TASK_RUNNING = 0x00041301,
            SCHED_S_TASK_NOT_SCHEDULED = 0x00041305
        }

        [FlagsAttribute]
        enum TASK_FLAG : uint
        {
            INTERACTIVE = 0x80000000,
            DELETE_WHEN_DONE = 0x40000000,
            DISABLED = 0x20000000,
            START_ONLY_IF_IDLE = 0x8000000,
            KILL_ON_IDLE_END = 0x4000000,
            DONT_START_IF_ON_BATTERIES = 0x2000000,
            KILL_IF_GOING_ON_BATTERIES = 0x1000000,
            RUN_ONLY_IF_DOCKED = 0x800000,
            HIDDEN = 0x400000,
            RUN_IF_CONNECTED_TO_INTERNET = 0x200000,
            RESTART_ON_IDLE_RESUME = 0x100000,
            SYSTEM_REQUIRED = 0x80000,
            RUN_ONLY_IF_LOGGED_ON = 0x40000,
            APPLICATION_NAME = 0x80,
        }

        #endregion Enums

        #region Properties

        // FIXDLEN_DATA
        public readonly string ProductVersion;
        public readonly ushort FileVersion;
        public readonly Guid UUID;
        private readonly ushort ApplicationNameOffset;
        private readonly ushort TriggerOffset;
        public readonly ushort ErrorRetryCount;
        public readonly ushort ErrorRetryInterval;
        public readonly ushort IdleDeadline;
        public readonly ushort IdleWait;
        //public readonly string Priority;
        public readonly uint MaximumRuntime;
        public readonly uint ExitCode;
        public readonly string Status;
        public readonly string Flags;
        public readonly DateTime RunTime;

        // Variable-Length Data Section
        public readonly ushort RunningInstanceCount;
        private readonly ushort ApplicationNameLength;
        public readonly string ApplicationName;
        private readonly ushort ParameterLength;
        public readonly string Parameters;
        private readonly ushort WorkingDirectoryLength;
        public readonly string WorkingDirectory;
        private readonly ushort AuthorLength;
        public readonly string Author;
        private readonly ushort CommentLength;
        public readonly string Comment;
        public readonly DateTime StartTime;

        #endregion Properties

        #region Constructors

        internal ScheduledJob(byte[] bytes)
        {
            // FIXDLEN_DATA
            ProductVersion = Enum.GetName(typeof(PRODUCT_VERSION), BitConverter.ToUInt16(bytes, 0x00));
            FileVersion = BitConverter.ToUInt16(bytes, 0x02);
            
            #region UUID

            int Data1 = BitConverter.ToInt32(bytes, 0x04);
            short Data2 = BitConverter.ToInt16(bytes, 0x08);
            short Data3 = BitConverter.ToInt16(bytes, 0x0A);
            byte[] Data4 = new byte[8];
            Array.Copy(bytes, 0x0C, Data4, 0, Data4.Length);
            UUID = new Guid(Data1, Data2, Data3, Data4);

            #endregion UUID

            ApplicationNameOffset = BitConverter.ToUInt16(bytes, 0x14);
            TriggerOffset = BitConverter.ToUInt16(bytes, 0x16);
            ErrorRetryCount = BitConverter.ToUInt16(bytes, 0x18);
            ErrorRetryInterval = BitConverter.ToUInt16(bytes, 0x1A);
            IdleDeadline = BitConverter.ToUInt16(bytes, 0x1C);
            IdleWait = BitConverter.ToUInt16(bytes, 0x1E);
            //Priority = ((PRIORITY_CLASS)BitConverter.ToUInt32(bytes, 0x20)).ToString();
            MaximumRuntime = BitConverter.ToUInt32(bytes, 0x24);
            ExitCode = BitConverter.ToUInt32(bytes, 0x28);
            Status = Enum.GetName(typeof(STATUS), BitConverter.ToUInt32(bytes, 0x2C));
            Flags = ((TASK_FLAG)BitConverter.ToUInt32(bytes, 0x30)).ToString();
            
            #region RunTime

            short year = BitConverter.ToInt16(bytes, 0x34);
            short month = BitConverter.ToInt16(bytes, 0x36);
            short day = BitConverter.ToInt16(bytes, 0x3A);
            short hour = BitConverter.ToInt16(bytes, 0x3C);
            short minute = BitConverter.ToInt16(bytes, 0x3E);
            short second = BitConverter.ToInt16(bytes, 0x40);
            short milliseconds = BitConverter.ToInt16(bytes, 0x42);
            RunTime = new DateTime(year, month, day, hour, minute, second, milliseconds, DateTimeKind.Utc);

            #endregion RunTime
            

            // Variable-Length Data Section
            RunningInstanceCount = BitConverter.ToUInt16(bytes, 0x44);
            
            #region ApplicationName
            
            ApplicationNameLength = BitConverter.ToUInt16(bytes, ApplicationNameOffset);
            byte[] appNameBytes = new byte[ApplicationNameLength * 2];
            Array.Copy(bytes, ApplicationNameOffset + 0x02, appNameBytes, 0, appNameBytes.Length);
            ApplicationName = Encoding.Unicode.GetString(appNameBytes).Split('\0')[0];
            
            #endregion ApplicationName

            #region Parameters

            int parameterOffset = ApplicationNameOffset + 0x02 + (ApplicationNameLength * 2);
            ParameterLength = BitConverter.ToUInt16(bytes, parameterOffset);
            byte[] parameterBytes = new byte[ParameterLength * 2];
            Array.Copy(bytes, parameterOffset + 0x02, parameterBytes, 0, parameterBytes.Length);
            Parameters = Encoding.Unicode.GetString(parameterBytes).Split('\0')[0];

            #endregion Parameters

            #region WorkingDirectory

            int workingdirectoryOffset = parameterOffset + 0x02 + (ParameterLength * 2);
            WorkingDirectoryLength = BitConverter.ToUInt16(bytes, workingdirectoryOffset);
            byte[] workingdirectoryBytes = new byte[WorkingDirectoryLength * 2];
            Array.Copy(bytes, workingdirectoryOffset + 0x02, workingdirectoryBytes, 0, workingdirectoryBytes.Length);
            WorkingDirectory = Encoding.Unicode.GetString(workingdirectoryBytes).Split('\0')[0];

            #endregion WorkingDirectory

            #region AuthorLength

            int authorOffset = workingdirectoryOffset + 0x02 + (WorkingDirectoryLength * 2);
            AuthorLength = BitConverter.ToUInt16(bytes, authorOffset);
            byte[] authorBytes = new byte[AuthorLength * 2];
            Array.Copy(bytes, authorOffset + 0x02, authorBytes, 0, authorBytes.Length);
            Author = Encoding.Unicode.GetString(authorBytes).Split('\0')[0];

            #endregion AuthorLength

            #region Comment

            int commentOffset = authorOffset + 0x02 + (AuthorLength * 2);
            CommentLength = BitConverter.ToUInt16(bytes, commentOffset);
            byte[] commentBytes = new byte[CommentLength * 2];
            Array.Copy(bytes, commentOffset + 0x02, commentBytes, 0, commentBytes.Length);
            Comment = Encoding.Unicode.GetString(commentBytes).Split('\0')[0];

            #endregion Comment

            #region StartTime

            year = BitConverter.ToInt16(bytes, TriggerOffset + 0x06);
            month = BitConverter.ToInt16(bytes, TriggerOffset + 0x08);
            day = BitConverter.ToInt16(bytes, TriggerOffset + 0x0A);
            hour = BitConverter.ToInt16(bytes, TriggerOffset + 0x12);
            minute = BitConverter.ToInt16(bytes, TriggerOffset + 0x14);
            StartTime = new DateTime(year, month, day, hour, minute, 0, DateTimeKind.Utc);

            #endregion StartTime
        }

        #endregion Constructors

        internal static ScheduledJob Get(string path)
        {
            string volume = "\\\\.\\" + path.Split('\\')[0];
            IndexEntry index = IndexEntry.Get(path);
            FileRecord record = new FileRecord(FileRecord.GetRecordBytes(volume, (int)index.RecordNumber), volume);
            return new ScheduledJob(record.GetBytes(volume));
        }

        internal static ScheduledJob Get(string volume, int recordNumber)
        {
            FileRecord record = new FileRecord(FileRecord.GetRecordBytes(volume, recordNumber), volume);
            return new ScheduledJob(record.GetBytes(volume));
        }
    }
}
