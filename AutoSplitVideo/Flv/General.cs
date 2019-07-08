using System;

namespace AutoSplitVideo.Flv
{
	internal static class General
	{
		public static void CopyBytes(byte[] dst, int dstOffset, byte[] src)
		{
			Buffer.BlockCopy(src, 0, dst, dstOffset, src.Length);
		}
	}
}
