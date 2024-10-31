using CFMediaPlayer.Enums;

namespace CFMediaPlayer.Models
{
    /// <summary>
    /// User settings
    /// </summary>
    public class UserSettings
    {
        /// <summary>
        /// Unique Id
        /// </summary>
        public string Id { get; set; } = String.Empty;

        /// <summary>
        /// User name
        /// </summary>
        public string Username { get; set; } = String.Empty;

        /// <summary>
        /// Culture for translations
        /// </summary>
        public string CultureName { get; set; } = String.Empty;

        /// <summary>
        /// UI Theme Id
        /// </summary>
        public string UIThemeId { get; set; } = String.Empty;

        /// <summary>
        /// Audio settings. Refers to either AudioSettings.Id (Preset) or CustomAudioSettings
        /// </summary>
        public string AudioSettingsId { get; set; } = String.Empty;
       
        /// <summary>
        /// Custom audio settings
        /// </summary>
        public List<CustomAudioSettings> CustomAudioSettingsList { get; set; } = new List<CustomAudioSettings>();

        /// <summary>
        /// Cloud credentials
        /// </summary>
        public List<CloudCredentials> CloudCredentialList { get; set; } = new();
    }
}
