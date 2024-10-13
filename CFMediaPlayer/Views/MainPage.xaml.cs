using CFMediaPlayer.Enums;
using CFMediaPlayer.ViewModels;

namespace CFMediaPlayer
{
    /// <summary>
    /// Main page view
    /// </summary>
    [QueryProperty(nameof(NewPlaylistName), "NewPlaylistName")] 
    public partial class MainPage : ContentPage
    {
        int count = 0;       
        private MainPageModel _model;        

        public MainPage(MainPageModel model)
        {
            InitializeComponent();

            _model = model;            
            this.BindingContext = _model;
            
            // Default to auto-play next media item on completion
            //_model.AutoPlayNext = true;       

            // Handle debug messages
            _model.SetDebugAction((debug) =>
            {
                DebugLabel.Text = debug;
            });
            
            // Set default media location to internal storage
            _model.SelectedMediaLocation = _model.MediaLocations.First(ml => ml.MediaSourceType == MediaSourceTypes.Storage);
            _model.OnPropertyChanged(nameof(_model.SelectedMediaLocation));

            var xxx = 1000;
        }        

        /// <summary>
        /// Handle new playlist created. Automatically select it
        /// </summary>
        public string NewPlaylistName
        {
            set
            {
                _model.SelectPlaylist(value);                
            }
        }
   
        private void OnElapsedSliderValueChanged(object? sender, ValueChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"{DateTimeOffset.UtcNow.ToString()} OnElapsedSliderValueChanged Old={e.OldValue}, New={e.NewValue}");
        }
     
        //private void OnCounterClicked(object sender, EventArgs e)
        //{            
        //    count++;

        //    if (count == 1)
        //        CounterBtn.Text = $"Clicked {count} time";
        //    else
        //        CounterBtn.Text = $"Clicked {count} times";

        //    SemanticScreenReader.Announce(CounterBtn.Text);
        //}
      
        private void OnDebugInfoClicked(object sender, EventArgs e)
        {
            if (_model.MediaItemCollections == null)
            {
                StatusLabel.Text = $"Collections=null";
            }
            else
            {
                StatusLabel.Text = $"Collections=" + _model.MediaItemCollections.Count;
            }
        }

        private void ElapsedSlider_DragCompleted(object sender, EventArgs e)
        {
            // Advance to particular time player media item. We can't set the slider to be TwoWay because that causes
            // let ElapsedTimeInt to be set whenever the elapsed time is updated
            _model.ElapsedTimeInt = (int)ElapsedSlider.Value;            
        }
    }
}
