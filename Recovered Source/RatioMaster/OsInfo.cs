using System;

namespace RatioMaster;

internal class OsInfo
{
	private static OsInfo Instance;

	public string ShortVersion { get; set; }

	public string FullVersion { get; set; }

	protected OsInfo()
	{
		ShortVersion = "";
		FullVersion = "";
	}

	public static OsInfo Get()
	{
		if (Instance == null)
		{
			OsInfo osInfo = new OsInfo();
			osInfo.ShortVersion = GetShortOsVersion();
			osInfo.FullVersion = Environment.OSVersion.VersionString;
			Instance = osInfo;
		}
		return Instance;
	}

	public static string GetShortOsVersion()
	{
		PlatformID platform = Environment.OSVersion.Platform;
		Version version = Environment.OSVersion.Version;
		switch (platform)
		{
		case PlatformID.Win32S:
			return "Windows 3.1";
		case PlatformID.Win32NT:
			if (version.Major <= 4)
			{
				return "Windows NT";
			}
			if (Environment.OSVersion.Version.Major == 5)
			{
				if (version.Minor == 0)
				{
					return "Windows 2000";
				}
				if (version.Minor == 1)
				{
					return "Windows XP";
				}
				if (version.Minor == 2)
				{
					return "Windows 2003";
				}
			}
			else if (version.Major == 6)
			{
				if (version.Minor == 0)
				{
					return "Windows Vista";
				}
				if (version.Minor == 1)
				{
					return "Windows 2008";
				}
				if (version.Minor == 2)
				{
					return "Windows 7";
				}
			}
			break;
		case PlatformID.Win32Windows:
			if (Environment.OSVersion.Version.Major >= 4)
			{
				if (version.Minor == 0)
				{
					return "Windows 95";
				}
				if (version.Minor < 90)
				{
					return "Windows 98";
				}
				if (version.Minor == 90)
				{
					return "Windows ME";
				}
			}
			break;
		case PlatformID.WinCE:
			return "Windows CE";
		case PlatformID.Unix:
			return "Unix";
		}
		return Environment.OSVersion.ToString();
	}
}
