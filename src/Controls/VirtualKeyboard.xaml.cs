using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TouchKeyboardMouse.Helpers;

namespace TouchKeyboardMouse.Controls;

public partial class VirtualKeyboard : UserControl
{
    private bool _shiftActive = false;
    private bool _ctrlActive = false;
    private bool _altActive = false;
    private bool _capsLockActive = false;

    public VirtualKeyboard()
    {
        InitializeComponent();
    }

    private void Key_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string keyTag)
        {
            ushort keyCode = GetKeyCode(keyTag);
            if (keyCode != 0)
            {
                // Apply modifiers
                if (_ctrlActive) InputSimulator.SendKeyDown(VirtualKeyCodes.VK_CONTROL);
                if (_altActive) InputSimulator.SendKeyDown(VirtualKeyCodes.VK_MENU);
                if (_shiftActive) InputSimulator.SendKeyDown(VirtualKeyCodes.VK_SHIFT);

                // Send the key
                InputSimulator.SendKey(keyCode);

                // Release modifiers
                if (_shiftActive) InputSimulator.SendKeyUp(VirtualKeyCodes.VK_SHIFT);
                if (_altActive) InputSimulator.SendKeyUp(VirtualKeyCodes.VK_MENU);
                if (_ctrlActive) InputSimulator.SendKeyUp(VirtualKeyCodes.VK_CONTROL);

                // Reset non-locked modifiers after key press
                if (_shiftActive && !_capsLockActive)
                {
                    _shiftActive = false;
                    UpdateModifierButtonStates();
                }
                if (_ctrlActive)
                {
                    _ctrlActive = false;
                    UpdateModifierButtonStates();
                }
                if (_altActive)
                {
                    _altActive = false;
                    UpdateModifierButtonStates();
                }
            }
        }
    }

    private void Shift_Click(object sender, RoutedEventArgs e)
    {
        _shiftActive = !_shiftActive;
        UpdateModifierButtonStates();
    }

    private void Ctrl_Click(object sender, RoutedEventArgs e)
    {
        _ctrlActive = !_ctrlActive;
        UpdateModifierButtonStates();
    }

    private void Alt_Click(object sender, RoutedEventArgs e)
    {
        _altActive = !_altActive;
        UpdateModifierButtonStates();
    }

    private void CapsLock_Click(object sender, RoutedEventArgs e)
    {
        _capsLockActive = !_capsLockActive;
        InputSimulator.SendKey(VirtualKeyCodes.VK_CAPITAL);
        UpdateModifierButtonStates();
    }

    private void UpDown_Click(object sender, RoutedEventArgs e)
    {
        // Toggle between up and down arrow functionality
        // For simplicity, we'll use a popup or alternate between them
        // Here we send UP; user can tap again for DOWN
        if (_shiftActive)
        {
            InputSimulator.SendKey(VirtualKeyCodes.VK_DOWN);
        }
        else
        {
            InputSimulator.SendKey(VirtualKeyCodes.VK_UP);
        }
    }

    private void UpdateModifierButtonStates()
    {
        var activeColor = new SolidColorBrush(Color.FromRgb(0x00, 0x7A, 0xCC));
        var inactiveColor = new SolidColorBrush(Color.FromRgb(0x3a, 0x3a, 0x3a));

        LeftShiftKey.Background = _shiftActive ? activeColor : inactiveColor;
        RightShiftKey.Background = _shiftActive ? activeColor : inactiveColor;
        CtrlKey.Background = _ctrlActive ? activeColor : inactiveColor;
        AltKey.Background = _altActive ? activeColor : inactiveColor;
        CapsLockKey.Background = _capsLockActive ? activeColor : inactiveColor;
    }

    private ushort GetKeyCode(string keyTag)
    {
        return keyTag switch
        {
            // Letters
            "A" => VirtualKeyCodes.VK_A,
            "B" => VirtualKeyCodes.VK_B,
            "C" => VirtualKeyCodes.VK_C,
            "D" => VirtualKeyCodes.VK_D,
            "E" => VirtualKeyCodes.VK_E,
            "F" => VirtualKeyCodes.VK_F,
            "G" => VirtualKeyCodes.VK_G,
            "H" => VirtualKeyCodes.VK_H,
            "I" => VirtualKeyCodes.VK_I,
            "J" => VirtualKeyCodes.VK_J,
            "K" => VirtualKeyCodes.VK_K,
            "L" => VirtualKeyCodes.VK_L,
            "M" => VirtualKeyCodes.VK_M,
            "N" => VirtualKeyCodes.VK_N,
            "O" => VirtualKeyCodes.VK_O,
            "P" => VirtualKeyCodes.VK_P,
            "Q" => VirtualKeyCodes.VK_Q,
            "R" => VirtualKeyCodes.VK_R,
            "S" => VirtualKeyCodes.VK_S,
            "T" => VirtualKeyCodes.VK_T,
            "U" => VirtualKeyCodes.VK_U,
            "V" => VirtualKeyCodes.VK_V,
            "W" => VirtualKeyCodes.VK_W,
            "X" => VirtualKeyCodes.VK_X,
            "Y" => VirtualKeyCodes.VK_Y,
            "Z" => VirtualKeyCodes.VK_Z,
            
            // Numbers
            "0" => VirtualKeyCodes.VK_0,
            "1" => VirtualKeyCodes.VK_1,
            "2" => VirtualKeyCodes.VK_2,
            "3" => VirtualKeyCodes.VK_3,
            "4" => VirtualKeyCodes.VK_4,
            "5" => VirtualKeyCodes.VK_5,
            "6" => VirtualKeyCodes.VK_6,
            "7" => VirtualKeyCodes.VK_7,
            "8" => VirtualKeyCodes.VK_8,
            "9" => VirtualKeyCodes.VK_9,
            
            // Function keys
            "F1" => VirtualKeyCodes.VK_F1,
            "F2" => VirtualKeyCodes.VK_F2,
            "F3" => VirtualKeyCodes.VK_F3,
            "F4" => VirtualKeyCodes.VK_F4,
            "F5" => VirtualKeyCodes.VK_F5,
            "F6" => VirtualKeyCodes.VK_F6,
            "F7" => VirtualKeyCodes.VK_F7,
            "F8" => VirtualKeyCodes.VK_F8,
            "F9" => VirtualKeyCodes.VK_F9,
            "F10" => VirtualKeyCodes.VK_F10,
            "F11" => VirtualKeyCodes.VK_F11,
            "F12" => VirtualKeyCodes.VK_F12,
            
            // Special keys
            "ESCAPE" => VirtualKeyCodes.VK_ESCAPE,
            "TAB" => VirtualKeyCodes.VK_TAB,
            "CAPITAL" => VirtualKeyCodes.VK_CAPITAL,
            "LSHIFT" => VirtualKeyCodes.VK_LSHIFT,
            "RSHIFT" => VirtualKeyCodes.VK_RSHIFT,
            "LCONTROL" => VirtualKeyCodes.VK_LCONTROL,
            "RCONTROL" => VirtualKeyCodes.VK_RCONTROL,
            "LMENU" => VirtualKeyCodes.VK_LMENU,
            "RMENU" => VirtualKeyCodes.VK_RMENU,
            "LWIN" => VirtualKeyCodes.VK_LWIN,
            "SPACE" => VirtualKeyCodes.VK_SPACE,
            "RETURN" => VirtualKeyCodes.VK_RETURN,
            "BACK" => VirtualKeyCodes.VK_BACK,
            "DELETE" => VirtualKeyCodes.VK_DELETE,
            "INSERT" => VirtualKeyCodes.VK_INSERT,
            "HOME" => VirtualKeyCodes.VK_HOME,
            "END" => VirtualKeyCodes.VK_END,
            "PRIOR" => VirtualKeyCodes.VK_PRIOR,
            "NEXT" => VirtualKeyCodes.VK_NEXT,
            "LEFT" => VirtualKeyCodes.VK_LEFT,
            "RIGHT" => VirtualKeyCodes.VK_RIGHT,
            "UP" => VirtualKeyCodes.VK_UP,
            "DOWN" => VirtualKeyCodes.VK_DOWN,
            
            // OEM keys
            "OEM_1" => VirtualKeyCodes.VK_OEM_1,
            "OEM_2" => VirtualKeyCodes.VK_OEM_2,
            "OEM_3" => VirtualKeyCodes.VK_OEM_3,
            "OEM_4" => VirtualKeyCodes.VK_OEM_4,
            "OEM_5" => VirtualKeyCodes.VK_OEM_5,
            "OEM_6" => VirtualKeyCodes.VK_OEM_6,
            "OEM_7" => VirtualKeyCodes.VK_OEM_7,
            "OEM_PLUS" => VirtualKeyCodes.VK_OEM_PLUS,
            "OEM_MINUS" => VirtualKeyCodes.VK_OEM_MINUS,
            "OEM_COMMA" => VirtualKeyCodes.VK_OEM_COMMA,
            "OEM_PERIOD" => VirtualKeyCodes.VK_OEM_PERIOD,
            
            _ => 0
        };
    }
}
