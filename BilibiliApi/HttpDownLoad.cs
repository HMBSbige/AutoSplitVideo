using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BilibiliApi
{
	public sealed class HttpDownLoad : IDisposable
	{
		private static readonly HttpClient Client = new HttpClient();
		private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();
		private readonly string Url;
		private readonly string Pathname;
		private Stream _responseStream;
		private FileStream _fileStream;
		private bool _isStarted;

		public HttpDownLoad(string url, string filename, bool overwrite)
		{
			Url = url;
			Pathname = Path.GetFullPath(filename);
			if (!overwrite && File.Exists(filename))
			{
				throw new InvalidOperationException($@"File {Pathname} already exists.");
			}
		}

		public async Task Start(HttpResponseMessage response = null)
		{
			if (_isStarted)
			{
				throw new InvalidOperationException($@"Downloading {Path.GetFileName(Pathname)}");
			}
			_isStarted = true;
			if (response == null)
			{
				response = await Client.GetAsync(Url, HttpCompletionOption.ResponseHeadersRead, _tokenSource.Token);
			}
			_responseStream = await response.Content.ReadAsStreamAsync();
			try
			{
				_fileStream = new FileStream(Pathname, FileMode.Create, FileAccess.Write, FileShare.Read);
				await _responseStream.CopyToAsync(_fileStream, 81920, _tokenSource.Token).ContinueWith(task =>
				{
					if (task.IsCanceled)
					{
						return;
					}

					_responseStream?.Dispose();
					_fileStream?.Dispose();
					_isStarted = false;
				});
			}
			catch
			{
				_responseStream.Close();
				_fileStream.Close();
				_isStarted = false;
				throw;
			}
			finally
			{
				response.Dispose();
			}
		}

		#region IDisposable Support
		private bool disposedValue = false; // 要检测冗余调用

		private void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					_tokenSource?.Cancel();
					//_tokenSource?.Dispose();
					_responseStream?.Dispose();
					_fileStream?.Dispose();
				}

				_fileStream = null;
				_responseStream = null;

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}
		#endregion
	}
}
