using CFMediaPlayer.Enums;
using CFMediaPlayer.Models;
using CFMediaPlayer.Utilities;
using CFMediaPlayer.ViewModels;

namespace CFMediaPlayer.Views;

public partial class SearchPage : ContentPage
{
	public SearchPage()
	{
		InitializeComponent();
	}

    private SearchPageModel _model;

    public SearchPage(SearchPageModel model)
    {
        InternalUtilities.Log("Entered SearchPage constructor");

        InitializeComponent();

        _model = model;
        this.BindingContext = _model;
     
        // Set general error handler
        _model.OnGeneralError += (exception) =>
        {
            var alertResult = DisplayAlert(LocalizationResources.Instance["Error"].ToString(), exception.Message,
                LocalizationResources.Instance["Close"].ToString());
        };
      
        InternalUtilities.Log("Leaving SearchPage constructor");
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
}