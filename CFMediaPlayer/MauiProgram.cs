using CFMediaPlayer.Enums;
using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;
using CFMediaPlayer.Services;
using CFMediaPlayer.Sources;
using CFMediaPlayer.ViewModels;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace CFMediaPlayer
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {           
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddTransient<IMediaPlayer, AndroidMediaPlayer>();           
            builder.Services.RegisterAllTypes<IPlaylist>(new[] { Assembly.GetExecutingAssembly() });

            /* Delete data files
         var dataFiles = Directory.GetFiles(FileSystem.AppDataDirectory, "*.xml");
         foreach(var file in dataFiles)
         {
             File.Delete(file);
         }
         */

            // Config services                       
            builder.Services.AddSingleton<IMediaLocationService, MediaLocationService>();
            builder.Services.AddSingleton<ICloudProviderService, CloudProviderService>();

            // Set IStreamSourceService, load sources if not set
            builder.Services.AddSingleton<IStreamSourceService>((scope) =>
            {
                var streamSourceService = new StreamSourceService(FileSystem.AppDataDirectory);
                var mediaItems = streamSourceService.GetAll();
                if (!mediaItems.Any())
                {
                    streamSourceService.LoadDefaults();
                    mediaItems = streamSourceService.GetAll();

                    //// Save to playlist format
                    //var folder = Path.Combine(FileSystem.AppDataDirectory, "RadioStreams");
                    //Directory.CreateDirectory(folder);
                    //var playlists = scope.GetServices<IPlaylist>();
                    //var playlist = playlists.FirstOrDefault(p => p.SupportsFile("Test.m3u"));
                    //playlist.SetFile(Path.Combine(folder, "Radio Streams 1.m3u"));
                    //playlist.SaveAll(mediaItems);
                    //playlist.SetFile("");
                }
                
                return streamSourceService;
            });

            // Set IAudioSettingsService, create if not present
            builder.Services.AddScoped<IAudioSettingsService>((scope) =>
            {
                var audioSettingsService = new AudioSettingsService(FileSystem.AppDataDirectory);

                // Create audio settings
                var audioSettings = audioSettingsService.GetAll();
                if (!audioSettings.Any())
                {
                    audioSettingsService.AddDefaults();
                }
                return audioSettingsService;
            });

            // Set IUserSettingsService, create user settings if not present for user
            builder.Services.AddScoped<IUserSettingsService>((scope) =>
            {                
                var userSettingsService = new UserSettingsService(FileSystem.AppDataDirectory);

                // Create user if not exists
                var userSettings = userSettingsService.GetByUsername(Environment.UserName);
                if (userSettings == null)
                {
                    var uiThemes = scope.GetRequiredService<IUIThemeService>().GetAll();
                    var systemSettings = scope.GetService<ISystemSettingsService>().GetAll().FirstOrDefault();                    

                    userSettings = new UserSettings()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Username = Environment.UserName,
                        AudioSettingsId = systemSettings.DefaultAudioSettingsId,
                        UIThemeId = systemSettings.DefaultUIThemeId,
                        CloudCredentialList = new List<CloudCredentials>()                        
                    };
                    userSettingsService.Update(userSettings);
                }
                return userSettingsService;
            });

            // Set ISystemSettingsService, create system settings if not present
            builder.Services.AddScoped<ISystemSettingsService>((scope) =>
            {
                var systemSettingsService = new SystemSettingsService(FileSystem.AppDataDirectory);

                // Create system settings if not exists
                var systemSettings = systemSettingsService.GetAll().FirstOrDefault();                
                if (systemSettings == null)
                {
                    var audioSettings = scope.GetService<IAudioSettingsService>().GetAll().First(s => s.Name.Equals("Normal"));
                    var uiTheme = scope.GetRequiredService<IUIThemeService>().GetAll().First();

                    systemSettings = new SystemSettings()
                    {                        
                        DefaultUIThemeId = uiTheme.Id,
                        DefaultAudioSettingsId = audioSettings.Id                        
                    };
                    systemSettingsService.Update(systemSettings);
                }
                return systemSettingsService;
            });

            builder.Services.AddSingleton<IUIThemeService, UIThemeService>();

            // Register IMediaSources to provide one IMediaSource per MediaLocation
            builder.Services.AddSingleton<IMediaSourceService>((scope) =>
            {
                List<IMediaSource> mediaSources = new List<IMediaSource>();
                var mediaLocationService = scope.GetRequiredService<IMediaLocationService>();                
                foreach(var mediaLocation in mediaLocationService.GetAll())
                {
                    switch (mediaLocation.MediaSourceType)
                    {
                        case MediaSourceTypes.Cloud:
                            mediaSources.Add(new CloudMediaSource(mediaLocation));
                            break;
                        case MediaSourceTypes.Playlist:
                            mediaSources.Add(new PlaylistMediaSource(mediaLocation, scope.GetServices<IPlaylist>()));
                            break;
                        case MediaSourceTypes.Queue:
                            mediaSources.Add(new QueueMediaSource(mediaLocation));
                            break;
                        case MediaSourceTypes.RadioStreams:                          
                            mediaSources.Add(new PlaylistMediaSource(mediaLocation, scope.GetServices<IPlaylist>()));
                            break;
                        case MediaSourceTypes.Storage:
                            mediaSources.Add(new StorageMediaSource(mediaLocation));
                            break;
                    }
                }
                mediaSources.ForEach(mediaSource => mediaSource.SetAllMediaSources(mediaSources));
                return new MediaSourceService(mediaSources);
            });            
            
            builder.Services.AddSingleton<IMediaSearchService, MediaSearchService>();

            // Register main page & model
            builder.Services.AddSingleton<MainPageModel>();
            builder.Services.AddSingleton<MainPage>();

            // Register other pages & models
            builder.Services.AddSingleton<ManagePlaylistsPageModel>();
            builder.Services.AddSingleton<ManagePlaylistsPage>();
            builder.Services.AddSingleton<ManageQueuePageModel>();
            builder.Services.AddSingleton<ManageQueuePage>();         
            builder.Services.AddSingleton<UserSettingsPageModel>();
            builder.Services.AddSingleton<UserSettingsPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        /// <summary>
        /// Registers all types implementing interface
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <param name="assemblies"></param>
        /// <param name="lifetime"></param>
        private static void RegisterAllTypes<T>(this IServiceCollection services, IEnumerable<Assembly> assemblies, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            var typesFromAssemblies = assemblies.SelectMany(a => a.DefinedTypes.Where(x => x.GetInterfaces().Contains(typeof(T))));
            foreach (var type in typesFromAssemblies)
            {
                services.Add(new ServiceDescriptor(typeof(T), type, lifetime));
            }
        }
    }
}
