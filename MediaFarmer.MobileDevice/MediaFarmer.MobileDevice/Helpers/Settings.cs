// Helpers/Settings.cs
using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace MediaFarmer.MobileDevice.Helpers
{
  /// <summary>
  /// This is the Settings static class that can be used in your Core solution or in any
  /// of your client applications. All settings are laid out the same exact way with getters
  /// and setters. 
  /// </summary>
  public static class Settings
  {
        private static ISettings AppSettings;

    #region Setting Constants

    private const string HostKey = "settings_key";
    private static readonly string HostKeyDefault = string.Empty;

    #endregion


    public static string HostKeySettings
    {
      get
      {
        return AppSettings.GetValueOrDefault<string>(HostKey, HostKeyDefault);
      }
      set
      {
        AppSettings.AddOrUpdateValue<string>(HostKey, value);
      }
    }

  }
}