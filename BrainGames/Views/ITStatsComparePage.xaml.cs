using System;
using System.Collections.Generic;
using Xamarin.Forms;
using BrainGames.ViewModels;

namespace BrainGames.Views
{
    public partial class ITStatsComparePage : ContentPage
    {
        public ITStatsCompareViewModel ViewModel
        {
            get { return BindingContext as ITStatsCompareViewModel; }
            set { BindingContext = value; }
        }

        public ITStatsComparePage()
        {
            ViewModel = new ITStatsCompareViewModel();
            InitializeComponent();
        }
        async void Return_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }

    }
}
