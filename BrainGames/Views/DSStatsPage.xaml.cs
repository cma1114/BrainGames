using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using BrainGames.ViewModels;


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
        }


        async void Return_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}