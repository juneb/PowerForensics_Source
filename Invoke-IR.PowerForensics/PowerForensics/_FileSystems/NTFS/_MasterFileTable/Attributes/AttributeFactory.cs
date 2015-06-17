using System;
using System.Text;

namespace InvokeIR.PowerForensics.NTFS
{
    class AttributeFactory
    {

        internal static Attr Get(byte[] Bytes, int offset, out int offsetToATTR)
        {
            byte[] commonHeaderBytes = new byte[16];
            Array.Copy(Bytes, offset, commonHeaderBytes, 0, commonHeaderBytes.Length);
            AttrHeader.ATTR_HEADER_COMMON commonAttributeHeader = new AttrHeader.ATTR_HEADER_COMMON(commonHeaderBytes);
            
            // Get byte[] representing the current attribute 
            byte[] AttrBytes = new byte[commonAttributeHeader.TotalSize];
            Array.Copy(Bytes, offset, AttrBytes, 0, AttrBytes.Length);

            // This needs to be looked at...
            if (BitConverter.ToUInt32(AttrBytes, 0) != 0xD0)
            {   
                // Get byte[] representing the Attribute Name
                byte[] NameBytes = new byte[commonAttributeHeader.NameLength * 2];
                Array.Copy(AttrBytes, commonAttributeHeader.NameOffset, NameBytes, 0, NameBytes.Length);
                
                // Decode byte[] into Unicode String
                string AttrName = Encoding.Unicode.GetString(NameBytes);

                // Update offset value
                offset += (int)commonAttributeHeader.TotalSize;
                
                // Set offset return
                offsetToATTR = offset;

                // If attribute is non-resident
                if (commonAttributeHeader.NonResident)
                {
                    return NonResident.Get(AttrBytes, AttrName);
                }
                
                // If attribute is resident
                else
                {
                    #region ATTRSwitch

                    switch (commonAttributeHeader.ATTRType)
                    {
                        case (Int32)Attr.ATTR_TYPE.STANDARD_INFORMATION:
                            return new StandardInformation(AttrBytes, AttrName);

                        case (Int32)Attr.ATTR_TYPE.FILE_NAME:
                            return new FileName(AttrBytes, AttrName);

                        case (Int32)Attr.ATTR_TYPE.OBJECT_ID:
                            return new ObjectId(AttrBytes, AttrName);

                        case (Int32)Attr.ATTR_TYPE.VOLUME_NAME:
                            return new VolumeName(AttrBytes, AttrName);

                        case (Int32)Attr.ATTR_TYPE.VOLUME_INFORMATION:
                            return new VolumeInformation(AttrBytes, AttrName);

                        case (Int32)Attr.ATTR_TYPE.DATA:
                            return new Data(AttrBytes, AttrName);
                            break;

                        case (Int32)Attr.ATTR_TYPE.INDEX_ROOT:
                            return new IndexRoot(AttrBytes, AttrName);

                        default:
                            break;
                    }

                    #endregion ATTRSwitch
                }

                return null;

            }
            else
            {
                offsetToATTR = 1025;
                return null;
            }
        }

    }

}
