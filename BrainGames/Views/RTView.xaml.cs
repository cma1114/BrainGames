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
    public partial class RTView : Grid
    {
        public TMDViewModel viewModel
        {
            get { return BindingContext as TMDViewModel; }
            set { BindingContext = value; }
        }

        private readonly Stopwatch _stopWatch = new Stopwatch();
        private TimeSpan ts;
        private const double _fpsWanted = 60.0;
        static float boxsize = 150;//200;
        static float crossmargin = 30;
        static float crossfont = 30;//40;
        float centerx = 0, centery = 0;
        bool showbox = false;
        bool showcross = false;
        bool clicked = false;
        bool firstshown = false;
        double ontime = 0;

        List<SkiaRectangleDrawingFigure> boxfigures;
        SkiaRectangleDrawingFigure boxfigure1, boxfigure2A, boxfigure2B, boxfigure4A, boxfigure4B, boxfigure4C, boxfigure4D;
        SkiaPathDrawingFigure crossfigure, crossfigure1, crossfigure2A, crossfigure2B, crossfigure4A, crossfigure4B, crossfigure4C, crossfigure4D;
        SKPaint skCrossPaint = new SKPaint()
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Black,
            StrokeWidth = crossfont,
            IsAntialias = true,
            StrokeCap = SKStrokeCap.Round
        };
        SKPaint skBoxPaint = new SKPaint()
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.White,
            StrokeWidth = 2
        };

        public RTView(TMDViewModel _viewModel)
        {
            InitializeComponent();
            viewModel = _viewModel;
            ts = TimeSpan.FromMilliseconds(1000.0 / _fpsWanted);
            boxfigures = new List<SkiaRectangleDrawingFigure>();
        }

        protected override void OnSizeAllocated(double w, double h)
        {
            base.OnSizeAllocated(w, h);
//            if (w != -1 && h != -1) Init();
        }

        private void Init()
        {
            centerx = canvasView.CanvasSize.Width == 0 ? (float)canvasView.Width : canvasView.CanvasSize.Width / 2;
            centery = canvasView.CanvasSize.Height == 0 ? (float)canvasView.Height : canvasView.CanvasSize.Height / 2;

            float tl_x, tl_y, bl_x, bl_y, tr_x, tr_y, br_x, br_y;

            tl_x = centerx - boxsize / 2;
            tl_y = centery - boxsize / 2;
            tr_x = centerx + boxsize / 2;
            tr_y = centery - boxsize / 2;
            bl_x = centerx - boxsize / 2;
            bl_y = centery + boxsize / 2;
            br_x = centerx + boxsize / 2;
            br_y = centery + boxsize / 2;

            boxfigure1 = new SkiaRectangleDrawingFigure();
            boxfigure1.FigurePaint = skBoxPaint;
            boxfigure1.StartPoint = new SKPoint(tl_x, tl_y);
            boxfigure1.EndPoint = new SKPoint(br_x, br_y);

            crossfigure1 = new SkiaPathDrawingFigure();
            crossfigure1.FigurePaint = skCrossPaint;

            SKPath path = new SKPath();
            path.MoveTo(bl_x + crossmargin, bl_y - crossmargin);
            crossfigure1.Path = path;
            crossfigure1.Path.LineTo(new SKPoint(tr_x - crossmargin, tr_y + crossmargin));
            crossfigure1.Path.MoveTo(br_x - crossmargin, br_y - crossmargin);
            crossfigure1.Path.LineTo(new SKPoint(tl_x + crossmargin, tl_y + crossmargin));


            float buffer = 10;
            boxfigure2A = new SkiaRectangleDrawingFigure();
            boxfigure2A.FigurePaint = skBoxPaint;
            boxfigure2A.StartPoint = new SKPoint(tl_x - boxsize / 2 - buffer, tl_y);
            boxfigure2A.EndPoint = new SKPoint(br_x - boxsize / 2 - buffer, br_y);

            boxfigure2B = new SkiaRectangleDrawingFigure();
            boxfigure2B.FigurePaint = skBoxPaint;
            boxfigure2B.StartPoint = new SKPoint(tl_x + boxsize / 2 + buffer, tl_y);
            boxfigure2B.EndPoint = new SKPoint(br_x + boxsize / 2 + buffer, br_y);

            crossfigure2A = new SkiaPathDrawingFigure();
            crossfigure2A.FigurePaint = skCrossPaint;

            SKPath path2A = new SKPath();
            path2A.MoveTo(boxfigure2A.Rectangle.Left + crossmargin, bl_y - crossmargin);
            crossfigure2A.Path = path2A;
            crossfigure2A.Path.LineTo(new SKPoint(boxfigure2A.Rectangle.Right - crossmargin, tr_y + crossmargin));
            crossfigure2A.Path.MoveTo(boxfigure2A.Rectangle.Right - crossmargin, br_y - crossmargin);
            crossfigure2A.Path.LineTo(new SKPoint(boxfigure2A.Rectangle.Left + crossmargin, tl_y + crossmargin));

            crossfigure2B = new SkiaPathDrawingFigure();
            crossfigure2B.FigurePaint = skCrossPaint;

            SKPath path2B = new SKPath();
            path2B.MoveTo(boxfigure2B.Rectangle.Left + crossmargin, bl_y - crossmargin);
            crossfigure2B.Path = path2B;
            crossfigure2B.Path.LineTo(new SKPoint(boxfigure2B.Rectangle.Right - crossmargin, tr_y + crossmargin));
            crossfigure2B.Path.MoveTo(boxfigure2B.Rectangle.Right - crossmargin, br_y - crossmargin);
            crossfigure2B.Path.LineTo(new SKPoint(boxfigure2B.Rectangle.Left + crossmargin, tl_y + crossmargin));

            boxfigure4A = new SkiaRectangleDrawingFigure();
            boxfigure4A.FigurePaint = skBoxPaint;
            boxfigure4A.StartPoint = new SKPoint(tl_x - boxsize * (float)1.5 - buffer, tl_y);
            boxfigure4A.EndPoint = new SKPoint(br_x - boxsize * (float)1.5 - buffer, br_y);

            boxfigure4B = new SkiaRectangleDrawingFigure();
            boxfigure4B.FigurePaint = skBoxPaint;
            boxfigure4B.StartPoint = new SKPoint(boxfigure4A.Rectangle.Right + buffer, tl_y);
            boxfigure4B.EndPoint = new SKPoint(boxfigure4A.Rectangle.Right + buffer + boxsize, br_y);

            boxfigure4C = new SkiaRectangleDrawingFigure();
            boxfigure4C.FigurePaint = skBoxPaint;
            boxfigure4C.StartPoint = new SKPoint(boxfigure4B.Rectangle.Right + buffer, tl_y);
            boxfigure4C.EndPoint = new SKPoint(boxfigure4B.Rectangle.Right + buffer + boxsize, br_y);

            boxfigure4D = new SkiaRectangleDrawingFigure();
            boxfigure4D.FigurePaint = skBoxPaint;
            boxfigure4D.StartPoint = new SKPoint(boxfigure4C.Rectangle.Right + buffer, tl_y);
            boxfigure4D.EndPoint = new SKPoint(boxfigure4C.Rectangle.Right + buffer + boxsize, br_y);

            crossfigure4A = new SkiaPathDrawingFigure();
            crossfigure4A.FigurePaint = skCrossPaint;

            SKPath path4A = new SKPath();
            path4A.MoveTo(boxfigure4A.Rectangle.Left + crossmargin, boxfigure4A.Rectangle.Bottom - crossmargin);
            crossfigure4A.Path = path4A;
            crossfigure4A.Path.LineTo(new SKPoint(boxfigure4A.Rectangle.Right - crossmargin, boxfigure4A.Rectangle.Top + crossmargin));
            crossfigure4A.Path.MoveTo(boxfigure4A.Rectangle.Right - crossmargin, boxfigure4A.Rectangle.Bottom - crossmargin);
            crossfigure4A.Path.LineTo(new SKPoint(boxfigure4A.Rectangle.Left + crossmargin, boxfigure4A.Rectangle.Top + crossmargin));

            crossfigure4B = new SkiaPathDrawingFigure();
            crossfigure4B.FigurePaint = skCrossPaint;

            SKPath path4B = new SKPath();
            path4B.MoveTo(boxfigure4B.Rectangle.Left + crossmargin, boxfigure4B.Rectangle.Bottom - crossmargin);
            crossfigure4B.Path = path4B;
            crossfigure4B.Path.LineTo(new SKPoint(boxfigure4B.Rectangle.Right - crossmargin, boxfigure4B.Rectangle.Top + crossmargin));
            crossfigure4B.Path.MoveTo(boxfigure4B.Rectangle.Right - crossmargin, boxfigure4B.Rectangle.Bottom - crossmargin);
            crossfigure4B.Path.LineTo(new SKPoint(boxfigure4B.Rectangle.Left + crossmargin, boxfigure4B.Rectangle.Top + crossmargin));

            crossfigure4C = new SkiaPathDrawingFigure();
            crossfigure4C.FigurePaint = skCrossPaint;

            SKPath path4C = new SKPath();
            path4C.MoveTo(boxfigure4C.Rectangle.Left + crossmargin, boxfigure4C.Rectangle.Bottom - crossmargin);
            crossfigure4C.Path = path4C;
            crossfigure4C.Path.LineTo(new SKPoint(boxfigure4C.Rectangle.Right - crossmargin, boxfigure4C.Rectangle.Top + crossmargin));
            crossfigure4C.Path.MoveTo(boxfigure4C.Rectangle.Right - crossmargin, boxfigure4C.Rectangle.Bottom - crossmargin);
            crossfigure4C.Path.LineTo(new SKPoint(boxfigure4C.Rectangle.Left + crossmargin, boxfigure4C.Rectangle.Top + crossmargin));

            crossfigure4D = new SkiaPathDrawingFigure();
            crossfigure4D.FigurePaint = skCrossPaint;

            SKPath path4D = new SKPath();
            path4D.MoveTo(boxfigure4D.Rectangle.Left + crossmargin, boxfigure4D.Rectangle.Bottom - crossmargin);
            crossfigure4D.Path = path4D;
            crossfigure4D.Path.LineTo(new SKPoint(boxfigure4D.Rectangle.Right - crossmargin, boxfigure4D.Rectangle.Top + crossmargin));
            crossfigure4D.Path.MoveTo(boxfigure4D.Rectangle.Right - crossmargin, boxfigure4D.Rectangle.Bottom - crossmargin);
            crossfigure4D.Path.LineTo(new SKPoint(boxfigure4D.Rectangle.Left + crossmargin, boxfigure4D.Rectangle.Top + crossmargin));
        }

        public void ReadyButton_Clicked(object sender, EventArgs e)
        {
            boxfigures.Clear();
            Init();
            if (viewModel.RTboxes == 1)
            {
                crossfigure = crossfigure1;
                boxfigures.Add(boxfigure1);
            }
            else if (viewModel.RTboxes == 2)
            {
                boxfigures.Add(boxfigure2A);
                boxfigures.Add(boxfigure2B);
                if (viewModel.RTcorboxes[0] == 0) crossfigure = crossfigure2A;
                else crossfigure = crossfigure2B;
            }
            else if (viewModel.RTboxes == 4)
            {
                boxfigures.Add(boxfigure4A);
                boxfigures.Add(boxfigure4B);
                boxfigures.Add(boxfigure4C);
                boxfigures.Add(boxfigure4D);
                if (viewModel.RTcorboxes[viewModel.RTblocktrialctr] == 0) { crossfigure = crossfigure4A; }
                else if (viewModel.RTcorboxes[viewModel.RTblocktrialctr] == 1) { crossfigure = crossfigure4B; }
                else if (viewModel.RTcorboxes[viewModel.RTblocktrialctr] == 2) { crossfigure = crossfigure4C; }
                else { crossfigure = crossfigure4D; }
            }

            clicked = false;
            firstshown = false;
            showbox = true;
            _stopWatch.Restart();
            Device.StartTimer(ts, TimerLoop);
        }

        public void ReactButton_Clicked(object sender, EventArgs e)
        {
            //            Console.WriteLine("blocktrialctr = {0}", viewModel.blocktrialctr);
            if (showcross) //ignore it if it's not during a trial
            {
                double rt = _stopWatch.Elapsed.TotalMilliseconds;
                viewModel.RTReactionTime += Math.Min(rt - ontime, viewModel.RTtimeout);
                viewModel.RTtrialctr++;
                viewModel.RTblocktrialctr++;
                clicked = true;

                if (viewModel.RTblocktrialctr < viewModel.RTtrialsperset) //load up next stim
                {
                    if (viewModel.RTboxes == 1)
                    {
                        crossfigure = crossfigure1;
                    }
                    else if (viewModel.RTboxes == 2)
                    {
                        if (viewModel.RTcorboxes[viewModel.RTblocktrialctr] == 0) crossfigure = crossfigure2A;
                        else crossfigure = crossfigure2B;
                    }
                    else if (viewModel.RTboxes == 4)
                    {
                        if (viewModel.RTcorboxes[viewModel.RTblocktrialctr] == 0) { crossfigure = crossfigure4A; }
                        else if (viewModel.RTcorboxes[viewModel.RTblocktrialctr] == 1) { crossfigure = crossfigure4B; }
                        else if (viewModel.RTcorboxes[viewModel.RTblocktrialctr] == 2) { crossfigure = crossfigure4C; }
                        else { crossfigure = crossfigure4D; }
                    }
                }
                if (viewModel.RTboxes == 1)
                {
                    viewModel.RTss1_trialcnt++;
                    viewModel.RTss1_cumrt += viewModel.RTReactionTime;
                }
                viewModel.RTReactButton(0, viewModel.RTReactionTime, (float)viewModel.RTss1_cumrt / viewModel.RTss1_trialcnt, viewModel.RTcorboxes[viewModel.RTblocktrialctr - 1], viewModel.cor);
            }
        }

        private bool TimerLoop()
        {
            // get the elapsed time from the stopwatch because the 1/30 timer interval is not accurate and can be off by 2 ms
            var dt = _stopWatch.Elapsed.TotalMilliseconds;

            if (viewModel.RTblocktrialctr < viewModel.RTtrialsperset && dt < viewModel.RTpausedurarr[viewModel.RTblocktrialctr]) //keep orienting cue onscreen
            {
                showcross = false;
            }
            else if (viewModel.RTblocktrialctr < viewModel.RTtrialsperset && dt < viewModel.RTpausedurarr[viewModel.RTblocktrialctr] + viewModel.RTtimeout && !clicked)
            {
                showcross = true;
            }
            else //clicked or timeout, done with trial
            {
                showcross = false;
                firstshown = false;
                _stopWatch.Restart();
                clicked = false;
                if (viewModel.RTblocktrialctr == viewModel.RTtrialsperset) //done with block
                {
                    showbox = false;
                    canvasView.InvalidateSurface();
                    viewModel.IsRunning = false;
                    viewModel.RTShowReact1 = false;
                    viewModel.RTShowReact2 = false;
                    viewModel.RTShowReact4 = false;
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

            canvas.Clear(SKColors.Blue);
            if (!showbox) return;
            foreach (SkiaRectangleDrawingFigure boxfigure in boxfigures)
            {
                canvas.DrawRect(boxfigure.Rectangle, boxfigure.FigurePaint);
            }
            if (showcross)
            {
                canvas.DrawPath((crossfigure).Path, crossfigure.FigurePaint);
                if (!firstshown)
                {
                    ontime = _stopWatch.Elapsed.TotalMilliseconds;
                    firstshown = true;
                }
            }
        }
    }
}
