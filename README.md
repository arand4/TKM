# Touch Keyboard & Mouse (TKM)

A Windows application that provides a fullscreen virtual keyboard with an integrated trackpad, designed for dual-screen devices where one screen serves as a touch input surface.

## Features

- **Full QWERTY Keyboard**: Complete keyboard layout with F-keys, modifiers, and special keys
- **Smart Trackpad**: 
  - Single-finger drag to move the cursor
  - Tap to left-click
  - Double-tap for double-click
  - Two-finger drag to scroll
  - Dedicated left/right click buttons
- **Modifier Key Support**: Shift, Ctrl, Alt, and Windows key with visual state indicators
- **Intelligent Posture Detection**: 
  - Automatically detects screen arrangement (vertical vs side-by-side)
  - Shows keyboard only in "Book Mode" (screens stacked vertically)
  - Auto-hides when screens are side-by-side ("Laptop Mode")
  - Automatically positions on the bottom screen
  - Real-time monitoring for device rotation/posture changes
- **System Tray Integration**: Stays in system tray when hidden, shows posture status
- **Non-activating Window**: Doesn't steal focus from the app you're typing into
- **Customizable**: Adjustable sensitivity settings for cursor and scroll

## Target Use Cases

- **Microsoft Surface Duo** running Windows
- **Surface Neo** and similar dual-screen devices
- **Laptop+tablet** setups where you want the tablet as an input device
- **Touch-enabled secondary monitors**

## Posture Detection

The app automatically detects how your screens are arranged:

| Arrangement | Behavior |
|-------------|----------|
| **Book Mode** (screens stacked vertically) | ✅ Keyboard appears on bottom screen |
| **Laptop Mode** (screens side-by-side) | ❌ Keyboard hides automatically |
| **Single Screen** | ✅ Keyboard shown on the screen |

When hidden in side-by-side mode, the app stays in the system tray and will automatically reappear when you rotate the device to book mode.

## Requirements

- Windows 10/11
- .NET 8.0 Runtime
- Touch-enabled display (recommended)

## Building from Source

### Prerequisites

1. Install [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
2. Install Visual Studio 2022 or VS Code with C# extension

### Build

```bash
cd src
dotnet build
```

### Run

```bash
cd src
dotnet run
```

### Publish (Create Standalone Executable)

```bash
cd src
# Self-contained deployment (no .NET runtime needed)
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# Or framework-dependent (smaller, requires .NET runtime)
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true
```

The executable will be in `src/bin/Release/net8.0-windows/win-x64/publish/`

## Usage

1. Launch `TouchKeyboardMouse.exe`
2. The app will open fullscreen on your secondary monitor (if available)
3. Use the virtual keyboard to type into any application
4. Use the trackpad area to move the cursor and click
5. Press `Esc` or click the ✕ button to close

### Keyboard Shortcuts

- **Esc**: Exit the application
- **Modifier keys** (Shift, Ctrl, Alt): Toggle on/off, auto-release after next key press

### Trackpad Gestures

| Gesture | Action |
|---------|--------|
| Single finger drag | Move cursor |
| Tap | Left click |
| Double tap | Double click |
| Two-finger drag | Scroll |
| Left button | Left mouse button (hold for drag) |
| Right button | Right mouse button |

## Settings

Click the ⚙ (gear) icon to access settings:

- **Cursor Sensitivity**: Adjust how fast the cursor moves (0.5x - 3x)
- **Scroll Sensitivity**: Adjust scroll speed (0.5x - 5x)
- **Tap Threshold**: Time window for tap detection (100ms - 500ms)
- **Always on Top**: Keep the keyboard above other windows
- **Open on Secondary Monitor**: Automatically position on second screen

## Architecture

```
src/
├── App.xaml                   # Application entry point
├── MainWindow.xaml            # Main fullscreen window
├── SettingsWindow.xaml        # Settings dialog
├── TrayIconManager.cs         # System tray icon management
├── Controls/
│   ├── VirtualKeyboard.xaml   # Keyboard control
│   └── Trackpad.xaml          # Trackpad control
├── Helpers/
│   ├── InputSimulator.cs      # Windows SendInput API wrapper
│   ├── VirtualKeyCodes.cs     # Virtual key code constants
│   └── DualScreenHelper.cs    # Screen posture detection
└── Styles/
    └── KeyboardStyles.xaml    # UI styles and themes
```

## Known Limitations

- Input injection may not work in some elevated (admin) applications unless TKM is also run as admin
- Some games with anti-cheat may block simulated input
- Multi-touch gestures require a touch-enabled display
- Posture detection relies on Windows display arrangement settings

## Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.

## License

MIT License - See [LICENSE](LICENSE) file for details.