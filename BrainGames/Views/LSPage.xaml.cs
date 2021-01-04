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
        float TILE_SIZE;
        float spacing = 10;
        bool showstim = false;

        public LSPage()
        {
            viewModel = new LSViewModel();
            InitializeComponent();
            ts = TimeSpan.FromMilliseconds(1000.0 / _fpsWanted);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            Grid g = this.Content.FindByName<Grid>("BoardGrid");
            Grid bg = this.Content.FindByName<Grid>("MasterGrid");
            double gridheight = bg.Height - bg.Children[0].Height - bg.Children[1].Height - bg.Children[3].Height;
            g.RowSpacing = spacing;
            g.ColumnSpacing = spacing;
            TILE_SIZE = (float)Math.Floor((Math.Min(this.Content.Width, gridheight) - ((viewModel.gridsize + 1) * spacing/*row/col spacing*/)) / viewModel.gridsize);
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
                    var tile = new Tile(x, y, TILE_SIZE, Color.Yellow, Color.Gray, "");
                    viewModel.AddTile(tile);
                    g.Children.Add(tile, x, y);
                }
            }

            Init();
        }

        protected override void OnDisappearing()
        {
            viewModel.OnDisappearing();
            base.OnDisappearing();
        }

        private void Init()
        {
            /*
            if (viewModel.EstSpan > 0)
            {
                estspanLabel.Text = "Est Span: " + Math.Round(viewModel.EstSpan, 1).ToString("N0", CultureInfo.InvariantCulture) + " items";
            }*/
        }

        async void Stats_Clicked(object sender, EventArgs e)
        {
            if (viewModel.trialctr == 0) return;
            await Navigation.PushModalAsync(new NavigationPage(new LSStatsPage()));
        }

        public void ReadyButton_Clicked(object sender, EventArgs e)
        {
            Grid g = this.Content.FindByName<Grid>("BoardGrid");
            Grid bg = this.Content.FindByName<Grid>("MasterGrid");
            double gridheight = bg.Height - bg.Children[0].Height - bg.Children[1].Height - bg.Children[3].Height;
            g.ColumnDefinitions.Clear();
            g.RowDefinitions.Clear();
            g.Children.Clear();
            g.RowSpacing = spacing;
            g.ColumnSpacing = spacing;
            TILE_SIZE = (float)Math.Floor((Math.Min(this.Content.Width, gridheight) - ((viewModel.gridsize + 1) * spacing/*row/col spacing*/)) / viewModel.gridsize);
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
                    var tile = new Tile(x, y, TILE_SIZE, Color.Yellow, Color.Gray, "");
                    viewModel.AddTile(tile);
                    g.Children.Add(tile, x, y);
                }
            }

            spanlenLabel.BackgroundColor = Color.Gray;
            _stopWatch.Restart();
            Device.StartTimer(ts, TimerLoop);
        }

        private bool TimerLoop()
        {
            var dt = _stopWatch.Elapsed.TotalMilliseconds;

            if (viewModel.blocktrialctr < viewModel.spanlen && dt < viewModel.stimonms)
            {
                if (!showstim) viewModel.FlipTile(viewModel.digitlist[viewModel.blocktrialctr]);
                showstim = true;
            }
            else if (viewModel.blocktrialctr < viewModel.spanlen && dt < viewModel.stimonms + viewModel.stimoffms)
            {
                if (showstim) viewModel.FlipTile(viewModel.digitlist[viewModel.blocktrialctr]);
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
                ReadyButton.Text = "Go!";
                viewModel.timer.Start();
            }
            else //entered response or timeout, done with trial, return to ready screen
            {
                if (dt >= viewModel.timeout)
                {
                    viewModel.timedout = true;
                    viewModel.ResponseButton();
                }
                viewModel.IsRunning = false;
                viewModel.EnableButtons = false;
                viewModel.timer.Stop();
                ReadyButton.Text = "Ready";
                if (viewModel.answered) spanlenLabel.BackgroundColor = viewModel.AnsClr;
                return false;
            }

            return true;
        }


    }
}