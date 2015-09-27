using System;
using System.Text;
using System.Net.NetworkInformation;
using InvokeIR.PowerForensics.Registry;

namespace InvokeIR.PowerForensics.Artifacts
{
    public class NetworkList
    {
        #region Properties

        public readonly DateTime WriteTime;
        public readonly string ProfileGuid;
        public readonly string Description;
        public readonly uint Source;
        public readonly string DnsSuffix;
        public readonly string FirstNetwork;
        public readonly PhysicalAddress DefaultGatewayMac;

        #endregion Properties

        #region Constructors

        internal NetworkList(NamedKey nk, byte[] bytes)
        {
            WriteTime = nk.WriteTime;

            foreach (ValueKey vk in nk.GetValues(bytes))
            {
                switch (vk.Name)
                {
                    case "ProfileGuid":
                        ProfileGuid = Encoding.Unicode.GetString(vk.GetData(bytes));
                        break;
                    case "Description":
                        Description = Encoding.Unicode.GetString(vk.GetData(bytes));
                        break;
                    case "Source":
                        Source = BitConverter.ToUInt32(vk.GetData(bytes), 0x00);
                        break;
                    case "DnsSuffix":
                        DnsSuffix = Encoding.Unicode.GetString(vk.GetData(bytes));
                        break;
                    case "FirstNetwork":
                        FirstNetwork = Encoding.Unicode.GetString(vk.GetData(bytes));
                        break;
                    case "DefaultGatewayMac":
                        DefaultGatewayMac = new PhysicalAddress(vk.GetData(bytes));
                        break;
                    default:
                        break;
                }
            }
        }

        #endregion Constructors
    }
}
