using System;
using System.Management.Automation;
using InvokeIR.PowerForensics.Formats;

namespace InvokeIR.PowerForensics.Cmdlets
{

    #region FormatHexCommand
    /// <summary> 
    /// This class implements the Format-Hex cmdlet. 
    /// </summary> 

    [Cmdlet(VerbsCommon.Format, "Hex")]
    public class FormatHexCommand : Cmdlet
    {

        #region Parameters

        /// <summary> 
        /// This parameter provides the byte array to
        /// derive HexDump objects from.
        /// </summary> 

        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public byte[] Bytes
        {
            get { return bytes; }
            set { bytes = value; }
        }
        private byte[] bytes;

        #endregion Parameters

        #region Cmdlet Overrides

        /// <summary> 
        /// The ProcessRecord method calls HexDump.Get() 
        /// method to return an array of HexDump objects
        /// for the inputted byte[] object.
        /// </summary> 

        protected override void ProcessRecord()
        {
            HexDump[] dump = HexDump.Get(bytes);
            foreach (HexDump d in dump)
            {
                WriteObject(d);
            }
        }

        protected override void EndProcessing()
        {
            GC.Collect();
        }

        #endregion Cmdlet Overrides

    } // End FormatHexCommand class. 

    #endregion FormatHexCommand

}
