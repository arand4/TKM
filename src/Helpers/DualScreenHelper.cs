using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.Win32;

namespace TouchKeyboardMouse.Helpers;

/// <summary>
/// Detects and monitors dual-screen device posture and screen arrangements
/// </summary>
public class DualScreenHelper : IDisposable
{
    public event EventHandler<PostureChangedEventArgs>? PostureChanged;

    private System.Windows.Threading.DispatcherTimer? _monitorTimer;
    private DevicePosture _currentPosture = DevicePosture.Unknown;
    private ScreenArrangement _currentArrangement = ScreenArrangement.Unknown;

    #region Native Imports

    [DllImport("user32.dll")]
    private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);

    private delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct MONITORINFOEX
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string szDevice;
    }

    private const uint MONITORINFOF_PRIMARY = 1;

    #endregion

    public DualScreenHelper()
    {
        // Start monitoring for display changes
        StartMonitoring();
        
        // Initial detection
        DetectPosture();
    }

    /// <summary>
    /// Gets all connected screens with their bounds
    /// </summary>
    public List<ScreenInfo> GetScreens()
    {
        var screens = new List<ScreenInfo>();

        EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData) =>
        {
            var info = new MONITORINFOEX();
            info.cbSize = Marshal.SizeOf<MONITORINFOEX>();

            if (GetMonitorInfo(hMonitor, ref info))
            {
                screens.Add(new ScreenInfo
                {
                    Handle = hMonitor,
                    Bounds = new Rect(
                        info.rcMonitor.Left,
                        info.rcMonitor.Top,
                        info.rcMonitor.Right - info.rcMonitor.Left,
                        info.rcMonitor.Bottom - info.rcMonitor.Top),
                    WorkArea = new Rect(
                        info.rcWork.Left,
                        info.rcWork.Top,
                        info.rcWork.Right - info.rcWork.Left,
                        info.rcWork.Bottom - info.rcWork.Top),
                    IsPrimary = (info.dwFlags & MONITORINFOF_PRIMARY) != 0,
                    DeviceName = info.szDevice
                });
            }
            return true;
        }, IntPtr.Zero);

        return screens.OrderBy(s => s.Bounds.Left).ThenBy(s => s.Bounds.Top).ToList();
    }

    /// <summary>
    /// Detects the current device posture and screen arrangement
    /// </summary>
    public void DetectPosture()
    {
        var screens = GetScreens();
        var oldPosture = _currentPosture;
        var oldArrangement = _currentArrangement;

        if (screens.Count < 2)
        {
            _currentPosture = DevicePosture.SingleScreen;
            _currentArrangement = ScreenArrangement.Single;
        }
        else
        {
            // Analyze screen positions to determine arrangement
            _currentArrangement = DetermineArrangement(screens);
            _currentPosture = DeterminePosture(_currentArrangement, screens);
        }

        // Notify if changed
        if (oldPosture != _currentPosture || oldArrangement != _currentArrangement)
        {
            PostureChanged?.Invoke(this, new PostureChangedEventArgs
            {
                Posture = _currentPosture,
                Arrangement = _currentArrangement,
                Screens = screens
            });
        }
    }

    private ScreenArrangement DetermineArrangement(List<ScreenInfo> screens)
    {
        if (screens.Count != 2)
            return ScreenArrangement.Unknown;

        var screen1 = screens[0];
        var screen2 = screens[1];

        // Check if screens are vertically stacked (one above the other)
        bool verticallyAligned = Math.Abs(screen1.Bounds.Left - screen2.Bounds.Left) < 50;
        bool horizontallyAligned = Math.Abs(screen1.Bounds.Top - screen2.Bounds.Top) < 50;

        if (verticallyAligned)
        {
            // Screens are stacked vertically
            if (screen1.Bounds.Top < screen2.Bounds.Top)
                return ScreenArrangement.VerticalTopBottom; // Screen 1 on top
            else
                return ScreenArrangement.VerticalBottomTop; // Screen 2 on top
        }
        else if (horizontallyAligned)
        {
            // Screens are side by side
            if (screen1.Bounds.Left < screen2.Bounds.Left)
                return ScreenArrangement.HorizontalLeftRight;
            else
                return ScreenArrangement.HorizontalRightLeft;
        }
        else
        {
            // Diagonal or unusual arrangement
            return ScreenArrangement.Other;
        }
    }

    private DevicePosture DeterminePosture(ScreenArrangement arrangement, List<ScreenInfo> screens)
    {
        return arrangement switch
        {
            ScreenArrangement.VerticalTopBottom or ScreenArrangement.VerticalBottomTop => DevicePosture.BookMode,
            ScreenArrangement.HorizontalLeftRight or ScreenArrangement.HorizontalRightLeft => DevicePosture.LaptopMode,
            ScreenArrangement.Single => DevicePosture.SingleScreen,
            _ => DevicePosture.Unknown
        };
    }

    /// <summary>
    /// Gets the screen that should be used for the keyboard (bottom screen in vertical mode)
    /// </summary>
    public ScreenInfo? GetKeyboardScreen()
    {
        var screens = GetScreens();
        
        if (screens.Count < 2)
            return null;

        return _currentArrangement switch
        {
            // In vertical arrangements, use the bottom screen
            ScreenArrangement.VerticalTopBottom => screens.OrderByDescending(s => s.Bounds.Top).First(),
            ScreenArrangement.VerticalBottomTop => screens.OrderByDescending(s => s.Bounds.Top).First(),
            // In horizontal arrangements, return null (keyboard should be disabled)
            ScreenArrangement.HorizontalLeftRight => null,
            ScreenArrangement.HorizontalRightLeft => null,
            _ => null
        };
    }

    /// <summary>
    /// Checks if the keyboard should be visible based on current posture
    /// </summary>
    public bool ShouldShowKeyboard()
    {
        return _currentPosture == DevicePosture.BookMode;
    }

    public DevicePosture CurrentPosture => _currentPosture;
    public ScreenArrangement CurrentArrangement => _currentArrangement;

    private void StartMonitoring()
    {
        // Monitor for display settings changes
        SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;

        // Also use a timer to periodically check (for device rotation that might not trigger events)
        _monitorTimer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };
        _monitorTimer.Tick += (s, e) => DetectPosture();
        _monitorTimer.Start();
    }

    private void OnDisplaySettingsChanged(object? sender, EventArgs e)
    {
        // Small delay to let the system settle
        System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(
            new Action(DetectPosture),
            System.Windows.Threading.DispatcherPriority.Background);
    }

    public void Dispose()
    {
        _monitorTimer?.Stop();
        SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;
    }
}

public enum DevicePosture
{
    Unknown,
    SingleScreen,
    BookMode,      // Screens stacked vertically (like a book/laptop) - keyboard visible
    LaptopMode     // Screens side by side - keyboard hidden
}

public enum ScreenArrangement
{
    Unknown,
    Single,
    VerticalTopBottom,    // Primary on top, secondary on bottom
    VerticalBottomTop,    // Secondary on top, primary on bottom
    HorizontalLeftRight,  // Primary on left, secondary on right
    HorizontalRightLeft,  // Secondary on left, primary on right
    Other
}

public class ScreenInfo
{
    public IntPtr Handle { get; set; }
    public Rect Bounds { get; set; }
    public Rect WorkArea { get; set; }
    public bool IsPrimary { get; set; }
    public string DeviceName { get; set; } = string.Empty;
}

public class PostureChangedEventArgs : EventArgs
{
    public DevicePosture Posture { get; set; }
    public ScreenArrangement Arrangement { get; set; }
    public List<ScreenInfo> Screens { get; set; } = new();
}
