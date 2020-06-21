using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using BrainGames.ViewModels;

namespace BrainGames.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class ITStatsPage : ContentPage
    {
        public ITStatsViewModel ViewModel
        {
            get { return BindingContext as ITStatsViewModel; }
            set { BindingContext = value; }
        }

        public ITStatsPage()
        {
            ViewModel = new ITStatsViewModel();
            InitializeComponent();
        }


        async void Return_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}