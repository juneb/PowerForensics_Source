using System;
using System.Text;
using InvokeIR.PowerForensics.NTFS;

namespace InvokeIR.PowerForensics.Artifacts
{
    // https://msdn.microsoft.com/en-us/library/cc248285.aspx
    public class AppCompat
    {
        #region Properties

        public readonly string Path;

        #endregion Properties

        #region Constructors

        internal AppCompat(byte[] bytes)
        {
            Path = Encoding.Unicode.GetString(bytes);
        }

        #endregion Constructors

        internal static AppCompat Get(byte[] bytes)
        {
            return new AppCompat(bytes);
        }

        internal static AppCompat[] GetInstances(string path)
        {
            string volume = "\\\\.\\" + path.Split('\\')[0];
            IndexEntry index = IndexEntry.Get(path);
            FileRecord record = new FileRecord(FileRecord.GetRecordBytes(volume, (int)index.RecordNumber), volume, true);
            byte[] fileBytes = record.GetBytes(volume);
            return null;
        }

        internal static AppCompat GetInstances(string volume, int recordNumber)
        {
            FileRecord record = new FileRecord(FileRecord.GetRecordBytes(volume, recordNumber), volume, true);
            byte[] fileBytes = record.GetBytes(volume);
            return null;
        }
    }
}
