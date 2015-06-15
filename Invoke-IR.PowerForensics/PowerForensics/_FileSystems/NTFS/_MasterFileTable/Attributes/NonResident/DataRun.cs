using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InvokeIR.PowerForensics.NTFS
{
    public class DataRun
    {
        #region Properties
        
        public readonly long StartCluster;
        public readonly long ClusterLength;
        public readonly bool Sparse;
        public readonly bool Compressed;

        #endregion Properties

        #region Constructors
        
        public DataRun(byte[] bytes, int lengthByteCount, int offsetByteCount, ref long previousStartCluster)
        {
            if (offsetByteCount == 0)
            {
                Sparse = true;
            }

            byte[] DataRunLengthBytes = new byte[8];
            Array.Copy(bytes, 0, DataRunLengthBytes, 0, lengthByteCount);
            long DataRunLength = BitConverter.ToInt64(DataRunLengthBytes, 0);

            byte[] DataRunOffsetBytes = new byte[8];
            Array.Copy(bytes, lengthByteCount, DataRunOffsetBytes, 0, offsetByteCount);
            long DataRunOffset = BitConverter.ToInt64(DataRunOffsetBytes, 0);

            if ((DataRunOffset & (1 << ((offsetByteCount * 8) - 1))) != 0)
            {
                DataRunOffset = (DataRunOffset | (~0) << (offsetByteCount * 8));
            }

            previousStartCluster += DataRunOffset;
            StartCluster = previousStartCluster;
            ClusterLength = DataRunLength;
        }

        #endregion Constructors
    }
}
