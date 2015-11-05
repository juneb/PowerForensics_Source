using System;
using System.Management.Automation;
using InvokeIR.Win32;
using PowerForensics.Ntfs;
using PowerForensics.Artifacts;

namespace PowerForensics.Cmdlets
{
    #region GetScheduledJobCommand
    
    /// <summary> 
    /// This class implements the Get-ScheduledJob cmdlet. 
    /// </summary> 
    [Cmdlet(VerbsCommon.Get, "ScheduledJobRaw", DefaultParameterSetName = "ByVolume")]
    public class GetScheduledJobCommand : PSCmdlet
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
        [Alias("FullName")]
        [Parameter(Mandatory = true, ParameterSetName = "ByPath", Position = 0, ValueFromPipelineByPropertyName = true)]
        public string Path
        {
            get { return path; }
            set { path = value; }
        }
        private string path;

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
        /// The ProcessRecord method calls TimeZone.CurrentTimeZone to return a TimeZone object.
        /// </summary> 
        protected override void ProcessRecord()
        {
            switch (ParameterSetName)
            {
                case "ByVolume":
                    WriteObject(ScheduledJob.GetInstances(volume), true);
                    break;
                case "ByPath":
                    WriteObject(ScheduledJob.Get(path));
                    break;
            }
        }

        #endregion Cmdlet Overrides
    }

    #endregion GetScheduledJobCommand
}
