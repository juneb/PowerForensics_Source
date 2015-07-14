using System;
using System.Management.Automation;
using InvokeIR.Win32;
using InvokeIR.PowerForensics.NTFS;
using InvokeIR.PowerForensics.Artifacts;

namespace InvokeIR.PowerForensics.Cmdlets
{

    #region GetScheduledJobCommand
    /// <summary> 
    /// This class implements the Get-ScheduledJob cmdlet. 
    /// </summary> 

    [Cmdlet(VerbsCommon.Get, "ScheduledJobRaw", DefaultParameterSetName = "None")]
    public class GetScheduledJobCommand : PSCmdlet
    {
        #region Parameters

        [Parameter(ParameterSetName = "None", Position = 0)]
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
        [Parameter(Mandatory = true, ParameterSetName = "Path", Position = 0)]
        public string Path
        {
            get { return filePath; }
            set { filePath = value; }
        }
        private string filePath;

        #endregion Parameters


        #region Cmdlet Overrides

        protected override void BeginProcessing()
        {
            NativeMethods.checkAdmin();
        }

        /// <summary> 
        /// The ProcessRecord method calls TimeZone.CurrentTimeZone to return a TimeZone object.
        /// </summary> 
        protected override void ProcessRecord()
        {
            if (this.MyInvocation.BoundParameters.ContainsKey("Path"))
            {
                WriteObject(ScheduledJob.Get(filePath));
            }
            else
            {
                NativeMethods.getVolumeName(ref volume);
                foreach (IndexEntry entry in IndexEntry.GetInstances("C:\\Windows\\Tasks"))
                {
                    if (entry.Filename.Contains(".job"))
                    {
                        string path = "C:\\Windows\\Tasks\\" + entry.Filename;
                        WriteObject(ScheduledJob.Get(volume, (int)entry.RecordNumber));
                    }
                }
            }

        } // ProcessRecord 

        protected override void EndProcessing()
        {
            GC.Collect();
        }

        #endregion Cmdlet Overrides

    } // End GetScheduledJobCommand class. 

    #endregion GetScheduledJobCommand

}
