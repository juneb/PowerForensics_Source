using System;
using System.IO;
using InvokeIR.Win32;

namespace InvokeIR.PowerForensics.NTFS
{
    public class BadClus
    {
        #region Properties

        public readonly long Cluster;
        public readonly bool Bad;

        #endregion Properties

        #region Constructors

        internal BadClus(long cluster, bool bad)
        {
            Cluster = cluster;
            Bad = bad;
        }

        #endregion Constructors

        internal static FileRecord GetFileRecord(string volume)
        {
            int index = IndexNumber.Get(volume, "\\$BadClus");
            return new FileRecord(FileRecord.GetBytes(volume, index));
        }

        internal static NonResident GetBadStream(FileRecord fileRecord)
        {
            foreach (Attr attr in fileRecord.Attribute)
            {
                if (attr.NameString == "$Bad")
                {
                    return attr as NonResident;
                }
            }
            throw new Exception("No $Bad attribute found.");
        }
    }
}
