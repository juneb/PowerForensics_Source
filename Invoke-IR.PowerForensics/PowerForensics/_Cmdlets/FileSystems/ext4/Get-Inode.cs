using System;
using System.IO;
using System.Management.Automation;
using InvokeIR.Win32;
using PowerForensics.Ext3;

namespace PowerForensics.Cmdlets
{
    #region GetInodeCommand
    
    /// <summary> 
    /// This class implements the Get-Inode cmdlet. 
    /// </summary> 
    [Cmdlet(VerbsCommon.Get, "Inode")]
    public class GetInodeCommand : PSCmdlet
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
        /// 
        /// </summary> 
        protected override void BeginProcessing()
        {
            // Ensure cmdlet is being run as Administrator
            NativeMethods.checkAdmin();

            // Check that drivePath is valid
            NativeMethods.getDriveName(devicePath);
        }

        /// <summary> 
        /// 
        /// </summary> 
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

                if (this.MyInvocation.BoundParameters.ContainsKey("Inode"))
                {
                    if (inode == 0)
                    {
                        throw new Exception("0 is not a valid Inode value.");
                    }
                    else
                    {
                        uint block_group = (inode - 1) / superBlock.InodesPerGroup;
                        
                        BlockGroupDescriptor bgd = new BlockGroupDescriptor(BlockGroupDescriptorTable.GetBytes(streamToRead, superblockOffset, superBlock));
                        
                        uint inodeTableOffset = (superblockOffset * NativeMethods.BYTES_PER_SECTOR) + (bgd.InodeTableOffset * superBlock.BlockSize);
                        
                        uint inodeSectorOffset = (inode - 1) / (NativeMethods.BYTES_PER_SECTOR / (uint)superBlock.InodeSize);

                        byte[] SectorBytes = NativeMethods.readDrive(streamToRead, inodeTableOffset + (inodeSectorOffset * NativeMethods.BYTES_PER_SECTOR), NativeMethods.BYTES_PER_SECTOR);

                        uint sectorOffset = ((inode - 1) % (NativeMethods.BYTES_PER_SECTOR / (uint)superBlock.InodeSize)) * (uint)superBlock.InodeSize;

                        byte[] inodeBytes = NativeMethods.GetSubArray(SectorBytes, sectorOffset, (uint)superBlock.InodeSize);

                        if (asbytes)
                        {
                            WriteObject(inodeBytes);
                        }
                        else
                        {
                            WriteObject(new Inode(inodeBytes, inode));
                        }
                    }
                }
                else
                {
                    // Create a byte array representing the BGDT
                    byte[] bgdtBytes = BlockGroupDescriptorTable.GetBytes(streamToRead, superblockOffset, superBlock);

                    // Iterate through BGDTs and output associate Inodes
                    for (uint o = 0; o < bgdtBytes.Length; o += BlockGroupDescriptor.BLOCK_GROUP_DESCRIPTOR_LENGTH)
                    {
                        BlockGroupDescriptor bgd = new BlockGroupDescriptor(NativeMethods.GetSubArray(bgdtBytes, o, BlockGroupDescriptor.BLOCK_GROUP_DESCRIPTOR_LENGTH));

                        uint inodeTableOffset = (superblockOffset * NativeMethods.BYTES_PER_SECTOR) + (bgd.InodeTableOffset * superBlock.BlockSize);
                        byte[] inodeTableBytes = InodeTable.GetBytes(streamToRead, superBlock, inodeTableOffset);

                        for (uint i = 0; i < inodeTableBytes.Length; i += (uint)superBlock.InodeSize)
                        {
                            uint inode = ((o / BlockGroupDescriptor.BLOCK_GROUP_DESCRIPTOR_LENGTH) + 1) * ((i / (uint)superBlock.InodeSize) + 1);
                            WriteObject(new Inode(NativeMethods.GetSubArray(inodeTableBytes, i, superBlock.InodeSize), inode));
                        }
                    }
                }
            }
        }

        #endregion Cmdlet Overrides
    }

    #endregion GetInodeCommand
}

