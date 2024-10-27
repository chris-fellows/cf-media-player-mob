using AndroidX.Core.Util;

namespace CFMediaPlayer.Models
{
    /// <summary>
    /// Events for current state
    /// </summary>
    public class CurrentStateEvents
    {        
        public delegate void QueueUpdated(MediaItem? mediaItem);
        public event QueueUpdated? OnQueueUpdated;

        public delegate void UserSettingsUpdated(UserSettings userSettings);
        public event UserSettingsUpdated? OnUserSettingsUpdated;

        public delegate void PlaylistUpdated(MediaItemCollection mediaItemCollection, MediaItem? mediaItem);
        public event PlaylistUpdated? OnPlaylistUpdated;

        public delegate void SelectedMediaItemChanged(MediaItem mediaItem);
        public event SelectedMediaItemChanged? OnSelectedMediaItemChanged;

        public delegate void CurrentMediaItemStatusChanged(MediaItem mediaItem, bool isPlaying, bool isPaused);
        public event CurrentMediaItemStatusChanged? OnCurrentMediaItemStatusChanged;

        public void RaiseOnQueueUpdated(MediaItem? mediaItem)
        {
            if (OnQueueUpdated != null)
            {
                OnQueueUpdated(mediaItem);
            }
        }

        public void RaiseOnUserSettingsUpdated(UserSettings userSettings)
        {
            if (OnUserSettingsUpdated != null)
            {
                OnUserSettingsUpdated(userSettings);
            }
        }

        public void RaiseOnPlaylistUpdated(MediaItemCollection mediaItemCollection, MediaItem? mediaItem)
        {
            if (OnPlaylistUpdated != null)
            {
                OnPlaylistUpdated(mediaItemCollection, mediaItem);
            }
        }

        public void RaiseOnSelectedMediaItemChanged(MediaItem mediaItem)
        {
            if (OnSelectedMediaItemChanged != null)
            {
                OnSelectedMediaItemChanged(mediaItem);
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
