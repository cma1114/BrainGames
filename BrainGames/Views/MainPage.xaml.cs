using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using BrainGames.Models;
using BrainGames.Utility;

namespace BrainGames.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : MasterDetailPage
    {
        Dictionary<int, NavigationPage> MenuPages = new Dictionary<int, NavigationPage>();
        public MainPage()
        {
            InitializeComponent();

            MasterBehavior = MasterBehavior.Popover;

            MenuPages.Add((int)MenuItemType.Play, (NavigationPage)Detail);
            /*
            IsPresentedChanged += (sender, args) =>
            {
                if (App.mum.has_notifications && !ToolbarItems.Contains()
                {
                    ToolbarItems.Add(new ToolbarItem
                    {
                        Text = "Notifications"
                    });
                }
            };*/
            //IsPresentedChanged += CheckNotifications;
        }

        public void CheckNotifications(object sender, EventArgs args)
        {
            if (App.mum.has_notifications && ToolbarItems.Count == 0)
                {
                ToolbarItems.Add(new ToolbarItem
                {
                    Text = "Notifications"
                });
            }
        }

        public async Task NavigateFromMenu(int id)
        {
            if (!MenuPages.ContainsKey(id))
            {
                switch (id)
                {
                    case (int)MenuItemType.Play:
                        MenuPages.Add(id, new NavigationPage(new HomePage()));
                        break;
                    case (int)MenuItemType.Invite:
                        if(Settings.Screenname != "") MenuPages.Add(id, new NavigationPage(new InvitePage()));
                        else await DisplayAlert("Select Screenname", "In order to invite friends you must first select a Screename for yourself on the Profile page", "OK");
                        break;
                    case (int)MenuItemType.Invitations:
                        MenuPages.Add(id, new NavigationPage(new InvitationsPage()));
                        break;
                    case (int)MenuItemType.Profile:
                        MenuPages.Add(id, new NavigationPage(new ProfilePage()));
                        break;
                    case (int)MenuItemType.About:
                        MenuPages.Add(id, new NavigationPage(new AboutPage()));
                        break;
                }
            }

            var newPage = MenuPages[id];

            if (newPage != null && Detail != newPage)
            {
                Detail = newPage;

                if (Device.RuntimePlatform == Device.Android)
                    await Task.Delay(100);

                IsPresented = false;
            }
        }
    }
}