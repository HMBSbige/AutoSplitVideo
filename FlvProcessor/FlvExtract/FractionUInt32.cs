namespace FlvProcessor.FlvExtract
{
	public struct FractionUInt32
	{
		public uint N;
		public uint D;

		public FractionUInt32(uint n, uint d)
		{
			N = n;
			D = d;
		}

		private double ToDouble()
		{
			return (double)N / D;
		}

		public void Reduce()
		{
			var gcd = Gcd(N, D);
			N /= gcd;
			D /= gcd;
		}

		private static uint Gcd(uint a, uint b)
		{
			while (b != 0)
			{
				var r = a % b;
				a = b;
				b = r;
			}

			return a;
		}

		public override string ToString()
		{
			return ToString(true);
		}

		private string ToString(bool full)
		{
			return full ? $@"{ToDouble()} ({N}/{D})" : ToDouble().ToString(@"0.####");
		}
	}
}