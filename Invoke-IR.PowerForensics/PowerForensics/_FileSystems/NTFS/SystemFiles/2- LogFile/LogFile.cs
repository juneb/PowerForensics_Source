using System;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace InvokeIR.PowerForensics.NTFS
{
    public class LogFile
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
        public readonly ushort RedoOP;
        public readonly ushort UndoOP;
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

        internal LogFile(byte[] bytes)
        {
            LSN = BitConverter.ToUInt64(bytes, 0);
            PreviousLSN = BitConverter.ToUInt64(bytes, 8);
            ClientUndoLSN = BitConverter.ToUInt64(bytes, 16);
            ClientDataLength = BitConverter.ToUInt32(bytes, 24);
            ClientID = BitConverter.ToUInt32(bytes, 28);
            RecordType= BitConverter.ToUInt32(bytes, 32);
            TransactionID = BitConverter.ToUInt32(bytes, 36);
            Flags = BitConverter.ToUInt16(bytes, 40);
            RedoOP = BitConverter.ToUInt16(bytes, 48);
            UndoOP = BitConverter.ToUInt16(bytes, 50);
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

        public static byte[] getBytes(string volume)
        {

            byte[] mftBytes = MasterFileTable.GetBytes(volume);

            MFTRecord logFileRecord = MFTRecord.Get(mftBytes, 2, null, null);

            NonResident data = null;

            foreach (Attr attr in logFileRecord.Attribute)
            {
                if (attr.Name == "DATA")
                {
                    data = attr as NonResident;
                    break;
                }
            }

            return (NonResident.GetContent(volume, data)).ToArray();
        }
    }
}
