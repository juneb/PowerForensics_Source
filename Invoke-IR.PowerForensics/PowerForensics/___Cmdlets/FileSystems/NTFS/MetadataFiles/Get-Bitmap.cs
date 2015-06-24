using System;
using System.Management.Automation;
using InvokeIR.Win32;
using InvokeIR.PowerForensics.NTFS;

namespace InvokeIR.PowerForensics.Cmdlets
{

    #region GetBitmapCommand
    /// <summary> 
    /// This class implements the Get-Bitmap cmdlet. 
    /// </summary> 

    [Cmdlet(VerbsCommon.Get, "Bitmap")]
    public class GetBitmapCommand : PSCmdlet
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

        [Parameter(Mandatory = true)]
        public ulong Cluster
        {
            get { return cluster; }
            set { cluster = value; }
        }
        private ulong cluster;

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

            WriteObject(Bitmap.Get(volume, cluster));

        } // ProcessRecord 

        protected override void EndProcessing()
        {
            GC.Collect();
        }

        #endregion Cmdlet Overrides

    } // End GetBitmapCommand class.

    #endregion GetBitmapCommand

}
