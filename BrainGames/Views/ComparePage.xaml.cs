using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Xamarin.Forms;
using Microcharts;
using BrainGames.ViewModels;
using BrainGames.Utility;

namespace BrainGames.Views
{
    public partial class ComparePage : ContentPage
    {
        public CompareViewModel viewModel
        {
            get { return BindingContext as CompareViewModel; }
            set { BindingContext = value; }
        }

        public ComparePage()
        {
            viewModel = new CompareViewModel();
            InitializeComponent();

            List<string> keys = viewModel.ThisUserStats.Keys.ToList();
            List<string> keystrs = new List<string>();
            keys.Sort();
            Frame f = null, fr = null;
            StackLayout sl = null;
            Label l = null, lbl = null;
            Grid g = null;
            string lblstr;
            Microcharts.Forms.ChartView cv = null;

            foreach (string key in keys)
            {
                if(!viewModel.OtherUserStats.ContainsKey(key)) { continue; }

                string keystr = viewModel.MapGameKey(key);

                if (!keystrs.Contains(keystr))
                {
                    keystrs.Add(keystr);

                    f = new Frame
                    {
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        VerticalOptions = LayoutOptions.FillAndExpand,
                        Margin = 2,
                        Padding = 0,
                        CornerRadius = 0
                    };

                    sl = new StackLayout { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.FillAndExpand };
                    l = new Label
                    {
                        Text = keystr,
                        TextColor = Color.Black,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalOptions = LayoutOptions.Start,
                        Margin = 10
                    };
                    sl.Children.Add(l);
                }

                fr = new Frame();

                g = new Grid
                {
                    Margin = new Thickness(5, 10, 5, 5),
                    RowSpacing = 20,
                    ColumnDefinitions =
                            {
                            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },//row labels
                            new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) }//content grid
                            },
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    HeightRequest = 150
                };

                l = new Label
                {
                    FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                    LineHeight = 1,
                    TextColor = Color.Black,
                    HorizontalOptions = LayoutOptions.CenterAndExpand
                };

                lbl = new Label
                {
                    FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                    LineHeight = 1,
                    TextColor = Color.Black,
                    HorizontalOptions = LayoutOptions.CenterAndExpand
                };

                cv = new Microcharts.Forms.ChartView
                {
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    BackgroundColor = Color.Transparent,
                };

                g.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
                g.Children.Add(l, 0, 2, 0, 1);
                g.Children.Add(lbl, 0, 1);
                g.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                g.Children.Add(cv, 1, 1);

                switch (keystr)
                {
                    case "Inspection Time":
                        fr.BackgroundColor = misc.getgradient(Color.IndianRed, Color.LightGreen,
                            (misc.RangeTrim((viewModel.OtherUserStats[key] - viewModel.ThisUserStats[key]) / viewModel.OtherUserSDs[key], 3d) - -3d) / 6d);
                        l.Text = "Average Correct IT";
                        lbl.Text = "You are " + Math.Round(Math.Abs(viewModel.ThisUserStats["IT"] - viewModel.OtherUserStats["IT"])) + " ms ";
                        lbl.Text += viewModel.ThisUserStats["IT"] <= viewModel.OtherUserStats["IT"] ? "faster " : "slower ";
                        lbl.Text += "than average";
                        cv.SetBinding(Microcharts.Forms.ChartView.ChartProperty, new Binding("IT_AvgCorITChart"));
                        break;
                    case "Reaction Time":
                        fr.BackgroundColor = misc.getgradient(Color.IndianRed, Color.LightGreen,
                            (misc.RangeTrim((viewModel.OtherUserStats[key] - viewModel.ThisUserStats[key]) / viewModel.OtherUserSDs[key], 3d) - -3d) / 6d);
                        if (key == "RT1")
                        {
                            l.Text = "Average RT: 1 Choice";
                            cv.SetBinding(Microcharts.Forms.ChartView.ChartProperty, new Binding("RT_Average1Chart"));
                        }
                        else if (key == "RT2")
                        {
                            l.Text = "Average RT: 2 Choice";
                            cv.SetBinding(Microcharts.Forms.ChartView.ChartProperty, new Binding("RT_Average2Chart"));
                        }
                        else
                        {
                            l.Text = "Average RT: 4 Choice";
                            cv.SetBinding(Microcharts.Forms.ChartView.ChartProperty, new Binding("RT_Average4Chart"));
                        }
                        lbl.Text = "You are " + Math.Round(Math.Abs(viewModel.ThisUserStats[key] - viewModel.OtherUserStats[key])) + " ms ";
                        lbl.Text += viewModel.ThisUserStats[key] <= viewModel.OtherUserStats[key] ? "faster " : "slower ";
                        lbl.Text += "than average";
                        break;
                    case "Stroop Effect":
                        fr.BackgroundColor = misc.getgradient(Color.IndianRed, Color.LightGreen,
                            (misc.RangeTrim((viewModel.OtherUserStats[key] - viewModel.ThisUserStats[key]) / viewModel.OtherUserSDs[key], 3d) - -3d) / 6d);
                        if (key == "Stroop1")
                        {
                            l.Text = "Average Correct IT";
                            cv.SetBinding(Microcharts.Forms.ChartView.ChartProperty, new Binding("Stroop_AvgCorRTChart"));
                            lbl.Text = "You are " + Math.Round(Math.Abs(viewModel.ThisUserStats[key] - viewModel.OtherUserStats[key])) + " ms ";
                            lbl.Text += viewModel.ThisUserStats[key] <= viewModel.OtherUserStats[key] ? "faster " : "slower ";
                            lbl.Text += "than average";
                        }
                        else
                        {
                            l.Text = "Average C-I Difference";
                            cv.SetBinding(Microcharts.Forms.ChartView.ChartProperty, new Binding("Stroop_AvgCIDifChart"));
                            lbl.Text = "You have a " + Math.Round(Math.Abs(viewModel.ThisUserStats[key] - viewModel.OtherUserStats[key])) + " ms ";
                            lbl.Text += viewModel.ThisUserStats[key] <= viewModel.OtherUserStats[key] ? "smaller " : "larger ";
                            lbl.Text += "incongruity cost than average";
                        }
                        break;
                    case "Digit Span":
                        fr.BackgroundColor = misc.getgradient(Color.IndianRed, Color.LightGreen,
                            (misc.RangeTrim((viewModel.ThisUserStats[key] - viewModel.OtherUserStats[key]) / viewModel.OtherUserSDs[key], 3d) - -3d) / 6d);
                        if (key == "DS1")
                        {
                            l.Text = "Longest Span: Forward";
                            cv.SetBinding(Microcharts.Forms.ChartView.ChartProperty, new Binding("DS_LongestFChart"));
                        }
                        else
                        {
                            l.Text = "Longest Span: Backward";
                            cv.SetBinding(Microcharts.Forms.ChartView.ChartProperty, new Binding("DS_LongestBChart"));
                        }
                        lbl.Text = "Your span is " + Math.Round(Math.Abs(viewModel.ThisUserStats[key] - viewModel.OtherUserStats[key]),1) + " digits ";
                        lbl.Text += viewModel.ThisUserStats[key] >= viewModel.OtherUserStats[key] ? "longer " : "shorter ";
                        lbl.Text += "than average";
                        break;
                    case "Location Span":
                        fr.BackgroundColor = misc.getgradient(Color.IndianRed, Color.LightGreen,
                            (misc.RangeTrim((viewModel.ThisUserStats[key] - viewModel.OtherUserStats[key]) / viewModel.OtherUserSDs[key], 3d) - -3d) / 6d);
                        if (key == "LS1")
                        {
                            l.Text = "Longest Span: Forward";
                            cv.SetBinding(Microcharts.Forms.ChartView.ChartProperty, new Binding("LS_LongestFChart"));
                        }
                        else
                        {
                            l.Text = "Longest Span: Backward";
                            cv.SetBinding(Microcharts.Forms.ChartView.ChartProperty, new Binding("LS_LongestBChart"));
                        }
                        lbl.Text = "Your span is " + Math.Round(Math.Abs(viewModel.ThisUserStats[key] - viewModel.OtherUserStats[key]),1) + " locations ";
                        lbl.Text += viewModel.ThisUserStats[key] >= viewModel.OtherUserStats[key] ? "longer " : "shorter ";
                        lbl.Text += "than average";
                        break;
                }
                fr.Content = g;
                sl.Children.Add(fr);
                f.Content = sl;
                this.SVSL.Children.Add(f);
            }
        }
    }
}
