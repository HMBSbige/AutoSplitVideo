using AutoSplitVideo.Utils;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;

namespace AutoSplitVideo.Service
{
	public static class AutoStartup
	{
		private static readonly string Key = $@"{UpdateChecker.Name}_{Directory.GetCurrentDirectory().GetDeterministicHashCode()}";
		private const string RegistryRunPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

		public static bool Set(bool enabled)
		{
			RegistryKey runKey = null;
			try
			{
				var path = Utils.Utils.GetExecutablePath();
				runKey = Utils.Utils.OpenRegKey(RegistryRunPath, true);
				if (enabled)
				{
					runKey.SetValue(Key, path);
				}
				else
				{
					runKey.DeleteValue(Key);
				}

				runKey.Close();
				return true;
			}
			catch
			{
				return Utils.Utils.RunAsAdmin(Utils.Utils.ParameterSetAutoRun) == 0;
			}
			finally
			{
				if (runKey != null)
				{
					try
					{
						runKey.Close();
					}
					catch (Exception e)
					{
						Log.ForceLog(e.ToString());
					}
				}
			}
		}

		public static bool Switch()
		{
			var enabled = !Check();
			RegistryKey runKey = null;
			try
			{
				var path = Utils.Utils.GetExecutablePath();
				runKey = Utils.Utils.OpenRegKey(RegistryRunPath, true);
				if (enabled)
				{
					runKey.SetValue(Key, path);
				}
				else
				{
					runKey.DeleteValue(Key);
				}

				runKey.Close();
				return true;
			}
			catch
			{
				return false;
			}
			finally
			{
				if (runKey != null)
				{
					try
					{
						runKey.Close();
					}
					catch (Exception e)
					{
						Log.ForceLog(e.ToString());
					}
				}
			}
		}

		public static bool Check()
		{
			RegistryKey runKey = null;
			try
			{
				runKey = Utils.Utils.OpenRegKey(RegistryRunPath, false);
				var runList = runKey.GetValueNames();
				runKey.Close();
				return runList.Any(item => item.Equals(Key));
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return false;
			}
			finally
			{
				if (runKey != null)
				{
					try
					{
						runKey.Close();
					}
					catch (Exception e)
					{
						Console.WriteLine(e);
					}
				}
			}
		}
	}
}
