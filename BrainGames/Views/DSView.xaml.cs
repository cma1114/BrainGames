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
    public partial class DSView : Grid
    {
        public TMDViewModel viewModel
        {
            get { return BindingContext as TMDViewModel; }
            set { BindingContext = value; }
        }

        private readonly Stopwatch _stopWatch = new Stopwatch();
        private TimeSpan ts;
        private const double _fpsWanted = 60.0;
        float digitsize = 70;
        float centerx, centery;
        bool showstim = false;

        SkiaTextFigure displayword;

        public DSView(TMDViewModel _viewModel)
        {
            InitializeComponent();
            viewModel = _viewModel;
            ts = TimeSpan.FromMilliseconds(1000.0 / _fpsWanted);
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
            centerx = canvasView.CanvasSize.Width == 0 ? (float)canvasView.Width : canvasView.CanvasSize.Width / 2;
            centery = canvasView.CanvasSize.Height == 0 ? (float)canvasView.Height : canvasView.CanvasSize.Height / 2;
        }

        public void ReadyButton_Clicked(object sender, EventArgs e)
        {
            Init();
            _stopWatch.Restart();
            Device.StartTimer(ts, TimerLoop);
        }

        public void ReactButton_Clicked(object sender, EventArgs e)
        {
//            if (viewModel.answered) spanlenLabel.BackgroundColor = viewModel.AnsClr;
        }

        private bool TimerLoop()
        {
            var dt = _stopWatch.Elapsed.TotalMilliseconds;

            if (viewModel.DSblocktrialctr < viewModel.DSspanlen && dt < viewModel.DSstimonms)
            {
                showstim = true;
                displayword = MakeWord(viewModel.DSdigitlist[viewModel.DSblocktrialctr], digitsize, SKColors.Black);
            }
            else if (viewModel.DSblocktrialctr < viewModel.DSspanlen && dt < viewModel.DSstimonms + viewModel.DSstimoffms)
            {
                showstim = false;
            }
            else if (viewModel.DSblocktrialctr < viewModel.DSspanlen && dt >= viewModel.DSstimonms + viewModel.DSstimoffms)
            {
                viewModel.DSblocktrialctr++;
                _stopWatch.Restart();
            }
            else if (viewModel.DSblocktrialctr == viewModel.DSspanlen && dt < viewModel.DStimeout && !viewModel.DSanswered)//key buttons enabled
            {
                viewModel.DSEnableButtons = true;
                viewModel.DStimer.Start();
            }
            else //entered response or timeout, done with trial, return to ready screen
            {
                if (dt >= viewModel.DStimeout) viewModel.DStimedout = true;
                viewModel.IsRunning = false;
                viewModel.DSEnableButtons = false;
                viewModel.DStimer.Stop();
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
