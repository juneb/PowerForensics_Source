using System;
using System.IO;
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
    public class GetChildItemRawCommand : PSCmdlet
    {

        #region Parameters

        /// <summary> 
        /// This parameter provides the Name of the Volume
        /// for which the FileRecord object should be
        /// returned.
        /// </summary> 

        [Alias("FilePath")]
        [Parameter(Position = 0)]
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

            if (!(this.MyInvocation.BoundParameters.ContainsKey("Path")))
            {
                path = this.SessionState.PSVariable.GetValue("pwd").ToString();
            }
        }

        protected override void ProcessRecord()
        {
            try
            {
                foreach (IndexEntry index in IndexEntry.GetInstances(path))
                {
                    WriteObject(index);
                }
            }
            catch
            {
                WriteObject(null);
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
