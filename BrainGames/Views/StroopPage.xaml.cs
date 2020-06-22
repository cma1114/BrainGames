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
    public partial class StroopPage : ContentPage
    {
        public StroopViewModel viewModel
        {
            get { return BindingContext as StroopViewModel; }
            set { BindingContext = value; }
        }

        private readonly Stopwatch _stopWatch = new Stopwatch();
        private TimeSpan ts;
        private const double _fpsWanted = 60.0;
        float centerx, centery;
        bool showstim = false;
        bool clicked = false;
        bool firstshown = false;
        bool congruent = false;
        float fixsize = 40;
        float wordsize = 80;
        double ontime = 0;

        SkiaTextFigure fixation, redword, greenword, blueword, yellowword, displayword;

        public StroopPage()
        {
            viewModel = new StroopViewModel();
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
                StrokeWidth = 1,
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

            if (viewModel.AvgRT > 0)
            {
                artLabel.Text = "Avg RT: " + Math.Round(viewModel.AvgRT, 0).ToString("N0", CultureInfo.InvariantCulture) + " ms";
            }

            if (viewModel.DifRT != 0)
            {
                difLabel.Text = "I-C Dif: " + Math.Round(viewModel.DifRT, 0).ToString("N0", CultureInfo.InvariantCulture) + " ms";
            }
        }

        async void Stats_Clicked(object sender, EventArgs e)
        {
            if (viewModel.trialctr == 0) return;
            await Navigation.PushModalAsync(new NavigationPage(new StroopStatsPage()));
        }

        public void ReadyButton_Clicked(object sender, EventArgs e)
        {
            rtLabel.BackgroundColor = Color.Gray;
            clicked = false;
            firstshown = false;
            _stopWatch.Restart();
            Device.StartTimer(ts, TimerLoop);
        }

        public void ReactButton_Clicked(object sender, EventArgs e)
        {
            if (showstim) //ignore it if it's not during a trial
            {
                double rt = _stopWatch.Elapsed.TotalMilliseconds;
                viewModel.ReactionTime = Math.Min(rt - ontime, viewModel.timeout);

                congruent = viewModel.colorwords.IndexOf(viewModel.words[viewModel.blocktrialctr]) == (int)viewModel.textcolors[viewModel.blocktrialctr];
                viewModel.trialctr++;
                viewModel.blocktrialctr++;

                if (viewModel.cor)
                {
                    rtLabel.BackgroundColor = Color.ForestGreen;
                    viewModel.cumcorrt += viewModel.ReactionTime;
                    viewModel.cortrialcnt++;
                    if (congruent)
                    {
                        viewModel.cumconcorrt += viewModel.ReactionTime;
                        viewModel.corcontrialcnt++;
                    }
                    else
                    {
                        viewModel.cuminconcorrt += viewModel.ReactionTime;
                        viewModel.corincontrialcnt++;
                    }
                }
                else
                { rtLabel.BackgroundColor = Color.OrangeRed; }
                if (viewModel.cortrialcnt > 0) viewModel.AvgRT = viewModel.cumcorrt / viewModel.cortrialcnt;
                if (viewModel.corcontrialcnt > 0 && viewModel.corincontrialcnt > 0) viewModel.DifRT = viewModel.cuminconcorrt / viewModel.corincontrialcnt - viewModel.cumconcorrt / viewModel.corcontrialcnt;

                viewModel.trialctr++;
                rtLabel.Text = "RT: " + viewModel.ReactionTime.ToString("N0", CultureInfo.InvariantCulture) + " ms";
                artLabel.Text = "Avg RT: " + (viewModel.AvgRT > 0 ? Math.Round(viewModel.AvgRT, 0).ToString("N0", CultureInfo.InvariantCulture) + " ms" : "");
                difLabel.Text = "I-C Dif: " + (viewModel.DifRT != 0 ? Math.Round(viewModel.DifRT, 0).ToString("N0", CultureInfo.InvariantCulture) + " ms" : "");
                clicked = true;

                viewModel.ReactButton(viewModel.trialctr, viewModel.ReactionTime, viewModel.AvgRT, viewModel.DifRT, viewModel.words[viewModel.blocktrialctr - 1], viewModel.colorwords[(int)viewModel.textcolors[viewModel.blocktrialctr - 1]], congruent, viewModel.cor);
            }
        }


        private bool TimerLoop()
        {
            // get the elapsed time from the stopwatch because the 1/30 timer interval is not accurate and can be off by 2 ms
            var dt = _stopWatch.Elapsed.TotalMilliseconds;

            if (viewModel.blocktrialctr < viewModel.trialsperset && dt < viewModel.itims) //keep screen blank
            {
                showstim = false;
            }
            else if (viewModel.blocktrialctr < viewModel.trialsperset && dt < viewModel.itims + viewModel.fixationondurms) //keep orienting cue onscreen
            {
                showstim = true;
                displayword = MakeWord("+", fixsize, SKColors.Black);
            }
            else if (viewModel.blocktrialctr < viewModel.trialsperset && dt < viewModel.itims + viewModel.fixationondurms + viewModel.fixationoffdurms) //keep orienting cue off
            {
                showstim = false;
            }
            else if (viewModel.blocktrialctr < viewModel.trialsperset && dt < viewModel.itims + viewModel.fixationondurms + viewModel.fixationoffdurms + viewModel.timeout && !clicked)
            {
                showstim = true;
                displayword = MakeWord(viewModel.words[viewModel.blocktrialctr], wordsize, viewModel.colortypes[(int)viewModel.textcolors[viewModel.blocktrialctr]]);
            }
            else //clicked or timeout, done with trial
            {
                //if (viewModel.blocktrialctr < viewModel.trialsperset && dt >= viewModel.pausedurarr[viewModel.blocktrialctr] + viewModel.timeout) 
                showstim = false;
                firstshown = false;
                _stopWatch.Restart();
                clicked = false;
                if (viewModel.blocktrialctr == viewModel.trialsperset) //done with block
                {
                    displayword = null;
                    canvasView.InvalidateSurface();
                    viewModel.IsRunning = false;
                    return false;
                }
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
                if (!firstshown)
                {
                    ontime = _stopWatch.Elapsed.TotalMilliseconds;
                    firstshown = true;
                }
            }
        }

    }
}