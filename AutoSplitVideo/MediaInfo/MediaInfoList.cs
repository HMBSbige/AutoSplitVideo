using System;
using System.Runtime.InteropServices;

namespace AutoSplitVideo.MediaInfo
{
	public partial class MediaInfoList
	{
		private readonly IntPtr _handle;

		public MediaInfoList()
		{
			try
			{
				_handle = MediaInfoList_New();
			}
			catch (BadImageFormatException)
			{
				throw new BadImageFormatException(@"目标平台错误");
			}
		}

		~MediaInfoList()
		{
			MediaInfoList_Delete(_handle);
		}

		public int Open(string fileName, InfoFileOptions options = InfoFileOptions.FileOptionNothing)
		{
			return (int)MediaInfoList_Open(_handle, fileName, (IntPtr)options);
		}

		public void Close(int filePos = -1)
		{
			MediaInfoList_Close(_handle, (IntPtr)filePos);
		}

		public string Inform(int filePos)
		{
			return Marshal.PtrToStringUni(MediaInfoList_Inform(_handle, (IntPtr)filePos, (IntPtr)0));
		}

		public string Get(int filePos, StreamKind streamKind, int streamNumber, string parameter, InfoKind kindOfInfo = InfoKind.Text, InfoKind kindOfSearch = InfoKind.Name)
		{
			return Marshal.PtrToStringUni(MediaInfoList_Get(_handle, (IntPtr)filePos, (IntPtr)streamKind, (IntPtr)streamNumber, parameter, (IntPtr)kindOfInfo, (IntPtr)kindOfSearch));
		}

		public string Get(int filePos, StreamKind streamKind, int streamNumber, int parameter, InfoKind kindOfInfo = InfoKind.Text)
		{
			return Marshal.PtrToStringUni(MediaInfoList_GetI(_handle, (IntPtr)filePos, (IntPtr)streamKind, (IntPtr)streamNumber, (IntPtr)parameter, (IntPtr)kindOfInfo));
		}

		public string Option(string option, string value = @"")
		{
			return Marshal.PtrToStringUni(MediaInfoList_Option(_handle, option, value));
		}

		public int State_Get()
		{
			return (int)MediaInfoList_State_Get(_handle);
		}

		public int Count_Get(int filePos, StreamKind streamKind, int streamNumber = -1)
		{
			return (int)MediaInfoList_Count_Get(_handle, (IntPtr)filePos, (IntPtr)streamKind, (IntPtr)streamNumber);
		}
	}
}
