using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using BrainGames.ViewModels;


namespace BrainGames.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LSStatsPage : ContentPage
    {
        public LSStatsViewModel ViewModel
        {
            get { return BindingContext as LSStatsViewModel; }
            set { BindingContext = value; }
        }

        public LSStatsPage()
        {
            ViewModel = new LSStatsViewModel();
            InitializeComponent();
        }


        async void Return_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}