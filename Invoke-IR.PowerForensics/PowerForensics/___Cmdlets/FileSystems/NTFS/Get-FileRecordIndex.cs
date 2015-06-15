using System;
using System.Management.Automation;
using InvokeIR.PowerForensics.NTFS;

namespace InvokeIR.PowerForensics.Cmdlets
{

    #region GetFileRecordIndexCommand
    /// <summary> 
    /// This class implements the Get-FileRecordIndex cmdlet. 
    /// </summary> 

    [Cmdlet(VerbsCommon.Get, "FileRecordIndex")]
    public class GetFileRecordIndexCommand : Cmdlet
    {

        #region Parameters

        /// <summary> 
        /// This parameter provides the Path for the 
        /// MFTRecordIndex that will be returned.
        /// </summary> 

        [Alias("FilePath")]
        [Parameter(Mandatory = true, Position = 0)]
        public string Path
        {
            get { return filePath; }
            set { filePath = value; }
        }
        private string filePath;

        #endregion Parameters

        #region Cmdlet Overrides

        /// <summary> 
        /// The ProcessRecord method calls ManagementClass.GetInstances() 
        /// method to iterate through each BindingObject on each system specified.
        /// </summary> 
        protected override void ProcessRecord()
        {
            string volume = @"\\.\" + filePath.Split('\\')[0];

            WriteObject(IndexNumber.Get(volume, filePath));

        } // ProcessRecord 

        protected override void EndProcessing()
        {
            GC.Collect();
        }

        #endregion Cmdlet Overrides

    } // End GetFileRecordCommand class. 
    #endregion GetFileRecordIndexCommand

}
