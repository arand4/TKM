using System;
using System.Windows;
using System.Diagnostics;
using System.Threading;

namespace TouchKeyboardMouse;

public partial class App : Application
{
    private Timer? _heartbeatTimer;
    public App()
    {
        this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
        _heartbeatTimer = new Timer(HeartbeatLog, null, 2000, 2000);
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

    private void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
    {
        TouchKeyboardMouse.Helpers.AppLogger.Log($"FirstChanceException: {e.Exception.Message}\n{e.Exception.StackTrace}");
    }

    private void HeartbeatLog(object? state)
    {
        try
        {
            this.Dispatcher.Invoke(() =>
            {
                var mainWindow = Current?.MainWindow as TouchKeyboardMouse.MainWindow;
                if (mainWindow != null)
                {
                    string info = $"Heartbeat: WindowState={mainWindow.WindowState}, NumpadVisible={mainWindow.NumpadPanel?.Visibility}, TrackpadWidth={mainWindow.Trackpad?.Width}, TrackpadActualWidth={mainWindow.Trackpad?.ActualWidth}, NumpadColumnWidth={mainWindow.NumpadColumn?.Width.Value}";
                    TouchKeyboardMouse.Helpers.AppLogger.Log(info);
                }
                else
                {
                    TouchKeyboardMouse.Helpers.AppLogger.Log("Heartbeat: MainWindow not available");
                }
            });
        }
        catch (Exception ex)
        {
            TouchKeyboardMouse.Helpers.AppLogger.LogException(ex, "HeartbeatLog");
        }
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
    }
}
