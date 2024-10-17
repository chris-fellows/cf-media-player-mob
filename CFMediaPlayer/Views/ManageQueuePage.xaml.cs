using CFMediaPlayer.ViewModels;

namespace CFMediaPlayer;

public partial class ManageQueuePage : ContentPage
{
    private ManageQueuePageModel _model;

    public ManageQueuePage(ManageQueuePageModel model)
    {
		InitializeComponent();

        _model = model;
        this.BindingContext = _model;
    }
}