﻿using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using InvokeIR.Win32;

namespace InvokeIR.PowerForensics.NTFS
{
    #region VolumeBootRecordClass

    public class VolumeBootRecord
    {

        #region Structs

        struct NTFS_VOLUME_BOOT_RECORD
        {
            // jump instruction
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 3)]
            internal byte[] Jmp;

            // signature
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 8)]
            internal string Signature;

            // BPB and extended BPB
            internal ushort BytesPerSector;
            internal byte SectorsPerCluster;
            internal ushort ReservedSectors;
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 3)]
            internal ushort NotUsed1;
            internal byte MediaDescriptor;
            internal ushort SectorsPerTrack;
            internal ushort NumberOfHeads;
            internal uint HiddenSectors;
            internal uint NotUsed2;
            internal uint NotUsed3;
            internal ulong TotalSectors;
            internal ulong LCN_MFT;
            internal ulong LCN_MFTMirr;
            internal sbyte ClustersPerFileRecord;
            internal sbyte ClustersPerIndexBlock;
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 8)]
            internal string VolumeSN;

            // boot code
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 430)]
            internal byte[] Code;

            //0xAA55
            internal byte _AA;
            internal byte _55;

            internal NTFS_VOLUME_BOOT_RECORD(byte[] bytes)
            {
                byte[] jmpBytes = new byte[3];
                Array.Copy(bytes, 0, jmpBytes, 0, jmpBytes.Length);
                Jmp = jmpBytes;
                byte[] sigBytes = new byte[8];
                Array.Copy(bytes, 3, sigBytes, 0, sigBytes.Length);
                Signature = Encoding.ASCII.GetString(sigBytes);
                BytesPerSector = BitConverter.ToUInt16(bytes, 11);
                SectorsPerCluster = bytes[13];
                ReservedSectors = BitConverter.ToUInt16(bytes, 14);
                NotUsed1 = BitConverter.ToUInt16(bytes, 19);
                MediaDescriptor = bytes[21];
                SectorsPerTrack = BitConverter.ToUInt16(bytes, 24);
                NumberOfHeads = BitConverter.ToUInt16(bytes, 26);
                HiddenSectors = BitConverter.ToUInt32(bytes, 28);
                NotUsed2 = BitConverter.ToUInt32(bytes, 32);
                NotUsed3 = BitConverter.ToUInt32(bytes, 36);
                TotalSectors = BitConverter.ToUInt64(bytes, 40);
                LCN_MFT = BitConverter.ToUInt64(bytes, 48);
                LCN_MFTMirr = BitConverter.ToUInt64(bytes, 56);
                ClustersPerFileRecord = (sbyte)bytes[64];
                ClustersPerIndexBlock = (sbyte)bytes[68];
                byte[] snBytes = new byte[8];
                Array.Copy(bytes, 72, snBytes, 0, snBytes.Length);
                VolumeSN = BitConverter.ToString(snBytes).Replace("-", "");
                byte[] codeBytes = new byte[430];
                Array.Copy(bytes, 80, codeBytes, 0, codeBytes.Length);
                Code = codeBytes;
                _AA = bytes[510];
                _55 = bytes[511];

            }

        }

        #endregion Structs

        #region Properties

        public readonly ushort BytesPerSector;
        public readonly uint BytesPerCluster;
        public readonly double BytesPerFileRecord;
        public readonly double BytesPerIndexBlock;
        public readonly ushort ReservedSectors;
        public readonly ushort SectorsPerTrack;
        public readonly ushort NumberOfHeads;
        public readonly uint HiddenSectors;
        public readonly ulong TotalSectors;
        public readonly ulong MFTStartIndex;
        public readonly ulong MFTMirrStartIndex;
        public readonly string VolumeSN;
        public readonly byte[] CodeSection;

        #endregion Properties

        #region Constructors

        internal VolumeBootRecord(string volumeName)
        {
            // Instantiate a NTFS_VOLUME_BOOT_RECORD struct
            NTFS_VOLUME_BOOT_RECORD structVBR = new NTFS_VOLUME_BOOT_RECORD(getBytes(volumeName));

            // Assign object properties
            BytesPerSector = structVBR.BytesPerSector;
            BytesPerCluster = (uint)(structVBR.SectorsPerCluster * BytesPerSector);
            if (structVBR.ClustersPerFileRecord < 0)
            {
                BytesPerFileRecord = Math.Pow(2, Math.Abs(structVBR.ClustersPerFileRecord));
            }
            else
            {
                BytesPerFileRecord = structVBR.ClustersPerFileRecord * BytesPerCluster;
            }

            if (structVBR.ClustersPerIndexBlock < 0)
            {
                BytesPerIndexBlock = Math.Pow(2, Math.Abs(structVBR.ClustersPerIndexBlock));
            }
            else
            {
                BytesPerIndexBlock = structVBR.ClustersPerIndexBlock * BytesPerCluster;
            }
            ReservedSectors = structVBR.ReservedSectors;
            NumberOfHeads = structVBR.NumberOfHeads;
            HiddenSectors = structVBR.HiddenSectors;
            TotalSectors = structVBR.TotalSectors;
            MFTStartIndex = structVBR.LCN_MFT;
            MFTMirrStartIndex = structVBR.LCN_MFTMirr;
            VolumeSN = structVBR.VolumeSN;
            CodeSection = structVBR.Code;
        }

        internal VolumeBootRecord(FileStream streamToRead)
        {
            // Instantiate a NTFS_VOLUME_BOOT_RECORD struct
            NTFS_VOLUME_BOOT_RECORD structVBR = new NTFS_VOLUME_BOOT_RECORD(getBytes(streamToRead));

            // Assign object properties
            BytesPerSector = structVBR.BytesPerSector;
            BytesPerCluster = (uint)(structVBR.SectorsPerCluster * BytesPerSector);
            if (structVBR.ClustersPerFileRecord < 0)
            {
                BytesPerFileRecord = Math.Pow(2, Math.Abs(structVBR.ClustersPerFileRecord));
            }
            else
            {
                BytesPerFileRecord = structVBR.ClustersPerFileRecord * BytesPerCluster;
            }

            if (structVBR.ClustersPerIndexBlock < 0)
            {
                BytesPerIndexBlock = Math.Pow(2, Math.Abs(structVBR.ClustersPerIndexBlock));
            }
            else
            {
                BytesPerIndexBlock = structVBR.ClustersPerIndexBlock * BytesPerCluster;
            }
            ReservedSectors = structVBR.ReservedSectors;
            NumberOfHeads = structVBR.NumberOfHeads;
            HiddenSectors = structVBR.HiddenSectors;
            TotalSectors = structVBR.TotalSectors;
            MFTStartIndex = structVBR.LCN_MFT;
            MFTMirrStartIndex = structVBR.LCN_MFTMirr;
            VolumeSN = structVBR.VolumeSN;
            CodeSection = structVBR.Code;
        }

        #endregion Constructors

        #region InternalMethods

        internal static byte[] getBytes(string volumeName)
        {
            // Get handle to Logical Volume
            IntPtr hVolume = NativeMethods.getHandle(volumeName);

            // Create a FileStream object for the Volume
            using (FileStream streamToRead = NativeMethods.getFileStream(hVolume))
            {
                return NativeMethods.readDrive(streamToRead, 0, 512);
            }
        }

        internal static byte[] getBytes(FileStream streamToRead)
        {
            return NativeMethods.readDrive(streamToRead, 0, 512);
        }

        #endregion InternalMethods

    }

    #endregion VolumeBootRecordClass
}
