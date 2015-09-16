using System;
using System.IO;
using System.Management.Automation;
using InvokeIR.Win32;
using InvokeIR.PowerForensics.NTFS;

namespace InvokeIR.PowerForensics.Cmdlets
{

    #region GetUsnJrnlInformationCommand
    /// <summary> 
    /// This class implements the Get-UsnJrnlInformation cmdlet. 
    /// </summary> 

    [Cmdlet(VerbsCommon.Get, "UsnJrnlInformation")]
    public class GetUsnJrnlInformationCommand : PSCmdlet
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

        [Parameter()]
        public SwitchParameter AsBytes
        {
            get { return asBytes; }
            set { asBytes = value; }
        }
        private SwitchParameter asBytes;

        #endregion Parameters

        #region Cmdlet Overrides

        /// <summary> 
        /// The ProcessRecord method returns.
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

            // Get VolumeBootRecord object for logical addressing
            VolumeBootRecord VBR = VolumeBootRecord.get(streamToRead);

            // Get the $Max Data attribute (contains UsnJrnl details)
            Data Max = UsnJrnl.GetMaxStream(UsnJrnl.GetFileRecord(volume));

            if (asBytes)
            {
                WriteObject(Max.RawData);
            }
            else
            {
                WriteObject(new UsnJrnlDetail(Max.RawData));
            }
        } // ProcessRecord 

        protected override void EndProcessing()
        {
            GC.Collect();
        }

        #endregion Cmdlet Overrides

    } // End GetUsnJrnlInformationCommand class.

    #endregion GetUsnJrnlInformationCommand

}
