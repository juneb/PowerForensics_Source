using System;
using System.Management.Automation;
using InvokeIR.PowerForensics.Registry;

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
            ValueKey vk = ValueKey.Get(@"C:\Windows\system32\config\SYSTEM", @"ControlSet001\Control\TimeZoneInformation", "TimeZoneKeyName");
            TimeZone tz = TimeZone.CurrentTimeZone;

            WriteObject(new InvokeIR.PowerForensics.Artifacts.Timezone(System.Text.Encoding.Unicode.GetString(vk.GetData()), tz.StandardName, tz.DaylightName, tz.IsDaylightSavingTime(DateTime.Now)));
        } // ProcessRecord 

        protected override void EndProcessing()
        {
            GC.Collect();
        }

        #endregion Cmdlet Overrides

    } // End GetTimezoneCommand class. 
    
    #endregion GetTimezoneCommand

}
