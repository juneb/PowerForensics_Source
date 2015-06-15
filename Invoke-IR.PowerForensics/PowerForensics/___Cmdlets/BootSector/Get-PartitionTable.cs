using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Management.Automation;
using InvokeIR.Win32;
using InvokeIR.PowerForensics.NTFS;

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

            [Alias("DevicePath")]
            [Parameter(Mandatory = true, Position = 0)]
            public string Path
            {
                get { return devicePath; }
                set { devicePath = value; }
            }
            private string devicePath;

            #endregion Parameters

            #region Cmdlet Overrides

            /// <summary> 
            /// The ProcessRecord instantiates a MasterBootRecord Object
            /// and outputs all Partitions that are not of the EMPTY type
            /// </summary> 

            protected override void ProcessRecord()
            {
                MasterBootRecord mbr = new MasterBootRecord(devicePath);

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
                    GuidPartitionTable gpt = new GuidPartitionTable(devicePath);
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
