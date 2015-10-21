using System;

namespace PowerForensics.EseDb
{
    /*public class RootPage : Page
    {
        #region Properties

        public readonly uint InitialNumberOfPages;
        public readonly uint FatherDataPage;
        public readonly uint ExtentSpace;
        public readonly uint SpaceTreePageNumber;

        #endregion Properties

        #region Constructors

        internal RootPage(byte[] bytes, int offset)
        {
            Header = new PageHeader(bytes, offset);
            Tags = DataTag.GetInstances(bytes, offset);
            InitialNumberOfPages = BitConverter.ToUInt32(bytes, Tags[0].Offset + offset);
            FatherDataPage = BitConverter.ToUInt32(bytes, Tags[0].Offset + 0x04 + offset);
            ExtentSpace = BitConverter.ToUInt32(bytes, Tags[0].Offset + 0x08 + offset);
            SpaceTreePageNumber = BitConverter.ToUInt32(bytes, Tags[0].Offset + 0x0C + offset);
        }

        #endregion Constructors
    }*/
}
