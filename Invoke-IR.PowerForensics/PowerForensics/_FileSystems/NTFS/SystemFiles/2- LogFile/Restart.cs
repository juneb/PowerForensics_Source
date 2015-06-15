using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InvokeIR.PowerForensics.NTFS
{
    public class Restart
    {
        public RestartAreaHeader restartHeader;
        public LogFile opRecord;

        public Restart(byte[] bytes)
        {
            restartHeader = new RestartAreaHeader(bytes.Take(72).ToArray());
            opRecord = new LogFile(bytes.Skip(72).Take(88).ToArray());
        }

        public static Restart[] Get(byte[] bytes)
        {
            Restart[] restartArray = new Restart[2];

            byte[] restart1 = new byte[0x1000];
            Array.Copy(bytes, 0, restart1, 0, restart1.Length);

            byte[] restart2 = new byte[0x1000];
            Array.Copy(bytes, 0x1000, restart2, 0, restart2.Length);

            restartArray[0] = new Restart(restart1);
            restartArray[1] = new Restart(restart2);

            return restartArray;
        }
    }
}
