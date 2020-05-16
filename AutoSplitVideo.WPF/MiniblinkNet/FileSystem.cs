using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Kyozy.MiniblinkNet
{
    public class FileSystem
    {
        #region API声明
        [DllImport("shlwapi.dll", EntryPoint = "PathIsRelativeW", CharSet = CharSet.Unicode)]
        private static extern int PathIsRelative(string pszPath);
        [DllImport("shlwapi.dll", EntryPoint = "PathRemoveFileSpecW", CharSet = CharSet.Unicode)]
        private static extern int PathRemoveFileSpec(StringBuilder pszPath);
        [DllImport("shlwapi.dll", EntryPoint = "PathCombineW", CharSet = CharSet.Unicode)]
        private static extern int PathCombine(StringBuilder szDest, string lpszDir, string lpszFile);
        [DllImport("kernel32.dll", EntryPoint = "CreateFileW", CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateFile(string lpFileName, int dwDesiredAccess, int dwShareMode, int lpSecurityAttributes, int dwCreationDisposition, int dwFlagsAndAttributes, int hTemplateFile);
        [DllImport("kernel32.dll", EntryPoint = "CloseHandle")]
        private static extern int CloseHandle(IntPtr hObject);
        [DllImport("kernel32.dll", EntryPoint = "GetFileSize")]
        private static extern int GetFileSize(IntPtr hFile, int lpFileSizeHigh);
        [DllImport("kernel32.dll", EntryPoint = "ReadFile")]
        private static extern int ReadFile(IntPtr hFile, IntPtr lpBuffer, int nNumberOfBytesToRead, ref int lpNumberOfBytesRead, int lpOverlapped);
        [DllImport("kernel32.dll", EntryPoint = "SetFilePointer")]
        private static extern int SetFilePointer(IntPtr hFile, int lDistanceToMove, int lpDistanceToMoveHigh, int dwMoveMethod);

        #endregion

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr FILE_OPEN(IntPtr path);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void FILE_CLOSE(IntPtr handle);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int FILE_SIZE(IntPtr handle);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int FILE_READ(IntPtr handle, IntPtr buffer, int size);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int FILE_SEEK(IntPtr handle, int offset, int origin);

        private const int GENERIC_READ = -2147483648;
        private const int OPEN_EXISTING = 3;
        private const int FILE_SHARE_READ = 1;
        private const int FILE_SHARE_WRITE = 2;
        private const int FILE_ATTRIBUTE_NORMAL = 128;

        internal FILE_OPEN m_fileOpen;
        internal FILE_CLOSE m_fileClose;
        internal FILE_SIZE m_fileSize;
        internal FILE_READ m_fileRead;
        internal FILE_SEEK m_fileSeek;

        public FileSystem()
        {
            m_fileOpen = new FILE_OPEN(FileOpen);
            m_fileClose = new FILE_CLOSE(FileClose);
            m_fileSize = new FILE_SIZE(FileSize);
            m_fileRead = new FILE_READ(FileRead);
            m_fileSeek = new FILE_SEEK(FileSeek);
        }

        public virtual IntPtr FileOpen(IntPtr Path)
        {
            string fileName = Help.PtrToStringUTF8(Path);
            IntPtr hFile = CreateFile(fileName, GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE, 0, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, 0);
            return hFile;
        }

        public virtual void FileClose(IntPtr handle)
        {
            CloseHandle(handle);
        }

        public virtual int FileSize(IntPtr handle)
        {
            return GetFileSize(handle, 0);
        }

        public virtual int FileRead(IntPtr handle, IntPtr buffer, int size)
        {
            if (handle != IntPtr.Zero)
            {
                int len = 0;
                ReadFile(handle, buffer, size, ref len, 0);
                return len;
            }
            return 0;
        }

        public virtual int FileSeek(IntPtr handle, int offset, int origin)
        {
            return SetFilePointer(handle, offset, 0, origin);
        }
    }
}
