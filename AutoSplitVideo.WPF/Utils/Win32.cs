using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace AutoSplitVideo.Utils
{
	public static class Win32
	{
		#region UnMinimize

		public static void UnMinimize(Window window)
		{
			if (PresentationSource.FromVisual(window) is HwndSource hs)
			{
				var handle = hs.Handle;
				ShowWindow(handle, ShowWindowCommands.Restore);
			}
		}

		[DllImport(@"user32.dll")]
		private static extern bool ShowWindow(IntPtr hWnd, ShowWindowCommands nCmdShow);

		//http://pinvoke.net/default.aspx/Enums/ShowWindowCommand.html
		private enum ShowWindowCommands
		{
			/// <summary>
			/// Activates and displays the window. If the window is minimized or 
			/// maximized, the system restores it to its original size and position. 
			/// An application should specify this flag when restoring a minimized window.
			/// </summary>
			Restore = 9,
		}

		#endregion

		#region DeleteToRecycleBin

		/// <summary>
		/// Possible flags for the SHFileOperation method.
		/// </summary>
		[Flags]
		private enum FileOperationFlags : ushort
		{
			/// <summary>
			/// Do not show a dialog during the process
			/// </summary>
			Silent = 0x0004,
			/// <summary>
			/// Do not ask the user to confirm selection
			/// </summary>
			NoConfirmation = 0x0010,
			/// <summary>
			/// Delete the file to the recycle bin. (Required flag to send a file to the bin)
			/// </summary>
			AllowUndo = 0x0040,
			/// <summary>
			/// Suppress errors, if any occur during the process.
			/// </summary>
			NoErrorUi = 0x0400
		}

		/// <summary>
		/// File Operation Function Type for SHFileOperation
		/// </summary>
		private enum FileOperationType : uint
		{
			/// <summary>
			/// Delete (or recycle) the objects
			/// </summary>
			Delete = 0x0003
		}

		/// <summary>
		/// SHFILEOPSTRUCT for SHFileOperation from COM
		/// </summary>
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		private struct SHFILEOPSTRUCT
		{
			public IntPtr hwnd;
			[MarshalAs(UnmanagedType.U4)]
			public FileOperationType wFunc;
			public string pFrom;
			public string pTo;
			public FileOperationFlags fFlags;
			[MarshalAs(UnmanagedType.Bool)]
			public bool fAnyOperationsAborted;
			public IntPtr hNameMappings;
			public string lpszProgressTitle;
		}

		[DllImport(@"shell32.dll", CharSet = CharSet.Auto)]
		private static extern int SHFileOperation(ref SHFILEOPSTRUCT fileOp);

		/// <summary>
		/// Send file to recycle bin
		/// </summary>
		private static bool Send(string path, FileOperationFlags flags)
		{
			try
			{
				var fs = new SHFILEOPSTRUCT
				{
					wFunc = FileOperationType.Delete,
					pFrom = path + '\0' + '\0',
					fFlags = FileOperationFlags.AllowUndo | flags
				};
				SHFileOperation(ref fs);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// Send file silently to recycle bin. Suppress dialog, suppress errors, delete if too large.
		/// </summary>
		public static bool MoveToRecycleBin(string path)
		{
			return Send(path, FileOperationFlags.NoConfirmation | FileOperationFlags.NoErrorUi | FileOperationFlags.Silent);
		}

		#endregion
	}
}
