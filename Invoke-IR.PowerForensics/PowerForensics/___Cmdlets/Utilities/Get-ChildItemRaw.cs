using System;
using System.Collections.Generic;
using System.Management.Automation;
using InvokeIR.Win32;

namespace InvokeIR.PowerForensics.NTFS
{

    #region GetChildItemRawCommand
    /// <summary> 
    /// This class implements the Get-ChildItemRaw cmdlet. 
    /// </summary> 

    [Cmdlet(VerbsCommon.Get, "ChildItemRaw")]
    public class GetChildItemRawCommand : Cmdlet
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
            foreach (IndexEntry index in IndexEntry.GetInstances(path))
            {
                WriteObject(index);
            }
        } // ProcessRecord 

        protected override void EndProcessing()
        {
            GC.Collect();
        }

        #endregion Cmdlet Overrides

    } // End GetChildItemRawCommand class. 
    #endregion GetChildItemRawCommand

}
