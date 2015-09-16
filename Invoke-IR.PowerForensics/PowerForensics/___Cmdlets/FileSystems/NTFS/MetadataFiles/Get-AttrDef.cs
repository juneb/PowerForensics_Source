using System;
using System.Management.Automation;
using InvokeIR.Win32;
using InvokeIR.PowerForensics.NTFS;

namespace InvokeIR.PowerForensics.Cmdlets
{

    #region GetAttrDefCommand
    /// <summary> 
    /// This class implements the Get-AttrDef cmdlet. 
    /// </summary> 

    [Cmdlet(VerbsCommon.Get, "AttrDef")]
    public class GetAttrDefCommand : Cmdlet
    {

        #region Parameters

        /// <summary> 
        /// This parameter provides the Volume Name for the 
        /// AttrDef objects that will be returned.
        /// </summary> 

        [Parameter(Position = 0)]
        public string VolumeName
        {
            get { return volume; }
            set { volume = value; }
        }
        private string volume;

        #endregion Parameters

        #region Cmdlet Overrides

        /// <summary> 
        /// The ProcessRecord method calls AttrDef.GetInstances() 
        /// method to iterate through each AttrDef object on the specified volume.
        /// </summary> 

        protected override void BeginProcessing()
        {
            NativeMethods.checkAdmin();
            NativeMethods.getVolumeName(ref volume);
        }
        
        protected override void ProcessRecord()
        {
            WriteObject(AttrDef.GetInstances(volume));
        } // ProcessRecord 

        #endregion Cmdlet Overrides

        protected override void EndProcessing()
        {
            GC.Collect();
        }

    } // End GetProcCommand class. 

    #endregion GetAttrDefCommand

}
