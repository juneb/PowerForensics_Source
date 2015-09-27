using System;
using System.Text;

namespace InvokeIR.PowerForensics.Ntfs
{
    #region VolumeClass

    public class Volume
    {
        #region Constants
        
        public const int VOLUME_INDEX = 3;
        
        #endregion Constants

        #region StaticMethods

        internal static FileRecord GetFileRecord(string volume)
        {
            return new FileRecord(FileRecord.GetRecordBytes(volume, VOLUME_INDEX), volume, true);
        }

        internal static VolumeName GetVolumeNameAttr(FileRecord fileRecord)
        {
            foreach (Attr attr in fileRecord.Attribute)
            {
                if (attr.Name == Attr.ATTR_TYPE.VOLUME_NAME)
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
                if (attr.Name == Attr.ATTR_TYPE.VOLUME_INFORMATION)
                {
                    return attr as VolumeInformation;
                }
            }
            throw new Exception("No VOLUME_INFORMATION attribute found.");
        }

        #endregion StaticMethods
    }

    #endregion VolumeClass
}
