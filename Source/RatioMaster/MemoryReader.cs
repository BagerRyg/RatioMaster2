using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ProcessMemoryReaderLib;

namespace RatioMaster;

public class MemoryReader : Form
{
	private IContainer components;

	private TextBox reProcessNameBox;

	private Label reProcessNameLabel;

	private Label rePeerIdLabel;

	private TextBox rePeerIdBox;

	private Label reKeyLabel;

	private TextBox reKeyBox;

	private ProgressBar reProgressBar;

	private Button reSearchButton;

	private Label rePortLabel;

	public TextBox reCustomPort;

	private Label numwantLabel;

	public TextBox customPeersNum;

	private Label reSearchStrLabel;

	private TextBox reSearchStr;

	private GroupBox groupBoxSearchResults;

	private Button reCloseButton;

	private Button reApplyButton;

	private ToolTip toolTip1;

	private TextBox reHashBox;

	private Label reHashLabel;

	private MainForm _mainForm;

	private string currentClientProcessName = "utorrent";

	private string clientSearchString = "&peer_id=-UT1600-";

	private int absoluteStartOffset = 0;

	private int absoluteEndOffset = 536870911;

	private int currentOffset;

	private uint bufferSize = 65536u;

	private ProcessMemoryReader pReader;

	private Encoding enc = Encoding.ASCII;

	private TorrentClient currentClient;

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			try
			{
				pReader?.Dispose();
			}
			catch
			{
			}
			rePeerIdBox?.Clear();
			reKeyBox?.Clear();
			reHashBox?.Clear();
			reSearchStr?.Clear();
			currentClient = null;
			components?.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RatioMaster.MemoryReader));
		this.reProcessNameBox = new System.Windows.Forms.TextBox();
		this.reProcessNameLabel = new System.Windows.Forms.Label();
		this.rePeerIdLabel = new System.Windows.Forms.Label();
		this.rePeerIdBox = new System.Windows.Forms.TextBox();
		this.reKeyLabel = new System.Windows.Forms.Label();
		this.reKeyBox = new System.Windows.Forms.TextBox();
		this.reProgressBar = new System.Windows.Forms.ProgressBar();
		this.reSearchButton = new RatioMaster.DarkButton();
		this.rePortLabel = new System.Windows.Forms.Label();
		this.reCustomPort = new System.Windows.Forms.TextBox();
		this.numwantLabel = new System.Windows.Forms.Label();
		this.customPeersNum = new System.Windows.Forms.TextBox();
		this.reSearchStrLabel = new System.Windows.Forms.Label();
		this.reSearchStr = new System.Windows.Forms.TextBox();
		this.groupBoxSearchResults = new System.Windows.Forms.GroupBox();
		this.reHashBox = new System.Windows.Forms.TextBox();
		this.reHashLabel = new System.Windows.Forms.Label();
		this.reCloseButton = new RatioMaster.DarkButton();
		this.reApplyButton = new RatioMaster.DarkButton();
		this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
		this.groupBoxSearchResults.SuspendLayout();
		base.SuspendLayout();
		this.reProcessNameBox.Location = new System.Drawing.Point(118, 22);
		this.reProcessNameBox.Name = "reProcessNameBox";
		this.reProcessNameBox.Size = new System.Drawing.Size(131, 20);
		this.reProcessNameBox.TabIndex = 0;
		this.toolTip1.SetToolTip(this.reProcessNameBox, "RM will look for this process.\r\nRM fills this box automatically according to your current Client Selection.");
		this.reProcessNameLabel.Location = new System.Drawing.Point(12, 20);
		this.reProcessNameLabel.Name = "reProcessNameLabel";
		this.reProcessNameLabel.Size = new System.Drawing.Size(100, 23);
		this.reProcessNameLabel.TabIndex = 1;
		this.reProcessNameLabel.Text = "Process Name:";
		this.reProcessNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.rePeerIdLabel.ForeColor = System.Drawing.SystemColors.ControlText;
		this.rePeerIdLabel.Location = new System.Drawing.Point(9, 25);
		this.rePeerIdLabel.Name = "rePeerIdLabel";
		this.rePeerIdLabel.Size = new System.Drawing.Size(115, 13);
		this.rePeerIdLabel.TabIndex = 2;
		this.rePeerIdLabel.Text = "Peer ID(peer_id):";
		this.rePeerIdLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.rePeerIdBox.Location = new System.Drawing.Point(130, 22);
		this.rePeerIdBox.Name = "rePeerIdBox";
		this.rePeerIdBox.Size = new System.Drawing.Size(236, 20);
		this.rePeerIdBox.TabIndex = 3;
		this.reKeyLabel.ForeColor = System.Drawing.SystemColors.ControlText;
		this.reKeyLabel.Location = new System.Drawing.Point(9, 51);
		this.reKeyLabel.Name = "reKeyLabel";
		this.reKeyLabel.Size = new System.Drawing.Size(115, 13);
		this.reKeyLabel.TabIndex = 4;
		this.reKeyLabel.Text = "Key(key):";
		this.reKeyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.reKeyBox.Location = new System.Drawing.Point(130, 48);
		this.reKeyBox.Name = "reKeyBox";
		this.reKeyBox.Size = new System.Drawing.Size(236, 20);
		this.reKeyBox.TabIndex = 5;
		this.reProgressBar.BackColor = System.Drawing.SystemColors.ControlLightLight;
		this.reProgressBar.Location = new System.Drawing.Point(24, 87);
		this.reProgressBar.Name = "reProgressBar";
		this.reProgressBar.Size = new System.Drawing.Size(468, 23);
		this.reProgressBar.TabIndex = 6;
		this.reProgressBar.Visible = false;
		this.reSearchButton.Location = new System.Drawing.Point(365, 48);
		this.reSearchButton.Name = "reSearchButton";
		this.reSearchButton.Size = new System.Drawing.Size(127, 23);
		this.reSearchButton.TabIndex = 7;
		this.reSearchButton.Text = "Start Search";
		this.toolTip1.SetToolTip(this.reSearchButton, "Press on \"Start Search\" to ... Start Search.");
		this.reSearchButton.UseVisualStyleBackColor = true;
		this.reSearchButton.Click += new System.EventHandler(reSearchButton_Click);
		this.rePortLabel.ForeColor = System.Drawing.SystemColors.ControlText;
		this.rePortLabel.Location = new System.Drawing.Point(12, 75);
		this.rePortLabel.Name = "rePortLabel";
		this.rePortLabel.Size = new System.Drawing.Size(112, 17);
		this.rePortLabel.TabIndex = 8;
		this.rePortLabel.Text = "Port (port):";
		this.rePortLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.reCustomPort.Location = new System.Drawing.Point(130, 75);
		this.reCustomPort.Name = "reCustomPort";
		this.reCustomPort.Size = new System.Drawing.Size(44, 20);
		this.reCustomPort.TabIndex = 9;
		this.numwantLabel.ForeColor = System.Drawing.SystemColors.ControlText;
		this.numwantLabel.Location = new System.Drawing.Point(180, 78);
		this.numwantLabel.Name = "numwantLabel";
		this.numwantLabel.Size = new System.Drawing.Size(234, 13);
		this.numwantLabel.TabIndex = 10;
		this.numwantLabel.Text = "Number of Peers (numwant):";
		this.numwantLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.customPeersNum.Location = new System.Drawing.Point(420, 74);
		this.customPeersNum.Name = "customPeersNum";
		this.customPeersNum.Size = new System.Drawing.Size(46, 20);
		this.customPeersNum.TabIndex = 11;
		this.reSearchStrLabel.Location = new System.Drawing.Point(255, 22);
		this.reSearchStrLabel.Name = "reSearchStrLabel";
		this.reSearchStrLabel.Size = new System.Drawing.Size(70, 23);
		this.reSearchStrLabel.TabIndex = 12;
		this.reSearchStrLabel.Text = "String:";
		this.reSearchStrLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.reSearchStr.Location = new System.Drawing.Point(331, 22);
		this.reSearchStr.Name = "reSearchStr";
		this.reSearchStr.Size = new System.Drawing.Size(161, 20);
		this.reSearchStr.TabIndex = 13;
		this.toolTip1.SetToolTip(this.reSearchStr, "RM looks for this string to find announce URL and parse search results.");
		this.groupBoxSearchResults.Controls.Add(this.reHashBox);
		this.groupBoxSearchResults.Controls.Add(this.reHashLabel);
		this.groupBoxSearchResults.Controls.Add(this.rePeerIdLabel);
		this.groupBoxSearchResults.Controls.Add(this.rePeerIdBox);
		this.groupBoxSearchResults.Controls.Add(this.reKeyLabel);
		this.groupBoxSearchResults.Controls.Add(this.customPeersNum);
		this.groupBoxSearchResults.Controls.Add(this.reKeyBox);
		this.groupBoxSearchResults.Controls.Add(this.numwantLabel);
		this.groupBoxSearchResults.Controls.Add(this.rePortLabel);
		this.groupBoxSearchResults.Controls.Add(this.reCustomPort);
		this.groupBoxSearchResults.ForeColor = System.Drawing.SystemColors.ActiveCaption;
		this.groupBoxSearchResults.Location = new System.Drawing.Point(12, 133);
		this.groupBoxSearchResults.Name = "groupBoxSearchResults";
		this.groupBoxSearchResults.Size = new System.Drawing.Size(486, 129);
		this.groupBoxSearchResults.TabIndex = 14;
		this.groupBoxSearchResults.TabStop = false;
		this.groupBoxSearchResults.Text = "Search Results";
		this.toolTip1.SetToolTip(this.groupBoxSearchResults, "If search successfull RM fills those fields with found results.\r\nCopy those values manually to RM Advanced Settings or\r\npress on Apply.");
		this.reHashBox.Location = new System.Drawing.Point(78, 100);
		this.reHashBox.Name = "reHashBox";
		this.reHashBox.Size = new System.Drawing.Size(388, 20);
		this.reHashBox.TabIndex = 13;
		this.reHashLabel.ForeColor = System.Drawing.SystemColors.ControlText;
		this.reHashLabel.Location = new System.Drawing.Point(6, 103);
		this.reHashLabel.Name = "reHashLabel";
		this.reHashLabel.Size = new System.Drawing.Size(66, 13);
		this.reHashLabel.TabIndex = 12;
		this.reHashLabel.Text = "Hash:";
		this.reHashLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.reCloseButton.Location = new System.Drawing.Point(384, 268);
		this.reCloseButton.Name = "reCloseButton";
		this.reCloseButton.Size = new System.Drawing.Size(114, 23);
		this.reCloseButton.TabIndex = 15;
		this.reCloseButton.Text = "Close";
		this.reCloseButton.UseVisualStyleBackColor = true;
		this.reCloseButton.Click += new System.EventHandler(reCloseButton_Click);
		this.reApplyButton.Enabled = false;
		this.reApplyButton.Location = new System.Drawing.Point(277, 268);
		this.reApplyButton.Name = "reApplyButton";
		this.reApplyButton.Size = new System.Drawing.Size(101, 23);
		this.reApplyButton.TabIndex = 16;
		this.reApplyButton.Text = "Apply";
		this.toolTip1.SetToolTip(this.reApplyButton, "Click on \"Apply\" to copy search results into Advanced Tab of RM.");
		this.reApplyButton.UseVisualStyleBackColor = true;
		this.reApplyButton.Click += new System.EventHandler(reApplyButton_Click);
		this.toolTip1.AutomaticDelay = 1000;
		this.toolTip1.Popup += new System.Windows.Forms.PopupEventHandler(toolTip1_Popup);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(510, 303);
		base.Controls.Add(this.reApplyButton);
		base.Controls.Add(this.reCloseButton);
		base.Controls.Add(this.groupBoxSearchResults);
		base.Controls.Add(this.reSearchStr);
		base.Controls.Add(this.reSearchStrLabel);
		base.Controls.Add(this.reSearchButton);
		base.Controls.Add(this.reProgressBar);
		base.Controls.Add(this.reProcessNameLabel);
		base.Controls.Add(this.reProcessNameBox);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "MemoryReader";
		this.RightToLeftLayout = true;
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Memory Reader";
		base.TopMost = true;
		base.Load += new System.EventHandler(MemoryReader_Load);
		base.Shown += new System.EventHandler(MemoryReader_Shown);
		this.groupBoxSearchResults.ResumeLayout(false);
		this.groupBoxSearchResults.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}

	public MemoryReader(MainForm MainForm)
	{
		InitializeComponent();
		_mainForm = MainForm;
		DarkTheme.Apply(this);
		DarkTheme.Apply(toolTip1);
	}

	private void MemoryReader_Load(object sender, EventArgs e)
	{
		currentClient = _mainForm.getCurrentClient();
		currentClientProcessName = currentClient.ProcessName;
		clientSearchString = "&peer_id=" + currentClient.PeerIDPrefix;
		reSearchStr.Text = clientSearchString;
		reProcessNameBox.Text = currentClientProcessName;
	}

	private bool InitSearch()
	{
		Process process = FindProcessByName(currentClientProcessName);
		if (process == null)
		{
			return false;
		}
		currentOffset = absoluteStartOffset;
		pReader = new ProcessMemoryReader();
		pReader.ReadProcess = process;
		return true;
	}

	private void StartSearch()
	{
		int num = 0;
		bool flag = false;
		pReader.OpenProcess();
		reProgressBar.Maximum = absoluteEndOffset;
		reProgressBar.Minimum = absoluteStartOffset;
		reProgressBar.Visible = true;
		while (currentOffset < absoluteEndOffset)
		{
			reProgressBar.Value = currentOffset;
			int bytesReaded;
			byte[] array = pReader.ReadProcessMemory((IntPtr)currentOffset, bufferSize, out bytesReaded);
			if (array == null || bytesReaded <= 0)
			{
				break;
			}
			num = getStringOffsetInsideArray(array);
			if (num >= 0)
			{
				string input = enc.GetString(array);
				Regex regex = new Regex("&peer_id=(.+?)(&| )", RegexOptions.Compiled);
				Match match = regex.Match(input);
				if (match.Success)
				{
					rePeerIdBox.Text = match.Groups[1].ToString();
				}
				regex = new Regex("&key=(.+?)(&| |\0)", RegexOptions.Compiled);
				match = regex.Match(input);
				if (match.Success)
				{
					reKeyBox.Text = match.Groups[1].ToString();
				}
				regex = new Regex("&port=([0-9]+)", RegexOptions.Compiled);
				match = regex.Match(input);
				if (match.Success)
				{
					reCustomPort.Text = match.Groups[1].ToString();
				}
				regex = new Regex("&numwant=([0-9]+)", RegexOptions.Compiled);
				match = regex.Match(input);
				if (match.Success)
				{
					customPeersNum.Text = match.Groups[1].ToString();
				}
				regex = new Regex("&info_hash=(.+?)(&| )", RegexOptions.Compiled);
				match = regex.Match(input);
				if (match.Success)
				{
					reHashBox.Text = match.Groups[1].ToString();
				}
				if (!string.IsNullOrEmpty(customPeersNum.Text))
				{
					flag = true;
					Array.Clear(array, 0, array.Length);
					break;
				}
			}
			Array.Clear(array, 0, array.Length);
			currentOffset = currentOffset + (int)bufferSize - 512;
		}
		pReader.CloseHandle();
		reProgressBar.Visible = false;
		if (flag)
		{
			string text = _mainForm.lclzManager.TranslateMessage("mrSearchSuccess", "Search finished successfully!\r\nPress Apply button to apply found values to current client.");
			string caption = _mainForm.lclzManager.TranslateMessage("mrSearchTitle", "Search");
			MessageBox.Show(text, caption, MessageBoxButtons.OK);
			reApplyButton.Enabled = true;
		}
		else
		{
			string text2 = _mainForm.lclzManager.TranslateMessage("mrSearchFail", "Search failed.\r\nMake sure that torrent client is running and that at least one torrent is working.");
			string caption2 = _mainForm.lclzManager.TranslateMessage("mrSearchTitle", "Search");
			MessageBox.Show(text2, caption2, MessageBoxButtons.OK);
		}
	}

	private int getStringOffsetInsideArray(byte[] memory)
	{
		string text = enc.GetString(memory);
		return text.IndexOf(clientSearchString);
	}

	private Process FindProcessByName(string processName)
	{
		_mainForm.AddLogLine("Looking for client process...");
		Process[] processesByName = Process.GetProcessesByName(processName);
		if (processesByName.Length == 0)
		{
			string logLine = _mainForm.lclzManager.TranslateMessageWithParams("mrNoProcessFound", "No {0} process found .\r\nMake sure that torrent client is running and that at least one torrent is working.", currentClientProcessName);
			_mainForm.AddLogLine(logLine);
			string caption = _mainForm.lclzManager.TranslateMessage("mrSearchTitle", "Search");
			MessageBox.Show(logLine, caption, MessageBoxButtons.OK);
			return null;
		}
		_mainForm.AddLogLine(currentClientProcessName + " process found! ");
		return processesByName[0];
	}

	private void reSearchButton_Click(object sender, EventArgs e)
	{
		clientSearchString = reSearchStr.Text;
		currentClientProcessName = reProcessNameBox.Text;
		if (InitSearch())
		{
			StartSearch();
		}
	}

	private void reCloseButton_Click(object sender, EventArgs e)
	{
		Close();
	}

	private void reApplyButton_Click(object sender, EventArgs e)
	{
		_mainForm.customPeerID.Text = rePeerIdBox.Text;
		if (customPeersNum.Text != "0")
		{
			_mainForm.customPeersNum.Text = customPeersNum.Text;
		}
		_mainForm.customKey.Text = reKeyBox.Text;
		_mainForm.customPort.Text = reCustomPort.Text;
		reApplyButton.Enabled = false;
	}

	private void MemoryReader_Shown(object sender, EventArgs e)
	{
		_mainForm.lclzManager.LocalizeForm(this, adjustRightToLeft: false);
		DarkTheme.Apply(this);
	}

	private void saveArrayToFile(byte[] arr, string filename)
	{
		FileStream fileStream = File.OpenWrite(filename);
		fileStream.Write(arr, 0, arr.Length);
		fileStream.Close();
	}

	private void toolTip1_Popup(object sender, PopupEventArgs e)
	{
		string text = _mainForm.lclzManager.TranslateTooltip(e.AssociatedControl.Name, "");
		if (!string.IsNullOrEmpty(text) && toolTip1.GetToolTip(e.AssociatedControl) != text)
		{
			toolTip1.SetToolTip(e.AssociatedControl, text);
		}
	}
}
