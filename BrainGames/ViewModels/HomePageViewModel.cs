using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using BrainGames.Views;

namespace BrainGames.ViewModels
{
    public class HomePageViewModel : BaseViewModel
    {
        private static bool first = true;

        private bool _isNotificationsEnabled = false;
        public bool IsNotificationsEnabled
        {
            get => _isNotificationsEnabled;
            set { SetProperty(ref _isNotificationsEnabled, value); }
        }

        private string _notificationsText = "";
        public string NotificationsText
        {
            get => _notificationsText;
            set { SetProperty(ref _notificationsText, value); }
        }

        public INavigation Navigation { get; set; }
        public ICommand GoButton_Clicked { get; protected set; }
        public HomePageViewModel(INavigation navigation)
        {
            this.Navigation = navigation;
            this.GoButton_Clicked = new Command(async () => await GoToInvitationsPage());
/*            this.GoButton_Clicked = new Command(async () => {

                await Application.Current.MainPage.Navigation.PushAsync(new InvitationsPage());
            });*/
        }

        public async Task OnAppearing()
        {
            if (first)
            {
                await App.mum.CheckInvitations();
                first = false;
            }

            if (App.mum.has_notifications)
            {
                NotificationsText = "Notifications";
                IsNotificationsEnabled = true;
            }
            else
            {
                NotificationsText = "";
                IsNotificationsEnabled = false;
            }
        }
        public async Task GoToInvitationsPage()
        {
            await Navigation.PushAsync(new InvitationsPage());
//            await Application.Current.MainPage.Navigation.PushAsync(new InvitationsPage());
        }
    }
}
