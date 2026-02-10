using System.IO;
using System.Text.Json;

namespace TouchKeyboardMouse.Shared
{
    public static class AppStateHelper
    {
        private static string SettingsPath => Path.Combine(Directory.GetCurrentDirectory(), "TKM.settings.json");

        public static void Save<T>(T state)
        {
            var json = JsonSerializer.Serialize(state);
            File.WriteAllText(SettingsPath, json);
        }

        public static T? Load<T>()
        {
            if (!File.Exists(SettingsPath)) return default;
            var json = File.ReadAllText(SettingsPath);
            return JsonSerializer.Deserialize<T>(json);
        }
    }
}
