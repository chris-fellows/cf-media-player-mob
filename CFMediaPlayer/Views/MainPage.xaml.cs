using CFMediaPlayer.Enums;
using CFMediaPlayer.ViewModels;
using System;
using Java.Lang;
using Microsoft.Maui.Controls;
using CFMediaPlayer.Models;

namespace CFMediaPlayer
{
    /// <summary>
    /// Main page view
    /// </summary>
    //[QueryProperty(nameof(NewPlaylistName), "NewPlaylistName")]
    [QueryProperty(nameof(EventData), "EventData")]
    public partial class MainPage : ContentPage
    {
        int count = 0;       
        private MainPageModel _model;        

        public MainPage(MainPageModel model)
        {
            InitializeComponent();

            _model = model;            
            this.BindingContext = _model;          

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
        /// Event data property set by QueryProperty from external page
        /// </summary>
        public string EventData
        {
            set
            {                
                switch(value)
                {
                    case "PlaylistsUpdated":
                        _model.HandlePlaylistsUpdated();
                        break;
                    case "QueueUpdated":
                        _model.HandleQueueUpdated();
                        break;
                    case "UserSettingsUpdated":
                        _model.HandleUserSettingsUpdated();
                        break;
                }                
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
            System.Text.StringBuilder output = new System.Text.StringBuilder("");
            //var drives = DriveInfo.GetDrives();
            //foreach(var drive in drives)
            //{
            //    if (output.Length > 0) output.Append("; ");
            //    output.Append(drive.Name);
            //}

            //var item1 = Android.OS.Environment.DirectoryMusic;
            //var item2 = Android.OS.Environment.ExternalStorageDirectory.Path;
            //var item3 = Android.OS.Environment.GetExternalStoragePublicDirectory("#DIRECTORY_MUSIC");
            //var item4 = Android.OS.Environment.StorageDirectory;

            var variables = Environment.GetEnvironmentVariables();
            var value1 = Environment.GetEnvironmentVariable("EXTERNAL_STORAGE");
            var value2 = Environment.GetEnvironmentVariable("SECONDARY_STORAGE");
            
            var files = Directory.GetFiles("/storage/1B04-2D0C");
            var drives = Environment.GetLogicalDrives();
            var folders = Directory.GetDirectories("/external_SD");
            foreach(var folder in folders)
            {
                if (output.Length > 0) output.Append("; ");
                output.Append(folder);
            }

            DebugLabel.Text = output.ToString();

            //_model.ApplyEqualizerTest();

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

        //private void SearchResultTextCell_Tapped(object sender, EventArgs e)
        //{
        //    TextCell textCell = (TextCell)sender;
        //    _model.SelectSearchResult(textCell.Text);            
        //}

        private void MediaSearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (e.NewTextValue.Length == 0) _model.ClearSearchResults();
        }

        private void SearchResultsList_ItemTapped(object sender, ItemTappedEventArgs e)
        {            
            _model.SelectSearchResult((SearchResult)e.Item);
            MediaSearchBar.Text = "";
        }        
    }
}
