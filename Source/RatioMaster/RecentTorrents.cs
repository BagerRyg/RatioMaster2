using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace RatioMaster;

internal class RecentTorrents
{
	private MainForm _mainForm;

	public int RecentTorrentsListLength = 10;

	private string TorrentsConfigDir = Application.StartupPath + "\\Torrents Config\\";

	private FileStream myFileStream;

	private XmlSerializer mySerializer = new XmlSerializer(typeof(ApplicationSettings));

	public RecentTorrents(MainForm mainForm)
	{
		_mainForm = mainForm;
	}

	public List<RecentTorrentListItem> GetRecentTorrents()
	{
		List<RecentTorrentListItem> list = new List<RecentTorrentListItem>();
		if (!Directory.Exists(TorrentsConfigDir))
		{
			return list;
		}
		string[] files = Directory.GetFiles(TorrentsConfigDir, "*.config");
		Array.Sort(files, new FileDateComparer());
		int num = 0;
		string[] array = files;
		foreach (string text in array)
		{
			try
			{
				FileInfo fileInfo = new FileInfo(text);
				if (!fileInfo.Exists)
				{
					continue;
				}
				myFileStream = fileInfo.OpenRead();
				ApplicationSettings applicationSettings = (ApplicationSettings)mySerializer.Deserialize(myFileStream);
				if (applicationSettings != null)
				{
					RecentTorrentListItem recentTorrentListItem = new RecentTorrentListItem();
					recentTorrentListItem.applicationSettings = applicationSettings;
					recentTorrentListItem.FilePath = text;
					RecentTorrentListItem item = recentTorrentListItem;
					list.Add(item);
					num++;
					if (num >= RecentTorrentsListLength)
					{
						break;
					}
				}
			}
			catch (Exception ex)
			{
				_mainForm.AddLogLine("Error loading config: " + ex.Message);
			}
			finally
			{
				if (myFileStream != null)
				{
					myFileStream.Close();
				}
			}
		}
		return list;
	}
}
