using Plugin.Settings;
using Plugin.Settings.Abstractions;
using Newtonsoft.Json;
using System.Runtime;

namespace Lexoh
{
    public class AppSettings
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public bool UseFingerprint { get; set; }
        public string Version { get; set; }
    }

    public static class Settings
    {
        static ISettings AppSettings
        {
            get { return CrossSettings.Current; }
        }

        public static AppSettings Current = new AppSettings();

        #region Setting Constants
        static readonly string SettingsDefault = JsonConvert.SerializeObject(new AppSettings());
        #endregion

        public static string GeneralSettings
        {
            get { return AppSettings.GetValueOrDefault(nameof(GeneralSettings), SettingsDefault); }
            set { AppSettings.AddOrUpdateValue(nameof(GeneralSettings), value); }
        }

        public static string UrlSettings
        {
            get { return AppSettings.GetValueOrDefault("url_key", ""); }
            set { AppSettings.AddOrUpdateValue("url_key", value); }
        }

        public static void SaveSettings()
        {
            GeneralSettings = JsonConvert.SerializeObject(Current);
        }

        public static void LoadSettings()
        {
            Current = JsonConvert.DeserializeObject<AppSettings>(GeneralSettings);
        }
    }
}