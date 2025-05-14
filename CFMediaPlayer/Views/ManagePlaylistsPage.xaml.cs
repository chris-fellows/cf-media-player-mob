using CFMediaPlayer.Utilities;
using CFMediaPlayer.ViewModels;

namespace CFMediaPlayer;

/// <summary>
/// Manage playlists page. Create playlist, delete playlist etc.
/// </summary>
public partial class ManagePlaylistsPage : ContentPage
{    
    private ManagePlaylistsPageModel _model;

    public ManagePlaylistsPage(ManagePlaylistsPageModel model)
	{
        InternalUtilities.Log("Entered MainPlaylistsPage constructor");

        InitializeComponent();

		_model = model;
		this.BindingContext = _model;

        // Set event handler for debug action
        _model.OnDebugAction += (debug) =>
        {
            System.Diagnostics.Debug.WriteLine(debug);
        };

        // Set general error handler
        _model.OnGeneralError += (exception) =>
        {
            var alertResult = DisplayAlert(LocalizationResources.Instance["Error"].ToString(), exception.Message,
                LocalizationResources.Instance["Close"].ToString());
        };

        InternalUtilities.Log("Leaving MainPlaylistsPage constructor");
    }
}