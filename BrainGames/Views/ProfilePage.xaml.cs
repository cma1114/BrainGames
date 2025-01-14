﻿using System;
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

        async void Entry_Completed(object sender, EventArgs e)
        {
            string s = ((Entry)sender).Text.Trim();
            if (s.Length > 0 && viewModel.CheckName(s)) //cast sender to access the properties of the Entry
            {
                ScreennameEntry.Text = Settings.Screenname;
                ScreennameEntry.Placeholder = "Screenname";
                ScreennameEntry.IsEnabled = false;
                ScreennameLabel.Text = "";
                if (viewModel.Edit) await DisplayAlert("Success", "Screenname changed successfully!", "OK");
                else await DisplayAlert("Success", "Screenname set successfully!", "OK");
            }
            else
            {
                ScreennameEntry.Text = "";
                ScreennameEntry.Placeholder = "Choose Screenname";
                ScreennameLabel.Text = "That Screenname is already taken";
            }
        }

        void EditButton_Clicked(object sender, EventArgs e)
        {
            ScreennameEntry.IsEnabled = true;
            viewModel.Edit = true;
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
