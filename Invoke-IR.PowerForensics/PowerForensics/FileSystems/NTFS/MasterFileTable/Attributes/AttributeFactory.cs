using System;
using System.Text;

namespace InvokeIR.PowerForensics.NTFS
{
    class CommonHeader
    {
        #region Constants

        internal const byte RESIDENT = 0x00;
        internal const byte NONRESIDENT = 0x01;

        #endregion Constants

        #region Properties

        internal uint ATTRType;			// Attribute Type
        internal uint TotalSize;		// Length (including this header)
        internal bool NonResident;	    // 0 - resident, 1 - non resident
        internal byte NameLength;		// name length in words
        internal ushort NameOffset;		// offset to the name
        internal ushort Flags;			// Flags
        internal ushort Id;				// Attribute Id

        #endregion Properties

        #region Constructors

        internal CommonHeader(byte[] bytes)
        {
            ATTRType = BitConverter.ToUInt32(bytes, 0x00);
            TotalSize = BitConverter.ToUInt32(bytes, 0x04);
            NonResident = (bytes[0x08] == NONRESIDENT);
            NameLength = bytes[0x09];
            NameOffset = BitConverter.ToUInt16(bytes, 0x0A);
            Flags = BitConverter.ToUInt16(bytes, 0x0C);
            Id = BitConverter.ToUInt16(bytes, 0x0E);
        }

        #endregion Constructors
    }

    class ResidentHeader
    {
        #region Properties

        internal CommonHeader commonHeader;
        internal uint AttrSize;
        internal ushort AttrOffset;
        internal byte IndexedFlag;

        #endregion Properties

        #region Constructors

        internal ResidentHeader(byte[] bytes, CommonHeader common)
        {
            commonHeader = common;
            AttrSize = BitConverter.ToUInt32(bytes, 0);
            AttrOffset = BitConverter.ToUInt16(bytes, 4);
            IndexedFlag = bytes[6];
        }

        #endregion Constructors
    }

    class NonResidentHeader
    {
        #region Properties

        internal CommonHeader commonHeader;
        internal ulong StartVCN;		        // Starting VCN
        internal ulong LastVCN;		            // Last VCN
        internal ushort DataRunOffset;	        // Offset to the Data Runs
        internal ushort CompUnitSize;	        // Compression unit size
        internal readonly ulong AllocatedSize;    // Allocated size of the attribute
        internal readonly ulong RealSize;         // Real size of the attribute
        internal readonly ulong InitializedSize;  // Initialized data size of the stream 

        #endregion Properties

        #region Constructors

        internal NonResidentHeader(byte[] bytes, CommonHeader common)
        {
            commonHeader = common;
            StartVCN = BitConverter.ToUInt64(bytes, 0x00);
            LastVCN = BitConverter.ToUInt64(bytes, 0x08);
            DataRunOffset = BitConverter.ToUInt16(bytes, 0x10);
            CompUnitSize = BitConverter.ToUInt16(bytes, 0x12);
            AllocatedSize = BitConverter.ToUInt64(bytes, 0x18);
            RealSize = BitConverter.ToUInt64(bytes, 0x20);
            InitializedSize = BitConverter.ToUInt64(bytes, 0x28);
        }

        #endregion Constructors
    }

    class AttributeFactory
    {
        #region Constants

        internal const byte RESIDENT = 0x00;
        private const int COMMONHEADERSIZE = 0x10;
        private const int RESIDENTHEADERSIZE = 0x08;
        private const int NONRESIDENTHEADERSIZE = 0x30;

        #endregion Constants

        internal static Attr Get(byte[] bytes, string volume)
        {
            #region CommonHeader

            if (bytes.Length == 0)
            {
                return null;
            }

            // Instantiate a Common Header Object
            byte[] commonHeaderBytes = new byte[COMMONHEADERSIZE];
            Array.Copy(bytes, 0, commonHeaderBytes, 0, commonHeaderBytes.Length);
            CommonHeader commonHeader = new CommonHeader(commonHeaderBytes);

            #endregion CommonHeader

            #region AttributeName
            
            // Get byte[] representing the Attribute Name
            int nameLength = commonHeader.NameLength * 2;
            byte[] attributeNameBytes = new byte[nameLength];
            try
            {
                Array.Copy(bytes, commonHeader.NameOffset, attributeNameBytes, 0, attributeNameBytes.Length);
            }
            catch
            {
                throw new Exception("Error creating attributeNameBytes array");
            }

            // Decode Name byte[] into Unicode String
            string attributeName = Encoding.Unicode.GetString(attributeNameBytes);

            #endregion AttributeName

            bool resident = (bytes[8] == 0x00);

            #region ResidentAttribute

            // If Attribute is Resident
            if (resident)
            {
                #region ResidentHeader
                
                // Instantiate a Resident Header Object
                byte[] residentHeaderBytes = new byte[RESIDENTHEADERSIZE];
                try 
                {
                    Array.Copy(bytes, COMMONHEADERSIZE, residentHeaderBytes, 0, residentHeaderBytes.Length);
                }
                catch
                {
                    throw new Exception("Error creating residentHeaderBytes array");
                }
                ResidentHeader residentHeader = new ResidentHeader(residentHeaderBytes, commonHeader);

                #endregion ResidentHeader

                #region AttributeBytes

                // Create a byte[] representing the attribute itself
                int headerSize = COMMONHEADERSIZE + RESIDENTHEADERSIZE + nameLength;

                byte[] attributeBytes = new byte[commonHeader.TotalSize - headerSize];
                try 
                {
                    Array.Copy(bytes, headerSize, attributeBytes, 0, attributeBytes.Length);
                }
                catch
                {
                    throw new Exception("Problem creating attributeBytes array");
                }

                #endregion AttributeBytes
                
                #region ATTRSwitch

                switch (residentHeader.commonHeader.ATTRType)
                {
                    case (Int32)Attr.ATTR_TYPE.STANDARD_INFORMATION:
                        return new StandardInformation(residentHeader, attributeBytes, attributeName);

                    case (Int32)Attr.ATTR_TYPE.FILE_NAME:
                        return new FileName(residentHeader, attributeBytes, attributeName);

                    case (Int32)Attr.ATTR_TYPE.OBJECT_ID:
                        return new ObjectId(residentHeader, attributeBytes, attributeName);

                    case (Int32)Attr.ATTR_TYPE.VOLUME_NAME:
                        return new VolumeName(residentHeader, attributeBytes, attributeName);

                    case (Int32)Attr.ATTR_TYPE.VOLUME_INFORMATION:
                        return new VolumeInformation(residentHeader, attributeBytes, attributeName);

                    case (Int32)Attr.ATTR_TYPE.DATA:
                        return new Data(residentHeader, attributeBytes, attributeName);

                    case (Int32)Attr.ATTR_TYPE.INDEX_ROOT:
                        return new IndexRoot(residentHeader, attributeBytes, attributeName);

                    default:
                        return null;
                }

                #endregion ATTRSwitch
            }

            #endregion ResidentAttribute

            #region NonResidentAttribute
            // Else Attribute is Non-Resident
            else
            {
                #region NonResidentHeader

                // Instantiate a Resident Header Object
                byte[] nonresidentHeaderBytes = new byte[NONRESIDENTHEADERSIZE];
                try 
                {
                    Array.Copy(bytes, COMMONHEADERSIZE, nonresidentHeaderBytes, 0, nonresidentHeaderBytes.Length);
                }
                catch
                {
                    throw new Exception("Error creating nonresidentHeaderBytes array");
                }
                NonResidentHeader nonresidentHeader = new NonResidentHeader(nonresidentHeaderBytes, commonHeader);

                #endregion NonResidentHeader

                #region DataRunBytes

                int headerSize = 0;

                if (commonHeader.NameOffset != 0) 
                {
                    headerSize = commonHeader.NameOffset + nameLength + (nameLength % 8);
                }
                else
                {
                    headerSize = COMMONHEADERSIZE + NONRESIDENTHEADERSIZE;
                }

                byte[] dataRunBytes = new byte[commonHeader.TotalSize - headerSize];
                
                try
                {
                    Array.Copy(bytes, headerSize, dataRunBytes, 0, dataRunBytes.Length);
                }
                catch
                {
                    throw new Exception("Error creating dataRunBytes array");
                }

                #endregion DataRunBytes

                return new NonResident(nonresidentHeader, dataRunBytes, attributeName);
            }
            #endregion NonResidentAttribute
        }
    }
}
