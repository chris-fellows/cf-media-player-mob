using CFMediaPlayer.Enums;
using CFMediaPlayer.Interfaces;

namespace CFMediaPlayer.Models
{
    public class CurrentState : ICurrentState
    {
        public bool ShufflePlay { get; set; }

        public bool AutoPlayNext { get; set; }

        public Action<string>? SelectTabByTitleAction { get; set; }

        public IMediaSource? SelectedMediaSource { get; set; }

        public MediaLocation? SelectedMediaLocation { get; set; }

        public Artist? SelectedArtist { get; set; }

        public MediaItemCollection? SelectedMediaItemCollection { get; set; }
        
        public MediaItem? SelectedMediaItem { get; set; }
        
        public MediaItem? CurrentMediaItem { get; set; }

        public List<MediaItem> MediaItems { get; set; } = new List<MediaItem>();

        //public IMediaPlayer? MediaPlayer { get; set; }
             
        public Action<MediaLocation, Artist, MediaItemCollection, MediaItem?>? SelectMediaItemCollectionAction { get; set; }

        public Func<MediaItem, MediaPlayerStatuses?>? GetMediaItemPlayStatusFunction { get; set; }        

        private CurrentStateEvents _events = new CurrentStateEvents();
        public CurrentStateEvents Events => _events;
    }
}
