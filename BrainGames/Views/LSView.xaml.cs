using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Xamarin.Forms;
using SkiaSharp;
using SkiaSharp.Views.Forms;

using BrainGames.Controls;
using BrainGames.ViewModels;

namespace BrainGames.Views
{
    public partial class LSView : Grid
    {
        public TMDViewModel viewModel
        {
            get { return BindingContext as TMDViewModel; }
            set { BindingContext = value; }
        }

        private readonly Stopwatch _stopWatch = new Stopwatch();
        private TimeSpan ts;
        private const double _fpsWanted = 60.0;
        float TILE_SIZE = 0;
        float spacing = 10;
        bool showstim = false;

        public LSView(TMDViewModel _viewModel)
        {
            viewModel = _viewModel;
            InitializeComponent();
            ts = TimeSpan.FromMilliseconds(1000.0 / _fpsWanted);
        }

        protected override void OnSizeAllocated(double w, double h)
        {
            base.OnSizeAllocated(w, h);
            if (w != -1 && h != -1 && TILE_SIZE == 0) Init();
        }

        private void Init()
        {
            Grid g = this.FindByName<Grid>("BoardGrid");
            Grid bg = (Grid)(g.Parent.Parent.Parent);
            Grid pg = (Grid)g.Parent;
            double gridheight = bg.Height - bg.Children[0].Height - pg.Children[1].Height;
            g.RowSpacing = spacing;
            g.ColumnSpacing = spacing;
            TILE_SIZE = (float)Math.Floor((Math.Min(this.Width, gridheight) - ((viewModel.LSgridsize + 1) * spacing/*row/col spacing*/)) / viewModel.LSgridsize);
            for (var i = 0; i < viewModel.LSgridsize; i++)
            {
                g.RowDefinitions.Add(new RowDefinition { Height = new GridLength(TILE_SIZE) });
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(TILE_SIZE) });
            }

            // Create the tiles
            for (var x = 0; x < viewModel.LSgridsize; x++)
            {
                for (var y = 0; y < viewModel.LSgridsize; y++)
                {
                    var tile = new Tile(x, y, TILE_SIZE, Color.Yellow, Color.Gray, "");
                    viewModel.AddTile(tile);
                    g.Children.Add(tile, x, y);
                }
            }
        }

        public void ReadyButton_Clicked(object sender, EventArgs e)
        {
            Grid g = this.FindByName<Grid>("BoardGrid");
            Grid bg = (Grid)(g.Parent.Parent.Parent);
            Grid pg = (Grid)g.Parent;
            double gridheight = bg.Height - bg.Children[0].Height - pg.Children[1].Height;
            g.ColumnDefinitions.Clear();
            g.RowDefinitions.Clear();
            g.Children.Clear();
            g.RowSpacing = spacing;
            g.ColumnSpacing = spacing;
            TILE_SIZE = (float)Math.Floor((Math.Min(this.Width, gridheight) - ((viewModel.LSgridsize + 1) * spacing/*row/col spacing*/)) / viewModel.LSgridsize);
            for (var i = 0; i < viewModel.LSgridsize; i++)
            {
                g.RowDefinitions.Add(new RowDefinition { Height = new GridLength(TILE_SIZE) });
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(TILE_SIZE) });
            }

            // Create the tiles
            for (var x = 0; x < viewModel.LSgridsize; x++)
            {
                for (var y = 0; y < viewModel.LSgridsize; y++)
                {
                    var tile = new Tile(x, y, TILE_SIZE, Color.Yellow, Color.Gray, "");
                    viewModel.AddTile(tile);
                    g.Children.Add(tile, x, y);
                }
            }

            _stopWatch.Restart();
            Device.StartTimer(ts, TimerLoop);
        }

        private bool TimerLoop()
        {
            var dt = _stopWatch.Elapsed.TotalMilliseconds;

            if (viewModel.LSblocktrialctr < viewModel.LSspanlen && dt < viewModel.LSstimonms)
            {
                if (!showstim) viewModel.FlipTile(viewModel.LSdigitlist[viewModel.LSblocktrialctr]);
                showstim = true;
            }
            else if (viewModel.LSblocktrialctr < viewModel.LSspanlen && dt < viewModel.LSstimonms + viewModel.LSstimoffms)
            {
                if (showstim) viewModel.FlipTile(viewModel.LSdigitlist[viewModel.LSblocktrialctr]);
                showstim = false;
            }
            else if (viewModel.LSblocktrialctr < viewModel.LSspanlen && dt >= viewModel.LSstimonms + viewModel.LSstimoffms)
            {
                viewModel.LSblocktrialctr++;
                _stopWatch.Restart();
            }
            else if (viewModel.LSblocktrialctr == viewModel.LSspanlen && dt < viewModel.LStimeout && !viewModel.LSanswered)//key buttons enabled
            {
                viewModel.LSEnableButtons = true;
                ReadyButton.Text = "Go!";
                viewModel.LStimer.Start();
            }
            else //entered response or timeout, done with trial, return to ready screen
            {
                if (dt >= viewModel.LStimeout) viewModel.LStimedout = true;
                viewModel.IsRunning = false;
                viewModel.LSEnableButtons = false;
                viewModel.LStimer.Stop();
                ReadyButton.Text = "Ready";
                return false;
            }

            return true;
        }
    }
}
