using CFMediaPlayer.Enums;
using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;

namespace CFMediaPlayer.Services
{
    public class MediaLocationService : IMediaLocationService
    {
        private readonly ICloudProviderService _cloudProviderService;

        public MediaLocationService(ICloudProviderService cloudProviderService)
        {
            _cloudProviderService = cloudProviderService;
        }

        /// <summary>
        /// Gets all media locations. We don't check if there's any audio content.
        /// 
        /// For playlists, podcasts etc then we check for a folder at the same level as the Music folder but also if there's a
        /// sub-folder in the Music folder.
        /// </summary>
        /// <returns></returns>
        public List<MediaLocation> GetAll()
        {
            // Set media locations
            var mediaLocations = new List<MediaLocation>()
            {
                new MediaLocation() { Name = LocalizationResources.Instance["MediaSourceInternalStorageText"].ToString() + " (Test)",
                                MediaSourceType = MediaSourceTypes.Storage,
                                Sources = new() { "/storage/emulated/0/Download" },
                                MediaItemTypes = new() { MediaItemTypes.Music }
                                },      // TODO: Remove this                
                
                //new MediaLocation() { Name = LocalizationResources.Instance["MediaSourceInternalStorageText"].ToString(),
                //                MediaSourceType = MediaSourceTypes.Storage,
                //                Source = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, Android.OS.Environment.DirectoryMusic) },

                //new MediaLocation() { Name = LocalizationResources.Instance["MediaSourceAudiobooksText"].ToString(),
                //                MediaSourceType = MediaSourceTypes.Storage,
                //                Source = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, Android.OS.Environment.DirectoryAudiobooks) },

                //// Handle local Playlists folder if created
                //new MediaLocation() { Name = LocalizationResources.Instance["MediaSourcePlaylists"].ToString(),
                //                MediaSourceType = MediaSourceTypes.Storage,
                //                Source = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, "Playlists") },

                //new MediaLocation() { Name = LocalizationResources.Instance["MediaSourcePodcastsText"].ToString(),
                //                MediaSourceType = MediaSourceTypes.Storage,
                //                Source = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, Android.OS.Environment.DirectoryPodcasts) }                                                 
            };
            
            var sourceInfos = new List<Tuple<string, MediaSourceTypes, List<MediaItemTypes>, List<string>>>()
                        {
                            new Tuple<string, MediaSourceTypes, List<MediaItemTypes>, List<string>>("MediaSourceInternalStorageText", MediaSourceTypes.Storage,
                                            new() { MediaItemTypes.Music },
                                            new() {
                                                    Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, Android.OS.Environment.DirectoryMusic)
                                            }),

                            new Tuple<string, MediaSourceTypes, List<MediaItemTypes>, List<string>>("MediaSourceAudiobooksText", MediaSourceTypes.Storage,
                                            new() { MediaItemTypes.Audiobooks },
                                            new() {
                                                    Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, Android.OS.Environment.DirectoryAudiobooks),
                                                    Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, Android.OS.Environment.DirectoryMusic, Android.OS.Environment.DirectoryAudiobooks)
                                            }),
                                                    
                            new Tuple<string, MediaSourceTypes, List<MediaItemTypes>, List<string>>("MediaSourcePlaylistsText", MediaSourceTypes.Playlist,
                                            new() { MediaItemTypes.PlaylistMediaItems },
                                                new() {
                                                    Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, "Playlists"),
                                                    Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, Android.OS.Environment.DirectoryMusic, "Playlists"),
                                                    Path.Combine(FileSystem.AppDataDirectory, "Playlists")
                                                }),
                                                   
                            new Tuple<string, MediaSourceTypes, List<MediaItemTypes>, List<string>>("MediaSourcePodcasts", MediaSourceTypes.Storage,
                                            new() { MediaItemTypes.Podcasts },
                                            new() {
                                                    Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, Android.OS.Environment.DirectoryPodcasts),
                                                    Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, Android.OS.Environment.DirectoryMusic, Android.OS.Environment.DirectoryPodcasts)
                                            }),
                                                    
                            new Tuple<string, MediaSourceTypes, List<MediaItemTypes>, List<string>>("MediaSourceRadioStreamsText", MediaSourceTypes.RadioStreams,
                                            new() { MediaItemTypes.RadioStreams },
                                            new() {
                                                Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, ".RadioStreams"),     // Old name (TODO: Remove)
                                                Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, "RadioStreams"),
                                                Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, Android.OS.Environment.DirectoryMusic, "RadioStreams")
                                            }),
                                               
                };            
                       
            // Check for Music folder on external device (SD card) and if exists then check for other audio folders
            var drives = Environment.GetLogicalDrives();
            foreach(var drive in drives)
            {                                
                var musicFolder = Path.Combine(drive, Android.OS.Environment.DirectoryMusic);
                if (Directory.Exists(musicFolder))
                {
                    sourceInfos.Add(new Tuple<string, MediaSourceTypes, List<MediaItemTypes>, List<string>>("MediaSourceInternalStorageText", MediaSourceTypes.Storage,
                                    new() { MediaItemTypes.Music },
                                    new() {
                                            musicFolder
                                    }));
                    sourceInfos.Add(new Tuple<string, MediaSourceTypes, List<MediaItemTypes>, List<string>>("MediaSourceAudiobooksText", MediaSourceTypes.Storage,
                                    new() { MediaItemTypes.Audiobooks },    
                                    new() {
                                            Path.Combine(drive, Android.OS.Environment.DirectoryAudiobooks),
                                            Path.Combine(drive, Android.OS.Environment.DirectoryMusic, Android.OS.Environment.DirectoryAudiobooks)
                                    }));
                    sourceInfos.Add(new Tuple<string, MediaSourceTypes, List<MediaItemTypes>, List<string>>("MediaSourcePlaylistsText", MediaSourceTypes.Playlist,
                                    new() { MediaItemTypes.PlaylistMediaItems },
                                    new() {
                                             Path.Combine(drive, "Playlists"),
                                             Path.Combine(drive, Android.OS.Environment.DirectoryMusic, "Playlists")
                                    }));
                    sourceInfos.Add(new Tuple<string, MediaSourceTypes, List<MediaItemTypes>, List<string>>("MediaSourcePodcasts", MediaSourceTypes.Storage,
                                  new() { MediaItemTypes.Podcasts },
                                  new() {
                                            Path.Combine(drive, Android.OS.Environment.DirectoryPodcasts),
                                            Path.Combine(drive, Android.OS.Environment.DirectoryMusic, Android.OS.Environment.DirectoryPodcasts)
                                  }));
                    sourceInfos.Add(new Tuple<string, MediaSourceTypes, List<MediaItemTypes>, List<string>>("MediaSourceRadioStreamsText", MediaSourceTypes.RadioStreams,
                                  new() { MediaItemTypes.RadioStreams },
                                  new() {
                                            Path.Combine(drive, ".RadioStreams"),       // Old name (TODO: Remove)
                                            Path.Combine(drive, "RadioStreams"),
                                            Path.Combine(drive, Android.OS.Environment.DirectoryMusic, "RadioStreams")
                                  }));
                }
            }

            // Add all sources where one of the folders exists
            foreach (var sourceInfo in sourceInfos)
            {                
                var folders = new List<string>();
                foreach (var folder in sourceInfo.Item4)
                {
                    var isFolderExists = false;
                    try
                    {
                        isFolderExists = Directory.Exists(folder);
                    }
                    catch { };    // Ignore errors (Not much we can do about them)

                    if (isFolderExists)
                    {
                        folders.Add(folder);
                    }
                }

                if (folders.Any())
                {
                    mediaLocations.Add(new MediaLocation()
                    {
                        Name = LocalizationResources.Instance[sourceInfo.Item1].ToString(),
                        MediaSourceType = sourceInfo.Item2,
                        MediaItemTypes = sourceInfo.Item3,
                        Sources = sourceInfo.Item4
                    });
                }
            }

            //mediaLocations.Add(new MediaLocation()
            //{
            //    Name = LocalizationResources.Instance["MediaSourcePlaylistsText"].ToString(),
            //    MediaSourceType = MediaSourceTypes.Playlist,
            //    Sources = new() { FileSystem.AppDataDirectory }
            //});
     
            mediaLocations.Add(new MediaLocation()
            {
                Name = LocalizationResources.Instance["MediaSourceQueueText"].ToString(),
                MediaSourceType = MediaSourceTypes.Queue,
                Sources = new()
            });

            //// Add cloud providers
            //foreach (var cloudProvider in _cloudProviderService.GetAll())
            //{
            //    mediaLocations.Add(new MediaLocation()
            //    {
            //        Name = LocalizationResources.Instance[cloudProvider.NameResource].ToString(),
            //        MediaSourceType = MediaSourceTypes.Cloud,
            //        Source = cloudProvider.Url
            //    });
            //}

            return mediaLocations;
        }
    }
}
