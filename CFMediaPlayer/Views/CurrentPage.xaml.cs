using CFMediaPlayer.Models;
using CFMediaPlayer.ViewModels;

namespace CFMediaPlayer;

public partial class CurrentPage : ContentPage
{
	private CurrentPageModel _model;

	public CurrentPage(CurrentPageModel model)
	{
		InitializeComponent();

		_model = model;
        this.BindingContext = _model;

        // Set event handler for debug action
        _model.OnDebugAction += (debug) =>
        {
            DebugLabel.Text = debug;
        };

        // Set event handler for media start error
        _model.OnMediaPlayerError += (mediaPlayerException) =>
        {
            var alertResult = DisplayAlert(LocalizationResources.Instance["Error"].ToString(), mediaPlayerException.Message,
                LocalizationResources.Instance["Close"].ToString());
        };

        // Set action to take when elapsed time changes. Binding Slider.Value to model doesn't update the
        // slider position in the UI although the ValueChanged event indicates that the value is updating.
        _model.SetElapsedAction((elapsed) =>
        {
            ElapsedSlider.Value = elapsed;
        });
    }

    private void OnDebugInfoClicked(object sender, EventArgs e)
    {

    }

    private void ElapsedSlider_DragCompleted(object sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"ElapsedSlider_DragCompleted");

        // Advance to particular time player media item. We can't set the slider value to be TwoWay because 
        // that causes ElapsedMS to be set whenever the elapsed time is updated and that causes jumpy playback.
        //_model.ElapsedMS = (int)ElapsedSlider.Value;        
        _model.SetElapsedMS(ElapsedSlider.Value);
    }
  
    private void MediaActionsView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _model.ExecuteMediaAction((MediaAction)e.CurrentSelection.First());
    }

    //private void ElapsedSlider_ValueChanged(object sender, ValueChangedEventArgs e)
    //{
    //    System.Diagnostics.Debug.WriteLine($"ElapsedSlider_ValueChanged {e.NewValue}");
    //}
}