using Android.Content;
using CFMediaPlayer.Enums;
using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;
using CFMediaPlayer.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CFMediaPlayer.ViewModels
{
    public class ManagePlaylistsPageModel : INotifyPropertyChanged
    {
        private readonly IMediaSourceService _mediaSourceService;
        private readonly IMediaSource _mediaSource;
        private readonly List<IPlaylist> _playlists;
        private readonly string _playlistFolder;
        //private string _lastNewPlaylistName = String.Empty;
        private bool _isPlaylistsUpdated = false;

        public LocalizationResources LocalizationResources => LocalizationResources.Instance;

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string name = "") =>
                     PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public ManagePlaylistsPageModel(IMediaSourceService mediaSourceService,
                IEnumerable<IPlaylist> playlists)
        {
            _mediaSourceService = mediaSourceService;
            _mediaSource = _mediaSourceService.GetAll().First(ms => ms.MediaLocation.MediaSourceType == MediaSourceTypes.Playlist);
            _playlists = playlists.ToList();
            _playlistFolder = FileSystem.AppDataDirectory;            

            // Set commands
            DeleteCommand = new Command(DoDelete);
            ClearCommand = new Command(DoClear);
            CreateCommand = new Command(DoCreate);
            CloseCommand = new Command(DoClose);

            // Set default new playlist
            NewPlaylistName = "My Favourites";
            
            LoadPlaylists();
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
                if (String.IsNullOrEmpty(NewPlaylistName)) return false;
                var playlistFile = Path.Combine(_playlistFolder, $"{NewPlaylistName}.playlist");
                return !File.Exists(playlistFile);
            }
        }

        private void LoadPlaylists(string? selectedName = null)
        {
            // Get playlists
            var playlists = _mediaSource.GetMediaItemCollectionsForArtist(null, false);

            // Add None if no playlists
            if (!playlists.Any())
            {
                playlists.Add(MediaItemCollection.InstanceNone);                
            }
            Playlists = playlists;

            // Select playlist
            var playlist = String.IsNullOrEmpty(selectedName) ? Playlists[0] : Playlists.First(p => p.Name == selectedName);
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
                OnPropertyChanged(nameof(IsPlaylistSelected));
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
            var mediaItemAction = new MediaItemAction()
            {
                ActionToExecute = MediaItemActions.DeletePlaylist,
                MediaLocationName = _mediaSource.MediaLocation.Name,
                File = _selectedPlaylist.Path 
            };

            _mediaSource.ExecuteMediaItemAction(new(), mediaItemAction);

            _isPlaylistsUpdated = true;

            // Refresh playlists
            LoadPlaylists();
        }

        public bool IsPlaylistSelected => _selectedPlaylist != null && _selectedPlaylist.EntityCategory == EntityCategory.Real;

        /// <summary>
        /// Command to create playlist
        /// </summary>
        public ICommand CreateCommand { get; set; }

        private void DoCreate()
        {
            // Create playlist
            var playlist = _playlists.First(pl => pl.SupportsFile(Path.Combine(_playlistFolder, "Test.playlist")));                        
            var playlistFile = Path.Combine(_playlistFolder, $"{NewPlaylistName}.playlist");
            playlist.SetFile(playlistFile);
            playlist.SaveAll(new());

            _isPlaylistsUpdated = true;

            // Record name of playlist so that we can select it when we direct to main page
            //_lastNewPlaylistName = NewPlaylistName;

            // Refresh playlists, with new playlist selected
            LoadPlaylists(NewPlaylistName);            
        }

        public ICommand CloseCommand { get; set; }

        private void DoClose()
        {
            // Redirect to main page. Indicating playlists updated causes a page refresh and so we avoid doing it
            // unless necessary.            
            if (_isPlaylistsUpdated)
            {
                Shell.Current.GoToAsync($"//{nameof(MainPage)}?EventData=PlaylistsUpdated");
            }
            else
            {
                Shell.Current.GoToAsync($"//{nameof(MainPage)}");
            }

            //Shell.Current.GoToAsync($"//{nameof(MainPage)}?NewPlaylistName={Name}");
            //Shell.Current.GoToAsync($"//{nameof(MainPage)}");
        }

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
            var mediaItemAction = new MediaItemAction()
            {
                ActionToExecute = MediaItemActions.ClearPlaylist,
                MediaLocationName = _mediaSource.MediaLocation.Name,
                File = _selectedPlaylist.Path
            };

            _mediaSource.ExecuteMediaItemAction(new(), mediaItemAction);

            _isPlaylistsUpdated = true;
        }
    }
}
