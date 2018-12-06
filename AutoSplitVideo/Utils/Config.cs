using System.Runtime.InteropServices;
using System.Text;

namespace AutoSplitVideo.Utils
{
	public static class Config
	{
		[DllImport(@"kernel32")]
		private static extern bool WritePrivateProfileString(byte[] section, byte[] key, byte[] val, string filePath);

		[DllImport(@"kernel32")]
		private static extern int GetPrivateProfileString(byte[] section, byte[] key, byte[] def, byte[] retVal, int size, string filePath);

		//与ini交互必须统一编码格式
		private static byte[] GetBytes(string s, string encodingName)
		{
			return null == s ? null : Encoding.GetEncoding(encodingName).GetBytes(s);
		}

		public static string ReadString(string section, string key, string def, string fileName, string encodingName = @"utf-8", int size = 1024)
		{
			var buffer = new byte[size];
			var count = GetPrivateProfileString(GetBytes(section, encodingName), GetBytes(key, encodingName), GetBytes(def, encodingName), buffer, size, fileName);
			return Encoding.GetEncoding(encodingName).GetString(buffer, 0, count).Trim();
		}

		public static bool WriteString(string section, string key, string value, string fileName, string encodingName = @"utf-8")
		{
			return WritePrivateProfileString(GetBytes(section, encodingName), GetBytes(key, encodingName), GetBytes(value, encodingName), fileName);
		}
	}
}
