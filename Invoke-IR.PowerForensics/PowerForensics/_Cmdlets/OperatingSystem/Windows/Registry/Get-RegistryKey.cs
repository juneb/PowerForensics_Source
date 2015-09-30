using System;
using System.Management.Automation;
using PowerForensics.Registry;

namespace PowerForensics.Cmdlets
{
    #region GetRegistryKeyCommand
    
    /// <summary> 
    /// This class implements the Get-RegistryKey cmdlet. 
    /// </summary> 
    [Cmdlet(VerbsCommon.Get, "RegistryKey", DefaultParameterSetName = "Path")]
    public class GetRegistryKeyCommand : PSCmdlet
    {
        #region Parameters

        /// <summary> 
        /// This parameter provides the Name of the Volume
        /// for which the FileRecord object should be
        /// returned.
        /// </summary> 
        [Parameter(ParameterSetName = "Path")]
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
        [Parameter(ParameterSetName = "Bytes")]
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

                WriteObject(NamedKey.GetInstances(path, key));
            }

        }

        #endregion Cmdlet Overrides
    }

    #endregion GetRegistryKeyCommand
}
