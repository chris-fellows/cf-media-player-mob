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
        /// File, type determined by ActionToExecute:
        /// - If playlist related then refers to playlist file.
        /// - If media item related then refers to media item file. It doesn't matter too much because the MediaItem
        ///   is passed to the Execute... method and it contains the file.
        /// </summary>
        public string File { get; set; } = String.Empty;

        /// <summary>
        /// Action
        /// </summary>
        public MediaItemActions ActionToExecute { get; set; }

        public static MediaItemAction InstanceNone => new MediaItemAction() { Name = LocalizationResources.Instance["NoneText"].ToString() };
    }
}
