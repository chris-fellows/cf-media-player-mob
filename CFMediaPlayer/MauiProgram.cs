using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Services;
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
            builder.Services.RegisterAllTypes<IMediaSource>(new[] { Assembly.GetExecutingAssembly() });
            builder.Services.RegisterAllTypes<IPlaylist>(new[] { Assembly.GetExecutingAssembly() });

            // Config services           
            builder.Services.AddScoped<IUserSettingsService>((scope) =>
            {
                return new UserSettingsService(FileSystem.AppDataDirectory);
            });

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
