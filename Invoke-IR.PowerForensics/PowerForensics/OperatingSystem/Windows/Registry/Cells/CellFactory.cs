using System;
using System.Text;

namespace InvokeIR.PowerForensics.OS.Windows.Registry
{
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
