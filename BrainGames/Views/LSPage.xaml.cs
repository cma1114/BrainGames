using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Globalization;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using SkiaSharp;
using SkiaSharp.Views.Forms;

using BrainGames.Models;
using BrainGames.Controls;
using BrainGames.Views;
using BrainGames.Utility;
using BrainGames.ViewModels;

namespace BrainGames.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LSPage : ContentPage
    {
        public LSViewModel viewModel
        {
            get { return BindingContext as LSViewModel; }
            set { BindingContext = value; }
        }

        private readonly Stopwatch _stopWatch = new Stopwatch();
        private TimeSpan ts;
        private const double _fpsWanted = 60.0;
        float digitsize = 70;
        float TILE_SIZE;
        bool showstim = false;

        SkiaTextFigure displayword;

        public LSPage()
        {
            viewModel = new LSViewModel();
            InitializeComponent();
            ts = TimeSpan.FromMilliseconds(1000.0 / _fpsWanted);

            Grid g = new Grid()
            {
                Opacity = 0.5,
                HorizontalOptions = LayoutOptions.Center,
                Margin = 10,
                RowSpacing = 2,
                ColumnSpacing = 2
            };
            for (var i = 0; i < viewModel.gridsize; i++)
            {
                g.RowDefinitions.Add(new RowDefinition { Height = new GridLength(TILE_SIZE) });
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(TILE_SIZE) });
            }

            // Create the tiles
            for (var x = 0; x < viewModel.gridsize; x++)
            {
                for (var y = 0; y < viewModel.gridsize; y++)
                {
                    var tile = new Tile(x, y, TILE_SIZE, Color.Gray, "");
                    viewModel.AddTile(tile);
                    g.Children.Add(tile, x, y);
                }
            }
            this.Content.FindByName<Grid>("MasterGrid").Children.Insert(3, g);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Init();
        }

        private void Init()
        {

            if (viewModel.EstSpan > 0)
            {
                estspanLabel.Text = "Est Span: " + Math.Round(viewModel.EstSpan, 0).ToString("N0", CultureInfo.InvariantCulture) + " ms";
            }
        }

        async void Stats_Clicked(object sender, EventArgs e)
        {
            if (viewModel.trialctr == 0) return;
            await Navigation.PushModalAsync(new NavigationPage(new DSStatsPage()));
        }

        public void ReadyButton_Clicked(object sender, EventArgs e)
        {
            spanlenLabel.BackgroundColor = Color.Gray;
            _stopWatch.Restart();
            Device.StartTimer(ts, TimerLoop);
        }

        public void ReactButton_Clicked(object sender, EventArgs e)
        {
            if (viewModel.answered) spanlenLabel.BackgroundColor = viewModel.AnsClr;
        }

        private bool TimerLoop()
        {
            var dt = _stopWatch.Elapsed.TotalMilliseconds;

            if (viewModel.blocktrialctr < viewModel.spanlen && dt < viewModel.stimonms)
            {
                showstim = true;
//                displayword = MakeWord(viewModel.digitlist[viewModel.blocktrialctr], digitsize, SKColors.Black);
            }
            else if (viewModel.blocktrialctr < viewModel.spanlen && dt < viewModel.stimonms + viewModel.stimoffms)
            {
                showstim = false;
            }
            else if (viewModel.blocktrialctr < viewModel.spanlen && dt >= viewModel.stimonms + viewModel.stimoffms)
            {
                viewModel.blocktrialctr++;
                _stopWatch.Restart();
            }
            else if (viewModel.blocktrialctr == viewModel.spanlen && dt < viewModel.timeout && !viewModel.answered)//key buttons enabled
            {
                viewModel.EnableButtons = true;
                viewModel.timer.Start();
            }
            else //entered response or timeout, done with trial, return to ready screen
            {
                if (dt >= viewModel.timeout) viewModel.timedout = true;
                viewModel.IsRunning = false;
                viewModel.EnableButtons = false;
                viewModel.timer.Stop();
                displayword = null;
//                canvasView.InvalidateSurface();
                return false;
            }
//            canvasView.InvalidateSurface();

            return true;
        }


    }
}