using System;
using System.IO;
using System.Management.Automation;
using InvokeIR.Win32;
using InvokeIR.PowerForensics.ext3;

namespace InvokeIR.PowerForensics.Cmdlets
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

                        byte[] inodeBytes = new byte[(uint)superBlock.InodeSize];
                        Array.Copy(SectorBytes, sectorOffset, inodeBytes, 0, inodeBytes.Length);
                        WriteObject(new Inode(inodeBytes, inode));
                    }
                }
                else
                {
                    // Create a byte array representing the BGDT
                    byte[] bgdtBytes = BlockGroupDescriptorTable.GetBytes(streamToRead, superblockOffset, superBlock);

                    // Iterate through BGDTs and output associate Inodes
                    for (uint o = 0; o < bgdtBytes.Length; o += BlockGroupDescriptor.BLOCK_GROUP_DESCRIPTOR_LENGTH)
                    {
                        byte[] bgdBytes = new byte[BlockGroupDescriptor.BLOCK_GROUP_DESCRIPTOR_LENGTH];
                        Array.Copy(bgdtBytes, o, bgdBytes, 0, bgdBytes.Length);
                        BlockGroupDescriptor bgd = new BlockGroupDescriptor(bgdBytes);

                        uint inodeTableOffset = (superblockOffset * NativeMethods.BYTES_PER_SECTOR) + (bgd.InodeTableOffset * superBlock.BlockSize);
                        byte[] inodeTableBytes = InodeTable.GetBytes(streamToRead, superBlock, inodeTableOffset);

                        byte[] inodeBytes = new byte[superBlock.InodeSize];

                        for (uint i = 0; i < inodeTableBytes.Length; i += (uint)superBlock.InodeSize)
                        {
                            uint inode = ((o / BlockGroupDescriptor.BLOCK_GROUP_DESCRIPTOR_LENGTH) + 1) * ((i / (uint)superBlock.InodeSize) + 1);
                            Array.Copy(inodeTableBytes, i, inodeBytes, 0, inodeBytes.Length);
                            WriteObject(new Inode(inodeBytes, inode));
                        }
                    }
                }
            }
        } // ProcessRecord 

        protected override void EndProcessing()
        {
            GC.Collect();
        }

        #endregion Cmdlet Overrides

    } // End GetInodeCommand class. 

    #endregion GetInodeCommand

}

