using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using BrainGames.Controls;
using BrainGames.ViewModels;

namespace BrainGames.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HomePage : ContentPage
    {
        public HomePageViewModel viewModel
        {
            get { return BindingContext as HomePageViewModel; }
            set { BindingContext = value; }
        }

        public HomePage()
        {
            NavigationPage.SetBackButtonTitle(this, "");
            viewModel = new HomePageViewModel(Navigation);
            InitializeComponent();
//            BindingContext = new HomePageViewModel(Navigation);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await viewModel.OnAppearing();
            if (ToolbarItems.Count == 0)
            {
                ToolbarItems.Add(new ToolbarItem
                {
                    //Icon = "notifications-icon.png", // change this based on your image requirements, Xamarin.Forms has more than one way to store images
                    Command = new Command(() =>
                    {
                        if (PopupNavigation.PopupStack.Count > 0)
                            PopupNavigation.PopAsync();
                        else
                            PopupNavigation.PushAsync(new NotificationsPanel());
                    })
                });
                ToolbarItems[0].SetBinding(ToolbarItem.IsEnabledProperty, new Binding("IsNotificationsEnabled"));
                ToolbarItems[0].SetBinding(ToolbarItem.TextProperty, new Binding("NotificationsText"));
                //                ToolbarItems[0].Clicked += Notifications_Clicked;
            }
            /*
            if (App.mum.has_notifications && ToolbarItems.Count == 0)
            {
                ToolbarItems.Add(new ToolbarItem
                {
                    Text = "Notifications"
    ,Command = new Command(() => 
    {
        if (PopupNavigation.PopupStack.Count > 0)
            PopupNavigation.PopAsync();
        else
            PopupNavigation.PushAsync(new NotificationsPanel());
    })
                });
//                ToolbarItems[0].Clicked += Notifications_Clicked;
            }*/
        }

        async void Notifications_Clicked(object sender, EventArgs e)
        {
            string action = await DisplayActionSheet("A user has invited you to share games with them", "Cancel", null, "Go to Invitations page to view");
            if(action != "") await Navigation.PushAsync(new InvitationsPage());
        }

        public async void ITPressed(object sender, EventArgs args)
        {
            await Navigation.PushAsync(new ITPage());
        }

        public async void RTPressed(object sender, EventArgs args)
        {
            await Navigation.PushAsync(new RTPage());
        }

        public async void StroopPressed(object sender, EventArgs args)
        {
            await Navigation.PushAsync(new StroopPage());
        }

        public async void DSPressed(object sender, EventArgs args)
        {
            await Navigation.PushAsync(new DSPage());
        }

        public async void LSPressed(object sender, EventArgs args)
        {
            await Navigation.PushAsync(new LSPage());
        }
    }
}