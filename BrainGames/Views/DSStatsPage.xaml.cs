using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using BrainGames.ViewModels;
using BrainGames.Utility;


namespace BrainGames.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DSStatsPage : ContentPage
    {
        public DSStatsViewModel ViewModel
        {
            get { return BindingContext as DSStatsViewModel; }
            set { BindingContext = value; }
        }

        public DSStatsPage()
        {
            ViewModel = new DSStatsViewModel();
            InitializeComponent();
            if (ViewModel.Compare)
            {
                ToolbarItems.Add(new ToolbarItem { Text = "Compare", Order = ToolbarItemOrder.Secondary, Priority = 1 });
                ToolbarItems[1].Clicked += Compare_Clicked;
            }
        }


        async void Return_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }

        async void Compare_Clicked(object sender, EventArgs e)
        {
            App.AnalyticsService.TrackEvent("DSStatsCompareView", new Dictionary<string, string> {
                    { "Type", "PageView" },
                    { "UserID", Settings.UserId.ToString()}
                });
            await Navigation.PushModalAsync(new NavigationPage(new DSStatsComparePage()));
        }
    }
}
