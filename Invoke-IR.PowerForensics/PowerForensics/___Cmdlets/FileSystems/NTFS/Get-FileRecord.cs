using System;
using System.Management.Automation;
using InvokeIR.Win32;
using InvokeIR.PowerForensics.NTFS;

namespace InvokeIR.PowerForensics.Cmdlets
{

    #region GetFileRecordCommand
    /// <summary> 
    /// This class implements the Get-FileRecord cmdlet. 
    /// </summary> 

    [Cmdlet(VerbsCommon.Get, "FileRecord", DefaultParameterSetName = "None", SupportsShouldProcess = true)]
    public class GetFileRecordCommand : PSCmdlet
    {

        #region Parameters

        /// <summary> 
        /// This parameter provides the Name of the Volume
        /// for which the FileRecord object should be
        /// returned.
        /// </summary> 

        [Parameter(ParameterSetName = "None")]
        [Parameter(ParameterSetName = "Index")]
        public string VolumeName
        {
            get { return volume; }
            set { volume = value; }
        }
        private string volume;

        /// <summary> 
        /// This parameter provides the IndexNumber for the 
        /// FileRecord object that will be returned.
        /// </summary> 

        [Alias("IndexNumber")]
        [Parameter(Mandatory = true, ParameterSetName = "Index")]
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

        [Alias("FilePath")]
        [Parameter(Mandatory = true, ParameterSetName = "Path")]
        public string Path
        {
            get { return filePath; }
            set { filePath = value; }
        }
        private string filePath;

        /// <summary> 
        /// This parameter provides the FileName for the 
        /// FileRecord object that will be returned.
        /// </summary> 

        [Parameter(Mandatory = false, ParameterSetName = "Path")]
        [Parameter(Mandatory = false, ParameterSetName = "Index")]
        public SwitchParameter AsBytes
        {
            get { return asbytes; }
            set { asbytes = value; }
        }
        private SwitchParameter asbytes;

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
            
            if(!(this.MyInvocation.BoundParameters.ContainsKey("Path")))
            {
                NativeMethods.getVolumeName(ref volume);
                string volLetter = volume.TrimStart('\\').TrimStart('.').TrimStart('\\') + '\\';

                if (this.MyInvocation.BoundParameters.ContainsKey("Index"))
                {
                    byte[] recordBytes = FileRecord.GetBytes(volume, indexNumber);

                    if (asbytes)
                    {
                        WriteObject(recordBytes);
                    }

                    else
                    {
                        WriteObject(new FileRecord(recordBytes));
                    }
                }

                else
                {
                    byte[] mftBytes = MasterFileTable.GetBytes(volume);
                    FileRecord[] records = FileRecord.GetInstances(mftBytes, volLetter);
                    foreach (FileRecord record in records)
                    {
                        WriteObject(record);
                    }
                }
            }

            else
            {
                // Derive volume path from the provided filePath
                string volumePath = "\\\\" + "." + "\\" + filePath.Split('\\')[0];
                NativeMethods.getVolumeName(ref volumePath);

                int index = IndexNumber.Get(volumePath, filePath);
                
                byte[] recordBytes = FileRecord.GetBytes(volumePath, index);
                
                if (asbytes)
                {
                    WriteObject(recordBytes);
                }

                else
                {
                    WriteObject(new FileRecord(recordBytes, filePath));
                }
            }

        } // ProcessRecord 

        protected override void EndProcessing()
        {
            GC.Collect();
        }

        #endregion Cmdlet Overrides

    } // End GetFileRecordCommand class. 

    #endregion GetFileRecordCommand

}
