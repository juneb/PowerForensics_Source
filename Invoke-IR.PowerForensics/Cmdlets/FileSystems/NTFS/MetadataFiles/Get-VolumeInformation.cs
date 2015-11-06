using System;
using System.IO;
using System.Management.Automation;
using InvokeIR.Win32;
using PowerForensics.Ntfs;

namespace PowerForensics.Cmdlets
{
    #region GetVolumeInformationCommand
    
    /// <summary> 
    /// This class implements the Get-VolumeInformation cmdlet. 
    /// </summary> 
    [Cmdlet(VerbsCommon.Get, "VolumeInformation", DefaultParameterSetName = "ByVolume")]
    public class GetVolumeInformationCommand : PSCmdlet
    {
        #region Parameters

        /// <summary> 
        /// This parameter provides the the name of the target volume.
        /// </summary> 
        [Parameter(ParameterSetName = "ByVolume")]
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
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "ByPath", ValueFromPipelineByPropertyName = true)]
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
        /// 
        /// </summary> 
        protected override void ProcessRecord()
        {
            switch (ParameterSetName)
            {
                case "ByVolume":
                    WriteObject(VolumeInformation.Get(volume));
                    break;
                case "ByPath":
                    WriteObject(VolumeInformation.GetByPath(path));
                    break;
            }
        }

        #endregion Cmdlet Overrides
    }

    #endregion GetVolumeInformationCommand
}
