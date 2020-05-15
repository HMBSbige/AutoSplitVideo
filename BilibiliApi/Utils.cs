using System;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace BilibiliApi
{
	public static class Utils
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void NoWarning(this Task t) { }

		internal static byte[] ToBE(this byte[] b)
		{
			return BitConverter.IsLittleEndian ? b.Reverse().ToArray() : b;
		}

		internal static short ToInt16(this byte[] bytes, int startIndex)
		{
			var a = new byte[2];
			Buffer.BlockCopy(bytes, startIndex, a, 0, a.Length);
			var buff = BitConverter.IsLittleEndian ? a.Reverse().ToArray() : a;
			return BitConverter.ToInt16(buff, 0);
		}

		internal static int ToInt32(this byte[] bytes, int startIndex)
		{
			var a = new byte[4];
			Buffer.BlockCopy(bytes, startIndex, a, 0, a.Length);
			var buff = BitConverter.IsLittleEndian ? a.Reverse().ToArray() : a;
			return BitConverter.ToInt32(buff, 0);
		}

		internal static async Task ReadByteAsync(this NetworkStream stream, byte[] buffer, int offset, int count, CancellationToken token = default)
		{
			if (offset + count > buffer.Length)
			{
				throw new ArgumentException();
			}

			var read = 0;
			while (read < count)
			{
				var available = await stream.ReadAsync(buffer, offset, count - read, token);
				if (available == 0)
				{
					throw new ObjectDisposedException(null);
				}

				read += available;
				offset += available;
			}
		}

		public static string UserAgent
		{
			get
			{
				var version = typeof(Utils).Assembly.GetName().Version?.ToString();
				return $@"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/81.0.4044.138 Safari/537.36 AutoSplitVideo/{version} (+https://github.com/HMBSbige/AutoSplitVideo)";
			}
		}

		public static string Md5(string str)
		{
			using MD5 md5 = new MD5CryptoServiceProvider();
			var data = Encoding.UTF8.GetBytes(str);
			var md5Data = md5.ComputeHash(data);
			return md5Data.Aggregate(string.Empty, (current, t) => current + t.ToString(@"x2").PadLeft(2, '0'));
		}

		private static RSA ReadKey(string pemContents)
		{
			const string header = @"-----BEGIN PUBLIC KEY-----";
			const string footer = @"-----END PUBLIC KEY-----";

			if (pemContents.StartsWith(header))
			{
				var endIdx = pemContents.IndexOf(footer, header.Length, StringComparison.Ordinal);
				var base64 = pemContents.Substring(header.Length, endIdx - header.Length);

				var der = Convert.FromBase64String(base64);
				var rsa = RSA.Create();
				rsa.ImportSubjectPublicKeyInfo(der, out _);
				return rsa;
			}

			throw new InvalidOperationException();
		}

		public static string RsaEncrypt(string publicKey, string str)
		{
			using var rsa = ReadKey(publicKey);
			var cipherBytes = rsa.Encrypt(Encoding.UTF8.GetBytes(str), RSAEncryptionPadding.Pkcs1);
			return Convert.ToBase64String(cipherBytes);
		}

		public static DateTime GetTime(string timeStamp)
		{
			var dtStart = TimeZoneInfo.ConvertTimeFromUtc(new DateTime(1970, 1, 1), TimeZoneInfo.Utc);
			var lTime = long.Parse($@"{timeStamp}0000000");
			var toNow = new TimeSpan(lTime);
			return dtStart.Add(toNow);
		}

		public static bool IsToken(string token)
		{
			return !string.IsNullOrEmpty(token) && token.Length == 32 && Regex.IsMatch(token, @"^[a-f0-9]+$");
		}
	}
}
