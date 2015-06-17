using System;
using System.Management.Automation;
using System.Text.RegularExpressions;
using InvokeIR.Win32;
using InvokeIR.PowerForensics.NTFS;

namespace InvokeIR.PowerForensics.Cmdlets
{

    #region GetFileRecordAttributeCommand
    /// <summary> 
    /// This class implements the Get-FileRecordAttribute cmdlet. 
    /// </summary> 

    [Cmdlet(VerbsCommon.Get, "FileRecordAttribute")]
    public class GetFileRecordAttributeCommand : Cmdlet
    {

        #region Parameters

        /// <summary> 
        /// This parameter provides the Name of the Volume
        /// for which the FileRecord object should be
        /// returned.
        /// </summary> 

        [Parameter(Position = 0)]
        public string VolumeName
        {
            get { return volume; }
            set { volume = value; }
        }
        private string volume;

        /// <summary> 
        /// This parameter provides the MFTIndexNumber for the 
        /// FileRecord object that will be returned.
        /// </summary> 

        [Alias("IndexNumber")]
        [Parameter(Mandatory = true)]
        public int Index
        {
            get { return indexNumber; }
            set { indexNumber = value; }
        }
        private int indexNumber;

        /// <summary> 
        /// This parameter provides the FileName for the 
        /// FileRecord object that will be returned.
        /// </summary> 

        [Parameter(Mandatory = true)]
        public uint AttributeId
        {
            get { return attribute; }
            set { attribute = value; }
        }
        private uint attribute;

/*        /// <summary> 
        /// This parameter provides the FileName for the 
        /// FileRecord object that will be returned.
        /// </summary> 

        [Parameter()]
        public SwitchParameter AsBytes
        {
            get { return asbytes; }
            set { asbytes = value; }
        }
        private SwitchParameter asbytes;
*/

        #endregion Parameters

        #region Cmdlet Overrides

        /// <summary> 

        /// </summary> 

        protected override void BeginProcessing()
        {
            NativeMethods.checkAdmin();
        }
        
        protected override void ProcessRecord()
        {
            Win32.NativeMethods.getVolumeName(ref volume);

            WriteDebug("VolumeName: " + volume);

            byte[] recordBytes = FileRecord.GetBytes(volume, indexNumber);

            FileRecord fileRecord = new FileRecord(recordBytes);

            foreach (Attr attr in fileRecord.Attribute)
            {
                string attrName = Enum.GetName(typeof(Attr.ATTR_TYPE), attribute);

                if(attr.Name == attrName)
                {
                    WriteObject(attr);
                }
            }

        } // ProcessRecord 

        protected override void EndProcessing()
        {
            GC.Collect();
        }

        #endregion Cmdlet Overrides

    } // End GetFileRecordAttributeCommand class. 
    #endregion GetFileRecordAttributeCommand

}
