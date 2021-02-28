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

        public static readonly BindableProperty CanSelectProperty = BindableProperty.Create(nameof(CanSelect), typeof(bool), typeof(HomePage), false, BindingMode.TwoWay);
        public bool CanSelect
        {
            get => (bool)GetValue(CanSelectProperty);
            set => SetValue(CanSelectProperty, value);
        }

        public static readonly BindableProperty ScreenNameProperty = BindableProperty.Create(nameof(ScreenName), typeof(string), typeof(HomePage), string.Empty, BindingMode.TwoWay);
        public string ScreenName
        {
            get => (string)GetValue(ScreenNameProperty);
            set => SetValue(ScreenNameProperty, value);
        }

        private bool ComingFromEC, ComingFromUnfocus, Firstpass;

        public InviteViewModel viewModel;

        public InvitePage()
        {
            viewModel = new InviteViewModel();
            BindingContext = this;
            InitializeComponent();
            Firstpass = true;
            ComingFromEC = false;
            ComingFromUnfocus = false;
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
            ScreennameEntry.BackgroundColor = Color.LightGray;
            CanSelect = false;
            ScreenName = string.Empty;
            ScreennameEntry.Placeholder = "Screenname";
            ScreennameEntry.IsEnabled = true;
            ScreennameLabel.Text = "Enter Screenname of user you wish to invite";
            ScreennameLabel.TextColor = Color.Gray;
            foreach (Element el in this.FindByName<Grid>("GameGrid").Children)
            {
                if (el.GetType() != typeof(CheckBox)) continue;
                ((CheckBox)el).IsChecked = false;
                ((CheckBox)el).Color = Color.Gray;
            }
            ComingFromEC = false;
            ComingFromUnfocus = false;
            Firstpass = true;
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
            if (!Firstpass) return;
            if (!ComingFromUnfocus) ComingFromEC = true;
            InviteOkay = false;
            string s = ((Entry)sender).Text.Trim();
            if (s.Length == 0 || s == Settings.Screenname) { ScreennameLabel.Text = ""; ScreennameLabel.TextColor = Color.Gray; Firstpass = false;  return; }
            if (viewModel.CheckName(s)) //cast sender to access the properties of the Entry
            {
                ScreennameEntry.Placeholder = "Screenname";
                ScreennameEntry.IsEnabled = false;
                ScreennameEntry.BackgroundColor = Color.LightGreen;
                ScreennameLabel.Text = "";
                CanSelect = true;
/*                foreach (Element el in this.FindByName<Grid>("GameGrid").Children)
                {
                    if (el.GetType() != typeof(CheckBox)) continue;
                    //                if (e.Value)
                    if (((CheckBox)el).IsChecked)
                    {
                        InviteOkay = true;
                        return;
                    }
                }*/

            }
            else
            {
                ScreennameEntry.Text = "";
                ScreennameEntry.Placeholder = "Enter Screenname of user you wish to invite";
                ScreennameLabel.Text = "That Screenname does not exist";
                ScreennameLabel.TextColor = Color.OrangeRed;
            }
            Firstpass = false;
        }

        public void OnTextChanged(object sender, EventArgs e)
        {
            ComingFromEC = false;
            ComingFromUnfocus = false;
            Firstpass = true;
        }

        public void OnFocus(object sender, EventArgs e)
        {
            ((SearchBar)sender).Text = "";
            ((SearchBar)sender).TextColor = Color.Black;
        }

        public void OnUnfocus(object sender, EventArgs e)
        {/*
            if (((SearchBar)sender).Text == "")
            {
                if (((SearchBar)sender).ClassId == "1") ((SearchBar)sender).Text = "Search Brand or Company";
                else ((SearchBar)sender).Text = "Search Product Category";
                ((SearchBar)sender).TextColor = Color.LightGray;
            }*/
            if (!ComingFromEC)
            {
                ComingFromUnfocus = true;
                Entry_Completed(sender, e);
            }
        }
    }
}
