using System;
using System.Text;

namespace InvokeIR.PowerForensics.Windows.Registry
{
    public class ValueKey
    {
        #region Enums

        #endregion Enums

        #region Properties

        private readonly ushort ValueNameSize;
        private readonly uint DataSize;
        private readonly uint DataOffset;
        public readonly string DataTypes;
        public readonly string Flags;
        public readonly string ValueName;

        #endregion Properties

        #region Constructors

        #endregion Constructors
    }
}
