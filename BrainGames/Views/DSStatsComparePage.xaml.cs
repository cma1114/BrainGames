using System;
using System.Collections.Generic;
using Xamarin.Forms;
using BrainGames.ViewModels;

namespace BrainGames.Views
{
    public partial class DSStatsComparePage : ContentPage
    {
        public DSStatsCompareViewModel ViewModel
        {
            get { return BindingContext as DSStatsCompareViewModel; }
            set { BindingContext = value; }
        }

        public DSStatsComparePage()
        {
            ViewModel = new DSStatsCompareViewModel();
            InitializeComponent();
        }

        async void Return_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}
