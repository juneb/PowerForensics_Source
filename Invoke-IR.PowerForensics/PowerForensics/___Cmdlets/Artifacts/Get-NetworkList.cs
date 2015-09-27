using System;
using System.Management.Automation;
using InvokeIR.PowerForensics.Artifacts;
using InvokeIR.PowerForensics.Registry;
using InvokeIR.PowerForensics.Utilities;

namespace InvokeIR.PowerForensics.Cmdlets
{

    #region GetNetworkListCommand
    /// <summary> 
    /// This class implements the Get-NetworkList cmdlet. 
    /// </summary> 

    [Cmdlet(VerbsCommon.Get, "NetworkList")]
    public class GetNetworkListCommand : PSCmdlet
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
        /// 
        /// </summary>  
        protected override void ProcessRecord()
        {
            if (!(this.MyInvocation.BoundParameters.ContainsKey("Path")))
            {
                hivePath = @"C:\windows\system32\config\SOFTWARE";
            }

            string Key = @"Software\Microsoft\Windows NT\CurrentVersion\NetworkList\Signatures";

            byte[] bytes = Registry.Helper.GetHiveBytes(hivePath);

            NamedKey[] SignatureKey = NamedKey.GetInstances(bytes, hivePath, Key);

            foreach (NamedKey key in SignatureKey)
            {
                if (key.NumberOfSubKeys != 0)
                {
                    foreach (NamedKey nk in key.GetSubKeys(bytes))
                    {
                        WriteObject(new NetworkList(nk, bytes));
                    }
                }
            }

        } // ProcessRecord 

        protected override void EndProcessing()
        {
            GC.Collect();
        }

        #endregion Cmdlet Overrides

    } // End GetNetworkListCommand class. 

    #endregion GetNetworkListCommand

}
