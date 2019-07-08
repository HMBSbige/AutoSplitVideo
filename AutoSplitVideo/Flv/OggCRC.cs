namespace AutoSplitVideo.Flv
{
	internal static class OggCrc
	{
		private static readonly uint[] Lut = new uint[256];

		static OggCrc()
		{
			for (uint i = 0; i < 256; i++)
			{
				var x = i << 24;
				for (uint j = 0; j < 8; j++)
				{
					x = (x & 0x80000000U) != 0 ? (x << 1) ^ 0x04C11DB7 : x << 1;
				}
				Lut[i] = x;
			}
		}

		public static uint Calculate(byte[] buff, int offset, int length)
		{
			uint crc = 0;
			for (var i = 0; i < length; i++)
			{
				crc = Lut[((crc >> 24) ^ buff[offset + i]) & 0xFF] ^ (crc << 8);
			}
			return crc;
		}
	}
}