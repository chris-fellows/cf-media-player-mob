using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
//using static Android.Provider.MediaStore.Audio;

namespace CFMediaPlayer.Sources
{
    /// <summary>
    /// Media source from storage (Internal/SD card)
    /// </summary>
    internal class StorageMediaSource : IMediaSource
    {
        private string _rootPath;

        public StorageMediaSource()
        {
            //_rootPath = rootPath;
        }

        public string Name => "Storage";

        public void SetSource(string source)
        {
            _rootPath = source;
        }

        public bool IsAvailable
        {
            get
            {
                return !String.IsNullOrEmpty(_rootPath) &&
                    Directory.Exists(_rootPath);
            }
        }

        public List<Artist> GetArtists()
        {
            var artists = new List<Artist>();

            if (Directory.Exists(_rootPath))
            {
                var folders = Directory.GetDirectories(_rootPath);
                foreach (var folder in folders)
                {
                    // Check that folder contains albums
                    var isHasMediaItemCollections = false;
                    foreach (var subFolder in Directory.GetDirectories(folder))
                    {
                        if (InternalUtilities.IsFolderHasAudioFiles(subFolder))
                        {
                            isHasMediaItemCollections = true;
                            break;
                        }
                    }

                    if (isHasMediaItemCollections)
                    {
                        artists.Add(new Artist() { Path = folder });
                    }
                }
            }

            return artists;
        }

        public List<MediaItemCollection> GetMediaItemCollectionsForArtist(string artistName)
        {
            var mediaItemCollections = new List<MediaItemCollection>();

            var path = Path.Combine(_rootPath, artistName);
            if (Directory.Exists(path))
            {
                var folders = Directory.GetDirectories(path);
                foreach (var folder in folders)
                {
                    if (InternalUtilities.IsFolderHasAudioFiles(folder))
                    {
                        mediaItemCollections.Add(new MediaItemCollection() { Path = folder });
                    }
                }
            }

            return mediaItemCollections;
        }

        public List<MediaItem> GetMediaItemsForMediaItemCollection(string artistName, string mediaItemCollectionName)
        {
            var mediaItems = new List<MediaItem>();

            // TODO: Filter by extension
            var path = Path.Combine(_rootPath, artistName, mediaItemCollectionName);
            if (Directory.Exists(path))
            {
                var files = Directory.GetFiles(path);
                foreach(var file in files)
                {
                    if (Array.IndexOf(InternalUtilities.AudioFileExtensions, Path.GetExtension(file).ToLower()) != -1)
                    {
                        mediaItems.Add(new MediaItem() { Path = file });
                    }
                }
            }

            return mediaItems;
        }
    }
}
