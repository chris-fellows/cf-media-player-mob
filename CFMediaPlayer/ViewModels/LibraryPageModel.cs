using CFMediaPlayer.Constants;
using CFMediaPlayer.Enums;
using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;
using CFMediaPlayer.Services;
using CFMediaPlayer.Utilities;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using static Android.Provider.MediaStore.Audio;

namespace CFMediaPlayer.ViewModels
{
    /// <summary>
    /// View model for Library page.
    /// 
    /// Notes:
    /// - A media item only becomes the current media item when uses presses Play button. Selecting a media item
    ///   does not make it the current media item.
    /// - When the Selected[X] property is set (Media item location, artist, media item collection etc) then the
    ///   child items below are refreshed and a default is selected. This cascades down until the lowest level.
    ///   When used internally then it can cause un-necessary file system reads because the default might select
    ///   artist 1 but we want artist 5.  
    /// - For setting SelectedMediaItemLocation then the child items can be loaded in the background so that the
    ///   UI can be updated to reflect that the app is busy.
    /// - We disable loading child items in the background (E.g. SelectedMediaItemLocationAsync) if we're resetting
    ///   the UI (Reset method) because the background code runs after the reset completes and corrupts the required
    ///   UI state.
    /// </summary>
    public class LibraryPageModel : PageModelBase, INotifyPropertyChanged
    {
        /*
        public event PropertyChangedEventHandler? PropertyChanged;

        public LocalizationResources LocalizationResources => LocalizationResources.Instance;

        public void OnPropertyChanged([CallerMemberName] string name = "") =>
                     PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        */

        //public delegate void GeneralError(Exception exception);
        //public event GeneralError? OnGeneralError;

        private readonly IAudioSettingsService _audioSettingsService;
        private readonly ICurrentState _currentState;
        private readonly ILogWriter _logWriter;
        private readonly IMediaLocationService _mediaLocationService;
        private readonly IMediaSearchService _mediaSearchService;
        private readonly IMediaSourceService _mediaSourceService;
        private readonly IUIThemeService _uiThemeService;
        private readonly IUserSettingsService _userSettingsService;

        private List<MediaAction> _mediaActions = new List<MediaAction>();
        private List<MediaLocation> _mediaLocations = new List<MediaLocation>();
        private List<Artist> _artists = new List<Artist>();
        private List<MediaItemCollection> _mediaItemCollections = new List<MediaItemCollection>();
        private List<MediaItem> _mediaItems = new List<MediaItem>();

        private bool _isResetActive = false;

        private UITheme _uiTheme;
        private AudioSettings _audioSettings;       
        private bool _isSearchBusy = false;
        
        /// <summary>
        /// Command to toggle play/pause/stop for selected media item
        /// </summary>
        public ICommand PlayToggleCommand { get; set; }

        public LibraryPageModel(IAudioSettingsService audioSettingsService,
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
            
            PlayToggleCommand = new Command(DoPlayToggle);
       
            // Set action to handle request to select media action.
            // This is typically called if user selects media action "Open album X" when playing playlist item
            _currentState.SelectMediaItemCollectionAction = (mediaLocation, artist, mediaItemCollection, mediaItem) =>
            {
                // Select correct media location, artist, media item collection & (if any) media item
                Reset(mediaLocation.Name, false, 
                      artist.Name, false,
                      mediaItemCollection.Name, false,
                      mediaItem?.Name, false);
                
                // Select Library tab
                _currentState.SelectTabByTitleAction(LocalizationResources["TabLibraryText"].ToString());                
            };

            ConfigureEvents();
            
            // Display media locations
            LoadMediaLocationsToDisplayInUI();

            //MediaLocations = _mediaLocationService.GetAll();

            //foreach (var myMediaLocation in MediaLocations.Where(ml => ml.MediaSourceType == MediaSourceTypes.Storage && ml.MediaItemTypes.Contains(MediaItemTypes.Music)))
            //{
            //    var myMediaSource = _mediaSourceService.GetAll().First(ms => ms.MediaLocation.Name == myMediaLocation.Name);
            //    var myArtists = myMediaSource.GetArtists(false);
            //    if (myArtists.Any())
            //    {
            //        var indexDataFolder = Path.Combine(FileSystem.AppDataDirectory, "Index");                    
            //        IIndexedData indexedData = new IndexedData(indexDataFolder);
            //        IndexData(indexedData, myMediaSource);
            //        SearchIndexData(indexedData);
            //    }
            //}

            // Set default media location
            var mediaLocation = MediaLocations.FirstOrDefault(ml => ml.MediaSourceType == MediaSourceTypes.Storage &&
                                                    ml.MediaItemTypes.Contains(MediaItemTypes.Music));

            if (mediaLocation == null)
            {
                mediaLocation = MediaLocations.First();
            }
            
            SelectedMediaLocation = mediaLocation;            
        }

        public ICurrentState CurrentState => _currentState;

        private void DoPlayToggle(object parameter)
        {
            // Get media item
            var mediaItem = _mediaItems.First(mi => mi.Name == parameter.ToString());

            // Get media player status to check if it has this media item
            var mediaPlayerStatus = _currentState.GetMediaItemPlayStatusFunction(mediaItem);

            if (mediaPlayerStatus != null &&
                (mediaPlayerStatus == MediaPlayerStatuses.Playing ||
                mediaPlayerStatus == MediaPlayerStatuses.Paused))    // Currently playing, toggle play/pause/stop
            {
                _currentState.Events.RaiseOnTogglePlayMediaItem(mediaItem);
            }
            else    // Play new media item
            {
                _currentState.Events.RaiseOnPlayMediaItem(mediaItem);
            }            
        }

        /// <summary>
        /// Configure event handlers
        /// </summary>
        private void ConfigureEvents()
        {
            // Set event handler for playlist updated.
            // Reasons:
            // - Media item added to playlist X (Only for storage media source).
            // - Media item removed from playlist X (Only for storage media source).
            // - Clear playlist.
            // - Delete playlist.
            //
            // If we're not displaying this playlist then we don't need to take action except to refresh media actions.            
            // If we're displaying another media source (E.g. Storage) then CurrentPage will handle updates such as
            // adding/removing from playlist which typically just requires a refresh of the media actions for the media item.
            _currentState.Events.OnPlaylistUpdated += (systemEventType, mediaItemCollection, mediaItem) =>
            {
                System.Diagnostics.Debug.WriteLine($"OnPlaylistUpdated in LibraryPageModel {systemEventType}");

                var isResetCalled = false;
                if (_selectedMediaLocation.MediaSourceType == MediaSourceTypes.Playlist &&
                    _selectedMediaItemCollection.Name == mediaItemCollection.Name)   // Updated playlist selected
                {
                    // Set media item collection to select, select any if playlist just deleted                                        
                    //var mediaItemCollectionNameToSelect = systemEventType == SystemEventTypes.PlaylistDeleted ? "" : mediaItemCollection.Name;

                    // Reset selected items
                    switch(systemEventType)
                    {
                        case SystemEventTypes.PlaylistCleared:
                        case SystemEventTypes.PlaylistItemAdded:
                        case SystemEventTypes.PlaylistItemRemoved:
                            // Refresh media items for playlist                            
                            isResetCalled = true;
                            Reset(SelectedMediaLocation.Name, false,
                                SelectedArtist.Name, false,
                                SelectedMediaItemCollection.Name, true, // Force SelectedMediaItemCollection to be set (Refreshes media items)
                                null, false);
                            break;

                        case SystemEventTypes.PlaylistDeleted:
                            // Select different playlist (or None)
                            isResetCalled = true;
                            Reset(SelectedMediaLocation.Name, false,
                                 SelectedArtist.Name, false,
                                 null, false,
                                 null, false);
                            break;                        
                    }
                }

                // Refesh media actions. E.g. User added/remove media item from playlist
                // TODO: Optimise this to only call if absolutely necessary
                if (!isResetCalled)
                {
                    LoadMediaActions(SelectedMediaLocation, SelectedMediaItem);
                }
            };

            // Set event handler for queue updated.
            // Reasons:
            // - Media item added to queue (Only for storage media source).
            // - Media item removed from queue (Only for storage media source).
            // - N random media items added to queue (Only for queue media source)
            // - Queue cleared (Only for queue media source).
            _currentState.Events.OnQueueUpdated += (systemEventType, mediaItem) =>
            {
                System.Diagnostics.Debug.WriteLine($"[in] OnQueueUpdated in LibraryPageModel {systemEventType}");

                var isResetCalled = false;
                if (_selectedMediaLocation.MediaSourceType == MediaSourceTypes.Queue)   // Displaying queue
                {
                    //switch (systemEventType)
                    //{
                    //    case SystemEventTypes.QueueCleared:
                    //        break;
                    //    case SystemEventTypes.QueueItemAdded:
                    //        break;
                    //    case SystemEventTypes.QueueItemsAdded:
                    //        break;
                    //    case SystemEventTypes.QueueItemRemoved:
                    //        break;
                    //}

                    isResetCalled = true;
                    Reset(SelectedMediaLocation.Name, false,
                            SelectedArtist.Name, false,
                            SelectedMediaItemCollection.Name, true,     // Force SelectedMediaItemCollection property to be set (Refreshes media items)
                            SelectedMediaItem.Name, true);              // Force SelectedMediaItem property to be set
                }

                // Refesh media actions. E.g. User added/removed media item from queue
                // TODO: Optimise this to only call if absolutely necessary
                if (!isResetCalled)
                {
                    LoadMediaActions(SelectedMediaLocation, SelectedMediaItem);
                }

                System.Diagnostics.Debug.WriteLine("[out] OnQueueUpdated in LibraryPageModel");
            };

            // Set event handled for current media item status. 
            // - If playing then animated equalizer.
            // - If paused then non-animated equalized.
            // - Else no image.            
            _currentState.Events.OnCurrentMediaItemStatusChanged += (mediaItem, isPlaying, isPaused) =>
            {                
                foreach(var currentMediaItem in _mediaItems)
                {
                    if (currentMediaItem.FilePath == mediaItem.FilePath)   // Current item
                    {
                        if (isPaused || isPlaying)
                        {
                            currentMediaItem.StatusImage = ImageConstants.AnimatedEqualizerImage;
                            currentMediaItem.IsStatusImageAnimating = isPlaying;
                            if (isPaused)
                            {
                                currentMediaItem.PlayToggleImage = ImageConstants.PlayMediaItemImage;
                            }
                            else if (isPlaying)
                            {
                                currentMediaItem.PlayToggleImage = currentMediaItem.IsPausable ? ImageConstants.PauseMediaItemImage :
                                                                    ImageConstants.StopMediaItemImage;                                                            
                            }                            
                        }
                        else
                        {                            
                            currentMediaItem.StatusImage = "";
                            currentMediaItem.IsStatusImageAnimating = false;
                            currentMediaItem.PlayToggleImage = ImageConstants.PlayMediaItemImage;                            
                        }
                    }
                    else
                    {                        
                        currentMediaItem.StatusImage = "";
                        currentMediaItem.IsStatusImageAnimating = false;
                        currentMediaItem.PlayToggleImage = ImageConstants.PlayMediaItemImage;                        
                    }
                }                          
                
                // Force refresh
                var mediaItems = MediaItems;
                MediaItems = new List<MediaItem>();
                MediaItems = mediaItems;
            };
        }

        //private void IndexData(IIndexedData indexedData, IMediaSource mediaSource)
        //{
        //    var artistIndexedItems = new List<IndexedItem>();
        //    var mediaItemCollectionIndexedItems = new List<IndexedItem>();
        //    var mediaItemIndexedItems = new List<IndexedItem>();

        //    var stopwatch = new Stopwatch();
        //    stopwatch.Start();

        //    // Index artists
        //    var artists = mediaSource.GetArtists(false);

        //    foreach (var artist in artists)
        //    {
        //        artistIndexedItems.Add(GetIndexedItem(artist));

        //        var mediaItemCollections = mediaSource.GetMediaItemCollectionsForArtist(artist, false);

        //        foreach (var mediaItemCollection in mediaItemCollections)
        //        {                    
        //            mediaItemCollectionIndexedItems.Add(GetIndexedItem(mediaItemCollection));

        //            var mediaItems = mediaSource.GetMediaItemsForMediaItemCollection(artist, mediaItemCollection, false);

        //            foreach (var mediaItem in mediaItems)
        //            {                        
        //                mediaItemIndexedItems.Add(GetIndexedItem(mediaItem));
        //            }
        //        }
        //    }

        //    stopwatch.Stop();
        //    var elaped = stopwatch.Elapsed;

        //    // Save indexed
        //    indexedData.Write(artistIndexedItems, "Artist");
        //    indexedData.Write(mediaItemCollectionIndexedItems, "MediaItemCollection");
        //    indexedData.Write(mediaItemIndexedItems, "MediaItem");
        //}

        //private void SearchIndexData(IIndexedData indexedData)
        //{
        //    var text = "Muse";

        //    var stopwatch = new Stopwatch();
        //    stopwatch.Start();
        //    var indexedItems = indexedData.Search(text, new List<string>() { "Artist", "MediaItemCollection", "MediaItem" });
        //    stopwatch.Stop();
        //    var elaped = stopwatch.Elapsed;
        //    int xxx = 1000;

        //}

        //private IndexedItem GetIndexedItem(Artist artist)
        //{
        //    var indexedItem = new IndexedItem()
        //    {
        //        Values = new List<IndexedItemValue>()
        //        {
        //            new IndexedItemValue() { Name = "Name", Value = artist.Name }
        //        },
        //        Items = $"{artist.Name}"
        //    };

        //    return indexedItem;
        //}

        //private IndexedItem GetIndexedItem(MediaItemCollection mediaItemCollection)
        //{
        //    var indexedItem = new IndexedItem()
        //    {
        //        Values = new List<IndexedItemValue>()
        //        {
        //            new IndexedItemValue() { Name = "Name", Value = mediaItemCollection.Name }
        //        },
        //        Items = $"{mediaItemCollection.Name}"
        //    };

        //    return indexedItem;
        //}

        //private IndexedItem GetIndexedItem(MediaItem mediaItem)
        //{
        //    var indexedItem = new IndexedItem()
        //    {
        //        Values = new List<IndexedItemValue>()
        //        {
        //            new IndexedItemValue() { Name = "Name", Value = mediaItem.Name }
        //        },
        //        Items = $"{mediaItem.Name}"
        //    };

        //    return indexedItem;
        //}

        public string GetDebugInfo()
        {
            StringBuilder debug = new StringBuilder("");

            if (_selectedMediaLocation != null)
            {
                debug.AppendLine($"Location={_selectedMediaLocation.Name}, MediaSourceType={SelectedMediaSource.MediaLocation.MediaSourceType}");
                debug.AppendLine($"IsAvailable={SelectedMediaSource.IsAvailable}; IsDisplayInUI={SelectedMediaSource.IsDisplayInUI}");

                var artists = SelectedMediaSource.GetArtists(false);
                debug.AppendLine($"Artists={artists.Count}");

                foreach (var artist in SelectedMediaSource.GetArtists(false))
                {
                    debug.AppendLine($"Artist={artist.Name}; {artist.Path}");
                }

                foreach (var source in _selectedMediaLocation.Sources)
                {
                    var isExists = Directory.Exists(source);
                    debug.AppendLine($"Folder {source}; Exists={isExists}");

                    if (isExists)
                    {
                        var subFolders = Directory.GetDirectories(source);
                        debug.AppendLine($"SubFolders={subFolders.Length}");
                        foreach (var subFolder in subFolders)
                        {
                            var files = Directory.GetFiles(subFolder);
                            debug.AppendLine($"SubFolder={subFolder}, Files={files.Length}");
                        }
                    }
                }

                debug.Append(GetArtistsDebug(_selectedMediaLocation));
            }
            else
            {
                debug.AppendLine("No media location");
            }

            return debug.ToString();            
        }

        public string GetArtistsDebug(MediaLocation mediaLocation)
        {
            var debugInfo = new StringBuilder("[Start]");

            foreach (var mediaLocationSource in mediaLocation.Sources)
            {
                if (Directory.Exists(mediaLocationSource))
                {
                    debugInfo.AppendLine($"Checking Media Location Folder={mediaLocationSource}, Exists=true");

                    var artistFolders = Directory.GetDirectories(mediaLocationSource);
                    foreach (var artistFolder in artistFolders)
                    {
                        debugInfo.AppendLine($"Checking IsCheckArtistsFolder for {artistFolder}");
                        var isCheckArtistsFolder = false;
                        if (mediaLocation.MediaItemTypes.Contains(MediaItemTypes.Music))
                        {
                            debugInfo.AppendLine("Checking folder name: WARNING");
                            var names = new[] { Android.OS.Environment.DirectoryAudiobooks.ToLower(),
                                    "playlists",
                                    Android.OS.Environment.DirectoryPodcasts.ToLower(),
                                    "radiostreams" };
                            var folderName = new DirectoryInfo(artistFolder).Name.ToLower();
                            isCheckArtistsFolder = !names.Contains(folderName);
                        }
                        else
                        {
                            isCheckArtistsFolder = true;                       
                        }
                        debugInfo.AppendLine($"IsCheckArtistsFolder={isCheckArtistsFolder}");

                        if (isCheckArtistsFolder)
                        {
                            // Check that folder contains albums
                            debugInfo.AppendLine($"Checking artist folder={artistFolder}");
                            var isHasMediaItemCollections = false;
                            var micFolders = Directory.GetDirectories(artistFolder);
                            debugInfo.AppendLine($"MIC folders={micFolders.Length}");

                            foreach (var mediaItemCollectionFolder in Directory.GetDirectories(artistFolder))
                            {
                                debugInfo.Append($"Checking media item collection folder {mediaItemCollectionFolder}");
                                if (MediaUtilities.IsFolderHasAudioFiles(mediaItemCollectionFolder))
                                {
                                    debugInfo.Append($"Has audio files=true");
                                    isHasMediaItemCollections = true;
                                    break;
                                }
                                else
                                {
                                    debugInfo.Append($"Has audio files=false");
                                }
                            }

                            debugInfo.AppendLine($"HasMediaItemCollections={isHasMediaItemCollections}");                                           
                        }
                        else
                        {
                            debugInfo.AppendLine($"Not checking artist folder={artistFolder}");
                        }

                        debugInfo.AppendLine($"Checked Artist Folder={artistFolder}");
                    }
                }
                else
                {
                    debugInfo.AppendLine($"Media Location Folder={mediaLocationSource}, Exists=false");
                }
            }

            debugInfo.AppendLine("[End]");

            return debugInfo.ToString();
        }

        public bool IsDebugMode => false;

        /// <summary>
        /// Whether component is busy. This is typically used for the ActivityIndicator which is used for any
        /// busy functions, not just search.
        /// </summary>
        public bool IsBusy => IsSearchBusy || IsRefreshBusy;

        /// <summary>
        /// Loads media action for media location and media item (if any)
        /// </summary>
        /// <param name="mediaLocation"></param>
        /// <param name="maxItems"></param>
        private void LoadMediaActions(MediaLocation mediaLocation, MediaItem? mediaItem, int maxItems = 10)
        {
            // Clear
            MediaActions = new List<MediaAction>();

            // Load actions
            var mediaActions = new List<MediaAction>();

            // Load actions for media location
            mediaActions.AddRange(_currentState.SelectedMediaSource!.GetMediaActionsForMediaLocation(_currentState.SelectedMediaLocation));

            // Load actions for media item (if any)
            if (mediaItem != null && mediaItem.EntityCategory == EntityCategory.Real)
            {
                mediaActions.AddRange(_currentState.SelectedMediaSource!.GetMediaActionsForMediaItem(_currentState.SelectedMediaLocation, mediaItem));
            }
           
            while (mediaActions.Count > maxItems)
            {
                mediaActions.RemoveAt(0);
            }

            // Consistent order
            mediaActions = mediaActions.OrderBy(mia => mia.Name).ToList();

            MediaActions = mediaActions;
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
                OnPropertyChanged(nameof(IsBusy));
            }
        }

        /// <summary>
        /// Whether refresh is busy. E.g. User selected media location.
        /// </summary>
        private bool _isRefreshBusy;
        public bool IsRefreshBusy
        {
            get { return _isRefreshBusy; }
            set
            {
                _isRefreshBusy = value;              

                OnPropertyChanged(nameof(IsRefreshBusy));
                OnPropertyChanged(nameof(IsSearchBusy));
                OnPropertyChanged(nameof(IsBusy));
            }
        }

        public List<MediaAction> MediaActions
        {
            get { return _mediaActions; }
            set
            {
                _mediaActions = value;

                OnPropertyChanged(nameof(MediaActions));
            }
        }

        ///// <summary>
        ///// Whether to shuffle media items
        ///// </summary>
        //public bool ShufflePlay
        //{
        //    get { return _shufflePlay; }
        //    set
        //    {
        //        _shufflePlay = value;

        //        OnPropertyChanged(nameof(ShufflePlay));

        //        // If shuffle selected then randomly sort media items
        //        if (_shufflePlay && !_isMediaItemsShuffled && _mediaItems.Any())
        //        {
        //            _mediaItems.SortRandom();
        //            _isMediaItemsShuffled = true;

        //            OnPropertyChanged(nameof(MediaItems));
        //        }
        //    }
        //}

        ///// <summary>
        ///// Whether to auto-play next media item when current media item completed
        ///// </summary>
        //public bool AutoPlayNext
        //{
        //    get { return _autoPlayNext; }

        //    set
        //    {
        //        _autoPlayNext = value;

        //        OnPropertyChanged(nameof(AutoPlayNext));
        //    }
        //}

        /// <summary>
        /// Command to play previous media item
        /// </summary>
        public ICommand PrevCommand { get; set; }

        /// <summary>
        /// Command to play next media item
        /// </summary>
        public ICommand NextCommand { get; set; }

        /// <summary>
        /// Command to play or pause/stop current media item
        /// </summary>
        public ICommand PlayOrNotCommand { get; set; }

        /// <summary>
        /// Command to stop playing or pausing current media item
        /// </summary>
        public ICommand StopCommand { get; set; }

        //public ICommand ExecuteMediaItemActionCommand { get; set; }    

        private IMediaSource? SelectedMediaSource
        {
            get
            {
                return _selectedMediaLocation != null ?
                    _mediaSourceService.GetAll().First(ms => ms.MediaLocation.Name == _selectedMediaLocation.Name) :
                    null;
            }
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

        private void ClearMediaItems()
        {
            MediaItems = new();
            SelectedMediaItem = null;
        }
      
        private void ClearMediaItemCollections()
        {
            MediaItemCollections = new List<MediaItemCollection>();
            SelectedMediaItemCollection = null;
        }

        private void ClearArtists()
        {
            Artists = new List<Artist>();
            SelectedArtist = null;
        }
     
        private MediaLocation? _selectedMediaLocation;

        /// <summary>
        /// Selected media location where some processing (E.g. Loading artists, albums) is done asynchronously. When this
        /// property returns then SelectedMediaLocation has been set correctly but the associated data (Artists etc) hasn't
        /// been loaded.
        /// 
        /// This property is used from the UI so that it can be updated with progress while the data is loading.
        /// </summary>
        public MediaLocation SelectedMediaLocationAsync
        {
            get { return SelectedMediaLocation; }
            set
            {              
                // Bit of a hack. If inside Reset then we don't want any async code triggered by the UI running. The async code
                // will run after Reset returns which messes up the UI state.
                // If this code is made synchronous then it just means that the UI won't display the busy indicator when a different
                // media location is selected.
                if (_isResetActive)   // Run synchronously
                {
                    SetSelectedMediaLocation(value, false);
                }
                else     // Run asynchrously
                {
                    SetSelectedMediaLocation(value, true);
                }
            }
        }

        /// <summary>
        /// Selected media location.
        /// 
        /// If media location is set then we display the defaults for the media location which selects an artist, media item collection
        /// and media item.
        /// </summary>
        public MediaLocation SelectedMediaLocation
        {
            get
            {
                return _selectedMediaLocation;
            }

            set
            {              
                SetSelectedMediaLocation(value, false);                

                //IsRefreshBusy = true;                

                //_selectedMediaLocation = value;                
                //_currentState.SelectedMediaLocation = value;
                //_currentState.SelectedMediaSource = value == null ? null : _mediaSourceService.GetAll().First(ms => ms.MediaLocation.Name == _selectedMediaLocation.Name);

                //// Reset ICurrentState.ShufflePlay if not allowed. E.g. Radio streams
                //if (_selectedMediaLocation != null && !_currentState.SelectedMediaSource.IsShufflePlayAllowed)
                //{
                //    _currentState.ShufflePlay = false;
                //}

                //// Reset ICurrentState.AutoPlayNext if not allowed. E.g. Radio streams
                //if (_selectedMediaLocation != null && !_currentState.SelectedMediaSource.IsAutoPlayNextAllowed)
                //{
                //    _currentState.AutoPlayNext = false;
                //}

                //System.Diagnostics.Debug.WriteLine(_selectedMediaLocation == null ? "Set SelectedMediaLocation=null" : $"Set SelectedMediaLocation={_selectedMediaLocation.Name}");

                //// Notify properties on change of selected media location
                //OnPropertyChanged(nameof(SelectedMediaLocation));
                //OnPropertyChanged(nameof(SearchBarPlaceholderText));

                //// Display artists for media source
                //if (_selectedMediaLocation != null)
                //{                    
                //    LoadMediaLocationDefaults();                    
                //}
                
                //IsRefreshBusy = false;
            }
        }

        //private Task SetSelectedArtist(Artist? artist, bool async = false)
        //{
        //    _selectedArtist = artist;
        //    _currentState.SelectedArtist = artist;

        //    OnPropertyChanged(nameof(SelectedArtist));

        //    Task task = Task.CompletedTask;
        //    if (async)
        //    {
        //        task = Task.Factory.StartNew(() =>
        //        {
        //            if (_selectedArtist != null)
        //            {
        //                // Load media item collections for artist
        //                LoadMediaItemCollections(_selectedArtist);

        //                // Select media item collection
        //                SelectTheMediaItemCollection(_selectedArtist, null);
        //            }
        //        });
        //    }
        //    else    // Synchronous
        //    {
        //        if (_selectedArtist != null)
        //        {
        //            // Load media item collections for artist
        //            LoadMediaItemCollections(_selectedArtist);

        //            // Select media item collection
        //            SelectTheMediaItemCollection(_selectedArtist, null);
        //        }
        //    }
        //    return task;
        //}

        /// <summary>
        /// Sets SelectedMediaLocation in one of the following ways:
        /// - Synchronously. E.g. Internal call.
        /// - Asynchronously. E.g. From UI.
        /// 
        /// The property is always set synchrounously. The slow part (Loading associated data such as artists, albums
        /// is done) may be done synchrously or asynchrously.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="async"></param>
        /// <returns></returns>
        private Task SetSelectedMediaLocation(MediaLocation? value, bool async = false)
        {            
            IsRefreshBusy = true;
          
            _selectedMediaLocation = value;
            _currentState.SelectedMediaLocation = value;
            _currentState.SelectedMediaSource = value == null ? null : _mediaSourceService.GetAll().First(ms => ms.MediaLocation.Name == _selectedMediaLocation.Name);

            if (value == null) return Task.CompletedTask;

            // Reset ICurrentState.ShufflePlay if not allowed. E.g. Radio streams
            if (_selectedMediaLocation != null && !_currentState.SelectedMediaSource.IsShufflePlayAllowed)
            {
                _currentState.ShufflePlay = false;
            }

            // Reset ICurrentState.AutoPlayNext if not allowed. E.g. Radio streams
            if (_selectedMediaLocation != null && !_currentState.SelectedMediaSource.IsAutoPlayNextAllowed)
            {
                _currentState.AutoPlayNext = false;
            }
            
            // Notify properties on change of selected media location
            OnPropertyChanged(nameof(SelectedMediaLocation));
            OnPropertyChanged(nameof(SelectedMediaLocationAsync));
            OnPropertyChanged(nameof(SearchBarPlaceholderText));

            Task task = Task.CompletedTask;
            if (async)
            {
                task = Task.Factory.StartNew(() =>
                {
                    // Run in main thread. If we don't do this then the UI is left in an inconsistent state. We will correctly
                    // select a real artist but the UI will automatically set SelectedArtist=[All] (First artist).
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        // Clear actions
                        ClearActionsForMediaLocation();

                        // Load actions
                        LoadMediaActions(SelectedMediaLocation, null);

                        // Display artists for media source
                        if (_selectedMediaLocation != null)
                        {                         
                            LoadArtists();
                            SelectArtist(null, false);
                        }

                        IsRefreshBusy = false;
                    });                  
                });
            }
            else    // Synchronous
            {                
                // Clear actions
                ClearActionsForMediaLocation();
               
                // Load actions
                LoadMediaActions(SelectedMediaLocation, null);

                if (_selectedMediaLocation != null)
                {
                    LoadArtists();
                    SelectArtist(null, false);
                }
                
                IsRefreshBusy = false;
            }
            
            return task;
        }

        ///// <summary>
        ///// Loads actions for media item
        ///// </summary>
        ///// <param name="mediaItem"></param>
        //private void LoadMediaItemActions(MediaItem mediaItem, int maxItems = 10)
        //{
        //    // Clear
        //    MediaItemActions = new List<MediaItemAction>();

        //    // Load actions
        //    var mediaItemActions = new List<MediaItemAction>();
        //    if (mediaItem.EntityCategory == EntityCategory.Real)
        //    {
        //        mediaItemActions.AddRange(CurrentMediaSource!.GetActionsForMediaItem(CurrentMediaSource.MediaLocation, mediaItem));
        //    }

        //    // Limit number of actions. If we have lots of playlists then there will be lots of actions for playlists.            
        //    while (mediaItemActions.Count > maxItems)
        //    {
        //        var playlistAction = mediaItemActions.FirstOrDefault(mia => mia.ActionToExecute == Enums.MediaItemActions.AddToPlaylist);
        //        if (playlistAction == null)
        //        {
        //            break;
        //        }
        //        else
        //        {
        //            mediaItemActions.Remove(playlistAction);
        //        }
        //    }
        //    while (mediaItemActions.Count > maxItems)
        //    {
        //        mediaItemActions.RemoveAt(0);
        //    }

        //    // Consistent order
        //    mediaItemActions = mediaItemActions.OrderBy(mia => mia.Name).ToList();

        //    MediaItemActions = mediaItemActions;
        //}

        private MediaItem? _selectedMediaItem;

        /// <summary>
        /// Selected media item.
        /// 
        /// If media item is set then we load list of actions for media item. E.g. Add to playlist X, remove from playlist Y etc
        /// </summary>
        public MediaItem? SelectedMediaItem
        {
            get
            {
                return _selectedMediaItem;
            }

            set
            {                
                //SetSelectedMediaItem(value);

                _selectedMediaItem = value;
                _currentState.SelectedMediaItem = value;    // Triggers CurrentPage to load media item actions
                System.Diagnostics.Debug.WriteLine(_selectedMediaItem == null ? "Set SelectedMediaItem=null" : $"Set SelectedMediaItem={_selectedMediaItem.Name}");

                // Load media actions
                LoadMediaActions(SelectedMediaLocation, SelectedMediaItem);

                // Notify properties on change of selected media item
                OnPropertyChanged(nameof(SelectedMediaItem));
                OnPropertyChanged(nameof(IsRealMediaItemSelected));                
                OnPropertyChanged(nameof(MainLogoImage));   // If media item specific image            
            }
        }      

        private MediaItemCollection? _selectedMediaItemCollection;

        /// <summary>
        /// Selected media item location.
        /// 
        /// If media item location the defaults for the media item location which selects a media item.
        /// </summary>
        public MediaItemCollection? SelectedMediaItemCollection
        {
            get
            {
                return _selectedMediaItemCollection;
            }

            set
            {                                
                _selectedMediaItemCollection = value;
                _currentState.SelectedMediaItemCollection = value;
                
                // Notify properties on change of selected media item collection
                OnPropertyChanged(nameof(SelectedMediaItemCollection));              
                OnPropertyChanged(nameof(MainLogoImage));   // If media item collection specific image

                // Display media items for album, select default
                if (_selectedMediaItemCollection != null)
                {
                    // Load media items
                    LoadMediaItems(_selectedArtist, _selectedMediaItemCollection);

                    // Select media item
                    SelectMediaItem(null, false);
                }
            }
        }

        //private void SetSelectedMediaItemCollection(MediaItemCollection? value, bool setSelectedMediaItem, string? mediaItemNameToSelect)
        //{
        //    _selectedMediaItemCollection = value;
        //    _currentState.SelectedMediaItemCollection = value;
        //    System.Diagnostics.Debug.WriteLine(_selectedMediaItemCollection == null ? "Set SelectedMediaItemCollection=null" : $"Set SelectedMediaItemCollection={_selectedMediaItemCollection.Name}");

        //    // Notify properties on change of selected media item collection
        //    OnPropertyChanged(nameof(SelectedMediaItemCollection));
        //    System.Diagnostics.Debug.WriteLine("[SelectedMediaItemCollection] Notifying OnPropertyChanged for MainLogoImage");
        //    OnPropertyChanged(nameof(MainLogoImage));   // If media item collection specific image

        //    // Display media items for album, select default
        //    if (_selectedMediaItemCollection != null)
        //    {
        //        // Load media items
        //        LoadMediaItems(_selectedArtist, _selectedMediaItemCollection);

        //        // Select media item
        //        if (setSelectedMediaItem)
        //        {
        //            SelectMediaItem(mediaItemNameToSelect);
        //        }
        //    }
        //}

        /// <summary>
        /// Main logo image to display   
        /// </summary>
        public string MainLogoImage
        {
            get
            {
                // Set media item level image. E.g. Radio stream, playlist item with image for album.
                // If storage then do nothing because we want to display the album image.
                if (_selectedMediaLocation != null && _selectedMediaLocation.MediaSourceType != MediaSourceTypes.Storage)
                {
                    if (_selectedMediaItem != null && !String.IsNullOrEmpty(_selectedMediaItem.ImagePath))
                    {
                        //System.Diagnostics.Debug.WriteLine($"MainLogoImage={_selectedMediaItem.ImagePath}");
                        return _selectedMediaItem.ImagePath;
                    }
                }

                // Set media item collection level image. E.g. Album image
                if (_selectedMediaItemCollection != null && !String.IsNullOrEmpty(_selectedMediaItemCollection.ImagePath))
                {
                    //System.Diagnostics.Debug.WriteLine($"MainLogoImage={_selectedMediaItemCollection.ImagePath}");
                    return _selectedMediaItemCollection.ImagePath;
                }

                // Set media source level image
                if (SelectedMediaSource != null)
                {
                    //System.Diagnostics.Debug.WriteLine($"MainLogoImage={SelectedMediaSource.ImagePath}");
                    return SelectedMediaSource.ImagePath;
                }

                //System.Diagnostics.Debug.WriteLine($"MainLogoImage=cassette_player_audio_speaker_sound_icon.png");
                return "cassette_player_audio_speaker_sound_icon.png";  // Default
            }
        }

        private Artist? _selectedArtist;

        /// <summary>
        /// Selected artist.
        /// 
        /// If artist is set then we display the defaults for the artist which selects a media item collection and a media
        /// item.
        /// </summary>
        public Artist? SelectedArtist
        {
            get
            {
                return _selectedArtist;
            }

            set
            {                               
                _selectedArtist = value;
                _currentState.SelectedArtist = value;
                
                // Notify properties on change of selected artist
                OnPropertyChanged(nameof(SelectedArtist));

                // Display albums & media items for artist, select default
                if (_selectedArtist != null)
                {
                    // Load media item collections for artist
                    LoadMediaItemCollections(_selectedArtist);

                    // Select media item collection
                    SelectMediaItemCollection(null, false);
                }                
            }
        }    
        
        ///// <summary>
        ///// Sets selected artist, loads media item collections and (if required) selects media item collection
        ///// </summary>
        ///// <param name="value"></param>
        ///// <param name="selectMediaItemCollection"></param>
        //private void SetSelectedArtist(Artist? value, bool selectMediaItemCollection, string? mediaItemCollectionToSelect)
        //{
        //    _selectedArtist = value;
        //    _currentState.SelectedArtist = value;
        //    System.Diagnostics.Debug.WriteLine(_selectedArtist == null ? "Set SelectedArtist=null" : $"Set SelectedArtist={_selectedArtist.Name}");

        //    // Notify properties on change of selected artist
        //    OnPropertyChanged(nameof(SelectedArtist));

        //    // Display albums & media items for artist, select default
        //    if (_selectedArtist != null)
        //    {
        //        // Load media item collections for artist
        //        LoadMediaItemCollections(_selectedArtist);

        //        // Select media item collection
        //        if (selectMediaItemCollection)
        //        {
        //            SelectMediaItemCollection(mediaItemCollectionToSelect);
        //        }
        //    }
        //}

        //private void LoadMediaItemsCollectionsNone()
        //{
        //    var mediaItemCollections = new List<MediaItemCollection>() { MediaItemCollection.InstanceNone };
        //    MediaItemCollections = mediaItemCollections;
        //}

        private void ClearActionsForMediaLocation()
        {
            MediaActions = new List<MediaAction>();           
        }

        /// <summary>
        /// Selects artist from Artists. Selects named artist if specified else other.
        /// </summary>
        private void SelectArtist(string? artistNameToSelect, bool isForce)
        {          
            // Check if artist already selected
            if (!isForce)
            {
                if (!String.IsNullOrEmpty(artistNameToSelect) &&
                    SelectedArtist != null &&
                    SelectedArtist.Name == artistNameToSelect)      // Already selected
                {
                    return;
                }
            }

            // Set default artist. Ideally real artist else none else shuffle
            var artist = GetArtistToSelect(Artists, artistNameToSelect);
            SelectedArtist = artist;    
        }

        /// <summary>
        /// Selects media item collection from MediaItemCollections. Selects named media item collection if specified else other.
        /// </summary>        
        /// <param name="mediaItemCollectionNameToSelect">Name of media item collection (if any) to select</param>
        private void SelectMediaItemCollection(string? mediaItemCollectionNameToSelect, bool isForce)
        {            
            // Check if media item collection already selected
            if (!isForce)
            {
                if (!String.IsNullOrEmpty(mediaItemCollectionNameToSelect) &&
                    SelectedMediaItemCollection != null &&
                    SelectedMediaItemCollection.Name == mediaItemCollectionNameToSelect)      // Already selected
                {
                    return;
                }
            }

            // Select media item collection
            MediaItemCollection mediaItemCollection = GetMediaItemCollectionToSelect(MediaItemCollections, mediaItemCollectionNameToSelect);            
            
            SelectedMediaItemCollection = mediaItemCollection;            
        }

        /// <summary>
        /// Selected media item from MediaItems. Selects named media item if specified else other.
        /// </summary>        
        /// <param name="mediaItemNameToSelect">Name of media item (if any) to select</param>
        private void SelectMediaItem(string? mediaItemNameToSelect, bool isForce)
        {            
            // Check if media item already selected
            if (!isForce)
            {
                if (!String.IsNullOrEmpty(mediaItemNameToSelect) &&
                    SelectedMediaItem != null &&
                    SelectedMediaItem.Name == mediaItemNameToSelect)      // Already selected
                {
                    return;
                }
            }

            // Select media item
            MediaItem mediaItem = GetMediaItemToSelect(MediaItems, mediaItemNameToSelect);

            SelectedMediaItem = mediaItem;            
        }       

        /// <summary>      
        /// Loads media items for artist and media item collection:
        /// - If [All] artist then loads N media items for any artist.
        /// - If [All] media item collection then loads all media items for artist.
        /// - If real media item collection then loads all media items.
        /// </summary>
        /// <param name="artist"></param>
        /// <param name="mediaItemCollection"></param>
        private void LoadMediaItems(Artist? artist, MediaItemCollection? mediaItemCollection)
        {          
            // Clear media items and everything below           
            ClearMediaItems();
            
            var mediaItems = new List<MediaItem>();
            if (SelectedMediaSource != null &&
                SelectedMediaSource.IsAvailable &&
                artist != null && 
                mediaItemCollection != null)
            {
                mediaItems.AddRange(SelectedMediaSource!.GetMediaItemsForMediaItemCollection(artist, mediaItemCollection, true));
                
                // If shuffle play then sort randomly
                if (_currentState.ShufflePlay)
                {
                    mediaItems.SortRandom();
                }
            }
            
            // Get media items
            //var mediaItems = SelectedMediaSource!.GetMediaItemsForMediaItemCollection(artist, mediaItemCollection, true);            

            // Add None if no media items
            if (!mediaItems.Any())
            {
                mediaItems.Add(MediaItem.InstanceNone);
            }

            // Set play image
            foreach (var mediaItem in mediaItems.Where(mi => mi.EntityCategory == EntityCategory.Real))
            {                                
                // Set defaults
                mediaItem.StatusImage = "";
                mediaItem.IsStatusImageAnimating = false;
                mediaItem.PlayToggleImage = ImageConstants.PlayMediaItemImage;   // Only enabled if media item playable          

                // Get media item status if current                
                var mediaPlayStatus = _currentState.GetMediaItemPlayStatusFunction == null ? null :     // CurrentPageModel constructor not completed yet
                                        _currentState.GetMediaItemPlayStatusFunction(mediaItem);                

                // Set status if current media item
                if (mediaPlayStatus != null)   // Current media item
                {                    
                    switch (mediaPlayStatus)
                    {
                        case MediaPlayerStatuses.Playing:
                            mediaItem.StatusImage = ImageConstants.AnimatedEqualizerImage;
                            mediaItem.IsStatusImageAnimating = true;
                            mediaItem.PlayToggleImage = mediaItem.IsPausable ? ImageConstants.PauseMediaItemImage :
                                                            ImageConstants.StopMediaItemImage;                                               
                            break;
                        case MediaPlayerStatuses.Paused:
                            mediaItem.StatusImage = ImageConstants.AnimatedEqualizerImage;                            
                            //mediaItem.PlayToggleImage = ImageConstants.PlayMediaItemImage;
                            break;                        
                    }
                }                
            }
            
            MediaItems = mediaItems;            
        }

        /// <summary>
        /// Loads all artists for media location. Adds [Shuffle] artist.
        /// </summary>
        private void LoadArtists()
        {            
            // Clear artists and everything below
            ClearArtists();
            ClearMediaItemCollections();
            ClearMediaItems();
            ClearSearchResults();   // Search results are for current media location

            // Get all artists
            // Storage - Real, [Shuffle], [None]
            var artists = new List<Artist>();
            if (SelectedMediaSource != null && SelectedMediaSource.IsAvailable)
            {
                artists.AddRange(SelectedMediaSource!.GetArtists(true));
            }
                
            // Add None if no artists
            if (!artists.Any())
            {
                artists.Add(Artist.InstanceNone);
            }

            Artists = artists;            
        }

        /// <summary>
        /// Loads media item collections for artist. May be a real artist or shuffle artist.
        /// </summary>
        /// <param name="artist"></param>
        private void LoadMediaItemCollections(Artist? artist)
        {            
            // Clear media item collections and below
            ClearMediaItemCollections();
            ClearMediaItems();

            var mediaItemCollections = new List<MediaItemCollection>();
            if (SelectedMediaSource != null && SelectedMediaSource.IsAvailable && artist != null)
            {
                mediaItemCollections.AddRange(SelectedMediaSource!.GetMediaItemCollectionsForArtist(artist, true));
            }

            if (!mediaItemCollections.Any())
            {
                mediaItemCollections.Add(MediaItemCollection.InstanceNone);
            }

            MediaItemCollections = mediaItemCollections;            
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

        /// <summary>
        /// Media items. May be for single media item collection, specific artist (if media item collection shuffle)
        /// or all artists (if artist shuffle)
        /// </summary>
        public List<MediaItem> MediaItems
        {
            get { return _mediaItems; }
            set
            {
                _mediaItems = value;
                _currentState.MediaItems = value;              

                OnPropertyChanged(nameof(MediaItems));
            }
        }

        /// <summary>
        /// Media item collections. May contain [Shuffle] and [Multiple] media item collections.
        /// </summary>
        public List<MediaItemCollection> MediaItemCollections
        {
            get { return _mediaItemCollections; }
            set
            {
                _mediaItemCollections = value;

                OnPropertyChanged(nameof(MediaItemCollections));
            }
        }

        /// <summary>
        /// Artists. May contain [Shuffle] artist
        /// </summary>
        public List<Artist> Artists
        {
            get { return _artists; }
            set
            {
                _artists = value;

                OnPropertyChanged(nameof(Artists));
            }
        }

        //public List<MediaItemAction> MediaItemActions
        //{
        //    get { return _mediaItemActions; }
        //    set
        //    {
        //        _mediaItemActions = value;

        //        OnPropertyChanged(nameof(MediaItemActions));
        //    }
        //}

        //private void _timer_Elapsed(object? sender, ElapsedEventArgs e)
        //{
        //    System.Diagnostics.Debug.WriteLine($"{DateTimeOffset.UtcNow.ToString()} _timer_Elapsed: IsRealMediaItemSelected={IsRealMediaItemSelected}, DurationInt={DurationMS}, ElapsedTimeInt={ElapsedMS}");

        //    // TODO: Consider disabling timer when app not visible

        //    // Notify property changes to update UI. Only need to be concerned about updating UI with progress
        //    // for current media item.
        //    OnPropertyChanged(nameof(ElapsedTime));
        //    OnPropertyChanged(nameof(ElapsedMS));
        //    OnPropertyChanged(nameof(RemainingTime));
        //    OnPropertyChanged(nameof(RemainingMS));
        //}        

        //public void CreateMediaItems()
        //{
        //    var directory = Path.GetDirectoryName(_selectedMediaItem.FilePath);
        //    for(int index =0; index < 20; index++)
        //    {
        //        File.Copy(_selectedMediaItem.FilePath, Path.Combine(directory, $"Test {index + 1}{Path.GetExtension(_selectedMediaItem.FilePath)}"));
        //    }
        //}

        //public string ElapsedTime
        //{
        //    get
        //    {
        //        if (_mediaPlayer.IsPlaying || _mediaPlayer.IsPaused)
        //        {
        //            return GetDurationString(_mediaPlayer.GetElapsedPlayTime());
        //        }
        //        return GetDurationString(TimeSpan.Zero);    // "00:00:00"
        //    }
        //}

        //public string RemainingTime
        //{
        //    get
        //    {
        //        if (_mediaPlayer.IsPlaying || _mediaPlayer.IsPaused)
        //        {
        //            return GetDurationString(_mediaPlayer.GetTotalDuration() - _mediaPlayer.GetElapsedPlayTime());
        //        }
        //        return GetDurationString(TimeSpan.Zero);
        //    }
        //}

        //private static string GetDurationString(TimeSpan duration)
        //{
        //    return string.Format("{0:00}", duration.Hours) + ":" +
        //    string.Format("{0:00}", duration.Minutes) + ":" +
        //    string.Format("{0:00}", duration.Seconds);
        //}

        //public System.Double RemainingMS
        //{
        //    get
        //    {
        //        if (_mediaPlayer.IsPlaying || _mediaPlayer.IsPaused)
        //        {
        //            return (int)(_mediaPlayer.GetTotalDuration().TotalMilliseconds - _mediaPlayer.GetElapsedPlayTime().TotalMilliseconds);
        //        }
        //        return 0;
        //    }
        //}

        ///// <summary>
        ///// Duration of current media item
        ///// </summary>
        //public System.Double DurationMS
        //{
        //    get
        //    {
        //        if (_mediaPlayer.IsPlaying || _mediaPlayer.IsPaused)
        //        {
        //            return (int)_mediaPlayer.GetTotalDuration().TotalMilliseconds;
        //        }
        //        return 1000;    // Any non-zero value
        //    }
        //}

        ///// <summary>
        ///// Elapsed time of current media item
        ///// </summary>
        //public System.Double ElapsedMS
        //{
        //    get
        //    {
        //        if (_mediaPlayer.IsPlaying || _mediaPlayer.IsPaused)
        //        {
        //            return (int)_mediaPlayer.GetElapsedPlayTime().TotalMilliseconds;
        //        }
        //        return 0;
        //    }

        //    set
        //    {
        //        if (_mediaPlayer.IsPlaying || _mediaPlayer.IsPaused)
        //        {
        //            _mediaPlayer.SetElapsedPlayTime(TimeSpan.FromMilliseconds(value));
        //        }
        //    }
        //}

        ///// <summary>
        ///// Whether media item is playing
        ///// </summary>
        //public bool IsPlaying
        //{
        //    get { return _mediaPlayer.IsPlaying; }
        //}

        ///// <summary>
        ///// Whether media item is paused
        ///// </summary>
        //public bool IsPaused
        //{
        //    get { return _mediaPlayer.IsPaused; }
        //}

        ///// <summary>
        ///// Pauses media item
        ///// </summary>
        //public void Pause()
        //{
        //    _mediaPlayer.Pause();
        //    _elapsedTimer.Enabled = false;
        //}

        ///// <summary>
        ///// Stops media item
        ///// </summary>
        //public void Stop()
        //{
        //    _mediaPlayer.Stop();
        //    _elapsedTimer.Enabled = false;
        //}

        ///// <summary>
        ///// Image source for the play button (Play/Pause/Stop)
        ///// </summary>
        //public string PlayButtonImageSource
        //{
        //    get
        //    {
        //        if (IsPlaying)
        //        {
        //            if (_selectedMediaItem.IsAllowPause)    // Pause
        //            {
        //                return "audio_media_media_player_music_pause_icon.png";
        //            }
        //            else     // Stop
        //            {
        //                return "audio_media_media_player_music_stop_icon.png";
        //            }
        //        }
        //        return "audio_media_media_player_music_play_icon.png";
        //    }
        //}

        ///// <summary>
        ///// Handles media player status change
        ///// </summary>
        ///// <param name="status"></param>
        //private void OnMediaItemStatusChange(MediaPlayerStatuses status, MediaPlayerException? exception)
        //{
        //    //System.Diagnostics.Debug.WriteLine($"{DateTimeOffset.UtcNow.ToString()} OnMediaItemStatusChange: {status}");

        //    switch (status)
        //    {
        //        case MediaPlayerStatuses.Completed:
        //            NotifyPropertiesChangedForPlayState();

        //            _elapsedTimer.Enabled = false;

        //            // Play next if configured                
        //            if (_autoPlayNext &&
        //                SelectedMediaItem != _mediaItems.Last())
        //            {
        //                NextCommand.Execute(null);
        //            }
        //            break;
        //        case MediaPlayerStatuses.Paused:
        //            NotifyPropertiesChangedForPlayState();

        //            _elapsedTimer.Enabled = false;
        //            break;
        //        case MediaPlayerStatuses.Playing:
        //            NotifyPropertiesChangedForPlayState();

        //            _elapsedTimer.Enabled = true;
        //            break;
        //        case MediaPlayerStatuses.Stopped:
        //            NotifyPropertiesChangedForPlayState();

        //            _elapsedTimer.Enabled = false;
        //            break;
        //        case MediaPlayerStatuses.PlayError:
        //            NotifyPropertiesChangedForPlayState();

        //            _elapsedTimer.Enabled = false;

        //            // Notify error
        //            if (OnMediaPlayerError != null && exception != null)
        //            {
        //                OnMediaPlayerError(exception);
        //            }
        //            break;
        //    }

        //    if (OnDebugAction != null) OnDebugAction($"Status={status}");
        //}      

        ///// <summary>
        ///// Whether 'previous media item' is enabled
        ///// </summary>
        //public bool IsPrevEnabled
        //{
        //    get
        //    {
        //        if (IsRealMediaItemSelected)
        //        {
        //            var index = _mediaItems.IndexOf(SelectedMediaItem);
        //            return index > 0;
        //        }
        //        return false;
        //    }
        //}

        //public bool IsPrevVisible
        //{
        //    get
        //    {
        //        if (_selectedMediaItem != null) return _selectedMediaItem.IsAllowPrev;
        //        return true;
        //    }
        //}

        ///// <summary>
        ///// Whether 'next media item' is enabled
        ///// </summary>
        //public bool IsNextEnabled
        //{
        //    get
        //    {
        //        if (IsRealMediaItemSelected)
        //        {
        //            var index = _mediaItems.IndexOf(SelectedMediaItem);
        //            return index < _mediaItems.Count - 1;
        //        }
        //        return false;
        //    }
        //}

        //public bool IsNextVisible
        //{
        //    get
        //    {
        //        if (_selectedMediaItem != null) return _selectedMediaItem.IsAllowNext;
        //        return true;
        //    }
        //}

        ///// <summary>
        ///// Whether user can select position in media item. False for streamed media
        ///// </summary>
        //public bool IsCanSelectPosition
        //{
        //    get
        //    {
        //        if (_selectedMediaItem != null) return _selectedMediaItem.IsCanSelectPosition;
        //        return true;
        //    }
        //}

        /// <summary>
        /// Whether a real media item is selected rather than [None], [Multiple], [Shuffle] etc
        /// </summary>
        public bool IsRealMediaItemSelected
        {
            get
            {
                return SelectedMediaItem != null &&
                    SelectedMediaItem.EntityCategory == EntityCategory.Real;
            }
        }

        /// <summary>
        /// Selects custom playlist.        
        ///
        /// TODO: Handle playlists with same name (Different file extensions for different file formats)
        /// </summary>
        /// <param name="playlistName"></param>
        public void SelectPlaylist(string playlistName)
        {
            // Select playlists
            SelectedMediaLocation = MediaLocations.First(ml => ml.MediaSourceType == MediaSourceTypes.Playlist);

            var artists = this.Artists;
            var mediaItemCollections = this.MediaItemCollections;

            // Select playlist
            var mediaItemCollection = MediaItemCollections.First(mic => mic.Name == playlistName);
            SelectedMediaItemCollection = mediaItemCollection;
        }

        ///// <summary>
        ///// Selected playlist action for media item
        ///// </summary>
        //private MediaItemAction? _selectedMediaItemAction;
        //public MediaItemAction? SelectedMediaItemAction
        //{
        //    get { return _selectedMediaItemAction;  }
        //    set
        //    {
        //        _selectedMediaItemAction = value;         

        //        OnPropertyChanged(nameof(SelectedMediaItemAction));

        //        //if (_selectedMediaItemAction != null && !_selectedMediaItemAction.Name.Equals("Select an action..."))
        //        //{
        //        //    ExecuteMediaItemActionCommand.Execute(null);
        //        //}
        //    }
        //}

        ///// <summary>
        ///// Executes media item action
        ///// </summary>
        ///// <param name="parameter"></param>
        //public void ExecuteMediaItemAction(MediaItemAction mediaItemAction)
        //{
        //    if (mediaItemAction != null)
        //    {
        //        // Get media source to execute action against. We may be displaying the storage source but need to execute
        //        // 'Add to playlist X' against the playlist source.
        //        var mediaSource = _mediaSourceService.GetAll().First(ms => ms.MediaLocation.Name == mediaItemAction.MediaLocationName);

        //        // Execute action. Storage related actions are handled from IMediaSource.ExecuteMediaItemAction and UI related
        //        // actions (E.g. Open album for playlist media item) are executed here.
        //        if (mediaItemAction.ActionToExecute == Enums.MediaItemActions.OpenMediaItemCollection)
        //        {
        //            ExecuteOpenMediaItemCollection(_selectedMediaItem, mediaItemAction, mediaSource);
        //        }
        //        else    // Storage related action
        //        {
        //            // Execute action. E.g. Add to playlist X, remove from playlist Y etc
        //            mediaSource.ExecuteMediaItemAction(_selectedMediaItem, mediaItemAction);

        //            // Check if media item should still be in the current media item list. E.g. If we're displaying the playlist
        //            // media source and user removes selected media item then we need to select another media item
        //            var mediaItems = CurrentMediaSource.GetMediaItemsForMediaItemCollection(_selectedArtist, _selectedMediaItemCollection, false);
        //            if (!mediaItems.Any(mi => mi.FilePath == mediaItemAction.MediaItemFile))   // Media item state changed
        //            {
        //                LoadMediaItemCollectionDefaults(_selectedArtist, _selectedMediaItemCollection);
        //            }
        //            else   // No state changed
        //            {
        //                // Refresh actions
        //                LoadMediaItemActions(_selectedMediaItem);
        //            }
        //        }
        //    }
        //}

        ///// <summary>
        ///// Executes action to open media item collection for media item. User was typically viewing playlist or queue
        ///// and clicked action "Open album [X]"
        ///// </summary>
        ///// <param name="mediaItem"></param>
        ///// <param name="mediaItemAction"></param>
        ///// <param name="mediaSource"></param>
        //private void ExecuteOpenMediaItemCollection(MediaItem mediaItem, MediaAction mediaItemAction, IMediaSource mediaSource)
        //{
        //    // Get Artist & MediaItemCollection for media item
        //    var ancestors = mediaSource.GetAncestorsForMediaItem(mediaItem).FirstOrDefault();
        //    if (ancestors != null)
        //    {
        //        Reset(mediaSource.MediaLocation.Name,
        //                    ancestors.Item1.Name,   // Artist
        //                    ancestors.Item2.Name,   // Media item collection
        //                    mediaItem.Name);
        //    }
        //}

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

                System.Diagnostics.Debug.WriteLine($"Search for {text} found {results.Count} items");
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
        /// Selects search result
        /// </summary>
        /// <param name="searchResult"></param>
        public void SelectSearchResult(SearchResult searchResult)
        {
            // Get media location
            var mediaLocation = _mediaLocations.First(ml => ml.Name == searchResult.MediaLocationName);

            // Select media location
            SelectedMediaLocation = mediaLocation;

            // Select relevant options. User may have selected artist, media item collection or media item
            switch (searchResult.EntityType)
            {
                case EntityTypes.Artist:
                    // Display media item collections for artist
                    SelectedArtist = _artists.First(a => a.Name == searchResult.Artist!.Name);
                    break;
                case EntityTypes.MediaItem:
                    // Display media item for media item collection
                    SelectedArtist = _artists.First(a => a.Name == searchResult.Artist!.Name);
                    SelectedMediaItemCollection = _mediaItemCollections.First(mic => mic.Name == searchResult.MediaItemCollection!.Name);
                    SelectedMediaItem = _mediaItems.First(mi => mi.Name == searchResult.MediaItem!.Name);
                    break;
                case EntityTypes.MediaItemCollection:
                    // Display media items for media item collection
                    SelectedArtist = _artists.First(a => a.Name == searchResult.Artist!.Name);
                    SelectedMediaItemCollection = _mediaItemCollections.First(mic => mic.Name == searchResult.MediaItemCollection!.Name);
                    break;
            }
        }

        //public void ApplyEqualizerTest()
        //{
        //    var url = "https://stream.rcs.revma.com/muddaykbyk3vv";
        //    _mediaPlayer.Play(url, (exception) =>
        //    {
        //        int xxx = 1000;
        //    });

        //    //_mediaPlayer.ApplyEqualizerTest();
        //    //int xxx = 1000;
        //}
      
        /// <summary>
        /// Resets UI state. Selects requested items, if not available then selects default.   
        /// 
        /// By default then if item is already selected then it won't be selected again (Selecting item refreshes child
        /// items.) The isForce[Something] parameter allows caller to force selection even if already selected. E.g. If
        /// new item added to playlist then caller can force selection of same media item collection (The playlist) so
        /// that the media items are refreshed.
        /// </summary>
        /// <param name="selectedMediaLocationName">Media location to select ("": Any)</param>
        /// <param name="isForceSelectMediaLocation">Whether to force media location select even if already selected and refresh child items</param>
        /// <param name="selectedArtistName">Artist to select ("": Any)</param>
        /// <param name="isForceSelectArtist">Whether to force artist select even if already selected and refresh child items</param>
        /// <param name="selectedMediaItemCollectionName">Media item collection to select ("": Any)</param>
        /// <param name="isForceSelectMediaItemCollection">Whether to force media item collection select even if already selected and refresh child items</param>        
        /// <param name="selectedMediaItemName">Media item to select ("": Any)</param>        
        /// <param name="isForceSelectMediaItem">Whether to force media item select even if already selected and refresh child items</param>
        private void Reset(string? selectedMediaLocationName,
                            bool isForceSelectMediaLocation,
                            string? selectedArtistName,
                            bool isForceSelectArtist,
                            string? selectedMediaItemCollectionName,
                            bool isForceSelectMediaItemCollection,
                            string? selectedMediaItemName,
                            bool isForceSelectMediaItem)
        {
            _isResetActive = true;

            // Clear selected media item location
            //SelectedMediaLocation = null;

            // Select media location, does nothing if already selected
            SelectMediaLocation(selectedMediaLocationName, isForceSelectMediaLocation);

            // Select artist, does nothing if already selected
            SelectArtist(selectedArtistName, isForceSelectArtist);

            // Select media item collection, does nothing if already selected
            SelectMediaItemCollection(selectedMediaItemCollectionName, isForceSelectMediaItemCollection);

            // Select media item, does nothing if already selected
            SelectMediaItem(selectedMediaItemName, isForceSelectMediaItem);

            //OnPropertyChanged(nameof(SelectedMediaLocation));
            _isResetActive = false;
        }

        private void SelectMediaLocation(string? selectedMediaLocationName, bool isForce)
        {
            // Check if already selected
            if (!isForce)
            {
                if (!String.IsNullOrEmpty(selectedMediaLocationName) &&
                    SelectedMediaLocation != null &&
                    SelectedMediaLocation.Name == selectedMediaLocationName)
                {
                    return;
                }
            }

            LoadMediaLocationsToDisplayInUI();

            // Set media location. When the property is set then we load the child items and set a default selected child
            // item and we repeat this until the lowest level (Media item actions)
            SelectedMediaLocation = MediaLocations.First(ml => ml.Name == selectedMediaLocationName);
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
        /// Whether the Shuffle Play switch is visible
        /// </summary>
        public bool IsShufflePlayVisible
        {
            get
            {
                return SelectedMediaSource != null && SelectedMediaSource.IsShufflePlayAllowed;                
            }
        }

        /// <summary>
        /// Whether Auto-Play Next switch is visible
        /// </summary>
        public bool IsAutoPlayNextVisible
        {
            get
            {
                return SelectedMediaSource != null && SelectedMediaSource.IsAutoPlayNextAllowed;                
            }
        }

        /// <summary>
        /// Executes media action. May trigger event from ICurrentState.Events which does more processing.
        /// </summary>
        /// <param name="mediaAction"></param>
        public void ExecuteMediaAction(MediaAction mediaAction)
        {            
            if (mediaAction != null)
            {
                // Get media source to execute action against
                var mediaSource = _mediaSourceService.GetAll().First(ms => ms.MediaLocation.Name == mediaAction.MediaLocationName);

                // Execute action. E.g. Clear queue
                mediaSource.ExecuteMediaAction(mediaAction);
            }
        }

        private Artist GetArtistToSelect(List<Artist> artists, string? name)
        {
            Artist? artist = String.IsNullOrEmpty(name) ? null : artists.FirstOrDefault(a => a.Name == name);
            if (artist == null)
            {
                foreach (EntityCategory entityCategory in new[] { EntityCategory.Real, EntityCategory.All, EntityCategory.None })
                {
                    artist = artists.FirstOrDefault(a => a.EntityCategory == entityCategory);
                    if (artist != null) break;
                }
            }
            if (artist == null) artist = artists.First();
           return artist;
        }

        private MediaItemCollection GetMediaItemCollectionToSelect(List<MediaItemCollection> mediaItemCollections, string? name)
        {
            MediaItemCollection? mediaItemCollection = String.IsNullOrEmpty(name) ? null : mediaItemCollections.FirstOrDefault(a => a.Name == name);
            if (mediaItemCollection == null)
            {
                foreach (EntityCategory entityCategory in new[] { EntityCategory.Real, EntityCategory.All, EntityCategory.None })
                {
                    mediaItemCollection = mediaItemCollections.FirstOrDefault(a => a.EntityCategory == entityCategory);
                    if (mediaItemCollection != null) break;
                }
            }
            if (mediaItemCollection == null) mediaItemCollection = mediaItemCollections.First();
            return mediaItemCollection;
        }

        private MediaItem GetMediaItemToSelect(List<MediaItem> mediaItems, string? name)
        {
            MediaItem? mediaItem = String.IsNullOrEmpty(name) ? null : mediaItems.FirstOrDefault(a => a.Name == name);
            if (mediaItem == null)
            {
                foreach (EntityCategory entityCategory in new[] { EntityCategory.Real, EntityCategory.All, EntityCategory.None })
                {
                    mediaItem = mediaItems.FirstOrDefault(a => a.EntityCategory == entityCategory);
                    if (mediaItem != null) break;
                }
            }
            if (mediaItem == null) mediaItem = mediaItems.First();
            return mediaItem;
        }

        public void PlayMediaItemAsCurrent(MediaItem mediaItem)
        {
            _currentState.Events.RaiseOnPlayMediaItem(mediaItem);
        }
    }
}
