﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using BrainGames.ViewModels;
using BrainGames.Utility;

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
            if (ViewModel.Compare)
            {
                ToolbarItems.Add(new ToolbarItem { Text = "Compare", Order = ToolbarItemOrder.Secondary, Priority = 1 });
                ToolbarItems[1].Clicked += Compare_Clicked;
            }
        }


        async void Return_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }

        async void Compare_Clicked(object sender, EventArgs e)
        {
            App.AnalyticsService.TrackEvent("RTStatsCompareView", new Dictionary<string, string> {
                    { "Type", "PageView" },
                    { "UserID", Settings.UserId.ToString()}
                });
            await Navigation.PushModalAsync(new NavigationPage(new RTStatsComparePage()));
        }
    }
}