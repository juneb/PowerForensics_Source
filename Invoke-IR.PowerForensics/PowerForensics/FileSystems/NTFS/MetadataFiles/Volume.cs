using System;
using System.Text;
using InvokeIR.Win32;

namespace InvokeIR.PowerForensics.Ntfs
{
    #region VolumeClass

    public class Volume
    {
        #region Constants
        
        public const int VOLUME_INDEX = 3;
        
        #endregion Constants

        #region StaticMethods

        #region GetVolumeNameMethod

        public static VolumeName GetVolumeName(string volume)
        {
            FileRecord record = FileRecord.Get(volume, VOLUME_INDEX, true);
            return GetVolumeName(record);
        }

        public static VolumeName GetVolumeNameByPath(string path)
        {
            FileRecord record = FileRecord.Get(path, true);
            return GetVolumeName(record);
        }

        private static VolumeName GetVolumeName(FileRecord fileRecord)
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

        #endregion GetVolumeNameMethod

        #region GetVolumeInformationMethod

        public static VolumeInformation GetVolumeInformation(string volume)
        {
            FileRecord record = FileRecord.Get(volume, VOLUME_INDEX, true);
            return GetVolumeInformation(record);
        }

        public static VolumeInformation GetVolumeInformationByPath(string path)
        {
            FileRecord record = FileRecord.Get(path, true);        
            return GetVolumeInformation(record);
        }

        private static VolumeInformation GetVolumeInformation(FileRecord fileRecord)
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

        #endregion GetVolumeInformationMethod

        internal static FileRecord GetFileRecord(string volume)
        {
            return FileRecord.Get(volume, VOLUME_INDEX, true);
        }

        #endregion StaticMethods
    }

    #endregion VolumeClass
}
