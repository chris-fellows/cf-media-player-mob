using CFMediaPlayer.Utilities;
using CFMediaPlayer.ViewModels;

namespace CFMediaPlayer;

/// <summary>
/// User settings page. Select language, change audio settings etc.
/// </summary>
public partial class UserSettingsPage : ContentPage
{
	private readonly UserSettingsPageModel _model;

	public UserSettingsPage(UserSettingsPageModel model)
	{
        InternalUtilities.Log("Entered UserSettingsPage constructor");

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

        InternalUtilities.Log("Leaving UserSettingsPage constructor");
    }
}