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
    public partial class DSPage : ContentPage
    {
        public DSViewModel viewModel
        {
            get { return BindingContext as DSViewModel; }
            set { BindingContext = value; }
        }

        private readonly Stopwatch _stopWatch = new Stopwatch();
        private TimeSpan ts;
        private const double _fpsWanted = 60.0;
        float digitsize = 70;
        float centerx, centery;
        bool showstim = false;

        SkiaTextFigure displayword;

        public DSPage()
        {
            viewModel = new DSViewModel();
            InitializeComponent();
            ts = TimeSpan.FromMilliseconds(1000.0 / _fpsWanted);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Init();
        }

        private SkiaTextFigure MakeWord(string w, float fsize, SKColor clr)
        {
            float tl_x, tl_y, br_x, br_y;

            SKPaint textPaint = new SKPaint
            {
                Typeface = SKTypeface.FromFamilyName("Verdana", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright),
                Style = SKPaintStyle.StrokeAndFill,
                StrokeWidth = 2,
                FakeBoldText = false,
                Color = clr,
                TextSize = fsize
            };

            SkiaTextFigure tf = new SkiaTextFigure(textPaint, w);

            tl_x = centerx - tf.textBounds.Width / 2;
            tl_y = centery - tf.textBounds.Height / 2;
            br_x = centerx + tf.textBounds.Width / 2;
            br_y = centery + tf.textBounds.Height / 2;

            SKRect r = new SKRect();
            textPaint.MeasureText(w, ref r);
            tf.RenderedRectangle = r;
            SKRect rect = new SKRect(tl_x, tl_y, br_x, br_y).Standardized;
            tf.Rectangle = rect;

            return tf;
        }

        private void Init()
        {
            centerx = canvasView.CanvasSize.Width / 2;
            centery = canvasView.CanvasSize.Height / 2;

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
            if(viewModel.answered) spanlenLabel.BackgroundColor = viewModel.AnsClr;
        }

        private bool TimerLoop()
        {
            var dt = _stopWatch.Elapsed.TotalMilliseconds;

            if (viewModel.blocktrialctr < viewModel.spanlen && dt < viewModel.stimonms)
            {
                showstim = true;
                displayword = MakeWord(viewModel.digitlist[viewModel.blocktrialctr], digitsize, SKColors.Black);
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
            }
            else //entered response or timeout, done with trial, return to ready screen
            {
                if (dt >= viewModel.timeout) viewModel.timedout = true;
                viewModel.IsRunning = false;
                viewModel.EnableButtons = false;
                displayword = null;
                canvasView.InvalidateSurface();
                return false;
            }
            canvasView.InvalidateSurface();

            return true;
        }

        private void OnPainting(object sender, SKPaintSurfaceEventArgs e)
        {
            var surface = e.Surface;
            var canvas = surface.Canvas;

            canvas.Clear(SKColors.Gray);
            if (displayword is null) return;
            if (showstim)
            {
                canvas.DrawText((displayword).Text, displayword.Rectangle.Left - (displayword).RenderedRectangle.Left, displayword.Rectangle.Top - (displayword).RenderedRectangle.Top, displayword.FigurePaint);
            }
        }

    }
}