using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using InvokeIR.Win32;

namespace InvokeIR.PowerForensics.NTFS
{
    #region AttrDefClass

    class AttrDef
    {
        #region Enums

        [FlagsAttribute]
        internal enum ATTR_DEF_ENTRY
        {
            INDEX = 0x02,
            ALWAYS_RESIDENT = 0x40,
            ALWAYS_NONRESIDENT = 0x80
        }
        
        #endregion Enums

        #region Properties

        public readonly string Name;
        public readonly uint Type;
        public readonly uint DisplayRule;
        public readonly string CollationRule;
        public readonly string Flags;
        public readonly ulong MinSize;
        public readonly ulong MaxSize;

        #endregion Properties

        #region Constructors

        internal AttrDef(byte[] bytes)
        {
            byte[] nameBytes = new byte[0x80];
            Array.Copy(bytes, 0, nameBytes, 0, nameBytes.Length);
            Name = Encoding.Unicode.GetString(nameBytes).TrimEnd('\0');
            Type = BitConverter.ToUInt32(bytes, 0x80);
            DisplayRule = BitConverter.ToUInt32(bytes, 0x84);
            #region CollationRuleSwitch
            
            switch (BitConverter.ToUInt32(bytes, 0x88))
            {
                case 0x00:
                    CollationRule = "Binary";
                    break;
                case 0x01: 
                    CollationRule = "Filename";
                    break;
                case 0x02: 
                    CollationRule = "Unicode String";
                    break;
                case 0x10: 
                    CollationRule = "Unsigned Long";
                    break;
                case 0x11: 
                    CollationRule = "SID";
                    break;
                case 0x12:
                    CollationRule = "Security Hash";
                    break;
                case 0x13:
                    CollationRule = "Multiple Unsigned Longs";
                    break;
                default:
                    CollationRule = "unknown";
                    break;
            }

            #endregion CollationRuleSwitch
            Flags = ((ATTR_DEF_ENTRY)BitConverter.ToUInt32(bytes, 0x8C)).ToString();
            MinSize = BitConverter.ToUInt64(bytes, 0x90);
            MaxSize = BitConverter.ToUInt64(bytes, 0x98);
        }

        #endregion Constructors

        #region GetInstancesMethod

        internal static AttrDef[] GetInstances(string volumeName)
        {
            // Get correct volume name from user input
            NativeMethods.getVolumeName(ref volumeName);

            // Get handle to Logical Volume
            IntPtr hVolume = NativeMethods.getHandle(volumeName);

            // Instantiate a List of AttrDef objects for output
            List<AttrDef> adList = new List<AttrDef>();

            // Create a FileStream object for the Volume
            using (FileStream streamToRead = NativeMethods.getFileStream(hVolume))
            {
                // Instantiate a VBR object
                VolumeBootRecord VBR = new VolumeBootRecord(streamToRead);

                // Calculate the offset of the AttrDef MFT record
                ulong attrDefOffset = ((VBR.MFTStartIndex * VBR.BytesPerCluster) + ((ulong)VBR.BytesPerFileRecord * 4));

                // Get the MFTRecord for the file with a record index of 4 ($AttrDef)
                FileRecord record = new FileRecord(NativeMethods.readDrive(streamToRead, attrDefOffset, (ulong)VBR.BytesPerFileRecord));

                // Get the content of the $AttrDef file in a byte array
                byte[] bytes = FileRecord.getFileBytes(streamToRead, record);

                // Iterate through 160 byte chunks (representing an AttrDef object)
                for (int i = 0; (i < bytes.Length) && (bytes[i] != 0); i += 160)
                {
                    byte[] attrDefBytes = new byte[160];

                    Array.Copy(bytes, i, attrDefBytes, 0, attrDefBytes.Length);
                    
                    // Intantiate a new AttrDef object and add it to the adList List of AttrDef objects
                    adList.Add(new AttrDef(attrDefBytes));
                }
            }

            NativeMethods.CloseHandle(hVolume);

            // Return an array of AttrDef objects
            return adList.ToArray();
        }

        #endregion GetInstancesMethod
    }

    #endregion AttrDefClass
}
