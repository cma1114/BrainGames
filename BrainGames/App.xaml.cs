﻿using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XF.Material.Forms;
using XF.Material.Forms.UI.Dialogs.Configurations;
using XF.Material.Forms.Resources;
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

        public static AppCenterAnalyticsService AnalyticsService = new AppCenterAnalyticsService();

        public static MasterUtilityModel mum;

        public App()
        {
            Device.SetFlags(new string[] { "RadioButton_Experimental" });

            InitializeComponent();
            mum = new MasterUtilityModel();

            DependencyService.Register<MockDataStore>();
            Material.Init(this);// , "Material.Configuration");
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

        private static MaterialAlertDialogConfiguration _materialAlertDialogConfiguration;

        public static MaterialAlertDialogConfiguration GetMaterialAlertDialogConfiguration()
        {
            if (_materialAlertDialogConfiguration == null)
            {
                var font = Material.GetResource<OnPlatform<string>>("FontFamily.RobotoRegular");
                var font2 = Material.GetResource<OnPlatform<string>>("FontFamily.RobotoMedium");

                _materialAlertDialogConfiguration = new MaterialAlertDialogConfiguration
                {
                    CornerRadius = 8,
                    BackgroundColor = Material.GetResource<Color>(MaterialConstants.Color.PRIMARY),
                    TintColor = Material.GetResource<Color>(MaterialConstants.Color.SECONDARY),
                    TitleTextColor = Material.GetResource<Color>(MaterialConstants.Color.SECONDARY),
                    MessageTextColor = Color.FromHex("#DEFFFFFF"),
                    TitleFontFamily = font,
                    MessageFontFamily = font,
                    ButtonFontFamily = font2,
                    ScrimColor = Color.Black.MultiplyAlpha(0.6)
                };
            }

            return _materialAlertDialogConfiguration;
        }

    }
}
