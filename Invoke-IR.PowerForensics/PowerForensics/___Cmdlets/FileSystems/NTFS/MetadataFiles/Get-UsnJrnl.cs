using System;
using System.Management.Automation;
using InvokeIR.Win32;
using InvokeIR.PowerForensics.Ntfs;

namespace InvokeIR.PowerForensics.Cmdlets
{
    #region GetUsnJrnlCommand
    /// <summary> 
    /// This class implements the Get-UsnJrnl cmdlet. 
    /// </summary> 

    [Cmdlet(VerbsCommon.Get, "UsnJrnl")]
    public class GetUsnJrnlCommand : PSCmdlet
    {

        #region Parameters

        /// <summary> 
        /// This parameter provides the the name of the target volume.
        /// </summary> 

        [Parameter(Position = 0)]
        public string VolumeName
        {
            get { return volume; }
            set { volume = value; }
        }
        private string volume;

        [Parameter(ParameterSetName = "USN")]
        public ulong USN
        {
            get { return usn; }
            set { usn = value; }
        }
        private ulong usn;

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
        /// The ProcessRecord method returns.
        /// </summary> 

        protected override void BeginProcessing()
        {
            NativeMethods.checkAdmin();
        }
        
        protected override void ProcessRecord()
        {
            
            if (this.MyInvocation.BoundParameters.ContainsKey("USN"))
            {
                WriteObject(UsnJrnl.Get(volume, usn));
            }
            
            else
            {
                UsnJrnl[] usn = UsnJrnl.GetInstances(volume);

                WriteObject(usn, true);
            }

        } // ProcessRecord 

        protected override void EndProcessing()
        {
            
        }

        #endregion Cmdlet Overrides

    } // End GetUsnJrnlCommand class.

    #endregion GetUsnJrnlCommand
}
