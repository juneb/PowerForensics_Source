using System;
using System.IO;
using System.Management.Automation;
using InvokeIR.Win32;
using InvokeIR.PowerForensics.Ntfs;

namespace InvokeIR.PowerForensics.Cmdlets
{

    #region GetVolumeInformationCommand
    /// <summary> 
    /// This class implements the Get-VolumeInformation cmdlet. 
    /// </summary> 

    [Cmdlet(VerbsCommon.Get, "VolumeInformation")]
    public class GetVolumeInformationCommand : Cmdlet
    {

        #region Parameters

        /// <summary> 
        /// This parameter provides the Volume Name for the 
        /// AttrDef objects that will be returned.
        /// </summary> 

        [Parameter(Position = 0)]
        public string VolumeName
        {
            get { return volume; }
            set { volume = value; }
        }
        private string volume;

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

            // Get the $J Data attribute (contains UsnJrnl details
            WriteObject(Volume.GetVolumeInformationAttr(Volume.GetFileRecord(volume)));

        } // ProcessRecord 

        #endregion Cmdlet Overrides

        protected override void EndProcessing()
        {
            GC.Collect();
        }

    } // End GetVolumeInformationCommand class. 

    #endregion GetVolumeInformationCommand

}
