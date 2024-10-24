using CFMediaPlayer.Models;

namespace CFMediaPlayer.Interfaces
{
    /// <summary>
    /// Interface to overall state. This is for sharing data between pages.
    /// </summary>
    public interface ICurrentState
    {
        /// <summary>
        /// Whether shuffle play is enabled
        /// </summary>
        bool ShufflePlay { get; set; }

        /// <summary>
        /// Whether auto-play next is enabled
        /// </summary>
        bool AutoPlayNext { get; set; }

        /// <summary>
        /// Action to set tab
        /// </summary>
        Action<string> SelectTabByTitleAction { get; set; }

        /// <summary>
        /// Selected media source
        /// </summary>
        IMediaSource? SelectedMediaSource { get; set; }

        /// <summary>
        /// Selected media location
        /// </summary>
        MediaLocation? SelectedMediaLocation { get; set; }

        /// <summary>
        /// Selected artist
        /// </summary>
        Artist? SelectedArtist { get; set; }

        /// <summary>
        /// Selected media item collection
        /// </summary>
        MediaItemCollection? SelectedMediaItemCollection { get; set; }

        /// <summary>
        /// Selected media item
        /// </summary>
        MediaItem? SelectedMediaItem { get; set; }

        /// <summary>
        /// Media items
        /// </summary>
        List<MediaItem> MediaItems { get; set; }

        /// <summary>
        /// Action to select media item. Assumed within same current media item collection.
        /// </summary>
        public Action<MediaItem>? SelectMediaItemAction { get; set; }

        /// <summary>
        /// Action to select media item collection. E.g. User selects "Open album X" media action.
        /// </summary>
        public Action<MediaLocation, Artist, MediaItemCollection>? SelectMediaItemCollectionAction { get; set; }

        /// <summary>
        /// Action when queue updated
        /// </summary>
        public Action? QueueUpdatedAction { get; set; }        

        public Action? UserSettingsUpdatedAction { get; set; }

        /// <summary>
        /// Register method to be notified when SelectedMediaItem changes
        /// </summary>
        /// <param name="action"></param>
        void RegisterSelectedMediaItemChanged(Action action);

        public IMediaPlayer? MediaPlayer { get; set; }
    }
}
