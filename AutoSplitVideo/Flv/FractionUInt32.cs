namespace AutoSplitVideo.Flv
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

		public double ToDouble()
		{
			return (double)N / (double)D;
		}

		public void Reduce()
		{
			var gcd = GCD(N, D);
			N /= gcd;
			D /= gcd;
		}

		private uint GCD(uint a, uint b)
		{
			uint r;

			while (b != 0)
			{
				r = a % b;
				a = b;
				b = r;
			}

			return a;
		}

		public override string ToString()
		{
			return ToString(true);
		}

		public string ToString(bool full)
		{
			if (full)
			{
				return ToDouble().ToString() + " (" + N.ToString() + "/" + D.ToString() + ")";
			}
			else
			{
				return ToDouble().ToString("0.####");
			}
		}
	}
}