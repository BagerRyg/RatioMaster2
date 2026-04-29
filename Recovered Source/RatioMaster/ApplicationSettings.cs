using System;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace RatioMaster;

public class ApplicationSettings
{
	private string _uploadRate;

	private string _downloadRate;

	private string _interval;

	private int _TorrentClientsIndex;

	private bool _checkLogEnabled;

	private bool _checkRequestScrap;

	private bool _checkShowTrayBaloon;

	private bool _checkTCPListen;

	private string _customPort;

	private string _customKey;

	private string _customPeersNum;

	private string _customPeerID;

	private string _textProxyHost;

	private string _textProxyPort;

	private string _textProxyPass;

	private string _textProxyUser;

	private int _comboProxyTypeIndex;

	private bool _checkRandomUpload;

	private bool _checkRandomDownload;

	private string _RandomUploadFrom;

	private string _RandomUploadTo;

	private string _RandomDownloadFrom;

	private string _RandomDownloadTo;

	private string _configPath = Application.StartupPath + "\\ratiomaster.config";

	private string _torrentsConfigDir = Application.StartupPath + "\\Torrents Config\\";

	private string _finishedPercent;

	private bool _updateAnnounceParamsOnStart;

	private decimal _textStopMinLeecher;

	private bool _ignoreFailureReason;

	private string _torrentFilePath;

	private string _torrentHash;

	private int _stopProcessActionBox;

	private string _stopProcessValue;

	private int _stopProcessUnitsBox;

	private string _selectedLanguage;

	private string _bindToIp;

	private bool _minimizeTotray;

	private bool _ignoreTimeout;

	private bool _useUPnP;

	private bool _savePeerList;

	private string _interfaceTheme;

	private Form1 _mainForm;

	public decimal textStopMinLeecher
	{
		get
		{
			return _textStopMinLeecher;
		}
		set
		{
			_textStopMinLeecher = value;
		}
	}

	public bool updateAnnounceParamsOnStart
	{
		get
		{
			return _updateAnnounceParamsOnStart;
		}
		set
		{
			_updateAnnounceParamsOnStart = value;
		}
	}

	public string finishedPercent
	{
		get
		{
			return _finishedPercent;
		}
		set
		{
			_finishedPercent = value;
		}
	}

	public string uploadRate
	{
		get
		{
			return _uploadRate;
		}
		set
		{
			_uploadRate = value;
		}
	}

	public string downloadRate
	{
		get
		{
			return _downloadRate;
		}
		set
		{
			_downloadRate = value;
		}
	}

	public string interval
	{
		get
		{
			return _interval;
		}
		set
		{
			_interval = value;
		}
	}

	public int TorrentClientsIndex
	{
		get
		{
			return _TorrentClientsIndex;
		}
		set
		{
			_TorrentClientsIndex = value;
		}
	}

	public bool checkLogEnabled
	{
		get
		{
			return _checkLogEnabled;
		}
		set
		{
			_checkLogEnabled = value;
		}
	}

	public bool checkRequestScrap
	{
		get
		{
			return _checkRequestScrap;
		}
		set
		{
			_checkRequestScrap = value;
		}
	}

	public bool checkShowTrayBaloon
	{
		get
		{
			return _checkShowTrayBaloon;
		}
		set
		{
			_checkShowTrayBaloon = value;
		}
	}

	public bool checkTCPListen
	{
		get
		{
			return _checkTCPListen;
		}
		set
		{
			_checkTCPListen = value;
		}
	}

	public string customPort
	{
		get
		{
			return _customPort;
		}
		set
		{
			_customPort = value;
		}
	}

	public string customKey
	{
		get
		{
			return _customKey;
		}
		set
		{
			_customKey = value;
		}
	}

	public string customPeersNum
	{
		get
		{
			return _customPeersNum;
		}
		set
		{
			_customPeersNum = value;
		}
	}

	public string customPeerID
	{
		get
		{
			return _customPeerID;
		}
		set
		{
			_customPeerID = value;
		}
	}

	public string textProxyHost
	{
		get
		{
			return _textProxyHost;
		}
		set
		{
			_textProxyHost = value;
		}
	}

	public string textProxyPort
	{
		get
		{
			return _textProxyPort;
		}
		set
		{
			_textProxyPort = value;
		}
	}

	public string textProxyPass
	{
		get
		{
			return _textProxyPass;
		}
		set
		{
			_textProxyPass = value;
		}
	}

	public string textProxyUser
	{
		get
		{
			return _textProxyUser;
		}
		set
		{
			_textProxyUser = value;
		}
	}

	public int comboProxyTypeIndex
	{
		get
		{
			return _comboProxyTypeIndex;
		}
		set
		{
			_comboProxyTypeIndex = value;
		}
	}

	public bool checkRandomUpload
	{
		get
		{
			return _checkRandomUpload;
		}
		set
		{
			_checkRandomUpload = value;
		}
	}

	public bool checkRandomDownload
	{
		get
		{
			return _checkRandomDownload;
		}
		set
		{
			_checkRandomDownload = value;
		}
	}

	public string RandomUploadFrom
	{
		get
		{
			return _RandomUploadFrom;
		}
		set
		{
			_RandomUploadFrom = value;
		}
	}

	public string RandomUploadTo
	{
		get
		{
			return _RandomUploadTo;
		}
		set
		{
			_RandomUploadTo = value;
		}
	}

	public string RandomDownloadFrom
	{
		get
		{
			return _RandomDownloadFrom;
		}
		set
		{
			_RandomDownloadFrom = value;
		}
	}

	public string RandomDownloadTo
	{
		get
		{
			return _RandomDownloadTo;
		}
		set
		{
			_RandomDownloadTo = value;
		}
	}

	public bool ignoreFailureReason
	{
		get
		{
			return _ignoreFailureReason;
		}
		set
		{
			_ignoreFailureReason = value;
		}
	}

	public string torrentFilePath
	{
		get
		{
			return _torrentFilePath;
		}
		set
		{
			_torrentFilePath = value;
		}
	}

	public string torrentHash
	{
		get
		{
			return _torrentHash;
		}
		set
		{
			_torrentHash = value;
		}
	}

	public int stopProcessActionBox
	{
		get
		{
			return _stopProcessActionBox;
		}
		set
		{
			_stopProcessActionBox = value;
		}
	}

	public string stopProcessValue
	{
		get
		{
			return _stopProcessValue;
		}
		set
		{
			_stopProcessValue = value;
		}
	}

	public int stopProcessUnitsBox
	{
		get
		{
			return _stopProcessUnitsBox;
		}
		set
		{
			_stopProcessUnitsBox = value;
		}
	}

	public string selectedLanguage
	{
		get
		{
			return _selectedLanguage;
		}
		set
		{
			_selectedLanguage = value;
		}
	}

	public string bindToIp
	{
		get
		{
			return _bindToIp;
		}
		set
		{
			_bindToIp = value;
		}
	}

	public bool minimizeTotray
	{
		get
		{
			return _minimizeTotray;
		}
		set
		{
			_minimizeTotray = value;
		}
	}

	public bool ignoreTimeout
	{
		get
		{
			return _ignoreTimeout;
		}
		set
		{
			_ignoreTimeout = value;
		}
	}

	public bool useUPnP
	{
		get
		{
			return _useUPnP;
		}
		set
		{
			_useUPnP = value;
		}
	}

	public bool savePeerList
	{
		get
		{
			return _savePeerList;
		}
		set
		{
			_savePeerList = value;
		}
	}

	public string interfaceTheme
	{
		get
		{
			return _interfaceTheme;
		}
		set
		{
			_interfaceTheme = value;
		}
	}

	public ApplicationSettings()
	{
	}

	public ApplicationSettings(Form1 MainForm)
	{
		_mainForm = MainForm;
		if (!Directory.Exists(_torrentsConfigDir))
		{
			Directory.CreateDirectory(_torrentsConfigDir);
		}
	}

	public void SaveAppSettings()
	{
		XmlSerializer xmlSerializer = null;
		try
		{
			uploadRate = _mainForm.uploadRate.Text;
			downloadRate = _mainForm.downloadRate.Text;
			interval = _mainForm.interval.Text;
			TorrentClientsIndex = _mainForm.TorrentClientsBox.SelectedIndex;
			checkLogEnabled = _mainForm.checkLogEnabled.Checked;
			checkRequestScrap = _mainForm.checkRequestScrap.Checked;
			checkShowTrayBaloon = _mainForm.checkShowTrayBaloon.Checked;
			checkTCPListen = _mainForm.checkTCPListen.Checked;
			customPort = _mainForm.customPort.Text;
			customKey = _mainForm.customKey.Text;
			customPeersNum = _mainForm.customPeersNum.Text;
			customPeerID = _mainForm.customPeerID.Text;
			textProxyHost = _mainForm.textProxyHost.Text;
			textProxyPort = _mainForm.textProxyPort.Text;
			textProxyPass = _mainForm.textProxyPass.Text;
			textProxyUser = _mainForm.textProxyUser.Text;
			comboProxyTypeIndex = _mainForm.comboProxyType.SelectedIndex;
			checkRandomUpload = _mainForm.checkRandomUpload.Checked;
			checkRandomDownload = _mainForm.checkRandomDownload.Checked;
			RandomUploadFrom = _mainForm.RandomUploadFrom.Text;
			RandomUploadTo = _mainForm.RandomUploadTo.Text;
			RandomDownloadFrom = _mainForm.RandomDownloadFrom.Text;
			RandomDownloadTo = _mainForm.RandomDownloadTo.Text;
			finishedPercent = _mainForm.fileSize.Text;
			updateAnnounceParamsOnStart = _mainForm.updateAnnounceParamsOnStart.Checked;
			textStopMinLeecher = _mainForm.textStopMinLeecher.Value;
			ignoreFailureReason = _mainForm.checkIgnoreFailureReason.Checked;
			torrentFilePath = _mainForm.torrentFile.Text;
			torrentHash = _mainForm.shaHash.Text.Trim();
			stopProcessActionBox = _mainForm.stopProcessActionBox.SelectedIndex;
			stopProcessUnitsBox = _mainForm.stopProcessUnitsBox.SelectedIndex;
			stopProcessValue = _mainForm.stopProcessValue.Text;
			selectedLanguage = ((LangInfo)_mainForm.cbbLanguages.SelectedItem).File;
			bindToIp = ((Form1.KeyValuePair)_mainForm.comboBindIp.SelectedItem).Key.ToString();
			minimizeTotray = _mainForm.checkMinimizeToTray.Checked;
			ignoreTimeout = _mainForm.checkIgnoreTimeout.Checked;
			useUPnP = _mainForm.checkUPnP.Checked;
			savePeerList = _mainForm.checkSavePeers.Checked;
			interfaceTheme = _mainForm.cbbInterfaceTheme.SelectedItem?.ToString() ?? DarkTheme.CurrentThemeName;
			xmlSerializer = new XmlSerializer(typeof(ApplicationSettings));
			using (StreamWriter streamWriter = new StreamWriter(_configPath, append: false))
			{
				xmlSerializer.Serialize(streamWriter, this);
			}
			if (torrentHash.Length == 40)
			{
				using StreamWriter streamWriter = new StreamWriter(getTorrentConfigPath(torrentHash), append: false);
				xmlSerializer.Serialize(streamWriter, this);
			}
		}
		catch (Exception ex)
		{
			_mainForm.AddLogLine("Error saving config: " + ex.Message);
		}
	}

	private string GetValueDef(string val, string defVal)
	{
		if (val.Length > 0)
		{
			return val;
		}
		return defVal;
	}

	public string getTorrentConfigPath(string torrentHash)
	{
		return _torrentsConfigDir + "torrent_" + torrentHash + ".config";
	}

	public ApplicationSettings getTorrentSettings(string torrentHash)
	{
		XmlSerializer xmlSerializer = null;
		FileStream fileStream = null;
		ApplicationSettings result = null;
		xmlSerializer = new XmlSerializer(typeof(ApplicationSettings));
		string torrentConfigPath = getTorrentConfigPath(torrentHash);
		FileInfo fileInfo = new FileInfo(torrentConfigPath);
		try
		{
			if (fileInfo.Exists)
			{
				fileStream = fileInfo.OpenRead();
				result = (ApplicationSettings)xmlSerializer.Deserialize(fileStream);
				_mainForm.AddLogLine("Loaded settings from torrent config: " + torrentConfigPath);
			}
		}
		catch (Exception ex)
		{
			_mainForm.AddLogLine("Error loading Torrent config: " + ex.Message);
			result = null;
		}
		finally
		{
			fileStream?.Close();
		}
		return result;
	}

	public void LoadTorrentSettings(string torrentHash)
	{
		ApplicationSettings applicationSettings = null;
		applicationSettings = getTorrentSettings(torrentHash);
		if (applicationSettings != null)
		{
			applySettingsToForm(applicationSettings);
		}
	}

	public bool LoadAppSettings()
	{
		XmlSerializer xmlSerializer = null;
		FileStream fileStream = null;
		bool result = false;
		try
		{
			xmlSerializer = new XmlSerializer(typeof(ApplicationSettings));
			FileInfo fileInfo = new FileInfo(_configPath);
			if (fileInfo.Exists)
			{
				fileStream = fileInfo.OpenRead();
				ApplicationSettings applicationSettings = null;
				ApplicationSettings applicationSettings2 = (ApplicationSettings)xmlSerializer.Deserialize(fileStream);
				ApplicationSettings applicationSettings3 = applicationSettings2;
				string text = applicationSettings2.torrentHash;
				if (text != null && text.Length > 0)
				{
					applicationSettings = getTorrentSettings(text);
					if (applicationSettings != null)
					{
						applicationSettings2 = applicationSettings;
						applicationSettings2.checkShowTrayBaloon = applicationSettings3.checkShowTrayBaloon;
						applicationSettings2.minimizeTotray = applicationSettings3.minimizeTotray;
						applicationSettings2.interfaceTheme = applicationSettings3.interfaceTheme;
					}
				}
				bindToIp = applicationSettings2.bindToIp;
				applySettingsToForm(applicationSettings2);
				_mainForm.torrentLoadedSuccessfully = _mainForm.loadTorrentFileInfo(applicationSettings2.torrentFilePath, loadSettings: false);
				result = true;
			}
		}
		catch (Exception ex)
		{
			_mainForm.AddLogLine("Error loading config: " + ex.Message);
		}
		finally
		{
			fileStream?.Close();
		}
		return result;
	}

	private void applySettingsToForm(ApplicationSettings myAppSettings)
	{
		try
		{
			_mainForm.uploadRate.Text = myAppSettings.uploadRate;
			_mainForm.downloadRate.Text = myAppSettings.downloadRate;
			_mainForm.interval.Text = myAppSettings.interval;
			_mainForm.TorrentClientsBox.SelectedIndex = myAppSettings.TorrentClientsIndex;
			_mainForm.checkLogEnabled.Checked = myAppSettings.checkLogEnabled;
			_mainForm.checkRequestScrap.Checked = myAppSettings.checkRequestScrap;
			_mainForm.checkShowTrayBaloon.Checked = myAppSettings.checkShowTrayBaloon;
			_mainForm.checkTCPListen.Checked = myAppSettings.checkTCPListen;
			_mainForm.customPort.Text = myAppSettings.customPort;
			_mainForm.customKey.Text = myAppSettings.customKey;
			_mainForm.customPeersNum.Text = myAppSettings.customPeersNum;
			_mainForm.customPeerID.Text = myAppSettings.customPeerID;
			_mainForm.textProxyHost.Text = myAppSettings.textProxyHost;
			_mainForm.textProxyPort.Text = myAppSettings.textProxyPort;
			_mainForm.textProxyPass.Text = myAppSettings.textProxyPass;
			_mainForm.textProxyUser.Text = myAppSettings.textProxyUser;
			_mainForm.comboProxyType.SelectedIndex = myAppSettings.comboProxyTypeIndex;
			_mainForm.checkRandomUpload.Checked = myAppSettings.checkRandomUpload;
			_mainForm.checkRandomDownload.Checked = myAppSettings.checkRandomDownload;
			_mainForm.RandomUploadFrom.Text = myAppSettings.RandomUploadFrom;
			_mainForm.RandomUploadTo.Text = myAppSettings.RandomUploadTo;
			_mainForm.RandomDownloadFrom.Text = myAppSettings.RandomDownloadFrom;
			_mainForm.RandomDownloadTo.Text = myAppSettings.RandomDownloadTo;
			_mainForm.fileSize.Text = GetValueDef(myAppSettings.finishedPercent, "100");
			_mainForm.updateAnnounceParamsOnStart.Checked = myAppSettings.updateAnnounceParamsOnStart;
			_mainForm.textStopMinLeecher.Value = myAppSettings.textStopMinLeecher;
			_mainForm.checkIgnoreFailureReason.Checked = myAppSettings.ignoreFailureReason;
			_mainForm.stopProcessActionBox.SelectedIndex = myAppSettings.stopProcessActionBox;
			_mainForm.stopProcessUnitsBox.SelectedIndex = myAppSettings.stopProcessUnitsBox;
			_mainForm.stopProcessValue.Text = myAppSettings.stopProcessValue;
			_mainForm.checkMinimizeToTray.Checked = myAppSettings.minimizeTotray;
			_mainForm.checkIgnoreTimeout.Checked = myAppSettings.ignoreTimeout;
			_mainForm.checkUPnP.Checked = myAppSettings.useUPnP;
			_mainForm.checkSavePeers.Checked = myAppSettings.savePeerList;
			string text = string.IsNullOrEmpty(myAppSettings.interfaceTheme) ? "Dark" : myAppSettings.interfaceTheme;
			_mainForm.cbbInterfaceTheme.SelectedItem = string.Equals(text, "Light", StringComparison.OrdinalIgnoreCase) ? "Light" : "Dark";
			_mainForm.setSelectedLanguage(myAppSettings.selectedLanguage);
		}
		catch (Exception ex)
		{
			_mainForm.AddLogLine("Error loading config: " + ex.Message);
		}
	}
}
