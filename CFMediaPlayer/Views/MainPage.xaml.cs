using CFMediaPlayer.Enums;
using CFMediaPlayer.ViewModels;

namespace CFMediaPlayer
{
    /// <summary>
    /// Main page view
    /// </summary>
    [QueryProperty(nameof(NewPlaylistName), "NewPlaylistName")]
    [QueryProperty(nameof(UserSettingsUpdated), "UserSettingsUpdated")]
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

        public string UserSettingsUpdated
        {
            set
            {
                _model.RefreshUserSettings();
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
            _model.ApplyEqualizerTest();

            //StatusLabel.Text = $"MediaItemActions=" + _model.MediaItemActions.Count;            

            //    if (_model.MediaItemCollections == null)
            //    {
            //        StatusLabel.Text = $"Collections=null";
            //    }
            //    else
            //    {
            //        StatusLabel.Text = $"Collections=" + _model.MediaItemCollections.Count;
            //    }
        }

        private void ElapsedSlider_DragCompleted(object sender, EventArgs e)
        {
            // Advance to particular time player media item. We can't set the slider to be TwoWay because that causes
            // let ElapsedTimeInt to be set whenever the elapsed time is updated and that causes jumpy playback
            _model.ElapsedTimeInt = (int)ElapsedSlider.Value;            
        }

        private void SearchResultTextCell_Tapped(object sender, EventArgs e)
        {
            TextCell textCell = (TextCell)sender;
            _model.SelectSearchResult(textCell.Text);            
        }
    }
}
