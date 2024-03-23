using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows.Input;
using CFMediaPlayer.Enums;
using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;

namespace CFMediaPlayer.ViewModels
{
    /// <summary>
    /// View model for main page
    /// </summary>
    public class MainPageModel : INotifyPropertyChanged
    {
        private System.Timers.Timer _elapsedTimer;

        public event PropertyChangedEventHandler? PropertyChanged;
        
        private Action<string>? _debugAction;

        public bool AutoPlayNext { get; set; }

        private IMediaPlayer _mediaPlayer { get; set; }

        private List<IMediaSource> _mediaSources { get; set; }
        private List<MediaLocation> _mediaLocations;
        private List<Artist> _artists = new List<Artist>();
        private List<MediaItemCollection> _mediaItemCollections = new List<MediaItemCollection>();
        private List<MediaItem> _mediaItems = new List<MediaItem>();

        public MainPageModel(IMediaPlayer mediaPlayer, IEnumerable<IMediaSource> mediaSources)
        {
            _mediaPlayer = mediaPlayer;
            _mediaSources = mediaSources.ToList();

            // Set media locations
            _mediaLocations = new List<MediaLocation>()
            {
                new MediaLocation() { Name = LocalizationResources.Instance["MediaSourceInternalStorageText"].ToString() + " (Test)",
                                MediaSourceType = MediaSourceTypes.Storage,
                                RootFolderPath = "/storage/emulated/0/Download" },      // TODO: Remove this
                new MediaLocation() { Name = LocalizationResources.Instance["MediaSourceInternalStorageText"].ToString(),
                                MediaSourceType = MediaSourceTypes.Storage,
                                RootFolderPath = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, Android.OS.Environment.DirectoryMusic) },
                //new MediaLocation() { Name = "Internal storage", MediaSourceName = MediaSourceNames.Storage, 
                //                RootFolderPath = Path.Combine(Android.OS.Environment.StorageDirectory.Path, Android.OS.Environment.DirectoryMusic) },
                new MediaLocation() { Name = LocalizationResources.Instance["MediaSourceSDCardText"].ToString(),
                                MediaSourceType = MediaSourceTypes.Storage,
                                RootFolderPath = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, Android.OS.Environment.DirectoryMusic) },
                new MediaLocation() { Name = LocalizationResources.Instance["MediaSourcePlaylistsText"].ToString(),
                                MediaSourceType = MediaSourceTypes.Playlist,
                                RootFolderPath = FileSystem.AppDataDirectory }
            };

            foreach (var mediaLocation in _mediaLocations)
            {
                _mediaSources.First(s => s.MediaSourceType == mediaLocation.MediaSourceType).SetSource(mediaLocation.RootFolderPath);
            }

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
        /// Command to play or pause media item
        /// </summary>
        public ICommand PlayOrPauseCommand { get; set; }

        /// <summary>
        /// Command to stop playing or pausing media item
        /// </summary>
        public ICommand StopCommand { get; set; }

        private IMediaSource? CurrentMediaSource
        {
            get
            {
                return _selectedMediaLocation != null ?
                    _mediaSources.First(ms => ms.MediaSourceType == _selectedMediaLocation.MediaSourceType) :
                    null;
            }
        }

        public void ClearMediaItems()
        {
            _mediaItems.Clear();
            _selectedMediaItem = null;

            OnPropertyChanged(nameof(MediaItems));
            OnPropertyChanged(nameof(SelectedMediaItem));
        }

        public void ClearMediaItemCollections()
        {
            _mediaItemCollections.Clear();
            _selectedMediaItemCollection = null;

            OnPropertyChanged(nameof(MediaItemCollections));
            OnPropertyChanged(nameof(SelectedMediaItemCollection));
        }

        public void ClearArtists()
        {
            _artists.Clear();
            _selectedArtist = null;

            OnPropertyChanged(nameof(Artists));
            OnPropertyChanged(nameof(SelectedArtist));
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
                    var isExists = Directory.Exists(_selectedMediaLocation.RootFolderPath);
                    if (_debugAction != null) _debugAction($"MediaLocation={_selectedMediaLocation.RootFolderPath}, Exists={isExists}");

                    // Set source location for media source to read
                    CurrentMediaSource!.SetSource(_selectedMediaLocation.RootFolderPath);

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
            }
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

            // Do nothing if media source not available (E.g. SD card removed)
            if (CurrentMediaSource == null || !CurrentMediaSource.IsAvailable)
            {
                return;
            }

            // Get artists
            var artists = GetArtists();
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

            GetAlbumsForArtist(artist.Name);
            if (MediaItemCollections.Any())
            {
                SelectedMediaItemCollection = MediaItemCollections[0];
            }

            if (SelectedMediaItemCollection != null)
            {
                GetMediaItems(artist.Name, SelectedMediaItemCollection.Name);
                if (MediaItems.Any())
                {
                    SelectedMediaItem = MediaItems[0];
                }
            }
        }

        /// <summary>
        /// Loads the default media items for the artist and album (First media item selected)
        /// </summary>
        /// <param name="artist"></param>
        /// <param name="album"></param>
        private void LoadMediaItemCollectionDefaults(Artist artist, MediaItemCollection album)
        {
            GetMediaItems(artist.Name, album.Name);
            if (MediaItems.Any())
            {
                SelectedMediaItem = MediaItems[0];
            }
        }

        private void DoNext(object parameter)
        {
            var index = _mediaItems.IndexOf(SelectedMediaItem!);
            SelectedMediaItem = _mediaItems[index + 1];

            OnPropertyChanged(nameof(SelectedMediaItem));

            PlayMediaItem(SelectedMediaItem);
        }

        private void DoPrev(object parameter)
        {
            var index = _mediaItems.IndexOf(SelectedMediaItem!);
            SelectedMediaItem = _mediaItems[index - 1];

            OnPropertyChanged(nameof(SelectedMediaItem));

            PlayMediaItem(SelectedMediaItem);
        }

        private void DoStop(object parameter)
        {
            Stop();
        }

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

            /*
            switch (PlayButtonText)
            {
                case "Play":
                case "Resume":
                    PlayMediaItem(SelectedMediaItem!);                    
                    break;
                case "Pause":
                    Pause();                    
                    break;
            }
            */
        }

        private void PlayMediaItem(MediaItem mediaItem)
        {
            PlayAudio(mediaItem.Path,
                (exception) =>
                {
                    //StatusLabel.Text = exception.Message;
                });
        }

        public List<MediaItem> GetMediaItems(string artistName, string albumName)
        {
            _mediaItems = CurrentMediaSource!.GetMediaItemsForMediaItemCollection(artistName, albumName);
            OnPropertyChanged(nameof(MediaItems));
            OnPropertyChanged(nameof(IsMediaItemSelected));
            return _mediaItems;
        }

        public List<Artist> GetArtists()
        {
            _artists = CurrentMediaSource!.GetArtists();
            OnPropertyChanged(nameof(Artists));
            return _artists;
        }

        public List<MediaItemCollection> GetAlbumsForArtist(string artistName)
        {
            _mediaItemCollections = CurrentMediaSource!.GetMediaItemCollectionsForArtist(artistName);
            OnPropertyChanged(nameof(MediaItemCollections));
            return _mediaItemCollections;
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

        public void SetDebugAction(Action<string> debugAction)
        {
            _debugAction = debugAction;
        }

        private void _timer_Elapsed(object? sender, ElapsedEventArgs e)
        {            
            // TODO: Consider disabling timer when app not visible
            OnPropertyChanged(nameof(ElapsedTime));
            OnPropertyChanged(nameof(ElapsedTimeInt));            
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
                    var elapsed = _mediaPlayer.GetElapsedPlayTime();
                    var elapsedString = string.Format("{0:00}", elapsed.Hours) + ":" +
                           string.Format("{0:00}", elapsed.Minutes) + ":" +
                           string.Format("{0:00}", elapsed.Seconds);

                    return elapsedString;
                }

                return "";
            }
        }

        public int DurationInt
        {
            get
            {
                if (_mediaPlayer.IsPlaying || _mediaPlayer.IsPaused)
                {
                    return (int)_mediaPlayer.GetTotalDuration().TotalMilliseconds;
                }
                return 1000;
            }
        }

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

        public bool IsPlaying
        {
            get { return _mediaPlayer.IsPlaying; }
        }

        public bool IsPaused
        {
            get { return _mediaPlayer.IsPaused; }
        }

        public void Pause()
        {
            _mediaPlayer.Pause();
            _elapsedTimer.Enabled = false;
        }

        public void Stop()
        {
            _mediaPlayer.Stop();
            _elapsedTimer.Enabled = false;
        }

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

        //public string PlayButtonText
        //{
        //    get
        //    {               
        //        return IsPlaying ? "Pause" : "Play";
        //    }
        //}

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
                    if (AutoPlayNext && SelectedMediaItem != _mediaItems.Last())
                    {
                        PlayMediaItem(_mediaItems[_mediaItems.IndexOf(SelectedMediaItem) + 1]);
                    }
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

        public bool IsMediaItemSelected
        {
            get
            {
                return SelectedMediaItem != null;
            }
        }
    }
}
