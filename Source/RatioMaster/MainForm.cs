using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using BitTorrent;
using NatTraversal;
using RJH.CommandLineHelper;
using Starksoft.Net.Proxy;

namespace RatioMaster;

public class MainForm : Form
{
	private const int TrackerSocketTimeoutMs = 15000;

	public class KeyValuePair
	{
		public object Key;

		public string Value;

		public KeyValuePair(object NewValue, string NewDescription)
		{
			Key = NewValue;
			Value = NewDescription;
		}

		public override string ToString()
		{
			return Value;
		}
	}

	private delegate void stopTimerAndCountersCallback();

	private delegate void SetCountersCallback(TorrentInfo torrentInfo);

	private delegate void updateTextBoxCallback(TextBox textbox, string text);

	private delegate void updateLabelCallback(Label textbox, string text);

	private delegate void ShowMessageCallback(string message, string title);

	private delegate void SetTextCallback(string logLine);

	public enum SizeUnits
	{
		KB,
		MB,
		GB
	}

	public enum TimeUnits
	{
		Seconds,
		Minutes,
		Hours
	}

	private IContainer components;

	public System.Windows.Forms.Timer serverUpdateTimer;

	private OpenFileDialog openFileDialog1;

	public Button StopButton;

	public Button StartButton;

	public Button closeButton;

	private TextBox textBox1;

	private Label label11;

	private TextBox textBox2;

	private Label label12;

	private Label label13;

	private Label label14;

	private TextBox textBox3;

	public Torrent currentTorrentFile;

	private ContextMenuStrip menuRightClickTray;

	private ToolStripMenuItem restoreToolStripMenuItem;

	private ToolStripSeparator toolStripSeparator1;

	private ToolStripMenuItem exitToolStripMenuItem;

	public Icon iconForTray;

	public TcpListener localListen;

	public TorrentInfo currentTorrent;

	public TorrentClient currentClient;

	public ProxyInfo currentProxy;

	private TorrentClientsEnum TorrentClientsObj;

	private NotifyIcon trayIcon;

	private TorrentClient[] TorrentClients;

	public TabPage tabAbout;

	private Label label10;

	private Label labelForums;

	private LinkLabel linkForums;

	private Label labelWebSite;

	private LinkLabel linkWebsite;

	private LinkLabel linkEmail1;

	private Label label7;

	private Label versionAboutLabel;

	private Label label2;

	public TabPage tabLog;

	public CheckBox checkLogEnabled;

	public Button clearLogButton;

	private RichTextBox logWindow;

	public TabPage tabNetwork;

	public GroupBox groupNetworkMisc;

	public TextBox interval;

	public Label intervalLabel;

	public CheckBox checkRequestScrap;

	public CheckBox checkTCPListen;

	public GroupBox proxySettingsGroup;

	public TextBox textProxyPass;

	public TextBox textProxyUser;

	public Label labelProxyPass;

	public Label labelProxyUser;

	public TextBox textProxyPort;

	public TextBox textProxyHost;

	public Label labelProxyPort;

	public Label labelProxyHost;

	public ComboBox comboProxyType;

	public Label labelProxyType;

	public TabPage tabAdvanced;

	public CheckBox checkShowTrayBaloon;

	public ComboBox TorrentClientsBox;

	public Label ClientLabel;

	public GroupBox randomSpeedGroup;

	public TextBox RandomDownloadTo;

	public TextBox RandomUploadTo;

	public TextBox RandomDownloadFrom;

	public TextBox RandomUploadFrom;

	public Label RandomDownloadToLabel;

	public Label RandomUploadToLabel;

	public Label RandomDownloadFromLabel;

	public Label RandomUploadFromLabel;

	public CheckBox checkRandomDownload;

	public CheckBox checkRandomUpload;

	public TextBox customPeerID;

	public Label labelPeerID;

	public GroupBox reportParamsGroup;

	public TextBox customPeersNum;

	public Label numwantLabel;

	public TextBox customKey;

	public Label keyLabel;

	public Label portInfoLabel;

	public Label portLabel;

	public TextBox customPort;

	public TabPage tabGeneral;

	public GroupBox groupTorrentFile;

	public Label TorrentFileLabel;

	public Button browseButton;

	public GroupBox groupStats;

	private Label totalRunningTime;

	public Label totalRunningTimeLabel;

	private Label downloadCount;

	private Label uploadCount;

	public Label leechLabel;

	public Label seedLabel;

	public Button manualUpdateButton;

	private Label timerValue;

	public Label labelUpdateIn;

	public Label downloadCountLabel;

	public Label uploadCountLabel;

	public GroupBox groupBoxOptions;

	public ComboBox stopProcessUnitsBox;

	public ComboBox stopProcessActionBox;

	public TextBox stopProcessValue;

	public Label stopProcessLabel;

	public TextBox fileSize;

	public Label FileSizeLabel;

	public TextBox uploadRate;

	public TextBox downloadRate;

	public Label uploadRateLabel;

	public Label downloadRateLabel;

	private Label speedWarningLabel;

	public GroupBox groupTorrentInfo;

	private Label torrentSize;

	public Label labelTorrentSize;

	public TextBox shaHash;

	public Label hashLabel;

	private TextBox trackerAddress;

	public Label TrackerLabel;

	public TabControl tabControl1;

	public Button saveLogButton;

	public CheckBox updateAnnounceParamsOnStart;

	public Button memoryReaderButton;

	public Label labelStopMinLeecher;

	public NumericUpDown textStopMinLeecher;

	public CheckBox checkIgnoreFailureReason;

	public Button applyStopSettingsButton;

	private ToolTip toolTip1;

	private ApplicationSettings applicationSettings;

	public ComboBox cbbLanguages;

	private Random random = new Random((int)DateTime.Now.Ticks);

	public Label lblLanguage;

	public Label lblInterface;

	public ComboBox cbbInterfaceTheme;

	public LocalizationManager lclzManager;

	public Label labelBindIp;

	public ComboBox comboBindIp;

	public CheckBox checkMinimizeToTray;

	public CheckBox checkIgnoreTimeout;

	public CheckBox checkUPnP;

	public Button testNetworkButton;

	public Button ResetCountersButton;

	public UPnPNat upnPNat;

	public ComboBox torrentFile;

	public CheckBox checkSavePeers;

	public bool upnPNatEnabled;

	public bool torrentLoadedSuccessfully;

	private string m_UploadRateCM = "";

	private string m_DownloadRateCM = "";

	private string m_PercentFinishedCM = "";

	private bool updateProcessStarted;

	private bool haveInitialPeers;

	private PortMappingInfo TcpListenerMappingInfo;

	public string ipAddress = "";

	public bool UseLocalIpBinding;

	public int temporaryIntervalCounter;

	public int totalRunningTimeCounter;

	private bool seedMode;

	private bool scrapStatsUpdated;

	private bool trayIconBaloonIsUp;

	private bool stopParamsUpdateInProgress;

	private bool TestNetworkInProgress;

	private FormStartPosition initialStartPosition = FormStartPosition.CenterScreen;

	[CommandLineSwitch("uploadrate", "Upload Rate")]
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string UploadRateCM
	{
		get
		{
			return m_UploadRateCM;
		}
		set
		{
			m_UploadRateCM = value;
		}
	}

	[CommandLineSwitch("downloadrate", "Download Rate")]
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string DownloadRateCM
	{
		get
		{
			return m_DownloadRateCM;
		}
		set
		{
			m_DownloadRateCM = value;
		}
	}

	[CommandLineSwitch("percent", "Finished Percent")]
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string PercentFinishedCM
	{
		get
		{
			return m_PercentFinishedCM;
		}
		set
		{
			m_PercentFinishedCM = value;
		}
	}

	public MainForm()
	{
		Opacity = 0.0;
		SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
		BackColor = DarkTheme.WindowColor;
		ForeColor = DarkTheme.TextColor;
		InitializeComponent();
		Text = "RatioMaster 2.0";
		cbbInterfaceTheme.SelectedItem = DarkTheme.CurrentThemeName;
		DarkTheme.Apply(this);
		DarkTheme.Apply(menuRightClickTray);
		DarkTheme.Apply(toolTip1);
		LayoutAboutTab();
		initialStartPosition = StartPosition;
		StartPosition = FormStartPosition.Manual;
		Location = new Point(-32000, -32000);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RatioMaster.MainForm));
		this.serverUpdateTimer = new System.Windows.Forms.Timer(this.components);
		this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
		this.StopButton = new RatioMaster.DarkButton();
		this.StartButton = new RatioMaster.DarkButton();
		this.closeButton = new RatioMaster.DarkButton();
		this.textBox1 = new System.Windows.Forms.TextBox();
		this.label11 = new System.Windows.Forms.Label();
		this.textBox2 = new System.Windows.Forms.TextBox();
		this.label12 = new System.Windows.Forms.Label();
		this.label13 = new System.Windows.Forms.Label();
		this.label14 = new System.Windows.Forms.Label();
		this.textBox3 = new System.Windows.Forms.TextBox();
		this.menuRightClickTray = new System.Windows.Forms.ContextMenuStrip(this.components);
		this.restoreToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
		this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.trayIcon = new System.Windows.Forms.NotifyIcon(this.components);
		this.tabAbout = new System.Windows.Forms.TabPage();
		this.label10 = new System.Windows.Forms.Label();
		this.labelForums = new System.Windows.Forms.Label();
		this.linkForums = new System.Windows.Forms.LinkLabel();
		this.labelWebSite = new System.Windows.Forms.Label();
		this.linkWebsite = new System.Windows.Forms.LinkLabel();
		this.linkEmail1 = new System.Windows.Forms.LinkLabel();
		this.label7 = new System.Windows.Forms.Label();
		this.versionAboutLabel = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.tabLog = new System.Windows.Forms.TabPage();
		this.saveLogButton = new RatioMaster.DarkButton();
		this.checkLogEnabled = new RatioMaster.DarkCheckBox();
		this.clearLogButton = new RatioMaster.DarkButton();
		this.logWindow = new RatioMaster.DarkRichTextBox();
		this.tabNetwork = new System.Windows.Forms.TabPage();
		this.testNetworkButton = new RatioMaster.DarkButton();
		this.groupNetworkMisc = new System.Windows.Forms.GroupBox();
		this.checkSavePeers = new RatioMaster.DarkCheckBox();
		this.checkUPnP = new RatioMaster.DarkCheckBox();
		this.checkIgnoreTimeout = new RatioMaster.DarkCheckBox();
		this.labelBindIp = new System.Windows.Forms.Label();
		this.comboBindIp = new RatioMaster.DarkComboBox();
		this.checkIgnoreFailureReason = new RatioMaster.DarkCheckBox();
		this.textStopMinLeecher = new RatioMaster.DarkNumericUpDown();
		this.labelStopMinLeecher = new System.Windows.Forms.Label();
		this.interval = new System.Windows.Forms.TextBox();
		this.intervalLabel = new System.Windows.Forms.Label();
		this.checkRequestScrap = new RatioMaster.DarkCheckBox();
		this.checkTCPListen = new RatioMaster.DarkCheckBox();
		this.proxySettingsGroup = new System.Windows.Forms.GroupBox();
		this.textProxyPass = new System.Windows.Forms.TextBox();
		this.textProxyUser = new System.Windows.Forms.TextBox();
		this.labelProxyPass = new System.Windows.Forms.Label();
		this.labelProxyUser = new System.Windows.Forms.Label();
		this.textProxyPort = new System.Windows.Forms.TextBox();
		this.textProxyHost = new System.Windows.Forms.TextBox();
		this.labelProxyPort = new System.Windows.Forms.Label();
		this.labelProxyHost = new System.Windows.Forms.Label();
		this.comboProxyType = new RatioMaster.DarkComboBox();
		this.labelProxyType = new System.Windows.Forms.Label();
		this.tabAdvanced = new System.Windows.Forms.TabPage();
		this.checkMinimizeToTray = new RatioMaster.DarkCheckBox();
		this.memoryReaderButton = new RatioMaster.DarkButton();
		this.updateAnnounceParamsOnStart = new RatioMaster.DarkCheckBox();
		this.checkShowTrayBaloon = new RatioMaster.DarkCheckBox();
		this.TorrentClientsBox = new RatioMaster.DarkComboBox();
		this.ClientLabel = new System.Windows.Forms.Label();
		this.randomSpeedGroup = new System.Windows.Forms.GroupBox();
		this.RandomDownloadTo = new System.Windows.Forms.TextBox();
		this.RandomUploadTo = new System.Windows.Forms.TextBox();
		this.RandomDownloadFrom = new System.Windows.Forms.TextBox();
		this.RandomUploadFrom = new System.Windows.Forms.TextBox();
		this.RandomDownloadToLabel = new System.Windows.Forms.Label();
		this.RandomUploadToLabel = new System.Windows.Forms.Label();
		this.RandomDownloadFromLabel = new System.Windows.Forms.Label();
		this.RandomUploadFromLabel = new System.Windows.Forms.Label();
		this.checkRandomDownload = new RatioMaster.DarkCheckBox();
		this.checkRandomUpload = new RatioMaster.DarkCheckBox();
		this.reportParamsGroup = new System.Windows.Forms.GroupBox();
		this.customKey = new System.Windows.Forms.TextBox();
		this.customPeerID = new System.Windows.Forms.TextBox();
		this.keyLabel = new System.Windows.Forms.Label();
		this.customPeersNum = new System.Windows.Forms.TextBox();
		this.labelPeerID = new System.Windows.Forms.Label();
		this.numwantLabel = new System.Windows.Forms.Label();
		this.portInfoLabel = new System.Windows.Forms.Label();
		this.portLabel = new System.Windows.Forms.Label();
		this.customPort = new System.Windows.Forms.TextBox();
		this.cbbLanguages = new RatioMaster.DarkComboBox();
		this.tabGeneral = new System.Windows.Forms.TabPage();
		this.groupTorrentFile = new System.Windows.Forms.GroupBox();
		this.torrentFile = new RatioMaster.DarkComboBox();
		this.TorrentFileLabel = new System.Windows.Forms.Label();
		this.browseButton = new RatioMaster.DarkButton();
		this.groupStats = new System.Windows.Forms.GroupBox();
		this.totalRunningTime = new System.Windows.Forms.Label();
		this.totalRunningTimeLabel = new System.Windows.Forms.Label();
		this.downloadCount = new System.Windows.Forms.Label();
		this.uploadCount = new System.Windows.Forms.Label();
		this.leechLabel = new System.Windows.Forms.Label();
		this.seedLabel = new System.Windows.Forms.Label();
		this.manualUpdateButton = new RatioMaster.DarkButton();
		this.timerValue = new System.Windows.Forms.Label();
		this.labelUpdateIn = new System.Windows.Forms.Label();
		this.downloadCountLabel = new System.Windows.Forms.Label();
		this.uploadCountLabel = new System.Windows.Forms.Label();
		this.groupBoxOptions = new System.Windows.Forms.GroupBox();
		this.applyStopSettingsButton = new RatioMaster.DarkButton();
		this.stopProcessUnitsBox = new RatioMaster.DarkComboBox();
		this.stopProcessActionBox = new RatioMaster.DarkComboBox();
		this.stopProcessValue = new System.Windows.Forms.TextBox();
		this.stopProcessLabel = new System.Windows.Forms.Label();
		this.fileSize = new System.Windows.Forms.TextBox();
		this.FileSizeLabel = new System.Windows.Forms.Label();
		this.uploadRate = new System.Windows.Forms.TextBox();
		this.downloadRate = new System.Windows.Forms.TextBox();
		this.uploadRateLabel = new System.Windows.Forms.Label();
		this.downloadRateLabel = new System.Windows.Forms.Label();
		this.speedWarningLabel = new System.Windows.Forms.Label();
		this.groupTorrentInfo = new System.Windows.Forms.GroupBox();
		this.torrentSize = new System.Windows.Forms.Label();
		this.labelTorrentSize = new System.Windows.Forms.Label();
		this.shaHash = new System.Windows.Forms.TextBox();
		this.hashLabel = new System.Windows.Forms.Label();
		this.trackerAddress = new System.Windows.Forms.TextBox();
		this.TrackerLabel = new System.Windows.Forms.Label();
		this.tabControl1 = new RatioMaster.DarkTabControl();
		this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
		this.lblLanguage = new System.Windows.Forms.Label();
		this.lblInterface = new System.Windows.Forms.Label();
		this.cbbInterfaceTheme = new RatioMaster.DarkComboBox();
		this.ResetCountersButton = new RatioMaster.DarkButton();
		this.menuRightClickTray.SuspendLayout();
		this.tabAbout.SuspendLayout();
		this.tabLog.SuspendLayout();
		this.tabNetwork.SuspendLayout();
		this.groupNetworkMisc.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.textStopMinLeecher).BeginInit();
		this.proxySettingsGroup.SuspendLayout();
		this.tabAdvanced.SuspendLayout();
		this.randomSpeedGroup.SuspendLayout();
		this.reportParamsGroup.SuspendLayout();
		this.tabGeneral.SuspendLayout();
		this.groupTorrentFile.SuspendLayout();
		this.groupStats.SuspendLayout();
		this.groupBoxOptions.SuspendLayout();
		this.groupTorrentInfo.SuspendLayout();
		this.tabControl1.SuspendLayout();
		base.SuspendLayout();
		this.serverUpdateTimer.Interval = 1000;
		this.serverUpdateTimer.Tick += new System.EventHandler(serverUpdateTimer_Tick);
		this.openFileDialog1.Filter = "Torrent Files|*.torrent";
		this.openFileDialog1.Title = "Open torrent files";
		this.openFileDialog1.AutoUpgradeEnabled = true;
		this.openFileDialog1.CheckFileExists = true;
		this.openFileDialog1.RestoreDirectory = true;
		this.openFileDialog1.ShowReadOnly = false;
		this.openFileDialog1.SupportMultiDottedExtensions = true;
		this.StopButton.Enabled = false;
		this.StopButton.Location = new System.Drawing.Point(378, 424);
		this.StopButton.Name = "StopButton";
		this.StopButton.Size = new System.Drawing.Size(101, 23);
		this.StopButton.TabIndex = 12;
		this.StopButton.Text = "Stop";
		this.StopButton.Click += new System.EventHandler(StopButton_Click);
		this.StartButton.Location = new System.Drawing.Point(271, 424);
		this.StartButton.Name = "StartButton";
		this.StartButton.Size = new System.Drawing.Size(101, 23);
		this.StartButton.TabIndex = 22;
		this.StartButton.Text = "Start";
		this.StartButton.Click += new System.EventHandler(StartButton_Click);
		this.closeButton.Location = new System.Drawing.Point(526, 424);
		this.closeButton.Name = "closeButton";
		this.closeButton.Size = new System.Drawing.Size(90, 23);
		this.closeButton.TabIndex = 23;
		this.closeButton.Text = "Close";
		this.closeButton.Click += new System.EventHandler(closeButton_Click);
		this.textBox1.Location = new System.Drawing.Point(377, 39);
		this.textBox1.Name = "textBox1";
		this.textBox1.Size = new System.Drawing.Size(43, 20);
		this.textBox1.TabIndex = 7;
		this.label11.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label11.Location = new System.Drawing.Point(273, 44);
		this.label11.Name = "label11";
		this.label11.Size = new System.Drawing.Size(98, 13);
		this.label11.TabIndex = 6;
		this.label11.Text = "Number of Peers:";
		this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.textBox2.Location = new System.Drawing.Point(208, 39);
		this.textBox2.Name = "textBox2";
		this.textBox2.Size = new System.Drawing.Size(41, 20);
		this.textBox2.TabIndex = 4;
		this.label12.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label12.Location = new System.Drawing.Point(143, 44);
		this.label12.Name = "label12";
		this.label12.Size = new System.Drawing.Size(59, 13);
		this.label12.TabIndex = 3;
		this.label12.Text = "Key:";
		this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label13.AutoSize = true;
		this.label13.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label13.Location = new System.Drawing.Point(6, 16);
		this.label13.Name = "label13";
		this.label13.Size = new System.Drawing.Size(354, 13);
		this.label13.TabIndex = 2;
		this.label13.Text = "If those fields are left empty, RatioMaster will use random or default values";
		this.label14.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label14.Location = new System.Drawing.Point(6, 40);
		this.label14.Name = "label14";
		this.label14.Size = new System.Drawing.Size(56, 17);
		this.label14.TabIndex = 1;
		this.label14.Text = "Port:";
		this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.textBox3.Location = new System.Drawing.Point(68, 39);
		this.textBox3.Name = "textBox3";
		this.textBox3.Size = new System.Drawing.Size(44, 20);
		this.textBox3.TabIndex = 0;
		this.menuRightClickTray.Items.AddRange(new System.Windows.Forms.ToolStripItem[3] { this.restoreToolStripMenuItem, this.toolStripSeparator1, this.exitToolStripMenuItem });
		this.menuRightClickTray.Name = "menuRightClickTray";
		this.menuRightClickTray.Size = new System.Drawing.Size(113, 54);
		this.restoreToolStripMenuItem.Name = "restoreToolStripMenuItem";
		this.restoreToolStripMenuItem.Size = new System.Drawing.Size(112, 22);
		this.restoreToolStripMenuItem.Text = "Restore";
		this.restoreToolStripMenuItem.Click += new System.EventHandler(restoreToolStripMenuItem_Click);
		this.toolStripSeparator1.Name = "toolStripSeparator1";
		this.toolStripSeparator1.Size = new System.Drawing.Size(109, 6);
		this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
		this.exitToolStripMenuItem.Size = new System.Drawing.Size(112, 22);
		this.exitToolStripMenuItem.Text = "Exit";
		this.exitToolStripMenuItem.Click += new System.EventHandler(exitToolStripMenuItem_Click);
		this.trayIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
		this.trayIcon.BalloonTipTitle = "RatioMaster 2.0";
		this.trayIcon.ContextMenuStrip = this.menuRightClickTray;
		this.trayIcon.Icon = (System.Drawing.Icon)resources.GetObject("trayIcon.Icon");
		this.trayIcon.BalloonTipClosed += new System.EventHandler(trayIcon_BalloonTipClosed);
		this.trayIcon.BalloonTipClicked += new System.EventHandler(trayIcon_BalloonTipClicked);
		this.trayIcon.MouseMove += new System.Windows.Forms.MouseEventHandler(trayIcon_MouseMove);
		this.trayIcon.BalloonTipShown += new System.EventHandler(trayIcon_BalloonTipShown);
		this.trayIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(trayIcon_MouseClick);
		this.tabAbout.Controls.Add(this.label10);
		this.tabAbout.Controls.Add(this.labelForums);
		this.tabAbout.Controls.Add(this.linkForums);
		this.tabAbout.Controls.Add(this.labelWebSite);
		this.tabAbout.Controls.Add(this.linkWebsite);
		this.tabAbout.Controls.Add(this.linkEmail1);
		this.tabAbout.Controls.Add(this.label7);
		this.tabAbout.Controls.Add(this.versionAboutLabel);
		this.tabAbout.Controls.Add(this.label2);
		this.tabAbout.Location = new System.Drawing.Point(4, 22);
		this.tabAbout.Name = "tabAbout";
		this.tabAbout.Size = new System.Drawing.Size(597, 348);
		this.tabAbout.TabIndex = 3;
		this.tabAbout.Text = "About";
		this.tabAbout.UseVisualStyleBackColor = true;
		this.tabAbout.Controls.Clear();
		this.tabAbout.Controls.Add(this.label10);
		this.tabAbout.Controls.Add(this.labelForums);
		this.tabAbout.Controls.Add(this.linkForums);
		this.tabAbout.Controls.Add(this.labelWebSite);
		this.tabAbout.Controls.Add(this.linkWebsite);
		this.tabAbout.Controls.Add(this.versionAboutLabel);
		this.tabAbout.Controls.Add(this.label2);
		this.label10.AutoSize = false;
		this.label10.Font = new System.Drawing.Font("Segoe UI", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label10.Location = new System.Drawing.Point(548, 224);
		this.label10.Name = "label10";
		this.label10.Size = new System.Drawing.Size(20, 20);
		this.label10.TabIndex = 8;
		this.label10.Text = "";
		this.labelForums.AutoSize = true;
		this.labelForums.Font = new System.Drawing.Font("Segoe UI", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.labelForums.Location = new System.Drawing.Point(142, 224);
		this.labelForums.Name = "labelForums";
		this.labelForums.TabIndex = 7;
		this.labelForums.Text = "Contribute or report any bugs on";
		this.labelForums.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.linkForums.AutoSize = true;
		this.linkForums.Font = new System.Drawing.Font("Segoe UI", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.linkForums.Location = new System.Drawing.Point(400, 224);
		this.linkForums.Name = "linkForums";
		this.linkForums.TabIndex = 6;
		this.linkForums.TabStop = true;
		this.linkForums.Text = "GitHub";
		this.linkForums.Tag = "https://github.com/BagerRyg/RatioMaster2";
		this.linkForums.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(linkForums_LinkClicked);
		this.labelWebSite.AutoSize = true;
		this.labelWebSite.Font = new System.Drawing.Font("Segoe UI", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.labelWebSite.Location = new System.Drawing.Point(75, 197);
		this.labelWebSite.Name = "labelWebSite";
		this.labelWebSite.TabIndex = 5;
		this.labelWebSite.Text = "Modernized and improved by RygTech -";
		this.labelWebSite.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.linkWebsite.AutoSize = true;
		this.linkWebsite.Font = new System.Drawing.Font("Segoe UI", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.linkWebsite.Location = new System.Drawing.Point(400, 197);
		this.linkWebsite.Name = "linkWebsite";
		this.linkWebsite.TabIndex = 4;
		this.linkWebsite.TabStop = true;
		this.linkWebsite.Text = "https://rygtech.org";
		this.linkWebsite.Tag = "https://rygtech.org";
		this.linkWebsite.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(linkWebsiteClicked);
		this.linkEmail1.AutoSize = true;
		this.linkEmail1.Location = new System.Drawing.Point(356, 57);
		this.linkEmail1.Name = "linkEmail1";
		this.linkEmail1.Size = new System.Drawing.Size(139, 13);
		this.linkEmail1.TabIndex = 3;
		this.linkEmail1.TabStop = true;
		this.linkEmail1.Text = "";
		this.linkEmail1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(linkEmail1_LinkClicked);
		this.label7.AutoSize = true;
		this.label7.Location = new System.Drawing.Point(202, 57);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(139, 13);
		this.label7.TabIndex = 2;
		this.label7.Text = "";
		this.versionAboutLabel.AutoSize = false;
		this.versionAboutLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.versionAboutLabel.Font = new System.Drawing.Font("Segoe UI", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.versionAboutLabel.Location = new System.Drawing.Point(40, 171);
		this.versionAboutLabel.Name = "versionAboutLabel";
		this.versionAboutLabel.Size = new System.Drawing.Size(520, 25);
		this.versionAboutLabel.TabIndex = 1;
		this.versionAboutLabel.Text = "Build 66 using .NET 10.0";
		this.versionAboutLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label2.AutoSize = false;
		this.label2.BackColor = System.Drawing.Color.Transparent;
		this.label2.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.label2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.label2.Font = new System.Drawing.Font("Segoe UI", 26.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label2.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label2.Location = new System.Drawing.Point(99, 112);
		this.label2.Name = "label2";
		this.label2.Padding = new System.Windows.Forms.Padding(0);
		this.label2.Size = new System.Drawing.Size(400, 56);
		this.label2.TabIndex = 0;
		this.label2.Text = "RatioMaster 2.0";
		this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.tabLog.Controls.Add(this.saveLogButton);
		this.tabLog.Controls.Add(this.checkLogEnabled);
		this.tabLog.Controls.Add(this.clearLogButton);
		this.tabLog.Controls.Add(this.logWindow);
		this.tabLog.Location = new System.Drawing.Point(4, 22);
		this.tabLog.Name = "tabLog";
		this.tabLog.Padding = new System.Windows.Forms.Padding(3);
		this.tabLog.Size = new System.Drawing.Size(597, 348);
		this.tabLog.TabIndex = 1;
		this.tabLog.Text = "Logging";
		this.tabLog.UseVisualStyleBackColor = true;
		this.saveLogButton.Location = new System.Drawing.Point(346, 317);
		this.saveLogButton.Name = "saveLogButton";
		this.saveLogButton.Size = new System.Drawing.Size(116, 23);
		this.saveLogButton.TabIndex = 16;
		this.saveLogButton.Text = "Save Log";
		this.saveLogButton.Click += new System.EventHandler(saveLogButton_Click);
		this.checkLogEnabled.Location = new System.Drawing.Point(6, 317);
		this.checkLogEnabled.Name = "checkLogEnabled";
		this.checkLogEnabled.Size = new System.Drawing.Size(184, 17);
		this.checkLogEnabled.TabIndex = 15;
		this.checkLogEnabled.Text = "Enable full logging";
		this.checkLogEnabled.UseVisualStyleBackColor = true;
		this.clearLogButton.Location = new System.Drawing.Point(480, 317);
		this.clearLogButton.Name = "clearLogButton";
		this.clearLogButton.Size = new System.Drawing.Size(109, 23);
		this.clearLogButton.TabIndex = 14;
		this.clearLogButton.Text = "Clear Log";
		this.clearLogButton.Click += new System.EventHandler(clearLogButton_Click);
		this.logWindow.Location = new System.Drawing.Point(6, 6);
		this.logWindow.Name = "logWindow";
		this.logWindow.ReadOnly = true;
		this.logWindow.Size = new System.Drawing.Size(585, 307);
		this.logWindow.TabIndex = 13;
		this.logWindow.Text = "";
		this.logWindow.WordWrap = false;
		this.tabNetwork.Controls.Add(this.testNetworkButton);
		this.tabNetwork.Controls.Add(this.groupNetworkMisc);
		this.tabNetwork.Controls.Add(this.proxySettingsGroup);
		this.tabNetwork.Location = new System.Drawing.Point(4, 22);
		this.tabNetwork.Name = "tabNetwork";
		this.tabNetwork.Padding = new System.Windows.Forms.Padding(3);
		this.tabNetwork.Size = new System.Drawing.Size(597, 348);
		this.tabNetwork.TabIndex = 4;
		this.tabNetwork.Text = "Network";
		this.tabNetwork.UseVisualStyleBackColor = true;
		this.testNetworkButton.ForeColor = System.Drawing.SystemColors.ControlText;
		this.testNetworkButton.Location = new System.Drawing.Point(437, 319);
		this.testNetworkButton.Name = "testNetworkButton";
		this.testNetworkButton.Size = new System.Drawing.Size(148, 23);
		this.testNetworkButton.TabIndex = 30;
		this.testNetworkButton.Text = "Test Settings...";
		this.testNetworkButton.Click += new System.EventHandler(testNetworkButton_Click);
		this.groupNetworkMisc.Controls.Add(this.checkSavePeers);
		this.groupNetworkMisc.Controls.Add(this.checkUPnP);
		this.groupNetworkMisc.Controls.Add(this.checkIgnoreTimeout);
		this.groupNetworkMisc.Controls.Add(this.labelBindIp);
		this.groupNetworkMisc.Controls.Add(this.comboBindIp);
		this.groupNetworkMisc.Controls.Add(this.checkIgnoreFailureReason);
		this.groupNetworkMisc.Controls.Add(this.textStopMinLeecher);
		this.groupNetworkMisc.Controls.Add(this.labelStopMinLeecher);
		this.groupNetworkMisc.Controls.Add(this.interval);
		this.groupNetworkMisc.Controls.Add(this.intervalLabel);
		this.groupNetworkMisc.Controls.Add(this.checkRequestScrap);
		this.groupNetworkMisc.Controls.Add(this.checkTCPListen);
		this.groupNetworkMisc.ForeColor = System.Drawing.SystemColors.Desktop;
		this.groupNetworkMisc.Location = new System.Drawing.Point(7, 117);
		this.groupNetworkMisc.Name = "groupNetworkMisc";
		this.groupNetworkMisc.Size = new System.Drawing.Size(584, 196);
		this.groupNetworkMisc.TabIndex = 4;
		this.groupNetworkMisc.TabStop = false;
		this.groupNetworkMisc.Text = "Miscellaneous";
		this.checkSavePeers.ForeColor = System.Drawing.SystemColors.ControlText;
		this.checkSavePeers.Location = new System.Drawing.Point(379, 17);
		this.checkSavePeers.Name = "checkSavePeers";
		this.checkSavePeers.Size = new System.Drawing.Size(205, 17);
		this.checkSavePeers.TabIndex = 30;
		this.checkSavePeers.Text = "Save list of peers";
		this.checkSavePeers.UseVisualStyleBackColor = true;
		this.checkUPnP.ForeColor = System.Drawing.SystemColors.ControlText;
		this.checkUPnP.Location = new System.Drawing.Point(379, 41);
		this.checkUPnP.Name = "checkUPnP";
		this.checkUPnP.Size = new System.Drawing.Size(205, 17);
		this.checkUPnP.TabIndex = 29;
		this.checkUPnP.Text = "Enable UPnP port mapping";
		this.checkUPnP.UseVisualStyleBackColor = true;
		this.checkIgnoreTimeout.ForeColor = System.Drawing.SystemColors.ControlText;
		this.checkIgnoreTimeout.Location = new System.Drawing.Point(8, 136);
		this.checkIgnoreTimeout.Name = "checkIgnoreTimeout";
		this.checkIgnoreTimeout.Size = new System.Drawing.Size(375, 17);
		this.checkIgnoreTimeout.TabIndex = 28;
		this.checkIgnoreTimeout.Text = "Ignore connection errors";
		this.toolTip1.SetToolTip(this.checkIgnoreTimeout, "RM will ignore connection errors");
		this.checkIgnoreTimeout.UseVisualStyleBackColor = true;
		this.labelBindIp.ForeColor = System.Drawing.SystemColors.ControlText;
		this.labelBindIp.Location = new System.Drawing.Point(8, 165);
		this.labelBindIp.Name = "labelBindIp";
		this.labelBindIp.Size = new System.Drawing.Size(150, 13);
		this.labelBindIp.TabIndex = 27;
		this.labelBindIp.Text = "Bind to IP:";
		this.labelBindIp.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.comboBindIp.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboBindIp.FormattingEnabled = true;
		this.comboBindIp.Items.AddRange(new object[3] { "None", "SOCKS4", "SOCKS5" });
		this.comboBindIp.Location = new System.Drawing.Point(162, 162);
		this.comboBindIp.Name = "comboBindIp";
		this.comboBindIp.Size = new System.Drawing.Size(152, 21);
		this.comboBindIp.TabIndex = 26;
		this.comboBindIp.SelectedIndexChanged += new System.EventHandler(comboBindIp_SelectedIndexChanged);
		this.checkIgnoreFailureReason.ForeColor = System.Drawing.SystemColors.ControlText;
		this.checkIgnoreFailureReason.Location = new System.Drawing.Point(8, 113);
		this.checkIgnoreFailureReason.Name = "checkIgnoreFailureReason";
		this.checkIgnoreFailureReason.Size = new System.Drawing.Size(375, 17);
		this.checkIgnoreFailureReason.TabIndex = 25;
		this.checkIgnoreFailureReason.Text = "Ignore 'failure reason' tracker response";
		this.toolTip1.SetToolTip(this.checkIgnoreFailureReason, "RM will ignore errors returning from the tracker and will upload as ussual when this options is checked.");
		this.checkIgnoreFailureReason.UseVisualStyleBackColor = true;
		this.textStopMinLeecher.Location = new System.Drawing.Point(334, 87);
		this.textStopMinLeecher.Name = "textStopMinLeecher";
		this.textStopMinLeecher.Size = new System.Drawing.Size(39, 20);
		this.textStopMinLeecher.TabIndex = 24;
		this.textStopMinLeecher.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.labelStopMinLeecher.ForeColor = System.Drawing.SystemColors.ControlText;
		this.labelStopMinLeecher.Location = new System.Drawing.Point(8, 89);
		this.labelStopMinLeecher.Name = "labelStopMinLeecher";
		this.labelStopMinLeecher.Size = new System.Drawing.Size(311, 13);
		this.labelStopMinLeecher.TabIndex = 22;
		this.labelStopMinLeecher.Text = "Stop uploading when number of leechers is less than:";
		this.labelStopMinLeecher.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.toolTip1.SetToolTip(this.labelStopMinLeecher, "RM can automatically zero upload speed,if number of leechers is less than choosen value.");
		this.interval.Location = new System.Drawing.Point(205, 15);
		this.interval.Name = "interval";
		this.interval.Size = new System.Drawing.Size(38, 20);
		this.interval.TabIndex = 21;
		this.interval.Text = "1800";
		this.interval.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.intervalLabel.ForeColor = System.Drawing.SystemColors.ControlText;
		this.intervalLabel.Location = new System.Drawing.Point(5, 13);
		this.intervalLabel.Name = "intervalLabel";
		this.intervalLabel.Size = new System.Drawing.Size(194, 23);
		this.intervalLabel.TabIndex = 19;
		this.intervalLabel.Text = "Tracker Update Interval (s):";
		this.intervalLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.toolTip1.SetToolTip(this.intervalLabel, "Default Update Interval for sending stats to the tracker.\r\nUsefull for those trackers that do no return Update Interval.");
		this.checkRequestScrap.ForeColor = System.Drawing.SystemColors.ControlText;
		this.checkRequestScrap.Location = new System.Drawing.Point(8, 64);
		this.checkRequestScrap.Name = "checkRequestScrap";
		this.checkRequestScrap.Size = new System.Drawing.Size(375, 17);
		this.checkRequestScrap.TabIndex = 9;
		this.checkRequestScrap.Text = "Get Seeders/Leechers stats (scrape info from tracker)";
		this.checkRequestScrap.UseVisualStyleBackColor = true;
		this.checkTCPListen.ForeColor = System.Drawing.SystemColors.ControlText;
		this.checkTCPListen.Location = new System.Drawing.Point(8, 41);
		this.checkTCPListen.Name = "checkTCPListen";
		this.checkTCPListen.Size = new System.Drawing.Size(365, 17);
		this.checkTCPListen.TabIndex = 0;
		this.checkTCPListen.Text = "Use TCP listener (appear connectable on tracker)";
		this.checkTCPListen.UseVisualStyleBackColor = true;
		this.proxySettingsGroup.Controls.Add(this.textProxyPass);
		this.proxySettingsGroup.Controls.Add(this.textProxyUser);
		this.proxySettingsGroup.Controls.Add(this.labelProxyPass);
		this.proxySettingsGroup.Controls.Add(this.labelProxyUser);
		this.proxySettingsGroup.Controls.Add(this.textProxyPort);
		this.proxySettingsGroup.Controls.Add(this.textProxyHost);
		this.proxySettingsGroup.Controls.Add(this.labelProxyPort);
		this.proxySettingsGroup.Controls.Add(this.labelProxyHost);
		this.proxySettingsGroup.Controls.Add(this.comboProxyType);
		this.proxySettingsGroup.Controls.Add(this.labelProxyType);
		this.proxySettingsGroup.ForeColor = System.Drawing.SystemColors.Desktop;
		this.proxySettingsGroup.Location = new System.Drawing.Point(6, 15);
		this.proxySettingsGroup.Name = "proxySettingsGroup";
		this.proxySettingsGroup.Size = new System.Drawing.Size(585, 96);
		this.proxySettingsGroup.TabIndex = 2;
		this.proxySettingsGroup.TabStop = false;
		this.proxySettingsGroup.Text = "Proxy Server Settings";
		this.toolTip1.SetToolTip(this.proxySettingsGroup, "RM can use proxy to connect to the tracker, so you can hide your real IP.");
		this.textProxyPass.Location = new System.Drawing.Point(456, 62);
		this.textProxyPass.Name = "textProxyPass";
		this.textProxyPass.Size = new System.Drawing.Size(90, 20);
		this.textProxyPass.TabIndex = 10;
		this.textProxyUser.Location = new System.Drawing.Point(267, 62);
		this.textProxyUser.Name = "textProxyUser";
		this.textProxyUser.Size = new System.Drawing.Size(90, 20);
		this.textProxyUser.TabIndex = 9;
		this.labelProxyPass.ForeColor = System.Drawing.SystemColors.ControlText;
		this.labelProxyPass.Location = new System.Drawing.Point(359, 65);
		this.labelProxyPass.Name = "labelProxyPass";
		this.labelProxyPass.Size = new System.Drawing.Size(84, 13);
		this.labelProxyPass.TabIndex = 8;
		this.labelProxyPass.Text = "Password:";
		this.labelProxyPass.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.labelProxyUser.ForeColor = System.Drawing.SystemColors.ControlText;
		this.labelProxyUser.Location = new System.Drawing.Point(203, 65);
		this.labelProxyUser.Name = "labelProxyUser";
		this.labelProxyUser.Size = new System.Drawing.Size(55, 13);
		this.labelProxyUser.TabIndex = 7;
		this.labelProxyUser.Text = "User:";
		this.labelProxyUser.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.textProxyPort.Location = new System.Drawing.Point(501, 26);
		this.textProxyPort.Name = "textProxyPort";
		this.textProxyPort.Size = new System.Drawing.Size(45, 20);
		this.textProxyPort.TabIndex = 5;
		this.textProxyHost.Location = new System.Drawing.Point(267, 26);
		this.textProxyHost.Name = "textProxyHost";
		this.textProxyHost.Size = new System.Drawing.Size(144, 20);
		this.textProxyHost.TabIndex = 4;
		this.labelProxyPort.ForeColor = System.Drawing.SystemColors.ControlText;
		this.labelProxyPort.Location = new System.Drawing.Point(432, 29);
		this.labelProxyPort.Name = "labelProxyPort";
		this.labelProxyPort.Size = new System.Drawing.Size(57, 13);
		this.labelProxyPort.TabIndex = 3;
		this.labelProxyPort.Text = "Port:";
		this.labelProxyPort.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.labelProxyHost.ForeColor = System.Drawing.SystemColors.ControlText;
		this.labelProxyHost.Location = new System.Drawing.Point(208, 28);
		this.labelProxyHost.Name = "labelProxyHost";
		this.labelProxyHost.Size = new System.Drawing.Size(50, 13);
		this.labelProxyHost.TabIndex = 2;
		this.labelProxyHost.Text = "Host:";
		this.labelProxyHost.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.comboProxyType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboProxyType.FormattingEnabled = true;
		this.comboProxyType.Items.AddRange(new object[6] { "None", "SOCKS4", "SOCKS4a", "SOCKS5", "HTTP (CONNECT)", "HTTP " });
		this.comboProxyType.Location = new System.Drawing.Point(75, 25);
		this.comboProxyType.Name = "comboProxyType";
		this.comboProxyType.Size = new System.Drawing.Size(125, 21);
		this.comboProxyType.TabIndex = 1;
		this.labelProxyType.AutoSize = true;
		this.labelProxyType.ForeColor = System.Drawing.SystemColors.ControlText;
		this.labelProxyType.Location = new System.Drawing.Point(6, 28);
		this.labelProxyType.Name = "labelProxyType";
		this.labelProxyType.Size = new System.Drawing.Size(63, 13);
		this.labelProxyType.TabIndex = 0;
		this.labelProxyType.Text = "Proxy Type:";
		this.tabAdvanced.Controls.Add(this.checkMinimizeToTray);
		this.tabAdvanced.Controls.Add(this.memoryReaderButton);
		this.tabAdvanced.Controls.Add(this.updateAnnounceParamsOnStart);
		this.tabAdvanced.Controls.Add(this.checkShowTrayBaloon);
		this.tabAdvanced.Controls.Add(this.TorrentClientsBox);
		this.tabAdvanced.Controls.Add(this.ClientLabel);
		this.tabAdvanced.Controls.Add(this.randomSpeedGroup);
		this.tabAdvanced.Controls.Add(this.reportParamsGroup);
		this.tabAdvanced.Location = new System.Drawing.Point(4, 22);
		this.tabAdvanced.Name = "tabAdvanced";
		this.tabAdvanced.Size = new System.Drawing.Size(597, 348);
		this.tabAdvanced.TabIndex = 2;
		this.tabAdvanced.Text = "Advanced";
		this.tabAdvanced.UseVisualStyleBackColor = true;
		this.checkMinimizeToTray.Checked = false;
		this.checkMinimizeToTray.CheckState = System.Windows.Forms.CheckState.Unchecked;
		this.checkMinimizeToTray.Location = new System.Drawing.Point(15, 328);
		this.checkMinimizeToTray.Name = "checkMinimizeToTray";
		this.checkMinimizeToTray.Size = new System.Drawing.Size(178, 17);
		this.checkMinimizeToTray.TabIndex = 29;
		this.checkMinimizeToTray.Text = "Minimize to tray";
		this.checkMinimizeToTray.UseVisualStyleBackColor = true;
		this.memoryReaderButton.Location = new System.Drawing.Point(476, 13);
		this.memoryReaderButton.Name = "memoryReaderButton";
		this.memoryReaderButton.Size = new System.Drawing.Size(107, 23);
		this.memoryReaderButton.TabIndex = 28;
		this.memoryReaderButton.Text = "Memory Reader...";
		this.toolTip1.SetToolTip(this.memoryReaderButton, "Memory Reader\r\nUse this tool when you want simulated client parameters \r\nas peer_id, key, port and others to match your real client parameters.");
		this.memoryReaderButton.UseVisualStyleBackColor = true;
		this.memoryReaderButton.Click += new System.EventHandler(memoryReaderButton_Click);
		this.updateAnnounceParamsOnStart.ForeColor = System.Drawing.SystemColors.ControlText;
		this.updateAnnounceParamsOnStart.Location = new System.Drawing.Point(25, 45);
		this.updateAnnounceParamsOnStart.Name = "updateAnnounceParamsOnStart";
		this.updateAnnounceParamsOnStart.Size = new System.Drawing.Size(276, 17);
		this.updateAnnounceParamsOnStart.TabIndex = 27;
		this.updateAnnounceParamsOnStart.Text = "Update peer_id and key on startup";
		this.toolTip1.SetToolTip(this.updateAnnounceParamsOnStart, "When this option checked,RM generates new peer_id and key each time you start it.\r\nIf unchecked RM  uses values saved from previous time.");
		this.updateAnnounceParamsOnStart.UseVisualStyleBackColor = true;
		this.checkShowTrayBaloon.Checked = false;
		this.checkShowTrayBaloon.CheckState = System.Windows.Forms.CheckState.Unchecked;
		this.checkShowTrayBaloon.Location = new System.Drawing.Point(15, 306);
		this.checkShowTrayBaloon.Name = "checkShowTrayBaloon";
		this.checkShowTrayBaloon.Size = new System.Drawing.Size(220, 17);
		this.checkShowTrayBaloon.TabIndex = 25;
		this.checkShowTrayBaloon.Text = "Enable tray hover-over info";
		this.checkShowTrayBaloon.UseVisualStyleBackColor = true;
		this.TorrentClientsBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.TorrentClientsBox.FormattingEnabled = true;
		this.TorrentClientsBox.Location = new System.Drawing.Point(187, 15);
		this.TorrentClientsBox.Name = "TorrentClientsBox";
		this.TorrentClientsBox.Size = new System.Drawing.Size(273, 21);
		this.TorrentClientsBox.TabIndex = 24;
		this.toolTip1.SetToolTip(this.TorrentClientsBox, "In this ComboBox choose the client you want to simulate");
		this.TorrentClientsBox.SelectedIndexChanged += new System.EventHandler(TorrentClientsBox_SelectedIndexChanged);
		this.ClientLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.ClientLabel.ForeColor = System.Drawing.SystemColors.ControlText;
		this.ClientLabel.Location = new System.Drawing.Point(15, 15);
		this.ClientLabel.Name = "ClientLabel";
		this.ClientLabel.Size = new System.Drawing.Size(166, 18);
		this.ClientLabel.TabIndex = 23;
		this.ClientLabel.Text = "Client Simulation:";
		this.ClientLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.randomSpeedGroup.Controls.Add(this.RandomDownloadTo);
		this.randomSpeedGroup.Controls.Add(this.RandomUploadTo);
		this.randomSpeedGroup.Controls.Add(this.RandomDownloadFrom);
		this.randomSpeedGroup.Controls.Add(this.RandomUploadFrom);
		this.randomSpeedGroup.Controls.Add(this.RandomDownloadToLabel);
		this.randomSpeedGroup.Controls.Add(this.RandomUploadToLabel);
		this.randomSpeedGroup.Controls.Add(this.RandomDownloadFromLabel);
		this.randomSpeedGroup.Controls.Add(this.RandomUploadFromLabel);
		this.randomSpeedGroup.Controls.Add(this.checkRandomDownload);
		this.randomSpeedGroup.Controls.Add(this.checkRandomUpload);
		this.randomSpeedGroup.ForeColor = System.Drawing.SystemColors.Desktop;
		this.randomSpeedGroup.Location = new System.Drawing.Point(15, 215);
		this.randomSpeedGroup.Name = "randomSpeedGroup";
		this.randomSpeedGroup.Size = new System.Drawing.Size(568, 74);
		this.randomSpeedGroup.TabIndex = 12;
		this.randomSpeedGroup.TabStop = false;
		this.randomSpeedGroup.Text = "Randomise Upload/Download speeds";
		this.toolTip1.SetToolTip(this.randomSpeedGroup, "Rm can randomise the speeds after each announce \r\naccording to those settings.\r\nUploading too long on exactly same speed is suspicious.");
		this.RandomDownloadTo.Location = new System.Drawing.Point(432, 39);
		this.RandomDownloadTo.Name = "RandomDownloadTo";
		this.RandomDownloadTo.Size = new System.Drawing.Size(56, 20);
		this.RandomDownloadTo.TabIndex = 9;
		this.RandomDownloadTo.Text = "100";
		this.RandomDownloadTo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.RandomUploadTo.Location = new System.Drawing.Point(432, 16);
		this.RandomUploadTo.Name = "RandomUploadTo";
		this.RandomUploadTo.Size = new System.Drawing.Size(56, 20);
		this.RandomUploadTo.TabIndex = 8;
		this.RandomUploadTo.Text = "100";
		this.RandomUploadTo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.RandomDownloadFrom.Location = new System.Drawing.Point(244, 39);
		this.RandomDownloadFrom.Name = "RandomDownloadFrom";
		this.RandomDownloadFrom.Size = new System.Drawing.Size(58, 20);
		this.RandomDownloadFrom.TabIndex = 7;
		this.RandomDownloadFrom.Text = "0";
		this.RandomDownloadFrom.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.RandomUploadFrom.Location = new System.Drawing.Point(244, 16);
		this.RandomUploadFrom.Name = "RandomUploadFrom";
		this.RandomUploadFrom.Size = new System.Drawing.Size(58, 20);
		this.RandomUploadFrom.TabIndex = 6;
		this.RandomUploadFrom.Text = "0";
		this.RandomUploadFrom.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.RandomDownloadToLabel.ForeColor = System.Drawing.SystemColors.ControlText;
		this.RandomDownloadToLabel.Location = new System.Drawing.Point(347, 43);
		this.RandomDownloadToLabel.Name = "RandomDownloadToLabel";
		this.RandomDownloadToLabel.Size = new System.Drawing.Size(75, 13);
		this.RandomDownloadToLabel.TabIndex = 5;
		this.RandomDownloadToLabel.Text = "Max:";
		this.RandomDownloadToLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.RandomUploadToLabel.ForeColor = System.Drawing.SystemColors.ControlText;
		this.RandomUploadToLabel.Location = new System.Drawing.Point(344, 20);
		this.RandomUploadToLabel.Name = "RandomUploadToLabel";
		this.RandomUploadToLabel.Size = new System.Drawing.Size(78, 13);
		this.RandomUploadToLabel.TabIndex = 4;
		this.RandomUploadToLabel.Text = "Max:";
		this.RandomUploadToLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.RandomDownloadFromLabel.ForeColor = System.Drawing.SystemColors.ControlText;
		this.RandomDownloadFromLabel.Location = new System.Drawing.Point(193, 43);
		this.RandomDownloadFromLabel.Name = "RandomDownloadFromLabel";
		this.RandomDownloadFromLabel.Size = new System.Drawing.Size(45, 13);
		this.RandomDownloadFromLabel.TabIndex = 3;
		this.RandomDownloadFromLabel.Text = "Min:";
		this.RandomDownloadFromLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.RandomUploadFromLabel.ForeColor = System.Drawing.SystemColors.ControlText;
		this.RandomUploadFromLabel.Location = new System.Drawing.Point(190, 20);
		this.RandomUploadFromLabel.Name = "RandomUploadFromLabel";
		this.RandomUploadFromLabel.Size = new System.Drawing.Size(48, 13);
		this.RandomUploadFromLabel.TabIndex = 2;
		this.RandomUploadFromLabel.Text = "Min:";
		this.RandomUploadFromLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.checkRandomDownload.ForeColor = System.Drawing.SystemColors.ControlText;
		this.checkRandomDownload.Location = new System.Drawing.Point(20, 42);
		this.checkRandomDownload.Name = "checkRandomDownload";
		this.checkRandomDownload.Size = new System.Drawing.Size(131, 17);
		this.checkRandomDownload.TabIndex = 1;
		this.checkRandomDownload.Text = "Download (kB)";
		this.checkRandomDownload.UseVisualStyleBackColor = true;
		this.checkRandomUpload.ForeColor = System.Drawing.SystemColors.ControlText;
		this.checkRandomUpload.Location = new System.Drawing.Point(20, 19);
		this.checkRandomUpload.Name = "checkRandomUpload";
		this.checkRandomUpload.Size = new System.Drawing.Size(131, 17);
		this.checkRandomUpload.TabIndex = 0;
		this.checkRandomUpload.Text = "Upload  (kB)";
		this.checkRandomUpload.UseVisualStyleBackColor = true;
		this.reportParamsGroup.Controls.Add(this.customKey);
		this.reportParamsGroup.Controls.Add(this.customPeerID);
		this.reportParamsGroup.Controls.Add(this.keyLabel);
		this.reportParamsGroup.Controls.Add(this.customPeersNum);
		this.reportParamsGroup.Controls.Add(this.labelPeerID);
		this.reportParamsGroup.Controls.Add(this.numwantLabel);
		this.reportParamsGroup.Controls.Add(this.portInfoLabel);
		this.reportParamsGroup.Controls.Add(this.portLabel);
		this.reportParamsGroup.Controls.Add(this.customPort);
		this.reportParamsGroup.ForeColor = System.Drawing.SystemColors.Desktop;
		this.reportParamsGroup.Location = new System.Drawing.Point(15, 68);
		this.reportParamsGroup.Name = "reportParamsGroup";
		this.reportParamsGroup.Size = new System.Drawing.Size(568, 132);
		this.reportParamsGroup.TabIndex = 1;
		this.reportParamsGroup.TabStop = false;
		this.reportParamsGroup.Text = "Announce Parameters";
		this.toolTip1.SetToolTip(this.reportParamsGroup, resources.GetString("reportParamsGroup.ToolTip"));
		this.customKey.Location = new System.Drawing.Point(149, 82);
		this.customKey.Name = "customKey";
		this.customKey.Size = new System.Drawing.Size(226, 20);
		this.customKey.TabIndex = 4;
		this.customPeerID.Location = new System.Drawing.Point(149, 56);
		this.customPeerID.Name = "customPeerID";
		this.customPeerID.Size = new System.Drawing.Size(339, 20);
		this.customPeerID.TabIndex = 10;
		this.keyLabel.ForeColor = System.Drawing.SystemColors.ControlText;
		this.keyLabel.Location = new System.Drawing.Point(15, 85);
		this.keyLabel.Name = "keyLabel";
		this.keyLabel.Size = new System.Drawing.Size(128, 13);
		this.keyLabel.TabIndex = 3;
		this.keyLabel.Text = "Key (key):";
		this.keyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.customPeersNum.Location = new System.Drawing.Point(442, 109);
		this.customPeersNum.Name = "customPeersNum";
		this.customPeersNum.Size = new System.Drawing.Size(46, 20);
		this.customPeersNum.TabIndex = 7;
		this.labelPeerID.ForeColor = System.Drawing.SystemColors.ControlText;
		this.labelPeerID.Location = new System.Drawing.Point(9, 59);
		this.labelPeerID.Name = "labelPeerID";
		this.labelPeerID.Size = new System.Drawing.Size(134, 13);
		this.labelPeerID.TabIndex = 9;
		this.labelPeerID.Text = "Peer ID (peer_id):";
		this.labelPeerID.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.numwantLabel.ForeColor = System.Drawing.SystemColors.ControlText;
		this.numwantLabel.Location = new System.Drawing.Point(199, 111);
		this.numwantLabel.Name = "numwantLabel";
		this.numwantLabel.Size = new System.Drawing.Size(235, 13);
		this.numwantLabel.TabIndex = 6;
		this.numwantLabel.Text = "Number of Peers (numwant):";
		this.numwantLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.portInfoLabel.ForeColor = System.Drawing.SystemColors.ControlText;
		this.portInfoLabel.Location = new System.Drawing.Point(6, 16);
		this.portInfoLabel.Name = "portInfoLabel";
		this.portInfoLabel.Size = new System.Drawing.Size(556, 37);
		this.portInfoLabel.TabIndex = 2;
		this.portInfoLabel.Text = "If those fields are left empty, RatioMaster will use random or default values";
		this.portLabel.ForeColor = System.Drawing.SystemColors.ControlText;
		this.portLabel.Location = new System.Drawing.Point(12, 109);
		this.portLabel.Name = "portLabel";
		this.portLabel.Size = new System.Drawing.Size(131, 17);
		this.portLabel.TabIndex = 1;
		this.portLabel.Text = "Port (port):";
		this.portLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.customPort.Location = new System.Drawing.Point(149, 108);
		this.customPort.Name = "customPort";
		this.customPort.Size = new System.Drawing.Size(44, 20);
		this.customPort.TabIndex = 0;
		this.cbbLanguages.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.cbbLanguages.FormattingEnabled = true;
		this.cbbLanguages.Location = new System.Drawing.Point(460, 17);
		this.cbbLanguages.Name = "cbbLanguages";
		this.cbbLanguages.Size = new System.Drawing.Size(156, 21);
		this.cbbLanguages.TabIndex = 29;
		this.cbbLanguages.SelectionChangeCommitted += new System.EventHandler(cbbLanguages_SelectionChangeCommitted);
		this.tabGeneral.Controls.Add(this.groupTorrentFile);
		this.tabGeneral.Controls.Add(this.groupStats);
		this.tabGeneral.Controls.Add(this.groupBoxOptions);
		this.tabGeneral.Controls.Add(this.groupTorrentInfo);
		this.tabGeneral.Location = new System.Drawing.Point(4, 22);
		this.tabGeneral.Name = "tabGeneral";
		this.tabGeneral.Padding = new System.Windows.Forms.Padding(3);
		this.tabGeneral.Size = new System.Drawing.Size(597, 348);
		this.tabGeneral.TabIndex = 0;
		this.tabGeneral.Text = "General";
		this.tabGeneral.UseVisualStyleBackColor = true;
		this.groupTorrentFile.Controls.Add(this.torrentFile);
		this.groupTorrentFile.Controls.Add(this.TorrentFileLabel);
		this.groupTorrentFile.Controls.Add(this.browseButton);
		this.groupTorrentFile.ForeColor = System.Drawing.SystemColors.Desktop;
		this.groupTorrentFile.Location = new System.Drawing.Point(12, 6);
		this.groupTorrentFile.Name = "groupTorrentFile";
		this.groupTorrentFile.Size = new System.Drawing.Size(579, 49);
		this.groupTorrentFile.TabIndex = 20;
		this.groupTorrentFile.TabStop = false;
		this.groupTorrentFile.Text = "Torrent File";
		this.torrentFile.FormattingEnabled = true;
		this.torrentFile.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.torrentFile.Location = new System.Drawing.Point(66, 19);
		this.torrentFile.MaxDropDownItems = 10;
		this.torrentFile.Name = "torrentFile";
		this.torrentFile.Size = new System.Drawing.Size(420, 21);
		this.torrentFile.TabIndex = 8;
		this.toolTip1.SetToolTip(this.torrentFile, "Torrent File Path\r\nClick Browse... or Drag & Drop torrent into RM window");
		this.torrentFile.SelectionChangeCommitted += new System.EventHandler(torrentFileBox_SelectionChangeCommitted);
		this.TorrentFileLabel.ForeColor = System.Drawing.SystemColors.ControlText;
		this.TorrentFileLabel.Location = new System.Drawing.Point(6, 17);
		this.TorrentFileLabel.Name = "TorrentFileLabel";
		this.TorrentFileLabel.Size = new System.Drawing.Size(54, 23);
		this.TorrentFileLabel.TabIndex = 14;
		this.TorrentFileLabel.Text = "Path:";
		this.TorrentFileLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.browseButton.ForeColor = System.Drawing.SystemColors.ControlText;
		this.browseButton.Location = new System.Drawing.Point(492, 17);
		this.browseButton.Name = "browseButton";
		this.browseButton.Size = new System.Drawing.Size(78, 23);
		this.browseButton.TabIndex = 16;
		this.browseButton.Text = "Browse...";
		this.browseButton.Click += new System.EventHandler(browseButton_Click);
		this.groupStats.Controls.Add(this.totalRunningTime);
		this.groupStats.Controls.Add(this.totalRunningTimeLabel);
		this.groupStats.Controls.Add(this.downloadCount);
		this.groupStats.Controls.Add(this.uploadCount);
		this.groupStats.Controls.Add(this.leechLabel);
		this.groupStats.Controls.Add(this.seedLabel);
		this.groupStats.Controls.Add(this.manualUpdateButton);
		this.groupStats.Controls.Add(this.timerValue);
		this.groupStats.Controls.Add(this.labelUpdateIn);
		this.groupStats.Controls.Add(this.downloadCountLabel);
		this.groupStats.Controls.Add(this.uploadCountLabel);
		this.groupStats.ForeColor = System.Drawing.SystemColors.Desktop;
		this.groupStats.Location = new System.Drawing.Point(12, 264);
		this.groupStats.Name = "groupStats";
		this.groupStats.Size = new System.Drawing.Size(579, 78);
		this.groupStats.TabIndex = 19;
		this.groupStats.TabStop = false;
		this.groupStats.Text = "Stats";
		this.totalRunningTime.ForeColor = System.Drawing.SystemColors.ControlText;
		this.totalRunningTime.Location = new System.Drawing.Point(495, 13);
		this.totalRunningTime.Name = "totalRunningTime";
		this.totalRunningTime.Size = new System.Drawing.Size(78, 18);
		this.totalRunningTime.TabIndex = 19;
		this.totalRunningTime.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.totalRunningTimeLabel.ForeColor = System.Drawing.SystemColors.ControlText;
		this.totalRunningTimeLabel.Location = new System.Drawing.Point(395, 13);
		this.totalRunningTimeLabel.Name = "totalRunningTimeLabel";
		this.totalRunningTimeLabel.Size = new System.Drawing.Size(94, 18);
		this.totalRunningTimeLabel.TabIndex = 18;
		this.totalRunningTimeLabel.Text = "Total Time :";
		this.totalRunningTimeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.toolTip1.SetToolTip(this.totalRunningTimeLabel, "Total running time for this session");
		this.downloadCount.ForeColor = System.Drawing.SystemColors.ControlText;
		this.downloadCount.Location = new System.Drawing.Point(300, 13);
		this.downloadCount.Name = "downloadCount";
		this.downloadCount.Size = new System.Drawing.Size(97, 18);
		this.downloadCount.TabIndex = 17;
		this.downloadCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.downloadCount.MouseHover += new System.EventHandler(downloadCount_MouseHover);
		this.uploadCount.ForeColor = System.Drawing.SystemColors.ControlText;
		this.uploadCount.Location = new System.Drawing.Point(124, 13);
		this.uploadCount.Name = "uploadCount";
		this.uploadCount.Size = new System.Drawing.Size(72, 18);
		this.uploadCount.TabIndex = 16;
		this.uploadCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.uploadCount.MouseHover += new System.EventHandler(uploadCount_MouseHover);
		this.leechLabel.ForeColor = System.Drawing.SystemColors.ControlText;
		this.leechLabel.Location = new System.Drawing.Point(438, 52);
		this.leechLabel.Name = "leechLabel";
		this.leechLabel.Size = new System.Drawing.Size(120, 13);
		this.leechLabel.TabIndex = 15;
		this.leechLabel.Text = "Leechers:";
		this.leechLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.toolTip1.SetToolTip(this.leechLabel, "Leechers on this torrent\r\nOn some trackers this data is not available");
		this.seedLabel.ForeColor = System.Drawing.SystemColors.ControlText;
		this.seedLabel.Location = new System.Drawing.Point(337, 52);
		this.seedLabel.Name = "seedLabel";
		this.seedLabel.Size = new System.Drawing.Size(95, 13);
		this.seedLabel.TabIndex = 14;
		this.seedLabel.Text = "Seeders:";
		this.seedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.toolTip1.SetToolTip(this.seedLabel, "Seeders on this torrent\r\nOn some trackers this data is not available");
		this.manualUpdateButton.Enabled = false;
		this.manualUpdateButton.ForeColor = System.Drawing.SystemColors.ControlText;
		this.manualUpdateButton.Location = new System.Drawing.Point(193, 47);
		this.manualUpdateButton.Name = "manualUpdateButton";
		this.manualUpdateButton.Size = new System.Drawing.Size(138, 23);
		this.manualUpdateButton.TabIndex = 13;
		this.manualUpdateButton.Text = "Manual Update";
		this.toolTip1.SetToolTip(this.manualUpdateButton, "Manual Update\r\nTracker can be updated manually, your stats on the tracker should change accordingly.");
		this.manualUpdateButton.UseVisualStyleBackColor = true;
		this.manualUpdateButton.Click += new System.EventHandler(manualUpdateButton_Click);
		this.timerValue.ForeColor = System.Drawing.SystemColors.ControlText;
		this.timerValue.Location = new System.Drawing.Point(124, 52);
		this.timerValue.Name = "timerValue";
		this.timerValue.Size = new System.Drawing.Size(57, 15);
		this.timerValue.TabIndex = 12;
		this.timerValue.Text = "0";
		this.timerValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelUpdateIn.ForeColor = System.Drawing.SystemColors.ControlText;
		this.labelUpdateIn.Location = new System.Drawing.Point(6, 52);
		this.labelUpdateIn.Name = "labelUpdateIn";
		this.labelUpdateIn.Size = new System.Drawing.Size(118, 15);
		this.labelUpdateIn.TabIndex = 11;
		this.labelUpdateIn.Text = "Update in:";
		this.labelUpdateIn.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.toolTip1.SetToolTip(this.labelUpdateIn, "Time for next update");
		this.downloadCountLabel.ForeColor = System.Drawing.SystemColors.ControlText;
		this.downloadCountLabel.Location = new System.Drawing.Point(202, 13);
		this.downloadCountLabel.Name = "downloadCountLabel";
		this.downloadCountLabel.Size = new System.Drawing.Size(92, 18);
		this.downloadCountLabel.TabIndex = 9;
		this.downloadCountLabel.Text = "Downloaded :";
		this.downloadCountLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.toolTip1.SetToolTip(this.downloadCountLabel, "Downloaded\r\nAfter next update your stats on tracker should change accordingly");
		this.uploadCountLabel.ForeColor = System.Drawing.SystemColors.ControlText;
		this.uploadCountLabel.Location = new System.Drawing.Point(9, 13);
		this.uploadCountLabel.Name = "uploadCountLabel";
		this.uploadCountLabel.Size = new System.Drawing.Size(109, 18);
		this.uploadCountLabel.TabIndex = 6;
		this.uploadCountLabel.Text = "Uploaded :";
		this.uploadCountLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.toolTip1.SetToolTip(this.uploadCountLabel, "Updoaded\r\nAfter next update your stats on tracker should change accordingly");
		this.groupBoxOptions.Controls.Add(this.applyStopSettingsButton);
		this.groupBoxOptions.Controls.Add(this.stopProcessUnitsBox);
		this.groupBoxOptions.Controls.Add(this.stopProcessActionBox);
		this.groupBoxOptions.Controls.Add(this.stopProcessValue);
		this.groupBoxOptions.Controls.Add(this.stopProcessLabel);
		this.groupBoxOptions.Controls.Add(this.fileSize);
		this.groupBoxOptions.Controls.Add(this.FileSizeLabel);
		this.groupBoxOptions.Controls.Add(this.uploadRate);
		this.groupBoxOptions.Controls.Add(this.downloadRate);
		this.groupBoxOptions.Controls.Add(this.uploadRateLabel);
		this.groupBoxOptions.Controls.Add(this.downloadRateLabel);
		this.groupBoxOptions.Controls.Add(this.speedWarningLabel);
		this.groupBoxOptions.ForeColor = System.Drawing.SystemColors.Desktop;
		this.groupBoxOptions.Location = new System.Drawing.Point(12, 143);
		this.groupBoxOptions.Name = "groupBoxOptions";
		this.groupBoxOptions.Size = new System.Drawing.Size(579, 115);
		this.groupBoxOptions.TabIndex = 18;
		this.groupBoxOptions.TabStop = false;
		this.groupBoxOptions.Text = "Options";
		this.applyStopSettingsButton.ForeColor = System.Drawing.SystemColors.ControlText;
		this.applyStopSettingsButton.Location = new System.Drawing.Point(460, 87);
		this.applyStopSettingsButton.Name = "applyStopSettingsButton";
		this.applyStopSettingsButton.Size = new System.Drawing.Size(98, 23);
		this.applyStopSettingsButton.TabIndex = 24;
		this.applyStopSettingsButton.Text = "Apply";
		this.applyStopSettingsButton.Visible = false;
		this.applyStopSettingsButton.Click += new System.EventHandler(applyStopSettingsButton_Click);
		this.stopProcessUnitsBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.stopProcessUnitsBox.FormattingEnabled = true;
		this.stopProcessUnitsBox.Location = new System.Drawing.Point(358, 88);
		this.stopProcessUnitsBox.Name = "stopProcessUnitsBox";
		this.stopProcessUnitsBox.Size = new System.Drawing.Size(79, 21);
		this.stopProcessUnitsBox.TabIndex = 23;
		this.stopProcessUnitsBox.Visible = false;
		this.stopProcessActionBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.stopProcessActionBox.FormattingEnabled = true;
		this.stopProcessActionBox.Items.AddRange(new object[4] { "Do not stop", "Uploaded", "Downloaded", "Time" });
		this.stopProcessActionBox.Location = new System.Drawing.Point(174, 88);
		this.stopProcessActionBox.Name = "stopProcessActionBox";
		this.stopProcessActionBox.Size = new System.Drawing.Size(92, 21);
		this.stopProcessActionBox.TabIndex = 22;
		this.toolTip1.SetToolTip(this.stopProcessActionBox, "RM can stop process automatically .\r\nChoose between different options and enter values.");
		this.stopProcessActionBox.SelectedIndexChanged += new System.EventHandler(stopProcessActionBox_SelectedIndexChanged);
		this.stopProcessActionBox.DropDown += new System.EventHandler(stopProcessActionBox_DropDown);
		this.stopProcessValue.Location = new System.Drawing.Point(272, 89);
		this.stopProcessValue.Name = "stopProcessValue";
		this.stopProcessValue.Size = new System.Drawing.Size(80, 20);
		this.stopProcessValue.TabIndex = 21;
		this.stopProcessValue.Text = "1000";
		this.stopProcessValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.stopProcessValue.Visible = false;
		this.stopProcessLabel.ForeColor = System.Drawing.SystemColors.ControlText;
		this.stopProcessLabel.Location = new System.Drawing.Point(9, 91);
		this.stopProcessLabel.Name = "stopProcessLabel";
		this.stopProcessLabel.Size = new System.Drawing.Size(155, 13);
		this.stopProcessLabel.TabIndex = 20;
		this.stopProcessLabel.Text = "Stop process after:";
		this.stopProcessLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.fileSize.Location = new System.Drawing.Point(473, 21);
		this.fileSize.Name = "fileSize";
		this.fileSize.Size = new System.Drawing.Size(62, 20);
		this.fileSize.TabIndex = 19;
		this.fileSize.Text = "0";
		this.fileSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.toolTip1.SetToolTip(this.fileSize, resources.GetString("fileSize.ToolTip"));
		this.FileSizeLabel.ForeColor = System.Drawing.SystemColors.ControlText;
		this.FileSizeLabel.Location = new System.Drawing.Point(334, 19);
		this.FileSizeLabel.Name = "FileSizeLabel";
		this.FileSizeLabel.Size = new System.Drawing.Size(133, 23);
		this.FileSizeLabel.TabIndex = 17;
		this.FileSizeLabel.Text = "Finished (%)";
		this.FileSizeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.uploadRate.Location = new System.Drawing.Point(214, 19);
		this.uploadRate.Name = "uploadRate";
		this.uploadRate.Size = new System.Drawing.Size(80, 20);
		this.uploadRate.TabIndex = 13;
		this.uploadRate.Text = "50";
		this.uploadRate.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.toolTip1.SetToolTip(this.uploadRate, "Upload Speed\r\nPut here speed that RM will report to the tracker");
		this.uploadRate.TextChanged += new System.EventHandler(uploadRate_TextChanged);
		this.downloadRate.Location = new System.Drawing.Point(214, 45);
		this.downloadRate.Name = "downloadRate";
		this.downloadRate.Size = new System.Drawing.Size(80, 20);
		this.downloadRate.TabIndex = 12;
		this.downloadRate.Text = "10";
		this.downloadRate.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.toolTip1.SetToolTip(this.downloadRate, "Download Speed\r\nPut here speed that RM will report to the tracker");
		this.downloadRate.TextChanged += new System.EventHandler(downloadRate_TextChanged);
		this.uploadRateLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.uploadRateLabel.ForeColor = System.Drawing.SystemColors.ControlText;
		this.uploadRateLabel.Location = new System.Drawing.Point(12, 19);
		this.uploadRateLabel.Name = "uploadRateLabel";
		this.uploadRateLabel.Size = new System.Drawing.Size(196, 23);
		this.uploadRateLabel.TabIndex = 11;
		this.uploadRateLabel.Text = "Upload Speed (kB/s) :";
		this.uploadRateLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.downloadRateLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.downloadRateLabel.ForeColor = System.Drawing.SystemColors.ControlText;
		this.downloadRateLabel.Location = new System.Drawing.Point(15, 45);
		this.downloadRateLabel.Name = "downloadRateLabel";
		this.downloadRateLabel.Size = new System.Drawing.Size(193, 23);
		this.downloadRateLabel.TabIndex = 10;
		this.downloadRateLabel.Text = "Download Speed (kB/s) :";
		this.downloadRateLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.speedWarningLabel.ForeColor = System.Drawing.Color.FromArgb(235, 85, 85);
		this.speedWarningLabel.Location = new System.Drawing.Point(214, 68);
		this.speedWarningLabel.Name = "speedWarningLabel";
		this.speedWarningLabel.Size = new System.Drawing.Size(330, 17);
		this.speedWarningLabel.TabIndex = 25;
		this.speedWarningLabel.Text = "Warning: speeds above 20000 KB/s may look suspicious.";
		this.speedWarningLabel.Visible = false;
		this.groupTorrentInfo.Controls.Add(this.torrentSize);
		this.groupTorrentInfo.Controls.Add(this.labelTorrentSize);
		this.groupTorrentInfo.Controls.Add(this.shaHash);
		this.groupTorrentInfo.Controls.Add(this.hashLabel);
		this.groupTorrentInfo.Controls.Add(this.trackerAddress);
		this.groupTorrentInfo.Controls.Add(this.TrackerLabel);
		this.groupTorrentInfo.ForeColor = System.Drawing.SystemColors.Desktop;
		this.groupTorrentInfo.Location = new System.Drawing.Point(12, 61);
		this.groupTorrentInfo.Name = "groupTorrentInfo";
		this.groupTorrentInfo.Size = new System.Drawing.Size(579, 76);
		this.groupTorrentInfo.TabIndex = 17;
		this.groupTorrentInfo.TabStop = false;
		this.groupTorrentInfo.Text = "Torrent Information";
		this.torrentSize.ForeColor = System.Drawing.SystemColors.ControlText;
		this.torrentSize.Location = new System.Drawing.Point(492, 43);
		this.torrentSize.Name = "torrentSize";
		this.torrentSize.Size = new System.Drawing.Size(78, 19);
		this.torrentSize.TabIndex = 7;
		this.torrentSize.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelTorrentSize.ForeColor = System.Drawing.SystemColors.ControlText;
		this.labelTorrentSize.Location = new System.Drawing.Point(404, 45);
		this.labelTorrentSize.Name = "labelTorrentSize";
		this.labelTorrentSize.Size = new System.Drawing.Size(82, 13);
		this.labelTorrentSize.TabIndex = 6;
		this.labelTorrentSize.Text = "Size:";
		this.labelTorrentSize.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.shaHash.Location = new System.Drawing.Point(95, 42);
		this.shaHash.Name = "shaHash";
		this.shaHash.Size = new System.Drawing.Size(302, 20);
		this.shaHash.TabIndex = 5;
		this.toolTip1.SetToolTip(this.shaHash, "Torrent Sha Hash\r\nYou can enter it manually or it will be generated from selected torrent");
		this.hashLabel.ForeColor = System.Drawing.SystemColors.ControlText;
		this.hashLabel.Location = new System.Drawing.Point(9, 39);
		this.hashLabel.Name = "hashLabel";
		this.hashLabel.Size = new System.Drawing.Size(80, 23);
		this.hashLabel.TabIndex = 4;
		this.hashLabel.Text = "SHA Hash:";
		this.hashLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.trackerAddress.Location = new System.Drawing.Point(95, 16);
		this.trackerAddress.Name = "trackerAddress";
		this.trackerAddress.Size = new System.Drawing.Size(302, 20);
		this.trackerAddress.TabIndex = 3;
		this.toolTip1.SetToolTip(this.trackerAddress, "Tracker Url\r\nYou can enter it manually or it will be generated from selected torrent");
		this.TrackerLabel.ForeColor = System.Drawing.SystemColors.ControlText;
		this.TrackerLabel.Location = new System.Drawing.Point(9, 16);
		this.TrackerLabel.Name = "TrackerLabel";
		this.TrackerLabel.Size = new System.Drawing.Size(80, 23);
		this.TrackerLabel.TabIndex = 2;
		this.TrackerLabel.Text = "Tracker:";
		this.TrackerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.tabControl1.Controls.Add(this.tabGeneral);
		this.tabControl1.Controls.Add(this.tabAdvanced);
		this.tabControl1.Controls.Add(this.tabNetwork);
		this.tabControl1.Controls.Add(this.tabLog);
		this.tabControl1.Controls.Add(this.tabAbout);
		this.tabControl1.Location = new System.Drawing.Point(15, 44);
		this.tabControl1.Name = "tabControl1";
		this.tabControl1.RightToLeftLayout = true;
		this.tabControl1.SelectedIndex = 0;
		this.tabControl1.Size = new System.Drawing.Size(605, 374);
		this.tabControl1.TabIndex = 20;
		this.toolTip1.AutomaticDelay = 100;
		this.toolTip1.AutoPopDelay = 10000;
		this.toolTip1.InitialDelay = 100;
		this.toolTip1.ReshowDelay = 20;
		this.toolTip1.Popup += new System.Windows.Forms.PopupEventHandler(toolTip1_Popup);
		this.lblInterface.ForeColor = System.Drawing.SystemColors.ControlText;
		this.lblInterface.Location = new System.Drawing.Point(15, 20);
		this.lblInterface.Name = "lblInterface";
		this.lblInterface.Size = new System.Drawing.Size(66, 13);
		this.lblInterface.TabIndex = 32;
		this.lblInterface.Text = "Interface:";
		this.lblInterface.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.cbbInterfaceTheme.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.cbbInterfaceTheme.FormattingEnabled = true;
		this.cbbInterfaceTheme.Items.AddRange(new object[2] { "Dark", "Light" });
		this.cbbInterfaceTheme.Location = new System.Drawing.Point(84, 16);
		this.cbbInterfaceTheme.Name = "cbbInterfaceTheme";
		this.cbbInterfaceTheme.Size = new System.Drawing.Size(92, 21);
		this.cbbInterfaceTheme.TabIndex = 33;
		this.cbbInterfaceTheme.SelectedIndexChanged += new System.EventHandler(cbbInterfaceTheme_SelectedIndexChanged);
		this.lblLanguage.ForeColor = System.Drawing.SystemColors.ControlText;
		this.lblLanguage.Location = new System.Drawing.Point(355, 20);
		this.lblLanguage.Name = "lblLanguage";
		this.lblLanguage.Size = new System.Drawing.Size(99, 13);
		this.lblLanguage.TabIndex = 30;
		this.lblLanguage.Text = "Language:";
		this.lblLanguage.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.ResetCountersButton.Location = new System.Drawing.Point(15, 425);
		this.ResetCountersButton.Name = "ResetCountersButton";
		this.ResetCountersButton.Size = new System.Drawing.Size(140, 23);
		this.ResetCountersButton.TabIndex = 31;
		this.ResetCountersButton.Text = "Reset Counters";
		this.ResetCountersButton.Visible = false;
		this.ResetCountersButton.Click += new System.EventHandler(ResetCountersButton_Click);
		this.AllowDrop = true;
		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
		this.AutoSize = true;
		base.ClientSize = new System.Drawing.Size(629, 460);
		base.Controls.Add(this.ResetCountersButton);
		base.Controls.Add(this.lblInterface);
		base.Controls.Add(this.cbbInterfaceTheme);
		base.Controls.Add(this.lblLanguage);
		base.Controls.Add(this.cbbLanguages);
		base.Controls.Add(this.StartButton);
		base.Controls.Add(this.StopButton);
		base.Controls.Add(this.closeButton);
		base.Controls.Add(this.tabControl1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.MaximizeBox = false;
		base.Name = "MainForm";
		this.RightToLeftLayout = true;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "RatioMaster 2.0";
		base.Load += new System.EventHandler(Form1_Load);
		base.Shown += new System.EventHandler(Form1_Shown);
		base.DragDrop += new System.Windows.Forms.DragEventHandler(Form1_DragDrop);
		base.DragEnter += new System.Windows.Forms.DragEventHandler(Form1_DragEnter);
		base.Move += new System.EventHandler(Form1_Move);
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(Form1_FormClosing);
		this.menuRightClickTray.ResumeLayout(false);
		this.tabAbout.ResumeLayout(false);
		this.tabAbout.PerformLayout();
		this.tabLog.ResumeLayout(false);
		this.tabNetwork.ResumeLayout(false);
		this.groupNetworkMisc.ResumeLayout(false);
		this.groupNetworkMisc.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.textStopMinLeecher).EndInit();
		this.proxySettingsGroup.ResumeLayout(false);
		this.proxySettingsGroup.PerformLayout();
		this.tabAdvanced.ResumeLayout(false);
		this.randomSpeedGroup.ResumeLayout(false);
		this.randomSpeedGroup.PerformLayout();
		this.reportParamsGroup.ResumeLayout(false);
		this.reportParamsGroup.PerformLayout();
		this.tabGeneral.ResumeLayout(false);
		this.groupTorrentFile.ResumeLayout(false);
		this.groupStats.ResumeLayout(false);
		this.groupBoxOptions.ResumeLayout(false);
		this.groupBoxOptions.PerformLayout();
		this.groupTorrentInfo.ResumeLayout(false);
		this.groupTorrentInfo.PerformLayout();
		this.tabControl1.ResumeLayout(false);
		base.ResumeLayout(false);
	}

	[STAThread]
	private static void Main()
	{
		try
		{
			RuntimeLog.Initialize();
			AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e)
			{
				if (e.ExceptionObject is Exception exception)
				{
					RuntimeLog.WriteException("Unhandled exception", exception);
				}
			};
			Application.ThreadException += delegate(object sender, ThreadExceptionEventArgs e)
			{
				RuntimeLog.WriteException("UI thread exception", e.Exception);
			};
			DarkTheme.LoadSavedThemeMode();
			DarkTheme.EnableDarkApplicationMode();
			Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(defaultValue: false);
			Application.Run(new MainForm());
		}
		catch (Exception ex)
		{
			RuntimeLog.WriteException("Fatal startup exception", ex);
			MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace, "Error");
		}
		try
		{
			Process.GetCurrentProcess().Kill();
		}
		catch (Exception)
		{
		}
	}

	private void Form1_Load(object sender, EventArgs e)
	{
		applicationSettings = new ApplicationSettings(this);
		TorrentClientsObj = new TorrentClientsEnum(this);
		versionAboutLabel.Text = "Build 66 using .NET 10.0";
		InitLocalization();
		deployDefaultValues();
		if (updateAnnounceParamsOnStart.Checked)
		{
			TorrentClientsBox_SelectedIndexChanged(null, null);
		}
		loadSelectedLanguage();
		ParseCommandLine();
		UseDNS();
		GetSystemVersion();
		GetJavaInfo();
		Thread thread = new Thread(GetUpnpInfo);
		thread.Name = "GetUpnpInfo() Thread";
		thread.Start();
		DeployRecentTorrents();
	}

	private void DeployRecentTorrents()
	{
		RefreshTorrentFileList(null);
	}

	private void RefreshTorrentFileList(string selectedPath)
	{
		RecentTorrents recentTorrents = new RecentTorrents(this);
		List<RecentTorrentListItem> items = recentTorrents.GetRecentTorrents();
		int selectedIndex = -1;
		if (!string.IsNullOrWhiteSpace(selectedPath))
		{
			for (int i = 0; i < items.Count; i++)
			{
				if (string.Equals(items[i].ToString(), selectedPath, StringComparison.OrdinalIgnoreCase))
				{
					selectedIndex = i;
					break;
				}
			}
			if (selectedIndex < 0)
			{
				items.Insert(0, new RecentTorrentListItem
				{
					applicationSettings = new ApplicationSettings { torrentFilePath = selectedPath },
					FilePath = selectedPath
				});
				selectedIndex = 0;
			}
		}
		torrentFile.BeginUpdate();
		try
		{
			torrentFile.DataSource = null;
			torrentFile.Items.Clear();
			torrentFile.DataSource = items;
			if (selectedIndex >= 0 && selectedIndex < torrentFile.Items.Count)
			{
				torrentFile.SelectedIndex = selectedIndex;
			}
		}
		finally
		{
			torrentFile.EndUpdate();
		}
	}

	private void GetUpnpInfo()
	{
		upnPNat = new UPnPNat(this);
		upnPNatEnabled = upnPNat.Enabled;
		_ = upnPNat.Enabled;
	}

	private void GetJavaInfo()
	{
		JavaInfo javaInfo = JavaInfo.Get();
		if (!javaInfo.Installed)
		{
			AddLogLine("Java Runtime Environment does not seems to be installed on this machine");
		}
		else
		{
			AddLogLine("Java Runtime Environment Version: " + javaInfo.CurrentVersion);
		}
	}

	private void GetSystemVersion()
	{
		string text = OsInfo.Get().ShortVersion + " (" + OsInfo.Get().FullVersion + ")";
		AddLogLine("Operating System: " + text);
	}

	private void Form1_Shown(object sender, EventArgs e)
	{
		lclzManager.LocalizeForm(this);
		localizeSpecialCases();
		DarkTheme.Apply(this);
		DarkTheme.Apply(menuRightClickTray);
		LayoutAboutTab();
		BeginInvoke(new Action(delegate
		{
			if (initialStartPosition == FormStartPosition.CenterScreen)
			{
				Rectangle workingArea = Screen.FromControl(this).WorkingArea;
				Location = new Point(workingArea.Left + (workingArea.Width - Width) / 2, workingArea.Top + (workingArea.Height - Height) / 2);
			}
			Opacity = 1.0;
		}));
	}

	public void InitLocalization()
	{
		lclzManager = new LocalizationManager(this);
		cbbLanguages.DataSource = lclzManager.GetLangList();
		if (cbbLanguages.Items.Count == 0)
		{
			cbbLanguages.Enabled = false;
		}
	}

	public void setSelectedLanguage(string filePath)
	{
		if (cbbLanguages.Items.Count == 0)
		{
			return;
		}
		if (string.IsNullOrEmpty(filePath))
		{
			cbbLanguages.SelectedIndex = 0;
		}
		foreach (object item in cbbLanguages.Items)
		{
			if (((LangInfo)item).File == filePath)
			{
				cbbLanguages.SelectedItem = item;
				return;
			}
		}
		cbbLanguages.SelectedIndex = 0;
	}

	public void loadSelectedLanguage()
	{
		if (cbbLanguages.SelectedItem != null)
		{
			lclzManager.LoadLanguageFromFile(((LangInfo)cbbLanguages.SelectedItem).File);
		}
	}

	private void cbbLanguages_SelectionChangeCommitted(object sender, EventArgs e)
	{
		loadSelectedLanguage();
		lclzManager.LocalizeForm(this);
		localizeSpecialCases();
		DarkTheme.Apply(this);
		DarkTheme.Apply(menuRightClickTray);
	}

	private void cbbInterfaceTheme_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (cbbInterfaceTheme.SelectedItem == null)
		{
			return;
		}
		DarkTheme.ApplyThemeMode(cbbInterfaceTheme.SelectedItem.ToString());
		DarkTheme.Apply(this);
		DarkTheme.Apply(menuRightClickTray);
		DarkTheme.Apply(toolTip1);
		LayoutAboutTab();
		Invalidate(true);
	}

	private void localizeSpecialCases()
	{
		labelWebSite.Text = "Modernized and improved by RygTech -";
		linkWebsite.Text = "https://rygtech.org";
		labelForums.Text = "Contribute or report any bugs on";
		linkForums.Text = "GitHub";
		stopProcessActionBox.Items[0] = lclzManager.TranslateMessage("stopProcessOpt1", "Do not stop");
		stopProcessActionBox.Items[1] = lclzManager.TranslateMessage("stopProcessOpt2", "Uploaded");
		stopProcessActionBox.Items[2] = lclzManager.TranslateMessage("stopProcessOpt3", "Downloaded");
		stopProcessActionBox.Items[3] = lclzManager.TranslateMessage("stopProcessOpt4", "Time");
		restoreToolStripMenuItem.Text = lclzManager.TranslateMessage("restoreToolStripMenuItem", "Restore");
		exitToolStripMenuItem.Text = lclzManager.TranslateMessage("exitToolStripMenuItem", "Exit");
		comboBindIp.Items[0] = new KeyValuePair("default", lclzManager.TranslateMessage("defaultBinding", "Default"));
		lblInterface.Text = "Interface:";
		LayoutAboutTab();
	}

	private void LayoutAboutTab()
	{
		int titleY = Math.Max(35, (tabAbout.ClientSize.Height - 190) / 2);
		label2.Location = new Point((tabAbout.ClientSize.Width - label2.Width) / 2, titleY);
		versionAboutLabel.Location = new Point(40, label2.Bottom + 13);
		CenterAboutRow(labelWebSite, linkWebsite, versionAboutLabel.Bottom + 18);
		CenterAboutRow(labelForums, linkForums, labelWebSite.Bottom + 7);
	}

	private void CenterAboutRow(Control textControl, Control linkControl, int y)
	{
		const int gap = 8;
		Size textSize = textControl.GetPreferredSize(Size.Empty);
		Size linkSize = linkControl.GetPreferredSize(Size.Empty);
		int totalWidth = textSize.Width + gap + linkSize.Width;
		int startX = Math.Max(0, (tabAbout.ClientSize.Width - totalWidth) / 2);
		textControl.Location = new Point(startX, y);
		linkControl.Location = new Point(startX + textSize.Width + gap, y);
	}

	public void ParseCommandLine()
	{
		if (Environment.GetCommandLineArgs().Length > 1)
		{
			Parser parser = new Parser(Environment.CommandLine, this);
			AddLogLine("Command Line = " + Environment.CommandLine);
			parser.AddSwitch("start", "Start Process");
			parser.AddSwitch("minimize", "Minimize Window");
			parser.AddSwitch("hide", "Hide Window");
			parser.Parse();
			if (parser.Parameters != null && parser.Parameters.Length > 0)
			{
				_ = parser.Parameters[0];
				torrentLoadedSuccessfully = loadTorrentFileInfo(parser.Parameters[0], loadSettings: true);
			}
			double result = 0.0;
			if (UploadRateCM.Length > 0 && double.TryParse(UploadRateCM, out result))
			{
				uploadRate.Text = result.ToString();
			}
			if (DownloadRateCM.Length > 0 && double.TryParse(DownloadRateCM, out result))
			{
				downloadRate.Text = result.ToString();
			}
			if (PercentFinishedCM.Length > 0 && double.TryParse(PercentFinishedCM, out result))
			{
				fileSize.Text = result.ToString();
				AddLogLine("fileSize.Text=" + fileSize.Text);
			}
			if (parser["hide"] != null)
			{
				base.WindowState = FormWindowState.Minimized;
				Hide();
				trayIcon.Visible = false;
			}
			else if (parser["minimize"] != null)
			{
				base.WindowState = FormWindowState.Minimized;
				Hide();
			}
			else
			{
				winRestore();
			}
			if (parser["start"] != null && torrentLoadedSuccessfully)
			{
				StartButton_Click(null, null);
			}
		}
	}

	private void OpenTcpListener()
	{
		try
		{
			if (checkTCPListen.Checked && localListen == null && currentProxy.proxyType == ProxyType.None)
			{
				if (upnPNatEnabled && checkUPnP.Checked)
				{
					TcpListenerMappingInfo = new PortMappingInfo("RatioMaster", "TCP", ipAddress, int.Parse(currentTorrent.port), IPAddress.Parse(ipAddress), int.Parse(currentTorrent.port), enabled: true);
					upnPNat.AddPortMapping(TcpListenerMappingInfo);
				}
				if (comboBindIp.SelectedIndex > 0)
				{
					IPAddress localaddr = IPAddress.Parse(comboBindIp.SelectedItem.ToString());
					localListen = new TcpListener(localaddr, int.Parse(currentTorrent.port));
				}
				else
				{
					localListen = new TcpListener(int.Parse(currentTorrent.port));
				}
				localListen.Start();
				AddLogLine("Started TCP listener on port " + currentTorrent.port);
				Thread thread = new Thread(AcceptTcpConnection);
				thread.Name = "AcceptTcpConnection() Thread";
				thread.Start();
			}
		}
		catch (Exception ex)
		{
			AddLogLine("Failed to open Tcp Listener: " + ex.Message);
			if (localListen != null)
			{
				localListen.Stop();
				localListen = null;
			}
		}
	}

	private void AcceptTcpConnection()
	{
		Socket socket = null;
		try
		{
			Encoding encoding = Encoding.GetEncoding(28591);
			string text = null;
			while (true)
			{
				socket = localListen.AcceptSocket();
				byte[] array = new byte[67];
				if (socket != null && socket.Connected)
				{
					AddLogLine("Client connected");
					NetworkStream networkStream = new NetworkStream(socket);
					networkStream.ReadTimeout = 1000;
					try
					{
						networkStream.Read(array, 0, array.Length);
					}
					catch (Exception)
					{
					}
					text = encoding.GetString(array, 0, array.Length);
					if (text.IndexOf("BitTorrent protocol") >= 0 && text.IndexOf(encoding.GetString(currentTorrentFile.InfoHash)) >= 0)
					{
						byte[] array2 = createHandshakeResponse();
						networkStream.Write(array2, 0, array2.Length);
					}
					socket.Close();
					networkStream.Close();
					networkStream.Dispose();
				}
			}
		}
		catch (Exception ex2)
		{
			AddLogLine("Error in AcceptTcpConnection(): " + ex2.Message);
		}
		finally
		{
			if (socket != null)
			{
				socket.Close();
				AddLogLine("Closed socket");
			}
			CloseTcpListener();
		}
	}

	private byte[] createChokeResponse()
	{
		return new byte[5] { 0, 0, 0, 1, 0 };
	}

	private byte[] createHandshakeResponse()
	{
		int num = 0;
		Encoding encoding = Encoding.GetEncoding(28591);
		new StringBuilder();
		string text = "BitTorrent protocol";
		byte[] array = new byte[256];
		array[num++] = (byte)text.Length;
		encoding.GetBytes(text, 0, text.Length, array, num);
		num += text.Length;
		for (int i = 0; i < 8; i++)
		{
			array[num++] = 0;
		}
		Buffer.BlockCopy(currentTorrentFile.InfoHash, 0, array, num, currentTorrentFile.InfoHash.Length);
		num += currentTorrentFile.InfoHash.Length;
		encoding.GetBytes(currentTorrent.peerID.ToCharArray(), 0, currentTorrent.peerID.Length, array, num);
		num += encoding.GetByteCount(currentTorrent.peerID);
		return array;
	}

	private void CloseTcpListener()
	{
		if (upnPNatEnabled && checkUPnP.Checked)
		{
			upnPNat.RemovePortMapping(TcpListenerMappingInfo);
		}
		if (localListen != null)
		{
			localListen.Stop();
			localListen = null;
			AddLogLine("TCP Listener closed");
		}
	}

	public void UseDNS()
	{
		string bindToIp = applicationSettings.bindToIp;
		int selectedIndex = 0;
		int num = 0;
		string hostName = Dns.GetHostName();
		AddLogLine("Host Name = " + hostName);
		IPHostEntry hostEntry = Dns.GetHostEntry(hostName);
		comboBindIp.Items.Clear();
		comboBindIp.Items.Add(new KeyValuePair("default", lclzManager.TranslateMessage("defaultBinding", "Default")));
		if (hostEntry.AddressList.Length > 0)
		{
			ipAddress = hostEntry.AddressList[0].ToString();
		}
		IPAddress[] addressList = hostEntry.AddressList;
		foreach (IPAddress iPAddress in addressList)
		{
			num++;
			AddLogLine("IPAddress = " + iPAddress.ToString() + " Family=" + iPAddress.AddressFamily);
			comboBindIp.Items.Add(new KeyValuePair(iPAddress, iPAddress.ToString()));
			if (bindToIp == iPAddress.ToString())
			{
				selectedIndex = num;
			}
		}
		comboBindIp.SelectedIndex = selectedIndex;
		SetLocalIpBinding();
	}

	public void SetLocalIpBinding()
	{
		if (comboBindIp.SelectedIndex > 0)
		{
			UseLocalIpBinding = true;
			ipAddress = comboBindIp.SelectedItem.ToString();
		}
		else
		{
			UseLocalIpBinding = false;
		}
	}

	private void StartButton_Click(object sender, EventArgs e)
	{
		seedMode = false;
		currentClient = getCurrentClient();
		currentProxy = GetCurrentProxy();
		currentTorrent = getCurrentTorrent();
		if (currentTorrent.trackerUri == null)
		{
			MessageBox.Show("To start , please select a valid torrent file by clicking on 'Browse...' button or Drag & Drop it into RatioMaster", "Start", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			return;
		}
		updateScrapStats("", "", "");
		totalRunningTimeCounter = 0;
		timerValue.Text = "updating...";
		StartButton.Enabled = false;
		StopButton.Enabled = true;
		manualUpdateButton.Enabled = true;
		ResetCountersButton.Visible = true;
		OpenTcpListener();
		Thread thread = new Thread(startProcess);
		thread.Name = "startProcess() Thread";
		thread.Start();
		serverUpdateTimer.Start();
	}

	private void StopButton_Click(object sender, EventArgs e)
	{
		stopTimerAndCounters();
		Thread thread = new Thread(stopProcess);
		thread.Name = "stopProcess() Thread";
		thread.Start();
	}

	private void stopTimerAndCounters()
	{
		if (StartButton.InvokeRequired)
		{
			stopTimerAndCountersCallback method = stopTimerAndCounters;
			Invoke(method, new object[0]);
			return;
		}
		serverUpdateTimer.Stop();
		StartButton.Enabled = true;
		StopButton.Enabled = false;
		manualUpdateButton.Enabled = false;
		ResetCountersButton.Visible = false;
		CloseTcpListener();
		temporaryIntervalCounter = 0;
		timerValue.Text = "0";
		currentTorrent.numberOfPeers = "0";
		updateProcessStarted = false;
	}

	private void updateInterval(string interval)
	{
		int result = 0;
		if (int.TryParse(interval, out result))
		{
			if (result > 3600)
			{
				result = 3600;
			}
			if (result < 60)
			{
				result = 60;
			}
			currentTorrent.interval = result;
			AddLogLine("Updating Interval: " + result);
			temporaryIntervalCounter = 5;
		}
	}

	private void manualUpdateButton_Click(object sender, EventArgs e)
	{
		if (updateProcessStarted)
		{
			OpenTcpListener();
			temporaryIntervalCounter = currentTorrent.interval;
		}
	}

	public string ConvertToTime(int seconds)
	{
		string text = "";
		if (seconds < 3600)
		{
			return (seconds / 60).ToString("00") + ":" + (seconds % 60).ToString("00");
		}
		return (seconds / 3600).ToString("00") + ":" + (seconds % 3600 / 60).ToString("00") + ":" + (seconds % 60).ToString("00");
	}

	public void randomiseSpeeds()
	{
		try
		{
			float num = (float)random.Next(100) / 100f;
			float num2 = (float)random.Next(100) / 100f;
			if (checkRandomUpload.Checked)
			{
				float num3 = (float)random.Next((int)float.Parse(RandomUploadFrom.Text), (int)float.Parse(RandomUploadTo.Text)) + num;
				uploadRate.Text = num3.ToString();
			}
			if (checkRandomDownload.Checked)
			{
				float num4 = (float)random.Next((int)float.Parse(RandomDownloadFrom.Text), (int)float.Parse(RandomDownloadTo.Text)) + num2;
				downloadRate.Text = num4.ToString();
			}
		}
		catch (Exception ex)
		{
			AddLogLine("Failed to randomise upload/download speeds: " + ex.Message);
		}
	}

	private void serverUpdateTimer_Tick(object sender, EventArgs e)
	{
		if (updateProcessStarted)
		{
			if (haveInitialPeers)
			{
				updateCounters(currentTorrent);
			}
			int num = currentTorrent.interval - temporaryIntervalCounter;
			totalRunningTimeCounter++;
			totalRunningTime.Text = ConvertToTime(totalRunningTimeCounter);
			if (IsStopProcessCondition())
			{
				StopButton_Click(null, null);
				return;
			}
			if (num > 0)
			{
				temporaryIntervalCounter++;
				timerValue.Text = ConvertToTime(num);
				return;
			}
			randomiseSpeeds();
			OpenTcpListener();
			Thread thread = new Thread(continueProcess);
			temporaryIntervalCounter = 0;
			timerValue.Text = "0";
			thread.Name = "continueProcess() Thread";
			thread.Start();
		}
	}

	private void clearLogButton_Click(object sender, EventArgs e)
	{
		ClearLog();
	}

	public void stopProcess()
	{
		sendEventToTracker(currentTorrent, "&event=stopped");
	}

	public void completedProcess()
	{
		if (sendEventToTracker(currentTorrent, "&event=completed"))
		{
			requestScrapeFromTracker(currentTorrent);
		}
	}

	public void continueProcess()
	{
		if (sendEventToTracker(currentTorrent, ""))
		{
			requestScrapeFromTracker(currentTorrent);
		}
	}

	public void startProcess()
	{
		stopParamsUpdateInProgress = false;
		if (sendEventToTracker(currentTorrent, "&event=started") || checkIgnoreTimeout.Checked)
		{
			updateProcessStarted = true;
			requestScrapeFromTracker(currentTorrent);
		}
	}

	public void deployDefaultValues()
	{
		TorrentInfo torrentInfo = new TorrentInfo(0L, 0L);
		trackerAddress.Text = torrentInfo.tracker;
		shaHash.Text = torrentInfo.hash;
		TorrentClients = TorrentClientsObj.TorrentClients;
		TorrentClientsBox.DataSource = TorrentClients;
		TorrentClientsBox.DisplayMember = "Name";
		comboProxyType.SelectedIndex = 0;
		stopProcessActionBox.SelectedIndex = 0;
		applicationSettings.LoadAppSettings();
		SelectTorrentClientByName("qBittorrent 5.1.0");
	}

	private ProxyInfo GetCurrentProxy()
	{
		ProxyInfo result = default(ProxyInfo);
		switch (comboProxyType.SelectedIndex)
		{
		case 0:
			result.proxyType = ProxyType.None;
			break;
		case 1:
			result.proxyType = ProxyType.Socks4;
			break;
		case 2:
			result.proxyType = ProxyType.Socks4a;
			break;
		case 3:
			result.proxyType = ProxyType.Socks5;
			break;
		case 4:
			result.proxyType = ProxyType.Http;
			break;
		case 5:
			result.proxyType = ProxyType.HttpDirect;
			break;
		default:
			result.proxyType = ProxyType.None;
			break;
		}
		result.proxyServer = textProxyHost.Text;
		result.proxyPort = ParseValidInt(textProxyPort.Text, 0);
		result.proxyUser = textProxyUser.Text;
		result.proxyPassword = textProxyPass.Text;
		return result;
	}

	private float parseValidFloat(string str, float defVal)
	{
		try
		{
			return float.Parse(str);
		}
		catch (Exception)
		{
			return defVal;
		}
	}

	private long parseValidInt64(string str, long defVal)
	{
		try
		{
			return long.Parse(str);
		}
		catch (Exception)
		{
			return defVal;
		}
	}

	private int ParseValidInt(string str, int defVal)
	{
		try
		{
			return int.Parse(str);
		}
		catch (Exception)
		{
			return defVal;
		}
	}

	private TorrentInfo getCurrentTorrent()
	{
		TorrentInfo result = new TorrentInfo(0L, 0L);
		Uri trackerUri;
		try
		{
			trackerUri = new Uri(trackerAddress.Text);
		}
		catch (Exception ex)
		{
			AddLogLine(ex.Message);
			return result;
		}
		result.tracker = trackerAddress.Text;
		result.trackerUri = trackerUri;
		result.trackers = GetTorrentTrackers();
		result.hash = shaHash.Text;
		result.uploadRate = (long)(parseValidFloat(uploadRate.Text, 50f) * 1024f);
		result.downloadRate = (long)(parseValidFloat(downloadRate.Text, 10f) * 1024f);
		result.interval = ParseValidInt(interval.Text, 1800);
		interval.Text = result.interval.ToString();
		float num = parseValidFloat(fileSize.Text, 0f);
		if (num < 0f || num > 100f)
		{
			AddLogLine("Finished value is invalid: " + fileSize.Text + ",assuming 0 as default value");
			num = 0f;
		}
		if (num >= 100f)
		{
			seedMode = true;
			num = 100f;
		}
		fileSize.Text = num.ToString();
		if (currentTorrentFile != null)
		{
			if (num == 0f)
			{
				result.totalsize = currentTorrentFile.totalLength;
			}
			else if (num == 100f)
			{
				result.totalsize = 0L;
			}
			else
			{
				result.totalsize = (long)((float)currentTorrentFile.totalLength * (100f - num) / 100f);
			}
		}
		else
		{
			result.totalsize = 0L;
		}
		result.left = result.totalsize;
		result.filename = torrentFile.Text;
		result.numberOfPeers = getValueDefault(customPeersNum.Text, currentClient.NumwantInitialValue.ToString());
		result.port = getValueDefault(customPort.Text, result.port);
		result.key = getValueDefault(customKey.Text, currentClient.Key);
		result.peerID = getValueDefault(customPeerID.Text, currentClient.PeerID);
		return result;
	}

	private string[] GetTorrentTrackers()
	{
		ArrayList arrayList = new ArrayList();
		AddTracker(arrayList, trackerAddress.Text);
		if (currentTorrentFile != null && currentTorrentFile.Data.Contains("announce-list"))
		{
			ValueList valueList = (ValueList)currentTorrentFile.Data["announce-list"];
			foreach (object tier in valueList)
			{
				if (tier is ValueList tierList)
				{
					foreach (object item in tierList)
					{
						AddTracker(arrayList, BEncode.String((BEncodeValue)item));
					}
				}
				else if (tier is BEncodeValue value)
				{
					AddTracker(arrayList, BEncode.String(value));
				}
			}
		}
		string[] array = new string[arrayList.Count];
		arrayList.CopyTo(array);
		return array;
	}

	private static void AddTracker(ArrayList trackers, string tracker)
	{
		if (string.IsNullOrEmpty(tracker))
		{
			return;
		}
		foreach (string item in trackers)
		{
			if (string.Equals(item, tracker, StringComparison.OrdinalIgnoreCase))
			{
				return;
			}
		}
		trackers.Add(tracker);
	}

	public string HashUrlEncode(string decoded, TorrentClient _currentClient)
	{
		StringBuilder stringBuilder = new StringBuilder();
		RandomStringGenerator randomStringGenerator = new RandomStringGenerator();
		try
		{
			for (int i = 0; i < decoded.Length; i += 2)
			{
				char value = (char)Convert.ToUInt16(decoded.Substring(i, 2), 16);
				stringBuilder.Append(value);
			}
		}
		catch (Exception ex)
		{
			AddLogLine(ex.ToString());
		}
		return randomStringGenerator.urlEncode(stringBuilder.ToString(), _currentClient.UrlEncodingExceptions, _currentClient.HashUpperCase, !_currentClient.HashUpperCase);
	}

	public string getScrapeUrlString(TorrentInfo torrentInfo)
	{
		string text = "";
		text = torrentInfo.tracker;
		int num = text.LastIndexOf("/");
		if (text.Substring(num + 1, 8).ToLower() != "announce")
		{
			return "";
		}
		text = text.Substring(0, num + 1) + "scrape" + text.Substring(num + 9);
		string text2 = HashUrlEncode(torrentInfo.hash, currentClient);
		if (text.Contains("?"))
		{
			if (!text.EndsWith("&"))
			{
				text += "&";
			}
		}
		else
		{
			text += "?";
		}
		return text + "info_hash=" + text2;
	}

	private long RoundByDenominator(long value, long denominator)
	{
		return denominator * (value / denominator);
	}

	public string getUrlString(TorrentInfo torrentInfo, string eventType)
	{
		return getUrlString(torrentInfo, eventType, torrentInfo.tracker);
	}

	public string getUrlString(TorrentInfo torrentInfo, string eventType, string tracker)
	{
		string newValue = "0";
		if (torrentInfo.uploaded > 0)
		{
			torrentInfo.uploaded = RoundByDenominator(torrentInfo.uploaded, 16384L);
			newValue = Convert.ToString(torrentInfo.uploaded);
		}
		string newValue2 = "0";
		if (torrentInfo.downloaded > 0)
		{
			torrentInfo.downloaded = RoundByDenominator(torrentInfo.downloaded, 16L);
			newValue2 = Convert.ToString(torrentInfo.downloaded);
		}
		if (torrentInfo.left > 0)
		{
			torrentInfo.left = torrentInfo.totalsize - torrentInfo.downloaded;
		}
		string newValue3 = torrentInfo.left.ToString();
		string newValue4 = torrentInfo.key.ToString();
		string newValue5 = torrentInfo.port.ToString();
		string peerID = torrentInfo.peerID;
		string text = "";
		text = tracker;
		if (text.Contains("?"))
		{
			if (!text.EndsWith("&"))
			{
				text += "&";
			}
		}
		else
		{
			text += "?";
		}
		text += currentClient.Query;
		text = text.Replace("{infohash}", HashUrlEncode(torrentInfo.hash, currentClient));
		text = text.Replace("{peerid}", peerID);
		text = text.Replace("{port}", newValue5);
		text = text.Replace("{uploaded}", newValue);
		text = text.Replace("{downloaded}", newValue2);
		text = text.Replace("{left}", newValue3);
		text = text.Replace("{event}", eventType);
		if (torrentInfo.numberOfPeers == "0" && !eventType.ToLower().Contains("stopped"))
		{
			torrentInfo.numberOfPeers = currentClient.NumwantInitialValue.ToString();
		}
		text = ((!currentClient.NumwantRandomize) ? text.Replace("{numwant}", torrentInfo.numberOfPeers) : text.Replace("{numwant}", getRandomizedNumwant(int.Parse(torrentInfo.numberOfPeers), currentClient, eventType)));
		text = text.Replace("{key}", newValue4);
		return text.Replace("{localip}", ipAddress);
	}

	private string getRandomizedNumwant(int _currentNumwant, TorrentClient _currentClient, string eventType)
	{
		if (eventType.ToLower().Contains("started") || eventType.ToLower().Contains("stopped"))
		{
			return _currentNumwant.ToString();
		}
		int numwantRadius = _currentClient.NumwantRadius;
		return (_currentNumwant + random.Next(0, numwantRadius * 2) - numwantRadius).ToString();
	}

	public void updateCounters(TorrentInfo torrentInfo)
	{
		if (uploadCount.InvokeRequired)
		{
			SetCountersCallback method = updateCounters;
			Invoke(method, torrentInfo);
			return;
		}
		Random random = new Random();
		uploadCount.Text = FormatFileSize(torrentInfo.uploaded);
		if (torrentInfo.uploadRate > 0)
		{
			long num = torrentInfo.uploadRate + random.Next(10240) - 5120;
			if (num < 0)
			{
				num = 0L;
			}
			torrentInfo.uploaded += num;
		}
		downloadCount.Text = FormatFileSize(torrentInfo.downloaded);
		if (!seedMode && torrentInfo.downloadRate > 0)
		{
			long num2 = torrentInfo.downloadRate + random.Next(10240) - 5120;
			if (num2 < 0)
			{
				num2 = 0L;
			}
			torrentInfo.downloaded += num2;
			torrentInfo.left = torrentInfo.totalsize - torrentInfo.downloaded;
		}
		if (torrentInfo.left <= 0)
		{
			torrentInfo.downloaded = torrentInfo.totalsize;
			torrentInfo.left = 0L;
			torrentInfo.downloadRate = 0L;
			if (!seedMode)
			{
				currentTorrent = torrentInfo;
				seedMode = true;
				temporaryIntervalCounter = 0;
				Thread thread = new Thread(completedProcess);
				thread.Name = "completedProcess() Thread";
				thread.Start();
			}
		}
		currentTorrent = torrentInfo;
		if (torrentInfo.totalsize == 0)
		{
			fileSize.Text = "100";
			return;
		}
		float num3 = (float)(currentTorrentFile.totalLength - torrentInfo.left) / (float)currentTorrentFile.totalLength * 100f;
		fileSize.Text = ((num3 >= 100f) ? "100" : num3.ToString("F2"));
	}

	public void requestScrapeFromTracker(TorrentInfo torrentInfo)
	{
		if (!checkRequestScrap.Checked || scrapStatsUpdated)
		{
			return;
		}
		try
		{
			string scrapeUrlString = getScrapeUrlString(torrentInfo);
			if (scrapeUrlString == "")
			{
				AddLogLine("This tracker doesnt seem to support scrape");
				return;
			}
			Uri reqUri = new Uri(scrapeUrlString);
			TrackerResponse trackerResponse = MakeWebRequestEx(reqUri);
			if (trackerResponse == null || trackerResponse.Dict == null)
			{
				return;
			}
			string text = BEncode.String(trackerResponse.Dict["failure reason"]);
			if (text.Length > 0)
			{
				AddLogLine("Tracker Error: " + text);
				return;
			}
			AddLogLine("---------- Scrape Info -----------");
			ValueDictionary valueDictionary = (ValueDictionary)trackerResponse.Dict["files"];
			string key = Encoding.GetEncoding(28591).GetString(currentTorrentFile.InfoHash);
			if (valueDictionary[key].GetType() == typeof(ValueDictionary))
			{
				ValueDictionary valueDictionary2 = (ValueDictionary)valueDictionary[key];
				AddLogLine("complete: " + BEncode.String(valueDictionary2["complete"]));
				AddLogLine("downloaded: " + BEncode.String(valueDictionary2["downloaded"]));
				AddLogLine("incomplete: " + BEncode.String(valueDictionary2["incomplete"]));
				updateScrapStats(BEncode.String(valueDictionary2["complete"]), BEncode.String(valueDictionary2["incomplete"]), BEncode.String(valueDictionary2["downloaded"]));
				decimal num = ParseValidInt(BEncode.String(valueDictionary2["incomplete"]), 0);
				if (textStopMinLeecher.Enabled && num < textStopMinLeecher.Value)
				{
					AddLogLine("Min number of leechers reached...setting Upload speed to 0");
					updateTextBox(uploadRate, "0");
				}
			}
			else
			{
				AddLogLine("Scrape returned : '" + ((ValueString)valueDictionary[key]).String + "'");
			}
		}
		catch (Exception ex)
		{
			AddLogLine("Error: " + ex.Message);
		}
	}

	private void updateTextBox(TextBox textbox, string text)
	{
		if (textbox.InvokeRequired)
		{
			updateTextBoxCallback method = updateTextBox;
			Invoke(method, textbox, text);
		}
		else
		{
			textbox.Text = text;
		}
	}

	private void updateTextBox(Label textbox, string text)
	{
		if (textbox.InvokeRequired)
		{
			updateLabelCallback method = updateTextBox;
			Invoke(method, textbox, text);
		}
		else
		{
			textbox.Text = text;
		}
	}

	private void updateScrapStats(string seedStr, string leechStr, string finishedStr)
	{
		updateTextBox(seedLabel, lclzManager.TranslateMessageInner("Form1", "seedLabel", "Seeders: ") + seedStr);
		updateTextBox(leechLabel, lclzManager.TranslateMessageInner("Form1", "leechLabel", "Leechers: ") + leechStr);
		scrapStatsUpdated = true;
	}

	public bool sendEventToTracker(TorrentInfo torrentInfo, string eventType)
	{
		scrapStatsUpdated = false;
		currentTorrent = torrentInfo;
		currentTorrent.uploadedLast = currentTorrent.uploaded;
		currentTorrent.downloadedLast = currentTorrent.downloaded;
		currentTorrent.leftLast = currentTorrent.left;
		ValueDictionary valueDictionary = null;
		string trackerError = "";
		string[] array = torrentInfo.trackers;
		if (array == null || array.Length == 0)
		{
			array = new string[1] { torrentInfo.tracker };
		}
		foreach (string text in array)
		{
			try
			{
				currentTorrent.tracker = text;
				currentTorrent.trackerUri = new Uri(text);
				string urlString = getUrlString(torrentInfo, eventType, text);
				Uri reqUri = new Uri(urlString);
				TrackerResponse trackerResponse = MakeWebRequestEx(reqUri);
				if (trackerResponse == null)
				{
					trackerError = "No tracker response";
					AddLogLine("Tracker failed: " + text + " - " + trackerError);
					continue;
				}
				if (trackerResponse.IsHttpError)
				{
					trackerError = "HTTP " + trackerResponse.StatusCode;
					AddLogLine("Tracker failed: " + text + " - " + trackerError);
					continue;
				}
				if (trackerResponse.Dict == null)
				{
					trackerError = "Failed to decode tracker response";
					AddLogLine("Tracker failed: " + text + " - " + trackerError);
					continue;
				}
				valueDictionary = trackerResponse.Dict;
				string failureReason = BEncode.String(valueDictionary["failure reason"]);
				if (failureReason.Length > 0)
				{
					trackerError = failureReason;
					AddLogLine("Tracker failed: " + text + " - " + trackerError);
					continue;
				}
				if (!string.Equals(trackerAddress.Text, text, StringComparison.OrdinalIgnoreCase))
				{
					trackerAddress.Text = text;
				}
				PeerList peerList = new PeerList();
				foreach (string key in valueDictionary.Keys)
				{
					if (key != "failure reason" && key != "peers")
					{
						AddLogLine(key + ": " + BEncode.String(valueDictionary[key]));
					}
				}
				if (valueDictionary.Contains("peers"))
				{
					haveInitialPeers = true;
					string text3 = "";
					if (valueDictionary["peers"] is ValueString)
					{
						text3 = BEncode.String(valueDictionary["peers"]);
						Encoding encoding = Encoding.GetEncoding(28591);
						byte[] bytes = encoding.GetBytes(text3);
						BinaryReader binaryReader = new BinaryReader(new MemoryStream(encoding.GetBytes(text3)));
						for (int i = 0; i < bytes.Length; i += 6)
						{
							peerList.Add(new Peer(binaryReader.ReadBytes(4), binaryReader.ReadInt16()));
						}
						binaryReader.Close();
						AddLogLine("peers: " + peerList.ToString());
					}
					else if (valueDictionary["peers"] is ValueList)
					{
						text3 = "";
						ValueList valueList = (ValueList)valueDictionary["peers"];
						foreach (object item in valueList)
						{
							if (item is ValueDictionary)
							{
								ValueDictionary valueDictionary2 = (ValueDictionary)item;
								peerList.Add(new Peer(BEncode.String(valueDictionary2["ip"]), BEncode.String(valueDictionary2["port"]), BEncode.String(valueDictionary2["peer id"])));
							}
						}
						AddLogLine("peers: " + peerList.ToString());
					}
					else
					{
						text3 = BEncode.String(valueDictionary["peers"]);
						AddLogLine("peers(x): " + text3);
					}
					if (checkSavePeers.Checked)
					{
						PeerListUtils.SavePeerList(this, peerList, currentTorrent.filename);
					}
				}
				if (valueDictionary.Contains("interval"))
				{
					updateInterval(BEncode.String(valueDictionary["interval"]));
				}
				if (!eventType.ToLower().Contains("stopped") && valueDictionary.Contains("complete") && valueDictionary.Contains("incomplete"))
				{
					updateScrapStats(BEncode.String(valueDictionary["complete"]), BEncode.String(valueDictionary["incomplete"]), "");
					decimal num = ParseValidInt(BEncode.String(valueDictionary["incomplete"]), 0);
					if (textStopMinLeecher.Enabled && num < textStopMinLeecher.Value)
					{
						AddLogLine("Min number of leechers reached...setting Upload speed to 0");
						updateTextBox(uploadRate, "0");
					}
				}
				else if (!eventType.ToLower().Contains("stopped") && textStopMinLeecher.Enabled && (decimal)peerList.Count < textStopMinLeecher.Value)
				{
					AddLogLine("No peers returned from tracker...setting Upload speed to 0");
					updateTextBox(uploadRate, "0");
				}
				return true;
			}
			catch (Exception ex)
			{
				trackerError = ex.Message;
				AddLogLine("Tracker failed: " + text + " - " + trackerError);
			}
		}
		if (!checkIgnoreFailureReason.Checked && !string.IsNullOrEmpty(trackerError))
		{
			if (HandleTrackerBackoff(trackerError))
			{
				return true;
			}
			stopTimerAndCounters();
			ShowDarkMessage(trackerError, "Tracker Response", MessageBoxIcon.Hand);
			return false;
		}
		updateInterval("1200");
		haveInitialPeers = true;
		return checkIgnoreFailureReason.Checked;
	}

	private bool HandleTrackerBackoff(string trackerError)
	{
		Match match = Regex.Match(trackerError, "announce too soon.*?(\\d+)\\s+seconds?", RegexOptions.IgnoreCase);
		if (!match.Success || !int.TryParse(match.Groups[1].Value, out int result))
		{
			return false;
		}
		int num = Math.Max(result + 10, 60);
		updateInterval(num.ToString());
		temporaryIntervalCounter = 0;
		AddLogLine("Tracker requested announce backoff. Waiting " + num + " seconds before next announce.");
		return true;
	}

	private void ShowDarkMessage(string message, string title, MessageBoxIcon icon)
	{
		if (base.InvokeRequired)
		{
			Invoke(new Action<string, string, MessageBoxIcon>(ShowDarkMessage), message, title, icon);
			return;
		}
		DarkTheme.ShowMessage(this, message, title, icon);
	}

	public void ShowMessage(string message, string title)
	{
		if (base.InvokeRequired)
		{
			ShowMessageCallback method = ShowMessage;
			Invoke(method, message, title);
		}
		else
		{
			DarkTheme.ShowMessage(this, message, title, MessageBoxIcon.Information);
		}
	}

	public void AddLogLine(string logLine)
	{
		if (logWindow.InvokeRequired)
		{
			SetTextCallback method = AddLogLine;
			Invoke(method, logLine);
		}
		else if (ShouldShowLogLine(logLine))
		{
			try
			{
				DateTime now = DateTime.Now;
				string text = "[" + $"{now:hh:mm:ss}" + "]";
				logWindow.AppendText(text + " " + logLine + "\r\n");
				logWindow.ScrollToCaret();
			}
			catch (Exception)
			{
			}
		}
	}

	private bool ShouldShowLogLine(string logLine)
	{
		if (checkLogEnabled.Checked)
		{
			return true;
		}
		if (string.IsNullOrWhiteSpace(logLine))
		{
			return false;
		}
		string text = logLine.TrimStart();
		return text.StartsWith("GET ", StringComparison.OrdinalIgnoreCase)
			|| text.StartsWith("POST ", StringComparison.OrdinalIgnoreCase)
			|| text.StartsWith("HTTP/", StringComparison.OrdinalIgnoreCase)
			|| text.StartsWith("----------- Sending Command to Tracker", StringComparison.OrdinalIgnoreCase)
			|| text.StartsWith("----------- Tracker Response", StringComparison.OrdinalIgnoreCase)
			|| text.StartsWith("*** Failed to decode tracker response", StringComparison.OrdinalIgnoreCase)
			|| text.IndexOf("failure reason", StringComparison.OrdinalIgnoreCase) >= 0
			|| text.IndexOf("tracker response is empty", StringComparison.OrdinalIgnoreCase) >= 0;
	}

	public void ClearLog()
	{
		logWindow.Clear();
	}

	private IProxyClient CreateProxyClient()
	{
		return CreateProxyClient(currentProxy);
	}

	private IProxyClient CreateProxyClient(ProxyInfo _currentProxy)
	{
		ProxyClientFactory proxyClientFactory = new ProxyClientFactory();
		return proxyClientFactory.CreateProxyClient(_currentProxy.proxyType, _currentProxy.proxyServer, _currentProxy.proxyPort);
	}

	private TcpClient CreateProxyConnection(IProxyClient proxy, string host, int port)
	{
		TcpClient tcpClient = null;
		if (proxy != null)
		{
			if (UseLocalIpBinding)
			{
				int num = random.Next(1025, 65535);
				AddLogLine("Binding socket to IP address: " + ipAddress + ":" + num);
				proxy.BindIpEndpoint = new IPEndPoint(IPAddress.Parse(ipAddress), num);
			}
			tcpClient = proxy.CreateConnection(host, port);
		}
		else
		{
			tcpClient = new TcpClient();
			if (UseLocalIpBinding)
			{
				int num2 = random.Next(1025, 65535);
				AddLogLine("Binding socket to IP address: " + ipAddress + ":" + num2);
				tcpClient.Client.Bind(new IPEndPoint(IPAddress.Parse(ipAddress), num2));
			}
			tcpClient.Connect(host, port);
		}
		tcpClient.ReceiveTimeout = TrackerSocketTimeoutMs;
		tcpClient.SendTimeout = TrackerSocketTimeoutMs;
		return tcpClient;
	}

	private TrackerResponse MakeWebRequestExSsl(Uri reqUri)
	{
		TrackerResponse trackerResponse = null;
		try
		{
			string text = reqUri.Host;
			int port = reqUri.Port;
			string pathAndQuery = reqUri.PathAndQuery;
			AddLogLine("----------- Connecting to tracker : " + text + " -------- Port : " + port);
			if (port != 80)
			{
				text = text + ":" + port;
			}
			string text2 = currentClient.Headers.Replace("{host}", text);
			text2 = text2.Replace("{javaver}", JavaInfo.Get().CurrentVersion);
			text2 = text2.Replace("{osver}", OsInfo.Get().ShortVersion);
			string text3 = NormalizeCommand("GET " + pathAndQuery + " " + currentClient.HttpProtocol + "\r\n" + text2 + "\r\n");
			AddLogLine("----------- Sending Command to Tracker --------");
			AddLogLine(text3);
			TrackerSslClient trackerSslClient = new TrackerSslClient(reqUri, this);
			MemoryStream memoryStream = trackerSslClient.ConnectToServer(text3);
			if (memoryStream == null)
			{
				AddLogLine("Error : Tracker Response is empty");
				return null;
			}
			trackerResponse = new TrackerResponse(memoryStream);
			if (trackerResponse.doRedirect)
			{
				return MakeWebRequestExSsl(new Uri(trackerResponse.RedirectionURL));
			}
			AddLogLine("----------- Tracker Response --------");
			AddLogLine(trackerResponse.Headers);
			if (trackerResponse.Dict == null)
			{
				AddLogLine("*** Failed to decode tracker response :");
				AddLogLine(trackerResponse.Body);
			}
			memoryStream.Dispose();
		}
		catch (Exception ex)
		{
			AddLogLine("Exception:" + ex.Message);
		}
		return trackerResponse;
	}

	private TrackerResponse MakeWebRequestEx(Uri reqUri)
	{
		if (reqUri.Scheme.ToLower() == "https")
		{
			return MakeWebRequestExSsl(reqUri);
		}
		Encoding encoding = Encoding.GetEncoding(28591);
		IProxyClient proxyClient = null;
		TrackerResponse trackerResponse = null;
		TcpClient tcpClient = null;
		try
		{
			string text = reqUri.Host;
			int port = reqUri.Port;
			string text2 = reqUri.PathAndQuery;
			proxyClient = CreateProxyClient();
			AddLogLine("----------- Connecting to tracker : " + text + " -------- Port : " + port);
			int num = 0;
			bool flag = false;
			while (num < 5 && !flag)
			{
				try
				{
					tcpClient = CreateProxyConnection(proxyClient, text, port);
					AddLogLine("Local end point=" + tcpClient.Client.LocalEndPoint);
					flag = true;
					AddLogLine("Connected Successfully");
				}
				catch (Exception ex)
				{
					AddLogLine("Exception: " + ex.Message);
					AddLogLine("Failed connection attempt: " + num);
					num++;
					CloseTcpClient(tcpClient);
				}
			}
			if (!flag)
			{
				CloseTcpClient(tcpClient);
				return trackerResponse;
			}
			if (port != 80)
			{
				text = text + ":" + port;
			}
			if (currentProxy.proxyType == ProxyType.HttpDirect)
			{
				text2 = "http://" + text + text2;
			}
			string text3 = currentClient.Headers.Replace("{host}", text);
			text3 = text3.Replace("{javaver}", JavaInfo.Get().CurrentVersion);
			text3 = text3.Replace("{osver}", OsInfo.Get().ShortVersion);
			string text4 = NormalizeCommand("GET " + text2 + " " + currentClient.HttpProtocol + "\r\n" + text3 + "\r\n");
			AddLogLine("----------- Sending Command to Tracker --------");
			AddLogLine(text4);
			tcpClient.Client.Send(encoding.GetBytes(text4));
			MemoryStream memoryStream = new MemoryStream();
			try
			{
				byte[] array = new byte[32768];
				AddLogLine("Waiting for tracker response...");
				int num2 = 0;
				while (true)
				{
					AddLogLine("Receiving...");
					int num3 = tcpClient.Client.Receive(array);
					if (num3 == 0)
					{
						AddLogLine("End of tracker response.");
						break;
					}
					AddLogLine("Received tracker response, length = " + num3);
					memoryStream.Write(array, 0, num3);
					if (isEndOfHttpMessage(array, num3, num2))
					{
						AddLogLine("End of HTTP response.");
						break;
					}
					num2++;
				}
				if (memoryStream.Length == 0)
				{
					AddLogLine("Error : Tracker Response is empty");
					memoryStream.Dispose();
					memoryStream = null;
					CloseTcpClient(tcpClient);
					return null;
				}
				trackerResponse = new TrackerResponse(memoryStream);
				if (trackerResponse.doRedirect)
				{
					memoryStream.Dispose();
					memoryStream = null;
					CloseTcpClient(tcpClient);
					return MakeWebRequestEx(new Uri(trackerResponse.RedirectionURL));
				}
				AddLogLine("----------- Tracker Response --------");
				AddLogLine(trackerResponse.Headers);
				if (trackerResponse.Dict == null)
				{
					AddLogLine("*** Failed to decode tracker response :");
					AddLogLine(trackerResponse.Body);
				}
				memoryStream.Dispose();
				memoryStream = null;
			}
			catch (Exception ex2)
			{
				AddLogLine(Environment.NewLine + ex2.Message);
				trackerResponse = null;
				if (memoryStream != null)
				{
					memoryStream.Dispose();
					memoryStream = null;
				}
			}
		}
		catch (Exception ex3)
		{
			AddLogLine("Exception:" + ex3.Message);
		}
		CloseTcpClient(tcpClient);
		return trackerResponse;
	}

	private bool isEndOfHttpMessage(byte[] data, int dataLen, int chunkNumber)
	{
		if (dataLen < 4)
		{
			return true;
		}
		string input = Encoding.ASCII.GetString(data, 0, dataLen);
		string[] array = Regex.Split(input, "\r\n\r\n", RegexOptions.None);
		if (chunkNumber == 0 && (array.Length <= 1 || string.IsNullOrEmpty(array[1])))
		{
			return false;
		}
		if (data[dataLen - 1] == 10 && data[dataLen - 2] == 13 && data[dataLen - 3] == 10 && data[dataLen - 4] == 13)
		{
			return true;
		}
		Regex regex = new Regex("Content-Length: ([0-9]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		try
		{
			Match match = regex.Match(input);
			if (match.Success && array.Length > 1)
			{
				int contentLength = int.Parse(match.Groups[1].ToString());
				int headerLength = Encoding.ASCII.GetByteCount(array[0]) + 4;
				if (dataLen - headerLength >= contentLength)
				{
					return true;
				}
			}
		}
		catch (Exception ex)
		{
			AddLogLine("Error: " + ex.Message);
		}
		return false;
	}

	private void CloseTcpClient(TcpClient tcpClient)
	{
		if (tcpClient != null)
		{
			if (tcpClient.Client != null)
			{
				tcpClient.Client.Close();
			}
			tcpClient.Close();
		}
	}

	private string NormalizeCommand(string cmd)
	{
		if (cmd.EndsWith("\r\n\r\n"))
		{
			return cmd;
		}
		return cmd + "\r\n";
	}

	public static string ToHexString(byte[] bytes)
	{
		char[] array = new char[16]
		{
			'0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
			'A', 'B', 'C', 'D', 'E', 'F'
		};
		char[] array2 = new char[bytes.Length * 2];
		for (int i = 0; i < bytes.Length; i++)
		{
			int num = bytes[i];
			array2[i * 2] = array[num >> 4];
			array2[i * 2 + 1] = array[num & 0xF];
		}
		return new string(array2);
	}

	public bool loadTorrentFileInfo(string torrentFilePath, bool loadSettings)
	{
		try
		{
			if (string.IsNullOrEmpty(torrentFilePath))
			{
				return false;
			}
			AddLogLine("Torrent Path=" + torrentFilePath);
			FileInfo fileInfo = new FileInfo(torrentFilePath);
			if (fileInfo.Exists)
			{
				currentTorrentFile = new Torrent(torrentFilePath);
				torrentFile.Text = torrentFilePath;
				trackerAddress.Text = currentTorrentFile.Announce;
				shaHash.Text = ToHexString(currentTorrentFile.InfoHash);
				torrentSize.Text = FormatFileSize(currentTorrentFile.totalLength);
				AddLogLine("Torrent size:" + currentTorrentFile.totalLength);
				if (loadSettings)
				{
					applicationSettings.LoadTorrentSettings(shaHash.Text);
				}
				RefreshTorrentFileList(torrentFilePath);
				return true;
			}
			AddLogLine("Torrent with this path doesnt exists.Please enter valid full path of the torrent.");
			return false;
		}
		catch (Exception ex)
		{
			AddLogLine(ex.ToString());
			return false;
		}
	}

	private void Form1_DragDrop(object sender, DragEventArgs e)
	{
		string[] array = (string[])e.Data.GetData(DataFormats.FileDrop);
		if (array == null)
		{
			array = (string[])e.Data.GetData("System.String[]", autoConvert: true);
			if (array == null)
			{
				return;
			}
		}
		loadTorrentFileInfo(array[0], loadSettings: true);
	}

	private void Form1_DragEnter(object sender, DragEventArgs e)
	{
		if (e.Data.GetDataPresent(DataFormats.FileDrop) || e.Data.GetDataPresent(DataFormats.Text) || e.Data.GetFormats().ToString().Equals("System.String[]"))
		{
			e.Effect = DragDropEffects.All;
		}
		else
		{
			e.Effect = DragDropEffects.None;
		}
	}

	private void browseButton_Click(object sender, EventArgs e)
	{
		try
		{
			RuntimeLog.Write("Opening native torrent file picker");
			DarkTheme.EnableDarkApplicationMode();
			if (!string.IsNullOrWhiteSpace(torrentFile.Text) && File.Exists(torrentFile.Text))
			{
				openFileDialog1.InitialDirectory = Path.GetDirectoryName(torrentFile.Text);
				openFileDialog1.FileName = Path.GetFileName(torrentFile.Text);
			}
			if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
			{
				RuntimeLog.Write("Torrent selected: " + openFileDialog1.FileName);
				loadTorrentFileInfo(openFileDialog1.FileName, loadSettings: true);
			}
		}
		catch (Exception ex)
		{
			RuntimeLog.WriteException("Torrent file picker failed", ex);
			MessageBox.Show(ex.Message, "Open", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}

	private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
	{
		string[] fileNames = openFileDialog1.FileNames;
		loadTorrentFileInfo(fileNames[0], loadSettings: true);
	}

	public TorrentClient getCurrentClient()
	{
		if (TorrentClientsBox.SelectedItem is TorrentClient client && !client.IsDivider)
		{
			return client;
		}
		return null;
	}

	private void SelectTorrentClientByName(string name)
	{
		for (int i = 0; i < TorrentClientsBox.Items.Count; i++)
		{
			if (TorrentClientsBox.Items[i] is TorrentClient client && client.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
			{
				TorrentClientsBox.SelectedIndex = i;
				return;
			}
		}
	}

	public string getValueDefault(string value, string defValue)
	{
		if (value == "")
		{
			return defValue;
		}
		return value;
	}

	private void linkWebsiteClicked(object sender, LinkLabelLinkClickedEventArgs e)
	{
		OpenLink(linkWebsite);
	}

	private void linkEmail1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
	{
		Process.Start("mailto:" + linkEmail1.Text);
	}

	private void linkForums_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
	{
		OpenLink(linkForums);
	}

	private void OpenLink(LinkLabel link)
	{
		string url = link.Tag as string ?? link.Text;
		Process.Start(new ProcessStartInfo(url)
		{
			UseShellExecute = true
		});
	}

	private void restoreToolStripMenuItem_Click(object sender, EventArgs e)
	{
		winRestore();
	}

	private void exitToolStripMenuItem_Click(object sender, EventArgs e)
	{
		ExitRatioMaster();
	}

	private void closeButton_Click(object sender, EventArgs e)
	{
		ExitRatioMaster();
	}

	private bool ExitRatioMaster()
	{
		if (updateProcessStarted)
		{
			string caption = lclzManager.TranslateMessage("confirmCloseTitle", "Confirmation");
			string text = lclzManager.TranslateMessage("confirmCloseMessage", "Torrent update is in process. Do you really want to exit RatioMaster?");
			if (MessageBox.Show(text, caption, MessageBoxButtons.YesNo) != DialogResult.Yes)
			{
				return false;
			}
			StopButton_Click(null, null);
		}
		applicationSettings.SaveAppSettings();
		Application.Exit();
		return true;
	}

	private void Form1_Move(object sender, EventArgs e)
	{
		if (this == null)
		{
			return;
		}
		if (base.WindowState == FormWindowState.Minimized)
		{
			if (checkMinimizeToTray.Checked)
			{
				Hide();
				trayIcon.Visible = true;
			}
		}
		else
		{
			Show();
		}
	}

	private void winRestore()
	{
		if (base.WindowState == FormWindowState.Minimized)
		{
			Show();
			base.WindowState = FormWindowState.Normal;
			trayIcon.Visible = false;
		}
		Activate();
		Focus();
	}

	private void trayIcon_BalloonTipShown(object sender, EventArgs e)
	{
		trayIconBaloonIsUp = true;
	}

	private void trayIcon_BalloonTipClosed(object sender, EventArgs e)
	{
		trayIconBaloonIsUp = false;
	}

	private void trayIcon_BalloonTipClicked(object sender, EventArgs e)
	{
		trayIconBaloonIsUp = false;
	}

	private void trayIcon_MouseMove(object sender, MouseEventArgs e)
	{
		if (checkShowTrayBaloon.Checked && !trayIconBaloonIsUp)
		{
			if (currentTorrent.trackerUri != null)
			{
				trayIcon.BalloonTipText = currentTorrent.trackerUri.Host + "\r\n";
			}
			else
			{
				trayIcon.BalloonTipText = "";
			}
			trayIcon.BalloonTipText += lclzManager.TranslateMessageWithParams("ballonUploaded", "Uploaded: {0} \r\n", FormatFileSize(currentTorrent.uploaded));
			trayIcon.BalloonTipText += lclzManager.TranslateMessageWithParams("ballonDownloaded", "Downloaded: {0} \r\n", FormatFileSize(currentTorrent.downloaded));
			trayIcon.BalloonTipText += lclzManager.TranslateMessageWithParams("ballonUpdateIn", "Update in: {0} \r\n", timerValue.Text);
			trayIcon.BalloonTipText += lclzManager.TranslateMessageWithParams("ballonTotalTime", "Total time: {0} \r\n", totalRunningTime.Text);
			trayIcon.ShowBalloonTip(1000);
		}
	}

	private void trayIcon_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button != MouseButtons.Right)
		{
			winRestore();
		}
	}

	private void uploadRate_TextChanged(object sender, EventArgs e)
	{
		currentTorrent.uploadRate = (long)(parseValidFloat(uploadRate.Text, 50f) * 1024f);
		UpdateSpeedWarning();
	}

	private void Form1_FormClosing(object sender, FormClosingEventArgs e)
	{
		e.Cancel = !ExitRatioMaster();
	}

	private void downloadRate_TextChanged(object sender, EventArgs e)
	{
		currentTorrent.downloadRate = (long)(parseValidFloat(downloadRate.Text, 10f) * 1024f);
		UpdateSpeedWarning();
	}

	private void UpdateSpeedWarning()
	{
		float uploadSpeed = parseValidFloat(uploadRate.Text, 0f);
		float downloadSpeed = parseValidFloat(downloadRate.Text, 0f);
		speedWarningLabel.Visible = uploadSpeed > 20000f || downloadSpeed > 20000f;
	}

	public string FormatFileSize(long fileSize)
	{
		if (fileSize < 0)
		{
			throw new ArgumentOutOfRangeException("FileSize");
		}
		if (fileSize >= 1073741824)
		{
			return $"{(double)fileSize / 1073741824.0:########0.00} GB";
		}
		if (fileSize >= 1048576)
		{
			return $"{(double)fileSize / 1048576.0:####0.00} MB";
		}
		if (fileSize >= 1024)
		{
			return $"{(double)fileSize / 1024.0:####0.00} KB";
		}
		return $"{fileSize} bytes";
	}

	private void applyStopSettingsButton_Click(object sender, EventArgs e)
	{
		stopParamsUpdateInProgress = false;
		applyStopSettingsButton.Visible = false;
	}

	private void stopProcessActionBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (updateProcessStarted)
		{
			stopParamsUpdateInProgress = true;
			applyStopSettingsButton.Visible = true;
		}
		AdjustProcessActionGui();
	}

	private void AdjustProcessActionGui()
	{
		int selectedIndex = 0;
		switch (stopProcessActionBox.SelectedIndex)
		{
		case 0:
			stopProcessValue.Visible = false;
			stopProcessUnitsBox.Visible = false;
			applyStopSettingsButton.Visible = false;
			break;
		case 1:
			stopProcessValue.Visible = true;
			stopProcessUnitsBox.Visible = true;
			if (stopProcessUnitsBox.SelectedIndex >= 0)
			{
				selectedIndex = stopProcessUnitsBox.SelectedIndex;
			}
			stopProcessUnitsBox.DataSource = Enum.GetValues(typeof(SizeUnits));
			stopProcessUnitsBox.SelectedIndex = selectedIndex;
			break;
		case 2:
			stopProcessValue.Visible = true;
			stopProcessUnitsBox.Visible = true;
			if (stopProcessUnitsBox.SelectedIndex >= 0)
			{
				selectedIndex = stopProcessUnitsBox.SelectedIndex;
			}
			stopProcessUnitsBox.DataSource = Enum.GetValues(typeof(SizeUnits));
			stopProcessUnitsBox.SelectedIndex = selectedIndex;
			break;
		case 3:
			stopProcessValue.Visible = true;
			stopProcessUnitsBox.Visible = true;
			if (stopProcessUnitsBox.SelectedIndex >= 0)
			{
				selectedIndex = stopProcessUnitsBox.SelectedIndex;
			}
			stopProcessUnitsBox.DataSource = null;
			stopProcessUnitsBox.Items.Clear();
			stopProcessUnitsBox.Items.Add(lclzManager.TranslateMessage("stopProcessUnits1", "Seconds"));
			stopProcessUnitsBox.Items.Add(lclzManager.TranslateMessage("stopProcessUnits2", "Minutes"));
			stopProcessUnitsBox.Items.Add(lclzManager.TranslateMessage("stopProcessUnits3", "Hours"));
			stopProcessUnitsBox.SelectedIndex = selectedIndex;
			break;
		}
	}

	private bool IsStopProcessCondition()
	{
		if (stopParamsUpdateInProgress)
		{
			return false;
		}
		long num = (long)(parseValidFloat(stopProcessValue.Text, 0f) * (float)GetStopProcessUnitsMultiplyer());
		bool result = false;
		switch (stopProcessActionBox.SelectedIndex)
		{
		case 0:
			result = false;
			break;
		case 1:
			if (num <= 0)
			{
				result = false;
			}
			result = ((currentTorrent.uploaded > num) ? true : false);
			break;
		case 2:
			if (num <= 0)
			{
				result = false;
			}
			result = ((currentTorrent.downloaded > num) ? true : false);
			break;
		case 3:
			if (num <= 0)
			{
				result = false;
			}
			result = ((totalRunningTimeCounter > num) ? true : false);
			break;
		}
		return result;
	}

	private long GetStopProcessUnitsMultiplyer()
	{
		long result = 0L;
		switch (stopProcessActionBox.SelectedIndex)
		{
		case 0:
			result = 0L;
			break;
		case 1:
		case 2:
			switch (stopProcessUnitsBox.SelectedIndex)
			{
			case 0:
				result = 1024L;
				break;
			case 1:
				result = 1048576L;
				break;
			case 2:
				result = 1073741824L;
				break;
			}
			break;
		case 3:
			switch (stopProcessUnitsBox.SelectedIndex)
			{
			case 0:
				result = 1L;
				break;
			case 1:
				result = 60L;
				break;
			case 2:
				result = 3600L;
				break;
			}
			break;
		}
		return result;
	}

	private void saveLogButton_Click(object sender, EventArgs e)
	{
		using SaveFileDialog saveFileDialog = new SaveFileDialog();
		saveFileDialog.Filter = "Log files (*.log)|*.log|All files (*.*)|*.*";
		saveFileDialog.FilterIndex = 2;
		saveFileDialog.RestoreDirectory = true;
		saveFileDialog.InitialDirectory = Application.StartupPath;
		saveFileDialog.FileName = "ratiomaster.log";
		Stream stream;
		DarkTheme.EnableDarkApplicationMode();
		if (saveFileDialog.ShowDialog() == DialogResult.OK && (stream = saveFileDialog.OpenFile()) != null)
		{
			using StreamWriter streamWriter = new StreamWriter(stream);
			streamWriter.Write(logWindow.Text);
		}
	}

	private void TorrentClientsBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		currentClient = getCurrentClient();
		if (currentClient == null)
		{
			SelectNextClientAfterDivider();
			return;
		}
		if (currentClient != null)
		{
			customKey.Text = currentClient.Key.ToString();
			customPeerID.Text = currentClient.PeerID.ToString();
			if (!string.IsNullOrEmpty(customPeersNum.Text))
			{
				customPeersNum.Text = currentClient.NumwantInitialValue.ToString();
			}
		}
	}

	private void memoryReaderButton_Click(object sender, EventArgs e)
	{
		Form form = new MemoryReader(this);
		form.ShowDialog();
	}

	private void stopProcessActionBox_DropDown(object sender, EventArgs e)
	{
		stopParamsUpdateInProgress = true;
	}

	private void uploadCount_MouseHover(object sender, EventArgs e)
	{
		try
		{
			toolTip1.SetToolTip(uploadCount, currentTorrent.uploaded + " bytes");
		}
		catch
		{
		}
	}

	private void downloadCount_MouseHover(object sender, EventArgs e)
	{
		try
		{
			toolTip1.SetToolTip(downloadCount, currentTorrent.downloaded + " bytes");
		}
		catch
		{
		}
	}

	private void toolTip1_Popup(object sender, PopupEventArgs e)
	{
		string text = lclzManager.TranslateTooltip(e.AssociatedControl.Name, "");
		if (!string.IsNullOrEmpty(text) && toolTip1.GetToolTip(e.AssociatedControl) != text)
		{
			toolTip1.SetToolTip(e.AssociatedControl, text);
		}
	}

	private void comboBindIp_SelectedIndexChanged(object sender, EventArgs e)
	{
		SetLocalIpBinding();
	}

	private void testNetworkButton_Click(object sender, EventArgs e)
	{
		currentProxy = GetCurrentProxy();
		Thread thread = new Thread(TestNetworkConnection);
		thread.Name = "TestNetworkConnection() Thread";
		thread.Start();
	}

	private void TestNetworkConnection()
	{
		if (TestNetworkInProgress)
		{
			string title = lclzManager.TranslateMessageInner("Form1", "tabNetwork", "Network");
			string message = lclzManager.TranslateMessage("testNetworkInProgress", "Network settings test is in progress...");
			ShowMessage(message, title);
			return;
		}
		IProxyClient proxyClient = null;
		TcpClient tcpClient = null;
		try
		{
			AddLogLine("Network settings test is started...");
			TestNetworkInProgress = true;
			proxyClient = CreateProxyClient();
			string host = "www.google.com";
			tcpClient = CreateProxyConnection(proxyClient, host, 80);
			AddLogLine("Local end point=" + tcpClient.Client.LocalEndPoint);
			string title2 = lclzManager.TranslateMessageInner("Form1", "tabNetwork", "Network");
			string message2 = lclzManager.TranslateMessage("testNetworkSuccess", "Connected successfully. Current RatioMaster network settings appear to be working.");
			ShowMessage(message2, title2);
		}
		catch (Exception ex)
		{
			AddLogLine("Connection Error:" + ex.Message);
			string title3 = lclzManager.TranslateMessage("testNetworkFailed", "Connection Error");
			ShowMessage(ex.Message, title3);
		}
		finally
		{
			CloseTcpClient(tcpClient);
			TestNetworkInProgress = false;
		}
	}

	private void ResetCountersButton_Click(object sender, EventArgs e)
	{
		currentTorrent.downloaded = currentTorrent.downloadedLast;
		currentTorrent.uploaded = currentTorrent.uploadedLast;
		currentTorrent.left = currentTorrent.leftLast;
	}

	private void torrentFileBox_SelectionChangeCommitted(object sender, EventArgs e)
	{
		if (torrentFile.SelectedItem is RecentTorrentListItem item)
		{
			loadTorrentFileInfo(item.ToString(), loadSettings: true);
		}
	}

	private void SelectNextClientAfterDivider()
	{
		for (int i = TorrentClientsBox.SelectedIndex + 1; i < TorrentClientsBox.Items.Count; i++)
		{
			if (TorrentClientsBox.Items[i] is TorrentClient client && !client.IsDivider)
			{
				TorrentClientsBox.SelectedIndex = i;
				return;
			}
		}
		for (int i = 0; i < TorrentClientsBox.Items.Count; i++)
		{
			if (TorrentClientsBox.Items[i] is TorrentClient client && !client.IsDivider)
			{
				TorrentClientsBox.SelectedIndex = i;
				return;
			}
		}
	}
}
