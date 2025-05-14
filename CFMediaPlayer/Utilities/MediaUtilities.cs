using Android.Net;
using Android.Net.Wifi;
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

        public static bool IsNoneMediaItemAction(MediaAction mediaItemAction)
        {
            return String.IsNullOrEmpty(mediaItemAction.MediaItemFile) &&
                        mediaItemAction.Name == LocalizationResources.Instance["NoneText"].ToString();
        }           

        ///// <summary>
        ///// Removes duplicates artist for not real items
        ///// </summary>
        ///// <param name="artists"></param>
        //public static void RemoveDuplicatesNotReal(List<Artist> artists)
        //{
        //    foreach (EntityCategory entityCategory in new[] { EntityCategory.None, EntityCategory.All })
        //    {
        //        var items = artists.Where(a => a.EntityCategory == entityCategory).ToList();
        //        while (items.Count > 1)
        //        {
        //            var item = items.Last();
        //            artists.Remove(item);
        //            items.Remove(item);
        //        }
        //    }
        //}

        ///// <summary>
        ///// Removes duplicate media item collections for not real items
        ///// </summary>
        ///// <param name="mediaItemCollections"></param>
        //public static void RemoveDuplicatesNotReal(List<MediaItemCollection> mediaItemCollections)
        //{
        //    foreach (EntityCategory entityCategory in new[] { EntityCategory.None, EntityCategory.All })
        //    {
        //        var items = mediaItemCollections.Where(a => a.EntityCategory == entityCategory).ToList();
        //        while (items.Count > 1)
        //        {
        //            var item = items.Last();
        //            mediaItemCollections.Remove(item);
        //            items.Remove(item);
        //        }
        //    }
        //}

        ///// <summary>
        ///// Removes duplicate media items for not real items
        ///// </summary>
        ///// <param name="mediaItems"></param>
        //public static void RemoveDuplicatesNotReal(List<MediaItem> mediaItems)
        //{
        //    foreach (EntityCategory entityCategory in new[] { EntityCategory.None, EntityCategory.All })
        //    {
        //        var items = mediaItems.Where(a => a.EntityCategory == EntityCategory.None).ToList();
        //        while (items.Count > 1)
        //        {
        //            var item = items.Last();
        //            mediaItems.Remove(item);
        //            items.Remove(item);
        //        }
        //    }           
        //}

        /// <summary>
        /// Gets media item name from media item path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetMediaItemNameForMediaItemPath(string path)
        {
            return Path.GetFileNameWithoutExtension(path);
        }

        /// <summary>
        /// Gets media item collection name from media item path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetMediaItemCollectionNameForMediaItemPath(string path)
        {
            return new DirectoryInfo(Path.GetDirectoryName(path)).Name;
        }
        
        /// <summary>
        /// Gets artist name from media item path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetArtistNameForMediaItemPath(string path)
        {
            return new DirectoryInfo(Path.GetDirectoryName(path)).Parent.Name;
        }        

        /// <summary>
        /// Gets image path for media item collection
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public static string GetMediaItemCollectionImagePath(string folder)
        {
            if (String.IsNullOrEmpty(folder) || !Directory.Exists(folder)) return String.Empty;
            var files = Directory.GetFiles(folder, "Folder.jpg"); // Hard-coding is fine for the moment
            if (files.Any()) return files[0];
            return String.Empty;            
        }    
    }
}
