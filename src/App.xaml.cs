using System;
using System.Windows;

namespace TouchKeyboardMouse;

public partial class App : Application
{
    public App()
    {
        this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
    }

    private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        TouchKeyboardMouse.Helpers.AppLogger.LogException(e.Exception, "DispatcherUnhandledException");
        e.Handled = true;
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
            TouchKeyboardMouse.Helpers.AppLogger.LogException(ex, "AppDomainUnhandledException");
        else
            TouchKeyboardMouse.Helpers.AppLogger.Log($"Unhandled non-Exception: {e.ExceptionObject}");
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
    }
}
