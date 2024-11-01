using CFMediaPlayer.Constants;
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

            builder.Services.AddSingleton<IMediaPlayer, AndroidMediaPlayer>();      // Was AddTransient
            builder.Services.AddSingleton<IAudioEqualizer, AndroidAudioEqualizer>();             
            builder.Services.RegisterAllTypes<IPlaylistManager>(new[] { Assembly.GetExecutingAssembly() });

            //// Enable this to reset all data files
            //// Delete data files
            //var dataFiles = Directory.GetFiles(FileSystem.AppDataDirectory, "*.xml");
            //foreach (var file in dataFiles)
            //{
            //    File.Delete(file);
            //}

            // Create folders
            var musicFolder = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, Android.OS.Environment.DirectoryMusic);
            if (Directory.Exists(musicFolder))
            {
                Directory.CreateDirectory(Path.Combine(musicFolder, GeneralConstants.PlaylistsFolderName));
                Directory.CreateDirectory(Path.Combine(musicFolder, GeneralConstants.RadioStreamsFolderName));
            }

            // Config services            
            builder.Services.AddSingleton<IMediaLocationService, MediaLocationService>();
            builder.Services.AddSingleton<ICloudProviderService, CloudProviderService>();
            builder.Services.AddSingleton<ICurrentState, CurrentState>();
            //builder.Services.AddSingleton<IEvents, EventsObject>();

            //builder.Services.AddSingleton<IIndexedData, XmlIndexedData>();

            // Set IAudioSettingsService, create if not present
            builder.Services.AddScoped<IAudioSettingsService>((scope) =>
            {                                
                var audioSettingsService = new AudioSettingsService(scope.GetRequiredService<IAudioEqualizer>(), FileSystem.AppDataDirectory);

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
                        CultureName = "en",
                        AudioSettingsId = systemSettings.DefaultAudioSettingsId,               
                        UIThemeId = systemSettings.DefaultUIThemeId,
                        CloudCredentialList = new List<CloudCredentials>(),
                        CustomAudioSettingsList = Enumerable.Range(0, GeneralConstants.NumberOfCustomAudioSettings).Select(index =>
                        {
                            return new CustomAudioSettings()
                            {
                                Id = Guid.NewGuid().ToString(),
                                Name = $"{GeneralConstants.CustomPresetName} {index + 1}",
                                AudioBands = systemSettings.DefaultCustomAudioBands
                            };
                        }).ToList()
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
                    var defaultAudioSettings = scope.GetService<IAudioSettingsService>().GetAll().First(s => s.Name.Equals(GeneralConstants.NormalAudioPresetName));
                    var uiTheme = scope.GetRequiredService<IUIThemeService>().GetAll().First();

                    systemSettings = new SystemSettings()
                    {                        
                        Id = Guid.NewGuid().ToString(),                        
                        DefaultUIThemeId = uiTheme.Id,                                             
                        DefaultCustomAudioBands = new List<short>() { 500, 500, 500, 500, 500 },   // TODO: Remove this hard-coding
                        DefaultAudioSettingsId = defaultAudioSettings.Id                      
                    };
                    systemSettingsService.Update(systemSettings);
                }
                return systemSettingsService;
            });

            builder.Services.AddSingleton<IUIThemeService, UIThemeService>();

            // Register IMediaSources to provide one IMediaSource per MediaLocation
            builder.Services.AddSingleton<IMediaSourceService>((scope) =>
            {                          
                List <IMediaSource> mediaSources = new List<IMediaSource>();
                var currentState = scope.GetRequiredService<ICurrentState>();
                var mediaLocationService = scope.GetRequiredService<IMediaLocationService>();                
                foreach(var mediaLocation in mediaLocationService.GetAll())
                {
                    switch (mediaLocation.MediaSourceType)
                    {
                        case MediaSourceTypes.Cloud:
                            mediaSources.Add(new CloudMediaSource(currentState,mediaLocation));
                            break;
                        case MediaSourceTypes.Playlist:
                            mediaSources.Add(new PlaylistMediaSource(currentState,mediaLocation, scope.GetServices<IPlaylistManager>()));
                            break;
                        case MediaSourceTypes.Queue:
                            mediaSources.Add(new QueueMediaSource(currentState, mediaLocation));
                            break;
                        case MediaSourceTypes.RadioStreams:                          
                            mediaSources.Add(new PlaylistMediaSource(currentState,mediaLocation, scope.GetServices<IPlaylistManager>()));
                            break;
                        case MediaSourceTypes.Storage:
                            mediaSources.Add(new StorageMediaSource(currentState, mediaLocation));
                            break;
                    }
                }
                mediaSources.ForEach(mediaSource => mediaSource.SetAllMediaSources(mediaSources));
                return new MediaSourceService(mediaSources);
            });            
            
            builder.Services.AddSingleton<IMediaSearchService, MediaSearchService>();            

            // Register main page & model
            builder.Services.AddSingleton<CurrentPage>();
            builder.Services.AddSingleton<CurrentPageModel>();
            builder.Services.AddSingleton<LibraryPage>();
            builder.Services.AddSingleton<LibraryPageModel>();
            builder.Services.AddSingleton<MainPageModel>();
            builder.Services.AddSingleton<MainPage>();
            //builder.Services.AddSingleton<TestFlyoutPage>();

            // Register other pages & models
            builder.Services.AddSingleton<ManagePlaylistsPageModel>();
            builder.Services.AddSingleton<ManagePlaylistsPage>();
            builder.Services.AddSingleton<ManageQueuePageModel>();
            builder.Services.AddSingleton<ManageQueuePage>();
            builder.Services.AddSingleton<TestPage>();
            builder.Services.AddSingleton<TestPageModel>();
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
