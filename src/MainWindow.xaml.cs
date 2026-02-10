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
    // UI controls are auto-generated from XAML
    // No custom activation or fullscreen logic needed

    private void MainWindow_StateChanged(object? sender, EventArgs e)
    {
        // Re-apply event handlers and visibility for controls after state change
        try
        {
            // Restore numpad visibility and column width
            NumpadPanel.Visibility = _numpadVisible ? Visibility.Visible : Visibility.Collapsed;
            NumpadColumn.Width = _numpadVisible ? new GridLength(1, GridUnitType.Auto) : new GridLength(0);
            TouchKeyboardMouse.Helpers.AppLogger.Log($"MainWindow_StateChanged: NumpadVisible={_numpadVisible}, NumpadColumn.Width={NumpadColumn.Width.Value}");
            // Restore settings sidebar visibility
            SettingsSidebar.Visibility = SettingsSidebar.Visibility;
            // Re-attach resize event handlers if needed
            LeftResizeGrip.MouseDown -= ResizeGrip_MouseDown;
            LeftResizeGrip.MouseMove -= ResizeGrip_MouseMove;
            LeftResizeGrip.MouseUp -= ResizeGrip_MouseUp;
            RightResizeGrip.MouseDown -= ResizeGrip_MouseDown;
            RightResizeGrip.MouseMove -= ResizeGrip_MouseMove;
            RightResizeGrip.MouseUp -= ResizeGrip_MouseUp;
            LeftResizeGrip.MouseDown += ResizeGrip_MouseDown;
            LeftResizeGrip.MouseMove += ResizeGrip_MouseMove;
            LeftResizeGrip.MouseUp += ResizeGrip_MouseUp;
            RightResizeGrip.MouseDown += ResizeGrip_MouseDown;
            RightResizeGrip.MouseMove += ResizeGrip_MouseMove;
            RightResizeGrip.MouseUp += ResizeGrip_MouseUp;
            // Force layout refresh
            InvalidateVisual();
            UpdateLayout();
        }
        catch (Exception ex) { TouchKeyboardMouse.Helpers.AppLogger.LogException(ex, "MainWindow_StateChanged"); }
    }
    // Dynamically update window activation style based on fullscreen/windowed mode
    public void UpdateWindowActivationStyle()
    {
        // No-op: using standard window chrome
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
    // Removed custom window style constants and interop

    private DualScreenHelper? _dualScreenHelper;
    private TrayIconManager? _trayIconManager;
    private bool _isHiddenDueToPosture = false;
    // Removed IsFullscreen property

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
        // Always start in windowed mode with persisted size/position
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
        // Removed IsFullscreen logic
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
        }
        catch { /* Ignore */ }
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        try { ToggleSettings(); } catch { }
    }
    
    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Drag window if not maximized (optional, or remove entirely)
    }
    
    private void NumpadButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _numpadVisible = !_numpadVisible;
            NumpadPanel.Visibility = _numpadVisible ? Visibility.Visible : Visibility.Collapsed;
            NumpadColumn.Width = _numpadVisible ? new GridLength(1, GridUnitType.Auto) : new GridLength(0);
            TouchKeyboardMouse.Helpers.AppLogger.Log($"NumpadButton_Click: NumpadVisible={_numpadVisible}, NumpadColumn.Width={NumpadColumn.Width.Value}");
            // Animate or update layout if needed
            if (_numpadVisible)
            {
                NumpadPanel.Visibility = Visibility.Visible;
                NumpadPanel.UpdateLayout();
                NumpadPanel.InvalidateVisual();
            }
            else
            {
                NumpadPanel.Visibility = Visibility.Collapsed;
            }
            // Save state
            if (_appState != null)
            {
                _appState.NumpadVisible = _numpadVisible;
                AppStateHelper.Save(_appState);
            }
        }
        catch (Exception ex) { TouchKeyboardMouse.Helpers.AppLogger.LogException(ex, "NumpadButton_Click"); }
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
        // Use built-in maximize/restore
        if (this.WindowState == WindowState.Maximized)
        {
            this.WindowState = WindowState.Normal;
        }
        else
        {
            this.WindowState = WindowState.Maximized;
        }
    }

    // Removed ToggleFullscreen method
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
            double delta = 0;
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
            // Calculate new width
            double newWidth = Math.Max(200, _resizeStartWidth + delta);
            var maxWidth = TrackpadArea.ActualWidth - 24; // Account for grip widths
            if (maxWidth > 200) newWidth = Math.Min(newWidth, maxWidth);
            Trackpad.Width = newWidth;
            _trackpadWasFullWidth = false; // User is manually resizing
            // Save trackpad width
            if (_appState != null)
            {
                _appState.TrackpadWidth = newWidth;
                AppStateHelper.Save(_appState);
            }
        }
        catch { /* Ignore resize errors */ }
        // After toggling fullscreen, refresh layout and controls
        try { InvalidateVisual(); UpdateLayout(); } catch { }
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
// Add missing closing brace for namespace
}
