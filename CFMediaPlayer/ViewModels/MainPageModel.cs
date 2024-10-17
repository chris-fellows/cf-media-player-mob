using Android.Media;
using CFMediaPlayer.Enums;
using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;
using CFMediaPlayer.Utilities;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows.Input;

namespace CFMediaPlayer.ViewModels
{
    /// <summary>
    /// View model for main page
    /// 
    /// Notes:
    /// - In the lists for artists, media item collections and media items then None is displayed rather than showing
    ///   an empty list.
    /// - Media player state is sync'd to the UI by the events from the player.
    /// - If user selects [Shuffle] for artist then media item collections contains only [Multiple] and media items
    ///   contains N media items for all artists.
    /// - If user selects [Shuffle] for media item collection then media items contains N media items for selected
    ///   artist.
    /// - We maintain the concept of real and not real artists, media item collections and media items. The not real
    ///   ones have special handling. E.g. [None], [Multiple] and [Shuffle].
    /// </summary>
    public class MainPageModel : INotifyPropertyChanged
    {
        private System.Timers.Timer _elapsedTimer;        

        public event PropertyChangedEventHandler? PropertyChanged;
        
        private Action<string>? _debugAction;        

        private NameValuePair<MediaPlayModes> _selectedPlayMode;

        private IMediaPlayer _mediaPlayer { get; set; }

        private readonly IAudioSettingsService _audioSettingsService;
        private readonly IMediaLocationService _mediaLocationService;
        private readonly IMediaSearchService _mediaSearchService;
        private readonly IMediaSourceService _mediaSourceService;
        private readonly IUIThemeService _uiThemeService;
        private readonly IUserSettingsService _userSettingsService; 
        
        private List<MediaLocation> _mediaLocations;
        private List<Artist> _artists = new List<Artist>();
        private List<MediaItemCollection> _mediaItemCollections = new List<MediaItemCollection>();
        private List<MediaItem> _mediaItems = new List<MediaItem>();

        private List<NameValuePair<MediaPlayModes>> _playModes = new List<NameValuePair<MediaPlayModes>>();
        private List<MediaItemAction> _mediaItemActions = new List<MediaItemAction>();

        private UITheme _uiTheme;
        private AudioSettings _audioSettings;
        private bool _autoPlayNext = false;

        public MainPageModel(IAudioSettingsService audioSettingsService,
                        IMediaLocationService mediaLocationService,
                         IMediaPlayer mediaPlayer, 
                         IMediaSearchService mediaSearchService,
                         IMediaSourceService mediaSourceService,
                         IUIThemeService uiThemeService,
                        IUserSettingsService userSettingsService)
        {
            _audioSettingsService = audioSettingsService;
            _mediaLocationService = mediaLocationService;
            _mediaSearchService = mediaSearchService;
            _uiThemeService = uiThemeService;
            _userSettingsService = userSettingsService;

            _mediaPlayer = mediaPlayer;
            _mediaSourceService = mediaSourceService;            

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

            // Load play modes
            foreach (MediaPlayModes mediaPlayMode in Enum.GetValues(typeof(MediaPlayModes)))
            {
                _playModes.Add(new NameValuePair<MediaPlayModes>()
                {
                    Name = LocalizationResources.Instance[InternalUtilities.GetEnumResourceKey(mediaPlayMode)].ToString(),
                    Value = mediaPlayMode
                });
            }

            // Set default play mode            
            AutoPlayNext = false;

            // Load available media locations
            LoadAvailableMediaLocations();

            // Get user settings (Theme, audio settings)
            var userSettings = _userSettingsService.GetByUsername(Environment.UserName);
            _uiTheme = _uiThemeService.GetAll().First(t => t.Id == userSettings.UIThemeId);           
            _audioSettings = _audioSettingsService.GetById(userSettings.AudioSettingsId)!;

            // Set equalizer preset
            _mediaPlayer.EqualizerPresetName = _audioSettings.PresetName;
        }

        public bool AutoPlayNext
        {
            get { return _autoPlayNext; }

            set
            {
                _autoPlayNext = value;

                OnPropertyChanged(nameof(AutoPlayNext));
            }
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

        ///// <summary>
        ///// Selected play mode
        ///// </summary>
        //public NameValuePair<MediaPlayModes> SelectedPlayMode
        //{
        //    get { return _selectedPlayMode; }
        //    set
        //    {
        //        _selectedPlayMode = value;
        //    }
        //}

        //public List<NameValuePair<MediaPlayModes>> PlayModes
        //{
        //    get { return _playModes; }
        //}

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
            var mediaLocations = _mediaLocationService.GetAll().Where(ml =>
                    _mediaSourceService.GetAll().First(ms => ms.MediaLocation.Name == ml.Name).IsAvailable).ToList();
            MediaLocations = mediaLocations;
        }

        private void ClearMediaItems()
        {
            MediaItems = new List<MediaItem>();
            SelectedMediaItem = null;
        }

        /// <summary>
        /// Notifies property changes for media item play state.
        /// </summary>
        private void NotifyPropertiesChangedForPlayState()
        {            
            OnPropertyChanged(nameof(IsPlaying));
            OnPropertyChanged(nameof(IsPaused));
            OnPropertyChanged(nameof(IsNextEnabled));
            OnPropertyChanged(nameof(IsPrevEnabled));
            OnPropertyChanged(nameof(PlayButtonImageSource));
            OnPropertyChanged(nameof(DurationInt));
            OnPropertyChanged(nameof(ElapsedTime));
            OnPropertyChanged(nameof(ElapsedTimeInt));
            OnPropertyChanged(nameof(RemainingTime));
            OnPropertyChanged(nameof(RemainingTimeInt));
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

        private void ClearActionsForMediaItem()
        {
            MediaItemActions = new List<MediaItemAction>();
            SelectedMediaItemAction = null;
        }

        private MediaLocation? _selectedMediaLocation;

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
                _selectedMediaLocation = value;

                // Notify properties on change of selected media location
                OnPropertyChanged(nameof(SelectedMediaLocation));

                // Display artists for media source
                if (_selectedMediaLocation != null)
                {
                    var isExists = Directory.Exists(_selectedMediaLocation.Source);
                    if (_debugAction != null) _debugAction($"MediaLocation={_selectedMediaLocation.Source}, Exists={isExists}");
            
                    LoadMediaLocationDefaults();
                }
                else
                {
                    if (_debugAction != null) _debugAction("MediaLocation=None");
                }
            }
        }

        /// <summary>
        /// Loads actions for media item
        /// </summary>
        /// <param name="mediaItem"></param>
        private void LoadMediaItemActions(MediaItem mediaItem)
        {
            MediaItemActions = new List<MediaItemAction>();
            SelectedMediaItemAction = null;            

            var mediaItemActions = new List<MediaItemAction>();
            if (MediaUtilities.IsRealMediaItem(mediaItem))
            {
                mediaItemActions.AddRange(CurrentMediaSource!.GetActionsForMediaItem(CurrentMediaSource.MediaLocation, mediaItem,
                                                        _mediaSourceService.GetAll()));
            }
            if (!mediaItemActions.Any())
            {
                var itemNone = new MediaItemAction()
                {
                    Name = LocalizationResources.Instance["NoneText"].ToString()
                };
                mediaItemActions.Add(itemNone);
            }            

            // Select default playlist action
            var action = mediaItemActions.FirstOrDefault(p => !String.IsNullOrEmpty(p.File));
            if (action == null)
            {
                action = mediaItemActions.First(p => String.IsNullOrEmpty(p.File));
            }

            MediaItemActions = mediaItemActions;
            SelectedMediaItemAction = action;            
        }

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
                _selectedMediaItem = value;

                // Notify properties on change of selected media item
                OnPropertyChanged(nameof(SelectedMediaItem));
                OnPropertyChanged(nameof(IsRealMediaItemSelected));
                OnPropertyChanged(nameof(IsPrevEnabled));                

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
                    if (isWasPlaying && IsRealMediaItemSelected)
                    {
                        PlayMediaItem(_selectedMediaItem);
                    }
                }

                // Notify play state changed. Can now play media item
                NotifyPropertiesChangedForPlayState();
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

                // Notify properties on change of selected media item collection
                OnPropertyChanged(nameof(SelectedMediaItemCollection));
                OnPropertyChanged(nameof(MainLogoImage));

                // Display media items for album, select default
                if (_selectedMediaItemCollection != null)
                {
                    //// Display New Playlist page if selected
                    //if (_selectedMediaItemCollection.Name.Equals(LocalizationResources.Instance["NewPlaylistText"].ToString()))
                    //{
                    //    Shell.Current.GoToAsync($"//{nameof(NewPlaylistPage)}");
                    //}

                    LoadMediaItemCollectionDefaults(_selectedArtist, _selectedMediaItemCollection);                    
                }
                
                // Player buttons                
                //NotifyPropertiesChangedForPlayState();
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
                //return "dotnet_bot.png";
                return "cassette_player_audio_speaker_sound_icon.png";
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

                // Notify properties on change of selected artist
                OnPropertyChanged(nameof(SelectedArtist));

                // Display albums & media items for artist, select default
                if (_selectedArtist != null)
                {                                       
                    LoadArtistDefaults(_selectedArtist);
                }

                // Player buttons
                //NotifyPropertiesChangedForPlayState();
            }
        }

        /// <summary>
        /// Loads the default artist for media location (First artist selected)
        /// </summary>
        private void LoadMediaLocationDefaults()
        {
            // Clear everything below media location level
            ClearArtists();
            ClearMediaItemCollections();
            ClearMediaItems();
            ClearActionsForMediaItem();
            ClearSearchResults();   // Search results are for current media location

            // Get artists (Adds None if necessary)
            LoadArtistsForMediaLocation();

            // Set default artist. Ideally real artist else none else shuffle
            var defaultArtist = Artists.FirstOrDefault(a => MediaUtilities.IsRealArtist(a));
            if (defaultArtist == null) defaultArtist = Artists.FirstOrDefault(a => MediaUtilities.IsNoneArtist(a));
            if (defaultArtist == null) defaultArtist = Artists[0];

            SelectedArtist = defaultArtist;
        }

        /// <summary>
        /// Loads the default media item collections and media items for artist
        /// </summary>
        /// <param name="artist"></param>
        private void LoadArtistDefaults(Artist artist)
        {
            // Clear everything below artist level
            ClearMediaItemCollections();
            ClearMediaItems();
            ClearActionsForMediaItem();

            // Gets media item collections (Adds None if necessary)
            LoadMediaItemCollectionsForArtist(artist);

            // Select default media item collection
            if (MediaUtilities.IsShuffleArtist(artist))   // Media item collection only contains [Multiple] if shuffle artist
            {
                SelectedMediaItemCollection = MediaItemCollections.First(mic => MediaUtilities.IsMultipleMediaItemCollection(mic));
            }
            else
            {
                // Select preferably real media item collection else none else (presumably) shuffle
                var selectedMediaItemCollection = MediaItemCollections.FirstOrDefault(mic => MediaUtilities.IsRealMediaItemCollection(mic));
                if (selectedMediaItemCollection == null) selectedMediaItemCollection = MediaItemCollections.FirstOrDefault(mic => MediaUtilities.IsNoneMediaItemCollection(mic));
                if (selectedMediaItemCollection == null) selectedMediaItemCollection = MediaItemCollections[0];

                SelectedMediaItemCollection = selectedMediaItemCollection;
            }                                
        }

        /// <summary>
        /// Loads the default media items for the artist and album (First media item selected)
        /// </summary>
        /// <param name="artist"></param>
        /// <param name="mediaItemCollection"></param>
        private void LoadMediaItemCollectionDefaults(Artist artist, MediaItemCollection mediaItemCollection)
        {            
            // Gets media items for media item collection (Adds None if necessary)            
            LoadMediaItemsForMediaItemCollection(artist, mediaItemCollection);                      

            // Select Nth media item
            SelectedMediaItem = MediaItems[0];            
        }

        /// <summary>
        /// Plays next media item
        /// </summary>
        /// <param name="parameter"></param>
        private void DoNext(object parameter)
        {
            var index = _mediaItems.IndexOf(SelectedMediaItem!);
            SelectedMediaItem = _mediaItems[index + 1];
            
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

        /// <summary>
        /// Plays media item
        /// </summary>
        /// <param name="mediaItem"></param>
        private void PlayMediaItem(MediaItem mediaItem)
        {
            if (!String.IsNullOrEmpty(mediaItem.FilePath))    // Sanity check that None not requested
            {
                _mediaPlayer.PlayAudio(mediaItem.FilePath, (exception) =>
                {
                    // TODO: Send to UI
                    System.Diagnostics.Debug.WriteLine($"Error playing audio: {exception.Message}");
                    //StatusLabel.Text = exception.Message;
                });
            }

            /*
            PlayAudio(mediaItem.FilePath,
                (exception) =>
                {
                    //StatusLabel.Text = exception.Message;
                });
            */
        }

        /// <summary>      
        /// Loads media items for artist and media item collection:
        /// - If [Shuffle] artist then loads N media items for any artist.
        /// - If [Shuffle] media item collection then loads all media items for artist.
        /// - If real media item collection then loads all media items.
        /// </summary>
        /// <param name="artist"></param>
        /// <param name="mediaItemCollection"></param>
        private void LoadMediaItemsForMediaItemCollection(Artist artist, MediaItemCollection mediaItemCollection)
        {                        
            // Get media items
            var mediaItems = CurrentMediaSource!.GetMediaItemsForMediaItemCollection(artist, mediaItemCollection, true);            
            
            // Add None if no media items
            if (!mediaItems.Any())
            {
                mediaItems.Add(new MediaItem()
                {
                    Name = LocalizationResources.Instance["NoneText"].ToString(),
                });
            }

            MediaItems = mediaItems;
        }

        /// <summary>
        /// Loads all artists for media location. Adds [Shuffle] artist.
        /// </summary>
        private void LoadArtistsForMediaLocation()
        {
            // Get all artists
            // Storage - Real, [Shuffle], [None]
            var artists = CurrentMediaSource.IsAvailable ? CurrentMediaSource!.GetArtists(true) : new List<Artist>();

            // Add None if no artists
            if (!artists.Any())
            {
                artists.Add(new Artist()
                {
                    Name = LocalizationResources.Instance["NoneText"].ToString(),
                });
            }

            Artists = artists;
        }

        /// <summary>
        /// Loads media item collections for artist. May be a real artist or shuffle artist.
        /// </summary>
        /// <param name="artist"></param>
        private void LoadMediaItemCollectionsForArtist(Artist artist)
        {                        
            var mediaItemCollections = CurrentMediaSource!.GetMediaItemCollectionsForArtist(artist, true);          

            if (!mediaItemCollections.Any())
            {
                mediaItemCollections.Add(new MediaItemCollection()
                {
                    Name = LocalizationResources.Instance["NoneText"].ToString(),
                });
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

        public List<MediaItemAction> MediaItemActions
        {
            get { return _mediaItemActions; }
            set
            {
                _mediaItemActions = value;

                OnPropertyChanged(nameof(MediaItemActions));
            }
        }

        public void SetDebugAction(Action<string> debugAction)
        {
            _debugAction = debugAction;
        }

        private bool _isTimerEvent = false;
        private void _timer_Elapsed(object? sender, ElapsedEventArgs e)
        {            
            // TODO: Consider disabling timer when app not visible

            // Notify property changes to update UI. Only need to be concerned about updating UI with progress
            // for current media item.
            OnPropertyChanged(nameof(ElapsedTime));            
            OnPropertyChanged(nameof(ElapsedTimeInt));
            OnPropertyChanged(nameof(RemainingTime));
            OnPropertyChanged(nameof(RemainingTimeInt));
        }        

        public void OnPropertyChanged([CallerMemberName] string name = "") => 
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        //private void PlayAudio(string filePath,
        //                    Action<Exception> errorAction)
        //{
        //    if (!String.IsNullOrEmpty(filePath))    // Sanity check that None not requested
        //    {
        //        _mediaPlayer.PlayAudio(filePath, errorAction);
        //    }
        //}

        public string ElapsedTime
        {
            get
            {
                if (_mediaPlayer.IsPlaying || _mediaPlayer.IsPaused)
                {
                    return GetDurationString(_mediaPlayer.GetElapsedPlayTime());                    
                }
                return GetDurationString(TimeSpan.Zero);    // "00:00:00"
            }
        }

        public string RemainingTime
        {
            get
            {
                if (_mediaPlayer.IsPlaying || _mediaPlayer.IsPaused)
                {
                    return GetDurationString(_mediaPlayer.GetTotalDuration() - _mediaPlayer.GetElapsedPlayTime());
                }
                return GetDurationString(TimeSpan.Zero);                
            }
        }

        private static string GetDurationString(TimeSpan duration)
        {
            return string.Format("{0:00}", duration.Hours) + ":" +
                          string.Format("{0:00}", duration.Minutes) + ":" +
                          string.Format("{0:00}", duration.Seconds);
        }

        public int RemainingTimeInt
        {
            get
            {
                if (_mediaPlayer.IsPlaying || _mediaPlayer.IsPaused)
                {
                    return (int)_mediaPlayer.GetTotalDuration().TotalMilliseconds - (int)_mediaPlayer.GetElapsedPlayTime().TotalMilliseconds;
                }

                return 0;
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
                //return IsPlaying switch
                //{
                //    true => "ic_media_pause.png",
                //    _ => "ic_media_play.png"
                //};
                return IsPlaying switch
                {
                    true => "audio_media_media_player_music_pause_icon.png",
                    _ => "audio_media_media_player_music_play_icon.png"
                };
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
                    NotifyPropertiesChangedForPlayState();

                    _elapsedTimer.Enabled = false;
                    
                    // Play next if configured
                    //if (_selectedPlayMode.Value == MediaPlayModes.Sequential &&
                    if (_autoPlayNext &&
                        SelectedMediaItem != _mediaItems.Last())
                    {                        
                        PlayMediaItem(_mediaItems[_mediaItems.IndexOf(SelectedMediaItem) + 1]);                        
                    }
                    break;
                case MediaPlayerStatuses.Paused:
                    NotifyPropertiesChangedForPlayState();

                    _elapsedTimer.Enabled = false;
                    break;
                case MediaPlayerStatuses.Playing:
                    NotifyPropertiesChangedForPlayState();

                    _elapsedTimer.Enabled = true;
                    break;                
                case MediaPlayerStatuses.Stopped:
                    NotifyPropertiesChangedForPlayState();

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
                if (IsRealMediaItemSelected)
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
                if (IsRealMediaItemSelected)
                {
                    var index = _mediaItems.IndexOf(SelectedMediaItem);
                    return index < _mediaItems.Count - 1;
                }
                return false;
            }
        }      
        
        /// <summary>
        /// Whether a real media item is selected rather than [None], [Multiple], [Shuffle] etc
        /// </summary>
        public bool IsRealMediaItemSelected
        {
            get
            {
                return SelectedMediaItem != null &&
                   MediaUtilities.IsRealMediaItem(SelectedMediaItem);
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
                
                OnPropertyChanged(nameof(SelectedMediaItemAction));
            }
        }
      
        /// <summary>
        /// Executes media item action
        /// </summary>
        /// <param name="parameter"></param>
        private void ExecuteMediaItemAction(object parameter)
        {                        
            if (_selectedMediaItemAction != null)                     
            {
                // Get media source to execute action against. We may be displaying the storage source but need to execute
                // 'Add to playlist X' against the playlist source.
                var mediaSource = _mediaSourceService.GetAll().First(ms => ms.MediaLocation.Name == _selectedMediaItemAction.MediaLocationName);

                // Execute action. E.g. Add to playlist X, remove from playlist Y etc
                mediaSource.ExecuteMediaItemAction(_selectedMediaItem, _selectedMediaItemAction);

                // Check if media item should still be in the current media item list. E.g. If we're displaying the playlist
                // media source and user removes selected media item then we need to select another media item
                var mediaItems = CurrentMediaSource.GetMediaItemsForMediaItemCollection(_selectedArtist, _selectedMediaItemCollection, false);
                if (!mediaItems.Any(mi => mi.FilePath == _selectedMediaItemAction.File))   // Media item state changed
                {
                    LoadMediaItemCollectionDefaults(_selectedArtist, _selectedMediaItemCollection);
                }
                else   // No state changed
                {
                    // Refresh actions
                    LoadMediaItemActions(_selectedMediaItem);
                }
            }
        }
        
        /// <summary>
        /// Command to execute search.
        /// </summary>
        /// <remarks>We could search all media locations but that could be slow</remarks>
        public ICommand SearchCommand => new Command<string>((string text) =>
        {
            System.Diagnostics.Debug.WriteLine($"Search for {text}");

            // Set search options            
            var searchOptions = new SearchOptions() { Text = text, MediaLocations = new() { _selectedMediaLocation! } };           

            // Get results
            var results = _mediaSearchService.SearchAsync(searchOptions).Result;

            SearchResults = results;

            System.Diagnostics.Debug.WriteLine($"Search for {text} found {results.Count} items");
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
            switch(searchResult.EntityType)
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

        public void ApplyEqualizerTest()
        {
            _mediaPlayer.ApplyEqualizerTest();
            int xxx = 1000;
        }
        
        /// <summary>
        /// Handles user settings updated
        /// </summary>
        public void HandleUserSettingsUpdated()
        {            
            var userSettings = _userSettingsService.GetByUsername(Environment.UserName)!;                        

            // Handle them change
            var isThemeChanged = _uiTheme.Id != userSettings.UIThemeId;
            if (isThemeChanged)
            {
                _uiTheme = _uiThemeService.GetAll().First(t => t.Id == userSettings.UIThemeId);
            }

            // Handle audio settings
            var isAudioSettingsChanged = _audioSettings.Id != userSettings.AudioSettingsId;
            if (isAudioSettingsChanged)
            {
                _audioSettings = _audioSettingsService.GetById(userSettings.AudioSettingsId)!;
                _mediaPlayer.EqualizerPresetName = _audioSettings.PresetName;
            }
        }

        /// <summary>
        /// Handles playlists updated. Playlist changes may playlist lists displayed, media item actions for adding/
        /// removing media items from playlists.
        /// </summary>
        public void HandlePlaylistsUpdated()
        {
            RefreshSelectedItems();           
        }

        /// <summary>
        /// Handles queue updated. E.g. Cleared.
        /// </summary>
        public void HandleQueueUpdated()
        {
            RefreshSelectedItems();
        }

        /// <summary>
        /// Refreshes selected items in UI. This may be necessary after dependent data changes on another screen.        
        /// E.g. User adding/deleted playlist.
        /// 
        /// If we can select the exact items (E.g. Media item collection for playlist no longer exists) then select another
        /// item.
        /// </summary>
        private void RefreshSelectedItems()
        {
            // Store current state
            var oldSelectedMediaLocation = SelectedMediaLocation;
            var oldSelectedArtist = SelectedArtist;
            var oldSelectedMediaItemCollection = SelectedMediaItemCollection;
            var oldSelectedMediaItem = SelectedMediaItem;
            var oldSelectedMediaItemAction = SelectedMediaItemAction;

            //var isPlayingOrPaused = IsPlaying || IsPaused;
            
            // Clear selected media item location
            SelectedMediaLocation = null;
            LoadAvailableMediaLocations();

            // Set media location. When the property is set then we load the child items and set a default selected child
            // item and we repeat this until the lowest level (Media item actions)
            SelectedMediaLocation = MediaLocations.First(ml => ml.Name == oldSelectedMediaLocation.Name);                       

            // Set selected artist
            if (oldSelectedArtist != null)
            {
                SelectedArtist = Artists.FirstOrDefault(a => a.Name == oldSelectedArtist.Name);
            }
            if (SelectedArtist == null)
            {
                SelectedArtist = Artists.First();
            }

            // Set selected media item collection
            if (oldSelectedMediaItemCollection != null)
            {
                SelectedMediaItemCollection = MediaItemCollections.FirstOrDefault(mic => mic.Name == oldSelectedMediaLocation.Name);                
            }
            if (SelectedMediaItemCollection == null)
            {
                SelectedMediaItemCollection = MediaItemCollections.First();
            }

            // Set selected media item
            if (oldSelectedMediaItem != null)
            {
                SelectedMediaItem = MediaItems.FirstOrDefault(mi => mi.Name == oldSelectedMediaItem.Name);
            }
            if (SelectedMediaItem == null)    
            {
                SelectedMediaItem = MediaItems.First();
            }

            // Selected selected media item action
            if (oldSelectedMediaItemAction != null)
            {
                SelectedMediaItemAction = MediaItemActions.FirstOrDefault(mia => mia.Name == oldSelectedMediaItemAction.Name);
            }
            if (SelectedMediaItemAction == null)
            {
                SelectedMediaItemAction = MediaItemActions.First();
            }
        }

        /// <summary>
        /// Clear search results
        /// </summary>
        public void ClearSearchResults()
        {
            SearchResults = new List<SearchResult>();
        }
    }
}
