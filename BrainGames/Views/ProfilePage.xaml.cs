using System;
using System.Collections.Generic;
using Xamarin.Forms;
using BrainGames.ViewModels;
using BrainGames.Utility;

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

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Init();
        }

        private void Init()
        {
            if (Settings.Screenname != "")
            {
                ScreennameEntry.Text = Settings.Screenname;
                ScreennameEntry.IsEnabled = false;
                ScreennameLabel.Text = "";
            }
        }

        void Entry_Completed(object sender, EventArgs e)
        {
            if(viewModel.CheckName(((Entry)sender).Text)) //cast sender to access the properties of the Entry
            {
                ScreennameEntry.Text = Settings.Screenname;
                ScreennameEntry.IsEnabled = false;
                ScreennameLabel.Text = "";
            }
            else
            {
                ScreennameEntry.Text = "";
                ScreennameEntry.Placeholder = "Choose Screenname";
                ScreennameLabel.Text = "That Screenname is already taken";
            }
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
