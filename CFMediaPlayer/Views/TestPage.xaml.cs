using CFMediaPlayer.Interfaces;
using CFMediaPlayer.ViewModels;

namespace CFMediaPlayer;

public partial class TestPage : TabbedPage
{	
	public TestPage()
	{
		InitializeComponent();

        // Register page routes
        Routing.RegisterRoute(nameof(CurrentPage), typeof(CurrentPage));
        Routing.RegisterRoute(nameof(LibraryPage), typeof(LibraryPage));
        Routing.RegisterRoute(nameof(TestPage), typeof(TestPage));
        Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
        Routing.RegisterRoute(nameof(ManagePlaylistsPage), typeof(ManagePlaylistsPage));
        Routing.RegisterRoute(nameof(ManageQueuePage), typeof(ManageQueuePage));
        Routing.RegisterRoute(nameof(UserSettingsPage), typeof(UserSettingsPage));

        // Set child pages
        // Need to this to use DI for page models
        var services = MauiApplication.Current.Services;               
        this.Children.Add(services.GetService<LibraryPage>());
        this.Children.Add(services.GetService<CurrentPage>());        
        this.Children.Add(services.GetService<ManagePlaylistsPage>());
        //this.Children.Add(services.GetService<ManageQueuePage>());
        this.Children.Add(services.GetService<UserSettingsPage>());

        // Set action to select tab
        var currentState = services.GetService<ICurrentState>();
        currentState.SelectTabByTitleAction = (title) =>
        {
            this.CurrentPage = this.Children.First(c => c.Title.Equals(title));
        };
    }
}