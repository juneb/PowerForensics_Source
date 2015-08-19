using System;
using System.Text;

namespace InvokeIR.PowerForensics.Windows.Registry
{
    class SecurityKey
    {
        #region Properties

        private readonly uint CellSize;
        private readonly string Signature;
        private readonly uint PreviousSecurityKeyOffset;
        private readonly uint NextSecurityKeyOffset;

        #endregion Properties

        #region Constructors

        internal SecurityKey(byte[] bytes)
        {

        }

        #endregion Constructors
    }
}
