using static WinPos.NativeMethods;

namespace WinPos;

public partial class MainForm : Form
{
    private bool[] _modifiers;
    private uint _key = VK_SUBTRACT;
    private Keys _keyCode;

    public MainForm()
    {
        InitializeComponent();
        _modifiers = [btnCtrl.Checked, btnShift.Checked, btnWin.Checked, btnAlt.Checked];
        _keyCode = Keys.Subtract;

        InitializeTrayIcon();

        WindowPositionManager.LoadFromDisk();
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_HOTKEY)
        {
            if (m.WParam.ToInt32() == WindowPositionManager.HOTKEY_ID)
                WindowPositionManager.RestoreWindowPositions();
        }

        base.WndProc(ref m);
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        WindowPositionManager.RegisterHotKey(Handle, false, true, true, false, _key);
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        Hide();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (WindowState == FormWindowState.Minimized)
            Hide();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            Hide();
        }
        else
            UnregisterHotKey(Handle, WindowPositionManager.HOTKEY_ID);

        base.OnFormClosing(e);
    }

    private void InitializeTrayIcon()
    {
        notifyIcon1.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

        notifyIcon1.Text =
            $"{string.Join(" + ", _modifiers.Select((m, i) => m ?
            new[] { "Ctrl", "Shift", "Win", "Alt" }[i] : null)
            .Where(m => m != null))} + {_keyCode}";

        notifyIcon1.DoubleClick += NotifyIcon_DoubleClick;

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("Save Window Positions", null, (s, e) => WindowPositionManager.SaveWindowPositions());
        contextMenu.Items.Add("Restore Window Positions", null, (s, e) => WindowPositionManager.RestoreWindowPositions());
        contextMenu.Items.Add("Exit", null, (s, e) => Application.Exit());
        notifyIcon1.ContextMenuStrip = contextMenu;
    }

    private void NotifyIcon_DoubleClick(object? sender, EventArgs? e)
    {
        Show();
        WindowState = FormWindowState.Normal;
    }

    private void TxtKey_KeyDown(object sender, KeyEventArgs e)
    {
        txtKey.Text = $"+ {e.KeyCode}";
        _key = (uint)e.KeyValue;
    }

    private void BtnOK_Click(object sender, EventArgs e)
    {
        UnregisterHotKey(Handle, WindowPositionManager.HOTKEY_ID);

        WindowPositionManager.RegisterHotKey(Handle,
            btnCtrl.Checked, btnShift.Checked, btnWin.Checked, btnAlt.Checked, _key);

        _modifiers = [btnCtrl.Checked, btnShift.Checked, btnWin.Checked, btnAlt.Checked];
        _keyCode = Enum.Parse<Keys>(txtKey.Text.TrimStart(['+', ' ']));

        notifyIcon1.Text =
            $"{string.Join(" + ", _modifiers.Select((m, i) => m ?
            new[] { "Ctrl", "Shift", "Win", "Alt" }[i] : null)
            .Where(m => m != null))} + {_keyCode}";

        Hide();
    }

    private void BtnCancel_Click(object sender, EventArgs e)
    {
        btnCtrl.Checked = _modifiers[0];
        btnShift.Checked = _modifiers[1];
        btnWin.Checked = _modifiers[2];
        btnAlt.Checked = _modifiers[3];
        txtKey.Text = $"+ {_keyCode}";

        Hide();
    }

    public class WindowInfo
    {
        public IntPtr Handle { get; }
        public int Left { get; set; }
        public int Top { get; set; }
        public int Right { get; set; }
        public int Bottom { get; set; }
        public string? WindowTitle { get; set; }
        public string? ExecutableName { get; set; }

        public WindowInfo() { }

        public WindowInfo(IntPtr handle, RECT rect, string title, string exeName)
        {
            Handle = handle;
            Left = rect.Left;
            Top = rect.Top;
            Right = rect.Right;
            Bottom = rect.Bottom;
            WindowTitle = title;
            ExecutableName = exeName;
        }

        public RECT Rect => new RECT
        {
            Left = Left,
            Top = Top,
            Right = Right,
            Bottom = Bottom
        };
    }

    private void Btn_CheckedChanged(object sender, EventArgs e)
    {
        ToolStripButton button = (ToolStripButton)sender;
        button.ForeColor = button.Checked ? Color.Black : Color.Gray;
    }
}
