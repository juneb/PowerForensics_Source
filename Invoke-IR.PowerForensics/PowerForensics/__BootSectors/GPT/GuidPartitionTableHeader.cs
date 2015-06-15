using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InvokeIR.PowerForensics
{
    public class GuidPartitionTableHeader
    {
        #region Constants

        private const string SIGNATURE_STRING = "EFI PART";

        #endregion Constants

        #region Properties

        internal readonly string Signature;
        internal readonly Version Revision;
        internal readonly uint HeaderSize;
        internal readonly uint HeaderCRC32;
        internal readonly ulong MyLBA;
        internal readonly ulong AlternateLBA;
        internal readonly ulong FirstUsableLBA;
        internal readonly ulong LastUsableLBA;
        internal readonly Guid DiskGUID;
        internal readonly ulong PartitionEntryLBA;
        internal readonly uint NumberOfPartitionEntries;
        internal readonly uint SizeOfPartitionEntry;
        internal readonly byte[] PartitionEntryArrayCRC32;

        #endregion Properties

        #region Constructors

        internal GuidPartitionTableHeader(byte[] bytes)
        {
            // Test GPT Signature
            byte[] SignatureArray = new byte[8];
            Array.Copy(bytes, SignatureArray, 8);
            Signature = Encoding.ASCII.GetString(SignatureArray);
            if (Signature != SIGNATURE_STRING)
            {
                throw new Exception("Invalid GPT Signature");
            }
            
            // Get Revision Number
            ushort revMajor = BitConverter.ToUInt16(bytes, 8);
            ushort revMinor = BitConverter.ToUInt16(bytes, 10);
            Revision = new Version(revMajor, revMinor);
            
            // Get Header Size
            HeaderSize = BitConverter.ToUInt32(bytes, 12);
            
            // Get HeaderCRC32 Value

            // Get/Test MyLBA
            MyLBA = BitConverter.ToUInt64(bytes, 24);
            if (MyLBA != 1)
            {
                throw new Exception("Invalid MyLBA property value");
            }

            AlternateLBA = BitConverter.ToUInt64(bytes, 32);            
            FirstUsableLBA = BitConverter.ToUInt64(bytes, 40);
            LastUsableLBA = BitConverter.ToUInt64(bytes, 48);
            
            // Get DiskGUID Value
            byte[] diskGUIDBytes = new byte[16];
            Array.Copy(bytes, 56, diskGUIDBytes, 0, diskGUIDBytes.Length);
            DiskGUID = new Guid(diskGUIDBytes);
            
            PartitionEntryLBA = BitConverter.ToUInt64(bytes, 72);
            NumberOfPartitionEntries = BitConverter.ToUInt32(bytes, 80);            
            SizeOfPartitionEntry = BitConverter.ToUInt32(bytes, 84);
            
            // Get PartitionEntryArrayCRC32 Value
        }

        #endregion Constructors
    }
}
