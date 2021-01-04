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
    public partial class StroopView : Grid
    {
        public TMDViewModel viewModel
        {
            get { return BindingContext as TMDViewModel; }
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
        float wordsize = 90;
        double ontime = 0;

        SkiaTextFigure fixation, redword, greenword, blueword, yellowword, displayword;

        public StroopView(TMDViewModel _viewModel)
        {
            InitializeComponent();
            viewModel = _viewModel;
            ts = TimeSpan.FromMilliseconds(1000.0 / _fpsWanted);
        }

        protected override void OnSizeAllocated(double w, double h)
        {
            base.OnSizeAllocated(w, h);
            if (w != -1 && h != -1) Init();
        }

        private SkiaTextFigure MakeWord(string w, float fsize, SKColor clr)
        {
            float tl_x, tl_y, br_x, br_y;

            SKPaint textPaint = new SKPaint
            {
                Typeface = SKTypeface.FromFamilyName("Verdana", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright),
                Style = SKPaintStyle.StrokeAndFill,
                StrokeWidth = 4,
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
            centerx = canvasView.CanvasSize.Width == 0 ? (float)canvasView.Width : canvasView.CanvasSize.Width / 2;
            centery = canvasView.CanvasSize.Height == 0 ? (float)canvasView.Height : canvasView.CanvasSize.Height / 2;
        }

        public void ReadyButton_Clicked(object sender, EventArgs e)
        {
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

                congruent = viewModel.Stroopcolorwords.IndexOf(viewModel.Stroopwords[viewModel.Stroopblocktrialctr]) == (int)viewModel.Strooptextcolors[viewModel.Stroopblocktrialctr];
                viewModel.Strooptrialctr++;
                viewModel.Stroopblocktrialctr++;
                if (congruent)
                {
                    viewModel.Stroopcontrialcnt++;
                }
                else
                {
                    viewModel.Stroopincontrialcnt++;
                }

                if (viewModel.cor)
                {
                    viewModel.Stroopcortrialcnt++;
                    if (congruent)
                    {
                        viewModel.Stroopcorcontrialcnt++;
                        viewModel.StroopConReactionTime += Math.Min(rt - ontime, viewModel.Strooptimeout);
                    }
                    else
                    {
                        viewModel.Stroopcorincontrialcnt++;
                        viewModel.StroopInconReactionTime += Math.Min(rt - ontime, viewModel.Strooptimeout);
                    }
                }
                clicked = true;

                viewModel.StroopReactButton(0, Math.Min(rt - ontime, viewModel.Strooptimeout), 0, 0, viewModel.Stroopwords[viewModel.Stroopblocktrialctr - 1], viewModel.Stroopcolorwords[(int)viewModel.Strooptextcolors[viewModel.Stroopblocktrialctr - 1]], congruent, viewModel.cor);
            }
        }


        private bool TimerLoop()
        {
            // get the elapsed time from the stopwatch because the 1/30 timer interval is not accurate and can be off by 2 ms
            var dt = _stopWatch.Elapsed.TotalMilliseconds;

            if (viewModel.Stroopblocktrialctr < viewModel.Strooptrialsperset && dt < viewModel.Stroopitims) //keep screen blank
            {
                showstim = false;
            }
            else if (viewModel.Stroopblocktrialctr < viewModel.Strooptrialsperset && dt < viewModel.Stroopitims + viewModel.Stroopfixationondurms) //keep orienting cue onscreen
            {
                showstim = true;
                displayword = MakeWord("+", fixsize, SKColors.Black);
            }
            else if (viewModel.Stroopblocktrialctr < viewModel.Strooptrialsperset && dt < viewModel.Stroopitims + viewModel.Stroopfixationondurms + viewModel.Stroopfixationoffdurms) //keep orienting cue off
            {
                showstim = false;
            }
            else if (viewModel.Stroopblocktrialctr < viewModel.Strooptrialsperset && dt < viewModel.Stroopitims + viewModel.Stroopfixationondurms + viewModel.Stroopfixationoffdurms + viewModel.Strooptimeout && !clicked)
            {
                showstim = true;
                displayword = MakeWord(viewModel.Stroopwords[viewModel.Stroopblocktrialctr], wordsize, viewModel.Stroopcolortypes[(int)viewModel.Strooptextcolors[viewModel.Stroopblocktrialctr]]);
            }
            else //clicked or timeout, done with trial
            {
                if (!clicked)//timeout
                {
                    ReactButton_Clicked(null, null);
                }
                //if (viewModel.blocktrialctr < viewModel.trialsperset && dt >= viewModel.pausedurarr[viewModel.blocktrialctr] + viewModel.timeout) 
                showstim = false;
                firstshown = false;
                _stopWatch.Restart();
                clicked = false;
                if (viewModel.Stroopblocktrialctr == viewModel.Strooptrialsperset) //done with block
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
