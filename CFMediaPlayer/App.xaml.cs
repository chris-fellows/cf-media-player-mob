using CFMediaPlayer.Interfaces;
using CFMediaPlayer.ViewModels;

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

        protected override void OnSleep()
        {
            // TODO: sleep
            base.OnSleep();
        }

        protected override void OnResume()
        {
            base.OnResume();
        }
    }
}
