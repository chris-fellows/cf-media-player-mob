using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;
using CFMediaPlayer.Utilities;
using Java.Util.Zip;
using System.ComponentModel.Design;
using System.IO;
using System.Text;

namespace CFMediaPlayer.Playlists
{
    /// <summary>
    /// M3U playlist. Also supports compressed .m3u.zip files.
    /// </summary>
    public class M3UPlaylist : IPlaylistManager
    {
        private string? _file;

        public string Name { get; set; } = String.Empty;

        public string FilePath
        {
            get { return _file; }
            set
            {
                _file = value;
                Name = "";
            }
        }

        public List<MediaItem> GetAll()
        {
            var mediaItems = new List<MediaItem>();
            
            // Get file content
            var lines = new List<string>();
            if (_file.EndsWith(".zip"))
            {
                var content = Encoding.UTF8.GetString(CompressionUtilities.DecompressWithDeflate(File.ReadAllBytes(_file)));                
                lines.AddRange(content.Split('\n'));
            }
            else
            {
                lines.AddRange(File.ReadAllLines(_file, Encoding.UTF8));
            }

            // Process file content
            MediaItem currentMediaItem = new MediaItem();
            foreach (var line in lines)
            {                
                if (line.StartsWith("#EXTM3U"))     // File header
                {
                    // No action
                }
                else if (line.StartsWith("#PLAYLIST:"))    // Playlist name
                {
                    Name = line.Substring(line.IndexOf(':') + 1).Trim();
                }
                else if (line.StartsWith("#EXTINF:"))   // Track info (Runtime secs, display title)
                {
                    var elements = line.Substring(line.IndexOf(':') + 1).Split(',');
                    currentMediaItem.Name = elements[1];
                }
                else if (line.StartsWith("#EXTIMG:"))   // Logo
                {
                    var elements = line.Substring(line.IndexOf(':') + 1).Split(',');
                    currentMediaItem.ImagePath = elements[0];
                }
                else if (!line.StartsWith("#") && line.Length > 0)   // Media item path
                {
                    // Add media item to list
                    currentMediaItem.FilePath = line.Trim();
                    mediaItems.Add(currentMediaItem);

                    // New media item
                    currentMediaItem = new MediaItem();
                }            
            }

            return mediaItems;                
        }

        //public void SetFile(string file)
        //{
        //    _file = file;
        //    Name = "";
        //}

        public bool SupportsFile(string file)
        {
            return Path.GetExtension(file).ToLower().Equals(".m3u") ||
                Path.GetExtension(file).ToLower().Equals(".m3u.zip");
        }

        public void SaveAll(List<MediaItem> mediaItems)
        {
            if (File.Exists(_file))
            {
                File.Delete(_file);
            }
            
            // Set content
            StringBuilder content = new StringBuilder("");
            content.AppendLine("#EXTM3U");
            foreach (var mediaItem in mediaItems)
            {
                var runtimeSecs = -1;
                content.AppendLine($"#EXTINF:{runtimeSecs},{mediaItem.Name}");
                if (!String.IsNullOrEmpty(Name))
                {
                    content.AppendLine($"#PLAYLIST:{Name}");
                }
                if (!String.IsNullOrEmpty(mediaItem.ImagePath))
                {
                    content.AppendLine($"#EXTIMG:{mediaItem.ImagePath}");
                }
                content.AppendLine(mediaItem.FilePath);
            }

            // Save
            if (_file.ToLower().EndsWith(".zip"))
            {
                File.WriteAllBytes(_file, CompressionUtilities.CompressWithDeflate(Encoding.UTF8.GetBytes(content.ToString())));                
            }
            else
            {
                File.WriteAllText(_file, content.ToString(), Encoding.UTF8);
            }
        }
    }
}
