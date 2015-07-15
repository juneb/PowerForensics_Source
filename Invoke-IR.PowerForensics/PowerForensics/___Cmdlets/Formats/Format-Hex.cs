using System;
using System.Collections.Generic;
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
        public byte Bytes
        {
            get { return bytes; }
            set { bytes = value; }
        }
        private byte bytes;

        private List<byte> byteList;
        #endregion Parameters

        #region Cmdlet Overrides

        /// <summary> 
        /// The ProcessRecord method calls HexDump.Get() 
        /// method to return an array of HexDump objects
        /// for the inputted byte[] object.
        /// </summary> 

        protected override void BeginProcessing()
        {
            byteList = new List<byte>();
        }

        protected override void ProcessRecord()
        {
            byteList.Add(bytes);
        }

        protected override void EndProcessing()
        {
            HexDump[] dump = HexDump.Get(byteList.ToArray());
            foreach (HexDump d in dump)
            {
                WriteObject(d);
            }
            GC.Collect();
        }

        #endregion Cmdlet Overrides

    } // End FormatHexCommand class. 

    #endregion FormatHexCommand

}
