using System;
using System.Management.Automation;
using InvokeIR.PowerForensics.NTFS;

namespace InvokeIR.PowerForensics.Cmdlets
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
        protected override void ProcessRecord()
        {

            string[] paths = path.Split('\\');
            
            // Determine Volume Name
            string volume = @"\\.\" + paths[0];

            int index = -1;

            IndexEntry[] arrayEntry = null;

            for (int i = 0; i < paths.Length; i++)
            {
                
                if (index == -1)
                {
                    index = 5;
                }
                else
                {
                    foreach (IndexEntry entry in arrayEntry)
                    {
                        if (entry.Name == paths[i])
                        {
                            index = (int)entry.FileIndex;
                        }
                    }
                }

                arrayEntry = IndexEntry.Get(volume, index);
               
            }

            foreach (IndexEntry entry in arrayEntry)
            {
                WriteObject(entry);
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
