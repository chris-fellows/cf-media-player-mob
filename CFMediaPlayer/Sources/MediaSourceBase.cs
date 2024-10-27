using CFMediaPlayer.Enums;
using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;
using System.Runtime.CompilerServices;

namespace CFMediaPlayer.Sources
{
    public abstract class MediaSourceBase
    {
        protected readonly ICurrentState _currentState;
        protected List<IMediaSource> _allMediaSources = new List<IMediaSource>();        
        protected readonly MediaLocation _mediaLocation;

        public MediaSourceBase(ICurrentState currentState, 
                MediaLocation mediaLocation)
        {
            _currentState = currentState;
            _mediaLocation = mediaLocation;
        }

        public void SetAllMediaSources(List<IMediaSource> allMediaSources)
        {
            _allMediaSources = allMediaSources;
        }

        public MediaLocation MediaLocation => _mediaLocation;


        /// <summary>
        /// Gets media item collection image path for media item
        /// </summary>
        /// <param name="mediaItem"></param>        
        /// <returns></returns>
        protected string GetMediaItemCollectionImagePath(MediaItem mediaItem)
        {
            foreach (var mediaSource in _allMediaSources.Where(ms => ms.MediaLocation.Name != _mediaLocation.Name &&
                                ms.MediaLocation.MediaSourceType == MediaSourceTypes.Storage))
            {
                var ancestors = mediaSource.GetAncestorsForMediaItem(mediaItem).FirstOrDefault();
                if (ancestors != null && !String.IsNullOrEmpty(ancestors.Item2.ImagePath))
                {
                    return ancestors.Item2.ImagePath;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets media item by file from original source
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        protected MediaItem? GetMediaItemByFileFromOriginalSource(string file)
        {            
            MediaItem? mediaItem = null;

            foreach (var mediaSource in _allMediaSources.Where(ms => ms.MediaLocation.MediaSourceType == MediaSourceTypes.Storage &&
                                    ms.IsAvailable))
            {
                mediaItem = mediaSource.GetMediaItemByFile(file);
                if (mediaItem != null) return mediaItem;
            }

            return null;
        }

        protected string GetImagePathByMediaItemTypes()
        {
            if (_mediaLocation.MediaItemTypes.Contains(MediaItemTypes.Audiobooks)) return "audio_book.png";
            if (_mediaLocation.MediaItemTypes.Contains(MediaItemTypes.Podcasts)) return "microphone.png";
            if (_mediaLocation.MediaItemTypes.Contains(MediaItemTypes.RadioStreams)) return "radio.png";
            if (_mediaLocation.MediaItemTypes.Contains(MediaItemTypes.PlaylistMediaItems)) return "playlist.png";
            return "music.png";
        }
    }
}
