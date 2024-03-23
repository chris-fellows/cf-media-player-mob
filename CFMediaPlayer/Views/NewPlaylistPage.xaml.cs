using CFMediaPlayer.ViewModels;

namespace CFMediaPlayer;

public partial class NewPlaylistPage : ContentPage
{
	private readonly NewPlaylistPageModel _model;

	public NewPlaylistPage(NewPlaylistPageModel model)
	{
		InitializeComponent();

        _model = model;
        this.BindingContext = _model;
    }

    private void SaveBtn_Clicked(object sender, EventArgs e)
    {

    }

    private void CancelBtn_Clicked(object sender, EventArgs e)
    {

    }
}