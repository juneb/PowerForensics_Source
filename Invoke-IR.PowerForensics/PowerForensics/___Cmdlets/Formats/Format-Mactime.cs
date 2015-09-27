using System;
using System.Management.Automation;
using InvokeIR.PowerForensics.Formats;
using InvokeIR.PowerForensics.Ntfs;

namespace InvokeIR.PowerForensics.Cmdlets
{

    #region FormatMactimeCommand
    /// <summary> 
    /// This class implements the Format-Mactime cmdlet. 
    /// </summary> 

    [Cmdlet(VerbsCommon.Format, "Mactime")]
    public class FormatMactimeCommand : Cmdlet
    {

        #region Parameters

        /// <summary> 
        /// This parameter provides the MFTRecord object(s) to
        /// derive Mactime objects from.
        /// </summary> 

        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public FileRecord MFTRecord
        {
            get { return mftRecord; }
            set { mftRecord = value; }
        }
        private FileRecord mftRecord;

        #endregion Parameters

        #region Cmdlet Overrides

        /// <summary> 
        /// The ProcessRecord method calls Mactime.Get() 
        /// method to return an array of Mactime objects
        /// for the inputted MFTRecord object.
        /// </summary> 
        protected override void ProcessRecord()
        {
            // Create an array of Mactime objects for the current MFTRecord object
            WriteObject(Mactime.Get(mftRecord));
        } // ProcessRecord 

        protected override void EndProcessing()
        {
            GC.Collect();
        }

        #endregion Cmdlet Overrides

    } // End FormatMactimeCommand class. 

    #endregion FormatMactimeCommand

}
