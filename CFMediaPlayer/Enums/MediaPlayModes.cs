using System.ComponentModel.DataAnnotations;

namespace CFMediaPlayer.Enums
{
    /// <summary>
    /// Media play modes
    /// </summary>
    public enum MediaPlayModes
    {
        /// <summary>
        /// Plays media item and stops
        /// </summary>
        [Display(Description = "MediaPlayModesSingleMediaItem")]
        SingleMediaItem,

        /// <summary>
        /// Plays each media item in sequence
        /// </summary>
        [Display(Description = "MediaPlayModesSequential")]
        Sequential
    }
}
