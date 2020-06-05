using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using BrainGames.Services;
using BrainGames.Views;
using BrainGames.Utility;

namespace BrainGames
{
    public partial class App : Application
    {
        public static int ScreenHeight { get; set; }
        public static int ScreenWidth { get; set; }
        public static int ScreenDensity { get; set; }

        public static MasterUtilityModel mum;

        public App()
        {
            InitializeComponent();
            mum = new MasterUtilityModel();

            DependencyService.Register<MockDataStore>();
            MainPage = new MainPage();

            ScreenDensity = (int)DeviceDisplay.MainDisplayInfo.Density;
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
