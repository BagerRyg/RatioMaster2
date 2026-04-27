using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RatioMaster;

internal static class DarkTheme
{
	private static readonly Color Window = Color.FromArgb(31, 31, 31);
	private static readonly Color Panel = Color.FromArgb(38, 38, 38);
	private static readonly Color Input = Color.FromArgb(25, 25, 25);
	private static readonly Color Border = Color.FromArgb(70, 70, 70);
	private static readonly Color Hover = Color.FromArgb(58, 58, 58);
	private static readonly Color Selected = Color.FromArgb(72, 72, 72);
	private static readonly Color Text = Color.White;
	private static readonly Color MutedText = Color.FromArgb(210, 210, 210);
	private static readonly Color Accent = Color.FromArgb(86, 156, 214);
	private static readonly Color GreenHover = Color.FromArgb(36, 110, 63);
	private static readonly Color RedHover = Color.FromArgb(132, 45, 45);

	private static readonly HashSet<ComboBox> ComboBoxes = new HashSet<ComboBox>();
	private static readonly HashSet<TabControl> TabControls = new HashSet<TabControl>();
	private static readonly HashSet<ToolTip> ToolTips = new HashSet<ToolTip>();

	public static void Apply(Form form)
	{
		form.BackColor = Window;
		form.ForeColor = Text;
		form.Font = new Font("Segoe UI", form.Font.SizeInPoints);
		UseDarkTitleBar(form);
		Apply((Control)form);
	}

	public static void EnableDarkApplicationMode()
	{
		try
		{
			SetPreferredAppMode(2);
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

	private static void UseDarkTitleBar(Form form)
	{
		if (!form.IsHandleCreated)
		{
			form.HandleCreated += delegate { UseDarkTitleBar(form); };
			return;
		}
		int value = 1;
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

	public static void ApplyDarkWindowTheme(IntPtr hwnd)
	{
		if (hwnd != IntPtr.Zero)
		{
			SetWindowTheme(hwnd, "DarkMode_Explorer", null);
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
}

internal sealed class DarkComboBox : ComboBox
{
	private const int WM_CTLCOLORLISTBOX = 0x0134;
	private static readonly Color Input = Color.FromArgb(25, 25, 25);
	private static readonly Color Border = Color.FromArgb(88, 88, 88);
	private static readonly Color Hover = Color.FromArgb(58, 58, 58);
	private static readonly Color TextColor = Color.White;
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
		using (Brush inputBrush = new SolidBrush(Input))
		using (Brush buttonBrush = new SolidBrush(hovering ? Hover : Color.FromArgb(42, 42, 42)))
		using (Brush textBrush = new SolidBrush(Enabled ? TextColor : Color.FromArgb(120, 120, 120)))
		using (Pen borderPen = new Pen(Border))
		using (Pen arrowPen = new Pen(TextColor, 2f))
		{
			e.Graphics.FillRectangle(inputBrush, bounds);
			e.Graphics.FillRectangle(buttonBrush, buttonBounds);
			e.Graphics.DrawRectangle(borderPen, 0, 0, bounds.Width - 1, bounds.Height - 1);
			TextRenderer.DrawText(e.Graphics, Text, Font, textBounds, Enabled ? TextColor : Color.FromArgb(120, 120, 120), TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
			Point middle = new Point(buttonBounds.Left + buttonBounds.Width / 2, buttonBounds.Top + buttonBounds.Height / 2);
			e.Graphics.DrawLines(arrowPen, new[] { new Point(middle.X - 4, middle.Y - 2), new Point(middle.X, middle.Y + 2), new Point(middle.X + 4, middle.Y - 2) });
		}
	}

	protected override void WndProc(ref Message m)
	{
		base.WndProc(ref m);
		if (m.Msg == WM_CTLCOLORLISTBOX)
		{
			DarkTheme.ApplyDarkWindowTheme(m.LParam);
		}
	}
}

internal sealed class DarkButton : Button
{
	private static readonly Color Back = Color.FromArgb(45, 45, 45);
	private static readonly Color Border = Color.FromArgb(70, 70, 70);
	private static readonly Color DisabledBack = Color.FromArgb(30, 30, 30);
	private static readonly Color DisabledBorder = Color.FromArgb(48, 48, 48);
	private static readonly Color Down = Color.FromArgb(72, 72, 72);
	private static readonly Color TextColor = Color.White;
	private static readonly Color DisabledText = Color.FromArgb(105, 105, 105);
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
		Color backColor = Enabled ? Back : DisabledBack;
		Color borderColor = Enabled ? Border : DisabledBorder;
		if (pressed && Enabled)
		{
			backColor = Down;
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
		TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, Enabled ? TextColor : DisabledText, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
	}
}

internal sealed class DarkCheckBox : CheckBox
{
	private static readonly Color Panel = Color.FromArgb(38, 38, 38);
	private static readonly Color Input = Color.FromArgb(25, 25, 25);
	private static readonly Color Border = Color.FromArgb(88, 88, 88);
	private static readonly Color Hover = Color.FromArgb(58, 58, 58);
	private static readonly Color TextColor = Color.White;
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
		e.Graphics.Clear(Panel);
		Rectangle box = new Rectangle(1, Math.Max(1, (Height - 14) / 2), 14, 14);
		using (Brush boxBrush = new SolidBrush(hovering ? Hover : Input))
		using (Pen borderPen = new Pen(Border))
		using (Pen checkPen = new Pen(TextColor, 2f))
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
		TextRenderer.DrawText(e.Graphics, Text, Font, textBounds, TextColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
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
	private static readonly Color Window = Color.FromArgb(31, 31, 31);
	private static readonly Color Panel = Color.FromArgb(38, 38, 38);
	private static readonly Color Tab = Color.FromArgb(42, 42, 42);
	private static readonly Color Selected = Color.FromArgb(72, 72, 72);
	private static readonly Color Border = Color.FromArgb(88, 88, 88);
	private static readonly Color TextColor = Color.White;

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
		using (Brush windowBrush = new SolidBrush(Window))
		using (Brush panelBrush = new SolidBrush(Panel))
		using (Pen borderPen = new Pen(Border))
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
		using (Brush backBrush = new SolidBrush(selected ? Selected : Tab))
		using (Pen borderPen = new Pen(Border))
		{
			graphics.FillRectangle(backBrush, bounds);
			graphics.DrawRectangle(borderPen, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
		}
		TextRenderer.DrawText(graphics, TabPages[index].Text, Font, bounds, TextColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
	}
}
