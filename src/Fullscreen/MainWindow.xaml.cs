using System.Windows;
using TouchKeyboardMouse.Shared;

namespace TouchKeyboardMouse.Fullscreen
{
    public partial class MainWindow : Window
    {
        private AppState? _appState;
        private bool _numpadVisible = false;
        private bool _trackpadWasFullWidth = true;
        public MainWindow()
        {
            InitializeComponent();
            _appState = AppStateHelper.Load<AppState>();
            if (_appState != null && _appState.TrackpadWidthFullscreen > 0 && Trackpad != null)
                Trackpad.Width = _appState.TrackpadWidthFullscreen;
            _numpadVisible = _appState != null && _appState.NumpadEnabled;
        }
        // ...existing logic for keyboard, trackpad, numpad, settings...
        private void SaveSettings()
        {
            if (_appState == null) _appState = new AppState();
            _appState.NumpadEnabled = _numpadVisible;
            _appState.TrackpadWidthFullscreen = Trackpad?.Width ?? 0;
            AppStateHelper.Save(_appState);
        }
        // Call SaveSettings() whenever numpad or trackpad width changes
    }
}
