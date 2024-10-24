using CFMediaPlayer.Enums;

namespace CFMediaPlayer.Models
{
    /// <summary>
    /// Action that can be executed against media (Media item, playlists etc)
    /// </summary>
    public class MediaAction
    {
        /// <summary>
        /// Media location
        /// </summary>
        public string MediaLocationName { get; set; } = String.Empty;

        /// <summary>
        /// Action name for UI. E.g. "Add to playlist My Favourites", "Add to queue"
        /// </summary>
        public string Name { get; set; } = String.Empty;

        /// <summary>
        /// Media item file (if any)
        /// </summary>
        public string MediaItemFile { get; set; } = String.Empty;

        /// <summary>
        /// Playlist file (if any)
        /// </summary>
        public string PlaylistFile { get; set; } = String.Empty;

        /// <summary>
        /// Action
        /// </summary>
        public MediaActionTypes ActionType { get; set; }

        /// <summary>
        /// Image to display in UI
        /// </summary>
        public string ImagePath { get; set; } = String.Empty;


        public static MediaAction InstanceNone => new MediaAction() { Name = LocalizationResources.Instance["NoneText"].ToString() };
    }
}
