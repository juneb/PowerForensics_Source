using System;
using System.Management.Automation;
using InvokeIR.Win32;
using PowerForensics.Ntfs;

namespace PowerForensics.Cmdlets
{
    #region GetUsnJrnlCommand
    
    /// <summary> 
    /// This class implements the Get-UsnJrnl cmdlet 
    /// </summary> 
    [Cmdlet(VerbsCommon.Get, "UsnJrnl", DefaultParameterSetName = "ByVolume")]
    public class GetUsnJrnlCommand : PSCmdlet
    {
        #region Parameters

        /// <summary> 
        /// This parameter provides the the name of the target volume.
        /// </summary> 
        [Parameter(Position = 0, ParameterSetName = "ByVolume")]
        [Parameter(Position = 0, ParameterSetName = "ByVolumeUsn")]
        public string VolumeName
        {
            get { return volume; }
            set { volume = value; }
        }
        private string volume;

        /// <summary> 
        /// 
        /// </summary> 
        [Alias("FullName")]
        [Parameter(Mandatory = true, ParameterSetName = "ByPath", ValueFromPipelineByPropertyName = true)]
        [Parameter(Mandatory = true, ParameterSetName = "ByPathUsn", ValueFromPipelineByPropertyName = true)]
        public string Path
        {
            get { return path; }
            set { path = value; }
        }
        private string path;

        /// <summary> 
        /// 
        /// </summary> 
        [Parameter(Mandatory = true, ParameterSetName = "ByVolumeUsn")]
        [Parameter(Mandatory = true, ParameterSetName = "ByPathUsn")]
        public ulong USN
        {
            get { return usn; }
            set { usn = value; }
        }
        private ulong usn;

        /// <summary> 
        /// 
        /// </summary> 

        /*[Parameter()]
        public SwitchParameter AsBytes
        {
            get { return asBytes; }
            set { asBytes = value; }
        }
        private SwitchParameter asBytes;*/

        #endregion Parameters

        #region Cmdlet Overrides

        /// <summary> 
        /// 
        /// </summary> 
        protected override void BeginProcessing()
        {
            NativeMethods.checkAdmin();
            
            if (ParameterSetName.Contains("Volume"))
            {
                NativeMethods.getVolumeName(ref volume);
            }
        }

        /// <summary> 
        /// 
        /// </summary> 
        protected override void ProcessRecord()
        {
            switch (ParameterSetName)
            {
                case "ByVolume":
                    WriteObject(UsnJrnl.GetInstances(volume.Split('\\')[3] + "\\$Extend\\$UsnJrnl"), true);
                    break;
                case "ByVolumeUsn":
                    WriteObject(UsnJrnl.Get(volume.Split('\\')[3] + "\\$Extend\\$UsnJrnl", usn));
                    break;
                case "ByPath":
                    WriteObject(UsnJrnl.GetInstances(path), true);
                    break;
                case "ByPathUsn":
                    WriteObject(UsnJrnl.Get(path, usn));
                    break;
            }
        }

        #endregion Cmdlet Overrides
    }

    #endregion GetUsnJrnlCommand
}
