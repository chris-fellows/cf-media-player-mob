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
        /// Plays each media item in the collection in sequence
        /// </summary>
        [Display(Description = "MediaPlayModesSequential")]
        Sequential,

        /// <summary>
        /// Randomly plays media items in the collection
        /// </summary>
        [Display(Description = "MediaPlayModesShuffleMediaItemCollection")]
        ShuffleMediaItemCollection,

        /// <summary>
        /// Randomy plays media items in any collection for the artist
        /// </summary>
        [Display(Description = "MediaPlayModesShuffleArtist")]
        ShuffleArtist,

        /// <summary>
        /// Random plays media items in any collection for any artist
        /// </summary>
        [Display(Description = "MediaPlayModesShuffleStorage")]
        ShuffleStorage
    }
}
