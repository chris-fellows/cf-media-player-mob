using Android.Net.Wifi.Aware;
using CFMediaPlayer.Enums;
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

        /// <summary>
        /// Audio files extensions supported by the app
        /// </summary>
        public static string[] AudioFileExtensions
        {
            get { return new[] { ".flac", ".mp3", ".ogg", ".wma", ".wav" }; }
        }

        public static bool IsNoneMediaItemAction(MediaItemAction mediaItemAction)
        {
            return String.IsNullOrEmpty(mediaItemAction.File) &&
                        mediaItemAction.Name == LocalizationResources.Instance["NoneText"].ToString();
        }           

        /// <summary>
        /// Removes duplicates artist for not real items
        /// </summary>
        /// <param name="artists"></param>
        public static void RemoveDuplicatesNotReal(List<Artist> artists)
        {
            foreach (EntityCategory entityCategory in new[] { EntityCategory.None, EntityCategory.Multiple, EntityCategory.All })
            {
                var items = artists.Where(a => a.EntityCategory == entityCategory).ToList();
                while (items.Count > 1)
                {
                    var item = items.Last();
                    artists.Remove(item);
                    items.Remove(item);
                }
            }
        }

        /// <summary>
        /// Removes duplicate media item collections for not real items
        /// </summary>
        /// <param name="mediaItemCollections"></param>
        public static void RemoveDuplicatesNotReal(List<MediaItemCollection> mediaItemCollections)
        {
            foreach (EntityCategory entityCategory in new[] { EntityCategory.None, EntityCategory.Multiple, EntityCategory.All })
            {
                var items = mediaItemCollections.Where(a => a.EntityCategory == entityCategory).ToList();
                while (items.Count > 1)
                {
                    var item = items.Last();
                    mediaItemCollections.Remove(item);
                    items.Remove(item);
                }
            }
        }

        /// <summary>
        /// Removes duplicate media items for not real items
        /// </summary>
        /// <param name="mediaItems"></param>
        public static void RemoveDuplicatesNotReal(List<MediaItem> mediaItems)
        {
            foreach (EntityCategory entityCategory in new[] { EntityCategory.None, EntityCategory.Multiple, EntityCategory.All })
            {
                var items = mediaItems.Where(a => a.EntityCategory == EntityCategory.None).ToList();
                while (items.Count > 1)
                {
                    var item = items.Last();
                    mediaItems.Remove(item);
                    items.Remove(item);
                }
            }           
        }
    }
}
