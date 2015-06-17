using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text.RegularExpressions;
using InvokeIR.Win32;
using InvokeIR.PowerForensics.NTFS;

namespace InvokeIR.PowerForensics.Cmdlets
{

    #region GetTimestompCommand
    /// <summary> 
    /// This class implements the Get-Timestomp cmdlet. 
    /// </summary> 

    [Cmdlet(VerbsCommon.Get, "Timestomp", DefaultParameterSetName = "Zero", SupportsShouldProcess = true)]
    public class GetTimstompCommand : PSCmdlet
    {

        #region Parameters

        /// <summary> 
        /// This parameter provides the Name of the Volume
        /// for which the FileRecord object should be
        /// returned.
        /// </summary> 

        [Parameter(ParameterSetName = "Zero")]
        public string VolumeName
        {
            get { return volume; }
            set { volume = value; }
        }
        private string volume;

        /// <summary> 
        /// This parameter provides the FileName for the 
        /// FileRecord object that will be returned.
        /// </summary> 

        [Alias("FilePath")]
        [Parameter(Mandatory = true, ParameterSetName = "Path")]
        public string Path
        {
            get { return filePath; }
            set { filePath = value; }
        }
        private string filePath;

        #endregion Parameters

        #region Cmdlet Overrides

        /// <summary> 
        /// The ProcessRecord instantiates a FileRecord objects that
        /// corresponds to the file(s) that is/are specified.
        /// </summary> 

        protected override void BeginProcessing()
        {
            NativeMethods.checkAdmin();
        }

        protected override void ProcessRecord()
        {
            NativeMethods.getVolumeName(ref volume);
            byte[] mftBytes = MasterFileTable.GetBytes(volume);
            string volLetter = volume.TrimStart('\\').TrimStart('.').TrimStart('\\') + '\\';
            FileRecord[] records = FileRecord.GetInstances(mftBytes, volLetter);

            foreach (FileRecord record in records)
            {
                
            }
        } // ProcessRecord 

        protected override void EndProcessing()
        {
            GC.Collect();
        }

        #endregion Cmdlet Overrides

    } // End GetTimestompCommand class. 

    #endregion GetTimestompCommand

}
