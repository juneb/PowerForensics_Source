using System;
using System.IO;
using System.Management.Automation;
using InvokeIR.Win32;
using InvokeIR.PowerForensics.NTFS;

namespace InvokeIR.PowerForensics.Cmdlets
{

    #region CopyFileRawCommand
    /// <summary> 
    /// This class implements the Get-Prefetch cmdlet. 
    /// </summary> 

    [Cmdlet(VerbsCommon.Copy, "FileRaw")]
    public class CopyFileRawCommand : Cmdlet
    {

        #region Parameters

        /// <summary> 
        /// This parameter provides the Name of the Volume
        /// for which the FileRecord object should be
        /// returned.
        /// </summary> 

        [Alias("FilePath")]
        [Parameter(Mandatory = true, Position = 0)]
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
        /// The ProcessRecord method calls ManagementClass.GetInstances() 
        /// method to iterate through each BindingObject on each system specified.
        /// </summary> 

        protected override void BeginProcessing()
        {
            NativeMethods.checkAdmin();
        }

        protected override void ProcessRecord()
        {
            // Determine Volume Name
            string volume = @"\\.\" + path.Split('\\')[0];

            IndexEntry indexEntry = IndexEntry.Get(path);

            // 
            FileRecord record = new FileRecord(FileRecord.GetRecordBytes(volume, (int)indexEntry.FileIndex), volume);

            byte[] fileBytes = record.GetBytes(volume);

            // Open file for writing
            FileStream streamToWrite = new FileStream(destination, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            // Writes a block of bytes to this stream using data from a byte array.
            streamToWrite.Write(fileBytes, 0, fileBytes.Length);
            // Close file stream
            streamToWrite.Close();
        } // ProcessRecord 

        protected override void EndProcessing()
        {
            GC.Collect();
        }

        #endregion Cmdlet Overrides

    } // End GetProcCommand class. 
    #endregion CopyFileRawCommand

}
