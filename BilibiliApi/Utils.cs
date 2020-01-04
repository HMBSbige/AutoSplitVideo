using System;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace BilibiliApi
{
	public static class Utils
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

		public static string UserAgent
		{
			get
			{
				var version = typeof(Utils).Assembly.GetName().Version.ToString();
				return $@"Mozilla/5.0 AutoSplitVideo/{version} (+https://github.com/HMBSbige/AutoSplitVideo)";
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
			return token.Length == 32 && Regex.IsMatch(token, @"^[a-f0-9]+$");
		}
	}
}
