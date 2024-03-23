using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;

namespace CFMediaPlayer.ViewModels
{
    /// <summary>
    /// View model for new playlist
    /// </summary>
    public class NewPlaylistPageModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public LocalizationResources LocalizationResources => LocalizationResources.Instance;
   
        public void OnPropertyChanged([CallerMemberName] string name = "") =>
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


        private readonly string _playlistFolder;
        private readonly IPlaylist _playlist;

        public NewPlaylistPageModel(IEnumerable<IPlaylist> playlists)
        {
            _playlistFolder = FileSystem.AppDataDirectory;
            _playlist = playlists.First(pl => pl.SupportsFile(Path.Combine(_playlistFolder, "Test.playlist")));

            // Set commands
            CreatePlaylistCommand = new Command(CreatePlaylist);
        }

        private string _name = String.Empty;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Command to create playlist
        /// </summary>
        public ICommand CreatePlaylistCommand { get; set; }

        private void CreatePlaylist()
        {          
            var playlistFile = Path.Combine(_playlistFolder, $"{Name}.playlist");            
            _playlist.SetFile(playlistFile);
            _playlist.SaveAll(new());

            Shell.Current.GoToAsync($"//{nameof(MainPage)}?NewPlaylistName={Name}");
        }
    }
}
