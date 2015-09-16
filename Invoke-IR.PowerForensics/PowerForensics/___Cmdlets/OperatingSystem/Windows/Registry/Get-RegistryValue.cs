using System;
using System.Management.Automation;
using InvokeIR.PowerForensics.Registry;

namespace InvokeIR.PowerForensics.Cmdlets
{

    #region GetRegistryValueCommand
    /// <summary> 
    /// This class implements the Get-RegistryValue cmdlet. 
    /// </summary> 

    [Cmdlet(VerbsCommon.Get, "RegistryValue", DefaultParameterSetName = "Path")]
    public class GetRegistryValueCommand : PSCmdlet
    {

        #region Parameters

        /// <summary> 
        /// This parameter provides the Name of the Volume
        /// for which the FileRecord object should be
        /// returned.
        /// </summary> 

        [Parameter(Mandatory = true, ParameterSetName = "Path")]
        public string HivePath
        {
            get { return path; }
            set { path = value; }
        }
        private string path;

        /// <summary> 
        /// This parameter provides the Name of the Volume
        /// for which the FileRecord object should be
        /// returned.
        /// </summary> 

        [Parameter(Mandatory = true, ParameterSetName = "Bytes")]
        public byte[] Bytes
        {
            get { return bytes; }
            set { bytes = value; }
        }
        private byte[] bytes;

        /// <summary> 
        /// This parameter provides the FileName for the 
        /// FileRecord object that will be returned.
        /// </summary> 

        [Parameter()]
        public string Key
        {
            get { return key; }
            set { key = value; }
        }
        private string key;

        /// <summary> 
        /// This parameter provides the FileName for the 
        /// FileRecord object that will be returned.
        /// </summary> 

        [Parameter()]
        public string Value
        {
            get { return val; }
            set { val = value; }
        }
        private string val;

        #endregion Parameters

        #region Cmdlet Overrides

        /// <summary> 
        /// The ProcessRecord instantiates a FileRecord objects that
        /// corresponds to the file(s) that is/are specified.
        /// </summary> 

        protected override void ProcessRecord()
        {
            if (ParameterSetName == "Path")
            {
                if (!(MyInvocation.BoundParameters.ContainsKey("Key")))
                {
                    key = null;
                }

                if (MyInvocation.BoundParameters.ContainsKey("Value"))
                {
                    WriteObject(ValueKey.Get(path, key, val));
                }
                else
                {
                    WriteObject(ValueKey.GetInstances(path, key));
                }
            }

            
            /*if (ParameterSetName == "Path")
            {
                bytes = Helper.GetHiveBytes(path);
            }

            NamedKey hiveroot = Helper.GetRootKey(bytes, path);

            NamedKey nk = hiveroot;

            if (MyInvocation.BoundParameters.ContainsKey("Key"))
            {
                foreach (string k in key.Split('\\'))
                {
                    foreach (NamedKey n in nk.GetSubKeys(bytes))
                    {
                        if (n.Name == k)
                        {
                            nk = n;
                        }
                    }
                }
            }

            ValueKey[] values = nk.GetValues(bytes);
            
            if (MyInvocation.BoundParameters.ContainsKey("Value"))
            {
                foreach (ValueKey v in values)
                {
                    if (v.Name == val)
                    {
                        WriteObject(v);
                    }
                }
            }
            else
            {
                WriteObject(values);
            }
            */
        } // ProcessRecord 

        protected override void EndProcessing()
        {
            GC.Collect();
        }

        #endregion Cmdlet Overrides

    } // End GetRegistryValueCommand class. 

    #endregion GetRegistryValueCommand
}