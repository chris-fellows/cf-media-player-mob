﻿using CFMediaPlayer.Enums;

namespace CFMediaPlayer.Models
{
    /// <summary>
    /// System settings. E.g. Defaults for new user
    /// </summary>
    public class SystemSettings
    {
        /// <summary>
        /// Unique Id
        /// </summary>
        public string Id { get; set; } = String.Empty;

        /// <summary>
        /// Default UI theme (UITheme.Id)
        /// </summary>
        public string DefaultUIThemeId { get; set; } = String.Empty;

        /// <summary>
        /// Default audio settings (AudioSettings.Id)
        /// </summary>
        public string DefaultAudioSettingsId { get; set; } = String.Empty;
    }
}
