using System;
using System.IO;

namespace InvokeIR.PowerForensics.NTFS
{
    class IndexNumber
    {
        internal static int Get(string volume, string path)
        {

            string[] paths = path.Split('\\');

            // Start index at volume root directory (C:\)
            int index = 5;

            for (int i = 1; i < paths.Length; i++)
            {
                IndexEntry[] arrayEntry = IndexEntry.Get(volume, index);
                foreach (IndexEntry entry in arrayEntry)
                {
                    if (entry.Name.ToUpper() == paths[i].ToUpper())
                    {
                        index = (int)entry.FileIndex;
                    }
                }
            }
            return index;
        }

        internal static int Get(FileStream streamToRead, byte[] MFT, string path)
        {

            string[] paths = path.Split('\\');

            // Start index at volume root directory (C:\)
            int index = 5;

            for (int i = 1; i < paths.Length; i++)
            {
                IndexEntry[] arrayEntry = IndexEntry.Get(streamToRead, MFT, index);
                foreach (IndexEntry entry in arrayEntry)
                {
                    if (entry.Name == paths[i])
                    {
                        index = (int)entry.FileIndex;
                    }
                }
            }
            return index;
        }
    }
}
