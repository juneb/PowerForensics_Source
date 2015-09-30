using System;
using System.IO;
using System.Management.Automation;
using InvokeIR.Win32;
using PowerForensics.Artifacts;
using PowerForensics.Ntfs;

namespace PowerForensics.Cmdlets
{
    #region GetPrefetchCommand
    
    /// <summary> 
    /// This class implements the Get-Prefetch cmdlet. 
    /// </summary> 
    [Cmdlet(VerbsCommon.Get, "Prefetch", DefaultParameterSetName = "ByVolume")]
    public class GetPrefetchCommand : PSCmdlet
    {
        #region Parameters

        /// <summary> 
        ///
        /// </summary> 
        [Parameter(ParameterSetName = "ByVolume", Position = 0)]
        public string VolumeName
        {
            get { return volume; }
            set { volume = value; }
        }
        private string volume;

        /// <summary> 
        /// This parameter provides the the path of the Prefetch file to parse.
        /// </summary> 
        [Alias("FilePath")]
        [Parameter(Mandatory = true, ParameterSetName = "ByPath", Position = 0)]
        public string Path
        {
            get { return filePath; }
            set { filePath = value; }
        }
        private string filePath;

        /// <summary> 
        ///
        /// </summary> 
        [Parameter()]
        public SwitchParameter Fast
        {
            get { return fast; }
            set { fast = value; }
        }
        private SwitchParameter fast;

        #endregion Parameters

        #region Cmdlet Overrides

        /// <summary> 
        /// 
        /// </summary> 
        protected override void BeginProcessing()
        {
            NativeMethods.checkAdmin();

            if (ParameterSetName == "ByVolume")
            {
                NativeMethods.getVolumeName(ref volume);
            }
        }

        /// <summary> 
        /// The ProcessRecord method returns a Prefetch object for the File specified
        /// by the Path property, or iterates through all .pf files in the
        /// C:\Windows\Prefetch directory to output an array of Prefetch objects.
        /// </summary> 
        protected override void ProcessRecord()
        {
            switch (ParameterSetName)
            {
                case "ByVolume":
                    if (fast)
                    {
                        WriteObject(Prefetch.GetInstances(volume, fast));
                    }
                    else
                    {
                        WriteObject(Prefetch.GetInstances(volume));
                    }
                    break;
                case "ByPath":
                    if (fast)
                    {
                        // Output the Prefetch object for the corresponding file
                        WriteObject(Prefetch.Get(filePath, fast));
                    }
                    else
                    {
                        // Output the Prefetch object for the corresponding file
                        WriteObject(Prefetch.Get(filePath));
                    }
                    break;
            }
        }  

        #endregion Cmdlet Overrides
    }
 
    #endregion GetPrefetchCommand
}
