using System;
using System.IO;
using System.Collections.Generic;
using System.Management.Automation;
using InvokeIR.Win32;

namespace InvokeIR.PowerForensics.NTFS
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

        [Parameter()]
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

        [Parameter(Mandatory = false, Position = 0, ParameterSetName = "Path")]
        public string Path
        {
            get { return path; }
            set { path = value; }
        }
        private string path;

        /// <summary>
        /// 
        /// </summary> 

        [Parameter(Mandatory = false, ParameterSetName = "Index")]
        [Parameter(Mandatory = false, ParameterSetName = "Path")]
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
            NativeMethods.getVolumeName(ref volume);
        }

        protected override void ProcessRecord()
        {
            if (this.MyInvocation.BoundParameters.ContainsKey("Index"))
            {
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
                IndexEntry entry = IndexEntry.Get(path);

                string volume = NativeMethods.getVolumeName(ref path.Split(':')[0]);

                if (asbytes)
                {
                    WriteObject(FileRecord.GetRecordBytes(volume, (int)entry.FileIndex));
                }
                else
                {
                    WriteObject(new FileRecord(FileRecord.GetRecordBytes(volume, (int)entry.FileIndex), volume, path));
                }
            }
            else
            {
                List<FileRecord> recordList = new List<FileRecord>();

                IntPtr hVolume = NativeMethods.getHandle(volume);
                
                using(FileStream streamToRead = NativeMethods.getFileStream(hVolume))
                {
                    // Instantiate a null NonResident Object
                    NonResident Data = null;

                    // Get the FileRecord for the $MFT file
                    FileRecord mftRecord = new FileRecord(FileRecord.GetRecordBytes(volume, 0), volume);

                    byte[] mftBytes = MasterFileTable.GetBytes(streamToRead, volume);

                    // Determine the size of an MFT File Record
                    int bytesPerFileRecord = (int)(new NTFS.VolumeBootRecord(streamToRead)).BytesPerFileRecord;

                    // Calulate the number of entries in the MFT
                    int fileCount = mftBytes.Length / bytesPerFileRecord;

                    byte[] recordBytes = new byte[bytesPerFileRecord];

                    for (int index = 0; index < fileCount; index++)
                    {
                        Array.Copy(mftBytes, index * bytesPerFileRecord, recordBytes, 0, recordBytes.Length);

                        // Take UpdateSequence into account
                        ushort usoffset = BitConverter.ToUInt16(recordBytes, 4); ;
                        ushort ussize = BitConverter.ToUInt16(recordBytes, 6);

                        if (ussize != 0)
                        {
                            byte[] usnBytes = new byte[2];
                            Array.Copy(recordBytes, usoffset, usnBytes, 0, usnBytes.Length);
                            ushort UpdateSequenceNumber = BitConverter.ToUInt16(usnBytes, 0);

                            byte[] UpdateSequenceArray = new byte[(2 * ussize)];
                            Array.Copy(recordBytes, (usoffset + 2), UpdateSequenceArray, 0, UpdateSequenceArray.Length);

                            recordBytes[0x1FE] = UpdateSequenceArray[0];
                            recordBytes[0x1FF] = UpdateSequenceArray[1];
                            recordBytes[0x3FE] = UpdateSequenceArray[2];
                            recordBytes[0x3FF] = UpdateSequenceArray[3];
                        }

                        WriteObject(new FileRecord(recordBytes, volume));
                    }
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
