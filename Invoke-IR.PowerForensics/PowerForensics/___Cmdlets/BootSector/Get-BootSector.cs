using System;
using System.Management.Automation;
using InvokeIR.Win32;

namespace InvokeIR.PowerForensics.Cmdlets
{

    #region GetBootSectorCommand
    /// <summary> 
    /// This class implements the Get-BootSector cmdlet. 
    /// </summary> 

    [Cmdlet(VerbsCommon.Get, "BootSector", SupportsShouldProcess = true)]
    public class GetBootSectorCommand : Cmdlet
    {

        #region Parameters

        /// <summary> 
        /// This parameter provides the Path of the Drive  
        /// for the BootSector that will be returned.
        /// </summary> 

        [Alias("DevicePath")]
        [Parameter(Mandatory = true, Position = 0)]
        public string Path
        {
            get { return devicePath; }
            set { devicePath = value; }
        }
        private string devicePath;

        /// <summary> 
        /// This parameter causes Get-BootSector to return the MBR or GPT as a Byte array
        /// </summary> 

        [Parameter()]
        public SwitchParameter AsBytes
        {
            get { return asBytes; }
            set { asBytes = value; }
        }
        private SwitchParameter asBytes;

        #endregion Parameters

        #region Cmdlet Overrides

        /// <summary> 
        /// The ProcessRecord outputs a MasterBootRecord GuidPartitionTable object for the specified Device
        /// </summary> 

        protected override void ProcessRecord()
        {
            MasterBootRecord mbr = new MasterBootRecord(devicePath);

            if (mbr.PartitionTable[0].SystemID == "EFI_GPT_DISK")
            {
                if (asBytes)
                {
                    WriteObject(GuidPartitionTable.GetBytes(devicePath));
                }
                else
                {
                    WriteObject(new GuidPartitionTable(devicePath));
                }
            }
            else
            {
                if (asBytes)
                {
                    WriteObject(MasterBootRecord.GetBytes(devicePath));
                }
                else
                {
                    WriteObject(mbr);
                }
            }


        } // ProcessRecord 

        protected override void EndProcessing()
        {
            GC.Collect();
        }

        #endregion Cmdlet Overrides

    } // End GetBootSectorCommand class. 

    #endregion GetBootSectorCommand

}
