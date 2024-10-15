using CFMediaPlayer.Enums;
using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;
using static AndroidX.ConstraintLayout.Core.Motion.Utils.HyperSpline;

namespace CFMediaPlayer.Services
{
    public class MediaLocationService : IMediaLocationService
    {
        private readonly ICloudProviderService _cloudProviderService;

        public MediaLocationService(ICloudProviderService cloudProviderService)
        {
            _cloudProviderService = cloudProviderService;
        }

        public List<MediaLocation> GetAll()
        {
            // TODO: Filter available locations
            // Set media locations
            var mediaLocations = new List<MediaLocation>()
            {
                new MediaLocation() { Name = LocalizationResources.Instance["MediaSourceInternalStorageText"].ToString() + " (Test)",
                                MediaSourceType = MediaSourceTypes.Storage,
                                Source = "/storage/emulated/0/Download" },      // TODO: Remove this                
                
                new MediaLocation() { Name = LocalizationResources.Instance["MediaSourceInternalStorageText"].ToString(),
                                MediaSourceType = MediaSourceTypes.Storage,
                                Source = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, Android.OS.Environment.DirectoryMusic) },
            
                new MediaLocation() { Name = LocalizationResources.Instance["MediaSourceSDCardText"].ToString(),
                                MediaSourceType = MediaSourceTypes.Storage,
                                Source = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, Android.OS.Environment.DirectoryMusic) },

                new MediaLocation() { Name = "/sdcard (Test)",
                                MediaSourceType = MediaSourceTypes.Storage,
                                Source = Path.Combine("/sdcard", Android.OS.Environment.DirectoryMusic) },

                new MediaLocation() { Name = "/storage/emulated/0/Music (Test)",
                                MediaSourceType = MediaSourceTypes.Storage,
                                Source = Path.Combine("/storage/emulated/0", Android.OS.Environment.DirectoryMusic) },                              

                //new MediaLocation() { Name = "Music Test (ExternalStorageDirectory)",
                //                MediaSourceType = MediaSourceTypes.Storage,
                //                RootFolderPath = Android.OS.Environment.ExternalStorageDirectory.Path },                                

                new MediaLocation() { Name = LocalizationResources.Instance["MediaSourcePlaylistsText"].ToString(),
                                MediaSourceType = MediaSourceTypes.Playlist,
                                Source = FileSystem.AppDataDirectory },

                new MediaLocation() { Name = LocalizationResources.Instance["MediaSourceQueueText"].ToString(),
                                MediaSourceType = MediaSourceTypes.Queue,
                                Source = "" },                
            };

            // Add cloud providers
            foreach(var cloudProvider in _cloudProviderService.GetAll())
            {
                mediaLocations.Add(new MediaLocation()
                {
                    Name = LocalizationResources.Instance[cloudProvider.NameResource].ToString(),
                    MediaSourceType = MediaSourceTypes.Cloud,
                    Source = cloudProvider.Url
                });
            }

            return mediaLocations;
        }
    }
}
