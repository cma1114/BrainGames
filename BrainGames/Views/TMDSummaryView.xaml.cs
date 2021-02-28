using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Xamarin.Forms;
using BrainGames.ViewModels;

namespace BrainGames.Views
{
    public partial class TMDSummaryView : ScrollView
    {
        public TMDViewModel viewModel
        {
            get { return BindingContext as TMDViewModel; }
            set { BindingContext = value; }
        }

        public TMDSummaryView(TMDViewModel _viewModel)
        {
            viewModel = _viewModel;
            InitializeComponent();

            var fr = new Frame
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                Margin = 2,
                Padding = 0,
                CornerRadius = 0
            };
            fr.SetBinding(Frame.BackgroundColorProperty, new Binding("TotalScoreColor"));

            var slt = new StackLayout { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
            var lb = new Label
            {
                TextColor = Color.Black,
                FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.CenterAndExpand
            };
            lb.SetBinding(Label.TextProperty, new Binding("TotalScoreString"));
            slt.Children.Add(lb);
            lb = new Label
            {
                TextColor = Color.Black,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Start
            };
            lb.SetBinding(Label.TextProperty, new Binding("SpeedScoreString"));
            lb.SetBinding(Label.BackgroundColorProperty, new Binding("SpeedScoreColor"));
            slt.Children.Add(lb);
            lb = new Label
            {
                TextColor = Color.Black,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Start
            };
            lb.SetBinding(Label.TextProperty, new Binding("MemoryScoreString"));
            lb.SetBinding(Label.BackgroundColorProperty, new Binding("MemoryScoreColor"));
            slt.Children.Add(lb);
            fr.Content = slt;
            this.TwoMinuteDrillSummaryView.Children.Add(fr);

            double points = 0;
            viewModel.MemoryScore = 0;
            viewModel.SpeedScore = 0;
            foreach (string gamename in viewModel.GamesPlayed)
            {
                var f = new Frame
                {
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    Margin = 2,
                    Padding = 0,
                    CornerRadius = 0
                };

                var sl = new StackLayout { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
                var l = new Label
                {
                    Text = gamename,
                    TextColor = Color.Black,
                    FontAttributes = FontAttributes.Bold,
                    HorizontalOptions = LayoutOptions.Start
                };
                sl.Children.Add(l);

                Grid g = null, g2 = null;
                Label lbl = null;
                int i;
                points = 0;
                switch (gamename)
                {
                    case "Inspection Time":
                        g = new Grid
                        {
                            Margin = new Thickness(10, 20, 10, 10),
                            ColumnDefinitions =
                            {
                            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },//row labels
                            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }//content grid
                            }
                        };
                        g.RowDefinitions.Add(new RowDefinition());//column labels
                        lbl = new Label { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
                        lbl.Text = "Ontime";
                        g.Children.Add(lbl, 0, 0);
                        i = 1;
                        foreach (var item in viewModel.ITscores.OrderBy(x => x.Key))
                        {
                            g.RowDefinitions.Add(new RowDefinition());//row label
                            lbl = new Label { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
                            lbl.Text = item.Key.ToString() + " ms";
                            g.Children.Add(lbl, 0, i++);
                        }
                        g2 = new Grid
                        {
                            ColumnDefinitions =
                            {
                            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },//This round
                            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) }//Historical
                            }
                        };
                        g2.RowDefinitions.Add(new RowDefinition());//row labels
                        lbl = new Label { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
                        lbl.Text = "This Round";
                        g2.Children.Add(lbl, 0, 0);
                        lbl = new Label { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
                        lbl.Text = "Historical";
                        g2.Children.Add(lbl, 1, 0);
                        i = 1;
                        foreach (var item in viewModel.ITscores.OrderBy(x => x.Key))
                        {
                            g2.RowDefinitions.Add(new RowDefinition());//row content
                            lbl = new Label { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
                            lbl.Text = item.Value[0].ToString() + " / " + item.Value[1].ToString();
                            lbl.Text += " (" + ((double)item.Value[0] / item.Value[1] * 100).ToString("N0", CultureInfo.InvariantCulture) + "%)";
                            g2.Children.Add(lbl, 0, i);

                            lbl = new Label { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
                            if (viewModel.it_AvgCorPctByStimDur != null && viewModel.it_AvgCorPctByStimDur.ContainsKey(item.Key))
                            {
                                lbl.Text = (viewModel.it_AvgCorPctByStimDur[item.Key] * 100).ToString("N0", CultureInfo.InvariantCulture) + "%";
                                if ((double)item.Value[0] / item.Value[1] > viewModel.it_AvgCorPctByStimDur[item.Key])
                                {
                                    ((Label)g2.Children[g2.Children.Count() - 1]).BackgroundColor = Color.ForestGreen;
                                }
                                else if ((double)item.Value[0] / item.Value[1] < viewModel.it_AvgCorPctByStimDur[item.Key])
                                {
                                    ((Label)g2.Children[g2.Children.Count() - 1]).BackgroundColor = Color.OrangeRed;
                                }
                                viewModel.itpoints += (double)item.Value[0] - (item.Value[1] * viewModel.it_AvgCorPctByStimDur[item.Key]);
                            }
                            else
                            {
                                lbl.Text = "N/A";
                            }
                            g2.Children.Add(lbl, 1, i);
                            i++;
                        }
                        points = viewModel.itpoints;
                        g.Children.Add(g2, 1, 0);
                        Grid.SetRowSpan(g2, i);
                        sl.Children.Add(g);
                        break;
                    case "Reaction Time":
                        g = new Grid
                        {
                            Margin = new Thickness(10, 20, 10, 10),
                            ColumnDefinitions =
                            {
                            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },//row labels
                            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }//content grid
                            }
                        };
                        g.RowDefinitions.Add(new RowDefinition());//column labels
                        lbl = new Label { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
                        lbl.Text = "";
                        g.Children.Add(lbl, 0, 0);
                        g.RowDefinitions.Add(new RowDefinition());//row label
                        lbl = new Label { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
                        lbl.Text = "Avg RT";
                        g.Children.Add(lbl, 0, 1);
                        g2 = new Grid
                        {
                            ColumnDefinitions =
                            {
                            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },//This round
                            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) }//Historical
                            }
                        };
                        g2.RowDefinitions.Add(new RowDefinition());//row labels
                        lbl = new Label { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
                        lbl.Text = "This Round";
                        g2.Children.Add(lbl, 0, 0);
                        lbl = new Label { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
                        lbl.Text = "Historical";
                        g2.Children.Add(lbl, 1, 0);

                        g2.RowDefinitions.Add(new RowDefinition());//row content
                        lbl = new Label { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
                        lbl.Text = (viewModel.RTReactionTime / viewModel.RTtrialsperset).ToString("N1", CultureInfo.InvariantCulture) + " ms";
                        g2.Children.Add(lbl, 0, 1);
                        lbl = new Label { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
                        lbl.Text = (viewModel.RTss1_trialcnt > 0) ? (viewModel.RTss1_cumrt / viewModel.RTss1_trialcnt).ToString("N1", CultureInfo.InvariantCulture) + " ms" : "N/A";
                        g2.Children.Add(lbl, 1, 1);
                        g.Children.Add(g2, 1, 0);
                        Grid.SetRowSpan(g2, 2);
                        sl.Children.Add(g);
                        if (viewModel.RTss1_trialcnt > 0) viewModel.rtpoints = Math.Round(((viewModel.RTss1_cumrt / viewModel.RTss1_trialcnt) - (viewModel.RTReactionTime / viewModel.RTtrialsperset)) / 20);
                        points = viewModel.rtpoints;
                        break;
                    case "Stroop Effect":
                        g = new Grid
                        {
                            Margin = new Thickness(10, 20, 10, 10),
                            ColumnDefinitions =
                            {
                            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },//row labels
                            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }//content grid
                            }
                        };
                        g.RowDefinitions.Add(new RowDefinition());//column labels
                        lbl = new Label { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
                        lbl.Text = "";
                        g.Children.Add(lbl, 0, 0);
                        g.RowDefinitions.Add(new RowDefinition());//row label
                        lbl = new Label { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
                        lbl.Text = "Avg RT";
                        g.Children.Add(lbl, 0, 1);
                        g.RowDefinitions.Add(new RowDefinition());//row label
                        lbl = new Label { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
                        lbl.Text = "I-C Dif";
                        g.Children.Add(lbl, 0, 2);
                        g.RowDefinitions.Add(new RowDefinition());//row label
                        lbl = new Label { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
                        lbl.Text = "C Cor%";
                        g.Children.Add(lbl, 0, 3);
                        g.RowDefinitions.Add(new RowDefinition());//row label
                        lbl = new Label { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
                        lbl.Text = "I Cor%";
                        g.Children.Add(lbl, 0, 4);
                        g2 = new Grid
                        {
                            ColumnDefinitions =
                            {
                            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },//This round
                            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) }//Historical
                            }
                        };
                        g2.RowDefinitions.Add(new RowDefinition());//row labels
                        lbl = new Label { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
                        lbl.Text = "This Round";
                        g2.Children.Add(lbl, 0, 0);
                        lbl = new Label { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
                        lbl.Text = "Historical";
                        g2.Children.Add(lbl, 1, 0);

                        double crt = viewModel.Stroopcorcontrialcnt > 0 ? viewModel.StroopConReactionTime / viewModel.Stroopcorcontrialcnt : 1000;
                        double irt = viewModel.Stroopcorincontrialcnt > 0 ? viewModel.StroopInconReactionTime / viewModel.Stroopcorincontrialcnt : 1000;
                        g2.RowDefinitions.Add(new RowDefinition());//row content
                        lbl = new Label { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
                        lbl.Text = ((crt + irt) / 2).ToString("N1", CultureInfo.InvariantCulture) + " ms";
                        bool histcor = false;
                        double histrt = 0, histrtdif = 0, hcp = 0, hip = 0;
                        if (viewModel.Stroophistcorcontrialcnt > 0 && viewModel.Stroophistcorincontrialcnt > 0)
                        {
                            histcor = true;
                            histrt = ((viewModel.Stroophistcumconcorrt / viewModel.Stroophistcorcontrialcnt + viewModel.Stroophistcuminconcorrt / viewModel.Stroophistcorincontrialcnt) / 2);
                            viewModel.stpoints += Math.Round((histrt - ((crt + irt) / 2)) / 30);
                            if (histrt > ((crt + irt) / 2)) lbl.BackgroundColor = Color.ForestGreen;
                            if (histrt < ((crt + irt) / 2)) lbl.BackgroundColor = Color.OrangeRed;

                            histrtdif = ((viewModel.Stroophistcuminconcorrt / viewModel.Stroophistcorincontrialcnt) - (viewModel.Stroophistcumconcorrt / viewModel.Stroophistcorcontrialcnt));
                            viewModel.stpoints += Math.Round((histrtdif - (irt - crt)) / 50);
                        }
                        g2.Children.Add(lbl, 0, 1);
                        lbl = new Label { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
                        lbl.Text = histcor ? histrt.ToString("N1", CultureInfo.InvariantCulture) + " ms" : "N/A";
                        g2.Children.Add(lbl, 1, 1);

                        g2.RowDefinitions.Add(new RowDefinition());//row content
                        lbl = new Label { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
                        lbl.Text = (irt - crt).ToString("N1", CultureInfo.InvariantCulture) + " ms";
                        if (histcor && histrtdif > (irt - crt)) lbl.BackgroundColor = Color.ForestGreen;
                        if (histcor && histrtdif < (irt - crt)) lbl.BackgroundColor = Color.OrangeRed;
                        g2.Children.Add(lbl, 0, 2);
                        lbl = new Label { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
                        lbl.Text = histcor ? histrtdif.ToString("N1", CultureInfo.InvariantCulture) + " ms" : "N/A";
                        g2.Children.Add(lbl, 1, 2);

                        g2.RowDefinitions.Add(new RowDefinition());//row content
                        double cp = viewModel.Stroopcontrialcnt > 0 ? (double)viewModel.Stroopcorcontrialcnt / viewModel.Stroopcontrialcnt : 0;
                        lbl = new Label { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
                        lbl.Text = (cp * 100).ToString("N0", CultureInfo.InvariantCulture) + "%";
                        if (viewModel.Stroophistcontrialcnt > 0)
                        {
                            hcp = ((double)viewModel.Stroophistcorcontrialcnt / viewModel.Stroophistcontrialcnt);
                            viewModel.stpoints += (cp > hcp * 1.1) ? 1 : 0;
                            viewModel.stpoints += (cp < hcp * 0.9) ? -1 : 0;
                            if (cp > hcp) lbl.BackgroundColor = Color.ForestGreen;
                            if (cp < hcp) lbl.BackgroundColor = Color.OrangeRed;
                        }
                        g2.Children.Add(lbl, 0, 3);
                        lbl = new Label { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
                        lbl.Text = (viewModel.Stroophistcontrialcnt > 0) ? (hcp * 100).ToString("N0", CultureInfo.InvariantCulture) + "%" : "N/A";
                        g2.Children.Add(lbl, 1, 3);

                        g2.RowDefinitions.Add(new RowDefinition());//row content
                        double ip = viewModel.Stroopincontrialcnt > 0 ? (double)viewModel.Stroopcorincontrialcnt / viewModel.Stroopincontrialcnt : 0;
                        lbl = new Label { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
                        lbl.Text = (ip * 100).ToString("N0", CultureInfo.InvariantCulture) + "%";
                        if (viewModel.Stroophistincontrialcnt > 0)
                        {
                            hip = ((double)viewModel.Stroophistcorincontrialcnt / viewModel.Stroophistincontrialcnt);
                            viewModel.stpoints += (ip > hip * 1.1) ? 1 : 0;
                            viewModel.stpoints += ip < (hip * 0.9) ? -1 : 0;
                            if (ip > hip) lbl.BackgroundColor = Color.ForestGreen;
                            if (ip < hip) lbl.BackgroundColor = Color.OrangeRed;
                        }
                        g2.Children.Add(lbl, 0, 4);
                        lbl = new Label { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
                        lbl.Text = (viewModel.Stroophistincontrialcnt > 0) ? (hip * 100).ToString("N0", CultureInfo.InvariantCulture) + "%" : "N/A";
                        g2.Children.Add(lbl, 1, 4);
                        g.Children.Add(g2, 1, 0);
                        Grid.SetRowSpan(g2, 5);
                        sl.Children.Add(g);
                        points = viewModel.stpoints;
                        break;
                    case "Digit Span":
                        g = new Grid
                        {
                            Margin = new Thickness(10, 20, 10, 10),
                            ColumnDefinitions =
                            {
                            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },//row labels
                            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }//content grid
                            }
                        };
                        g.RowDefinitions.Add(new RowDefinition());//column labels
                        lbl = new Label { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
                        lbl.Text = "Digits";
                        g.Children.Add(lbl, 0, 0);
                        i = 1;
                        foreach (var item in viewModel.DSscores.OrderBy(x => x.Key))
                        {
                            g.RowDefinitions.Add(new RowDefinition());//row label
                            lbl = new Label { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
                            lbl.Text = item.Key.ToString();
                            g.Children.Add(lbl, 0, i++);
                        }
                        g2 = new Grid
                        {
                            ColumnDefinitions =
                            {
                            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },//This round
                            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) }//Historical
                            }
                        };
                        g2.RowDefinitions.Add(new RowDefinition());//row labels
                        lbl = new Label { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
                        lbl.Text = "This Round";
                        g2.Children.Add(lbl, 0, 0);
                        lbl = new Label { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
                        lbl.Text = "Historical";
                        g2.Children.Add(lbl, 1, 0);
                        i = 1;
                        foreach (var item in viewModel.DSscores.OrderBy(x => x.Key))
                        {
                            g2.RowDefinitions.Add(new RowDefinition());//row content
                            lbl = new Label { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
                            lbl.Text = item.Value[0].ToString() + " / " + item.Value[1].ToString();
                            lbl.Text += " (" + ((double)item.Value[0] / item.Value[1] * 100).ToString("N0", CultureInfo.InvariantCulture) + "%)";
                            g2.Children.Add(lbl, 0, i);

                            lbl = new Label { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
                            if (viewModel.ds_AvgCorPctBySpanLen_f != null && viewModel.ds_AvgCorPctBySpanLen_f.ContainsKey((int)item.Key))
                            {
                                lbl.Text = (viewModel.ds_AvgCorPctBySpanLen_f[(int)item.Key] * 100).ToString("N0", CultureInfo.InvariantCulture) + "%";
                                if ((double)item.Value[0] / item.Value[1] > viewModel.ds_AvgCorPctBySpanLen_f[(int)item.Key])
                                {
                                    ((Label)g2.Children[g2.Children.Count() - 1]).BackgroundColor = Color.ForestGreen;
                                }
                                else if ((double)item.Value[0] / item.Value[1] < viewModel.ds_AvgCorPctBySpanLen_f[(int)item.Key])
                                {
                                    ((Label)g2.Children[g2.Children.Count() - 1]).BackgroundColor = Color.OrangeRed;
                                }
                                viewModel.dspoints += (double)item.Value[0] - (item.Value[1] * viewModel.ds_AvgCorPctBySpanLen_f[(int)item.Key]);
                            }
                            else
                            {
                                lbl.Text = "N/A";
                            }
                            g2.Children.Add(lbl, 1, i);
                            i++;
                        }
                        g.Children.Add(g2, 1, 0);
                        Grid.SetRowSpan(g2, i);
                        sl.Children.Add(g);
                        points = viewModel.dspoints;
                        break;
                    case "Location Span":
                        g = new Grid
                        {
                            Margin = new Thickness(10, 20, 10, 10),
                            ColumnDefinitions =
                            {
                            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },//row labels
                            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }//content grid
                            }
                        };
                        g.RowDefinitions.Add(new RowDefinition());//column labels
                        lbl = new Label { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
                        lbl.Text = "Locations";
                        g.Children.Add(lbl, 0, 0);
                        i = 1;
                        foreach (var item in viewModel.LSscores.OrderBy(x => x.Key))
                        {
                            g.RowDefinitions.Add(new RowDefinition());//row label
                            lbl = new Label { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
                            lbl.Text = item.Key.ToString();
                            g.Children.Add(lbl, 0, i++);
                        }
                        g2 = new Grid
                        {
                            ColumnDefinitions =
                            {
                            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },//This round
                            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) }//Historical
                            }
                        };
                        g2.RowDefinitions.Add(new RowDefinition());//row labels
                        lbl = new Label { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
                        lbl.Text = "This Round";
                        g2.Children.Add(lbl, 0, 0);
                        lbl = new Label { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
                        lbl.Text = "Historical";
                        g2.Children.Add(lbl, 1, 0);
                        i = 1;
                        foreach (var item in viewModel.LSscores.OrderBy(x => x.Key))
                        {
                            g2.RowDefinitions.Add(new RowDefinition());//row content
                            lbl = new Label { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
                            lbl.Text = item.Value[0].ToString() + " / " + item.Value[1].ToString();
                            lbl.Text += " (" + ((double)item.Value[0] / item.Value[1] * 100).ToString("N0", CultureInfo.InvariantCulture) + "%)";
                            g2.Children.Add(lbl, 0, i);

                            lbl = new Label { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
                            if (viewModel.ls_AvgCorPctBySpanLen_f != null && viewModel.ls_AvgCorPctBySpanLen_f.ContainsKey((int)item.Key))
                            {
                                lbl.Text = (viewModel.ls_AvgCorPctBySpanLen_f[(int)item.Key] * 100).ToString("N0", CultureInfo.InvariantCulture) + "%";
                                if ((double)item.Value[0] / item.Value[1] > viewModel.ls_AvgCorPctBySpanLen_f[(int)item.Key])
                                {
                                    ((Label)g2.Children[g2.Children.Count() - 1]).BackgroundColor = Color.ForestGreen;
                                }
                                else if ((double)item.Value[0] / item.Value[1] < viewModel.ls_AvgCorPctBySpanLen_f[(int)item.Key])
                                {
                                    ((Label)g2.Children[g2.Children.Count() - 1]).BackgroundColor = Color.OrangeRed;
                                }
                                viewModel.lspoints += (double)item.Value[0] - (item.Value[1] * viewModel.ls_AvgCorPctBySpanLen_f[(int)item.Key]);
                            }
                            else
                            {
                                lbl.Text = "N/A";
                            }
                            g2.Children.Add(lbl, 1, i);
                            i++;
                        }
                        g.Children.Add(g2, 1, 0);
                        Grid.SetRowSpan(g2, i);
                        sl.Children.Add(g);
                        points = viewModel.lspoints;
                        break;
                }
                f.Content = sl;
                if (points > 0) f.BackgroundColor = Color.LightGreen;
                //f.BackgroundColor = Color.LightYellow;
                if (points < 0) f.BackgroundColor = Color.IndianRed;
                this.TwoMinuteDrillSummaryView.Children.Add(f);
            }
        }
    }
}
