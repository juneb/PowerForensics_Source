using System;
using System.IO;
using System.Management.Automation;
using InvokeIR.Win32;
using InvokeIR.PowerForensics.NTFS;

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
                //UsnJrnl.Get(volume, usn);
                WriteObject(UsnJrnl.Get(volume, usn));
            }
            
            else
            {
                UsnJrnl[] usn = UsnJrnl.GetInstances(volume);
                foreach (UsnJrnl u in usn)
                {
                    WriteObject(u);
                }
            }

        } // ProcessRecord 

        protected override void EndProcessing()
        {
            GC.Collect();
        }

        #endregion Cmdlet Overrides

    } // End GetUsnJrnlCommand class.

    #endregion GetUsnJrnlCommand

}
