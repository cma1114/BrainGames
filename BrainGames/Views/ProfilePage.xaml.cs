using System;
using System.Collections.Generic;
using Xamarin.Forms;
using BrainGames.ViewModels;

namespace BrainGames.Views
{
    public partial class ProfilePage : ContentPage
    {
        public ProfileViewModel viewModel
        {
            get { return BindingContext as ProfileViewModel; }
            set { BindingContext = value; }
        }

        public ProfilePage()
        {
            viewModel = new ProfileViewModel();
            InitializeComponent();
        }

        void Entry_Completed(object sender, EventArgs e)
        {
            var text = ((Entry)sender).Text; //cast sender to access the properties of the Entry
        }

        public void OnFocus(object sender, EventArgs e)
        {
            ((SearchBar)sender).Text = "";
            ((SearchBar)sender).TextColor = Color.Black;
        }

        public void OnUnfocus(object sender, EventArgs e)
        {
            if (((SearchBar)sender).Text == "")
            {
                if (((SearchBar)sender).ClassId == "1") ((SearchBar)sender).Text = "Search Brand or Company";
                else ((SearchBar)sender).Text = "Search Product Category";
                ((SearchBar)sender).TextColor = Color.LightGray;
            }
        }
    }
}
