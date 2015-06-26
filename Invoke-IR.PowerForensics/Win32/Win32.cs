using System;
using System.IO;
using System.Security.Principal;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace InvokeIR.Win32
{

    public static class NativeMethods
    {
        #region Constants

        internal const Int32 FILE_ATTRIBUTE_ARCHIVE = 0x00000020;
        internal const Int32 FILE_ATTRIBUTE_ENCRYPTED = 0x00004000;
        internal const Int32 FILE_ATTRIBUTE_HIDDEN = 0x00000002;
        internal const Int32 FILE_ATTRIBUTE_NORMAL = 0x00000080;
        internal const Int32 FILE_ATTRIBUTE_OFFLINE = 0x00001000;
        internal const Int32 FILE_ATTRIBUTE_READONLY = 0x00000001;
        internal const Int32 FILE_ATTRIBUTE_SYSTEM = 0x00000004;
        internal const Int32 FILE_ATTRIBUTE_TEMPORARY = 0x00000100;

        internal const Int32 FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;
        internal const Int32 FILE_FLAG_DELETE_ON_CLOSE = 0x04000000;
        internal const Int32 FILE_FLAG_NO_BUFFERING = 0x20000000;
        internal const Int32 FILE_FLAG_OPEN_NO_RECALL = 0x00100000;
        internal const Int32 FILE_FLAG_OPEN_REPARSE_POINT = 0x00200000;
        internal const Int32 FILE_FLAG_OVERLAPPED = 0x40000000;
        internal const Int32 FILE_FLAG_POSIX_SEMANTICS = 0x00100000;
        internal const Int32 FILE_FLAG_RANDOM_ACCESS = 0x10000000;
        internal const Int32 FILE_FLAG_SESSION_AWARE = 0x00800000;

        internal const Int32 INVALID_HANDLE_VALUE = -1;

        internal const Int32 BYTES_PER_SECTOR = 0x200;

        #endregion Constants

        #region structs

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct BY_HANDLE_FILE_INFORMATION
        {
            internal uint dwFileAttributes;
            internal DateTime ftCreationTime;
            internal DateTime ftLastAccessTime;
            internal DateTime ftLastWriteTime;
            internal uint dwVolumeSerialNumber;
            internal uint nFileSizeHigh;
            internal uint nFileSizeLow;
            internal uint nNumberOfLinks;
            internal uint nFileIndexHigh;
            internal uint nFileIndexLow;
        }

        #endregion structs

        #region PInvoke

        [DllImport(ExternDll.Kernel32, CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr CreateFile
            (
                string fileName,
                [MarshalAs(UnmanagedType.U4)] FileAccess fileAccess,
                [MarshalAs(UnmanagedType.U4)] FileShare fileShare,
                IntPtr securityAttributes,
                [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
                int flags,
                IntPtr template
            );

        [DllImport(ExternDll.Kernel32, SetLastError = true)]
        internal static extern bool CloseHandle
            (
                IntPtr hObject
            );

        #endregion PInvoke

        #region Helper Functions

        internal static void checkAdmin()
        {
            bool admin = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
             
            if(!(admin))
            {
                throw new Exception("PowerForensics must be executed from an Administrator PowerShell");
            }
        }

        internal static string getDriveName(string drive)
        {
            Regex lettersOnly = new Regex(@"\\\\\.\\PHYSICALDRIVE\d");

            if (!(lettersOnly.IsMatch(drive.ToUpper())))
            {
                throw new Exception("Provided Drive Name is not acceptable.");
            }

            return drive;
        }
        
        internal static string getVolumeName(ref string volume)
        {
            if (volume == null)
            {
                volume = System.IO.Directory.GetCurrentDirectory().Split(':')[0];
            }

            Regex lettersOnly = new Regex("^[a-zA-Z]{1}$");
            Regex volLetter = new Regex(@"[a-zA-Z]:\\");
            Regex uncPath = new Regex(@"\\\\\.\\[a-zA-Z]:");
            Regex vsc = new Regex(@"\\\\\?\\GLOBALROOT\\DEVICE\\HARDDISKVOLUMESHADOWCOPY\d+$");
            Regex physicalDrive = new Regex(@"\\\\\.\\PHYSICALDRIVE\d");

            if (lettersOnly.IsMatch(volume))
            {
                volume = @"\\.\" + volume + ":";
            }
            else if(volLetter.IsMatch(volume))
            {
                volume = @"\\.\" + volume.TrimEnd('\\');
            }
            else if(uncPath.IsMatch(volume))
            {

            }
            else if(vsc.IsMatch(volume.ToUpper()))
            {

            }
            else if(physicalDrive.IsMatch(volume.ToUpper()))
            {

            }
            else
            {
                throw new Exception("Provided Volume Name is not acceptable.");
            }

            return volume;

        }

        internal static IntPtr getHandle(string FileName)
        {

            // Get Handle to specified Volume/File/Directory
            IntPtr hDrive = CreateFile(
                fileName: FileName,
                fileAccess: FileAccess.Read,
                fileShare: FileShare.Write | FileShare.Read | FileShare.Delete,
                securityAttributes: IntPtr.Zero,
                creationDisposition: FileMode.Open,
                flags: FILE_FLAG_BACKUP_SEMANTICS,
                template: IntPtr.Zero);

            // Check if handle is valid
            if (hDrive.ToInt32() == INVALID_HANDLE_VALUE)
            {
                // If handle is not valid throw an error
                throw new Exception("Invalid handle to Volume/Drive returned");
            }

            // Return handle
            return hDrive;

        }

        internal static FileStream getFileStream(IntPtr hVolume)
        {
            // Return a FileStream to read from the specified handle
            return new FileStream(hVolume, FileAccess.Read);
        }

        internal static byte[] readDrive(FileStream streamToRead, ulong offset, ulong sizeToRead)
        {

            // Bytes must be read by sector
            if ((sizeToRead < 1)) throw new System.ArgumentException("Size parameter cannot be null or 0 or less than 0!");
            if (((sizeToRead % 512) != 0)) throw new System.ArgumentException("Size parameter must be divisible by 512");
            if (((offset % 512) != 0)) throw new System.ArgumentException("Offset parameter must be divisible by 512");

            // Set offset to begin reading from the drive
            streamToRead.Position = (long)offset;

            // Create a byte array to read into
            byte[] buf = new byte[sizeToRead];

            // Read buf.Length bytes (sizeToRead) from offset 
            try
            {
                Int32 bytesRead = streamToRead.Read(buf, 0, buf.Length);

                if (bytesRead != buf.Length)
                {
                    if (bytesRead > buf.Length)
                    {
                        throw new Exception("The readDrive method read more bytes from disk than expected.");
                    }
                    /*else if (bytesRead < buf.Length)
                    {
                        throw new Exception("The readDrive method read less bytes from disk than expected.");
                    }*/
                }

            }
            catch (ArgumentNullException)
            {
                throw new ArgumentNullException("The readDrive method experienced an ArgumentNullException.");
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new ArgumentOutOfRangeException("The readDrive method experienced an ArgumentOutOfRangeException.");
            }
            catch (EndOfStreamException)
            {
                throw new EndOfStreamException("The readDrive method experienced an EndOfStreamException.");
            }
            catch (IOException)
            {
                throw new IOException("The readDrive method experienced an IOException.");
            }
            catch (ArgumentException)
            {
                throw new ArgumentException("The readDrive method experienced an ArgumentException");
            }
            catch (ObjectDisposedException)
            {
                throw new ObjectDisposedException("The readDrive method experienced an ObjectDisposedException");
            }

            return buf;

        }

        internal static DateTime FromUnixTime(uint unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unixTime);
        }

        #endregion Helper Functions
    }
}

