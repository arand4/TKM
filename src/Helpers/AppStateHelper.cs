using System;
using System.IO;
using System.Windows;
using System.Text.Json;

namespace TouchKeyboardMouse.Helpers
{
    public class AppState
    {
        public double WindowLeft { get; set; }
        public double WindowTop { get; set; }
        public double WindowWidth { get; set; }
        public double WindowHeight { get; set; }
        public bool IsFullscreen { get; set; }
        public double TrackpadWidth { get; set; }
        public bool NumpadVisible { get; set; }
        // Add settings properties as needed
    }

    public static class AppStateHelper
    {
        private static readonly string StateFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TouchKeyboardMouse", "appstate.json");

        public static void Save(AppState state)
        {
            try
            {
                var dir = Path.GetDirectoryName(StateFile);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                var json = JsonSerializer.Serialize(state);
                File.WriteAllText(StateFile, json);
            }
            catch { }
        }

        public static AppState Load()
        {
            try
            {
                if (File.Exists(StateFile))
                {
                    var json = File.ReadAllText(StateFile);
                    return JsonSerializer.Deserialize<AppState>(json) ?? new AppState();
                }
            }
            catch { }
            return new AppState();
        }
    }
}
