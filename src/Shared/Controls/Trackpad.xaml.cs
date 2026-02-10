using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using TouchKeyboardMouse.Helpers;

namespace TouchKeyboardMouse.Controls;

public partial class Trackpad : UserControl
{
    private Point _lastPosition;
    private bool _isDragging = false;
    private DateTime _lastTapTime = DateTime.MinValue;
    private bool _isLeftButtonDown = false;
    private bool _isRightButtonDown = false;
    
    // Touch tracking for multi-touch gestures
    private Dictionary<int, Point> _activeTouches = new();
    private DateTime _touchStartTime;
    private Point _touchStartPosition;
    private bool _hasMoved = false;
    
    // Sensitivity settings
    public double Sensitivity { get; set; } = 1.5;
    public double ScrollSensitivity { get; set; } = 2.0;
    public int TapThresholdMs { get; set; } = 200;
    public double TapMoveThreshold { get; set; } = 10;
    public int DoubleTapThresholdMs { get; set; } = 300;

    public Trackpad()
    {
        InitializeComponent();
    }

    #region Touch Events

    private void TrackpadSurface_TouchDown(object sender, TouchEventArgs e)
    {
        var position = e.GetTouchPoint(TrackpadSurface).Position;
        _activeTouches[e.TouchDevice.Id] = position;
        
        if (_activeTouches.Count == 1)
        {
            _lastPosition = position;
            _touchStartPosition = position;
            _touchStartTime = DateTime.Now;
            _hasMoved = false;
            _isDragging = true;
            
            ShowTouchIndicator(position);
        }
        
        e.Handled = true;
        TrackpadSurface.CaptureTouch(e.TouchDevice);
    }

    private void TrackpadSurface_TouchMove(object sender, TouchEventArgs e)
    {
        if (!_activeTouches.ContainsKey(e.TouchDevice.Id))
            return;
            
        var currentPosition = e.GetTouchPoint(TrackpadSurface).Position;
        _activeTouches[e.TouchDevice.Id] = currentPosition;
        
        if (_activeTouches.Count == 1)
        {
            // Single finger - move cursor
            var deltaX = (currentPosition.X - _lastPosition.X) * Sensitivity;
            var deltaY = (currentPosition.Y - _lastPosition.Y) * Sensitivity;
            
            if (Math.Abs(deltaX) > 0.5 || Math.Abs(deltaY) > 0.5)
            {
                InputSimulator.MoveMouse((int)deltaX, (int)deltaY);
                _hasMoved = true;
            }
            
            _lastPosition = currentPosition;
            UpdateTouchIndicator(currentPosition);
        }
        else if (_activeTouches.Count == 2)
        {
            // Two finger - scroll
            var deltaY = (currentPosition.Y - _lastPosition.Y) * ScrollSensitivity;
            var deltaX = (currentPosition.X - _lastPosition.X) * ScrollSensitivity;
            
            if (Math.Abs(deltaY) > Math.Abs(deltaX) && Math.Abs(deltaY) > 2)
            {
                InputSimulator.ScrollVertical((int)(-deltaY * 10));
                _hasMoved = true;
            }
            else if (Math.Abs(deltaX) > 2)
            {
                InputSimulator.ScrollHorizontal((int)(deltaX * 10));
                _hasMoved = true;
            }
            
            _lastPosition = currentPosition;
        }
        
        e.Handled = true;
    }

    private void TrackpadSurface_TouchUp(object sender, TouchEventArgs e)
    {
        _activeTouches.Remove(e.TouchDevice.Id);
        
        var elapsed = (DateTime.Now - _touchStartTime).TotalMilliseconds;
        
        // Check for tap gesture
        if (elapsed < TapThresholdMs && !_hasMoved)
        {
            var timeSinceLastTap = (DateTime.Now - _lastTapTime).TotalMilliseconds;
            
            if (timeSinceLastTap < DoubleTapThresholdMs)
            {
                // Double tap - double click
                InputSimulator.LeftClick();
                InputSimulator.LeftClick();
            }
            else
            {
                // Single tap - left click
                InputSimulator.LeftClick();
            }
            
            _lastTapTime = DateTime.Now;
        }
        
        if (_activeTouches.Count == 0)
        {
            _isDragging = false;
            HideTouchIndicator();
        }
        
        TrackpadSurface.ReleaseTouchCapture(e.TouchDevice);
        e.Handled = true;
    }

    #endregion

    #region Mouse Events (for testing without touch)

    private void TrackpadSurface_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.StylusDevice != null) return; // Ignore if this is actually a touch
        
        _lastPosition = e.GetPosition(TrackpadSurface);
        _touchStartPosition = _lastPosition;
        _touchStartTime = DateTime.Now;
        _hasMoved = false;
        _isDragging = true;
        
        ShowTouchIndicator(_lastPosition);
        TrackpadSurface.CaptureMouse();
        e.Handled = true;
    }

    private void TrackpadSurface_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.StylusDevice != null) return;
        
        if (_isDragging)
        {
            var currentPosition = e.GetPosition(TrackpadSurface);
            var deltaX = (currentPosition.X - _lastPosition.X) * Sensitivity;
            var deltaY = (currentPosition.Y - _lastPosition.Y) * Sensitivity;
            
            if (Math.Abs(deltaX) > 0.5 || Math.Abs(deltaY) > 0.5)
            {
                InputSimulator.MoveMouse((int)deltaX, (int)deltaY);
                _hasMoved = true;
            }
            
            _lastPosition = currentPosition;
            UpdateTouchIndicator(currentPosition);
        }
    }

    private void TrackpadSurface_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (e.StylusDevice != null) return;
        
        var elapsed = (DateTime.Now - _touchStartTime).TotalMilliseconds;
        
        if (elapsed < TapThresholdMs && !_hasMoved)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                InputSimulator.LeftClick();
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                InputSimulator.RightClick();
            }
        }
        
        _isDragging = false;
        HideTouchIndicator();
        TrackpadSurface.ReleaseMouseCapture();
        e.Handled = true;
    }

    #endregion

    #region Manipulation Events (for pinch/zoom if needed)

    private void TrackpadSurface_ManipulationStarting(object sender, ManipulationStartingEventArgs e)
    {
        e.ManipulationContainer = TrackpadSurface;
        e.Mode = ManipulationModes.All;
    }

    private void TrackpadSurface_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
    {
        // Can be used for additional gestures like pinch-to-zoom
        // Currently using direct touch tracking instead
    }

    #endregion

    #region Click Buttons

    private void LeftClickButton_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.StylusDevice != null) return;
        _isLeftButtonDown = true;
        InputSimulator.LeftMouseDown();
        e.Handled = true;
    }

    private void LeftClickButton_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (e.StylusDevice != null) return;
        if (_isLeftButtonDown)
        {
            InputSimulator.LeftMouseUp();
            _isLeftButtonDown = false;
        }
        e.Handled = true;
    }

    private void LeftClickButton_TouchDown(object sender, TouchEventArgs e)
    {
        _isLeftButtonDown = true;
        InputSimulator.LeftMouseDown();
        e.Handled = true;
    }

    private void LeftClickButton_TouchUp(object sender, TouchEventArgs e)
    {
        if (_isLeftButtonDown)
        {
            InputSimulator.LeftMouseUp();
            _isLeftButtonDown = false;
        }
        e.Handled = true;
    }

    private void RightClickButton_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.StylusDevice != null) return;
        _isRightButtonDown = true;
        InputSimulator.RightMouseDown();
        e.Handled = true;
    }

    private void RightClickButton_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (e.StylusDevice != null) return;
        if (_isRightButtonDown)
        {
            InputSimulator.RightMouseUp();
            _isRightButtonDown = false;
        }
        e.Handled = true;
    }

    private void RightClickButton_TouchDown(object sender, TouchEventArgs e)
    {
        _isRightButtonDown = true;
        InputSimulator.RightMouseDown();
        e.Handled = true;
    }

    private void RightClickButton_TouchUp(object sender, TouchEventArgs e)
    {
        if (_isRightButtonDown)
        {
            InputSimulator.RightMouseUp();
            _isRightButtonDown = false;
        }
        e.Handled = true;
    }

    #endregion

    #region Visual Feedback

    private void ShowTouchIndicator(Point position)
    {
        TouchIndicator.Visibility = Visibility.Visible;
        Canvas.SetLeft(TouchIndicator, position.X - 20);
        Canvas.SetTop(TouchIndicator, position.Y - 20);
    }

    private void UpdateTouchIndicator(Point position)
    {
        Canvas.SetLeft(TouchIndicator, position.X - 20);
        Canvas.SetTop(TouchIndicator, position.Y - 20);
    }

    private void HideTouchIndicator()
    {
        TouchIndicator.Visibility = Visibility.Collapsed;
    }

    #endregion
}
