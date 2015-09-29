using System;

namespace InvokeIR.PowerForensics.Ntfs
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

        #region StaticMethods
        
        internal static FileRecord GetFileRecord(string volume)
        {
            return FileRecord.Get(volume, BADCLUS_INDEX, true);
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

        #endregion StaticMethods
    }
}
