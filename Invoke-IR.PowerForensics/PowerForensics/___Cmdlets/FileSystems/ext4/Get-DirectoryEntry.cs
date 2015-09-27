using System;
using System.IO;
using System.Management.Automation;
using InvokeIR.Win32;
using InvokeIR.PowerForensics.Ext3;

namespace InvokeIR.PowerForensics.Cmdlets
{

    #region GetDirectoryEntryCommand
    /// <summary> 
    /// This class implements the Get-DirectoryEntry cmdlet. 
    /// </summary> 

    [Cmdlet(VerbsCommon.Get, "DirectoryEntry")]
    public class GetDirectoryEntryCommand : PSCmdlet
    {

        #region Parameters

        /// <summary> 
        /// This parameter provides the path for the 
        /// Superblock that will be returned.
        /// </summary> 

        [Parameter(Mandatory = true, Position = 0)]
        public string Path
        {
            get { return devicePath; }
            set { devicePath = value; }
        }
        private string devicePath;

        /// <summary> 
        /// This parameter provides the path for the 
        /// Superblock that will be returned.
        /// </summary> 

        [Parameter(Mandatory = false, Position = 1)]
        public uint Inode
        {
            get { return inode; }
            set { inode = value; }
        }
        private uint inode;

        /// <summary> 
        /// This parameter provides causes Get-Superblock
        /// to return the Superblock as a byte array.
        /// </summary> 

        [Parameter()]
        public SwitchParameter AsBytes
        {
            get { return asbytes; }
            set { asbytes = value; }
        }
        private SwitchParameter asbytes;

        #endregion Parameters

        #region Cmdlet Overrides

        /// <summary> 
        /// The ProcessRecord method instantiates a Superblock object based 
        /// on the disk given as an argument.
        /// </summary> 

        protected override void BeginProcessing()
        {
            // Ensure cmdlet is being run as Administrator
            NativeMethods.checkAdmin();

            // Check that drivePath is valid
            NativeMethods.getDriveName(devicePath);
        }

        protected override void ProcessRecord()
        {
            #region MBR

            MasterBootRecord mbr = MasterBootRecord.Get(devicePath);

            uint superblockOffset = 0;

            foreach (PartitionEntry partition in mbr.PartitionTable)
            {
                if (partition.Bootable && partition.SystemID == "LINUX")
                {
                    superblockOffset = partition.StartSector;
                }
            }

            #endregion MBR
            // Obtain a handle to the device named "devicePath"
            IntPtr hDevice = NativeMethods.getHandle(devicePath);

            using (FileStream streamToRead = NativeMethods.getFileStream(hDevice))
            {
                // Get Superblock to understand File System Layout
                Superblock superBlock = new Superblock(Superblock.GetBytes(streamToRead, superblockOffset));

                // Derive the location and length of the Block Group Descriptor Table
                uint bgdtOffset = (superblockOffset * NativeMethods.BYTES_PER_SECTOR) + ((superBlock.FirstDataBlock + 1) * superBlock.BlockSize);
                uint bgdtEntries = (superBlock.TotalBlockCount / superBlock.BlocksPerGroup) + 1;
                uint bgdtLength = bgdtEntries * BlockGroupDescriptor.BLOCK_GROUP_DESCRIPTOR_LENGTH;



            }
        } // ProcessRecord 

        protected override void EndProcessing()
        {
            GC.Collect();
        }

        #endregion Cmdlet Overrides

    } // End GetDirectoryEntryCommand class. 

    #endregion GetDirectoryEntryCommand

}

