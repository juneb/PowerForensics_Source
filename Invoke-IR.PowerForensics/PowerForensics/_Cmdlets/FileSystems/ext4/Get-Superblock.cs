using System;
using System.IO;
using System.Management.Automation;
using InvokeIR.Win32;
using PowerForensics.Ext3;

namespace PowerForensics.Cmdlets
{
    #region GetSuperblockCommand
    
    /// <summary> 
    /// This class implements the Get-Superblock cmdlet. 
    /// </summary> 
    [Cmdlet(VerbsCommon.Get, "Superblock")]
    public class GetSuperblockCommand : PSCmdlet
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

            uint offset = 0;

            foreach (PartitionEntry partition in mbr.PartitionTable)
            {
                if (partition.Bootable && partition.SystemID == "LINUX")
                {
                    offset = partition.StartSector;
                }
            }

            #endregion MBR

            IntPtr hDevice = NativeMethods.getHandle(devicePath);
            
            using (FileStream streamToRead = NativeMethods.getFileStream(hDevice))
            {
                if (asbytes)
                {
                    WriteObject(Superblock.GetBytes(streamToRead, offset));
                }
                else
                {
                    WriteObject(new Superblock(Superblock.GetBytes(streamToRead, offset)));
                }
            }
        }

        #endregion Cmdlet Overrides
    }

    #endregion GetSuperblockCommand
}
