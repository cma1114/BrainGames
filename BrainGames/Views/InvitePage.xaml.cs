using System;
using System.Collections.Generic;
using Xamarin.Forms;
using BrainGames.ViewModels;
using BrainGames.Utility;
using System.Linq;

namespace BrainGames.Views
{
    public partial class InvitePage : ContentPage
    {
        public static readonly BindableProperty InviteOkayProperty = BindableProperty.Create(nameof(InviteOkay), typeof(bool), typeof(HomePage), false, BindingMode.TwoWay);
        public bool InviteOkay
        {
            get => (bool)GetValue(InviteOkayProperty);
            set => SetValue(InviteOkayProperty, value);
        }

        public InviteViewModel viewModel;

        public InvitePage()
        {
            viewModel = new InviteViewModel();
            BindingContext = this;
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Init();
        }

        private void Init()
        {
        }

        void Invite_Clicked(object sender, EventArgs e)
        {
            string games = "";
            if (this.FindByName<CheckBox>("chkIT").IsChecked) games += "IT,";
            if (this.FindByName<CheckBox>("chkRT").IsChecked) games += "RT,";
            if (this.FindByName<CheckBox>("chkStroop").IsChecked) games += "Stroop,";
            if (this.FindByName<CheckBox>("chkDS").IsChecked) games += "DS,";
            if (this.FindByName<CheckBox>("chkLS").IsChecked) games += "LS,";
            games = games.Substring(0, games.Length - 1);
            viewModel.SendInvite(games);
            InviteOkay = false;
            DisplayAlert("Invitation Sent!", ScreennameEntry.Text + " has been invited to share with you.", "OK");
            ScreennameEntry.Placeholder = "Screenname";
            ScreennameEntry.IsEnabled = true;
            ScreennameLabel.Text = "Enter Screenname of user you wish to invite";
            foreach (Element el in this.FindByName<Grid>("GameGrid").Children)
            {
                if (el.GetType() != typeof(CheckBox)) continue;
                ((CheckBox)el).IsChecked = false;
            }
        }

        void OnCheckChanged(object sender, CheckedChangedEventArgs e)
        {
            if (ScreennameLabel.Text != "")
            {
                InviteOkay = false;
                return;
            }
            foreach (Element el in this.FindByName<Grid>("GameGrid").Children)
            {
                if (el.GetType() != typeof(CheckBox)) continue;
//                if (e.Value)
                if (((CheckBox)el).IsChecked)
                {
                    InviteOkay = true;
                    return;
                }
            }
            InviteOkay = false;
        }
        void Entry_Completed(object sender, EventArgs e)
        {
            InviteOkay = false;
            string s = ((Entry)sender).Text.Trim();
            if (s.Length > 0 && s != Settings.Screenname && viewModel.CheckName(s)) //cast sender to access the properties of the Entry
            {
                ScreennameEntry.Placeholder = "Screenname";
                ScreennameEntry.IsEnabled = false;
                ScreennameLabel.Text = "";
                foreach (Element el in this.FindByName<Grid>("GameGrid").Children)
                {
                    if (el.GetType() != typeof(CheckBox)) continue;
                    //                if (e.Value)
                    if (((CheckBox)el).IsChecked)
                    {
                        InviteOkay = true;
                        return;
                    }
                }

            }
            else
            {
                ScreennameEntry.Text = "";
                ScreennameEntry.Placeholder = "Enter Screenname of user you wish to invite";
                ScreennameLabel.Text = "That Screenname does not exist";
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
