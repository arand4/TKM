using System.Windows;
using System.Windows.Input;

namespace TouchKeyboardMouse;

public partial class SettingsWindow : Window
{
    public double CursorSensitivity { get; private set; } = 1.5;
    public double ScrollSensitivity { get; private set; } = 2.0;
    public int TapThreshold { get; private set; } = 200;
    public bool AlwaysOnTop { get; private set; } = true;
    public bool ShowOnSecondaryMonitor { get; private set; } = true;

    public SettingsWindow()
    {
        InitializeComponent();
    }

    private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            this.DragMove();
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        // Apply settings to main window before closing
        if (Owner is MainWindow mainWindow)
        {
            mainWindow.Topmost = AlwaysOnTopCheckBox.IsChecked ?? true;
            mainWindow.Trackpad.Sensitivity = CursorSensitivity;
            mainWindow.Trackpad.ScrollSensitivity = ScrollSensitivity;
            mainWindow.Trackpad.TapThresholdMs = TapThreshold;
        }
        
        this.Close();
    }

    private void CursorSensitivitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (CursorSensitivityValue != null)
        {
            CursorSensitivity = e.NewValue;
            CursorSensitivityValue.Text = $"{e.NewValue:F1}x";
        }
    }

    private void ScrollSensitivitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (ScrollSensitivityValue != null)
        {
            ScrollSensitivity = e.NewValue;
            ScrollSensitivityValue.Text = $"{e.NewValue:F1}x";
        }
    }

    private void TapThresholdSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (TapThresholdValue != null)
        {
            TapThreshold = (int)e.NewValue;
            TapThresholdValue.Text = $"{(int)e.NewValue}ms";
        }
    }

    private void AlwaysOnTopCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        AlwaysOnTop = AlwaysOnTopCheckBox.IsChecked ?? true;
    }

    private void ResetDefaults_Click(object sender, RoutedEventArgs e)
    {
        CursorSensitivitySlider.Value = 1.5;
        ScrollSensitivitySlider.Value = 2.0;
        TapThresholdSlider.Value = 200;
        AlwaysOnTopCheckBox.IsChecked = true;
        ShowOnSecondaryMonitorCheckBox.IsChecked = true;
    }
}
