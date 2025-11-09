using CFMediaPlayer.Constants;
using CFMediaPlayer.Enums;
using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;
using CFMediaPlayer.Services;
using CFMediaPlayer.Utilities;
using IntelliJ.Lang.Annotations;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using static Android.Media.Browse.MediaBrowser;
using static Android.Provider.MediaStore.Audio;

namespace CFMediaPlayer.ViewModels
{
    public class SearchPageModel : PageModelBase, INotifyPropertyChanged
    {
        private readonly IAudioSettingsService _audioSettingsService;
        private readonly ICurrentState _currentState;
        private readonly ILogWriter _logWriter;
        private readonly IMediaLocationService _mediaLocationService;
        private readonly IMediaSearchService _mediaSearchService;
        private readonly IMediaSourceService _mediaSourceService;
        private readonly IUIThemeService _uiThemeService;
        private readonly IUserSettingsService _userSettingsService;

        private MediaLocation _selectedMediaLocation = new();

        private List<MediaLocation> _mediaLocations = new List<MediaLocation>();

        private bool _isSearchBusy;

        public SearchPageModel(IAudioSettingsService audioSettingsService,
                              ICurrentState currentState,
                              ILogWriter logWriter,
                              IMediaLocationService mediaLocationService,
                              IMediaSearchService mediaSearchService,
                              IMediaSourceService mediaSourceService,
                              IUIThemeService uiThemeService,
                              IUserSettingsService userSettingsService)
        {
            _audioSettingsService = audioSettingsService;
            _currentState = currentState;
            _logWriter = logWriter;
            _mediaLocationService = mediaLocationService;
            _mediaSearchService = mediaSearchService;
            _mediaSourceService = mediaSourceService;
            _uiThemeService = uiThemeService;
            _userSettingsService = userSettingsService;

            LoadMediaLocationsToDisplayInUI();

            SelectedMediaLocation = MediaLocations.First();
        }

        /// <summary>
        /// Loads media locations to display. Some sources are always displayed even if no media items. Others
        /// are only displayed if media items (E.g. Audiobooks, podcasts etc)
        /// </summary>
        private void LoadMediaLocationsToDisplayInUI()
        {
            var mediaLocations = new List<MediaLocation>();
            var mediaSources = _mediaSourceService.GetAll();
            foreach (var mediaLocation in _mediaLocationService.GetAll())
            {
                var mediaSource = mediaSources.First(ms => ms.MediaLocation.Name == mediaLocation.Name);
                if (mediaSource.IsDisplayInUI)
                {
                    mediaLocations.Add(mediaLocation);
                }
            }
            mediaLocations = mediaLocations.OrderBy(ml => ml.Name).ToList();

            //if (_currentState.SetNoMediaLocationAction != null)
            //{
            //    _currentState.SetNoMediaLocationAction();
            //}

            // Clearing SelectedMediaLocation doesn't stop it trying to set default
            //this.SelectedMediaLocationAsync = null;

            // Setting this triggers SelectedMediaLocationSync set when we're calling Reset().
            // So when you notify that a list property is updated then the UI selects the first one.
            MediaLocations = mediaLocations;            
        }

        /// <summary>
        /// Media locations
        /// </summary>
        public List<MediaLocation> MediaLocations
        {
            get { return _mediaLocations; }
            set
            {
                _mediaLocations = value;

                OnPropertyChanged(nameof(MediaLocations));
            }
        }

        public MediaLocation SelectedMediaLocation
        {
            get
            {
                return _selectedMediaLocation;
            }
            set
            {
                _selectedMediaLocation = value;
            }
        }

        /// <summary>
        /// Whether search is busy
        /// </summary>
        public bool IsSearchBusy
        {
            get { return _isSearchBusy; }
            set
            {
                _isSearchBusy = value;

                OnPropertyChanged(nameof(IsSearchBusy));
                //OnPropertyChanged(nameof(IsBusy));
            }
        }

        /// <summary>
        /// Command to start search. Runs asynchronously.
        /// </summary>
        /// <remarks>We could search all media locations but that could be slow</remarks>        
        public ICommand StartSearchCommand => new Command<string>((string text) =>
        {
            var task = Task.Factory.StartNew(() =>
            {
                IsSearchBusy = true;
                System.Diagnostics.Debug.WriteLine($"Search for {text}");

                // Clear results
                SearchResults = new List<SearchResult>();

                //Thread.Sleep(5000); // Simulate delay

                var cancellationTokenSource = new CancellationTokenSource();

                // Get results
                var searchOptions = new SearchOptions() { Text = text, MediaLocations = new() { _selectedMediaLocation! } };
                var results = _mediaSearchService.SearchAsync(searchOptions, cancellationTokenSource.Token).Result;

                // Set results
                SearchResults = results;
                
                IsSearchBusy = false;
            });
        });

        public bool IsSearchResults => _searchResults.Any();

        /// <summary>
        /// Search results
        /// </summary>
        private List<SearchResult> _searchResults = new List<SearchResult>();
        public List<SearchResult> SearchResults
        {
            get
            {
                return _searchResults;
            }
            set
            {
                _searchResults = value;

                OnPropertyChanged(nameof(SearchResults));
                OnPropertyChanged(nameof(IsSearchResults));
            }
        }


        /// <summary>
        /// Clear search results
        /// </summary>
        public void ClearSearchResults()
        {
            SearchResults = new List<SearchResult>();
        }

        public string SearchBarPlaceholderText => LocalizationResources.Instance["Search"].ToString();

        /// <summary>
        /// Selects search result
        /// </summary>
        /// <param name="searchResult"></param>
        public void SelectSearchResult(SearchResult searchResult)
        {            
            _currentState.SelectSearchResultAction(searchResult);

            //// Select media location
            //SelectedMediaLocation = mediaLocation;

            //// Select relevant options. User may have selected artist, media item collection or media item
            //switch (searchResult.EntityType)
            //{
            //    case EntityTypes.Artist:
            //        // Display media item collections for artist
            //        SelectedArtist = _artists.First(a => a.Name == searchResult.Artist!.Name);
            //        break;
            //    case EntityTypes.MediaItem:
            //        // Display media item for media item collection
            //        SelectedArtist = _artists.First(a => a.Name == searchResult.Artist!.Name);
            //        SelectedMediaItemCollection = _mediaItemCollections.First(mic => mic.Name == searchResult.MediaItemCollection!.Name);
            //        SelectedMediaItem = _mediaItems.First(mi => mi.Name == searchResult.MediaItem!.Name);
            //        break;
            //    case EntityTypes.MediaItemCollection:
            //        // Display media items for media item collection
            //        SelectedArtist = _artists.First(a => a.Name == searchResult.Artist!.Name);
            //        SelectedMediaItemCollection = _mediaItemCollections.First(mic => mic.Name == searchResult.MediaItemCollection!.Name);
            //        break;
            //}
        }
    }
}
