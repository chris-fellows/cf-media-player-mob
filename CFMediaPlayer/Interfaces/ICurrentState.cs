﻿using CFMediaPlayer.Enums;
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
        /// Selected media item. This is not necessarily the currently playing
        /// </summary>
        MediaItem? SelectedMediaItem { get; set; }

        /// <summary>
        /// Current media item (CurrentPage)
        /// </summary>
        MediaItem? CurrentMediaItem { get; set; }

        /// <summary>
        /// Media items
        /// </summary>
        List<MediaItem> MediaItems { get; set; }

        /// <summary>
        /// Media player
        /// </summary>
        public IMediaPlayer? MediaPlayer { get; set; }

        ///// <summary>
        ///// Action to select media item. Assumed within same current media item collection.
        ///// </summary>
        //public Action<MediaItem>? SelectMediaItemAction { get; set; }

        /// <summary>
        /// Action to select media item collection. E.g. User selects "Open album X" media action.
        /// </summary>
        public Action<MediaLocation, Artist, MediaItemCollection, MediaItem?>? SelectMediaItemCollectionAction { get; set; }

        ///// <summary>
        ///// Action when queue updated
        ///// </summary>
        //public Action? QueueUpdatedAction { get; set; }        

        ///// <summary>
        ///// Action when user settings updated
        ///// </summary>
        //public Action? UserSettingsUpdatedAction { get; set; }        

        ///// <summary>
        ///// Action when selected media item changed
        ///// </summary>
        //public Action<MediaItem>? SelectedMediaItemChangedAction { get; set; }        

        //public Action<MediaItemCollection, MediaItem?>? PlaylistUpdatedAction { get; set; }

        /// <summary>
        /// Events to raise
        /// </summary>
        public CurrentStateEvents Events { get; }        

        ///// <summary>
        ///// Register method to be notified when SelectedMediaItem changes
        ///// </summary>
        ///// <param name="action"></param>
        //void RegisterSelectedMediaItemChanged(Action action);

        //public Action<bool>? SetIsBusyAction { get; set; }

        /// <summary>
        /// Function to query the status media player status for the media item. Returns null if the media item isn't
        /// the current one that the player has.
        /// </summary>
        Func<MediaItem, MediaPlayerStatuses?>? GetMediaItemPlayStatusFunction { get; set; }

        public Action? SetNoMediaLocationAction { get; set; }
    }
}
