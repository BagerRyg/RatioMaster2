using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace RatioMaster;

internal static class AppPaths
{
	private const string AppDataFolderName = "RatioMaster 2.0";

	public static string DataDirectory
	{
		get
		{
			string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			if (string.IsNullOrWhiteSpace(localAppData))
			{
				return Application.StartupPath;
			}
			return Path.Combine(localAppData, AppDataFolderName);
		}
	}

	public static string ConfigPath => Path.Combine(DataDirectory, "ratiomaster.config");

	public static string LegacyConfigPath => Path.Combine(Application.StartupPath, "ratiomaster.config");

	public static string TorrentConfigDirectory => Path.Combine(DataDirectory, "Torrents Config");

	public static string LegacyTorrentConfigDirectory => Path.Combine(Application.StartupPath, "Torrents Config");

	public static string PeerListDirectory => Path.Combine(DataDirectory, "PeerLists");

	public static string RuntimeLogPath => Path.Combine(DataDirectory, "runtime.log");

	public static void EnsureDataDirectory()
	{
		Directory.CreateDirectory(DataDirectory);
	}

	public static void MigrateLegacyUserData()
	{
		EnsureDataDirectory();
		TryDataMaintenance(() => CopySanitizedIfMissing(LegacyConfigPath, ConfigPath), "migrate legacy settings");
		TryDataMaintenance(() => CopySanitizedDirectoryFilesIfMissing(LegacyTorrentConfigDirectory, TorrentConfigDirectory, "*.config"), "migrate legacy torrent settings");
		TryDataMaintenance(() => ScrubConfigFile(ConfigPath), "scrub application settings");
		TryDataMaintenance(() => ScrubConfigDirectory(TorrentConfigDirectory), "scrub torrent settings");
		TryDataMaintenance(() => DeleteDirectory(PeerListDirectory), "remove legacy peer lists");
	}

	private static void CopySanitizedIfMissing(string sourcePath, string destinationPath)
	{
		if (!File.Exists(sourcePath) || File.Exists(destinationPath))
		{
			return;
		}
		Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
		File.WriteAllText(destinationPath, ScrubConfigText(File.ReadAllText(sourcePath)));
	}

	private static void CopySanitizedDirectoryFilesIfMissing(string sourceDirectory, string destinationDirectory, string searchPattern)
	{
		if (!Directory.Exists(sourceDirectory))
		{
			return;
		}
		Directory.CreateDirectory(destinationDirectory);
		foreach (string sourcePath in Directory.GetFiles(sourceDirectory, searchPattern))
		{
			string destinationPath = Path.Combine(destinationDirectory, Path.GetFileName(sourcePath));
			CopySanitizedIfMissing(sourcePath, destinationPath);
		}
	}

	private static void ScrubConfigDirectory(string directory)
	{
		if (!Directory.Exists(directory))
		{
			return;
		}
		foreach (string path in Directory.GetFiles(directory, "*.config"))
		{
			ScrubConfigFile(path);
		}
	}

	private static void ScrubConfigFile(string path)
	{
		if (File.Exists(path))
		{
			File.WriteAllText(path, ScrubConfigText(File.ReadAllText(path)));
		}
	}

	private static string ScrubConfigText(string content)
	{
		string[] elements = { "customKey", "customPeerID", "textProxyPass" };
		foreach (string element in elements)
		{
			content = Regex.Replace(content, "<" + element + @">.*?</" + element + ">", "<" + element + " />", RegexOptions.IgnoreCase | RegexOptions.Singleline);
		}
		return content;
	}

	private static void DeleteDirectory(string path)
	{
		if (Directory.Exists(path))
		{
			Directory.Delete(path, recursive: true);
		}
	}

	private static void TryDataMaintenance(Action action, string operation)
	{
		try
		{
			action();
		}
		catch (Exception ex)
		{
			RuntimeLog.WriteException("Failed to " + operation, ex);
		}
	}
}
