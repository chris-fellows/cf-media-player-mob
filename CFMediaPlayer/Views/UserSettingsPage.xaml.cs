using CFMediaPlayer.ViewModels;

namespace CFMediaPlayer;

public partial class UserSettingsPage : ContentPage
{
	private readonly UserSettingsPageModel _model;

	public UserSettingsPage(UserSettingsPageModel model)
	{
        InitializeComponent();

        _model = model;
		this.BindingContext = _model;
	}

    private void SliderBand0_DragCompleted(object sender, EventArgs e)
    {
        _model.EqualizerBandLevel0 = (short)this.SliderBand0.Value;
    }

    private void SliderBand1_DragCompleted(object sender, EventArgs e)
    {
        _model.EqualizerBandLevel1 = (short)this.SliderBand1.Value;
    }

    private void SliderBand2_DragCompleted(object sender, EventArgs e)
    {
        _model.EqualizerBandLevel2 = (short)this.SliderBand2.Value;
    }

    private void SliderBand3_DragCompleted(object sender, EventArgs e)
    {
        _model.EqualizerBandLevel3 = (short)this.SliderBand3.Value;
    }

    private void SliderBand4_DragCompleted(object sender, EventArgs e)
    {
        _model.EqualizerBandLevel4 = (short)this.SliderBand4.Value;
    }
}