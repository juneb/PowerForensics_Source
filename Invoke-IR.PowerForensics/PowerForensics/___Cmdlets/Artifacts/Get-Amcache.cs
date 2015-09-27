using System;
using System.Management.Automation;
using InvokeIR.PowerForensics.Artifacts;
using InvokeIR.PowerForensics.Registry;

namespace InvokeIR.PowerForensics.Cmdlets
{

    #region GetAmcacheCommand
    /// <summary> 
    /// This class implements the Get-AppCompatCache cmdlet. 
    /// </summary> 

    [Cmdlet(VerbsCommon.Get, "Amcache")]
    public class GetAmcacheCommand : PSCmdlet
    {
        #region Parameters

        /// <summary> 
        /// This parameter provides the the path of the Registry Hive to parse.
        /// </summary> 

        [Alias("HivePath")]
        [Parameter(Mandatory = false, Position = 0)]
        public string Path
        {
            get { return hivePath; }
            set { hivePath = value; }
        }
        private string hivePath;

        #endregion Parameters

        #region Cmdlet Overrides

        /// <summary> 
        /// The ProcessRecord method reads the raw contents of the Amcache.hve into memory and parses its
        /// values to create/output AppCompat Objects.
        /// </summary>  
        protected override void ProcessRecord()
        {
            if (!(this.MyInvocation.BoundParameters.ContainsKey("Path")))
            {
                hivePath = @"C:\Windows\AppCompat\Programs\Amcache.hve";    
            }
            
            string Key = @"Root\File";

            byte[] bytes = Registry.Helper.GetHiveBytes(hivePath);

            NamedKey[] FileSubKey = NamedKey.GetInstances(bytes, hivePath, Key);

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

    } // End GetAmcacheCommand class. 

    #endregion GetAmcacheCommand

}
