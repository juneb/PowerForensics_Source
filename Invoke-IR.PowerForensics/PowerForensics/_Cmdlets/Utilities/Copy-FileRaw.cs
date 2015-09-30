using System;
using System.IO;
using System.Management.Automation;
using InvokeIR.Win32;
using PowerForensics.Ntfs;

namespace PowerForensics.Cmdlets
{
    #region CopyFileRawCommand
    
    /// <summary> 
    /// This class implements the Get-Prefetch cmdlet. 
    /// </summary> 
    [Cmdlet(VerbsCommon.Copy, "FileRaw")]
    public class CopyFileRawCommand : PSCmdlet
    {
        #region Parameters

        /// <summary> 
        /// This parameter provides the Name of the Volume
        /// for which the FileRecord object should be
        /// returned.
        /// </summary> 
        [Alias("FilePath")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Path")]
        public string Path
        {
            get { return path; }
            set { path = value; }
        }
        private string path;

        /// <summary> 
        /// This parameter provides the MFTIndexNumber for the 
        /// FileRecord object that will be returned.
        /// </summary> 
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Index")]
        public int Index
        {
            get { return index; }
            set { index = value; }
        }
        private int index;

        /// <summary> 
        /// This parameter provides the MFTIndexNumber for the 
        /// FileRecord object that will be returned.
        /// </summary> 
        [Parameter(Mandatory = true, Position = 1)]
        public string Destination
        {
            get { return destination; }
            set { destination = value; }
        }
        private string destination;

        #endregion Parameters

        #region Cmdlet Overrides

        /// <summary> 
        /// 
        /// </summary> 
        protected override void BeginProcessing()
        {
            NativeMethods.checkAdmin();
        }

        /// <summary> 
        /// The ProcessRecord method calls ManagementClass.GetInstances() 
        /// method to iterate through each BindingObject on each system specified.
        /// </summary> 
        protected override void ProcessRecord()
        {
            // Determine Volume Name
            string volume = @"\\.\" + path.Split('\\')[0];

            int indexNumber = 0;

            if (ParameterSetName == "Path")
            {
                IndexEntry indexEntry = IndexEntry.Get(path);
                indexNumber = (int)indexEntry.RecordNumber;
            }
            else
            {
                indexNumber = index;
            }

            FileRecord record = FileRecord.Get(volume, indexNumber, true);

            byte[] fileBytes = record.GetBytes();

            // Open file for writing
            FileStream streamToWrite = new FileStream(destination, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            // Writes a block of bytes to this stream using data from a byte array.
            streamToWrite.Write(fileBytes, 0, fileBytes.Length);
            // Close file stream
            streamToWrite.Close();
        }

        #endregion Cmdlet Overrides
    }
    #endregion CopyFileRawCommand
}
