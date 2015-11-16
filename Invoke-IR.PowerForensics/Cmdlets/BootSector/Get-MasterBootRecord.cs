using System;
using System.Management.Automation;
using InvokeIR.Win32;

namespace PowerForensics.Cmdlets
{
    #region GetMasterBootRecordCommand

    /// <summary> 
    /// This class implements the Get-MasterBootRecord cmdlet. 
    /// </summary> 
    [Cmdlet(VerbsCommon.Get, "MasterBootRecord")]
    public class GetMBRCommand : PSCmdlet
    {
        #region Parameters

        /// <summary> 
        /// This parameter provides the Path of the Drive  
        /// for the MBR that will be returned.
        /// </summary> 
        [Parameter(Mandatory = false, ParameterSetName = "Remote")]
        public string ComputerName
        {
            get { return computerName; }
            set { computerName = value; }
        }
        private string computerName;

        /// <summary> 
        /// This parameter provides the Path of the Drive  
        /// for the MBR that will be returned.
        /// </summary> 
        [Alias("DrivePath")]
        [Parameter(Mandatory = true, Position = 0,  ParameterSetName = "Local")]
        public string Path
        {
            get { return drivePath; }
            set { drivePath = value; }
        }
        private string drivePath;

        /// <summary> 
        /// This parameter causes Get-MBR to return the MBR as a Byte array
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
        /// 
        /// </summary> 
        protected override void BeginProcessing()
        {
            // Ensure cmdlet is being run as Administrator
            NativeMethods.checkAdmin();
        }

        /// <summary> 
        /// The ProcessRecord outputs a MasterBootRecord object for the
        /// specified Drive Path
        /// </summary> 
        protected override void ProcessRecord()
        {
            byte[] bytes = new byte[0x200];

            if (this.MyInvocation.BoundParameters.ContainsKey("ComputerName"))
            {
                bytes = NativeMethods.readDrive(computerName, 60000, 0, 0x200);
                if (asBytes)
                {
                    WriteObject(bytes);
                }
                else
                {
                    WriteObject(new MasterBootRecord(bytes));
                }
            }
            else
            {
                // Check that drivePath is valid
                NativeMethods.getDriveName(drivePath);

                if (asBytes)
                {
                    WriteObject(MasterBootRecord.GetBytes(drivePath));
                }
                else
                {
                    WriteObject(new MasterBootRecord(MasterBootRecord.GetBytes(drivePath)));
                }
            }
        }

        #endregion Cmdlet Overrides
    }

    #endregion GetMasterBootRecordCommand
}

