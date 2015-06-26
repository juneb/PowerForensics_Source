﻿using System;
using System.IO;
using System.Management.Automation;
using InvokeIR.Win32;
using InvokeIR.PowerForensics.ext3;

namespace InvokeIR.PowerForensics.Cmdlets
{

    #region GetBlockGroupDescriptorCommand
    /// <summary> 
    /// This class implements the Get-BlockGroupDescriptor cmdlet. 
    /// </summary> 

    [Cmdlet(VerbsCommon.Get, "BlockGroupDescriptor")]
    public class GetBlockGroupDescriptorCommand : PSCmdlet
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
        public uint GroupNumber
        {
            get { return number; }
            set { number = value; }
        }
        private uint number;

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

                if(this.MyInvocation.BoundParameters.ContainsKey("GroupNumber"))
                {
                    if (asbytes)
                    {
                        WriteObject(BlockGroupDescriptor.GetBytes(streamToRead, superblockOffset, superBlock, number));
                    }
                    else
                    {
                        WriteObject(new BlockGroupDescriptor(BlockGroupDescriptor.GetBytes(streamToRead, superblockOffset, superBlock, number)));
                    }
                }
                else
                {
                    if (asbytes)
                    {
                        BlockGroupDescriptorTable.GetBytes(streamToRead, superblockOffset, superBlock);
                    }
                    else
                    {
                        byte[] bgdtBytes = BlockGroupDescriptorTable.GetBytes(streamToRead, superblockOffset, superBlock);
                        for (uint o = 0; o < bgdtBytes.Length; o += BlockGroupDescriptor.BLOCK_GROUP_DESCRIPTOR_LENGTH)
                        {
                            byte[] bgdBytes = new byte[BlockGroupDescriptor.BLOCK_GROUP_DESCRIPTOR_LENGTH];
                            Array.Copy(bgdtBytes, o, bgdBytes, 0, bgdBytes.Length);
                            WriteObject(new BlockGroupDescriptor(bgdBytes));
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

    } // End GetBlockGroupDescriptorCommand class. 

    #endregion GetBlockGroupDescriptorCommand

}
