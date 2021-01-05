using System;
using System.Collections.Generic;
using System.Linq;
using BrainGames.Utility;
using Foundation;
using UIKit;

namespace BrainGames.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {

            bool bSucceeded = false;
            string uid = null;
            var oKeyValueStore = NSUbiquitousKeyValueStore.DefaultStore;
            if (oKeyValueStore != null)
            {
                bSucceeded = oKeyValueStore.Synchronize();
                uid = oKeyValueStore.GetString("UserId");
            }

            if (Settings.UserId == "-1" && uid == null) //a new user
            {
                Settings.UserId = MasterUtilityModel.RandomNumberLong().ToString();
                if (oKeyValueStore != null)
                {
                    oKeyValueStore.SetString("UserId", Settings.UserId);
                    bSucceeded = oKeyValueStore.Synchronize();
                }
            }
            else if (Settings.UserId == "-1" && uid != null) //a new device
            {
                Settings.UserId = uid;
            }
            else if (Settings.UserId != "-1" && uid == null)// userid already on device but not in cloud - should only happen in dev
            {
                if (oKeyValueStore != null)
                {
                    oKeyValueStore.SetString("UserId", Settings.UserId);
                    bSucceeded = oKeyValueStore.Synchronize();
                }
            }

            Rg.Plugins.Popup.Popup.Init();
            global::Xamarin.Forms.Forms.Init();
            global::Xamarin.Forms.FormsMaterial.Init();
            XF.Material.iOS.Material.Init();
            LoadApplication(new App());
            App.ScreenHeight = (int)UIScreen.MainScreen.Bounds.Height;
            App.ScreenWidth = (int)UIScreen.MainScreen.Bounds.Width;

            return base.FinishedLaunching(app, options);
        }
    }
}
