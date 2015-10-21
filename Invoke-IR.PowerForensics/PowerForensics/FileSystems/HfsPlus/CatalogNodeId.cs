using System;

namespace PowerForensics.HfsPlus
{
    public class CatalogNodeId
    {
        #region Enums

        public enum CATALOG_ID{
            kHFSRootParentId = 0x01,
            kHFSRootFolderID = 0x02,
            kHFSExtentsFileID = 0x03,
            kHFSCatalogFileID = 0x04,
            kHFSBadBlockFileID = 0x05,
            kHFSAllocationFileID = 0x06,
            kHFSStartupFileID = 0x07,
            kHFSAttributesFileID = 0x08,
            kHFSRepairCatalogFileID = 0x0E,
            kHFSBogusExtentFileID = 0x0F,
            kHFSFirstUserCatalogNodeID = 0x10
        }

        #endregion Enums
    }
}
