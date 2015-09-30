using System;

namespace PowerForensics.EseDb
{
    internal class RootPage
    {
        #region Properties

        internal readonly uint InitialNumberOfPages;
        internal readonly uint FatherDataPage;
        internal readonly uint ExtentSpace;
        internal readonly uint SpaceTreePageNumber;

        #endregion Properties

        #region Constructors

        internal RootPage(byte[] bytes)
        {
            InitialNumberOfPages = BitConverter.ToUInt32(bytes, 0x00);
            FatherDataPage = BitConverter.ToUInt32(bytes, 0x04);
            ExtentSpace = BitConverter.ToUInt32(bytes, 0x08);
            SpaceTreePageNumber = BitConverter.ToUInt32(bytes, 0x0C);
        }

        #endregion Constructors
    }
}
