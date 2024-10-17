using Android.Net.Wifi.Aware;
using CFMediaPlayer.Models;

namespace CFMediaPlayer.Utilities
{
    /// <summary>
    /// Media utilities
    /// </summary>
    internal class MediaUtilities
    {
        /// <summary>
        /// Whether folder contains audio files in root
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsFolderHasAudioFiles(string path)
        {            
            foreach (var extension in AudioFileExtensions)
            {
                if (Directory.GetFiles(path, $"*{extension}").Any())
                {
                    return true;
                }
            }

            return false;
        }

        public static string[] AudioFileExtensions
        {
            get { return new[] { ".flac", ".mp3", ".ogg", ".wma", ".wav" }; }
        }

        public static bool IsNoneMediaItemAction(MediaItemAction mediaItemAction)
        {
            return mediaItemAction.Name == LocalizationResources.Instance["NoneText"].ToString();
        }

        public static bool IsRealMediaItem(MediaItem mediaItem)
        {
            return !IsNoneMediaItem(mediaItem) &&
                    !IsMultipleMediaItem(mediaItem) && 
                    !IsShuffleMediaItem(mediaItem);
        }

        public static bool IsRealArtist(Artist artist)
        {
            return !IsNoneArtist(artist) && 
                    !IsMultipleArtist(artist) && 
                    !IsShuffleArtist(artist);
        }

        public static bool IsRealMediaItemCollection(MediaItemCollection mediaItemCollection)
        {
            return !IsNoneMediaItemCollection(mediaItemCollection) && 
                    !IsMultipleMediaItemCollection(mediaItemCollection) && 
                    !IsShuffleMediaItemCollection(mediaItemCollection);
        }

        public static bool IsNoneMediaItem(MediaItem mediaItem)
        {
            return mediaItem.Name == LocalizationResources.Instance["NoneText"].ToString();
        }

        public static bool IsNoneArtist(Artist artist)
        {
            return artist.Name == LocalizationResources.Instance["NoneText"].ToString();
        }

        public static bool IsNoneMediaItemCollection(MediaItemCollection mediaItemCollection)
        {
            return mediaItemCollection.Name == LocalizationResources.Instance["NoneText"].ToString();
        }

        public static bool IsMultipleMediaItem(MediaItem mediaItem)
        {
            return mediaItem.Name == LocalizationResources.Instance["MultipleText"].ToString();
        }

        public static bool IsMultipleArtist(Artist artist)
        {
            return artist.Name == LocalizationResources.Instance["MultipleText"].ToString();
        }

        public static bool IsMultipleMediaItemCollection(MediaItemCollection mediaItemCollection)
        {
            return mediaItemCollection.Name == LocalizationResources.Instance["MultipleText"].ToString();
        }

        public static bool IsShuffleMediaItem(MediaItem mediaItem)
        {
            return mediaItem.Name == LocalizationResources.Instance["ShuffleText"].ToString();
        }

        public static bool IsShuffleArtist(Artist artist)
        {
            return artist.Name == LocalizationResources.Instance["ShuffleText"].ToString();
        }

        public static bool IsShuffleMediaItemCollection(MediaItemCollection mediaItemCollection)
        {
            return mediaItemCollection.Name == LocalizationResources.Instance["ShuffleText"].ToString();
        }

        public static void RemoveDuplicatesNotReal(List<Artist> artists)
        {
            var items = artists.Where(a => IsNoneArtist(a)).ToList();
            while (items.Count > 1)
            {
                var item = items.Last();
                artists.Remove(item);
                items.Remove(item);
            }

            items = artists.Where(a => IsMultipleArtist(a)).ToList();
            while (items.Count > 1)
            {
                var item = items.Last();
                artists.Remove(item);
                items.Remove(item);
            }

            items = artists.Where(a => IsShuffleArtist(a)).ToList();
            while (items.Count > 1)
            {
                var item = items.Last();
                artists.Remove(item);
                items.Remove(item);
            }
        }

        public static void RemoveDuplicatesNotReal(List<MediaItemCollection> mediaItemCollections)
        {
            var items = mediaItemCollections.Where(a => IsNoneMediaItemCollection(a)).ToList();
            while (items.Count > 1)
            {
                var item = items.Last();
                mediaItemCollections.Remove(item);
                items.Remove(item);
            }

            items = mediaItemCollections.Where(a => IsMultipleMediaItemCollection(a)).ToList();
            while (items.Count > 1)
            {
                var item = items.Last();
                mediaItemCollections.Remove(item);
                items.Remove(item);
            }

            items = mediaItemCollections.Where(a => IsShuffleMediaItemCollection(a)).ToList();
            while (items.Count > 1)
            {
                var item = items.Last();
                mediaItemCollections.Remove(item);
                items.Remove(item);
            }
        }

        public static void RemoveDuplicatesNotReal(List<MediaItem> mediaItems)
        {
            var items = mediaItems.Where(a => IsNoneMediaItem(a)).ToList();
            while (items.Count > 1)
            {
                var item = items.Last();
                mediaItems.Remove(item);
                items.Remove(item);
            }

            items = mediaItems.Where(a => IsMultipleMediaItem(a)).ToList();
            while (items.Count > 1)
            {
                var item = items.Last();
                mediaItems.Remove(item);
                items.Remove(item);
            }

            items = mediaItems.Where(a => IsShuffleMediaItem(a)).ToList();
            while (items.Count > 1)
            {
                var item = items.Last();
                mediaItems.Remove(item);
                items.Remove(item);
            }
        }
    }
}
