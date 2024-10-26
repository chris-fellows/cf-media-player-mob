using CFMediaPlayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace CFMediaPlayer.Models
{
    public class CurrentState : ICurrentState
    {
        public bool ShufflePlay { get; set; }

        public bool AutoPlayNext { get; set; }

        public Action<string> SelectTabByTitleAction { get; set; }

        public IMediaSource? SelectedMediaSource { get; set; }

        public MediaLocation? SelectedMediaLocation { get; set; }

        public Artist? SelectedArtist { get; set; }

        public MediaItemCollection? SelectedMediaItemCollection { get; set; }

        private MediaItem? _selectedMediaItem;
        public MediaItem? SelectedMediaItem
        {
            get { return _selectedMediaItem; }
            set
            {
                _selectedMediaItem = value;

                if (SelectedMediaItemChangedAction != null)
                {
                    SelectedMediaItemChangedAction(_selectedMediaItem);
                }

                /*
                if (_selectedMediaItemChangedAction != null)
                {
                    _selectedMediaItemChangedAction();
                }
                */
            }
        }

        public List<MediaItem> MediaItems { get; set; } = new List<MediaItem>();

        public IMediaPlayer MediaPlayer { get; set; }

        //private Action _selectedMediaItemChangedAction;

        //public void RegisterSelectedMediaItemChanged(Action action)
        //{
        //    _selectedMediaItemChangedAction = action;
        //}

        public Action<MediaItem>? SelectMediaItemAction { get; set; }

        public Action<MediaLocation, Artist, MediaItemCollection>? SelectMediaItemCollectionAction { get; set; }

        public Action? QueueUpdatedAction { get; set; }

        public Action? UserSettingsUpdatedAction { get; set; }

        public Action<MediaItem>? SelectedMediaItemChangedAction { get; set; }
    }
}
