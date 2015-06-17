using System;
using System.Management;
using System.Management.Automation;

namespace InvokeIR.PowerForensics.Cmdlets
{

    #region GetVolumeShadowCopyCommand
    /// <summary> 
    /// This class implements the Get-VolumeShadowCopy cmdlet. 
    /// </summary> 

    [Cmdlet(VerbsCommon.Get, "VolumeShadowCopy")]
    public class GetVolumeShadowCopyCommand : Cmdlet
    {

        #region Cmdlet Overrides

        /// <summary> 
        /// The ProcessRecord method calls TimeZone.CurrentTimeZone to return a TimeZone object.
        /// </summary> 
        protected override void ProcessRecord()
        {

            ManagementClass vscClass = new ManagementClass("Win32_ShadowCopy");

            foreach (ManagementObject obj in vscClass.GetInstances())
            {
                WriteObject(obj);
            }

        } // ProcessRecord 

        protected override void EndProcessing()
        {
            GC.Collect();
        }

        #endregion Cmdlet Overrides

    } // End GetVolumeShadowCopyCommand class. 

    #endregion GetVolumeShadowCopyCommand

}
