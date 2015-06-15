using System;
using System.Text;

namespace InvokeIR.PowerForensics
{
    public class GuidPartitionTableEntry
    {

        #region Enums

        /*enum PARTITION_TYPE_GUID : string
        {
            UNUSED_ENTRY = "00000000-0000-0000-0000-000000000000",
            EFI_SYSTEM_PARTITION = "C12A7328-F81F-11D2-BA4B-00A0C93EC93B",
            LEGACY_MBR = "024DEE41-33E7-11D3-9D69-0008C781F39F"
        }*/

        [FlagsAttribute]
        enum PARTITION_ATTRIBUTE
        {
            RequirePartition = 0x01,
            NoBlockIOProtocol = 0x02,
            LegacyBIOSBootable = 0x04
        }

        #endregion Enums

        #region Properties

        public readonly Guid PartitionTypeGUID;
        public readonly Guid UniquePartitionGUID;
        public readonly ulong StartingLBA;
        public readonly ulong EndingLBA;
        public readonly string Attributes;
        public readonly string PartitionName;

        #endregion Properties

        #region Constructors

        internal GuidPartitionTableEntry(byte[] bytes)
        {
            // Get PartitionTypeGuid Value
            byte[] partitionTypeGuidBytes = new byte[16];
            Array.Copy(bytes, 0, partitionTypeGuidBytes, 0, partitionTypeGuidBytes.Length);
            PartitionTypeGUID = new Guid(partitionTypeGuidBytes);
            
            // Get UniquePartitionGuid Value
            byte[] uniquePartitionGuidBytes = new byte[16];
            Array.Copy(bytes, 16, uniquePartitionGuidBytes, 0, uniquePartitionGuidBytes.Length);
            UniquePartitionGUID = new Guid(uniquePartitionGuidBytes);

            StartingLBA = BitConverter.ToUInt64(bytes, 32);
            EndingLBA = BitConverter.ToUInt64(bytes, 40);
            
            // Get Attributes Value
            Attributes = ((PARTITION_ATTRIBUTE)(BitConverter.ToUInt64(bytes, 48))).ToString();

            // Get the name of the Partition
            byte[] nameBytes = new byte[72];
            Array.Copy(bytes, 56, nameBytes, 0, nameBytes.Length);
            PartitionName = Encoding.Unicode.GetString(nameBytes).TrimEnd('\0');
        }

        #endregion Constructors

    }
}
