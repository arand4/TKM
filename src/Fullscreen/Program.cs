using System;
using System.Windows;

namespace TouchKeyboardMouse.Fullscreen
{
    public partial class App : Application
    {
        [STAThread]
        public static void Main()
        {
            var app = new App();
            var window = new MainWindow();
            window.WindowStyle = WindowStyle.None;
            window.ResizeMode = ResizeMode.NoResize;
            window.WindowState = WindowState.Maximized;
            window.Show();
            app.Run();
        }
    }
}
