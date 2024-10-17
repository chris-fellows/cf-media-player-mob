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
	}
}