using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using BrainGames.ViewModels;

namespace BrainGames.Views
{
    public partial class RTStatsComparePage : ContentPage
    {
        public RTStatsCompareViewModel ViewModel
        {
            get { return BindingContext as RTStatsCompareViewModel; }
            set { BindingContext = value; }
        }

        public RTStatsComparePage()
        {
            ViewModel = new RTStatsCompareViewModel();
            InitializeComponent();
        }

        async void Return_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }

    }
}
