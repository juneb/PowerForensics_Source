using System;

namespace InvokeIR.PowerForensics.NTFS
{
    public class BadClus
    {
        #region Constants

        public const int BADCLUS_INDEX = 8;

        #endregion Constants

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
            return new FileRecord(FileRecord.GetRecordBytes(volume, BADCLUS_INDEX), volume, true);
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
