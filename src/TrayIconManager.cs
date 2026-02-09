using System;
using System.Drawing;
using System.Windows.Forms;
using TouchKeyboardMouse.Helpers;

namespace TouchKeyboardMouse;

/// <summary>
/// Manages the system tray icon for the application
/// </summary>
public class TrayIconManager : IDisposable
{
    private NotifyIcon? _notifyIcon;
    private readonly MainWindow _mainWindow;
    private ToolStripMenuItem? _postureMenuItem;

    public TrayIconManager(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
        InitializeTrayIcon();
    }

    private void InitializeTrayIcon()
    {
        _notifyIcon = new NotifyIcon
        {
            // Use a built-in icon (keyboard-like)
            Icon = SystemIcons.Application,
            Visible = true,
            Text = "Touch Keyboard & Mouse"
        };

        // Create context menu
        var contextMenu = new ContextMenuStrip();
        
        _postureMenuItem = new ToolStripMenuItem("Posture: Detecting...")
        {
            Enabled = false
        };
        contextMenu.Items.Add(_postureMenuItem);
        contextMenu.Items.Add(new ToolStripSeparator());
        
        var showItem = new ToolStripMenuItem("Show Keyboard", null, (s, e) => ShowMainWindow());
        contextMenu.Items.Add(showItem);
        
        var settingsItem = new ToolStripMenuItem("Settings", null, (s, e) => OpenSettings());
        contextMenu.Items.Add(settingsItem);
        
        contextMenu.Items.Add(new ToolStripSeparator());
        
        var exitItem = new ToolStripMenuItem("Exit", null, (s, e) => ExitApplication());
        contextMenu.Items.Add(exitItem);

        _notifyIcon.ContextMenuStrip = contextMenu;
        _notifyIcon.DoubleClick += (s, e) => ShowMainWindow();
    }

    public void UpdatePostureStatus(DevicePosture posture, ScreenArrangement arrangement)
    {
        if (_postureMenuItem == null || _notifyIcon == null) return;

        var postureText = posture switch
        {
            DevicePosture.BookMode => "Book Mode (Keyboard Active)",
            DevicePosture.LaptopMode => "Side-by-Side (Keyboard Hidden)",
            DevicePosture.SingleScreen => "Single Screen",
            _ => "Detecting..."
        };

        _postureMenuItem.Text = $"Posture: {postureText}";

        // Update tooltip
        _notifyIcon.Text = posture == DevicePosture.LaptopMode 
            ? "TKM - Hidden (Side-by-Side mode)" 
            : "Touch Keyboard & Mouse";

        // Show balloon notification when hiding
        if (posture == DevicePosture.LaptopMode)
        {
            _notifyIcon.ShowBalloonTip(
                2000,
                "Keyboard Hidden",
                "Screens are side-by-side. Keyboard will reappear when you switch to book mode.",
                ToolTipIcon.Info);
        }
    }

    private void ShowMainWindow()
    {
        _mainWindow.Show();
        // Restore to last user state (windowed or fullscreen)
        if (_mainWindow.WindowState == System.Windows.WindowState.Minimized)
        {
            _mainWindow.WindowState = _mainWindow._isFullscreen ? System.Windows.WindowState.Maximized : System.Windows.WindowState.Normal;
        }
        _mainWindow.Activate();
    }

    private void OpenSettings()
    {
        ShowMainWindow();
        // Toggle the settings sidebar
        _mainWindow.ToggleSettings();
    }

    private void ExitApplication()
    {
        _mainWindow.Close();
    }

    public void Dispose()
    {
        if (_notifyIcon != null)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _notifyIcon = null;
        }
    }
}
