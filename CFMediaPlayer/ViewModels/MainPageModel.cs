using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows.Input;
using CFMediaPlayer.Enums;
using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;
using CFMediaPlayer.Services;
using CFMediaPlayer.Sources;
using CFMediaPlayer.Utilities;
using Java.Util.Concurrent.Locks;
using Kotlin.IO;
using static Java.Util.Jar.Attributes;

namespace CFMediaPlayer.ViewModels
{
    /// <summary>
    /// View model for main page
    /// 
    /// Notes:
    /// - In the lists for artists, media item collections and media items then None is displayed rather than showing
    ///   an empty list.
    /// - Media player state is sync'd to the UI by the events from the player.
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
            SelectedPlayMode = _playModes.First();

            // Load available media locations
            LoadAvailableMediaLocations();

            // Get user settings (Theme, audio settings)
            var userSettings = _userSettingsService.GetByUsername(Environment.UserName);
            _uiTheme = _uiThemeService.GetAll().First(t => t.Id == userSettings.UIThemeId);           
            _audioSettings = _audioSettingsService.GetById(userSettings.AudioSettingsId)!;

            // Set equalizer preset
            _mediaPlayer.EqualizerPresetName = _audioSettings.PresetName;
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

                // Player buttons
                //NotifyPropertiesChangedForPlayState();
            }
        }

        /// <summary>
        /// Loads action for media item
        /// </summary>
        /// <param name="mediaItem"></param>
        private void LoadMediaItemActions(MediaItem mediaItem)
        {
            MediaItemActions = new List<MediaItemAction>();
            SelectedMediaItemAction = null;            

            var mediaItemActions = new List<MediaItemAction>();
            if (!MediaUtilities.IsNoneMediaItem(mediaItem))
            {
                // Get media item actions
                // TODO: Clean this up. It's confusing how it works                
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
            }
            if (!mediaItemActions.Any())
            {
                var itemNone = new MediaItemAction()
                {
                    Name = LocalizationResources.Instance["None"].ToString()
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
                OnPropertyChanged(nameof(IsNotNoneMediaItemSelected));
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
                    if (isWasPlaying && IsNotNoneMediaItemSelected)
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
                    // Display New Playlist page if selected
                    if (_selectedMediaItemCollection.Name.Equals(LocalizationResources.Instance["NewPlaylistText"].ToString()))
                    {
                        Shell.Current.GoToAsync($"//{nameof(NewPlaylistPage)}");
                    }

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
            LoadArtists();    
            
            // Select Nth artist
            SelectedArtist = Artists[0];            
        }

        /// <summary>
        /// Loads the default media item collections and media items for artist (First album selected)
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

            // Select Nth media item collection
            SelectedMediaItemCollection = MediaItemCollections[0];            
        }

        /// <summary>
        /// Loads the default media items for the artist and album (First media item selected)
        /// </summary>
        /// <param name="artist"></param>
        /// <param name="mediaItemCollection"></param>
        private void LoadMediaItemCollectionDefaults(Artist artist, MediaItemCollection mediaItemCollection)
        {            
            // Gets media items for media item collection (Adds None if necessary)            
            LoadMediaItemsForMediaItemCollection(artist.Name, mediaItemCollection);                      

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

        private void LoadMediaItemsForMediaItemCollection(string artistName, MediaItemCollection mediaItemCollection)
        {
            //_mediaItems.Clear();
            var mediaItems = new List<MediaItem>();
            if (!MediaUtilities.IsNoneMediaItemCollection(mediaItemCollection) && CurrentMediaSource.IsAvailable)
            {
                mediaItems = CurrentMediaSource!.GetMediaItemsForMediaItemCollection(artistName, mediaItemCollection.Name);
            }            

            // Add None if no media items
            if (!mediaItems.Any())
            {
                mediaItems.Add(new MediaItem()
                {
                    Name = LocalizationResources.Instance["None"].ToString(),
                });
            }

            MediaItems = mediaItems;
        }

        private void LoadArtists()
        {
            // Get artists
            var artists = CurrentMediaSource.IsAvailable ? CurrentMediaSource!.GetArtists() : new List<Artist>();

            // Add None if no artists
            if (!artists.Any())
            {
                artists.Add(new Artist()
                {
                    Name = LocalizationResources.Instance["None"].ToString(),
                });
            }

            Artists = artists;
        }

        private void LoadMediaItemCollectionsForArtist(Artist artist)
        {            
            var mediaItemCollections = new List<MediaItemCollection>();
            if (!MediaUtilities.IsNoneArtist(artist) && 
                CurrentMediaSource!.IsAvailable)
            {
                mediaItemCollections = CurrentMediaSource!.GetMediaItemCollectionsForArtist(artist.Name);
            }

            // Add None if no media item collections
            if (!mediaItemCollections.Any())
            {
                mediaItemCollections.Add(new MediaItemCollection()
                {
                    Name = LocalizationResources.Instance["None"].ToString(),
                });
            }

            MediaItemCollections = mediaItemCollections;         
        }

        public List<MediaLocation> MediaLocations
        {
            get { return _mediaLocations; }
            set
            {
                _mediaLocations = value;

                OnPropertyChanged(nameof(MediaLocations));
            }
        }

        public List<MediaItem> MediaItems
        {
            get { return _mediaItems; }
            set
            {
                _mediaItems = value;

                OnPropertyChanged(nameof(MediaItems));
            }
        }

        public List<MediaItemCollection> MediaItemCollections
        {
            get { return _mediaItemCollections; }
            set
            {
                _mediaItemCollections = value;

                OnPropertyChanged(nameof(MediaItemCollections));
            }
        }

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
                    var duration = _mediaPlayer.GetElapsedPlayTime();
                    var durationString = string.Format("{0:00}", duration.Hours) + ":" +
                           string.Format("{0:00}", duration.Minutes) + ":" +
                           string.Format("{0:00}", duration.Seconds);
                    return durationString;
                }

                return "00:00:00";
            }
        }

        public string RemainingTime
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

                return "00:00:00";
            }
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
                    NotifyPropertiesChangedForPlayState();

                    _elapsedTimer.Enabled = false;

                    PlayNextMediaItemIfConfigured();
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
                if (IsNotNoneMediaItemSelected)
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
                if (IsNotNoneMediaItemSelected)
                {
                    var index = _mediaItems.IndexOf(SelectedMediaItem);
                    return index < _mediaItems.Count - 1;
                }
                return false;
            }
        }      
        
        /// <summary>
        /// Whether a media item is selected that isn't None
        /// </summary>
        public bool IsNotNoneMediaItemSelected
        {
            get
            {
                return SelectedMediaItem != null &&
                   !MediaUtilities.IsNoneMediaItem(SelectedMediaItem);
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
            //NotifyPropertiesChangesForSelectedArtist();
            //NotifyPropertiesChangesForSelectedMediaItemCollection();
            //NotifyPropertiesChangedForSelectedMediaItem();
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
            var results = _mediaSearchService.SearchAsync(searchOptions).Result;

            SearchResults = results;
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
        /// <param name="name"></param>
        public void SelectSearchResult(string name)
        {
            // Get search result
            var searchResult = _searchResults.First(sr => sr.Name == name);

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
        /// Refreshes user settings
        /// </summary>
        public void RefreshUserSettings()
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
        /// Clear search results
        /// </summary>
        public void ClearSearchResults()
        {
            SearchResults = new List<SearchResult>();
        }
    }
}
