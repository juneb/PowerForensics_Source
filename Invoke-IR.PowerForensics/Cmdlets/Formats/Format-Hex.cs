using System;
using System.Collections.Generic;
using System.Management.Automation;
using PowerForensics.Formats;

namespace PowerForensics.Cmdlets
{
    #region FormatHexCommand
    
    /// <summary> 
    /// This class implements the Format-Hex cmdlet. 
    /// </summary> 
    [Cmdlet(VerbsCommon.Format, "Hex")]
    public class FormatHexCommand : PSCmdlet
    {
        #region Parameters

        /// <summary> 
        /// This parameter provides the byte array to
        /// derive HexDump objects from.
        /// </summary> 
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
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
        /// 
        /// </summary> 
        protected override void BeginProcessing()
        {
            byteList = new List<byte>();
        }

        /// <summary> 
        /// The ProcessRecord method calls HexDump.Get() 
        /// method to return an array of HexDump objects
        /// for the inputted byte[] object.
        /// </summary> 
        protected override void ProcessRecord()
        {
            byteList.Add(bytes);
        }

        /// <summary> 
        /// 
        /// </summary> 
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
    }

    #endregion FormatHexCommand
}
