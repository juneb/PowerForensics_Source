using System;
using System.Text;

namespace InvokeIR.PowerForensics.OS.Windows.Registry
{
    public class HiveBinHeader
    {
        #region Properties

        public readonly string Signature;
        public readonly uint hBinOffset;
        public readonly uint hBinSize;

        #endregion Properties

        #region Constructors

        internal HiveBinHeader(byte[] bytes)
        {
            #region Signature

            byte[] sigBytes = new byte[4];
            Array.Copy(bytes, 0, sigBytes, 0, sigBytes.Length);
            Signature = Encoding.ASCII.GetString(sigBytes);

            if (Signature != "hbin")
            {
                throw new Exception("Invalid HiveBinHeader found.");
            }

            #endregion Signature

            hBinOffset = BitConverter.ToUInt32(bytes, 0x04);
            hBinSize = BitConverter.ToUInt32(bytes, 0x08);
        }

        #endregion Constructors
    }

    class CellFactory
    {
        internal static Cell Get(byte[] bytes)
        {
            #region Signature

            byte[] sigBytes = new byte[2];
            Array.Copy(bytes, 0x00, sigBytes, 0, sigBytes.Length);
            string signature = Encoding.ASCII.GetString(sigBytes);

            #endregion Signature

            #region CellSwitch

            switch (signature)
            {
                case "lf":
                    return null;

                case "lh":
                    return null;

                case "li":
                    return null;

                case "ri":
                    return null;

                case "nk":
                    return new NamedKey(bytes, signature);

                case "sk":
                    return null;

                case "vk":
                    return null;

                case "db":
                    return null;

                default:
                    return null;
            }

            #endregion CellSwitch
        }
    }
}
