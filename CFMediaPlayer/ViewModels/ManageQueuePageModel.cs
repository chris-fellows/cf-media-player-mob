using CFMediaPlayer.Enums;
using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;
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
    public class ManageQueuePageModel : INotifyPropertyChanged
    {
        private readonly ICurrentState _currentState;
        private readonly IMediaSourceService _mediaSourceService;
        private readonly IMediaSource _mediaSource;
        //private bool _isQueueUpdated = false;

        public LocalizationResources LocalizationResources => LocalizationResources.Instance;

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string name = "") =>
                     PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public ManageQueuePageModel(ICurrentState currentState,
                                    IMediaSourceService mediaSourceService)
        {
            _currentState = currentState;
            _mediaSourceService = mediaSourceService;
            _mediaSource = _mediaSourceService.GetAll().First(ms => ms.MediaLocation.MediaSourceType == MediaSourceTypes.Queue);
               
            // Set commands            
            ClearCommand = new Command(DoClear);
            //CloseCommand = new Command(DoClose);

            ConfigureEvents();

            LoadMediaItems();
        }

        private void ConfigureEvents()
        {
            // Set action to handle change of queue updated
            _currentState.Events.OnQueueUpdated += (mediaItem) =>
            {
                LoadMediaItems();
            };

        }

        /// <summary>
        /// Loads meda items for queue
        /// </summary>
        private void LoadMediaItems()
        {
            MediaItems = _mediaSource.GetMediaItemsForMediaItemCollection(null, null, false);
        }

        /// <summary>
        /// Media items in queue
        /// </summary>
        private List<MediaItem> _mediaItems = new List<MediaItem>();

        public List<MediaItem> MediaItems
        {
            get { return _mediaItems; }
            set
            {
                _mediaItems = value;

                OnPropertyChanged(nameof(MediaItems));
            }
        }

        //public ICommand CloseCommand { get; set; }

        //private void DoClose()
        //{
        //    // Redirect to main page. Indicating queue updated causes a page refresh and so we avoid doing it
        //    // unless necessary.            
        //    if (_isQueueUpdated)
        //    {
        //        Shell.Current.GoToAsync($"//{nameof(MainPage)}?EventData=QueueUpdated");
        //    }
        //    else
        //    {
        //        Shell.Current.GoToAsync($"//{nameof(MainPage)}");
        //    }
        //}

        /// <summary>
        /// Command to clear queue
        /// </summary>
        public ICommand ClearCommand { get; set; }

        /// <summary>
        /// Clear playlist
        /// </summary>
        /// <param name="parameter"></param>
        private void DoClear(object parameter)
        {
            var mediaAction = new MediaAction()
            {
                ActionType = MediaActionTypes.ClearQueue,
                MediaLocationName = _mediaSource.MediaLocation.Name
            };

            // Executing will trigger ICurrentState.QueueUpdatedAction
            _mediaSource.ExecuteMediaAction(mediaAction);            
        }
    }
}
