using System;
using System.IO;
using System.Collections.Generic;
using System.Management.Automation;
using InvokeIR.Win32;

namespace InvokeIR.PowerForensics.Ntfs
{
    #region GetFileRecordCommand
    /// <summary> 
    /// This class implements the Get-FileRecord cmdlet. 
    /// </summary> 

    [Cmdlet(VerbsCommon.Get, "FileRecord", SupportsShouldProcess = true, DefaultParameterSetName = "Default")]
    public class GetFileRecordCommand : PSCmdlet
    {

        #region Parameters

        /// <summary> 
        /// This parameter provides the Name of the Volume
        /// for which the FileRecord object should be
        /// returned.
        /// </summary> 

        [Parameter(ParameterSetName = "Default")]
        [Parameter(ParameterSetName = "Index")]
        public string VolumeName
        {
            get { return volume; }
            set { volume = value; }
        }
        private string volume;

        /// <summary>
        /// 
        /// </summary> 

        [Parameter(ParameterSetName = "Index")]
        [Parameter(ParameterSetName = "Path")]
        public SwitchParameter AsBytes
        {
            get { return asbytes; }
            set { asbytes = value; }
        }
        private SwitchParameter asbytes;

        /// <summary> 
        /// This parameter provides the IndexNumber for the 
        /// FileRecord object that will be returned.
        /// </summary> 

        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Index")]
        public int Index
        {
            get { return indexNumber; }
            set { indexNumber = value; }
        }
        private int indexNumber;

        /// <summary>
        /// 
        /// </summary> 

        [Alias("FullName")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Path", ValueFromPipelineByPropertyName = true)]
        public string Path
        {
            get { return path; }
            set { path = value; }
        }
        private string path;

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
            if (this.MyInvocation.BoundParameters.ContainsKey("Index"))
            {
                NativeMethods.getVolumeName(ref volume);

                if (asbytes)
                {
                    WriteObject(FileRecord.GetRecordBytes(volume, indexNumber));
                }
                else
                {
                    WriteObject(new FileRecord(FileRecord.GetRecordBytes(volume, indexNumber), volume));
                }
            }
            else if (this.MyInvocation.BoundParameters.ContainsKey("Path"))
            {
                string volume = NativeMethods.getVolumeName(ref path.Split(':')[0]);

                IndexEntry entry = IndexEntry.Get(path);

                if (asbytes)
                {
                    WriteObject(FileRecord.GetRecordBytes(volume, (int)entry.RecordNumber));
                }
                else
                {
                    WriteObject(new FileRecord(FileRecord.GetRecordBytes(volume, (int)entry.RecordNumber), volume));
                }
            }
            else
            {
                NativeMethods.getVolumeName(ref volume);

                WriteObject(FileRecord.GetInstances(volume), true);
            }
        } // ProcessRecord 

        #endregion Cmdlet Overrides

    } // End GetFileRecordCommand class. 

    #endregion GetFileRecordCommand
}
