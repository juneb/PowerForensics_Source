using System;

namespace PowerForensics.Formats
{
    #region HexDumpClass
    
    public class HexDump
    {       
        #region Properties
        
        public readonly string Offset;
        public readonly string _00_01_02_03_04_05_06_07_08_09_0A_0B_0C_0D_0E_0F;
        public readonly string Ascii;
    
        #endregion Properties

        #region Constructors

        internal HexDump(string offset, string hex, string ascii)
        {
            Offset = offset;
            _00_01_02_03_04_05_06_07_08_09_0A_0B_0C_0D_0E_0F = hex;
            Ascii = ascii;
        }

        #endregion Constructors

        #region StaticMethods

        public static HexDump[] Get(byte[] bytes)
        {
            int i = 0;
            HexDump[] dump = new HexDump[(bytes.Length / 16) + 1];
            while (i < bytes.Length)
            {
                string hex = null;
                string ascii = null;
                string offset = String.Format("0x{0:X8}", i);

                for (int j = 0; j < 16; j++)
                {
                    if ((i + j) >= bytes.Length)
                    {
                        break;
                    }
                    hex += ' ';
                    hex += String.Format("{0:X2}", bytes[i+j]);

                    if (bytes[i + j] >= 0x20 && bytes[i + j] < 0x7F)
                    {
                        ascii += Convert.ToChar(bytes[i + j]);
                    }
                    else
                    {
                        ascii += '.';
                    }
                }
                dump[i / 16] = new HexDump(offset, hex, ascii);
                i += 16;
            }
            return dump;
        }

        #endregion StaticMethods
    }

    #endregion HexDumpClass
}
