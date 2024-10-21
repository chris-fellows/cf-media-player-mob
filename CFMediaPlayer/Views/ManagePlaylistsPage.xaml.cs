using CFMediaPlayer.ViewModels;

namespace CFMediaPlayer;

public partial class ManagePlaylistsPage : ContentPage
{    
    private ManagePlaylistsPageModel _model;

    public ManagePlaylistsPage(ManagePlaylistsPageModel model)
	{
		InitializeComponent();

		_model = model;
		this.BindingContext = _model;

        // Set event handler for media start error
        _model.OnError += (exception) =>
        {
            var alertResult = DisplayAlert(LocalizationResources.Instance["Error"].ToString(), exception.Message,
                LocalizationResources.Instance["Close"].ToString());
        };
    }
}