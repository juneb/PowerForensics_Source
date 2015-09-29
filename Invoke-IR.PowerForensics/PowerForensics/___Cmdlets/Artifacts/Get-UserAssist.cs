using System;
using System.Management.Automation;
using InvokeIR.PowerForensics.Artifacts;
using InvokeIR.PowerForensics.Registry;
using InvokeIR.PowerForensics.Utilities;

namespace InvokeIR.PowerForensics.Cmdlets
{
    #region GetUserAssistCommand
    /// <summary> 
    /// This class implements the Get-UserAssist cmdlet. 
    /// </summary> 

    [Cmdlet(VerbsCommon.Get, "UserAssist")]
    public class GetUserAssistCommand : Cmdlet
    {
        #region Parameters

        /// <summary> 
        /// This parameter provides the the path of the Registry Hive to parse.
        /// </summary> 

        [Alias("HivePath")]
        [Parameter(Mandatory = true, Position = 0)]
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
            string Key = @"Software\Microsoft\Windows\CurrentVersion\Explorer\UserAssist";

            byte[] bytes = Registry.Helper.GetHiveBytes(hivePath);

            NamedKey[] FileSubKey = NamedKey.GetInstances(bytes, hivePath, Key);

            foreach (NamedKey key in FileSubKey)
            {
                foreach (NamedKey nk in key.GetSubKeys(bytes))
                {
                    if (nk.NumberOfValues != 0)
                    {
                        foreach (ValueKey vk in nk.GetValues(bytes))
                        {
                            WriteObject(new UserAssist(vk, bytes));
                        }
                    }
                }
            }

        } // ProcessRecord 

        protected override void EndProcessing()
        {
            GC.Collect();
        }

        #endregion Cmdlet Overrides

    } // End GetUserAssistCommand class. 

    #endregion GetUserAssistCommand
}
