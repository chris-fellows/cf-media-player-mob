using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;

namespace CFMediaPlayer
{
    public partial class App : Application
    {
        private MainPageModel _mainPageModel;

        public App(MainPageModel mainPageModel)
        {
            InitializeComponent();

            _mainPageModel = mainPageModel;

            MainPage = new AppShell();            
        }

        protected override void OnSleep()  // CMF Added
        {
            // TODO: sleep
            base.OnSleep();
        }

        protected override void OnResume()  // CMF Added
        {
            base.OnResume();
        }
    }
}
