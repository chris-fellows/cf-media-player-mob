using AndroidX.Core.Util;
using CFMediaPlayer.Enums;

namespace CFMediaPlayer.Models
{
    /// <summary>
    /// Events for current state
    /// </summary>
    public class CurrentStateEvents
    {
        /// <summary>
        /// Queue updated event. E.g. Cleared, media item added, media item removed
        /// </summary>
        /// <param name="systemEventType"></param>
        /// <param name="mediaItem"></param>
        public delegate void QueueUpdated(SystemEventTypes systemEventType, MediaItem? mediaItem);
        public event QueueUpdated? OnQueueUpdated;

        /// <summary>
        /// User settings updated event
        /// </summary>
        /// <param name="userSettings"></param>
        public delegate void UserSettingsUpdated(UserSettings userSettings);
        public event UserSettingsUpdated? OnUserSettingsUpdated;

        /// <summary>
        /// Playlist updated event. E.g. Cleared, media item added, media item removed
        /// </summary>
        /// <param name="systemEventTypes"></param>
        /// <param name="mediaItemCollection"></param>
        /// <param name="mediaItem"></param>
        public delegate void PlaylistUpdated(SystemEventTypes systemEventTypes, MediaItemCollection mediaItemCollection, MediaItem? mediaItem);
        public event PlaylistUpdated? OnPlaylistUpdated;

        //public delegate void SelectedMediaItemChanged(MediaItem mediaItem);
        //public event SelectedMediaItemChanged? OnSelectedMediaItemChanged;

        /// <summary>
        /// Player status of current media item changed. E.g. Playing, paused, completed etc.
        /// </summary>
        /// <param name="mediaItem"></param>
        /// <param name="isPlaying"></param>
        /// <param name="isPaused"></param>
        public delegate void CurrentMediaItemStatusChanged(MediaItem mediaItem, bool isPlaying, bool isPaused);
        public event CurrentMediaItemStatusChanged? OnCurrentMediaItemStatusChanged;

        /// <summary>
        /// Request to start playing media item
        /// </summary>
        /// <param name="mediaItem"></param>
        public delegate void PlayMediaItem(MediaItem mediaItem);
        public event PlayMediaItem? OnPlayMediaItem;

        /// <summary>
        /// Toggle play of media item (Play/pause/stop)
        /// </summary>
        /// <param name="mediaItem"></param>
        public delegate void TogglePlayMediaItem(MediaItem mediaItem);
        public event TogglePlayMediaItem? OnTogglePlayMediaItem;

        public void RaiseOnPlayMediaItem(MediaItem mediaItem)
        {
            if (OnPlayMediaItem != null)
            {
                OnPlayMediaItem(mediaItem);
            }
        }

        public void RaiseOnTogglePlayMediaItem(MediaItem mediaItem)
        {
            if (OnTogglePlayMediaItem != null)
            {
                OnTogglePlayMediaItem(mediaItem);
            }
        }

        public void RaiseOnQueueUpdated(SystemEventTypes systemEventType, MediaItem? mediaItem)
        {
            if (OnQueueUpdated != null)
            {
                OnQueueUpdated(systemEventType, mediaItem);
            }
        }

        public void RaiseOnUserSettingsUpdated(UserSettings userSettings)
        {
            if (OnUserSettingsUpdated != null)
            {
                OnUserSettingsUpdated(userSettings);
            }
        }

        public void RaiseOnPlaylistUpdated(SystemEventTypes systemEventType, MediaItemCollection mediaItemCollection, MediaItem? mediaItem)
        {
            if (OnPlaylistUpdated != null)
            {
                OnPlaylistUpdated(systemEventType, mediaItemCollection, mediaItem);
            }
        }

        public void RaiseOnCurrentMediaItemStatusChanged(MediaItem mediaItem, bool isPlaying, bool isPaused)
        {
            if (OnCurrentMediaItemStatusChanged != null)
            {
                OnCurrentMediaItemStatusChanged(mediaItem, isPlaying, isPaused);
            }
        }
    }
}
