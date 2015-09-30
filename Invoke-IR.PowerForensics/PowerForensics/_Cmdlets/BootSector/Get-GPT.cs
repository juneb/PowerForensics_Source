using System;
using System.Management.Automation;
using InvokeIR.Win32;

namespace PowerForensics.Cmdlets
{
    #region GetGPTCommand
    
    /// <summary> 
    /// This class implements the Get-GPT cmdlet. 
    /// </summary> 
    [Cmdlet(VerbsCommon.Get, "GPT", SupportsShouldProcess = true)]
    public class GetGPTCommand : PSCmdlet
    {
        #region Parameters

        /// <summary> 
        /// This parameter provides the Path of the Drive  
        /// for the GPT that will be returned.
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
        /// This parameter causes Get-GPT to return the GPT as a Byte array
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
            // Check that drivePath is valid
            NativeMethods.getDriveName(drivePath);
        }

        /// <summary> 
        /// The ProcessRecord outputs a GuidPartitionTable object for the specified Drive Path
        /// </summary> 
        protected override void ProcessRecord()
        {
            if (asBytes)
            {
                WriteObject(GuidPartitionTable.GetBytes(drivePath));
            }
            else
            {
                WriteObject(new GuidPartitionTable(drivePath));
            }
        }

        #endregion Cmdlet Overrides
    }

    #endregion GetGPTCommand
}
