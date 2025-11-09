using CFMediaPlayer.Enums;
using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;
using CFMediaPlayer.Utilities;
using CFMediaPlayer.ViewModels;

namespace CFMediaPlayer;

/// <summary>
/// Library page. Allows user to navigate and search music library. User can start playing a media item
/// which becomes current media item (Current page)
/// </summary>
public partial class LibraryPage : ContentPage
{
    private LibraryPageModel _model;
    
    public LibraryPage(LibraryPageModel model)
    {
        InternalUtilities.Log("Entered LibraryPage constructor");

        InitializeComponent();
        
        _model = model;
        this.BindingContext = _model;

        // Set event handler for debug action
        _model.OnDebugAction += (debug) =>
        {
            System.Diagnostics.Debug.WriteLine(debug);
            DebugLabel.Text = debug;
        };

        // Set general error handler
        _model.OnGeneralError += (exception) =>
        {
            var alertResult = DisplayAlert(LocalizationResources.Instance["Error"].ToString(), exception.Message,
                LocalizationResources.Instance["Close"].ToString());
        };

        // Set default media location
        _model.SelectedMediaLocation = _model.MediaLocations.FirstOrDefault(ml => ml.MediaSourceType == MediaSourceTypes.Storage &&
                                                    ml.MediaItemTypes.Contains(MediaItemTypes.Music));
        if (_model.SelectedMediaLocation == null)
        {
            _model.SelectedMediaLocation = _model.MediaLocations.First();
        }

        //this.MediaLocationPicker.SelectedIndex = _model.MediaLocations.IndexOf(_model.SelectedMediaLocation);        

        InternalUtilities.Log("Leaving LibraryPage constructor");
    }

    //private void MediaSearchBar_TextChanged(object sender, TextChangedEventArgs e)
    //{
    //    if (e.NewTextValue.Length == 0) _model.ClearSearchResults();
    //}

    //private void SearchResultsList_ItemTapped(object sender, ItemTappedEventArgs e)
    //{
    //    _model.SelectSearchResult((SearchResult)e.Item);
    //    MediaSearchBar.Text = "";    
    //}

    private void MediaActionsView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _model.ExecuteMediaAction((MediaAction)e.CurrentSelection.First());
    }

    private void OnDebugInfoClicked(object sender, EventArgs e)
    {        
        DebugLabel.Text = _model.GetDebugInfo();       
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {

    }

    private void MenuFlyoutItem_Clicked(object sender, EventArgs e)
    {
        int xxx = 1000;
    }

    private void PlayToggleImageButton_Clicked(object sender, EventArgs e)
    {
        ImageButton imageButton = (ImageButton)sender;

       
        int xxx = 1000;
    }

    //private void MediaLocation_SelectedIndexChanged(object sender, EventArgs e)
    //{
    //    Picker picker = (Picker)sender;
    //    var index = picker.SelectedIndex;
    //    if (index == -1)
    //    {
    //        int xxxxx = 1000;
    //        var item = picker.SelectedItem;
    //        _model.SelectedMediaLocation = null;
    //    }
    //    else
    //    {
    //        var mediaLocation = (MediaLocation)picker.SelectedItem;
    //        _model.SelectedMediaLocationAsync = mediaLocation;
    //        int xxxx = 1000;
    //    }        
    //    var xxx = 1000;
    //}

    //private void ImageButton_Clicked(object sender, EventArgs e)
    //{
    //    int xxx = 1000;
    //}
}