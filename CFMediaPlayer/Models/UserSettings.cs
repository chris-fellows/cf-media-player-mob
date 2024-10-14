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
        /// UI Theme Id
        /// </summary>
        public string UIThemeId { get; set; } = String.Empty;

        /// <summary>
        /// Default play mode
        /// </summary>
        public MediaPlayModes PlayMode { get; set; }

        /// <summary>
        /// Cloud credentials
        /// </summary>
        public List<CloudCredentials> CloudCredentialList { get; set; } = new();
    }
}
