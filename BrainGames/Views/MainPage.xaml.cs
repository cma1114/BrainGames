using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Xamarin.Forms;
using XF.Material.Forms.UI.Dialogs;
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

//////            MenuPages.Add((int)MenuItemType.Play, (NavigationPage)Detail);

            masterPage.listView.ItemSelected += OnItemSelected;
            this.IsPresentedChanged += OnPresentedChanged;


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

        private void OnPresentedChanged(object sender, EventArgs e)
        {
            if (this.IsPresented)
            {
                App.AnalyticsService.TrackEvent("SideNavView", new Dictionary<string, string> {
                    { "Type", "PageView" },
                    { "UserID", Settings.UserId.ToString()}
                });
            }
        }

        private void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var item = e.SelectedItem as MasterPageItem;

            if (item != null)
            {
                if (item.TargetType == null)
                {
                    App.AnalyticsService.TrackEvent("FeedbackClick", new Dictionary<string, string> {
                        { "Type", "MenuSelection" },
                        { "UserID", Settings.UserId.ToString() }
                    });
                    OnFeedbackSelected();
                }
                else if (item.TargetType.Name == nameof(InvitePage))
                {
                    App.AnalyticsService.TrackEvent("InvitePageClick", new Dictionary<string, string> {
                        { "Type", "MenuSelection" },
                        { "UserID", Settings.UserId.ToString()}
                    });
                    Detail = new NavigationPage((Page)Activator.CreateInstance(item.TargetType));//, _purchaseService));
                }
                else if (item.TargetType.Name == nameof(InvitationsPage))
                {
                    App.AnalyticsService.TrackEvent("InvitationsPageClick", new Dictionary<string, string> {
                        { "Type", "MenuSelection" },
                        { "UserID", Settings.UserId.ToString()}
                    });
                    if (/*Settings.ActiveSubscription ==*/ true)
                    {
                        Detail = new NavigationPage((Page)Activator.CreateInstance(item.TargetType));
                    }
                    else
                    {
//                        OnDiagramsSelected();
                    }
                }
                else
                {
                    App.AnalyticsService.TrackEvent(item.Title.Replace(" ", "") + "Click", new Dictionary<string, string> {
                        { "Type", "MenuSelection" },
                        { "UserID", Settings.UserId.ToString()}
                    });
                    Detail = new NavigationPage((Page)Activator.CreateInstance(item.TargetType));
                }
                masterPage.listView.SelectedItem = null;
                IsPresented = false;
            }
        }

        private async void OnFeedbackSelected()
        {
            var feedbackView = new Controls.FeedbackView();
            var input = await MaterialDialog.Instance
                                            .ShowCustomContentAsync(view: feedbackView,
                                                                    title: "Feedback",
                                                                    message: "We'd love to hear your thoughts:",
                                                                    confirmingText: "Send",
                                                                    configuration: App.GetMaterialAlertDialogConfiguration());
            if (input.HasValue && input.Value) // send feedback
            {
                var text = feedbackView.FeedbackEditor.Text;
                MasterUtilityModel.WriteUF("0", "", "", text);
            }
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
                    case (int)MenuItemType.Sharing:
                        MenuPages.Add(id, new NavigationPage(new ManageSharingPage()));
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