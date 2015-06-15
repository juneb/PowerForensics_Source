using System;
using System.Management.Automation;

namespace InvokeIR.PowerForensics.Cmdlets
{

    #region GetTimezoneCommand
    /// <summary> 
    /// This class implements the Get-Timezone cmdlet. 
    /// </summary> 

    [Cmdlet(VerbsCommon.Get, "Timezone")]
    public class GetTimezoneCommand : Cmdlet
    {

        #region Cmdlet Overrides

        /// <summary> 
        /// The ProcessRecord method calls TimeZone.CurrentTimeZone to return a TimeZone object.
        /// </summary> 
        protected override void ProcessRecord()
        {
            // Output the current TimeZone object
            WriteObject(TimeZone.CurrentTimeZone);
        } // ProcessRecord 

        protected override void EndProcessing()
        {
            GC.Collect();
        }

        #endregion Cmdlet Overrides

    } // End GetTimezoneCommand class. 
    
    #endregion GetTimezoneCommand

}
