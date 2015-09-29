using System;
using System.Management.Automation;
using InvokeIR.Win32;
using InvokeIR.PowerForensics.Ntfs;

namespace InvokeIR.PowerForensics.Cmdlets
{
    #region GetBitmapCommand
    /// <summary> 
    /// This class implements the Get-Bitmap cmdlet. 
    /// </summary> 

    [Cmdlet(VerbsCommon.Get, "Bitmap", DefaultParameterSetName = "ByVolume")]
    public class GetBitmapCommand : PSCmdlet
    {
        #region Parameters

        /// <summary> 
        /// This parameter provides the the name of the target volume.
        /// </summary> 

        [Parameter(Position = 0, ParameterSetName = "ByVolume")]
        public string VolumeName
        {
            get { return volume; }
            set { volume = value; }
        }
        private string volume;

        [Alias("FullName")]
        [Parameter(Mandatory = true, ParameterSetName = "ByPath", ValueFromPipelineByPropertyName = true)]
        public string Path
        {
            get { return path; }
            set { path = value; }
        }
        private string path;

        [Parameter(Mandatory = true, ParameterSetName = "ByVolume")]
        [Parameter(Mandatory = true, ParameterSetName = "ByPath")]
        public ulong Cluster
        {
            get { return cluster; }
            set { cluster = value; }
        }
        private ulong cluster;

        #endregion Parameters

        #region Cmdlet Overrides

        /// <summary> 
        /// The BeginProcessing method returns.
        /// </summary> 

        protected override void BeginProcessing()
        {
            NativeMethods.checkAdmin();
        }

        /// <summary> 
        /// The ProcessRecord method returns.
        /// </summary> 

        protected override void ProcessRecord()
        {
            if (ParameterSetName == "ByVolume")
            {
                WriteObject(Bitmap.Get(volume, cluster));
            }
            else
            {
                WriteObject(Bitmap.GetByPath(path, cluster));
            }
        }

        /// <summary> 
        /// The EndProcessing method calls for Garbage Collection.
        /// </summary> 

        protected override void EndProcessing()
        {
            GC.Collect();
        }

        #endregion Cmdlet Overrides
    } // End GetBitmapCommand class.

    #endregion GetBitmapCommand
}
