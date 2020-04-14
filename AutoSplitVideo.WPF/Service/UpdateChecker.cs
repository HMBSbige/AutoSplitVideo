using AutoSplitVideo.Core.GitHubRelease;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace AutoSplitVideo.Service
{
	public class UpdateChecker : HttpRequest
	{
		private const string Owner = @"HMBSbige";
		private const string Repo = @"AutoSplitVideo";

		public string LatestVersionNumber;
		public string LatestVersionUrl;

		public event EventHandler NewVersionFound;
		public event EventHandler NewVersionFoundFailed;
		public event EventHandler NewVersionNotFound;

		public const string Name = @"AutoSplitVideo";
		public const string Copyright = @"Copyright © HMBSbige 2018 - 2020";
		public const string Version = @"2.2.1";

		public async Task Check(bool notifyNoFound, bool isPreRelease)
		{
			try
			{
				var updater = new GitHubRelease(Owner, Repo);
				var url = updater.AllReleaseUrl;

				var json = await GetAsync(url);

				var releases = JsonSerializer.Deserialize<List<Release>>(json);
				var latestRelease = VersionUtil.GetLatestRelease(releases, isPreRelease);
				if (VersionUtil.CompareVersion(latestRelease.tag_name, Version) > 0)
				{
					LatestVersionNumber = latestRelease.tag_name;
					LatestVersionUrl = latestRelease.html_url;
					NewVersionFound?.Invoke(this, new EventArgs());
				}
				else
				{
					LatestVersionNumber = latestRelease.tag_name;
					if (notifyNoFound)
					{
						NewVersionNotFound?.Invoke(this, new EventArgs());
					}
				}
			}
			catch
			{
				if (notifyNoFound)
				{
					NewVersionFoundFailed?.Invoke(this, new EventArgs());
				}
			}
		}
	}
}
