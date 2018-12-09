using System;
using System.Runtime.InteropServices;

namespace AutoSplitVideo.MediaInfo
{
	public partial class MediaInfo : IDisposable
	{
		private readonly IntPtr _handle;
		private readonly bool _mustUseAnsi;

		public MediaInfo()
		{
			try
			{
				_handle = MediaInfo_New();
			}
			catch (BadImageFormatException)
			{
				throw new BadImageFormatException(@"目标平台错误");
			}

			_mustUseAnsi = !Environment.OSVersion.ToString().Contains(@"Windows");
		}

		public int Open(string fileName)
		{
			if (_mustUseAnsi)
			{
				var fileNamePtr = Marshal.StringToHGlobalAnsi(fileName);
				var toReturn = (int)MediaInfoA_Open(_handle, fileNamePtr);
				Marshal.FreeHGlobal(fileNamePtr);
				return toReturn;
			}

			return (int)MediaInfo_Open(_handle, fileName);
		}

		public int Open_Buffer_Init(long fileSize, long fileOffset)
		{
			return (int)MediaInfo_Open_Buffer_Init(_handle, fileSize, fileOffset);
		}

		public int Open_Buffer_Continue(IntPtr buffer, IntPtr bufferSize)
		{
			return (int)MediaInfo_Open_Buffer_Continue(_handle, buffer, bufferSize);
		}

		public long Open_Buffer_Continue_GoTo_Get()
		{
			return MediaInfo_Open_Buffer_Continue_GoTo_Get(_handle);
		}

		public int Open_Buffer_Finalize()
		{
			return (int)MediaInfo_Open_Buffer_Finalize(_handle);
		}

		public string Inform()
		{
			if (_mustUseAnsi)
			{
				return Marshal.PtrToStringAnsi(MediaInfoA_Inform(_handle, (IntPtr)0));
			}

			return Marshal.PtrToStringUni(MediaInfo_Inform(_handle, (IntPtr)0));
		}

		public string Get(StreamKind streamKind, int streamNumber, string parameter, InfoKind kindOfInfo = InfoKind.Text, InfoKind kindOfSearch = InfoKind.Name)
		{
			if (_mustUseAnsi)
			{
				var parameterPtr = Marshal.StringToHGlobalAnsi(parameter);
				var toReturn = Marshal.PtrToStringAnsi(MediaInfoA_Get(_handle, (IntPtr)streamKind, (IntPtr)streamNumber, parameterPtr, (IntPtr)kindOfInfo, (IntPtr)kindOfSearch));
				Marshal.FreeHGlobal(parameterPtr);
				return toReturn;
			}

			return Marshal.PtrToStringUni(MediaInfo_Get(_handle, (IntPtr)streamKind, (IntPtr)streamNumber, parameter, (IntPtr)kindOfInfo, (IntPtr)kindOfSearch));
		}

		public string Get(StreamKind streamKind, int streamNumber, int parameter, InfoKind kindOfInfo = InfoKind.Text)
		{
			if (_mustUseAnsi)
			{
				return Marshal.PtrToStringAnsi(MediaInfoA_GetI(_handle, (IntPtr)streamKind, (IntPtr)streamNumber, (IntPtr)parameter, (IntPtr)kindOfInfo));
			}

			return Marshal.PtrToStringUni(MediaInfo_GetI(_handle, (IntPtr)streamKind, (IntPtr)streamNumber, (IntPtr)parameter, (IntPtr)kindOfInfo));
		}

		public string Option(string option, string value = @"")
		{
			if (_mustUseAnsi)
			{
				var optionPtr = Marshal.StringToHGlobalAnsi(option);
				var valuePtr = Marshal.StringToHGlobalAnsi(value);
				var toReturn = Marshal.PtrToStringAnsi(MediaInfoA_Option(_handle, optionPtr, valuePtr));
				Marshal.FreeHGlobal(optionPtr);
				Marshal.FreeHGlobal(valuePtr);
				return toReturn;
			}

			return Marshal.PtrToStringUni(MediaInfo_Option(_handle, option, value));
		}

		public int State_Get()
		{
			return (int)MediaInfo_State_Get(_handle);
		}

		public int Count_Get(StreamKind streamKind, int streamNumber = -1)
		{
			return (int)MediaInfo_Count_Get(_handle, (IntPtr)streamKind, (IntPtr)streamNumber);
		}

		private void ReleaseUnmanagedResources()
		{
			MediaInfo_Close(_handle);
			MediaInfo_Delete(_handle);
		}

		~MediaInfo()
		{
			ReleaseUnmanagedResources();
		}

		public void Dispose()
		{
			ReleaseUnmanagedResources();
			GC.SuppressFinalize(this);
		}
	}
}