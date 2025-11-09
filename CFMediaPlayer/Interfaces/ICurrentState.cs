using CFMediaPlayer.Enums;
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
        /// Action to select tab. E.g. User clicks "Go to album X]" on Current page and app opens the album on
        /// the Library page.
        /// </summary>
        Action<string>? SelectTabByTitleAction { get; set; }

        /// <summary>
        /// Selected media source (Library)
        /// </summary>
        IMediaSource? SelectedMediaSource { get; set; }

        /// <summary>
        /// Selected media location (Library)
        /// </summary>
        MediaLocation? SelectedMediaLocation { get; set; }

        /// <summary>
        /// Selected artist (Library)
        /// </summary>
        Artist? SelectedArtist { get; set; }

        /// <summary>
        /// Selected media item collection
        /// </summary>
        MediaItemCollection? SelectedMediaItemCollection { get; set; }

        /// <summary>
        /// Selected media item (Library). This is not necessarily the currently playing item.
        /// </summary>
        MediaItem? SelectedMediaItem { get; set; }

        /// <summary>
        /// Current media item (Current). User has pressed play to start this media item and it may be playing, 
        /// paused, stopped or completed.
        /// </summary>
        MediaItem? CurrentMediaItem { get; set; }

        /// <summary>
        /// Media items for SelectedMediaItemCollection.
        /// </summary>
        List<MediaItem> MediaItems { get; set; }

        ///// <summary>
        ///// Media player
        ///// </summary>
        //public IMediaPlayer? MediaPlayer { get; set; }

        /// <summary>
        /// Events to raise
        /// </summary>
        public CurrentStateEvents Events { get; }

        /// <summary>
        /// Action to select media item collection. E.g. User selects "Go to album X" media action.
        /// </summary>
        public Action<MediaLocation, Artist, MediaItemCollection, MediaItem?>? SelectMediaItemCollectionAction { get; set; }   

        /// <summary>
        /// Function to query the status media player status for the media item. Returns null if the media item isn't
        /// the current one that the player has.
        /// </summary>
        Func<MediaItem, MediaPlayerStatuses?>? GetMediaItemPlayStatusFunction { get; set; }

        /// <summary>
        /// Action to select search result
        /// </summary>
        public Action<SearchResult>? SelectSearchResultAction { get; set; }
    }
}
