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
    private bool _isFullscreen = true;
    private bool _settingsInitialized = false;
    
    // Trackpad resize state
    private bool _isResizingTrackpad = false;
    private Point _resizeStartPoint;
    private double _resizeStartWidth;

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

        // Handle window resize to update resize grips position
        this.SizeChanged += MainWindow_SizeChanged;

        // Mark settings as initialized (prevents slider events during load)
        _settingsInitialized = true;
        
        // Initialize trackpad width
        UpdateTrackpadWidth();

        // Initial positioning based on current posture
        UpdateWindowForPosture();
    }

    private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        // Update trackpad resize grip positions when window is resized
        UpdateResizeGripPositions();
    }
    
    private void UpdateTrackpadWidth()
    {
        // Default to full width
        var availableWidth = TrackpadContainer.ActualWidth;
        if (availableWidth > 0)
        {
            Trackpad.Width = availableWidth;
        }
        UpdateResizeGripPositions();
    }
    
    private void UpdateResizeGripPositions()
    {
        if (Trackpad == null || TrackpadContainer == null) return;
        if (TrackpadContainer.ActualWidth <= 0) return;
        
        // Get trackpad width (handle NaN for auto-sized)
        var trackpadWidth = double.IsNaN(Trackpad.Width) ? Trackpad.ActualWidth : Trackpad.Width;
        if (trackpadWidth <= 0) trackpadWidth = TrackpadContainer.ActualWidth;
        
        // Position grips at the edges of the trackpad
        var trackpadLeft = (TrackpadContainer.ActualWidth - trackpadWidth) / 2;
        if (trackpadLeft < 0) trackpadLeft = 0;
        
        LeftResizeGrip.Margin = new Thickness(trackpadLeft - 4, 0, 0, 0);
        RightResizeGrip.Margin = new Thickness(0, 0, trackpadLeft - 4, 0);
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
                if (_isFullscreen)
                {
                    PositionOnScreen(keyboardScreen);
                }
                
                if (_isHiddenDueToPosture)
                {
                    this.Show();
                    if (_isFullscreen)
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
                if (_isFullscreen)
                {
                    PositionOnScreen(screens[0]);
                }
                if (_isHiddenDueToPosture)
                {
                    this.Show();
                    if (_isFullscreen)
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
            // Close settings sidebar if open, otherwise close app
            if (SettingsSidebar.Visibility == Visibility.Visible)
            {
                SettingsSidebar.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.Close();
            }
        }
        else if (e.Key == Key.F11)
        {
            ToggleFullscreen();
        }
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        ToggleSettings();
    }

    public void ToggleSettings()
    {
        SettingsSidebar.Visibility = SettingsSidebar.Visibility == Visibility.Visible 
            ? Visibility.Collapsed 
            : Visibility.Visible;
    }

    private void CloseSettings_Click(object sender, RoutedEventArgs e)
    {
        SettingsSidebar.Visibility = Visibility.Collapsed;
    }

    private void FullscreenButton_Click(object sender, RoutedEventArgs e)
    {
        ToggleFullscreen();
    }

    private void ToggleFullscreen()
    {
        if (_isFullscreen)
        {
            // Exit fullscreen
            this.WindowState = WindowState.Normal;
            
            // Set to a reasonable windowed size
            var workArea = SystemParameters.WorkArea;
            this.Width = Math.Min(1200, workArea.Width * 0.8);
            this.Height = Math.Min(700, workArea.Height * 0.8);
            this.Left = (workArea.Width - this.Width) / 2;
            this.Top = (workArea.Height - this.Height) / 2;
            
            FullscreenButton.Content = "⛶";
            FullscreenButton.ToolTip = "Enter Fullscreen (F11)";
            _isFullscreen = false;
        }
        else
        {
            // Enter fullscreen
            this.WindowState = WindowState.Maximized;
            FullscreenButton.Content = "⧉";
            FullscreenButton.ToolTip = "Exit Fullscreen (F11)";
            _isFullscreen = true;
        }
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    #region Settings Handlers

    private void CursorSensitivitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_settingsInitialized || CursorSensitivityValue == null) return;
        
        CursorSensitivityValue.Text = $"{e.NewValue:F1}x";
        Trackpad.Sensitivity = e.NewValue;
    }

    private void ScrollSensitivitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_settingsInitialized || ScrollSensitivityValue == null) return;
        
        ScrollSensitivityValue.Text = $"{e.NewValue:F1}x";
        Trackpad.ScrollSensitivity = e.NewValue;
    }

    private void TapThresholdSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_settingsInitialized || TapThresholdValue == null) return;
        
        var ms = (int)e.NewValue;
        TapThresholdValue.Text = $"{ms}ms";
        Trackpad.TapThresholdMs = ms;
    }

    private void AlwaysOnTopCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (!_settingsInitialized) return;
        this.Topmost = AlwaysOnTopCheckBox.IsChecked ?? true;
    }

    private void ResetDefaults_Click(object sender, RoutedEventArgs e)
    {
        CursorSensitivitySlider.Value = 1.5;
        ScrollSensitivitySlider.Value = 2.0;
        TapThresholdSlider.Value = 200;
        AlwaysOnTopCheckBox.IsChecked = true;
        
        // Reset trackpad to full width
        Trackpad.Width = TrackpadContainer.ActualWidth;
        UpdateResizeGripPositions();
    }
    
    #endregion
    
    #region Trackpad Resize Handlers
    
    private void ResizeGrip_MouseDown(object sender, MouseButtonEventArgs e)
    {
        _isResizingTrackpad = true;
        _resizeStartPoint = e.GetPosition(TrackpadContainer);
        _resizeStartWidth = Trackpad.Width;
        
        if (double.IsNaN(_resizeStartWidth))
            _resizeStartWidth = Trackpad.ActualWidth;
            
        ((Border)sender).CaptureMouse();
        e.Handled = true;
    }
    
    private void ResizeGrip_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isResizingTrackpad) return;
        
        var currentPoint = e.GetPosition(TrackpadContainer);
        var containerCenter = TrackpadContainer.ActualWidth / 2;
        var isLeftGrip = sender == LeftResizeGrip;
        
        // Calculate delta from center
        double delta;
        if (isLeftGrip)
        {
            // Left grip: moving left increases width, moving right decreases
            delta = _resizeStartPoint.X - currentPoint.X;
        }
        else
        {
            // Right grip: moving right increases width, moving left decreases
            delta = currentPoint.X - _resizeStartPoint.X;
        }
        
        // Apply symmetrical resize (delta applies to BOTH sides)
        var newWidth = _resizeStartWidth + (delta * 2);
        
        // Clamp to valid range
        var minWidth = 300.0;
        var maxWidth = TrackpadContainer.ActualWidth;
        newWidth = Math.Max(minWidth, Math.Min(maxWidth, newWidth));
        
        Trackpad.Width = newWidth;
        UpdateResizeGripPositions();
        
        e.Handled = true;
    }
    
    private void ResizeGrip_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_isResizingTrackpad)
        {
            _isResizingTrackpad = false;
            ((Border)sender).ReleaseMouseCapture();
            e.Handled = true;
        }
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
