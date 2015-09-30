using System;
using System.Management.Automation;
using PowerForensics.Artifacts;
using PowerForensics.Registry;
using PowerForensics.Utilities;

namespace PowerForensics.Cmdlets
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
        [Parameter(Position = 0)]
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
            if (MyInvocation.BoundParameters.ContainsKey("Path"))
            {
                WriteObject(NetworkList.GetInstances(), true);
            }
            else
            {
                WriteObject(NetworkList.GetInstances(hivePath), true);
            }
        }

        #endregion Cmdlet Overrides
    }

    #endregion GetNetworkListCommand
}
