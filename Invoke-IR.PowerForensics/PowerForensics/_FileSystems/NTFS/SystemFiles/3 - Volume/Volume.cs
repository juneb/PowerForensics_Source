using System;
using System.IO;
using InvokeIR.Win32;

namespace InvokeIR.PowerForensics.NTFS
{
    public class Volume
    {
        internal static FileRecord GetFileRecord(string volume)
        {
            int index = IndexNumber.Get(volume, "\\$Volume");
            return new FileRecord(FileRecord.GetBytes(volume, index));
        }

        internal static VolumeName GetVolumeNameAttr(FileRecord fileRecord)
        {
            foreach (Attr attr in fileRecord.Attribute)
            {
                if (attr.Name == "VOLUME_NAME")
                {
                    return attr as VolumeName;
                }
            }
            throw new Exception("No VOLUME_NAME attribute found.");
        }

        internal static VolumeInformation GetVolumeInformationAttr(FileRecord fileRecord)
        {
            foreach (Attr attr in fileRecord.Attribute)
            {
                if (attr.Name == "VOLUME_INFORMATION")
                {
                    return attr as VolumeInformation;
                }
            }
            throw new Exception("No VOLUME_INFORMATION attribute found.");
        }
    }
}
