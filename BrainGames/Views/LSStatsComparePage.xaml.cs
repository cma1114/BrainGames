using System;
using System.Collections.Generic;
using Xamarin.Forms;
using BrainGames.ViewModels;

namespace BrainGames.Views
{
    public partial class LSStatsComparePage : ContentPage
    {
        public LSStatsCompareViewModel ViewModel
        {
            get { return BindingContext as LSStatsCompareViewModel; }
            set { BindingContext = value; }
        }

        public LSStatsComparePage()
        {
            ViewModel = new LSStatsCompareViewModel();
            InitializeComponent();
        }
        async void Return_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }

    }
}
