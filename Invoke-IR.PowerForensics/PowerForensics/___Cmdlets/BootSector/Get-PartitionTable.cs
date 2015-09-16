using System;
using System.Management.Automation;
using InvokeIR.Win32;

namespace InvokeIR.PowerForensics.Cmdlets
{

    #region GetPartitionTableCommand
        /// <summary> 
        /// This class implements the Get-PartitionTable cmdlet. 
        /// </summary> 

        [Cmdlet(VerbsCommon.Get, "PartitionTable", SupportsShouldProcess = true)]
        public class GetPartitionTableCommand : Cmdlet
        {

            #region Parameters

            /// <summary> 
            /// This parameter provides the DriveName for the 
            /// Partition Table that will be returned.
            /// </summary> 

            [Alias("DrivePath")]
            [Parameter(Mandatory = true, Position = 0)]
            public string Path
            {
                get { return drivePath; }
                set { drivePath = value; }
            }
            private string drivePath;

            #endregion Parameters

            #region Cmdlet Overrides

            /// <summary> 
            /// The ProcessRecord instantiates a MasterBootRecord Object
            /// and outputs all Partitions that are not of the EMPTY type
            /// </summary> 

            protected override void BeginProcessing()
            {
                // Ensure cmdlet is being run as Administrator
                NativeMethods.checkAdmin();
                // Check that drivePath is valid
                NativeMethods.getDriveName(drivePath);
            }

            protected override void ProcessRecord()
            {
                MasterBootRecord mbr = MasterBootRecord.Get(drivePath);

                if (mbr.PartitionTable[0].SystemID != "EFI_GPT_DISK")
                {
                    foreach (PartitionEntry partition in mbr.PartitionTable)
                    {

                        if (partition.SystemID != "EMPTY")
                        {
                            WriteObject(partition);
                        }

                        else if (partition.SystemID.Contains("EXTENDED"))
                        {
                            // Add code to parse EXTENDED partitions
                        }
                    }
                }
                else
                {
                    GuidPartitionTable gpt = new GuidPartitionTable(drivePath);
                    foreach (GuidPartitionTableEntry entry in gpt.PartitionTable)
                    {
                        WriteObject(entry);
                    }
                }

            } // ProcessRecord 

            protected override void EndProcessing()
            {
                GC.Collect();
            }

            #endregion Cmdlet Overrides

        } // End GetPartitionTableCommand class. 

        #endregion GetPartitionTableCommand

}
