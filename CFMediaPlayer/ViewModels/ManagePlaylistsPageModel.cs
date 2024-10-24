using Android.Content;
using CFMediaPlayer.Enums;
using CFMediaPlayer.Exceptions;
using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;
using CFMediaPlayer.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace CFMediaPlayer.ViewModels
{
    public class ManagePlaylistsPageModel : INotifyPropertyChanged
    {        
        private readonly IMediaSourceService _mediaSourceService;        
        private IMediaSource _mediaSource;        
        private readonly List<IPlaylistManager> _playlistManagers;    
        private bool _isPlaylistsUpdated = false;

        private List<MediaLocation> _mediaLocations = new List<MediaLocation>();

        public delegate void Error(Exception exception);
        public event Error? OnError;

        public LocalizationResources LocalizationResources => LocalizationResources.Instance;

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string name = "") =>
                     PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public ManagePlaylistsPageModel(IMediaSourceService mediaSourceService,
                                       IEnumerable<IPlaylistManager> playlistManagers)
        {
            _mediaSourceService = mediaSourceService;            
            _playlistManagers = playlistManagers.ToList();            

            // Set commands
            DeleteCommand = new Command(DoDelete);
            ClearCommand = new Command(DoClear);
            CreateCommand = new Command(DoCreate);
            //CloseCommand = new Command(DoClose);

            // Set default new playlist
            NewPlaylistName = "My Favourites";

            // Set all playlist media locations
            var mediaSources = _mediaSourceService.GetAll();
            var mediaLocations = mediaSources.Where(ms => ms.MediaLocation.MediaSourceType == MediaSourceTypes.Playlist && 
                                            ms.IsAvailable)
                                            .Select(ms => ms.MediaLocation).ToList();            
            if (!mediaLocations.Any())
            {
                mediaLocations.Add(MediaLocation.InstanceNone);
            }
            MediaLocations = mediaLocations;
            
            // Default to first media location
            SelectedMediaLocation = MediaLocations.First();           
        }

        private MediaLocation _selectedMediaLocation;
      
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

                _mediaSource = null;
                if (_selectedMediaLocation != null && _selectedMediaLocation.EntityCategory == EntityCategory.Real)
                {
                    _mediaSource = _mediaSourceService.GetAll().First(ms => ms.MediaLocation.Name == _selectedMediaLocation.Name);
                }

                LoadPlaylists();
            }
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

        private string _newPlaylistName = String.Empty;
        public string NewPlaylistName
        {
            get { return _newPlaylistName; }
            set 
            { 
                _newPlaylistName = value;

                OnPropertyChanged(nameof(NewPlaylistName));
                OnPropertyChanged(nameof(IsCreateEnabled));
            }
        }
        
        public bool IsCreateEnabled
        {
            get
            {
                if (String.IsNullOrEmpty(NewPlaylistName) ||
                    _selectedMediaLocation == null ||
                    _selectedMediaLocation.EntityCategory != EntityCategory.Real) return false;

                // Check that playlist doesn't exist. Media source could relate to multiple folders but we just prevent
                // create if any folder contains it
                var playlists = _mediaSource.GetMediaItemCollectionsForArtist(null, false);
                return !playlists.Any(mic => mic.Name.Equals(NewPlaylistName, StringComparison.OrdinalIgnoreCase));                
            }
        }

        /// <summary>
        /// Loads playlist and selects one playlist
        /// </summary>
        /// <param name="selectedPlaylistFile"></param>
        private void LoadPlaylists(string? selectedPlaylistFile = null)
        {
            List<MediaItemCollection> mediaItemCollections = new List<MediaItemCollection>();
            if (_selectedMediaLocation.EntityCategory == EntityCategory.Real)
            {
                // Get playlists
                mediaItemCollections = _mediaSource.GetMediaItemCollectionsForArtist(null, false);
            }

            // Add None if no playlists
            if (!mediaItemCollections.Any())
            {
                mediaItemCollections.Add(MediaItemCollection.InstanceNone);
            }
            Playlists = mediaItemCollections;

            // Select playlist            
            var playlist = String.IsNullOrEmpty(selectedPlaylistFile) ? Playlists[0] : Playlists.First(p => p.Path == selectedPlaylistFile);
            SelectedPlaylist = playlist;           
        }

        private List<MediaItemCollection> _mediaItemCollections = new List<MediaItemCollection>();
        public List<MediaItemCollection> Playlists
        {
            get { return _mediaItemCollections; }

            set
            {
                _mediaItemCollections = value;

                OnPropertyChanged(nameof(Playlists));
            }
        }

        /// <summary>
        /// Selected playlist
        /// </summary>
        private MediaItemCollection? _selectedPlaylist;
        public MediaItemCollection? SelectedPlaylist
        {
            get { return _selectedPlaylist; }
            set
            {
                _selectedPlaylist = value;

                OnPropertyChanged(nameof(SelectedPlaylist));
                OnPropertyChanged(nameof(IsRealPlaylistSelected));
            }
        }

        /// <summary>
        /// Command to delete playlist
        /// </summary>
        public ICommand DeleteCommand { get; set; }

        /// <summary>
        /// Deletes playlist
        /// </summary>
        /// <param name="parameter"></param>
        private void DoDelete(object parameter)
        {
            var mediaItemAction = new MediaAction()
            {
                ActionType = MediaActionTypes.DeletePlaylist,
                MediaLocationName = _mediaSource.MediaLocation.Name,
                PlaylistFile = _selectedPlaylist.Path 
            };

            _mediaSource.ExecuteMediaAction(mediaItemAction);

            _isPlaylistsUpdated = true;

            // Refresh playlists
            LoadPlaylists();
        }

        /// <summary>
        /// Whether real playlist is selected (i.e. Not None)
        /// </summary>
        public bool IsRealPlaylistSelected => _selectedPlaylist != null && _selectedPlaylist.EntityCategory == EntityCategory.Real;

        /// <summary>
        /// Command to create playlist
        /// </summary>
        public ICommand CreateCommand { get; set; }

        private void DoCreate()
        {
            // Create playlist
            var playlistFolder = _selectedMediaLocation.Sources.First();
            var playlistManager = _playlistManagers.First(pl => pl.SupportsFile(Path.Combine(playlistFolder, "Test.playlist")));
            var playlistFile = Path.Combine(playlistFolder, $"{NewPlaylistName}.playlist");
            playlistManager.FilePath = playlistFile;
            playlistManager.Name = NewPlaylistName;
            playlistManager.SaveAll(new());
            playlistManager.FilePath = "";   // Clean up

            _isPlaylistsUpdated = true;


            // Refresh playlists, with new playlist selected
            LoadPlaylists(playlistFile);        
        }

        //public ICommand CloseCommand { get; set; }

        //private void DoClose()
        //{
        //    // Redirect to main page. Indicating playlists updated causes a page refresh and so we avoid doing it
        //    // unless necessary.            
        //    if (_isPlaylistsUpdated)
        //    {
        //        Shell.Current.GoToAsync($"//{nameof(MainPage)}?EventData=PlaylistsUpdated");
        //    }
        //    else
        //    {
        //        Shell.Current.GoToAsync($"//{nameof(MainPage)}");
        //    }
        //}

        /// <summary>
        /// Command to clear playlist
        /// </summary>
        public ICommand ClearCommand { get; set; }

        /// <summary>
        /// Clear playlist
        /// </summary>
        /// <param name="parameter"></param>
        private void DoClear(object parameter)
        {
            var mediaItemAction = new MediaAction()
            {
                ActionType = MediaActionTypes.ClearPlaylist,
                MediaLocationName = _mediaSource.MediaLocation.Name,
                PlaylistFile = _selectedPlaylist.Path
            };

            _mediaSource.ExecuteMediaAction(mediaItemAction);

            _isPlaylistsUpdated = true;
        }
    }
}
