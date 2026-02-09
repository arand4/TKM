using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using TouchKeyboardMouse.Helpers;

namespace TouchKeyboardMouse;

public partial class MainWindow : Window
{
    // Keep window from being focused to allow typing in other apps
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_NOACTIVATE = 0x08000000;
    private const int WS_EX_TOOLWINDOW = 0x00000080;

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    private DualScreenHelper? _dualScreenHelper;
    private TrayIconManager? _trayIconManager;
    private bool _isHiddenDueToPosture = false;

    public MainWindow()
    {
        InitializeComponent();
        
        // Handle keyboard shortcuts
        this.KeyDown += MainWindow_KeyDown;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // Set window as non-activating so it doesn't steal focus from other apps
        var hwnd = new WindowInteropHelper(this).Handle;
        var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW);

        // Initialize system tray icon
        _trayIconManager = new TrayIconManager(this);

        // Initialize dual-screen detection
        _dualScreenHelper = new DualScreenHelper();
        _dualScreenHelper.PostureChanged += OnPostureChanged;

        // Initial positioning based on current posture
        UpdateWindowForPosture();
    }

    private void OnPostureChanged(object? sender, PostureChangedEventArgs e)
    {
        // Run on UI thread
        Dispatcher.Invoke(() =>
        {
            UpdateWindowForPosture();
        });
    }

    private void UpdateWindowForPosture()
    {
        if (_dualScreenHelper == null) return;

        var posture = _dualScreenHelper.CurrentPosture;
        var arrangement = _dualScreenHelper.CurrentArrangement;

        // Update status in title bar
        UpdatePostureIndicator(posture, arrangement);

        if (_dualScreenHelper.ShouldShowKeyboard())
        {
            // Book mode - show keyboard on bottom screen
            var keyboardScreen = _dualScreenHelper.GetKeyboardScreen();
            if (keyboardScreen != null)
            {
                PositionOnScreen(keyboardScreen);
                
                if (_isHiddenDueToPosture)
                {
                    this.Show();
                    this.WindowState = WindowState.Maximized;
                    _isHiddenDueToPosture = false;
                }
            }
        }
        else if (posture == DevicePosture.LaptopMode)
        {
            // Side-by-side mode - hide the keyboard
            _isHiddenDueToPosture = true;
            this.Hide();
        }
        else if (posture == DevicePosture.SingleScreen)
        {
            // Single screen - show on that screen (user might want virtual keyboard anyway)
            var screens = _dualScreenHelper.GetScreens();
            if (screens.Count > 0)
            {
                PositionOnScreen(screens[0]);
                if (_isHiddenDueToPosture)
                {
                    this.Show();
                    this.WindowState = WindowState.Maximized;
                    _isHiddenDueToPosture = false;
                }
            }
        }
    }

    private void PositionOnScreen(ScreenInfo screen)
    {
        this.WindowState = WindowState.Normal;
        this.Left = screen.Bounds.Left;
        this.Top = screen.Bounds.Top;
        this.Width = screen.Bounds.Width;
        this.Height = screen.Bounds.Height;
        this.WindowState = WindowState.Maximized;
    }

    private void UpdatePostureIndicator(DevicePosture posture, ScreenArrangement arrangement)
    {
        // Update UI with current posture
        var postureText = posture switch
        {
            DevicePosture.BookMode => "📖 Book Mode",
            DevicePosture.LaptopMode => "💻 Side-by-Side",
            DevicePosture.SingleScreen => "🖥️ Single Screen",
            _ => ""
        };
        PostureStatusText.Text = postureText;

        // Update system tray icon with current posture
        _trayIconManager?.UpdatePostureStatus(posture, arrangement);
    }

    private void MainWindow_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            this.Close();
        }
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow();
        settingsWindow.Owner = this;
        settingsWindow.ShowDialog();
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    // Prevent window from being activated when clicking
    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        // Cleanup
        if (_dualScreenHelper != null)
        {
            _dualScreenHelper.PostureChanged -= OnPostureChanged;
            _dualScreenHelper.Dispose();
        }

        _trayIconManager?.Dispose();
        
        base.OnClosing(e);
    }
}
