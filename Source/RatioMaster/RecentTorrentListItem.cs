namespace RatioMaster;

internal class RecentTorrentListItem
{
	public ApplicationSettings applicationSettings;

	public string FilePath;

	public override string ToString()
	{
		return applicationSettings.torrentFilePath;
	}
}
