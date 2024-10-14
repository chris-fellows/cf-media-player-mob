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
            //builder.Services.RegisterAllTypes<IMediaSource>(new[] { Assembly.GetExecutingAssembly() });   // We want one IMediaSource per MediaLocation
            builder.Services.RegisterAllTypes<IPlaylist>(new[] { Assembly.GetExecutingAssembly() });

            // Config services                       
            builder.Services.AddSingleton<IMediaLocationService, MediaLocationService>();
            builder.Services.AddSingleton<ICloudProviderService, CloudProviderService>();
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
                        UIThemeId = systemSettings.DefaultUIThemeId,
                        CloudCredentialList = new List<CloudCredentials>(),
                        PlayMode = systemSettings.DefaultPlayMode
                    };
                    userSettingsService.Update(userSettings);
                }
                return userSettingsService;
            });
            builder.Services.AddScoped<ISystemSettingsService>((scope) =>
            {
                var systemSettingsService = new SystemSettingsService(FileSystem.AppDataDirectory);

                // Create system settings if not exists
                var systemSettings = systemSettingsService.GetAll().FirstOrDefault();
                if (systemSettings == null)
                {
                    var uiThemes = scope.GetRequiredService<IUIThemeService>().GetAll();
                    systemSettings = new SystemSettings()
                    {                        
                        DefaultUIThemeId = uiThemes.First().Id,
                        DefaultPlayMode = MediaPlayModes.Sequential
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
                        case Enums.MediaSourceTypes.Cloud:
                            mediaSources.Add(new CloudMediaSource(mediaLocation));
                            break;
                        case Enums.MediaSourceTypes.Playlist:
                            mediaSources.Add(new PlaylistMediaSource(mediaLocation, scope.GetServices<IPlaylist>()));
                            break;
                        case Enums.MediaSourceTypes.Queue:
                            mediaSources.Add(new QueueMediaSource(mediaLocation));
                            break;
                        case Enums.MediaSourceTypes.Storage:
                            mediaSources.Add(new StorageMediaSource(mediaLocation));
                            break;
                    }
                }
                return new MediaSourceService(mediaSources);
            });

            builder.Services.AddSingleton<IMediaSearchService, MediaSearchService>();

            // Register main page & model
            builder.Services.AddSingleton<MainPageModel>();
            builder.Services.AddSingleton<MainPage>();

            // Register other pages & models
            builder.Services.AddSingleton<NewPlaylistPageModel>();
            builder.Services.AddSingleton<NewPlaylistPage>();            
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
