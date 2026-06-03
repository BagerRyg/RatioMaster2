using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace RatioMaster;

internal sealed class DarkTorrentFileDialog : Form
{
	private static readonly Color WindowBack = Color.FromArgb(31, 31, 31);
	private static readonly Color PanelBack = Color.FromArgb(38, 38, 38);
	private static readonly Color InputBack = Color.FromArgb(25, 25, 25);
	private static readonly Color BorderColor = Color.FromArgb(70, 70, 70);
	private static readonly Color SelectedBack = Color.FromArgb(72, 72, 72);
	private static readonly Color TextColor = Color.White;
	private static readonly Color MutedText = Color.FromArgb(210, 210, 210);

	private readonly TextBox pathBox;
	private readonly ListView fileList;
	private readonly TextBox fileNameBox;
	private readonly DarkButton openButton;
	private readonly DarkButton cancelButton;
	private readonly DarkButton upButton;
	private readonly Panel sidebar;
	private string currentDirectory;

	public string SelectedFile { get; private set; }

	public DarkTorrentFileDialog(string initialFile)
	{
		Text = "Open";
		StartPosition = FormStartPosition.CenterParent;
		MinimumSize = new Size(900, 620);
		Size = new Size(980, 680);
		ShowIcon = true;
		Icon = SystemIcons.Application;

		sidebar = new Panel
		{
			Location = new Point(16, 56),
			Size = new Size(150, 496),
			Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left,
			BackColor = PanelBack
		};
		AddShortcut("Downloads", GetDownloadsDirectory(), 12);
		AddShortcut("Desktop", Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), 52);
		AddShortcut("Documents", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 92);
		AddShortcut("This PC", null, 132);

		Label locationLabel = new Label
		{
			Text = "Look in:",
			AutoSize = true,
			Location = new Point(18, 22)
		};
		pathBox = new TextBox
		{
			ReadOnly = true,
			Location = new Point(88, 18),
			Size = new Size(728, 27),
			BackColor = InputBack,
			ForeColor = TextColor,
			BorderStyle = BorderStyle.FixedSingle
		};

		upButton = new DarkButton
		{
			Text = "Up",
			Location = new Point(830, 16),
			Size = new Size(110, 31)
		};
		upButton.Click += delegate
		{
			DirectoryInfo parent = Directory.GetParent(currentDirectory);
			if (parent != null)
			{
				LoadDirectory(parent.FullName);
			}
		};

		fileList = new ListView
		{
			Location = new Point(180, 56),
			Size = new Size(760, 456),
			Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
			View = View.Details,
			FullRowSelect = true,
			HideSelection = false,
			OwnerDraw = true,
			BackColor = InputBack,
			ForeColor = TextColor,
			BorderStyle = BorderStyle.FixedSingle
		};
		fileList.Columns.Add("Name", 460);
		fileList.Columns.Add("Date modified", 190);
		fileList.Columns.Add("Size", 110);
		fileList.DrawColumnHeader += DrawColumnHeader;
		fileList.DrawItem += DrawItem;
		fileList.DrawSubItem += DrawSubItem;
		fileList.SelectedIndexChanged += FileList_SelectedIndexChanged;
		fileList.DoubleClick += FileList_DoubleClick;

		Label fileNameLabel = new Label
		{
			Text = "File name:",
			AutoSize = true,
			Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
			Location = new Point(180, 532)
		};
		fileNameBox = new TextBox
		{
			Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
			Location = new Point(282, 528),
			Size = new Size(498, 27),
			BackColor = InputBack,
			ForeColor = TextColor,
			BorderStyle = BorderStyle.FixedSingle
		};
		fileNameBox.KeyDown += delegate(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				OpenSelected();
			}
		};

		Label fileTypeLabel = new Label
		{
			Text = "File type:",
			AutoSize = true,
			Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
			Location = new Point(180, 570)
		};
		TextBox fileTypeBox = new TextBox
		{
			Text = "Torrent Files (*.torrent)",
			ReadOnly = true,
			Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
			Location = new Point(282, 566),
			Size = new Size(498, 27),
			BackColor = InputBack,
			ForeColor = TextColor,
			BorderStyle = BorderStyle.FixedSingle
		};

		openButton = new DarkButton
		{
			Text = "Open",
			Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
			Location = new Point(800, 527),
			Size = new Size(140, 34)
		};
		openButton.Click += delegate { OpenSelected(); };
		cancelButton = new DarkButton
		{
			Text = "Cancel",
			Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
			Location = new Point(800, 567),
			Size = new Size(140, 34)
		};
		cancelButton.Click += delegate
		{
			DialogResult = DialogResult.Cancel;
			Close();
		};

		Controls.AddRange(new Control[] { locationLabel, pathBox, upButton, sidebar, fileList, fileNameLabel, fileNameBox, fileTypeLabel, fileTypeBox, openButton, cancelButton });
		AcceptButton = openButton;
		CancelButton = cancelButton;
		DarkTheme.Apply(this);

		string start = GetInitialDirectory(initialFile);
		Shown += delegate { LoadDirectory(start); };
	}

	private void AddShortcut(string text, string path, int top)
	{
		DarkButton button = new DarkButton
		{
			Text = text,
			Location = new Point(10, top),
			Size = new Size(130, 32),
			Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
		};
		button.Click += delegate
		{
			if (path == null)
			{
				ShowDrives();
			}
			else if (Directory.Exists(path))
			{
				LoadDirectory(path);
			}
		};
		sidebar.Controls.Add(button);
	}

	private static string GetDownloadsDirectory()
	{
		return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
	}

	private static string GetInitialDirectory(string initialFile)
	{
		if (!string.IsNullOrWhiteSpace(initialFile) && File.Exists(initialFile))
		{
			return Path.GetDirectoryName(initialFile);
		}
		string downloads = GetDownloadsDirectory();
		return Directory.Exists(downloads) ? downloads : Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
	}

	private void LoadDirectory(string path)
	{
		try
		{
			RuntimeLog.Write("File picker loading directory: " + path);
			UseWaitCursor = true;
			currentDirectory = Path.GetFullPath(path);
			pathBox.Text = currentDirectory;
			fileList.Items.Clear();
			AddDirectories(currentDirectory);
			AddTorrentFiles(currentDirectory);
			upButton.Enabled = Directory.GetParent(currentDirectory) != null;
			RuntimeLog.Write("File picker loaded directory: " + currentDirectory + " items=" + fileList.Items.Count);
		}
		catch (Exception ex)
		{
			RuntimeLog.WriteException("File picker directory load failed: " + path, ex);
			MessageBox.Show(ex.Message, "Open", MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}
		finally
		{
			UseWaitCursor = false;
		}
	}

	private void ShowDrives()
	{
		RuntimeLog.Write("File picker showing drives");
		currentDirectory = null;
		pathBox.Text = "This PC";
		fileList.Items.Clear();
		foreach (DriveInfo drive in DriveInfo.GetDrives())
		{
			ListViewItem item = new ListViewItem(drive.Name);
			item.SubItems.Add("");
			item.SubItems.Add("");
			item.Tag = drive.RootDirectory.FullName;
			item.ImageIndex = 0;
			fileList.Items.Add(item);
		}
		upButton.Enabled = false;
	}

	private void AddDirectories(string path)
	{
		foreach (DirectoryInfo directory in SafeGetDirectories(path))
		{
			if ((directory.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
			{
				continue;
			}
			ListViewItem item = new ListViewItem(directory.Name);
			item.SubItems.Add(directory.LastWriteTime.ToString());
			item.SubItems.Add("");
			item.Tag = directory.FullName;
			item.ImageIndex = 0;
			fileList.Items.Add(item);
		}
	}

	private void AddTorrentFiles(string path)
	{
		foreach (FileInfo file in SafeGetTorrentFiles(path))
		{
			ListViewItem item = new ListViewItem(file.Name);
			item.SubItems.Add(file.LastWriteTime.ToString());
			item.SubItems.Add(FormatSize(file.Length));
			item.Tag = file.FullName;
			item.ImageIndex = 1;
			fileList.Items.Add(item);
		}
	}

	private static IEnumerable<DirectoryInfo> SafeGetDirectories(string path)
	{
		DirectoryInfo[] directories;
		try
		{
			directories = new DirectoryInfo(path).GetDirectories();
		}
		catch (Exception ex)
		{
			RuntimeLog.WriteException("Unable to enumerate directories: " + path, ex);
			return Array.Empty<DirectoryInfo>();
		}
		return directories;
	}

	private static IEnumerable<FileInfo> SafeGetTorrentFiles(string path)
	{
		FileInfo[] files;
		try
		{
			files = new DirectoryInfo(path).GetFiles("*.torrent");
		}
		catch (Exception ex)
		{
			RuntimeLog.WriteException("Unable to enumerate torrent files: " + path, ex);
			return Array.Empty<FileInfo>();
		}
		return files;
	}

	private static string FormatSize(long bytes)
	{
		if (bytes >= 1048576)
		{
			return $"{bytes / 1048576d:0.##} MB";
		}
		if (bytes >= 1024)
		{
			return $"{bytes / 1024d:0.##} KB";
		}
		return bytes + " bytes";
	}

	private void FileList_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (fileList.SelectedItems.Count == 0)
		{
			return;
		}
		string path = (string)fileList.SelectedItems[0].Tag;
		if (IsFileItem(fileList.SelectedItems[0]))
		{
			fileNameBox.Text = Path.GetFileName(path);
		}
	}

	private void FileList_DoubleClick(object sender, EventArgs e)
	{
		if (fileList.SelectedItems.Count == 0)
		{
			return;
		}
		string path = (string)fileList.SelectedItems[0].Tag;
		if (IsDirectoryItem(fileList.SelectedItems[0]))
		{
			LoadDirectory(path);
			return;
		}
		OpenSelected();
	}

	private void OpenSelected()
	{
		string selectedPath = null;
		if (fileList.SelectedItems.Count > 0)
		{
			selectedPath = (string)fileList.SelectedItems[0].Tag;
		}
		if (fileList.SelectedItems.Count > 0 && IsDirectoryItem(fileList.SelectedItems[0]))
		{
			LoadDirectory(selectedPath);
			return;
		}
		if (string.IsNullOrWhiteSpace(selectedPath))
		{
			selectedPath = Path.Combine(currentDirectory, fileNameBox.Text);
		}
		if (!File.Exists(selectedPath))
		{
			return;
		}
		SelectedFile = selectedPath;
		DialogResult = DialogResult.OK;
		Close();
	}

	private static bool IsDirectoryItem(ListViewItem item)
	{
		return item.ImageIndex == 0;
	}

	private static bool IsFileItem(ListViewItem item)
	{
		return item.ImageIndex == 1;
	}

	private static void DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
	{
		using (Brush back = new SolidBrush(PanelBack))
		using (Brush text = new SolidBrush(TextColor))
		using (Pen border = new Pen(BorderColor))
		{
			e.Graphics.FillRectangle(back, e.Bounds);
			e.Graphics.DrawRectangle(border, e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1);
			e.Graphics.DrawString(e.Header.Text, SystemFonts.MessageBoxFont, text, new Rectangle(e.Bounds.X + 8, e.Bounds.Y, e.Bounds.Width - 12, e.Bounds.Height));
		}
	}

	private static void DrawItem(object sender, DrawListViewItemEventArgs e)
	{
	}

	private static void DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
	{
		bool selected = e.Item.Selected;
		using (Brush back = new SolidBrush(selected ? SelectedBack : InputBack))
		using (Brush text = new SolidBrush(e.ColumnIndex == 0 ? TextColor : MutedText))
		{
			e.Graphics.FillRectangle(back, e.Bounds);
			string value = e.SubItem.Text;
			if (e.ColumnIndex == 0 && IsDirectoryItem(e.Item))
			{
				value = "[Folder] " + value;
			}
			Rectangle textBounds = new Rectangle(e.Bounds.X + 8, e.Bounds.Y, e.Bounds.Width - 12, e.Bounds.Height);
			TextRenderer.DrawText(e.Graphics, value, e.Item.Font, textBounds, e.ColumnIndex == 0 ? TextColor : MutedText, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
		}
	}
}
