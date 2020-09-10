using System;
using System.Collections.Generic;
using Xamarin.Forms;
using BrainGames.ViewModels;

namespace BrainGames.Views
{
    public partial class StroopStatsComparePage : ContentPage
    {
        public StroopStatsCompareViewModel ViewModel
        {
            get { return BindingContext as StroopStatsCompareViewModel; }
            set { BindingContext = value; }
        }

        public StroopStatsComparePage()
        {
            ViewModel = new StroopStatsCompareViewModel();
            InitializeComponent();
        }

        async void Return_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }

    }
}
