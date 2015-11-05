using System;
using System.Management.Automation;
using InvokeIR.Win32;
using PowerForensics.Artifacts;
using PowerForensics.Formats;
using PowerForensics.Ntfs;
using PowerForensics.Registry;

namespace PowerForensics.Cmdlets
{
    #region InvokeForensicTimelineCommand

    /// <summary> 
    /// This class implements the Invoke-ForensicTimeline cmdlet. 
    /// </summary> 
    [Cmdlet(VerbsLifecycle.Invoke, "ForensicTimeline")]
    public class InvokeForensicTimelineCommand : PSCmdlet
    {
        #region Parameters

        /// <summary> 
        /// This parameter provides the Name of the Volume
        /// for which the FileRecord object should be
        /// returned.
        /// </summary> 
        [Parameter()]
        public string VolumeName
        {
            get { return volume; }
            set { volume = value; }
        }
        private string volume;

        #endregion Parameters

        string volLetter = null;

        #region Cmdlet Overrides

        /// <summary> 
        ///
        /// </summary> 
        protected override void BeginProcessing()
        {
            NativeMethods.checkAdmin();
            NativeMethods.getVolumeName(ref volume);
            volLetter = NativeMethods.GetVolumeLetter(volume);
        }

        /// <summary> 
        ///
        /// </summary>
        protected override void ProcessRecord()
        {
            //WriteObject(ForensicTimeline.GetInstances(Prefetch.GetInstances(volume)), true);
            WriteObject(ForensicTimeline.GetInstances(ScheduledJob.GetInstances(volume)), true);
            WriteObject(ForensicTimeline.GetInstances(FileRecord.GetInstances(volume)), true);
            WriteObject(ForensicTimeline.GetInstances(UsnJrnl.GetInstances(volume)), true);
            WriteObject(ForensicTimeline.GetInstances(NamedKey.GetInstancesRecurse(volLetter + "\\Windows\\system32\\config\\DRIVERS")), true);
            WriteObject(ForensicTimeline.GetInstances(NamedKey.GetInstancesRecurse(volLetter + "\\Windows\\system32\\config\\SAM")), true);
            WriteObject(ForensicTimeline.GetInstances(NamedKey.GetInstancesRecurse(volLetter + "\\Windows\\system32\\config\\SECURITY")), true);
            WriteObject(ForensicTimeline.GetInstances(NamedKey.GetInstancesRecurse(volLetter + "\\Windows\\system32\\config\\SOFTWARE")), true);
            WriteObject(ForensicTimeline.GetInstances(NamedKey.GetInstancesRecurse(volLetter + "\\Windows\\system32\\config\\SYSTEM")), true);
        }

        #endregion Cmdlet Overrides
    }

    #endregion InvokeForensicTimelineCommand
}
