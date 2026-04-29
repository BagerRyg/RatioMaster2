using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace RatioMaster;

internal static class DarkTheme
{
	private static Color Window = Color.FromArgb(31, 31, 31);
	private static Color Panel = Color.FromArgb(38, 38, 38);
	private static Color Input = Color.FromArgb(25, 25, 25);
	private static Color Border = Color.FromArgb(70, 70, 70);
	private static Color Hover = Color.FromArgb(58, 58, 58);
	private static Color Selected = Color.FromArgb(72, 72, 72);
	private static Color Text = Color.White;
	private static Color MutedText = Color.FromArgb(210, 210, 210);
	private static Color Accent = Color.FromArgb(86, 156, 214);
	private static Color GreenHover = Color.FromArgb(36, 110, 63);
	private static Color RedHover = Color.FromArgb(132, 45, 45);
	private static Color Button = Color.FromArgb(45, 45, 45);
	private static Color DisabledButton = Color.FromArgb(30, 30, 30);
	private static Color DisabledBorder = Color.FromArgb(48, 48, 48);
	private static Color DisabledText = Color.FromArgb(105, 105, 105);
	private static Color Tab = Color.FromArgb(42, 42, 42);

	private static readonly HashSet<ComboBox> ComboBoxes = new HashSet<ComboBox>();
	private static readonly HashSet<TabControl> TabControls = new HashSet<TabControl>();
	private static readonly HashSet<ToolTip> ToolTips = new HashSet<ToolTip>();

	public static string CurrentThemeName { get; private set; } = "Dark";
	public static bool IsDark => CurrentThemeName == "Dark";
	internal static Color WindowColor => Window;
	internal static Color PanelColor => Panel;
	internal static Color InputColor => Input;
	internal static Color BorderColor => Border;
	internal static Color HoverColor => Hover;
	internal static Color SelectedColor => Selected;
	internal static Color TextColor => Text;
	internal static Color MutedTextColor => MutedText;
	internal static Color AccentColor => Accent;
	internal static Color ButtonColor => Button;
	internal static Color DisabledButtonColor => DisabledButton;
	internal static Color DisabledBorderColor => DisabledBorder;
	internal static Color DisabledTextColor => DisabledText;
	internal static Color TabColor => Tab;

	public static void LoadSavedThemeMode()
	{
		try
		{
			string configPath = Path.Combine(Application.StartupPath, "ratiomaster.config");
			if (!File.Exists(configPath))
			{
				ApplyThemeMode("Dark");
				return;
			}
			Match match = Regex.Match(File.ReadAllText(configPath), "<interfaceTheme>(.*?)</interfaceTheme>", RegexOptions.IgnoreCase);
			ApplyThemeMode(match.Success ? match.Groups[1].Value : "Dark");
		}
		catch
		{
			ApplyThemeMode("Dark");
		}
	}

	public static void ApplyThemeMode(string themeName)
	{
		CurrentThemeName = string.Equals(themeName, "Light", StringComparison.OrdinalIgnoreCase) ? "Light" : "Dark";
		if (IsDark)
		{
			Window = Color.FromArgb(31, 31, 31);
			Panel = Color.FromArgb(38, 38, 38);
			Input = Color.FromArgb(25, 25, 25);
			Border = Color.FromArgb(70, 70, 70);
			Hover = Color.FromArgb(58, 58, 58);
			Selected = Color.FromArgb(72, 72, 72);
			Text = Color.White;
			MutedText = Color.FromArgb(230, 230, 230);
			Accent = Color.FromArgb(86, 156, 214);
			GreenHover = Color.FromArgb(36, 110, 63);
			RedHover = Color.FromArgb(132, 45, 45);
			Button = Color.FromArgb(45, 45, 45);
			DisabledButton = Color.FromArgb(30, 30, 30);
			DisabledBorder = Color.FromArgb(48, 48, 48);
			DisabledText = Color.FromArgb(105, 105, 105);
			Tab = Color.FromArgb(42, 42, 42);
		}
		else
		{
			Window = Color.FromArgb(246, 247, 249);
			Panel = Color.FromArgb(255, 255, 255);
			Input = Color.FromArgb(255, 255, 255);
			Border = Color.FromArgb(176, 181, 188);
			Hover = Color.FromArgb(229, 235, 243);
			Selected = Color.FromArgb(214, 224, 238);
			Text = Color.FromArgb(24, 28, 34);
			MutedText = Color.FromArgb(41, 48, 57);
			Accent = Color.FromArgb(0, 102, 204);
			GreenHover = Color.FromArgb(198, 232, 211);
			RedHover = Color.FromArgb(244, 204, 204);
			Button = Color.FromArgb(245, 246, 248);
			DisabledButton = Color.FromArgb(232, 234, 237);
			DisabledBorder = Color.FromArgb(205, 210, 216);
			DisabledText = Color.FromArgb(136, 142, 150);
			Tab = Color.FromArgb(238, 240, 243);
		}
		EnableDarkApplicationMode();
	}

	public static void Apply(Form form)
	{
		form.BackColor = Window;
		form.ForeColor = Text;
		form.Font = new Font("Segoe UI", form.Font.SizeInPoints);
		UseTitleBarTheme(form);
		Apply((Control)form);
	}

	public static void EnableDarkApplicationMode()
	{
		try
		{
			SetPreferredAppMode(IsDark ? 2 : 0);
			FlushMenuThemes();
		}
		catch
		{
		}
	}

	public static void Apply(ContextMenuStrip menu)
	{
		if (menu == null)
		{
			return;
		}
		menu.BackColor = Panel;
		menu.ForeColor = Text;
		menu.Renderer = new DarkToolStripRenderer();
		foreach (ToolStripItem item in menu.Items)
		{
			ApplyToolStripItem(item);
		}
	}

	public static void Apply(ToolTip toolTip)
	{
		if (toolTip == null || ToolTips.Contains(toolTip))
		{
			return;
		}
		ToolTips.Add(toolTip);
		toolTip.OwnerDraw = true;
		toolTip.BackColor = Input;
		toolTip.ForeColor = Text;
		toolTip.Draw += DrawToolTip;
	}

	public static DialogResult ShowMessage(IWin32Window owner, string message, string title, MessageBoxIcon icon)
	{
		using Form form = new Form();
		form.Text = title;
		form.StartPosition = FormStartPosition.CenterParent;
		form.FormBorderStyle = FormBorderStyle.FixedDialog;
		form.MinimizeBox = false;
		form.MaximizeBox = false;
		form.ShowInTaskbar = false;
		form.ClientSize = new Size(560, 190);
		UseTitleBarTheme(form);

		PictureBox pictureBox = new PictureBox();
		pictureBox.Location = new Point(28, 46);
		pictureBox.Size = new Size(42, 42);
		pictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
		pictureBox.Image = GetMessageIcon(icon)?.ToBitmap();

		Label label = new Label();
		label.AutoSize = false;
		label.Location = new Point(88, 35);
		label.Size = new Size(435, 78);
		label.Text = message;
		label.TextAlign = ContentAlignment.MiddleLeft;

		Button button = new Button();
		button.Text = "OK";
		button.DialogResult = DialogResult.OK;
		button.Location = new Point(392, 132);
		button.Size = new Size(130, 36);

		form.Controls.Add(pictureBox);
		form.Controls.Add(label);
		form.Controls.Add(button);
		form.AcceptButton = button;
		Apply(form);
		return form.ShowDialog(owner);
	}

	private static Icon GetMessageIcon(MessageBoxIcon icon)
	{
		switch (icon)
		{
		case MessageBoxIcon.Hand:
			return SystemIcons.Error;
		case MessageBoxIcon.Question:
			return SystemIcons.Question;
		case MessageBoxIcon.Exclamation:
			return SystemIcons.Warning;
		case MessageBoxIcon.Asterisk:
			return SystemIcons.Information;
		default:
			return SystemIcons.Information;
		}
	}

	private static void Apply(Control control)
	{
		StyleControl(control);
		foreach (Control child in control.Controls)
		{
			Apply(child);
		}
	}

	private static void StyleControl(Control control)
	{
		control.BackColor = Panel;
		control.ForeColor = Text;

		if (control is Form)
		{
			control.BackColor = Window;
		}
		else if (control is TabPage || control is Panel || control is GroupBox)
		{
			control.BackColor = Panel;
		}
		else if (control is TextBoxBase textBox)
		{
			textBox.BackColor = Input;
			textBox.ForeColor = Text;
			textBox.BorderStyle = BorderStyle.FixedSingle;
			ApplyDarkControlChrome(textBox);
		}
		else if (control is NumericUpDown numeric)
		{
			numeric.BackColor = Input;
			numeric.ForeColor = Text;
		}
		else if (control is Button button)
		{
			StyleButton(button);
		}
		else if (control is CheckBox checkBox)
		{
			checkBox.BackColor = Panel;
			checkBox.ForeColor = Text;
			checkBox.FlatStyle = FlatStyle.Flat;
			checkBox.FlatAppearance.BorderColor = Border;
			checkBox.FlatAppearance.CheckedBackColor = Selected;
			checkBox.FlatAppearance.MouseOverBackColor = Hover;
			checkBox.FlatAppearance.MouseDownBackColor = Selected;
		}
		else if (control is ComboBox comboBox)
		{
			StyleComboBox(comboBox);
			ApplyDarkControlChrome(comboBox);
		}
		else if (control is DataGridView grid)
		{
			StyleGrid(grid);
		}
		else if (control is TabControl tabControl)
		{
			StyleTabControl(tabControl);
		}
		else if (control is LinkLabel linkLabel)
		{
			linkLabel.BackColor = Panel;
			linkLabel.LinkColor = Accent;
			linkLabel.ActiveLinkColor = Color.White;
			linkLabel.VisitedLinkColor = Color.FromArgb(160, 200, 235);
		}
		else if (control is Label)
		{
			control.BackColor = Panel;
			control.ForeColor = MutedText;
		}
		else if (control is ProgressBar)
		{
			control.BackColor = Input;
			control.ForeColor = Accent;
		}
	}

	private static void StyleButton(Button button)
	{
		button.BackColor = Color.FromArgb(45, 45, 45);
		button.ForeColor = Text;
		button.FlatStyle = FlatStyle.Flat;
		button.FlatAppearance.BorderColor = Border;
		button.FlatAppearance.MouseOverBackColor = GetButtonHoverColor(button);
		button.FlatAppearance.MouseDownBackColor = Selected;
		button.UseVisualStyleBackColor = false;
	}

	private static void StyleComboBox(ComboBox comboBox)
	{
		comboBox.BackColor = Input;
		comboBox.ForeColor = Text;
		comboBox.FlatStyle = FlatStyle.Flat;
		comboBox.DrawMode = DrawMode.OwnerDrawFixed;
		if (!ComboBoxes.Contains(comboBox))
		{
			ComboBoxes.Add(comboBox);
			comboBox.DrawItem += DrawComboBoxItem;
			comboBox.DropDown += ComboBox_DropDown;
			ApplyDarkComboDropDown(comboBox);
		}
	}

	private static void ComboBox_DropDown(object sender, EventArgs e)
	{
		if (sender is ComboBox comboBox)
		{
			ApplyDarkComboDropDown(comboBox);
		}
	}

	private static void StyleGrid(DataGridView grid)
	{
		grid.BackgroundColor = Window;
		grid.BorderStyle = BorderStyle.FixedSingle;
		grid.GridColor = Border;
		grid.EnableHeadersVisualStyles = false;
		grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 45);
		grid.ColumnHeadersDefaultCellStyle.ForeColor = Text;
		grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(45, 45, 45);
		grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Text;
		grid.DefaultCellStyle.BackColor = Input;
		grid.DefaultCellStyle.ForeColor = Text;
		grid.DefaultCellStyle.SelectionBackColor = Selected;
		grid.DefaultCellStyle.SelectionForeColor = Text;
		grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(32, 32, 32);
		grid.AlternatingRowsDefaultCellStyle.ForeColor = Text;
		grid.RowHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 45);
		grid.RowHeadersDefaultCellStyle.ForeColor = Text;
	}

	private static void StyleTabControl(TabControl tabControl)
	{
		tabControl.BackColor = Window;
		tabControl.Appearance = TabAppearance.FlatButtons;
		tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
		tabControl.SizeMode = TabSizeMode.Fixed;
		tabControl.ItemSize = new Size(128, 30);
		if (!TabControls.Contains(tabControl))
		{
			TabControls.Add(tabControl);
			tabControl.DrawItem += DrawTab;
		}
	}

	private static void UseTitleBarTheme(Form form)
	{
		if (!form.IsHandleCreated)
		{
			form.HandleCreated += delegate { UseTitleBarTheme(form); };
			return;
		}
		int value = IsDark ? 1 : 0;
		DwmSetWindowAttribute(form.Handle, 20, ref value, sizeof(int));
		DwmSetWindowAttribute(form.Handle, 19, ref value, sizeof(int));
	}

	public static void ApplyDarkControlChrome(Control control)
	{
		if (!control.IsHandleCreated)
		{
			control.HandleCreated += delegate { ApplyDarkControlChrome(control); };
			return;
		}
		ApplyDarkWindowTheme(control.Handle);
	}

	private static void ApplyDarkComboDropDown(ComboBox comboBox)
	{
		if (!comboBox.IsHandleCreated)
		{
			comboBox.HandleCreated += delegate { ApplyDarkComboDropDown(comboBox); };
			return;
		}
		ComboBoxInfo comboBoxInfo = new ComboBoxInfo();
		comboBoxInfo.cbSize = Marshal.SizeOf(typeof(ComboBoxInfo));
		if (GetComboBoxInfo(comboBox.Handle, ref comboBoxInfo))
		{
			ApplyDarkWindowTheme(comboBoxInfo.hwndList);
		}
	}

	public static void ApplyDarkWindowTheme(IntPtr hwnd)
	{
		if (hwnd != IntPtr.Zero)
		{
			SetWindowTheme(hwnd, IsDark ? "DarkMode_Explorer" : null, null);
		}
	}

	internal static Color GetButtonHoverColor(Button button)
	{
		if (button.Name == "StartButton")
		{
			return GreenHover;
		}
		if (button.Name == "StopButton")
		{
			return RedHover;
		}
		return Hover;
	}

	private static void DrawComboBoxItem(object sender, DrawItemEventArgs e)
	{
		if (e.Index < 0)
		{
			return;
		}
		ComboBox comboBox = (ComboBox)sender;
		bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
		using (Brush backBrush = new SolidBrush(selected ? Selected : Input))
		using (Brush textBrush = new SolidBrush(Text))
		{
			e.Graphics.FillRectangle(backBrush, e.Bounds);
			string value = comboBox.GetItemText(comboBox.Items[e.Index]);
			Rectangle textBounds = new Rectangle(e.Bounds.X + 6, e.Bounds.Y, e.Bounds.Width - 8, e.Bounds.Height);
			e.Graphics.DrawString(value, comboBox.Font, textBrush, textBounds);
		}
		e.DrawFocusRectangle();
	}

	private static void DrawTab(object sender, DrawItemEventArgs e)
	{
		TabControl tabControl = (TabControl)sender;
		bool selected = e.Index == tabControl.SelectedIndex;
		Rectangle bounds = e.Bounds;
		using (Brush backBrush = new SolidBrush(selected ? Selected : Color.FromArgb(42, 42, 42)))
		using (Brush textBrush = new SolidBrush(Text))
		{
			e.Graphics.FillRectangle(backBrush, bounds);
			StringFormat format = new StringFormat
			{
				Alignment = StringAlignment.Center,
				LineAlignment = StringAlignment.Center
			};
			e.Graphics.DrawString(tabControl.TabPages[e.Index].Text, tabControl.Font, textBrush, bounds, format);
		}
	}

	private static void DrawToolTip(object sender, DrawToolTipEventArgs e)
	{
		using (Brush backBrush = new SolidBrush(Input))
		using (Brush textBrush = new SolidBrush(Text))
		using (Pen borderPen = new Pen(Border))
		{
			e.Graphics.FillRectangle(backBrush, e.Bounds);
			e.Graphics.DrawRectangle(borderPen, new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1));
			e.Graphics.DrawString(e.ToolTipText, e.Font, textBrush, new PointF(e.Bounds.X + 6, e.Bounds.Y + 4));
		}
	}

	private static void ApplyToolStripItem(ToolStripItem item)
	{
		item.BackColor = Panel;
		item.ForeColor = Text;
		if (item is ToolStripMenuItem menuItem)
		{
			foreach (ToolStripItem child in menuItem.DropDownItems)
			{
				ApplyToolStripItem(child);
			}
		}
	}

	private sealed class DarkToolStripRenderer : ToolStripProfessionalRenderer
	{
		public DarkToolStripRenderer()
			: base(new DarkColorTable())
		{
		}

		protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
		{
			using (Pen pen = new Pen(Border))
			{
				e.Graphics.DrawLine(pen, 4, e.Item.Height / 2, e.Item.Width - 4, e.Item.Height / 2);
			}
		}
	}

	private sealed class DarkColorTable : ProfessionalColorTable
	{
		public override Color ToolStripDropDownBackground => Panel;
		public override Color ImageMarginGradientBegin => Panel;
		public override Color ImageMarginGradientMiddle => Panel;
		public override Color ImageMarginGradientEnd => Panel;
		public override Color MenuItemSelected => Hover;
		public override Color MenuItemBorder => Border;
		public override Color MenuBorder => Border;
		public override Color SeparatorDark => Border;
		public override Color SeparatorLight => Border;
	}

	[DllImport("dwmapi.dll")]
	private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

	[DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
	private static extern int SetWindowTheme(IntPtr hwnd, string subAppName, string subIdList);

	[DllImport("uxtheme.dll", EntryPoint = "#135")]
	private static extern int SetPreferredAppMode(int appMode);

	[DllImport("uxtheme.dll", EntryPoint = "#136")]
	private static extern void FlushMenuThemes();

	[DllImport("user32.dll")]
	private static extern bool GetComboBoxInfo(IntPtr hwndCombo, ref ComboBoxInfo info);

	[StructLayout(LayoutKind.Sequential)]
	private struct ComboBoxInfo
	{
		public int cbSize;
		public Rectangle rcItem;
		public Rectangle rcButton;
		public int stateButton;
		public IntPtr hwndCombo;
		public IntPtr hwndItem;
		public IntPtr hwndList;
	}
}

internal sealed class DarkComboBox : ComboBox
{
	private bool hovering;

	public DarkComboBox()
	{
		SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
		DrawMode = DrawMode.OwnerDrawFixed;
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);
		DarkTheme.ApplyDarkControlChrome(this);
	}

	protected override void OnMouseEnter(EventArgs e)
	{
		hovering = true;
		Invalidate();
		base.OnMouseEnter(e);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		hovering = false;
		Invalidate();
		base.OnMouseLeave(e);
	}

	protected override void OnSelectedIndexChanged(EventArgs e)
	{
		Invalidate();
		base.OnSelectedIndexChanged(e);
	}

	protected override void OnTextChanged(EventArgs e)
	{
		Invalidate();
		base.OnTextChanged(e);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Rectangle bounds = ClientRectangle;
		Rectangle buttonBounds = new Rectangle(bounds.Right - SystemInformation.HorizontalScrollBarArrowWidth - 1, 1, SystemInformation.HorizontalScrollBarArrowWidth, bounds.Height - 2);
		Rectangle textBounds = new Rectangle(6, 0, Math.Max(0, buttonBounds.Left - 8), bounds.Height);
		Color textColor = Enabled ? DarkTheme.TextColor : DarkTheme.DisabledTextColor;
		using (Brush inputBrush = new SolidBrush(DarkTheme.InputColor))
		using (Brush buttonBrush = new SolidBrush(hovering ? DarkTheme.HoverColor : DarkTheme.TabColor))
		using (Brush textBrush = new SolidBrush(textColor))
		using (Pen borderPen = new Pen(DarkTheme.BorderColor))
		using (Pen arrowPen = new Pen(textColor, 2f))
		{
			e.Graphics.FillRectangle(inputBrush, bounds);
			e.Graphics.FillRectangle(buttonBrush, buttonBounds);
			e.Graphics.DrawRectangle(borderPen, 0, 0, bounds.Width - 1, bounds.Height - 1);
			TextRenderer.DrawText(e.Graphics, Text, Font, textBounds, textColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
			Point middle = new Point(buttonBounds.Left + buttonBounds.Width / 2, buttonBounds.Top + buttonBounds.Height / 2);
			e.Graphics.DrawLines(arrowPen, new[] { new Point(middle.X - 4, middle.Y - 2), new Point(middle.X, middle.Y + 2), new Point(middle.X + 4, middle.Y - 2) });
		}
	}

}

internal sealed class DarkNumericUpDown : NumericUpDown
{
	private bool hovering;

	public DarkNumericUpDown()
	{
		SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
		BorderStyle = BorderStyle.FixedSingle;
		BackColor = DarkTheme.InputColor;
		ForeColor = DarkTheme.TextColor;
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);
		DarkTheme.ApplyDarkControlChrome(this);
	}

	protected override void OnMouseEnter(EventArgs e)
	{
		hovering = true;
		Invalidate();
		base.OnMouseEnter(e);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		hovering = false;
		Invalidate();
		base.OnMouseLeave(e);
	}

	protected override void OnValueChanged(EventArgs e)
	{
		Invalidate();
		base.OnValueChanged(e);
	}

	protected override void OnTextChanged(EventArgs e)
	{
		Invalidate();
		base.OnTextChanged(e);
	}

	protected override void OnEnabledChanged(EventArgs e)
	{
		Invalidate();
		base.OnEnabledChanged(e);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Rectangle bounds = ClientRectangle;
		int buttonWidth = SystemInformation.VerticalScrollBarWidth;
		Rectangle buttonBounds = new Rectangle(bounds.Right - buttonWidth - 1, 1, buttonWidth, bounds.Height - 2);
		Rectangle upBounds = new Rectangle(buttonBounds.Left, buttonBounds.Top, buttonBounds.Width, buttonBounds.Height / 2);
		Rectangle downBounds = new Rectangle(buttonBounds.Left, upBounds.Bottom, buttonBounds.Width, buttonBounds.Bottom - upBounds.Bottom);
		Rectangle textBounds = new Rectangle(4, 0, Math.Max(0, buttonBounds.Left - 6), bounds.Height);
		Color textColor = Enabled ? DarkTheme.TextColor : DarkTheme.DisabledTextColor;

		using (Brush inputBrush = new SolidBrush(DarkTheme.InputColor))
		using (Brush buttonBrush = new SolidBrush(hovering && Enabled ? DarkTheme.HoverColor : DarkTheme.TabColor))
		using (Pen borderPen = new Pen(DarkTheme.BorderColor))
		using (Brush arrowBrush = new SolidBrush(textColor))
		{
			e.Graphics.FillRectangle(inputBrush, bounds);
			e.Graphics.FillRectangle(buttonBrush, buttonBounds);
			e.Graphics.DrawRectangle(borderPen, 0, 0, bounds.Width - 1, bounds.Height - 1);
			e.Graphics.DrawLine(borderPen, buttonBounds.Left, buttonBounds.Top, buttonBounds.Left, buttonBounds.Bottom);
			e.Graphics.DrawLine(borderPen, buttonBounds.Left, upBounds.Bottom, buttonBounds.Right, upBounds.Bottom);
			TextRenderer.DrawText(e.Graphics, Text, Font, textBounds, textColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Right | TextFormatFlags.EndEllipsis);
			DrawTriangle(e.Graphics, arrowBrush, upBounds, true);
			DrawTriangle(e.Graphics, arrowBrush, downBounds, false);
		}
	}

	private static void DrawTriangle(Graphics graphics, Brush brush, Rectangle bounds, bool up)
	{
		int centerX = bounds.Left + bounds.Width / 2;
		int centerY = bounds.Top + bounds.Height / 2;
		Point[] points = up
			? new[] { new Point(centerX, centerY - 3), new Point(centerX - 4, centerY + 2), new Point(centerX + 4, centerY + 2) }
			: new[] { new Point(centerX, centerY + 3), new Point(centerX - 4, centerY - 2), new Point(centerX + 4, centerY - 2) };
		graphics.FillPolygon(brush, points);
	}
}

internal sealed class DarkButton : Button
{
	private bool hovering;
	private bool pressed;

	public DarkButton()
	{
		SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
		FlatStyle = FlatStyle.Flat;
		UseVisualStyleBackColor = false;
	}

	protected override void OnMouseEnter(EventArgs e)
	{
		hovering = true;
		Invalidate();
		base.OnMouseEnter(e);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		hovering = false;
		pressed = false;
		Invalidate();
		base.OnMouseLeave(e);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		pressed = true;
		Invalidate();
		base.OnMouseDown(e);
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		pressed = false;
		Invalidate();
		base.OnMouseUp(e);
	}

	protected override void OnEnabledChanged(EventArgs e)
	{
		Invalidate();
		base.OnEnabledChanged(e);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Color backColor = Enabled ? DarkTheme.ButtonColor : DarkTheme.DisabledButtonColor;
		Color borderColor = Enabled ? DarkTheme.BorderColor : DarkTheme.DisabledBorderColor;
		if (pressed && Enabled)
		{
			backColor = DarkTheme.SelectedColor;
		}
		else if (hovering && Enabled)
		{
			backColor = DarkTheme.GetButtonHoverColor(this);
		}
		using (Brush backBrush = new SolidBrush(backColor))
		using (Pen borderPen = new Pen(borderColor))
		{
			e.Graphics.FillRectangle(backBrush, ClientRectangle);
			e.Graphics.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);
		}
		TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, Enabled ? DarkTheme.TextColor : DarkTheme.DisabledTextColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
	}
}

internal sealed class DarkCheckBox : CheckBox
{
	private bool hovering;

	public DarkCheckBox()
	{
		SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
	}

	protected override void OnMouseEnter(EventArgs e)
	{
		hovering = true;
		Invalidate();
		base.OnMouseEnter(e);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		hovering = false;
		Invalidate();
		base.OnMouseLeave(e);
	}

	protected override void OnCheckedChanged(EventArgs e)
	{
		Invalidate();
		base.OnCheckedChanged(e);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		e.Graphics.Clear(DarkTheme.PanelColor);
		Rectangle box = new Rectangle(1, Math.Max(1, (Height - 14) / 2), 14, 14);
		using (Brush boxBrush = new SolidBrush(hovering ? DarkTheme.HoverColor : DarkTheme.InputColor))
		using (Pen borderPen = new Pen(DarkTheme.BorderColor))
		using (Pen checkPen = new Pen(DarkTheme.TextColor, 2f))
		{
			e.Graphics.FillRectangle(boxBrush, box);
			e.Graphics.DrawRectangle(borderPen, box);
			if (Checked)
			{
				e.Graphics.DrawLines(checkPen, new[]
				{
					new Point(box.Left + 3, box.Top + 7),
					new Point(box.Left + 6, box.Top + 10),
					new Point(box.Left + 11, box.Top + 4)
				});
			}
		}
		Rectangle textBounds = new Rectangle(box.Right + 6, 0, Math.Max(0, Width - box.Right - 6), Height);
		TextRenderer.DrawText(e.Graphics, Text, Font, textBounds, DarkTheme.TextColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
	}
}

internal sealed class DarkRichTextBox : RichTextBox
{
	public DarkRichTextBox()
	{
		BorderStyle = BorderStyle.FixedSingle;
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);
		DarkTheme.ApplyDarkControlChrome(this);
	}
}

internal sealed class DarkTabControl : TabControl
{
	public DarkTabControl()
	{
		SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
		DrawMode = TabDrawMode.OwnerDrawFixed;
		Appearance = TabAppearance.FlatButtons;
		SizeMode = TabSizeMode.Fixed;
		ItemSize = new Size(128, 30);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		using (Brush windowBrush = new SolidBrush(DarkTheme.WindowColor))
		using (Brush panelBrush = new SolidBrush(DarkTheme.PanelColor))
		using (Pen borderPen = new Pen(DarkTheme.BorderColor))
		{
			e.Graphics.FillRectangle(windowBrush, ClientRectangle);
			Rectangle body = DisplayRectangle;
			body.Inflate(4, 4);
			e.Graphics.FillRectangle(panelBrush, body);
			e.Graphics.DrawRectangle(borderPen, body.X, body.Y, body.Width - 1, body.Height - 1);
		}
		for (int i = 0; i < TabPages.Count; i++)
		{
			DrawTab(e.Graphics, i);
		}
	}

	private void DrawTab(Graphics graphics, int index)
	{
		Rectangle bounds = GetTabRect(index);
		bool selected = index == SelectedIndex;
		using (Brush backBrush = new SolidBrush(selected ? DarkTheme.SelectedColor : DarkTheme.TabColor))
		using (Pen borderPen = new Pen(DarkTheme.BorderColor))
		{
			graphics.FillRectangle(backBrush, bounds);
			graphics.DrawRectangle(borderPen, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
		}
		TextRenderer.DrawText(graphics, TabPages[index].Text, Font, bounds, DarkTheme.TextColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
	}
}
