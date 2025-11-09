using CFMediaPlayer.Views;

namespace CFMediaPlayer
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register page routes
            Routing.RegisterRoute(nameof(TestPage), typeof(TestPage));
            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
            Routing.RegisterRoute(nameof(ManagePlaylistsPage), typeof(ManagePlaylistsPage));            
            Routing.RegisterRoute(nameof(UserSettingsPage), typeof(UserSettingsPage));
            Routing.RegisterRoute(nameof(SearchPage), typeof(SearchPage));
        }        
    }
}
