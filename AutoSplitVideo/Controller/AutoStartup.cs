using AutoSplitVideo.Utils;
using Microsoft.Win32;
using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace AutoSplitVideo.Controller
{
	public static class AutoStartup
	{
		private static readonly string Key = $@"{ExeName}_" + Application.StartupPath.GetHashCode();
		private static readonly string RegistryRunPath = !Environment.Is64BitProcess ? @"Software\Microsoft\Windows\CurrentVersion\Run" : @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Run";

		private static string ExecutablePath => Assembly.GetExecutingAssembly().Location;
		private static string ExeName => Assembly.GetExecutingAssembly().GetName().Name;

		public static bool Set(bool enabled)
		{
			RegistryKey runKey = null;
			try
			{
				var path = $@"""{ExecutablePath}"" --silent";
				runKey = Registry.LocalMachine.OpenSubKey(RegistryRunPath, true);
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
				return Util.RunAsAdmin(@"--setAutoRun") == 0;
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

		public static bool Switch()
		{
			var enabled = !Check();
			RegistryKey runKey = null;
			try
			{
				var path = $@"""{ExecutablePath}"" --silent";
				runKey = Registry.LocalMachine.OpenSubKey(RegistryRunPath, true);
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
						Console.WriteLine(e);
					}
				}
			}
		}

		public static bool Check()
		{
			RegistryKey runKey = null;
			try
			{
				runKey = Registry.LocalMachine.OpenSubKey(RegistryRunPath, false);
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
