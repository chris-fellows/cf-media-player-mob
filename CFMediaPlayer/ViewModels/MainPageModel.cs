using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows.Input;
using CFMediaPlayer.Enums;
using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;
using CFMediaPlayer.Services;
using CFMediaPlayer.Sources;
using Java.Util.Concurrent.Locks;
using static Java.Util.Jar.Attributes;

namespace CFMediaPlayer.ViewModels
{
    /// <summary>
    /// View model for main page
    /// </summary>
    public class MainPageModel : INotifyPropertyChanged
    {
        private System.Timers.Timer _elapsedTimer;
        //private System.Timers.Timer _refreshPlaylistsTimer;

        public event PropertyChangedEventHandler? PropertyChanged;
        
        private Action<string>? _debugAction;        

        private NameValuePair<MediaPlayModes> _selectedPlayMode;

        private IMediaPlayer _mediaPlayer { get; set; }

        private readonly IMediaLocationService _mediaLocationService;
        private readonly IMediaSearchService _mediaSearchService;
        private readonly IMediaSourceService _mediaSourceService;
        private readonly IUIThemeService _uiThemeService;
        private readonly IUserSettingsService _userSettingsService; 

        //private List<IMediaSource> _mediaSources { get; set; }
        private List<MediaLocation> _mediaLocations;
        private List<Artist> _artists = new List<Artist>();
        private List<MediaItemCollection> _mediaItemCollections = new List<MediaItemCollection>();
        private List<MediaItem> _mediaItems = new List<MediaItem>();

        private List<NameValuePair<MediaPlayModes>> _playModes = new List<NameValuePair<MediaPlayModes>>();
        private List<MediaItemAction> _mediaItemActions = new List<MediaItemAction>();

        private UITheme _uiTheme;

        public MainPageModel(IMediaLocationService mediaLocationService,
                         IMediaPlayer mediaPlayer, 
                         IMediaSearchService mediaSearchService,
                         IMediaSourceService mediaSourceService,
                         IUIThemeService uiThemeService,
                        IUserSettingsService userSettingsService)
        {
            _mediaLocationService = mediaLocationService;
            _mediaSearchService = mediaSearchService;
            _uiThemeService = uiThemeService;
            _userSettingsService = userSettingsService;

            _mediaPlayer = mediaPlayer;
            _mediaSourceService = mediaSourceService;            

            //foreach (var mediaLocation in _mediaLocations)
            //{
            //    _mediaSources.First(s => s.MediaSourceType == mediaLocation.MediaSourceType).SetSource(mediaLocation.Source);
            //}

            // Handle status change
            _mediaPlayer.SetStatusAction(OnMediaItemStatusChange);

            _mediaPlayer.SetDebugAction((action) => { });

            // Set timer for elapsed play time
            _elapsedTimer = new System.Timers.Timer();
            _elapsedTimer.Elapsed += _timer_Elapsed;
            _elapsedTimer.Interval = 1000;
            _elapsedTimer.Enabled = false;            

            // Set commands
            NextCommand = new Command(DoNext);
            PrevCommand = new Command(DoPrev);
            PlayOrPauseCommand = new Command(DoPlayOrPause);
            StopCommand = new Command(DoStop);
            ExecuteMediaItemActionCommand = new Command(ExecuteMediaItemAction);
            
            _playModes = new List<NameValuePair<MediaPlayModes>>()
            {
                new NameValuePair<MediaPlayModes>() { Name = "Single play", Value = Enums.MediaPlayModes.SingleMediaItem },
                new NameValuePair<MediaPlayModes>() { Name = "Sequential play", Value = Enums.MediaPlayModes.Sequential },
                new NameValuePair<MediaPlayModes>() { Name = "Shuffle (Album)", Value = Enums.MediaPlayModes.ShuffleMediaItemCollection },
                new NameValuePair<MediaPlayModes>() { Name = "Shuffle (Artist)", Value = Enums.MediaPlayModes.ShuffleArtist },
                new NameValuePair<MediaPlayModes>() { Name = "Shuffle (Storage)", Value = Enums.MediaPlayModes.ShuffleStorage }                
            };

            // Set default play mode
            SelectedPlayMode = _playModes.First();

            // Load available media locations
            LoadAvailableMediaLocations();

            // Get UI theme
            var userSettings = _userSettingsService.GetByUsername(Environment.UserName);
            _uiTheme = _uiThemeService.GetAll().First(t => t.Id == userSettings.UIThemeId);            
        }
       
        /// <summary>
        /// Command to play previous media item
        /// </summary>
        public ICommand PrevCommand { get; set; }

        /// <summary>
        /// Command to play next media item
        /// </summary>
        public ICommand NextCommand { get; set; }

        /// <summary>
        /// Command to play or pause current media item
        /// </summary>
        public ICommand PlayOrPauseCommand { get; set; }

        /// <summary>
        /// Command to stop playing or pausing current media item
        /// </summary>
        public ICommand StopCommand { get; set; }

        public ICommand ExecuteMediaItemActionCommand { get; set; }    

        /// <summary>
        /// Selected play mode
        /// </summary>
        public NameValuePair<MediaPlayModes> SelectedPlayMode
        {
            get { return _selectedPlayMode; }
            set
            {
                _selectedPlayMode = value;
            }
        }

        public List<NameValuePair<MediaPlayModes>> PlayModes
        {
            get { return _playModes; }
        }

        private IMediaSource? CurrentMediaSource
        {
            get
            {
                return _selectedMediaLocation != null ?
                    _mediaSourceService.GetAll().First(ms => ms.MediaLocation.Name == _selectedMediaLocation.Name) :
                    null;                
            }
        }
      
        private void LoadAvailableMediaLocations()
        {
            _mediaLocations = null;
            _mediaLocations = _mediaLocationService.GetAll().Where(ml =>
                    _mediaSourceService.GetAll().First(ms => ms.MediaLocation.Name == ml.Name).IsAvailable).ToList();
        }

        private void ClearMediaItems()
        {
            _mediaItems.Clear();
            _selectedMediaItem = null;

            OnPropertyChanged(nameof(MediaItems));
            OnPropertyChanged(nameof(SelectedMediaItem));
        }

        private void ClearMediaItemCollections()
        {
            _mediaItemCollections.Clear();
            _selectedMediaItemCollection = null;

            OnPropertyChanged(nameof(MediaItemCollections));
            OnPropertyChanged(nameof(SelectedMediaItemCollection));
        }

        private void ClearArtists()
        {
            _artists.Clear();
            _selectedArtist = null;

            OnPropertyChanged(nameof(Artists));
            OnPropertyChanged(nameof(SelectedArtist));
        }

        private void ClearPlaylistsActionsForMediaItem()
        {
            _mediaItemActions.Clear();
            _selectedMediaItemAction = null;

            OnPropertyChanged(nameof(MediaItemActions));
            OnPropertyChanged(nameof(SelectedMediaItemAction));
        }

        private MediaLocation? _selectedMediaLocation;
        public MediaLocation SelectedMediaLocation
        {
            get
            {
                return _selectedMediaLocation;
            }

            set
            {
                _selectedMediaLocation = value;

                // Display artists for media source
                if (_selectedMediaLocation != null)
                {
                    var isExists = Directory.Exists(_selectedMediaLocation.Source);
                    if (_debugAction != null) _debugAction($"MediaLocation={_selectedMediaLocation.Source}, Exists={isExists}");

                    // Set source location for media source to read
                    //CurrentMediaSource!.SetSource(_selectedMediaLocation.Source);

                    LoadMediaLocationDefaults();
                }
                else
                {
                    if (_debugAction != null) _debugAction("MediaLocation=None");
                }

                // Child items
                OnPropertyChanged(nameof(Artists));
                OnPropertyChanged(nameof(SelectedArtist));
                OnPropertyChanged(nameof(MediaItemCollections));
                OnPropertyChanged(nameof(SelectedMediaItemCollection));
                OnPropertyChanged(nameof(MediaItems));
                OnPropertyChanged(nameof(SelectedMediaItem));

                // Player buttons
                OnPropertyChanged(nameof(IsNextEnabled));
                OnPropertyChanged(nameof(IsPrevEnabled));
                OnPropertyChanged(nameof(IsPlaying));
                OnPropertyChanged(nameof(IsMediaItemSelected));
                OnPropertyChanged(nameof(PlayButtonImageSource));

                OnPropertyChanged(nameof(IsMediaItemActionsEnabled));
            }
        }

        /// <summary>
        /// Loads action for media item
        /// </summary>
        /// <param name="mediaItem"></param>
        private void LoadMediaItemActions(MediaItem mediaItem)
        {
            SelectedMediaItemAction = null;

            _mediaItemActions.Clear();
            _mediaItemActions = new();

            /*
            OnPropertyChanged(nameof(NewMediaItemActions));
            OnPropertyChanged(nameof(SelectedMediaItemAction));
            */

            // Get media item actions
            // TODO: Clean this up. It's confusing how it works
            var mediaItemActions = new List<MediaItemAction>();
            switch (CurrentMediaSource!.MediaLocation.MediaSourceType)
            {
                //case MediaSourceTypes.Playlist:
                //    mediaItemActions.AddRange(_mediaSources.First(ms => ms.MediaSourceType == MediaSourceTypes.Playlist)
                //                .GetActionsForMediaItem(mediaItem));                    
                //    break;
                case MediaSourceTypes.Queue:
                    // Bit of a hack. Just adds the "Clear queue" item
                    mediaItemActions.AddRange(_mediaSourceService.GetAll().First(ms => ms.MediaLocation.MediaSourceType == MediaSourceTypes.Queue)
                                .GetActionsForMediaItem(null));                    
                    break;
                case MediaSourceTypes.Storage:
                    // Add playlist actions
                    mediaItemActions.AddRange(_mediaSourceService.GetAll().First(ms => ms.MediaLocation.MediaSourceType == MediaSourceTypes.Playlist)
                                .GetActionsForMediaItem(mediaItem));

                    // Add queue actions
                    mediaItemActions.AddRange(_mediaSourceService.GetAll().First(ms => ms.MediaLocation.MediaSourceType == MediaSourceTypes.Queue)
                                .GetActionsForMediaItem(mediaItem));
                    break;
            }         
            if (!mediaItemActions.Any())
            {
                var itemNone = new MediaItemAction()
                {
                    Name = LocalizationResources.Instance["None"].ToString()
                };
                mediaItemActions.Add(itemNone);
            }
            _mediaItemActions.AddRange(mediaItemActions);            

            // Select default playlist action
            var action = _mediaItemActions.FirstOrDefault(p => !String.IsNullOrEmpty(p.File));
            if (action == null)
            {
                action = _mediaItemActions.First(p => String.IsNullOrEmpty(p.File));
            }
            SelectedMediaItemAction = action;

            OnPropertyChanged(nameof(MediaItemActions));
            OnPropertyChanged(nameof(SelectedMediaItemAction));
        }

        private MediaItem? _selectedMediaItem;
        public MediaItem? SelectedMediaItem
        {
            get
            {
                return _selectedMediaItem;
            }

            set
            {
                _selectedMediaItem = value;

                // Stop current media
                var isWasPlaying = IsPlaying;
                if (isWasPlaying || IsPaused)
                {
                    Stop();
                }
                
                if (_selectedMediaItem != null)
                {
                    // Set actions for media item (Add to playlist X etc)
                    LoadMediaItemActions(_selectedMediaItem);

                    // Respect previous play state. Only play if was playing. Don't play if was paused or stopped.
                    if (isWasPlaying)
                    {
                        PlayMediaItem(_selectedMediaItem);
                    }
                }

                // Player buttons
                OnPropertyChanged(nameof(IsNextEnabled));
                OnPropertyChanged(nameof(IsPrevEnabled));
                OnPropertyChanged(nameof(IsPlaying));
                OnPropertyChanged(nameof(IsMediaItemSelected));
                OnPropertyChanged(nameof(PlayButtonImageSource));                
            }
        }

        private MediaItemCollection? _selectedMediaItemCollection;
        public MediaItemCollection? SelectedMediaItemCollection
        {
            get
            {
                return _selectedMediaItemCollection;
            }

            set
            {
                _selectedMediaItemCollection = value;

                // Display media items for album, select default
                if (_selectedMediaItemCollection != null)
                {
                    // Display New Playlist page if selected
                    if (_selectedMediaItemCollection.Name.Equals(LocalizationResources.Instance["NewPlaylistText"].ToString()))
                    {
                        Shell.Current.GoToAsync($"//{nameof(NewPlaylistPage)}");
                    }

                    LoadMediaItemCollectionDefaults(_selectedArtist, _selectedMediaItemCollection);                    
                }

                OnPropertyChanged(nameof(MainLogoImage));

                // Child items
                OnPropertyChanged(nameof(MediaItems));
                OnPropertyChanged(nameof(SelectedMediaItem));

                // Player buttons
                OnPropertyChanged(nameof(IsNextEnabled));
                OnPropertyChanged(nameof(IsPrevEnabled));
                OnPropertyChanged(nameof(IsPlaying));
                OnPropertyChanged(nameof(IsMediaItemSelected));
                OnPropertyChanged(nameof(PlayButtonImageSource));                
            }
        }

        /// <summary>
        /// Main logo image to display. Either selected album art or default image.
        /// </summary>
        public string MainLogoImage
        {
            get
            {
                if (_selectedMediaItemCollection != null && !string.IsNullOrEmpty(_selectedMediaItemCollection.ImagePath))
                {
                    return _selectedMediaItemCollection.ImagePath;
                }
                return "dotnet_bot.png";
            }
        }

        private Artist? _selectedArtist;
        public Artist? SelectedArtist
        {
            get
            {
                return _selectedArtist;
            }

            set
            {
                _selectedArtist = value;              

                // Display albums & media items for artist, select default
                if (_selectedArtist != null)
                {                                       
                    LoadArtistDefaults(_selectedArtist);
                }

                // Child items
                OnPropertyChanged(nameof(MediaItemCollections));
                OnPropertyChanged(nameof(SelectedMediaItemCollection));
                OnPropertyChanged(nameof(MediaItems));
                OnPropertyChanged(nameof(SelectedMediaItem));

                // Player buttons
                OnPropertyChanged(nameof(IsNextEnabled));
                OnPropertyChanged(nameof(IsPrevEnabled));
                OnPropertyChanged(nameof(IsPlaying));
                OnPropertyChanged(nameof(IsMediaItemSelected));
                OnPropertyChanged(nameof(PlayButtonImageSource));                
            }
        }

        /// <summary>
        /// Loads the default artist for media location (First artist selected)
        /// </summary>
        private void LoadMediaLocationDefaults()
        {
            ClearArtists();
            ClearMediaItemCollections();
            ClearMediaItems();
            ClearPlaylistsActionsForMediaItem();

            // Do nothing if media source not available (E.g. SD card removed)
            if (CurrentMediaSource == null || !CurrentMediaSource.IsAvailable)
            {
                return;
            }

            // Get artists
            LoadArtists();
            if (Artists.Any())
            {
                SelectedArtist = Artists[0];
            }
        }

        /// <summary>
        /// Loads the default media item collections and media items for artist (First album selected)
        /// </summary>
        /// <param name="artist"></param>
        private void LoadArtistDefaults(Artist artist)
        {
            ClearMediaItemCollections();
            ClearMediaItems();
            ClearPlaylistsActionsForMediaItem();

            LoadAlbumsForArtist(artist.Name);
            if (MediaItemCollections.Any())
            {
                SelectedMediaItemCollection = MediaItemCollections[0];
            }
        }

        /// <summary>
        /// Loads the default media items for the artist and album (First media item selected)
        /// </summary>
        /// <param name="artist"></param>
        /// <param name="album"></param>
        private void LoadMediaItemCollectionDefaults(Artist artist, MediaItemCollection album)
        {            
            LoadMediaItems(artist.Name, album.Name);            
            if (MediaItems.Any())
            {
                SelectedMediaItem = MediaItems[0];
            }
        }

        /// <summary>
        /// Plays next media item
        /// </summary>
        /// <param name="parameter"></param>
        private void DoNext(object parameter)
        {
            var index = _mediaItems.IndexOf(SelectedMediaItem!);
            SelectedMediaItem = _mediaItems[index + 1];

            OnPropertyChanged(nameof(SelectedMediaItem));

            PlayMediaItem(SelectedMediaItem);
        }

        /// <summary>
        /// Plays previous media item
        /// </summary>
        /// <param name="parameter"></param>
        private void DoPrev(object parameter)
        {
            var index = _mediaItems.IndexOf(SelectedMediaItem!);
            SelectedMediaItem = _mediaItems[index - 1];

            OnPropertyChanged(nameof(SelectedMediaItem));

            PlayMediaItem(SelectedMediaItem);
        }

        /// <summary>
        /// Stops playing current media item
        /// </summary>
        /// <param name="parameter"></param>
        private void DoStop(object parameter)
        {
            Stop();
        }

        /// <summary>
        /// Plays or pauses current media item
        /// </summary>
        /// <param name="parameter"></param>
        private void DoPlayOrPause(object parameter)
        {
            if (IsPlaying)
            {
                Pause();
            }
            else
            {
                PlayMediaItem(SelectedMediaItem!);
            }
        }

        private void PlayMediaItem(MediaItem mediaItem)
        {
            PlayAudio(mediaItem.FilePath,
                (exception) =>
                {
                    //StatusLabel.Text = exception.Message;
                });
        }

        private void LoadMediaItems(string artistName, string albumName)
        {
            _mediaItems = CurrentMediaSource!.GetMediaItemsForMediaItemCollection(artistName, albumName);
            
            OnPropertyChanged(nameof(MediaItems));
            OnPropertyChanged(nameof(IsMediaItemSelected));            
        }

        private void LoadArtists()
        {
            _artists = CurrentMediaSource!.GetArtists();

            OnPropertyChanged(nameof(Artists));            
        }

        private void LoadAlbumsForArtist(string artistName)
        {
            _mediaItemCollections = CurrentMediaSource!.GetMediaItemCollectionsForArtist(artistName);
            
            OnPropertyChanged(nameof(MediaItemCollections));            
        }

        public IList<MediaLocation> MediaLocations
        {
            get { return _mediaLocations; }
        }

        public IList<MediaItem> MediaItems
        {
            get
            {
                return _mediaItems;
            }
        }

        public IList<MediaItemCollection> MediaItemCollections
        {
            get { return _mediaItemCollections; }
        }

        public IList<Artist> Artists
        {
            get { return _artists; }
        }    

        public IList<MediaItemAction> MediaItemActions
        {
            get { return _mediaItemActions; }
        }

        public void SetDebugAction(Action<string> debugAction)
        {
            _debugAction = debugAction;
        }

        private bool _isTimerEvent = false;
        private void _timer_Elapsed(object? sender, ElapsedEventArgs e)
        {            
            // TODO: Consider disabling timer when app not visible
            OnPropertyChanged(nameof(ElapsedTime));            
            OnPropertyChanged(nameof(ElapsedTimeInt));
            OnPropertyChanged(nameof(DurationRemainingTime));
        }        

        public void OnPropertyChanged([CallerMemberName] string name = "") => 
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public void PlayAudio(string filePath,
                            Action<Exception> errorAction)
        {
            _mediaPlayer.PlayAudio(filePath, errorAction);            
        }

        public string ElapsedTime
        {
            get
            {
                if (_mediaPlayer.IsPlaying || _mediaPlayer.IsPaused)
                {
                    var duration = _mediaPlayer.GetElapsedPlayTime();
                    var durationString = string.Format("{0:00}", duration.Hours) + ":" +
                           string.Format("{0:00}", duration.Minutes) + ":" +
                           string.Format("{0:00}", duration.Seconds);
                    return durationString;
                }

                return "00:00:00";
            }
        }

        public string DurationRemainingTime
        {
            get
            {
                if (_mediaPlayer.IsPlaying || _mediaPlayer.IsPaused)
                {
                    var duration = _mediaPlayer.GetTotalDuration() - _mediaPlayer.GetElapsedPlayTime();
                    var durationString = string.Format("{0:00}", duration.Hours) + ":" +
                           string.Format("{0:00}", duration.Minutes) + ":" +
                           string.Format("{0:00}", duration.Seconds);
                    return durationString;
                }

                return "";
            }
        }

        /// <summary>
        /// Duration of current media item
        /// </summary>
        public int DurationInt
        {
            get
            {
                if (_mediaPlayer.IsPlaying || _mediaPlayer.IsPaused)
                {
                    return (int)_mediaPlayer.GetTotalDuration().TotalMilliseconds;
                }
                return 1000;    // Any non-zero value
            }
        }      

        /// <summary>
        /// Elapsed time of current media item
        /// </summary>
        public int ElapsedTimeInt
        {
            get
            {
                if (_mediaPlayer.IsPlaying || _mediaPlayer.IsPaused)
                {
                    return (int)_mediaPlayer.GetElapsedPlayTime().TotalMilliseconds;
                }
                return 0;
            }

            set
            {
                if (_mediaPlayer.IsPlaying || _mediaPlayer.IsPaused)
                {                    
                    _mediaPlayer.SetElapsedPlayTime(TimeSpan.FromMilliseconds(value));
                }             
            }
        }

        /// <summary>
        /// Whether media item is playing
        /// </summary>
        public bool IsPlaying
        {
            get { return _mediaPlayer.IsPlaying; }
        }

        /// <summary>
        /// Whether media item is paused
        /// </summary>
        public bool IsPaused
        {
            get { return _mediaPlayer.IsPaused; }
        }

        /// <summary>
        /// Pauses media item
        /// </summary>
        public void Pause()
        {
            _mediaPlayer.Pause();
            _elapsedTimer.Enabled = false;
        }

        /// <summary>
        /// Stops media item
        /// </summary>
        public void Stop()
        {
            _mediaPlayer.Stop();
            _elapsedTimer.Enabled = false;
        }

        /// <summary>
        /// Image source for the play button (Play/Pause)
        /// </summary>
        public string PlayButtonImageSource
        {
            get
            {
                return IsPlaying switch
                {
                    true => "ic_media_pause.png",
                    _ => "ic_media_play.png"
                };
            }
        }

        /// <summary>
        /// Plays next media item if configured
        /// </summary>
        private void PlayNextMediaItemIfConfigured()
        {
            switch (_selectedPlayMode.Value)
            {
                //case Enums.PlayModes.SingleMediaItem:
                //    break;
                case Enums.MediaPlayModes.Sequential:
                    if (SelectedMediaItem != _mediaItems.Last())
                    {
                        PlayMediaItem(_mediaItems[_mediaItems.IndexOf(SelectedMediaItem) + 1]);
                    }
                    break;
                case Enums.MediaPlayModes.ShuffleMediaItemCollection:
                    {
                        int mediaItemIndex = new Random().Next(0, _mediaItems.Count - 1);   // TODO: Keep track of which items played
                        PlayMediaItem(_mediaItems[mediaItemIndex]);
                    }
                    break;
                case Enums.MediaPlayModes.ShuffleArtist:
                    {
                        // Select random media item collection
                        int mediaCollectionIndex = new Random().Next(0, _mediaItemCollections.Count - 1);
                        SelectedMediaItemCollection = _mediaItemCollections[mediaCollectionIndex];

                        // Select random media item
                        int mediaItemIndex = new Random().Next(0, _mediaItems.Count - 1);   // TODO: Keep track of which items played
                        PlayMediaItem(_mediaItems[mediaItemIndex]);
                    }
                    break;
                case Enums.MediaPlayModes.ShuffleStorage:
                    {
                        // Select random artist
                        int artistIndex = new Random().Next(0, _artists.Count - 1);
                        SelectedArtist = _artists[artistIndex];

                        // Select random media item collection
                        int mediaCollectionIndex = new Random().Next(0, _mediaItemCollections.Count - 1);
                        SelectedMediaItemCollection = _mediaItemCollections[mediaCollectionIndex];

                        // Select random media item
                        int mediaItemIndex = new Random().Next(0, _mediaItems.Count - 1);   // TODO: Keep track of which items played
                        PlayMediaItem(_mediaItems[mediaItemIndex]);
                    }
                    break;
            }            
        }
     
        /// <summary>
        /// Handles media player status change
        /// </summary>
        /// <param name="status"></param>
        private void OnMediaItemStatusChange(MediaPlayerStatuses status)
        {
            System.Diagnostics.Debug.WriteLine($"{DateTimeOffset.UtcNow.ToString()} OnMediaItemStatusChange: {status}");

            switch (status)
            {
                case MediaPlayerStatuses.Completed:
                    OnPropertyChanged(nameof(IsPlaying));
                    OnPropertyChanged(nameof(PlayButtonImageSource));                                        
                    OnPropertyChanged(nameof(DurationInt));
                    OnPropertyChanged(nameof(ElapsedTime));
                    OnPropertyChanged(nameof(ElapsedTimeInt));

                    _elapsedTimer.Enabled = false;

                    // Auto-play nextm media item
                    /*
                    if (AutoPlayNext && SelectedMediaItem != _mediaItems.Last())
                    {
                        PlayMediaItem(_mediaItems[_mediaItems.IndexOf(SelectedMediaItem) + 1]);
                    }
                    */

                    PlayNextMediaItemIfConfigured();

                    break;
                case MediaPlayerStatuses.Paused:
                    OnPropertyChanged(nameof(IsPlaying));
                    OnPropertyChanged(nameof(PlayButtonImageSource));                    
                    OnPropertyChanged(nameof(DurationInt));
                    OnPropertyChanged(nameof(ElapsedTime));
                    OnPropertyChanged(nameof(ElapsedTimeInt));

                    _elapsedTimer.Enabled = false;
                    break;
                case MediaPlayerStatuses.Playing:
                    OnPropertyChanged(nameof(IsPlaying));
                    OnPropertyChanged(nameof(PlayButtonImageSource));                    
                    OnPropertyChanged(nameof(DurationInt));
                    OnPropertyChanged(nameof(ElapsedTime));
                    OnPropertyChanged(nameof(ElapsedTimeInt));

                    _elapsedTimer.Enabled = true;
                    break;                
                case MediaPlayerStatuses.Stopped:
                    OnPropertyChanged(nameof(IsPlaying));
                    OnPropertyChanged(nameof(PlayButtonImageSource));                    
                    OnPropertyChanged(nameof(DurationInt));
                    OnPropertyChanged(nameof(ElapsedTime));
                    OnPropertyChanged(nameof(ElapsedTimeInt));

                    _elapsedTimer.Enabled = false;
                    break;
            }

            if (_debugAction != null) _debugAction($"Status={status}");
        }

        public bool IsMediaItemActionsEnabled
        {
            get { return _selectedMediaLocation != null && 
                        _selectedMediaLocation.MediaSourceType != MediaSourceTypes.Playlist; }
        }

        /// <summary>
        /// Whether 'previous media item' is enabled
        /// </summary>
        public bool IsPrevEnabled
        {
            get
            {
                if (SelectedMediaItem != null)
                {
                    var index = _mediaItems.IndexOf(SelectedMediaItem);
                    return index > 0;
                }
                return false;
            }
        }

        /// <summary>
        /// Whether 'next media item' is enabled
        /// </summary>
        public bool IsNextEnabled
        {
            get
            {
                if (SelectedMediaItem != null)
                {
                    var index = _mediaItems.IndexOf(SelectedMediaItem);
                    return index < _mediaItems.Count - 1;
                }
                return false;
            }
        }

        /// <summary>
        /// Whether media item is selected
        /// </summary>
        public bool IsMediaItemSelected
        {
            get
            {
                return SelectedMediaItem != null;
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

            // Select playlist
            var mediaItemCollection = MediaItemCollections.First(mic => mic.Name == playlistName);
            SelectedMediaItemCollection = mediaItemCollection;

            // Child items
            OnPropertyChanged(nameof(Artists));
            OnPropertyChanged(nameof(SelectedArtist));
            OnPropertyChanged(nameof(MediaItemCollections));
            OnPropertyChanged(nameof(SelectedMediaItemCollection));
            OnPropertyChanged(nameof(MediaItems));
            OnPropertyChanged(nameof(SelectedMediaItem));
        }

        /// <summary>
        /// Selected playlist action for media item
        /// </summary>
        private MediaItemAction? _selectedMediaItemAction;
        public MediaItemAction? SelectedMediaItemAction
        {
            get { return _selectedMediaItemAction;  }
            set
            {
                _selectedMediaItemAction = value;                
            }
        }
      
        /// <summary>
        /// Executes media item action
        /// </summary>
        /// <param name="parameter"></param>
        private void ExecuteMediaItemAction(object parameter)
        {                        
            if (_selectedMediaItemAction != null &&        
                _selectedMediaItemAction.Name != LocalizationResources.Instance["None"].ToString() &&
                _selectedMediaItem != null)
            {                
                _mediaSourceService.GetAll().First(ms => ms.MediaLocation.Name == _selectedMediaItemAction.MediaLocationName)
                    .ExecuteMediaItemAction(_selectedMediaItem, _selectedMediaItemAction);
             
                // Refresh actions
                LoadMediaItemActions(_selectedMediaItem);
            }
        }
        
        /// <summary>
        /// Command to execute search.
        /// </summary>
        /// <remarks>We could search all media locations but that could be slow</remarks>
        public ICommand SearchCommand => new Command<string>((string text) =>
        {
            // Set search options            
            var searchOptions = new SearchOptions() { Text = text, MediaLocations = new() { _selectedMediaLocation! } };

            // Get results
            var results = _mediaSearchService.Search(searchOptions).Result;

            SearchResults = results;
        });

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
            }
        }

        /// <summary>
        /// Selects search result
        /// </summary>
        /// <param name="name"></param>
        public void SelectSearchResult(string name)
        {
            // Get search result
            var searchResult = _searchResults.First(sr => sr.Name == name);

            // Get media location
            var mediaLocation = _mediaLocations.First(ml => ml.Name == searchResult.MediaLocationName);

            // Select media location. Sets media source
            SelectedMediaLocation = mediaLocation;

            // Select item
            switch(searchResult.EntityType)
            {
                case EntityTypes.Artist:
                    SelectedArtist = _artists.First(a => a.Name == searchResult.Artist!.Name);
                    break;
                case EntityTypes.MediaItem:
                    SelectedArtist = _artists.First(a => a.Name == searchResult.Artist!.Name);
                    SelectedMediaItemCollection = _mediaItemCollections.First(mic => mic.Name == searchResult.MediaItemCollection!.Name);
                    SelectedMediaItem = _mediaItems.First(mi => mi.Name == searchResult.MediaItem!.Name);
                    break;
                case EntityTypes.MediaItemCollection:
                    SelectedArtist = _artists.First(a => a.Name == searchResult.Artist!.Name);
                    SelectedMediaItemCollection = _mediaItemCollections.First(mic => mic.Name == searchResult.MediaItemCollection!.Name);                    
                    break;     
            }

            // Notify properties changed
            OnPropertyChanged(nameof(Artists));
            OnPropertyChanged(nameof(MediaItemCollections));
            OnPropertyChanged(nameof(MediaItems));
            OnPropertyChanged(nameof(SelectedArtist));
            OnPropertyChanged(nameof(SelectedMediaItemCollection));
            OnPropertyChanged(nameof(SelectedMediaItem));
        }        
    }
}
