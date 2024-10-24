using CFMediaPlayer.ViewModels;
using System.Runtime.CompilerServices;

namespace CFMediaPlayer
{
    public partial class App : Application
    {
        //private MainPageModel _mainPageModel;
        private TestPageModel _testPageModel;

        public App(TestPageModel testPageModel)
        {
            InitializeComponent();

            _testPageModel = testPageModel;

            //MainPage = new AppShell();            
            MainPage = new TestPage();
        }

        //public App(MainPageModel mainPageModel)
        //{
        //    InitializeComponent();

        //    _mainPageModel = mainPageModel;

        //    MainPage = new AppShell();
        //}

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
