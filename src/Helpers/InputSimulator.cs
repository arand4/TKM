using System;
using System.Runtime.InteropServices;

namespace TouchKeyboardMouse.Helpers;

/// <summary>
/// Provides methods to simulate keyboard and mouse input using Windows SendInput API
/// </summary>
public static class InputSimulator
{
    #region Native Structures

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint Type;
        public INPUTUNION Data;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct INPUTUNION
    {
        [FieldOffset(0)]
        public MOUSEINPUT Mouse;
        [FieldOffset(0)]
        public KEYBDINPUT Keyboard;
        [FieldOffset(0)]
        public HARDWAREINPUT Hardware;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct HARDWAREINPUT
    {
        public uint uMsg;
        public ushort wParamL;
        public ushort wParamH;
    }

    #endregion

    #region Native Constants

    private const uint INPUT_MOUSE = 0;
    private const uint INPUT_KEYBOARD = 1;

    // Mouse event flags
    private const uint MOUSEEVENTF_MOVE = 0x0001;
    private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    private const uint MOUSEEVENTF_LEFTUP = 0x0004;
    private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
    private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
    private const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
    private const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
    private const uint MOUSEEVENTF_WHEEL = 0x0800;
    private const uint MOUSEEVENTF_HWHEEL = 0x1000;
    private const uint MOUSEEVENTF_ABSOLUTE = 0x8000;

    // Keyboard event flags
    private const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
    private const uint KEYEVENTF_KEYUP = 0x0002;
    private const uint KEYEVENTF_UNICODE = 0x0004;
    private const uint KEYEVENTF_SCANCODE = 0x0008;

    #endregion

    #region Native Imports

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    private const int SM_CXSCREEN = 0;
    private const int SM_CYSCREEN = 1;

    #endregion

    #region Keyboard Methods

    /// <summary>
    /// Simulates a key press (down and up)
    /// </summary>
    public static void SendKey(ushort virtualKeyCode)
    {
        SendKeyDown(virtualKeyCode);
        SendKeyUp(virtualKeyCode);
    }

    /// <summary>
    /// Simulates a key down event
    /// </summary>
    public static void SendKeyDown(ushort virtualKeyCode)
    {
        var input = new INPUT
        {
            Type = INPUT_KEYBOARD,
            Data = new INPUTUNION
            {
                Keyboard = new KEYBDINPUT
                {
                    wVk = virtualKeyCode,
                    wScan = 0,
                    dwFlags = IsExtendedKey(virtualKeyCode) ? KEYEVENTF_EXTENDEDKEY : 0,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };

        SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>());
    }

    /// <summary>
    /// Simulates a key up event
    /// </summary>
    public static void SendKeyUp(ushort virtualKeyCode)
    {
        var input = new INPUT
        {
            Type = INPUT_KEYBOARD,
            Data = new INPUTUNION
            {
                Keyboard = new KEYBDINPUT
                {
                    wVk = virtualKeyCode,
                    wScan = 0,
                    dwFlags = KEYEVENTF_KEYUP | (IsExtendedKey(virtualKeyCode) ? KEYEVENTF_EXTENDEDKEY : 0),
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };

        SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>());
    }

    /// <summary>
    /// Types a unicode character
    /// </summary>
    public static void SendUnicode(char character)
    {
        var inputs = new INPUT[2];

        inputs[0] = new INPUT
        {
            Type = INPUT_KEYBOARD,
            Data = new INPUTUNION
            {
                Keyboard = new KEYBDINPUT
                {
                    wVk = 0,
                    wScan = (ushort)character,
                    dwFlags = KEYEVENTF_UNICODE,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };

        inputs[1] = new INPUT
        {
            Type = INPUT_KEYBOARD,
            Data = new INPUTUNION
            {
                Keyboard = new KEYBDINPUT
                {
                    wVk = 0,
                    wScan = (ushort)character,
                    dwFlags = KEYEVENTF_UNICODE | KEYEVENTF_KEYUP,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };

        SendInput(2, inputs, Marshal.SizeOf<INPUT>());
    }

    private static bool IsExtendedKey(ushort keyCode)
    {
        // Extended keys include arrow keys, Insert, Delete, Home, End, Page Up, Page Down,
        // Num Lock, Break, Print Screen, Divide, and Enter on numpad
        return keyCode is 0x21 or 0x22 or 0x23 or 0x24 or 0x25 or 0x26 or 0x27 or 0x28
            or 0x2D or 0x2E or 0x90 or 0x6F;
    }

    #endregion

    #region Mouse Methods

    /// <summary>
    /// Moves the mouse cursor by the specified delta
    /// </summary>
    public static void MoveMouse(int deltaX, int deltaY)
    {
        var input = new INPUT
        {
            Type = INPUT_MOUSE,
            Data = new INPUTUNION
            {
                Mouse = new MOUSEINPUT
                {
                    dx = deltaX,
                    dy = deltaY,
                    mouseData = 0,
                    dwFlags = MOUSEEVENTF_MOVE,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };

        SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>());
    }

    /// <summary>
    /// Clicks the left mouse button
    /// </summary>
    public static void LeftClick()
    {
        LeftMouseDown();
        LeftMouseUp();
    }

    /// <summary>
    /// Presses the left mouse button down
    /// </summary>
    public static void LeftMouseDown()
    {
        var input = new INPUT
        {
            Type = INPUT_MOUSE,
            Data = new INPUTUNION
            {
                Mouse = new MOUSEINPUT
                {
                    dx = 0,
                    dy = 0,
                    mouseData = 0,
                    dwFlags = MOUSEEVENTF_LEFTDOWN,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };

        SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>());
    }

    /// <summary>
    /// Releases the left mouse button
    /// </summary>
    public static void LeftMouseUp()
    {
        var input = new INPUT
        {
            Type = INPUT_MOUSE,
            Data = new INPUTUNION
            {
                Mouse = new MOUSEINPUT
                {
                    dx = 0,
                    dy = 0,
                    mouseData = 0,
                    dwFlags = MOUSEEVENTF_LEFTUP,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };

        SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>());
    }

    /// <summary>
    /// Clicks the right mouse button
    /// </summary>
    public static void RightClick()
    {
        RightMouseDown();
        RightMouseUp();
    }

    /// <summary>
    /// Presses the right mouse button down
    /// </summary>
    public static void RightMouseDown()
    {
        var input = new INPUT
        {
            Type = INPUT_MOUSE,
            Data = new INPUTUNION
            {
                Mouse = new MOUSEINPUT
                {
                    dx = 0,
                    dy = 0,
                    mouseData = 0,
                    dwFlags = MOUSEEVENTF_RIGHTDOWN,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };

        SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>());
    }

    /// <summary>
    /// Releases the right mouse button
    /// </summary>
    public static void RightMouseUp()
    {
        var input = new INPUT
        {
            Type = INPUT_MOUSE,
            Data = new INPUTUNION
            {
                Mouse = new MOUSEINPUT
                {
                    dx = 0,
                    dy = 0,
                    mouseData = 0,
                    dwFlags = MOUSEEVENTF_RIGHTUP,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };

        SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>());
    }

    /// <summary>
    /// Scrolls the mouse wheel vertically
    /// </summary>
    /// <param name="delta">Positive for scroll up, negative for scroll down</param>
    public static void ScrollVertical(int delta)
    {
        var input = new INPUT
        {
            Type = INPUT_MOUSE,
            Data = new INPUTUNION
            {
                Mouse = new MOUSEINPUT
                {
                    dx = 0,
                    dy = 0,
                    mouseData = (uint)delta,
                    dwFlags = MOUSEEVENTF_WHEEL,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };

        SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>());
    }

    /// <summary>
    /// Scrolls the mouse wheel horizontally
    /// </summary>
    /// <param name="delta">Positive for scroll right, negative for scroll left</param>
    public static void ScrollHorizontal(int delta)
    {
        var input = new INPUT
        {
            Type = INPUT_MOUSE,
            Data = new INPUTUNION
            {
                Mouse = new MOUSEINPUT
                {
                    dx = 0,
                    dy = 0,
                    mouseData = (uint)delta,
                    dwFlags = MOUSEEVENTF_HWHEEL,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };

        SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>());
    }

    #endregion
}
