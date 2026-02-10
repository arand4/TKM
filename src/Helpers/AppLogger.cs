using System;
using System.IO;

namespace TouchKeyboardMouse.Helpers
{
    public static class AppLogger
    {
        private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TKM.log");

        public static void Log(string message)
        {
            try
            {
                File.AppendAllText(LogFilePath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n");
            }
            catch { /* Ignore logging errors */ }
        }

        public static void LogException(Exception ex, string context = "")
        {
            Log($"ERROR{(string.IsNullOrEmpty(context) ? "" : $" ({context})")}: {ex.Message}\n{ex.StackTrace}");
        }
    }
}
