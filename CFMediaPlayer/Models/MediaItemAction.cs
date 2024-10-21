using CFMediaPlayer.Enums;

namespace CFMediaPlayer.Models
{
    /// <summary>
    /// Action that can be executed against a media item. E.g. Add to playlist, remove from playlist,
    /// add to queue etc.
    /// </summary>
    public class MediaItemAction
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
        public MediaItemActions ActionToExecute { get; set; }

        public string ImagePath { get; set; } = String.Empty;


        public static MediaItemAction InstanceNone => new MediaItemAction() { Name = LocalizationResources.Instance["NoneText"].ToString() };
    }
}
