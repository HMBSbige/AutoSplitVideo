using System;

namespace AutoSplitVideo.Flv
{
	internal static class BitHelper
	{
		public static int Read(ref ulong x, int length)
		{
			var r = (int)(x >> (64 - length));
			x <<= length;
			return r;
		}

		public static int Read(byte[] bytes, ref int offset, int length)
		{
			var startByte = offset / 8;
			var endByte = (offset + length - 1) / 8;
			var skipBits = offset % 8;
			ulong bits = 0;
			for (var i = 0; i <= Math.Min(endByte - startByte, 7); i++)
			{
				bits |= (ulong)bytes[startByte + i] << (56 - i * 8);
			}
			if (skipBits != 0) Read(ref bits, skipBits);
			offset += length;
			return Read(ref bits, length);
		}

		public static void Write(ref ulong x, int length, int value)
		{
			var mask = 0xFFFFFFFFFFFFFFFF >> (64 - length);
			x = (x << length) | ((ulong)value & mask);
		}

		public static byte[] CopyBlock(byte[] bytes, int offset, int length)
		{
			var startByte = offset / 8;
			var endByte = (offset + length - 1) / 8;
			var shiftA = offset % 8;
			var shiftB = 8 - shiftA;
			var dst = new byte[(length + 7) / 8];
			if (shiftA == 0)
			{
				Buffer.BlockCopy(bytes, startByte, dst, 0, dst.Length);
			}
			else
			{
				int i;
				for (i = 0; i < endByte - startByte; i++)
				{
					dst[i] = (byte)((bytes[startByte + i] << shiftA) | (bytes[startByte + i + 1] >> shiftB));
				}
				if (i < dst.Length)
				{
					dst[i] = (byte)(bytes[startByte + i] << shiftA);
				}
			}
			dst[dst.Length - 1] &= (byte)(0xFF << (dst.Length * 8 - length));
			return dst;
		}
	}
}