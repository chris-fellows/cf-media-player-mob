using CFMediaPlayer.Enums;
using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;

namespace CFMediaPlayer.Sources
{
    public abstract class MediaSourceBase
    {
        protected List<IMediaSource> _allMediaSources = new List<IMediaSource>();        
        protected readonly MediaLocation _mediaLocation;

        public MediaSourceBase(MediaLocation mediaLocation)
        {
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
    }
}
