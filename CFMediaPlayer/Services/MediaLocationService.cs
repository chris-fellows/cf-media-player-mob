using CFMediaPlayer.Enums;
using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;

namespace CFMediaPlayer.Services
{
    public class MediaLocationService : IMediaLocationService
    {
        public List<MediaLocation> GetAll()
        {
            // Set media locations
            var mediaLocations = new List<MediaLocation>()
            {
                new MediaLocation() { Name = LocalizationResources.Instance["MediaSourceInternalStorageText"].ToString() + " (Test)",
                                MediaSourceType = MediaSourceTypes.Storage,
                                RootFolderPath = "/storage/emulated/0/Download" },      // TODO: Remove this                
                new MediaLocation() { Name = LocalizationResources.Instance["MediaSourceInternalStorageText"].ToString(),
                                MediaSourceType = MediaSourceTypes.Storage,
                                RootFolderPath = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, Android.OS.Environment.DirectoryMusic) },

                //new MediaLocation() { Name = "Internal storage", MediaSourceName = MediaSourceNames.Storage, 
                //                RootFolderPath = Path.Combine(Android.OS.Environment.StorageDirectory.Path, Android.OS.Environment.DirectoryMusic) },

                new MediaLocation() { Name = LocalizationResources.Instance["MediaSourceSDCardText"].ToString(),
                                MediaSourceType = MediaSourceTypes.Storage,
                                RootFolderPath = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, Android.OS.Environment.DirectoryMusic) },

                //new MediaLocation() { Name = "Music Test (DirectoryMusic)",
                //                MediaSourceType = MediaSourceTypes.Storage,
                //                RootFolderPath = Android.OS.Environment.DirectoryMusic },

                //new MediaLocation() { Name = "Music Test (StorageDirectory)",
                //                MediaSourceType = MediaSourceTypes.Storage,
                //                RootFolderPath = Android.OS.Environment.StorageDirectory.Path },

                //new MediaLocation() { Name = "Music Test (ExternalStorageDirectory)",
                //                MediaSourceType = MediaSourceTypes.Storage,
                //                RootFolderPath = Android.OS.Environment.ExternalStorageDirectory.Path },                                

                new MediaLocation() { Name = LocalizationResources.Instance["MediaSourcePlaylistsText"].ToString(),
                                MediaSourceType = MediaSourceTypes.Playlist,
                                RootFolderPath = FileSystem.AppDataDirectory },

                new MediaLocation() { Name = LocalizationResources.Instance["MediaSourceQueueText"].ToString(),
                                MediaSourceType = MediaSourceTypes.Queue,
                                RootFolderPath = "" }
            };

            return mediaLocations;
        }
    }
}
