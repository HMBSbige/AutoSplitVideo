namespace FlvProcessor.FlvExtract.Utils
{
	internal static class BitConverterLE
	{
		public static byte[] GetBytes(ulong value)
		{
			var buff = new byte[8];
			buff[0] = (byte)value;
			buff[1] = (byte)(value >> 8);
			buff[2] = (byte)(value >> 16);
			buff[3] = (byte)(value >> 24);
			buff[4] = (byte)(value >> 32);
			buff[5] = (byte)(value >> 40);
			buff[6] = (byte)(value >> 48);
			buff[7] = (byte)(value >> 56);
			return buff;
		}

		public static byte[] GetBytes(uint value)
		{
			var buff = new byte[4];
			buff[0] = (byte)value;
			buff[1] = (byte)(value >> 8);
			buff[2] = (byte)(value >> 16);
			buff[3] = (byte)(value >> 24);
			return buff;
		}

		public static byte[] GetBytes(ushort value)
		{
			var buff = new byte[2];
			buff[0] = (byte)value;
			buff[1] = (byte)(value >> 8);
			return buff;
		}
	}
}