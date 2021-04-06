using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XF.Material.Forms;
using XF.Material.Forms.UI.Dialogs.Configurations;
using XF.Material.Forms.Resources;
using XF.Material.Forms.Resources.Typography;
using Xamarin.Essentials;
using BrainGames.Services;
using BrainGames.Views;
using BrainGames.Utility;

using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

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
            Xamarin.Forms.Device.SetFlags(new string[] { "RadioButton_Experimental" });

            InitializeComponent();
            mum = new MasterUtilityModel();

            DependencyService.Register<MockDataStore>();
            Material.Init(this);//, (MaterialConfiguration)Resources["Material.Configuration"]);
            Material.Use("Material.Configuration");
            MainPage = new MainPage();

            ScreenDensity = (int)DeviceDisplay.MainDisplayInfo.Density;
        }

        protected override void OnStart()
        {
            // Handle when your app starts
            AppCenter.Start("ios=abc625ee-28f5-4cad-bf37-f8367eccfcaa;" +
                              "uwp={Your UWP App secret here};" +
                              "android=d647dd2e-9b67-4eff-bdaa-3486be68318e;",
                              typeof(Analytics), typeof(Crashes));
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
