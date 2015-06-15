using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using InvokeIR.Win32;

namespace InvokeIR.PowerForensics
{
    #region MasterBootRecordClass

    class MasterBootRecord
    {

        #region MBRSignatures

        private const string WINDOWS5_X = "8F558EB6672622401DA993E1E865C861";
        private const string WINDOWS6_0 = "5C616939100B85E558DA92B899A0FC36";
        private const string WINDOWS6_1 = "A36C5E4F47E84449FF07ED3517B43A31";
        //private const string LILO = "";
        private const string GRUB = "A6C7E63CA46F1CB2307E0F10AD897BDE";
        private const string NYANCAT = "B40C0E49689A0ABD2A51379FED1800F3";
        private const string STONEDv2 = "72B8CE41AF0DE751C946802B3ED844B4";
        private const string STONEDv2_TRUE_CRYPT = "5C7DE5F58B276CBE84B8B7E25F08318E";

        #endregion MBRSignatures

        #region Constants

        private const int PARTITION_ENTRY_SIZE = 16;

        #endregion Constants

        #region Properties

        public readonly string MBRSignature;
        public readonly string DiskSignature;
        public readonly byte[] BootCode;
        public readonly PartitionEntry[] PartitionTable;

        #endregion Properties

        #region Constructor

        public MasterBootRecord(string devicePath)
        {
            // Check drivePath parameter
            NativeMethods.getDriveName(devicePath);

            // Get Handle to Hard Drive
            IntPtr hDrive = NativeMethods.getHandle(devicePath);
            
            // Create a FileStream to read from hDrive
            using (FileStream streamToRead = NativeMethods.getFileStream(hDrive))
            {
                // Read Master Boot Record (first 512 bytes) from disk
                byte[] MBRBytes = NativeMethods.readDrive(streamToRead, 0, 512);

                // Instantiate a byte array to hold 440 bytes (size of MBR Boot Code)
                // Copy MBR sub-array into mbrCode
                byte[] mbrCode = new byte[440];
                Array.Copy(MBRBytes, 0, mbrCode, 0, mbrCode.Length);

                // Check MBR Code Section against a list of known signatures
                #region MD5Signature

                string MD5Signature = null;

                switch (Hash.Get(mbrCode, mbrCode.Length, "MD5"))
                {
                    case WINDOWS5_X:
                        MD5Signature = "Windows 5.X";
                        break;
                    case WINDOWS6_0:
                        MD5Signature = "Windows 6.0";
                        break;
                    case WINDOWS6_1:
                        MD5Signature = "Windows 6.1+";
                        break;
                    case GRUB:
                        MD5Signature = "GRUB";
                        break;
                    case NYANCAT:
                        MD5Signature = "BOOTKIT Nyan Cat";
                        break;
                    case STONEDv2:
                        MD5Signature = "BOOTKIT Stonedv2";
                        break;
                    case STONEDv2_TRUE_CRYPT:
                        MD5Signature = "BOOTKIT Stonedv2";
                        break;
                    default:
                        MD5Signature = "UNKNOWN";
                        break;
                }

                #endregion MD5Signature

                // Instantiate a blank Partition List
                List<PartitionEntry> partitionList = new List<PartitionEntry>();

                // Set object properties
                BootCode = mbrCode;
                DiskSignature = BitConverter.ToString(MBRBytes.Skip(440).Take(4).ToArray()).Replace("-", "");
                MBRSignature = MD5Signature;

                for (int i = 446; i <= 478; i += PARTITION_ENTRY_SIZE)
                {
                    PartitionEntry entry = new PartitionEntry(MBRBytes.Skip(i).Take(PARTITION_ENTRY_SIZE).ToArray());
                    if (entry.SystemID != "EMPTY")
                    {
                        partitionList.Add(entry);
                    }
                }

                PartitionEntry entry4 = new PartitionEntry(MBRBytes.Skip(494).Take(PARTITION_ENTRY_SIZE).ToArray());
                if (entry4.SystemID == "MS_EXTENDED_LBA")
                {
                    List<PartitionEntry> pList = GetExtended(streamToRead, entry4.StartSector);
                    partitionList.AddRange(pList);
                }
                else if (entry4.SystemID != "EMPTY")
                {
                    partitionList.Add(entry4);
                }

                PartitionTable = partitionList.ToArray();
            }
        }

        internal MasterBootRecord(FileStream streamToRead)
        {
            // Read Master Boot Record (first 512 bytes) from disk
            byte[] MBRBytes = NativeMethods.readDrive(streamToRead, 0, 512);

            // Instantiate a byte array to hold 440 bytes (size of MBR Boot Code)
            // Copy MBR sub-array into mbrCode
            byte[] mbrCode = new byte[440];
            Array.Copy(MBRBytes, 0, mbrCode, 0, mbrCode.Length);

            // Check MBR Code Section against a list of known signatures
            #region MD5Signature

            string MD5Signature = null;

            switch (Hash.Get(mbrCode, mbrCode.Length, "MD5"))
            {
                case WINDOWS5_X:
                    MD5Signature = "Windows 5.X";
                    break;
                case WINDOWS6_0:
                    MD5Signature = "Windows 6.0";
                    break;
                case WINDOWS6_1:
                    MD5Signature = "Windows 6.1+";
                    break;
                case GRUB:
                    MD5Signature = "GRUB";
                    break;
                case NYANCAT:
                    MD5Signature = "BOOTKIT Nyan Cat";
                    break;
                case STONEDv2:
                    MD5Signature = "BOOTKIT Stonedv2";
                    break;
                case STONEDv2_TRUE_CRYPT:
                    MD5Signature = "BOOTKIT Stonedv2";
                    break;
                default:
                    MD5Signature = "UNKNOWN";
                    break;
            }

            #endregion MD5Signature

            // Instantiate a blank Partition List
            List<PartitionEntry> partitionList = new List<PartitionEntry>();

            // Set object properties
            BootCode = mbrCode;
            DiskSignature = BitConverter.ToString(MBRBytes.Skip(440).Take(4).ToArray()).Replace("-", "");
            MBRSignature = MD5Signature;
            partitionList.Add(new PartitionEntry(MBRBytes.Skip(446).Take(16).ToArray()));
            partitionList.Add(new PartitionEntry(MBRBytes.Skip(462).Take(16).ToArray()));
            partitionList.Add(new PartitionEntry(MBRBytes.Skip(478).Take(16).ToArray()));
            partitionList.Add(new PartitionEntry(MBRBytes.Skip(494).Take(16).ToArray()));
            PartitionTable = partitionList.ToArray();
        }

        #endregion Constructor

        #region PublicMethods

        public static MasterBootRecord Get(string devicePath)
        {
            // Check devicePath parameter
            NativeMethods.getDriveName(devicePath);

            // Return a MasterBootRecord object for the given device path
            return new MasterBootRecord(devicePath);
        }

        public static byte[] GetBytes(string devicePath)
        {
            // Check devicePath parameter
            NativeMethods.getDriveName(devicePath);

            // Get Handle to Hard Drive
            IntPtr hDrive = NativeMethods.getHandle(devicePath);
            
            // Create a FileStream to read from hDrive
            using (FileStream streamToRead = NativeMethods.getFileStream(hDrive))
            {
                // Read Master Boot Record (first 512 bytes) from disk
                 return NativeMethods.readDrive(streamToRead, 0, 512);
            }
        }

        public static List<PartitionEntry> GetExtended(FileStream streamToRead, uint startSector)
        {
            List<PartitionEntry> pList = new List<PartitionEntry>();
            
            ulong offset = 512 * (ulong)startSector;

            byte[] extendedMBR = NativeMethods.readDrive(streamToRead, offset, 512);

            pList.Add(new PartitionEntry(extendedMBR.Skip(446).Take(16).ToArray(), startSector));
            PartitionEntry secondEntry = new PartitionEntry(extendedMBR.Skip(462).Take(16).ToArray(), startSector);
            pList.Add(secondEntry);

            if(secondEntry.SystemID == "MS_EXTENDED")
            {
                pList.AddRange(GetExtended(streamToRead, secondEntry.StartSector));
            }
            
            return pList;
        }

        #endregion PublicMethods

    }

    #endregion MasterBootRecordClass
}
