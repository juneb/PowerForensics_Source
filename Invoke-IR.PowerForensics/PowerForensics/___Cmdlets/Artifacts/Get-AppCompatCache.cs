using System;
using System.Management.Automation;
using InvokeIR.PowerForensics.Artifacts;
using InvokeIR.PowerForensics.Registry;

namespace InvokeIR.PowerForensics.Cmdlets
{

    #region GetAppCompatCacheCommand
    /// <summary> 
    /// This class implements the Get-AppCompatCache cmdlet. 
    /// </summary> 

    [Cmdlet(VerbsCommon.Get, "AppCompatCache")]
    public class GetAppCompatCacheCommand : Cmdlet
    {

        #region Cmdlet Overrides

        /// <summary> 
        /// The ProcessRecord method calls TimeZone.CurrentTimeZone to return a TimeZone object.
        /// </summary>  
        protected override void ProcessRecord()
        {
            string HivePath = @"C:\Windows\AppCompat\Programs\Amcache.hve";
            string Key = @"Root\File";

            byte[] bytes = Registry.Helper.GetHiveBytes(HivePath);

            NamedKey[] FileSubKey = NamedKey.GetInstances(bytes, HivePath, Key);

            foreach (NamedKey key in FileSubKey)
            {
                foreach (NamedKey nk in key.GetSubKeys(bytes))
                {
                    WriteObject(new AppCompat(nk, bytes));
                }
            }

        } // ProcessRecord 

        protected override void EndProcessing()
        {
            GC.Collect();
        }

        #endregion Cmdlet Overrides

    } // End GetAppCompatCacheCommand class. 

    #endregion GetAppCompatCacheCommand

}
