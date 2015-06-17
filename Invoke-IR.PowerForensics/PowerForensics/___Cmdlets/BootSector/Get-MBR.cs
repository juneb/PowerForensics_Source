using System;
using System.Management.Automation;
using InvokeIR.Win32;

namespace InvokeIR.PowerForensics.Cmdlets
{

    #region GetMBRCommand
    /// <summary> 
    /// This class implements the Get-MBR cmdlet. 
    /// </summary> 

    [Cmdlet(VerbsCommon.Get, "MBR", SupportsShouldProcess = true)]
    public class GetMBRCommand : PSCmdlet
    {

        #region Parameters

        /// <summary> 
        /// This parameter provides the Path of the Drive  
        /// for the MBR that will be returned.
        /// </summary> 

        [Alias("DrivePath")]
        [Parameter(Mandatory = true, Position = 0)]
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
        /// The ProcessRecord outputs a MasterBootRecord object for the
        /// specified Drive Path
        /// </summary> 

        protected override void BeginProcessing()
        {
            // Ensure cmdlet is being run as Administrator
            NativeMethods.checkAdmin();
            // Check that drivePath is valid
            NativeMethods.getDriveName(drivePath);
        } // End BeginProcessing

        protected override void ProcessRecord()
        {
            

            if (asBytes)
            {
                WriteObject(MasterBootRecord.GetBytes(drivePath));
            }
            else
            {
                WriteObject(new MasterBootRecord(drivePath));
            }
        } // End ProcessRecord 

        protected override void EndProcessing()
        {
            GC.Collect();
        } // End EndProcessing

        #endregion Cmdlet Overrides

    } // End GetMBRCommand class. 

    #endregion GetMBRCommand

}

