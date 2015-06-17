using System;
using System.IO;
using System.Text;
using InvokeIR.Win32;

namespace InvokeIR.PowerForensics.NTFS
{
    public class LogFile
    {
        internal static FileRecord GetFileRecord(string volume)
        {
            return new FileRecord(FileRecord.GetBytes(volume, 2));
        }

        internal static NonResident GetDataAttr(FileRecord fileRecord)
        {
            foreach (Attr attr in fileRecord.Attribute)
            {
                if (attr.Name == "DATA")
                {
                    return attr as NonResident;
                }
            }
            throw new Exception("No DATA attribute found.");
        }


        public static byte[] getBytes(string volume)
        {
            // Get handle for volume
            IntPtr hVolume = NativeMethods.getHandle(volume);

            // Get filestream based on hVolume
            using (FileStream streamToRead = NativeMethods.getFileStream(hVolume))
            {
                VolumeBootRecord VBR = new VolumeBootRecord(streamToRead);

                FileRecord logFileRecord = GetFileRecord(volume);

                NonResident data = GetDataAttr(logFileRecord);

                return NativeMethods.readDrive(streamToRead, (((ulong)data.DataRun[0].StartCluster * VBR.BytesPerCluster) + 0x4000), 0x1000);
            }
        }

        public static OperationRecord Get(string volume)
        {
            // Get Page Bytes
            byte[]pageBytes = getBytes(volume);

            // Get Page Header
            byte[] headerBytes = new byte[0x40];
            Array.Copy(pageBytes, 0, headerBytes, 0, headerBytes.Length);
            PageHeader pageHeader = new PageHeader(headerBytes);

            byte[] opsRecordBytes = new byte[0x58];
            Array.Copy(pageBytes, 0x40, opsRecordBytes, 0 , opsRecordBytes.Length);
            OperationRecord opsRecord = new OperationRecord(opsRecordBytes);
            return opsRecord;
            
            /*return pageHeader;
            
            for(int i = 0x40; i < (pageBytes.Length - (0x40 + 0x58)); i += 0x58)
            {
                new
            }*/
        }
    }

    public class RestartAreaHeader
    {
        #region Properties

        public readonly string Signature;
        public readonly ushort USOffset;
        public readonly ushort USCount;
        public readonly ulong CheckDiskLSN;
        public readonly uint SystemPageSize;
        public readonly uint LogPageSize;
        public readonly ushort RestartOffset;
        public readonly ushort MinorVersion;
        public readonly ushort MajorVersion;
        public readonly byte[] USArray;
        public readonly ulong CurrentLSN;
        public readonly uint LogClient;
        public readonly uint ClientList;
        public readonly ulong Flags;

        #endregion Properties

        #region Constructors

        internal RestartAreaHeader(byte[] bytes)
        {
            byte[] sigBytes = new byte[4];
            Array.Copy(bytes, 0, sigBytes, 0, sigBytes.Length);
            Signature = Encoding.ASCII.GetString(sigBytes); ;
            USOffset = BitConverter.ToUInt16(bytes, 4);
            USCount = BitConverter.ToUInt16(bytes, 6);
            CheckDiskLSN = BitConverter.ToUInt64(bytes, 8);
            SystemPageSize = BitConverter.ToUInt32(bytes, 16);
            LogPageSize = BitConverter.ToUInt32(bytes, 20);
            RestartOffset = BitConverter.ToUInt16(bytes, 24);
            MinorVersion = BitConverter.ToUInt16(bytes, 26);
            MajorVersion = BitConverter.ToUInt16(bytes, 28);
            //USArray = bytes.Skip(30).Take(18).ToArray();
            CurrentLSN = BitConverter.ToUInt64(bytes, 48);
            LogClient = BitConverter.ToUInt32(bytes, 56);
            ClientList = BitConverter.ToUInt32(bytes, 60);
            Flags = BitConverter.ToUInt64(bytes, 64);
        }

        #endregion Constructors

        public static RestartAreaHeader Get(string volume)
        {
            return new RestartAreaHeader(LogFile.getBytes(volume));
        }
    }

    public class Restart
    {
        public RestartAreaHeader restartHeader;

        public Restart(byte[] bytes)
        {
            byte[] restartHeaderBytes = new byte[72];
            Array.Copy(bytes, 0, restartHeaderBytes, 0, restartHeaderBytes.Length);
            restartHeader = new RestartAreaHeader(restartHeaderBytes);
            //opRecord = new LogFile(bytes.Skip(72).Take(88).ToArray());
        }

        public static Restart[] Get(byte[] bytes)
        {
            Restart[] restartArray = new Restart[2];

            byte[] restart1 = new byte[0x1000];
            Array.Copy(bytes, 0, restart1, 0, restart1.Length);

            byte[] restart2 = new byte[0x1000];
            Array.Copy(bytes, 0x1000, restart2, 0, restart2.Length);

            restartArray[0] = new Restart(restart1);
            restartArray[1] = new Restart(restart2);

            return restartArray;
        }
    }

    public class PageHeader
    {
        #region Properties

        public readonly string Signature;
        public readonly ushort USOffset;
        public readonly ushort USCount;
        public readonly ulong LastLSN;
        public readonly uint Flags;
        public readonly ushort PageCount;
        public readonly ushort PagePosition;
        public readonly ushort NextRecordOffset;
        public readonly ulong LastEndLSN;
        public readonly ushort USN;
        public readonly byte[] USArray;

        #endregion Properties

        #region Constructors

        internal PageHeader(byte[] bytes)
        {
            byte[] sigBytes = new byte[4];
            Array.Copy(bytes, 0, sigBytes, 0, sigBytes.Length);
            Signature = Encoding.ASCII.GetString(sigBytes);
            USOffset = BitConverter.ToUInt16(bytes, 4);
            USCount = BitConverter.ToUInt16(bytes, 6);
            LastLSN = BitConverter.ToUInt64(bytes, 8);
            Flags = BitConverter.ToUInt32(bytes, 16);
            PageCount = BitConverter.ToUInt16(bytes, 20);
            PagePosition = BitConverter.ToUInt16(bytes, 22);
            NextRecordOffset = BitConverter.ToUInt16(bytes, 24);
            LastLSN = BitConverter.ToUInt64(bytes, 32);
            //USN = ;
            //USArray = ;
        }

        #endregion Constructors

        public static PageHeader Get(string volume)
        {
            return new PageHeader(LogFile.getBytes(volume));
        }
    }

    public class OperationRecord
    {
        #region Enums

        internal enum OPERATION_CODE
        {
            Noop = 0x00,
            CompensationlogRecord = 0x01,
            InitializeFileRecordSegment = 0x02,
            DeallocateFileRecordSegment = 0x03,
            WriteEndofFileRecordSegement = 0x04,
            CreateAttribute = 0x05,
            DeleteAttribute = 0x06,
            UpdateResidentValue = 0x07,
            UpdataeNonResidentValue = 0x08,
            UpdateMappingPairs = 0x09,
            DeleteDirtyClusters = 0x0A,
            SetNewAttributeSizes = 0x0B,
            AddindexEntryRoot = 0x0C,
            DeleteindexEntryRoot = 0x0D,
            AddIndexEntryAllocation = 0x0F,
            SetIndexEntryVenAllocation = 0x12,
            UpdateFileNameRoot = 0x13,
            UpdateFileNameAllocation = 0x14,
            SetBitsInNonresidentBitMap = 0x15,
            ClearBitsInNonresidentBitMap = 0x16,
            PrepareTransaction = 0x19,
            CommitTransaction = 0x1A,
            ForgetTransaction = 0x1B,
            OpenNonresidentAttribute = 0x1C,
            DirtyPageTableDump = 0x1F,
            TransactionTableDump = 0x20,
            UpdateRecordDataRoot = 0x21
        }

        #endregion Enums

        #region Properties

        public readonly ulong LSN;
        public readonly ulong PreviousLSN;
        public readonly ulong ClientUndoLSN;
        public readonly uint ClientDataLength;
        public readonly uint ClientID;
        public readonly uint RecordType;
        public readonly uint TransactionID;
        public readonly ushort Flags;
        public readonly string RedoOP;
        public readonly string UndoOP;
        public readonly ushort RedoOffset;
        public readonly ushort RedoLength;
        public readonly ushort UndoOffset;
        public readonly ushort UndoLength;
        public readonly ushort TargetAttribute;
        public readonly ushort LCNtoFollow;
        public readonly ushort RecordOffset;
        public readonly ushort AttrOffset;
        public readonly ushort MFTClusterIndex;
        public readonly uint TargetVCN;
        public readonly uint TargetLCN;

        #endregion Properties

        #region Constructors

        internal OperationRecord(byte[] bytes)
        {
            LSN = BitConverter.ToUInt64(bytes, 0);
            PreviousLSN = BitConverter.ToUInt64(bytes, 8);
            ClientUndoLSN = BitConverter.ToUInt64(bytes, 16);
            ClientDataLength = BitConverter.ToUInt32(bytes, 24);
            ClientID = BitConverter.ToUInt32(bytes, 28);
            RecordType = BitConverter.ToUInt32(bytes, 32);
            TransactionID = BitConverter.ToUInt32(bytes, 36);
            Flags = BitConverter.ToUInt16(bytes, 40);
            RedoOP = Enum.GetName(typeof(OPERATION_CODE), BitConverter.ToUInt16(bytes, 48)).ToString();
            UndoOP = Enum.GetName(typeof(OPERATION_CODE), BitConverter.ToUInt16(bytes, 50)).ToString();
            RedoOffset = BitConverter.ToUInt16(bytes, 52);
            RedoLength = BitConverter.ToUInt16(bytes, 54);
            UndoOffset = BitConverter.ToUInt16(bytes, 56);
            UndoLength = BitConverter.ToUInt16(bytes, 58);
            TargetAttribute = BitConverter.ToUInt16(bytes, 60);
            LCNtoFollow = BitConverter.ToUInt16(bytes, 62);
            RecordOffset = BitConverter.ToUInt16(bytes, 64);
            AttrOffset = BitConverter.ToUInt16(bytes, 66);
            MFTClusterIndex = BitConverter.ToUInt16(bytes, 68);
            TargetVCN = BitConverter.ToUInt32(bytes, 72);
            TargetLCN = BitConverter.ToUInt32(bytes, 80);
        }

        #endregion Constructors
    }

}
