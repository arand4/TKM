namespace TouchKeyboardMouse.Helpers;

/// <summary>
/// Windows Virtual Key Codes
/// </summary>
public static class VirtualKeyCodes
{
    // Letters
    public const ushort VK_A = 0x41;
    public const ushort VK_B = 0x42;
    public const ushort VK_C = 0x43;
    public const ushort VK_D = 0x44;
    public const ushort VK_E = 0x45;
    public const ushort VK_F = 0x46;
    public const ushort VK_G = 0x47;
    public const ushort VK_H = 0x48;
    public const ushort VK_I = 0x49;
    public const ushort VK_J = 0x4A;
    public const ushort VK_K = 0x4B;
    public const ushort VK_L = 0x4C;
    public const ushort VK_M = 0x4D;
    public const ushort VK_N = 0x4E;
    public const ushort VK_O = 0x4F;
    public const ushort VK_P = 0x50;
    public const ushort VK_Q = 0x51;
    public const ushort VK_R = 0x52;
    public const ushort VK_S = 0x53;
    public const ushort VK_T = 0x54;
    public const ushort VK_U = 0x55;
    public const ushort VK_V = 0x56;
    public const ushort VK_W = 0x57;
    public const ushort VK_X = 0x58;
    public const ushort VK_Y = 0x59;
    public const ushort VK_Z = 0x5A;

    // Numbers
    public const ushort VK_0 = 0x30;
    public const ushort VK_1 = 0x31;
    public const ushort VK_2 = 0x32;
    public const ushort VK_3 = 0x33;
    public const ushort VK_4 = 0x34;
    public const ushort VK_5 = 0x35;
    public const ushort VK_6 = 0x36;
    public const ushort VK_7 = 0x37;
    public const ushort VK_8 = 0x38;
    public const ushort VK_9 = 0x39;

    // Function Keys
    public const ushort VK_F1 = 0x70;
    public const ushort VK_F2 = 0x71;
    public const ushort VK_F3 = 0x72;
    public const ushort VK_F4 = 0x73;
    public const ushort VK_F5 = 0x74;
    public const ushort VK_F6 = 0x75;
    public const ushort VK_F7 = 0x76;
    public const ushort VK_F8 = 0x77;
    public const ushort VK_F9 = 0x78;
    public const ushort VK_F10 = 0x79;
    public const ushort VK_F11 = 0x7A;
    public const ushort VK_F12 = 0x7B;

    // Modifier Keys
    public const ushort VK_SHIFT = 0x10;
    public const ushort VK_LSHIFT = 0xA0;
    public const ushort VK_RSHIFT = 0xA1;
    public const ushort VK_CONTROL = 0x11;
    public const ushort VK_LCONTROL = 0xA2;
    public const ushort VK_RCONTROL = 0xA3;
    public const ushort VK_MENU = 0x12; // Alt key
    public const ushort VK_LMENU = 0xA4;
    public const ushort VK_RMENU = 0xA5;
    public const ushort VK_LWIN = 0x5B;
    public const ushort VK_RWIN = 0x5C;

    // Special Keys
    public const ushort VK_BACK = 0x08;      // Backspace
    public const ushort VK_TAB = 0x09;
    public const ushort VK_RETURN = 0x0D;    // Enter
    public const ushort VK_ESCAPE = 0x1B;
    public const ushort VK_SPACE = 0x20;
    public const ushort VK_PRIOR = 0x21;     // Page Up
    public const ushort VK_NEXT = 0x22;      // Page Down
    public const ushort VK_END = 0x23;
    public const ushort VK_HOME = 0x24;
    public const ushort VK_LEFT = 0x25;
    public const ushort VK_UP = 0x26;
    public const ushort VK_RIGHT = 0x27;
    public const ushort VK_DOWN = 0x28;
    public const ushort VK_INSERT = 0x2D;
    public const ushort VK_DELETE = 0x2E;
    public const ushort VK_CAPITAL = 0x14;   // Caps Lock
    public const ushort VK_NUMLOCK = 0x90;
    public const ushort VK_SCROLL = 0x91;    // Scroll Lock
    public const ushort VK_SNAPSHOT = 0x2C;  // Print Screen
    public const ushort VK_PAUSE = 0x13;

    // OEM Keys (punctuation, etc.)
    public const ushort VK_OEM_1 = 0xBA;     // ;:
    public const ushort VK_OEM_PLUS = 0xBB;  // =+
    public const ushort VK_OEM_COMMA = 0xBC; // ,<
    public const ushort VK_OEM_MINUS = 0xBD; // -_
    public const ushort VK_OEM_PERIOD = 0xBE;// .>
    public const ushort VK_OEM_2 = 0xBF;     // /?
    public const ushort VK_OEM_3 = 0xC0;     // `~
    public const ushort VK_OEM_4 = 0xDB;     // [{
    public const ushort VK_OEM_5 = 0xDC;     // \|
    public const ushort VK_OEM_6 = 0xDD;     // ]}
    public const ushort VK_OEM_7 = 0xDE;     // '"

    // Numpad
    public const ushort VK_NUMPAD0 = 0x60;
    public const ushort VK_NUMPAD1 = 0x61;
    public const ushort VK_NUMPAD2 = 0x62;
    public const ushort VK_NUMPAD3 = 0x63;
    public const ushort VK_NUMPAD4 = 0x64;
    public const ushort VK_NUMPAD5 = 0x65;
    public const ushort VK_NUMPAD6 = 0x66;
    public const ushort VK_NUMPAD7 = 0x67;
    public const ushort VK_NUMPAD8 = 0x68;
    public const ushort VK_NUMPAD9 = 0x69;
    public const ushort VK_MULTIPLY = 0x6A;
    public const ushort VK_ADD = 0x6B;
    public const ushort VK_SUBTRACT = 0x6D;
    public const ushort VK_DECIMAL = 0x6E;
    public const ushort VK_DIVIDE = 0x6F;
    
    /// <summary>
    /// Converts a key tag string to its virtual key code
    /// </summary>
    public static ushort GetKeyCode(string keyTag)
    {
        return keyTag switch
        {
            // Letters
            "A" => VK_A, "B" => VK_B, "C" => VK_C, "D" => VK_D, "E" => VK_E,
            "F" => VK_F, "G" => VK_G, "H" => VK_H, "I" => VK_I, "J" => VK_J,
            "K" => VK_K, "L" => VK_L, "M" => VK_M, "N" => VK_N, "O" => VK_O,
            "P" => VK_P, "Q" => VK_Q, "R" => VK_R, "S" => VK_S, "T" => VK_T,
            "U" => VK_U, "V" => VK_V, "W" => VK_W, "X" => VK_X, "Y" => VK_Y,
            "Z" => VK_Z,
            
            // Numbers
            "0" => VK_0, "1" => VK_1, "2" => VK_2, "3" => VK_3, "4" => VK_4,
            "5" => VK_5, "6" => VK_6, "7" => VK_7, "8" => VK_8, "9" => VK_9,
            
            // Function keys
            "F1" => VK_F1, "F2" => VK_F2, "F3" => VK_F3, "F4" => VK_F4,
            "F5" => VK_F5, "F6" => VK_F6, "F7" => VK_F7, "F8" => VK_F8,
            "F9" => VK_F9, "F10" => VK_F10, "F11" => VK_F11, "F12" => VK_F12,
            
            // Special keys
            "ESCAPE" => VK_ESCAPE, "TAB" => VK_TAB, "CAPITAL" => VK_CAPITAL,
            "LSHIFT" => VK_LSHIFT, "RSHIFT" => VK_RSHIFT,
            "LCONTROL" => VK_LCONTROL, "RCONTROL" => VK_RCONTROL,
            "LMENU" => VK_LMENU, "RMENU" => VK_RMENU, "LWIN" => VK_LWIN,
            "SPACE" => VK_SPACE, "RETURN" => VK_RETURN, "BACK" => VK_BACK,
            "DELETE" => VK_DELETE, "INSERT" => VK_INSERT,
            "HOME" => VK_HOME, "END" => VK_END,
            "PRIOR" => VK_PRIOR, "NEXT" => VK_NEXT,
            "LEFT" => VK_LEFT, "RIGHT" => VK_RIGHT, "UP" => VK_UP, "DOWN" => VK_DOWN,
            "NUMLOCK" => VK_NUMLOCK,
            
            // OEM keys
            "OEM_1" => VK_OEM_1, "OEM_2" => VK_OEM_2, "OEM_3" => VK_OEM_3,
            "OEM_4" => VK_OEM_4, "OEM_5" => VK_OEM_5, "OEM_6" => VK_OEM_6,
            "OEM_7" => VK_OEM_7, "OEM_PLUS" => VK_OEM_PLUS, "OEM_MINUS" => VK_OEM_MINUS,
            "OEM_COMMA" => VK_OEM_COMMA, "OEM_PERIOD" => VK_OEM_PERIOD,
            
            // Numpad
            "NUMPAD0" => VK_NUMPAD0, "NUMPAD1" => VK_NUMPAD1, "NUMPAD2" => VK_NUMPAD2,
            "NUMPAD3" => VK_NUMPAD3, "NUMPAD4" => VK_NUMPAD4, "NUMPAD5" => VK_NUMPAD5,
            "NUMPAD6" => VK_NUMPAD6, "NUMPAD7" => VK_NUMPAD7, "NUMPAD8" => VK_NUMPAD8,
            "NUMPAD9" => VK_NUMPAD9, "MULTIPLY" => VK_MULTIPLY, "ADD" => VK_ADD,
            "SUBTRACT" => VK_SUBTRACT, "DECIMAL" => VK_DECIMAL, "DIVIDE" => VK_DIVIDE,
            
            _ => 0
        };
    }
}
