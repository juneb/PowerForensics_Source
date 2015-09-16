using System;
using System.IO;
using System.Management.Automation;
using InvokeIR.Win32;
using InvokeIR.PowerForensics.NTFS;

namespace InvokeIR.PowerForensics.Cmdlets
{

    #region GetUnallocatedSpaceCommand
    /// <summary> 
    /// This class implements the Get-UnallocatedSpace cmdlet. 
    /// </summary> 

    [Cmdlet(VerbsCommon.Get, "UnallocatedSpace")]
    public class GetUnallocatedSpaceCommand : PSCmdlet
    {

        #region Parameters

        /// <summary> 
        /// This parameter provides the the name of the target volume.
        /// </summary> 

        [Parameter(Position = 0)]
        public string VolumeName
        {
            get { return volume; }
            set { volume = value; }
        }
        private string volume;

        [Parameter(Mandatory = false)]
        public ulong Path
        {
            get { return path; }
            set { path = value; }
        }
        private ulong path;

        #endregion Parameters

        #region Cmdlet Overrides

        /// <summary> 
        /// The ProcessRecord method returns.
        /// </summary> 

        protected override void BeginProcessing()
        {
            NativeMethods.checkAdmin();
            NativeMethods.getVolumeName(ref volume);
        }

        protected override void ProcessRecord()
        {
            IntPtr hVolume = NativeMethods.getHandle(volume);
            using(FileStream streamToRead = NativeMethods.getFileStream(hVolume))
            {
                foreach (Bitmap b in Bitmap.GetInstances(volume))
                {
                    if (!(b.InUse))
                    {
                        WriteObject(b);
                    }
                }
            }
        } // ProcessRecord 

        protected override void EndProcessing()
        {
            GC.Collect();
        }

        #endregion Cmdlet Overrides

    } // End GetUnallocatedSpaceCommand class.

    #endregion GetUnallocatedSpaceCommand

}
