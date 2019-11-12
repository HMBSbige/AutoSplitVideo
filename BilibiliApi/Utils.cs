using System;
using System.Linq;
using System.Net.Sockets;

namespace BilibiliApi
{
	internal static class Utils
	{
		internal static byte[] ToBE(this byte[] b)
		{
			return BitConverter.IsLittleEndian ? b.Reverse().ToArray() : b;
		}

		internal static void ReadB(this NetworkStream stream, byte[] buffer, int offset, int count)
		{
			if (offset + count > buffer.Length)
			{
				throw new ArgumentException();
			}

			var read = 0;
			while (read < count)
			{
				var available = stream.Read(buffer, offset, count - read);
				if (available == 0)
				{
					throw new ObjectDisposedException(null);
				}

				read += available;
				offset += available;
			}
		}

		internal static string UserAgent
		{
			get
			{
				var version = typeof(Utils).Assembly.GetName().Version.ToString();
				return $@"Mozilla/5.0 AutoSplitVideo/{version} (+https://github.com/HMBSbige/AutoSplitVideo)";
			}
		}
	}
}
