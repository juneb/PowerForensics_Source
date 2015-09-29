using System;
using System.IO;
using System.Management.Automation;
using InvokeIR.Win32;
using InvokeIR.PowerForensics.Ntfs;

namespace InvokeIR.PowerForensics.Cmdlets
{
    #region GetBadClusterCommand
    /// <summary> 
    /// This class implements the Get-BadCluster cmdlet. 
    /// </summary> 

    [Cmdlet(VerbsCommon.Get, "BadCluster")]
    public class GetBadClusterCommand : PSCmdlet
    {
        #region Parameters

        /// <summary> 
        /// This parameter provides the Volume Name for the 
        /// AttrDef objects that will be returned.
        /// </summary> 

        [Parameter(Position = 0, ParameterSetName = "None")]
        public string VolumeName
        {
            get { return volume; }
            set { volume = value; }
        }
        private string volume;

        /// <summary> 
        /// 
        /// </summary> 

        /*[Alias("FullName")]
        [Parameter(ParameterSetName = "Path", ValueFromPipelineByPropertyName = true)]
        public string Path
        {
            get { return path; }
            set { path = value; }
        }
        private string path;*/
        
        #endregion Parameters

        #region Cmdlet Overrides

        /// <summary> 
        /// The ProcessRecord method calls AttrDef.GetInstances() 
        /// method to iterate through each AttrDef object on the specified volume.
        /// </summary> 

        protected override void BeginProcessing()
        {
            NativeMethods.checkAdmin();
        }
        
        protected override void ProcessRecord()
        {
            // Check for valid Volume name
            NativeMethods.getVolumeName(ref volume);

            // Set up FileStream to read volume
            IntPtr hVolume = NativeMethods.getHandle(volume);
            FileStream streamToRead = NativeMethods.getFileStream(hVolume);

            NonResident Bad = BadClus.GetBadStream(BadClus.GetFileRecord(volume));

            foreach (DataRun d in Bad.DataRun)
            {
                if (!(d.Sparse))
                {
                    WriteObject(new BadClus(d.StartCluster, true));
                }

            }

        } // ProcessRecord 

        #endregion Cmdlet Overrides

        protected override void EndProcessing()
        {
            GC.Collect();
        }

    } // End GetBadClusterCommand class. 

    #endregion GetBadClusterCommand
}
