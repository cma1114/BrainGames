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
    public partial class InvitationsPage : ContentPage
    {
        public InvitationsViewModel viewModel;

        public static readonly BindableProperty GamesToAcceptProperty = BindableProperty.Create(nameof(GamesToAccept), typeof(bool), typeof(HomePage), true, BindingMode.TwoWay);
        public bool GamesToAccept
        {
            get => (bool)GetValue(GamesToAcceptProperty);
            set => SetValue(GamesToAcceptProperty, value);
        }
        void OnCheckChanged(object sender, CheckedChangedEventArgs e)
        {
            foreach (Element el in (((Grid)((CheckBox)sender).Parent)).Children)
            {
                if (el.GetType() != typeof(CheckBox)) continue;
                //                if (e.Value)
                if (((CheckBox)el).IsChecked)
                {
                    GamesToAccept = true;
                    return;
                }
            }
            GamesToAccept = false;
        }

        void AcceptButtonPressed(object sender, EventArgs e)
        {
            string games = "", game = "";
            foreach (Element el in ((Grid)(((StackLayout)((Button)sender).Parent.Parent)).Children[1]).Children)
            {
                if (el.GetType() == typeof(Label)) { game = ((Label)el).Text; continue; }
                if (el.GetType() == typeof(CheckBox) && ((CheckBox)el).IsChecked)
                {
                    if (game == "Inspection Time") games += "IT,";
                    if (game == "Reaction Time") games += "RT,";
                    if (game == "Stroop Effect") games += "Stroop,";
                    if (game == "Digit Span") games += "DS,";
                    if (game == "Location Span") games += "LS,";
                }
            }
            games = games.Substring(0, games.Length - 1);
            string screenname = ((Label)(((StackLayout)((Button)sender).Parent.Parent)).Children[0]).Text.Split(new[] { " has " }, StringSplitOptions.None)[0];
            viewModel.AcceptInvite(screenname, games);
            GamesToAccept = false;
        }

        void RejectButtonPressed(object sender, EventArgs e)
        {
            string screenname = ((Label)(((StackLayout)((Button)sender).Parent.Parent)).Children[0]).Text.Split(new[] { " has " }, StringSplitOptions.None)[0];
            viewModel.RejectInvite(screenname);
        }

        public InvitationsPage()
        {
            viewModel = new InvitationsViewModel();
            BindingContext = this;
            InitializeComponent();
            if (App.mum.has_notifications == false)
            {
                lblHeadline.Text = "No Outstanding Invitations";
            }
            else
            {
                lblHeadline.Text = "Invitations";
                foreach (SharingInvitation su in App.mum.Invitations)
                {
                    su.games.Sort();
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
                        Text = su.Screenname + " has invited you to share the following game" + (su.games.Count() > 1 ? "s" : "") + ":",
                        TextColor = Color.Black 
                    };
                    sl.Children.Add(l);
                    var g = new Grid 
                    { 
                        Margin = new Thickness(20,35,20,20),
                        ColumnDefinitions =
                        {
                            new ColumnDefinition { Width = new GridLength(0.75, GridUnitType.Star) },
                            new ColumnDefinition { Width = new GridLength(0.25, GridUnitType.Star) }
                        }
                    };
                    for (int i = 0; i < su.games.Count(); i++)
                    {
                        g.RowDefinitions.Add(new RowDefinition());
                        var lbl = new Label { VerticalOptions = LayoutOptions.Center };
                        switch (su.games[i])
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
                        var chk = new CheckBox { IsChecked = true, VerticalOptions = LayoutOptions.Center };
                        chk.CheckedChanged += OnCheckChanged;
                        g.Children.Add(chk, 1, i);
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
                    var b = new Button { Text = "Accept Selected", HorizontalOptions = LayoutOptions.CenterAndExpand, IsEnabled = GamesToAccept};
                    b.Clicked += AcceptButtonPressed;
                    g.Children.Add(b, 0, 0);
                    b = new Button { Text = "Reject All", HorizontalOptions = LayoutOptions.CenterAndExpand };
                    b.Clicked += RejectButtonPressed;
                    g.Children.Add(b, 1, 0);
                    sl.Children.Add(g);
                    f.Content = sl;
                    this.mainSL.Children.Add(f);
                }
            }
        }
    }
}