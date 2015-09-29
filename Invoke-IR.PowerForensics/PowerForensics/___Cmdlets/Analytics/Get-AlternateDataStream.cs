using System;
using System.IO;
using System.Management.Automation;
using InvokeIR.Win32;
using InvokeIR.PowerForensics.Ntfs;
using InvokeIR.PowerForensics.Artifacts;

namespace InvokeIR.PowerForensics.Cmdlets
{
    #region GetAlternateDataStreamCommand
    /// <summary> 
    /// This class implements the Get-AlternateDataStream cmdlet. 
    /// </summary> 

    [Cmdlet(VerbsCommon.Get, "AlternateDataStream", DefaultParameterSetName = "Zero", SupportsShouldProcess = true)]
    public class GetAlternateDataStreamCommand : PSCmdlet
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
            NativeMethods.getVolumeName(ref volume);
        }

        protected override void ProcessRecord()
        {
            if (ParameterSetName == "Path")
            {
                FileRecord record = FileRecord.Get(filePath, false);

                if (record.Attribute != null)
                {
                    foreach (Attr attr in record.Attribute)
                    {
                        if (attr.Name == Attr.ATTR_TYPE.DATA)
                        {
                            //if (attr.NameString != "")
                            if(attr.NameString.Length > 0)
                            {
                                WriteObject(new AlternateDataStream(record.FullName, record.Name, attr.NameString));
                            }
                        }
                    }
                }
            }
            else
            {
                FileRecord[] records = FileRecord.GetInstances(volume);

                foreach (FileRecord record in records)
                {
                    if (record.Attribute != null)
                    {
                        foreach (Attr attr in record.Attribute)
                        {
                            if (attr.Name == Attr.ATTR_TYPE.DATA)
                            {
                                //if (attr.NameString != "")
                                if (attr.NameString.Length > 0)
                                {
                                    WriteObject(new AlternateDataStream(record.FullName, record.Name, attr.NameString));
                                }
                            }
                        }

                    }
                }
            }
        } // ProcessRecord 

        protected override void EndProcessing()
        {
            GC.Collect();
        }

        #endregion Cmdlet Overrides

    } // End GetAlternateDataStreamCommand class. 

    #endregion GetAlternateDataStreamCommand
}
