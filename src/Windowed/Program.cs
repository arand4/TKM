using System;
using System.Windows;

namespace TouchKeyboardMouse.Windowed
{
    public partial class App : Application
    {
        [STAThread]
        public static void Main()
        {
            var app = new App();
            var window = new MainWindow();
            window.WindowStyle = WindowStyle.SingleBorderWindow;
            window.ResizeMode = ResizeMode.CanResize;
            window.Show();
            app.Run();
        }
    }
}
