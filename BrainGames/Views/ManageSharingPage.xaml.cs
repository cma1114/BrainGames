using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrainGames.Models;
using BrainGames.ViewModels;
using BrainGames.Utility;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BrainGames.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ManageSharingPage : ContentPage
    {
        public InvitationsViewModel viewModel;

        void EditButtonPressed(object sender, EventArgs e)
        {
            foreach (Element el in ((Grid)(((StackLayout)((Button)sender).Parent.Parent)).Children[1]).Children)
            {
                if (el.GetType() != typeof(CheckBox)) continue;
                if (((CheckBox)el).IsVisible == false) continue;
                ((CheckBox)el).IsEnabled = true;
            }
            ((Button)sender).IsVisible = false;
            ((Button)((Grid)((Button)sender).Parent).Children[1]).IsVisible = true;
            ((Button)((Grid)((Button)sender).Parent).Children[2]).IsVisible = true;
        }

        void SaveButtonPressed(object sender, EventArgs e)
        {
            string games = "", game = "";
            foreach (Element el in ((Grid)(((StackLayout)((Button)sender).Parent.Parent)).Children[1]).Children)
            {
                if (el.GetType() == typeof(Label)) { game = ((Label)el).Text; continue; }
                if (el.GetType() == typeof(CheckBox) && ((CheckBox)el).IsChecked && ((CheckBox)el).IsVisible)
                {
                    if (game == "Inspection Time") games += "IT,";
                    if (game == "Reaction Time") games += "RT,";
                    if (game == "Stroop Effect") games += "Stroop,";
                    if (game == "Digit Span") games += "DS,";
                    if (game == "Location Span") games += "LS,";
                }
            }
            if (games.Length > 0) games = games.Substring(0, games.Length - 1);

            string screenname = ((Label)(((StackLayout)((Button)sender).Parent.Parent)).Children[0]).Text;
            viewModel.UpdateInvite(screenname, games);

            foreach (Element el in ((Grid)(((StackLayout)((Button)sender).Parent.Parent)).Children[1]).Children)
            {
                if (el.GetType() != typeof(CheckBox)) continue;
                if(((CheckBox)el).IsVisible == false) continue;
                ((CheckBox)el).IsEnabled = false;
            }
            ((Button)sender).IsVisible = false;
            ((Button)((Grid)((Button)sender).Parent).Children[0]).IsVisible = true;
            ((Button)((Grid)((Button)sender).Parent).Children[2]).IsVisible = false;
        }

        void CancelButtonPressed(object sender, EventArgs e)
        {
            bool shadowon = false;
            foreach (Element el in ((Grid)(((StackLayout)((Button)sender).Parent.Parent)).Children[1]).Children)
            {
                if (el.GetType() != typeof(CheckBox)) continue;
                if (((CheckBox)el).IsVisible == false)
                {
                    shadowon = ((CheckBox)el).IsChecked;
                    continue;
                }
                ((CheckBox)el).IsChecked = shadowon;
                ((CheckBox)el).IsEnabled = false;
            }
            ((Button)sender).IsVisible = false;
            ((Button)((Grid)((Button)sender).Parent).Children[0]).IsVisible = true;
            ((Button)((Grid)((Button)sender).Parent).Children[1]).IsVisible = false;
        }

        public ManageSharingPage()
        {
            viewModel = new InvitationsViewModel();
            BindingContext = this;
            InitializeComponent();
            if (App.mum.Shares.Count() == 0)
            {
                lblHeadline.Text = "You are not currently sharing games with anyone; head on over to the Invite Friends page and send some invitations to get started!";
            }
            else
            {
                lblHeadline.Text = "Sharing";
                foreach (SharingUserRecord su in App.mum.Shares)
                {
                    var f = new Frame
                    {
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        VerticalOptions = LayoutOptions.FillAndExpand,
                        Margin = 10,
                        Padding = 0,
                        CornerRadius = 0
                    };

                    var sl = new StackLayout { VerticalOptions = LayoutOptions.Center };
                    var l = new Label
                    {
                        Text = su.Screenname,
                        TextColor = Color.Black
                    };
                    sl.Children.Add(l);
                    var g = new Grid
                    {
                        Margin = new Thickness(20, 35, 20, 20),
                        ColumnDefinitions =
                        {
                            new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) },
                            new ColumnDefinition { Width = new GridLength(0.25, GridUnitType.Star) },
                            new ColumnDefinition { Width = new GridLength(0.25, GridUnitType.Star) }
                        }
                    };
                    for (int i = 0; i < DataSchemas.GameTypes.Count(); i++)
//                    for (int i = 0; i < su.games.Count(); i++)
                    {
                        int sidx = su.games.IndexOf(DataSchemas.GameTypes[i]);

                        g.RowDefinitions.Add(new RowDefinition());
                        var lbl = new Label { VerticalOptions = LayoutOptions.Center };
                        switch (DataSchemas.GameTypes[i]/*su.games[i]*/)
                        {
                            case "IT":
                                lbl.Text = "Inspection Time";
                                break;
                            case "RT":
                                lbl.Text = "Reaction Time";
                                break;
                            case "Stroop":
                                lbl.Text = "Stroop Effect";
                                break;
                            case "DS":
                                lbl.Text = "Digit Span";
                                break;
                            case "LS":
                                lbl.Text = "Location Span";
                                break;
                            default:
                                lbl.Text = "Unknown";
                                break;
                        }
                        g.Children.Add(lbl, 0, i);
                        var origchk = new CheckBox { IsVisible = false, IsChecked = (sidx >= 0 ? su.status[sidx] : false), VerticalOptions = LayoutOptions.Center, IsEnabled = false };
                        g.Children.Add(origchk, 1, i);
                        var chk = new CheckBox { IsChecked = (sidx >= 0 ? su.status[sidx] : false), VerticalOptions = LayoutOptions.Center, IsEnabled = false };
                        g.Children.Add(chk, 1, i);
                        var rb = new RadioButton { IsChecked = (sidx >= 0 ? su.theirstatus[sidx] : false), VerticalOptions = LayoutOptions.Center, IsEnabled = false };
                        g.Children.Add(rb, 2, i);
                    }
                    sl.Children.Add(g);
                    g = new Grid
                    {
                        Margin = new Thickness(20, 35, 20, 20),
                        ColumnDefinitions =
                        {
                            new ColumnDefinition(),
                            new ColumnDefinition()
                        }
                    };
                    var b = new Button { Text = "Edit", HorizontalOptions = LayoutOptions.CenterAndExpand };
                    b.Clicked += EditButtonPressed;
                    g.Children.Add(b, 0, 0);
                    b = new Button { Text = "Save", HorizontalOptions = LayoutOptions.CenterAndExpand, IsVisible = false };
                    b.Clicked += SaveButtonPressed;
                    g.Children.Add(b, 0, 0);
                    b = new Button { Text = "Cancel", HorizontalOptions = LayoutOptions.CenterAndExpand, IsVisible = false };
                    b.Clicked += CancelButtonPressed;
                    g.Children.Add(b, 1, 0);
                    sl.Children.Add(g);
                    f.Content = sl;
                    this.mainSL.Children.Add(f);
                }
            }
        }
    }
}