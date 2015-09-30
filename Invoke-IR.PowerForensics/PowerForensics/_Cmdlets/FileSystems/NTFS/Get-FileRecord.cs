using System;
using System.IO;
using System.Collections.Generic;
using System.Management.Automation;
using PowerForensics.Ntfs;
using InvokeIR.Win32;

namespace PowerForensics.Cmdlet
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
        [Parameter(ParameterSetName = "ByIndex")]
        public string VolumeName
        {
            get { return volume; }
            set { volume = value; }
        }
        private string volume;

        /// <summary>
        /// 
        /// </summary> 
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "MFTPath")]
        //[Parameter(Mandatory = true, Position = 0, ParameterSetName = "MFTPathByIndex")]
        //[Parameter(Mandatory = true, Position = 0, ParameterSetName = "MFTPathByPath")]
        public string MftPath
        {
            get { return mftpath; }
            set { mftpath = value; }
        }
        private string mftpath;

        /// <summary> 
        /// This parameter provides the IndexNumber for the 
        /// FileRecord object that will be returned.
        /// </summary> 
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "ByIndex")]
        //[Parameter(Mandatory = true, Position = 1, ParameterSetName = "MFTPathByIndex")]
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
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "ByPath", ValueFromPipelineByPropertyName = true)]
        //[Parameter(Mandatory = true, Position = 1, ParameterSetName = "MFTPathByPath", ValueFromPipelineByPropertyName = true)]
        public string Path
        {
            get { return path; }
            set { path = value; }
        }
        private string path;

        /// <summary>
        /// 
        /// </summary> 
        [Parameter(ParameterSetName = "ByIndex")]
        [Parameter(ParameterSetName = "ByPath")]
        //[Parameter(ParameterSetName = "MFTPathByIndex")]
        //[Parameter(ParameterSetName = "MFTPathByPath")]
        public SwitchParameter AsBytes
        {
            get { return asbytes; }
            set { asbytes = value; }
        }
        private SwitchParameter asbytes;

        #endregion Parameters

        #region Cmdlet Overrides

        /// <summary> 
        /// 
        /// </summary> 
        protected override void BeginProcessing()
        {
            NativeMethods.checkAdmin();
            if ((ParameterSetName == "ByIndex") || (ParameterSetName == "Default"))
            {
                NativeMethods.getVolumeName(ref volume);
            }
        }

        /// <summary> 
        /// The ProcessRecord instantiates a FileRecord objects that
        /// corresponds to the file(s) that is/are specified.
        /// </summary> 
        protected override void ProcessRecord()
        {
            switch (ParameterSetName)
            {
                case "ByIndex":
                    if (asbytes)
                    {
                        WriteObject(FileRecord.GetRecordBytes(volume, indexNumber));
                    }
                    else
                    {
                        WriteObject(FileRecord.Get(volume, indexNumber, false));
                    }
                    break;
                case "ByPath":
                    if (asbytes)
                    {
                        WriteObject(FileRecord.GetRecordBytes(path));
                    }
                    else
                    {
                        WriteObject(FileRecord.Get(path, false));
                    }
                    break;
                /*case "MFTPathByIndex":
                    if (asbytes)
                    {
                        //WriteObject(FileRecord.Get)
                    }
                    else
                    {

                    }
                    break;
                case "MFTPathByPath":
                    if (asbytes)
                    {

                    }
                    else
                    {

                    }
                    break;*/
                case "MFTPath":
                    WriteObject(FileRecord.GetInstancesByPath(mftpath), true);
                    break;
                case "Default":
                    WriteObject(FileRecord.GetInstances(volume), true);
                    break;
            }
        }

        #endregion Cmdlet Overrides
    } 

    #endregion GetFileRecordCommand
}
