using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Xamarin.Essentials;
using BrainGames.Utility;

namespace BrainGames.Droid
{
    [Activity(Label = "BrainGames", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private async void GetUser()
        {
            try
            {
                var uid = await SecureStorage.GetAsync("UserId");
                if (Settings.UserId == "-1" && uid == null) //a new user
                {
                    Settings.UserId = MasterUtilityModel.RandomNumberLong().ToString();
                    try
                    {
                        await SecureStorage.SetAsync("UserId", Settings.UserId);
                    }
                    catch (Exception ex)
                    {
                        ;
                    }
                }
                else if (Settings.UserId == "-1" && uid != null) //a new device
                {
                    Settings.UserId = uid;
                }
                else if (Settings.UserId != "-1" && uid == null)// userid already on device but not in cloud - should only happen in dev
                {
                    try
                    {
                        await SecureStorage.SetAsync("UserId", Settings.UserId);
                    }
                    catch (Exception ex)
                    {
                        ;
                    }
                }
            }
            catch (Exception ex)
            {
                ;
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            //GetUser();
            try
            {
                bool hasKey = Preferences.ContainsKey("BrainGamesUserId");
                if (Settings.UserId == "-1" && hasKey == false) //a new user
                {
                    Settings.UserId = MasterUtilityModel.RandomNumberLong().ToString();
                    Preferences.Set("BrainGamesUserId", Settings.UserId);//should be backed up automatically
                }
                else if (Settings.UserId == "-1" && hasKey == true) //a new device
                {
                    Settings.UserId = Preferences.Get("BrainGamesUserId", "-1");
                }
                else if (Settings.UserId != "-1" && hasKey == false)// userid already on device but not in cloud - should only happen in dev
                {
                    Preferences.Set("BrainGamesUserId", Settings.UserId); ;
                }
            }
            catch (Exception ex)
            {
                ;
            }

            Rg.Plugins.Popup.Popup.Init(this);//this, savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            global::Xamarin.Forms.FormsMaterial.Init(this, savedInstanceState);
            XF.Material.Droid.Material.Init(this, savedInstanceState);
            LoadApplication(new App());
            App.ScreenHeight = (int)(Resources.DisplayMetrics.HeightPixels / Resources.DisplayMetrics.Density);
            App.ScreenWidth = (int)(Resources.DisplayMetrics.WidthPixels / Resources.DisplayMetrics.Density);
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}