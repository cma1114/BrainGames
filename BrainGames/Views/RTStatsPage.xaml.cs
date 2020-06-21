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
    public partial class RTStatsPage : ContentPage
    {
        public RTStatsViewModel ViewModel
        {
            get { return BindingContext as RTStatsViewModel; }
            set { BindingContext = value; }
        }

        public RTStatsPage()
        {
            ViewModel = new RTStatsViewModel();
            InitializeComponent();
        }


        async void Return_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}