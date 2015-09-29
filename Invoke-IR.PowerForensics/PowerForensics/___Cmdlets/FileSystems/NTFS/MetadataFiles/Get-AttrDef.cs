using System;
using System.Management.Automation;
using InvokeIR.Win32;
using InvokeIR.PowerForensics.Ntfs;

namespace InvokeIR.PowerForensics.Cmdlets
{
    #region GetAttrDefCommand
    /// <summary> 
    /// This class implements the Get-AttrDef cmdlet. 
    /// </summary> 

    [Cmdlet(VerbsCommon.Get, "AttrDef", DefaultParameterSetName = "Volume")]
    public class GetAttrDefCommand : PSCmdlet
    {
        #region Parameters

        /// <summary> 
        /// This parameter provides the Volume Name for the 
        /// AttrDef objects that will be returned.
        /// </summary> 

        [Parameter(ParameterSetName = "Volume", Position = 0)]
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
        [Parameter(Mandatory = true, ParameterSetName = "Path", ValueFromPipelineByPropertyName = true)]
        public string Path
        {
            get { return path; }
            set { path = value; }
        }
        private string path;

        #endregion Parameters

        #region Cmdlet Overrides

        /// <summary> 
        /// The ProcessRecord method calls AttrDef.GetInstances() 
        /// method to iterate through each AttrDef object on the specified volume.
        /// </summary> 

        protected override void BeginProcessing()
        {
            NativeMethods.checkAdmin();
            NativeMethods.getVolumeName(ref volume);
        }
        
        protected override void ProcessRecord()
        {
            if (ParameterSetName == "Volume")
            {
                WriteObject(AttrDef.GetInstances(volume));
            }
            else
            {
                WriteObject(AttrDef.GetInstancesByPath(path));
            }
        } // ProcessRecord 

        protected override void EndProcessing()
        {
            GC.Collect();
        }
        
        #endregion Cmdlet Overrides

    } // End GetProcCommand class. 

    #endregion GetAttrDefCommand
}
