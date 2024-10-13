namespace CFMediaPlayer
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register page routes
            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
            Routing.RegisterRoute(nameof(NewPlaylistPage), typeof(NewPlaylistPage));
            Routing.RegisterRoute(nameof(UserSettingsPage), typeof(UserSettingsPage));
        }        
    }
}
