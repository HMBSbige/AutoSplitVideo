using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AutoSplitVideo.Utils
{
	public class HttpDownLoad : IDisposable
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

		public async Task Start()
		{
			if (_isStarted)
			{
				throw new InvalidOperationException($@"Downloading {Path.GetFileName(Pathname)}");
			}
			_isStarted = true;
			using (var response = await Client.GetAsync(Url, HttpCompletionOption.ResponseHeadersRead, _tokenSource.Token))
			{
				_responseStream = await response.Content.ReadAsStreamAsync();
				try
				{
					_fileStream = new FileStream(Pathname, FileMode.Create, FileAccess.Write, FileShare.None);
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
			}
		}

		public void Stop()
		{
			if (_isStarted)
			{
				_tokenSource.Cancel();
				_responseStream.Dispose();
				_responseStream = null;
				_fileStream.Dispose();
				_fileStream = null;
				_isStarted = false;
			}
		}

		public void Dispose()
		{
			_tokenSource?.Dispose();
			_responseStream?.Dispose();
			_fileStream?.Dispose();
		}
	}
}
