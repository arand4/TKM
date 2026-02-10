using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using TouchKeyboardMouse.Helpers;

namespace TouchKeyboardMouse
{
public partial class MainWindow : Window
{
    // UI controls
    private Button FullscreenButton;
    private Button MinimizeButton;
    private Button CloseButton;
    private Button SettingsButton;
    private Button NumpadButton;
    private TouchKeyboardMouse.Controls.Trackpad Trackpad;
    private Grid TrackpadArea;
    private Grid TrackpadWithGrips;
    private Border SettingsSidebar;
    private Border NumpadPanel;
    private ColumnDefinition NumpadColumn;
    private Grid NumpadGrid;
    private TouchKeyboardMouse.Controls.VirtualKeyboard Keyboard;
    private TextBlock PostureStatusText;
    private TextBlock CursorSensitivityValue;
    private TextBlock ScrollSensitivityValue;
    private TextBlock TapThresholdValue;
    // Workaround for WPF/WindowChrome bug: restore interactivity after state change
    private void RestoreTitlebarInteractivity()
    {
        try
        {
            if (FullscreenButton != null)
            {
                FullscreenButton.IsEnabled = true;
                FullscreenButton.IsHitTestVisible = true;
            }
            if (MinimizeButton != null)
            {
                MinimizeButton.IsEnabled = true;
                MinimizeButton.IsHitTestVisible = true;
            }
            if (CloseButton != null)
            {
                CloseButton.IsEnabled = true;
                CloseButton.IsHitTestVisible = true;
            }
            if (SettingsButton != null)
            {
                SettingsButton.IsEnabled = true;
                SettingsButton.IsHitTestVisible = true;
            }
            if (NumpadButton != null)
            {
                NumpadButton.IsEnabled = true;
                NumpadButton.IsHitTestVisible = true;
            }
            if (Trackpad != null)
            {
                Trackpad.IsEnabled = true;
                Trackpad.IsHitTestVisible = true;
            }
            if (TrackpadArea != null)
            {
                TrackpadArea.IsEnabled = true;
                TrackpadArea.IsHitTestVisible = true;
            }
        }
        catch { /* Ignore */ }
    }
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using TouchKeyboardMouse.Helpers;

namespace TouchKeyboardMouse
{
public partial class MainWindow : Window
{
    // Prevent recursive state changes
    private bool _handlingStateChange = false;

    private void MainWindow_StateChanged(object? sender, EventArgs e)
    {
        if (_handlingStateChange) return;
        _handlingStateChange = true;
        try
        {
            if (this.WindowState == WindowState.Maximized && !IsFullscreen)
            {
                // User maximized window, treat as fullscreen
                IsFullscreen = true;
                FullscreenButton.Content = "⧉";
                FullscreenButton.ToolTip = "Exit Fullscreen (F11)";
            }
            else if (this.WindowState == WindowState.Normal && IsFullscreen)
            {
                // User restored window, treat as windowed
                IsFullscreen = false;
                FullscreenButton.Content = "⛶";
                FullscreenButton.ToolTip = "Enter Fullscreen (F11)";
            }
            if (_appState != null)
            {
                _appState.IsFullscreen = IsFullscreen;
                AppStateHelper.Save(_appState);
            }
            this.UpdateWindowActivationStyle();
        }
        finally
        {
            _handlingStateChange = false;
            RestoreTitlebarInteractivity();
        }
    }
    // Dynamically update window activation style based on fullscreen/windowed mode
    public void UpdateWindowActivationStyle()
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        if (IsFullscreen)
        {
            // Remove NOACTIVATE and TOOLWINDOW for fullscreen
            extendedStyle &= ~WS_EX_NOACTIVATE;
            extendedStyle &= ~WS_EX_TOOLWINDOW;
        }
        else
        {
            // Add NOACTIVATE and TOOLWINDOW for windowed mode
            extendedStyle |= WS_EX_NOACTIVATE;
            extendedStyle |= WS_EX_TOOLWINDOW;
        }
        SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle);
    }
    // ...existing code...
    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        try { this.WindowState = WindowState.Minimized; } catch { }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        try { this.Close(); } catch { }
    }

    private void ResizeGrip_MouseUp(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (_isResizingTrackpad)
            {
                _isResizingTrackpad = false;
                (sender as Border)?.ReleaseMouseCapture();
                e.Handled = true;
            }
        }
        catch { /* Ignore */ }
    }
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
    public bool IsFullscreen { get; set; } = false;

    // ...existing code...
    private bool _settingsInitialized = false;
    private bool _numpadVisible = false;
    private bool _trackpadWasFullWidth = true;
    
    // Trackpad resize state
    private bool _isResizingTrackpad = false;
    private Point _resizeStartPoint;
    private double _resizeStartWidth;

    private AppState? _appState;
    public MainWindow()
    {
        InitializeComponent();
        this.KeyDown += MainWindow_KeyDown;
        _appState = AppStateHelper.Load();
        // Default to windowed mode unless persisted fullscreen
        IsFullscreen = _appState != null && _appState.IsFullscreen;
        if (IsFullscreen)
        {
            this.WindowState = WindowState.Maximized;
        }
        else
        {
            this.WindowState = WindowState.Normal;
            if (_appState != null && _appState.WindowWidth > 0 && _appState.WindowHeight > 0)
            {
                this.Width = _appState.WindowWidth;
                this.Height = _appState.WindowHeight;
            }
            if (_appState != null && _appState.WindowLeft >= 0 && _appState.WindowTop >= 0)
            {
                this.Left = _appState.WindowLeft;
                this.Top = _appState.WindowTop;
            }
        }
        // Restore trackpad width
        if (_appState != null && _appState.TrackpadWidth > 0 && Trackpad != null)
            Trackpad.Width = _appState.TrackpadWidth;
        // Restore numpad state
        _numpadVisible = _appState != null && _appState.NumpadVisible;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // Set window as non-activating so it doesn't steal focus from other apps (only in windowed mode)
        this.UpdateWindowActivationStyle();

        // Initialize system tray icon
        _trayIconManager = new TrayIconManager(this);

        // Initialize dual-screen detection

        // Handle window resize to update resize grips position
        this.SizeChanged += MainWindow_SizeChanged;
        this.StateChanged += MainWindow_StateChanged;

        // Mark settings as initialized (prevents slider events during load)
        _settingsInitialized = true;
        
        // Initialize trackpad width
        UpdateTrackpadWidth();

        // Initial positioning based on current posture
    }

    private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        try
        {
            // Update trackpad width if it was at full width
            if (_trackpadWasFullWidth)
            {
                UpdateTrackpadToFullWidth();
            }
            // Save window size and position
            if (this.WindowState == WindowState.Normal)
            {
                if (_appState != null)
                {
                    _appState.WindowWidth = this.Width;
                    _appState.WindowHeight = this.Height;
                    _appState.WindowLeft = this.Left;
                    _appState.WindowTop = this.Top;
                    AppStateHelper.Save(_appState);
                }
            }
            RestoreTitlebarInteractivity();
        }
        catch { /* Ignore */ }
    }
    
    private void UpdateTrackpadWidth()
    {
        // Set trackpad to full width on startup
        try { UpdateTrackpadToFullWidth(); } catch { }
    }
    
    private void UpdateTrackpadToFullWidth()
    {
        try
        {
            if (TrackpadArea == null || Trackpad == null || TrackpadWithGrips == null) return;
        
            var availableWidth = TrackpadArea.ActualWidth - 24; // Subtract grip widths
            if (availableWidth > 300)
            {
                Trackpad.Width = availableWidth;
                _trackpadWasFullWidth = true;
            }
        }
        catch { /* Ignore */ }
    }
    
    private bool IsTrackpadAtFullWidth()
    {
        try
        {
            if (TrackpadArea == null || Trackpad == null) return true;
            
            var availableWidth = TrackpadArea.ActualWidth - 24;
            var trackpadWidth = double.IsNaN(Trackpad.Width) ? Trackpad.ActualWidth : Trackpad.Width;
            
            // Consider "full width" if within 50 pixels of available
            return trackpadWidth >= availableWidth - 50;
        }
        catch { return true; }
    }



    private void PositionOnScreen(ScreenInfo screen)
    {
        this.WindowState = WindowState.Normal;
        this.Left = screen.Bounds.Left;
        this.Top = screen.Bounds.Top;
        this.Width = screen.Bounds.Width;
        this.Height = screen.Bounds.Height;
        if (IsFullscreen)
            this.WindowState = WindowState.Maximized;
    }


    private void MainWindow_KeyDown(object sender, KeyEventArgs e)
    {
        try
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
        catch { /* Ignore */ }
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        try { ToggleSettings(); } catch { }
    }
    
    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (e.ClickCount == 2)
            {
                // Double-click to toggle fullscreen
                ToggleFullscreen();
            }
            else if (WindowState != WindowState.Maximized)
            {
                // Drag window when not maximized
                this.DragMove();
            }
        }
        catch { /* Ignore drag errors */ }
    }
    
    private void NumpadButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _numpadVisible = !_numpadVisible;
            
            if (_numpadVisible)
            {
                // Remember if trackpad was at full width before showing numpad
                _trackpadWasFullWidth = IsTrackpadAtFullWidth();
                
                NumpadColumn.Width = new GridLength(200);
                NumpadPanel.Visibility = Visibility.Visible;
                NumpadButton.Content = "⌨";
                
                // If trackpad was at full width, adjust it to fit the new available space
                if (_trackpadWasFullWidth)
                {
                    // Wait for layout to update, then set full width
                    Dispatcher.BeginInvoke(new Action(() => UpdateTrackpadToFullWidth()), 
                        System.Windows.Threading.DispatcherPriority.Loaded);
                }
            }
            else
            {
                NumpadColumn.Width = new GridLength(0);
                NumpadPanel.Visibility = Visibility.Collapsed;
                NumpadButton.Content = "#";
                
                // If trackpad was at full width before numpad was shown, restore full width
                if (_trackpadWasFullWidth)
                {
                    // Wait for layout to update, then set full width
                    Dispatcher.BeginInvoke(new Action(() => UpdateTrackpadToFullWidth()), 
                        System.Windows.Threading.DispatcherPriority.Loaded);
                }
            }
        }
        catch { /* Ignore */ }
    }
    
    private void Numpad_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is Button button && button.Tag is string keyName)
            {
                var keyCode = VirtualKeyCodes.GetKeyCode(keyName);
                if (keyCode != 0)
                {
                    InputSimulator.SendKey(keyCode);
                }
            }
        }
        catch { /* Ignore */ }
    }
    
    private void NumpadPanel_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        try
        {
            // Make numpad width match its height to keep buttons square
            // 4 rows of buttons, so width should be height (for 4 columns)
            if (NumpadPanel != null && NumpadPanel.ActualHeight > 0)
            {
                var buttonHeight = (NumpadPanel.ActualHeight - 10) / 4; // 4 rows, minus margins
                var desiredWidth = (buttonHeight * 4) + 10; // 4 columns of square buttons
                NumpadColumn.Width = new GridLength(Math.Max(desiredWidth, 150));
            }
        }
        catch { /* Ignore */ }
    }

    public void ToggleSettings()
    {
        SettingsSidebar.Visibility = SettingsSidebar.Visibility == Visibility.Visible 
            ? Visibility.Collapsed 
            : Visibility.Visible;
    }

    private void CloseSettings_Click(object sender, RoutedEventArgs e)
    {
        try { SettingsSidebar.Visibility = Visibility.Collapsed; } catch { }
    }

    private void FullscreenButton_Click(object sender, RoutedEventArgs e)
    {
        try { ToggleFullscreen(); } catch { }
    }

    private void ToggleFullscreen()
    {
        try
        {
            if (IsFullscreen)
            {
                // Exit fullscreen
                this.WindowState = WindowState.Normal;
                var workArea = SystemParameters.WorkArea;
                this.Width = Math.Min(1200, workArea.Width * 0.8);
                this.Height = Math.Min(700, workArea.Height * 0.8);
                this.Left = (workArea.Width - this.Width) / 2;
                this.Top = (workArea.Height - this.Height) / 2;
                FullscreenButton.Content = "⛶";
                FullscreenButton.ToolTip = "Enter Fullscreen (F11)";
                IsFullscreen = false;
            }
            else
            {
                // Enter fullscreen
                this.WindowState = WindowState.Maximized;
                FullscreenButton.Content = "⧉";
                FullscreenButton.ToolTip = "Exit Fullscreen (F11)";
                IsFullscreen = true;
            }
            if (_appState != null)
            {
                _appState.IsFullscreen = IsFullscreen;
                AppStateHelper.Save(_appState);
            }
        }
        catch { /* Ignore */ }
        this.UpdateWindowActivationStyle();
        RestoreTitlebarInteractivity();
    }
    // ...existing code...


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
        _trackpadWasFullWidth = true;
        UpdateTrackpadToFullWidth();
    }
    
    #endregion
    
    #region Trackpad Resize Handlers
    
    private void ResizeGrip_MouseDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (TrackpadArea == null || Trackpad == null) return;
            
            _isResizingTrackpad = true;
            _resizeStartPoint = e.GetPosition(TrackpadArea);
            _resizeStartWidth = Trackpad.Width;
            
            if (double.IsNaN(_resizeStartWidth))
                _resizeStartWidth = Trackpad.ActualWidth;
            if (_resizeStartWidth <= 0)
                _resizeStartWidth = TrackpadArea.ActualWidth - 24;
                
            (sender as Border)?.CaptureMouse();
            e.Handled = true;
        }
        catch { /* Ignore resize errors */ }
    }
    
    private void ResizeGrip_MouseMove(object sender, MouseEventArgs e)
    {
        try
        {
            if (!_isResizingTrackpad || TrackpadArea == null || Trackpad == null) return;
            var currentPoint = e.GetPosition(TrackpadArea);
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
            var maxWidth = TrackpadArea.ActualWidth - 24; // Account for grip widths
            if (maxWidth <= minWidth) return;
            newWidth = Math.Max(minWidth, Math.Min(maxWidth, newWidth));
            Trackpad.Width = newWidth;
            _trackpadWasFullWidth = false; // User is manually resizing
            if (_appState != null)
            {
                _appState.TrackpadWidth = newWidth;
                AppStateHelper.Save(_appState);
            }
            e.Handled = true;
        }
        catch { /* Ignore resize errors */ }
    }
    
    #endregion

    // Prevent window from being activated when clicking
    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        // Cleanup

        _trayIconManager?.Dispose();
        
        base.OnClosing(e);
    }
}
}
