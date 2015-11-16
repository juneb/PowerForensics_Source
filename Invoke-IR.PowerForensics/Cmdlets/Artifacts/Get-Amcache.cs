using System;
using System.Management.Automation;
using InvokeIR.Win32;
using PowerForensics.Artifacts;
using PowerForensics.Registry;

namespace PowerForensics.Cmdlets
{
    #region GetAmcacheCommand
    
    /// <summary> 
    /// This class implements the Get-AppCompatCache cmdlet. 
    /// </summary> 
    [Cmdlet(VerbsCommon.Get, "Amcache", DefaultParameterSetName = "ByVolume")]
    public class GetAmcacheCommand : PSCmdlet
    {
        #region Parameters

        /// <summary> 
        /// 
        /// </summary> 
        [Parameter(Position = 0, ParameterSetName = "ByVolume")]
        public string VolumeName
        {
            get { return volume; }
            set { volume = value; }
        }
        private string volume;

        /// <summary> 
        /// This parameter provides the the path of the Registry Hive to parse.
        /// </summary> 
        [Alias("Path")]
        [Parameter(Mandatory = true, ParameterSetName = "ByPath")]
        public string HivePath
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
        protected override void BeginProcessing()
        {
            NativeMethods.checkAdmin();

            if (ParameterSetName == "ByVolume")
            {
                NativeMethods.getVolumeName(ref volume);
            }
        }

        /// <summary> 
        /// The ProcessRecord method reads the raw contents of the Amcache.hve into memory and parses its
        /// values to create/output AppCompat Objects.
        /// </summary>  
        protected override void ProcessRecord()
        {
            switch (ParameterSetName)
            {
                case "ByVolume":
                    WriteObject(Amcache.GetInstances(volume), true);
                    break;
                case "ByPath":
                    WriteObject(Amcache.GetInstancesByPath(hivePath), true);
                    break;
            }
        }

        #endregion Cmdlet Overrides
    }

    #endregion GetAmcacheCommand
}
