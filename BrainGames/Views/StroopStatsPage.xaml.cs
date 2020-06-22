using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using BrainGames.ViewModels;


namespace BrainGames.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class StroopStatsPage : ContentPage
    {
        public StroopStatsViewModel ViewModel
        {
            get { return BindingContext as StroopStatsViewModel; }
            set { BindingContext = value; }
        }

        public StroopStatsPage()
        {
            ViewModel = new StroopStatsViewModel();
            InitializeComponent();
        }


        async void Return_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}