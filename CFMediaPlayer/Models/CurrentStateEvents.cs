using AndroidX.Core.Util;
using CFMediaPlayer.Enums;

namespace CFMediaPlayer.Models
{
    /// <summary>
    /// Events for current state
    /// </summary>
    public class CurrentStateEvents
    {        
        public delegate void QueueUpdated(SystemEventTypes systemEventType, MediaItem? mediaItem);
        public event QueueUpdated? OnQueueUpdated;

        public delegate void UserSettingsUpdated(UserSettings userSettings);
        public event UserSettingsUpdated? OnUserSettingsUpdated;

        public delegate void PlaylistUpdated(SystemEventTypes systemEventTypes, MediaItemCollection mediaItemCollection, MediaItem? mediaItem);
        public event PlaylistUpdated? OnPlaylistUpdated;

        //public delegate void SelectedMediaItemChanged(MediaItem mediaItem);
        //public event SelectedMediaItemChanged? OnSelectedMediaItemChanged;

        public delegate void CurrentMediaItemStatusChanged(MediaItem mediaItem, bool isPlaying, bool isPaused);
        public event CurrentMediaItemStatusChanged? OnCurrentMediaItemStatusChanged;

        public delegate void PlayMediaItem(MediaItem mediaItem);
        public event PlayMediaItem? OnPlayMediaItem;

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

        //public void RaiseOnSelectedMediaItemChanged(MediaItem mediaItem)
        //{
        //    if (OnSelectedMediaItemChanged != null)
        //    {
        //        OnSelectedMediaItemChanged(mediaItem);
        //    }
        //}

        public void RaiseOnCurrentMediaItemStatusChanged(MediaItem mediaItem, bool isPlaying, bool isPaused)
        {
            if (OnCurrentMediaItemStatusChanged != null)
            {
                OnCurrentMediaItemStatusChanged(mediaItem, isPlaying, isPaused);
            }
        }
    }
}
