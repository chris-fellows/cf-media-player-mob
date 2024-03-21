using CFMediaPlayer.Constants;
using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;
using System.IO;
using System.Text;
using System.Timers;

namespace CFMediaPlayer
{
    public partial class MainPage : ContentPage
    {
        int count = 0;       
        private MainPageModel _model;
        //private string _rootPath;
        //private System.Timers.Timer _elapsedTimer;

        public MainPage(MainPageModel model)
        {
            InitializeComponent();

            _model = model;
            //this.BindingContext = this; // CMF Added            
            this.BindingContext = _model;

            //MusicSource.SelectedIndexChanged += MusicSource_SelectedIndexChanged;
            //ArtistList.SelectedIndexChanged += ArtistList_SelectedIndexChanged;
            //AlbumList.SelectedIndexChanged += AlbumList_SelectedIndexChanged;
            //MediaItemList.SelectedIndexChanged += MediaItemList_SelectedIndexChanged;

            //ElapsedSlider.Minimum = 0;                   

            // Default to auto-play next media item on completion
            _model.AutoPlayNext = true;

            //_model.MediaPlayer.SetDebugAction((status) =>
            //{
            //    StatusLabel.Text = status;
            //});

            //_model.MediaPlayer.SetStatusAction(OnMediaItemStatusChange);

            // Handle media item status
            _model.SetMediaItemStatusAction((status) =>
            {
                //switch (status)
                //{
                //    case"Completed":
                //        // Play next item
                //        if (MediaItemList.SelectedIndex != MediaItemList.Items.Count - 1)
                //        {
                //            PlayMediaItem(MediaItemList.SelectedIndex + 1);
                //        }
                //        break;
                //}
            });

            // Handle debug messages
            _model.SetDebugAction((debug) =>
            {
                DebugLabel.Text = debug;
            });
            
            // Set default media location to internal storage
            _model.SelectedMediaLocation = _model.MediaLocations.First(ml => ml.MediaSourceName.Equals(MediaSourceNames.Storage));
            _model.OnPropertyChanged("SelectedMediaLocation");
        }        

        //private void MediaItemList_SelectedIndexChanged(object? sender, EventArgs e)
        //{
        //    System.Diagnostics.Debug.WriteLine($"{DateTimeOffset.UtcNow.ToString()} MediaItemList_SelectedIndexChanged {MediaItemList.SelectedIndex}");

        //    // Stop media if playing
        //    if (_model.IsPlaying)
        //    {
        //        _model.Stop();
        //    }
        //}

        private void OnElapsedSliderValueChanged(object? sender, ValueChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"{DateTimeOffset.UtcNow.ToString()} OnElapsedSliderValueChanged Old={e.OldValue}, New={e.NewValue}");
        }

     
        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }
      
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

            //StringBuilder debug = new StringBuilder("");

            //debug.Append($"IsPlaying={IsPlaying}, StopEnabled={StopMediaBtn.IsEnabled}");

            //StatusLabel.Text = debug.ToString();                        
        }      
    }

}
