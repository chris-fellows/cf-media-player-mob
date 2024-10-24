using CFMediaPlayer.Enums;
using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;
using CFMediaPlayer.ViewModels;

namespace CFMediaPlayer;

public partial class LibraryPage : ContentPage
{
    private LibraryPageModel _model;

    public LibraryPage(LibraryPageModel model)
    {
        InitializeComponent();

        _model = model;
        this.BindingContext = _model;

        // Set default media location
        _model.SelectedMediaLocation = _model.MediaLocations.FirstOrDefault(ml => ml.MediaSourceType == MediaSourceTypes.Storage &&
                                                    ml.MediaItemTypes.Contains(MediaItemTypes.Music));
        if (_model.SelectedMediaLocation == null)
        {
            _model.SelectedMediaLocation = _model.MediaLocations.First();
        }
    }

    private void MediaSearchBar_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (e.NewTextValue.Length == 0) _model.ClearSearchResults();
    }

    private void SearchResultsList_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        _model.SelectSearchResult((SearchResult)e.Item);
        MediaSearchBar.Text = "";    
    }

    private void MediaActionsView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _model.ExecuteMediaAction((MediaAction)e.CurrentSelection.First());
    }

    private void OnDebugInfoClicked(object sender, EventArgs e)
    {        
        DebugLabel.Text = _model.GetDebugInfo();       
    }
}