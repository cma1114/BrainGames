using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Xamarin.Forms;
using SkiaSharp;
using SkiaSharp.Views.Forms;

using BrainGames.Controls;
using BrainGames.ViewModels;
using BrainGames.Utility;

namespace BrainGames.Views
{
    public partial class RTPage : ContentPage
    {
        public RTViewModel viewModel
        {
            get { return BindingContext as RTViewModel; }
            set { BindingContext = value; }
        }

        private readonly Stopwatch _stopWatch = new Stopwatch();
        private TimeSpan ts;
        private const double _fpsWanted = 60.0;
        static float boxsize = 150;//200;
        static float crossmargin = 30;
        static float crossfont = 30;//40;
        float centerx, centery;
        bool showbox = false;
        bool showcross = false;
        bool clicked = false;
        bool firstshown = false;
        double blockcumrt = 0;
        double ontime = 0;
        bool timedout = false;

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

        public RTPage()
        {
            viewModel = new RTViewModel();
            InitializeComponent();
            ts = TimeSpan.FromMilliseconds(1000.0 / _fpsWanted);
            boxfigures = new List<SkiaRectangleDrawingFigure>();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            //            Init();
            if (viewModel.boxopt == "1") ((RadioButton)FindByName("box1opt")).IsChecked = true;
            if (viewModel.boxopt == "2") ((RadioButton)FindByName("box2opt")).IsChecked = true;
            if (viewModel.boxopt == "4") ((RadioButton)FindByName("box4opt")).IsChecked = true;
            if (viewModel.boxopt == "auto") ((RadioButton)FindByName("autoopt")).IsChecked = true;

            if (viewModel.AvgRT > 0)
            {
                crtLabel.Text = "Avg Cor RT: " + viewModel.AvgRT.ToString("N1", CultureInfo.InvariantCulture) + " ms";
            }
        }

        protected override void OnSizeAllocated(double w, double h)
        {
            base.OnSizeAllocated(w, h);
            if (canvasView.CanvasSize.Width != 0 && canvasView.CanvasSize.Height != 0 && centerx == 0) Init();
        }

        protected override void OnDisappearing()
        {
            viewModel.OnDisappearing();
            base.OnDisappearing();
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

            /*
            boxfigure4A = new SkiaRectangleDrawingFigure();
            boxfigure4A.FigurePaint = skBoxPaint;
            boxfigure4A.StartPoint = new SKPoint(tl_x - boxsize / 2 - buffer, tl_y - boxsize / 2 - buffer);
            boxfigure4A.EndPoint = new SKPoint(br_x - boxsize / 2 - buffer, br_y - boxsize / 2 - buffer);

            boxfigure4B = new SkiaRectangleDrawingFigure();
            boxfigure4B.FigurePaint = skBoxPaint;
            boxfigure4B.StartPoint = new SKPoint(tl_x + boxsize / 2 + buffer, tl_y - boxsize / 2 - buffer);
            boxfigure4B.EndPoint = new SKPoint(br_x + boxsize / 2 + buffer, br_y - boxsize / 2 - buffer);

            boxfigure4C = new SkiaRectangleDrawingFigure();
            boxfigure4C.FigurePaint = skBoxPaint;
            boxfigure4C.StartPoint = new SKPoint(tl_x - boxsize / 2 - buffer, tl_y + boxsize / 2 + buffer);
            boxfigure4C.EndPoint = new SKPoint(br_x - boxsize / 2 - buffer, br_y + boxsize / 2 + buffer);

            boxfigure4D = new SkiaRectangleDrawingFigure();
            boxfigure4D.FigurePaint = skBoxPaint;
            boxfigure4D.StartPoint = new SKPoint(tl_x + boxsize / 2 + buffer, tl_y + boxsize / 2 + buffer);
            boxfigure4D.EndPoint = new SKPoint(br_x + boxsize / 2 + buffer, br_y + boxsize / 2 + buffer);
            */

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

        void BoxOptChanged(object sender, CheckedChangedEventArgs e)
        {
            if (((RadioButton)FindByName("box1opt")).IsChecked) viewModel.boxopt = "1";
            if (((RadioButton)FindByName("box2opt")).IsChecked) viewModel.boxopt = "2";
            if (((RadioButton)FindByName("box4opt")).IsChecked) viewModel.boxopt = "4";
            if (((RadioButton)FindByName("autoopt")).IsChecked) viewModel.boxopt = "auto";
        }

        async void Stats_Clicked(object sender, EventArgs e)
        {
            if (viewModel.trialctr == 0) return;
            App.AnalyticsService.TrackEvent("RTStatsView", new Dictionary<string, string> {
                    { "Type", "PageView" },
                    { "UserID", Settings.UserId.ToString()}
                });
            await Navigation.PushModalAsync(new NavigationPage(new RTStatsPage()));
        }

        public void ReadyButton_Clicked(object sender, EventArgs e)
        {
            rtLabel.BackgroundColor = Color.Gray;
            crtLabel.Text = "SS Cor RT: ";
            viewModel.AvgRT = 0;
            boxfigures.Clear();
            if (viewModel.boxes == 1)
            {
                viewModel.AvgRT = (float)viewModel.ss1_cumrt / viewModel.ss1_trialcnt;
                crossfigure = crossfigure1;
                boxfigures.Add(boxfigure1);
            }
            else if (viewModel.boxes == 2)
            {
                if (viewModel.ss2_trialcnt >= 10 && (float)viewModel.ss2_cortrialcnt / viewModel.ss2_trialcnt > 0.9) { viewModel.AvgRT = (float)viewModel.ss2_cumcorrt / viewModel.ss2_cortrialcnt; } else { viewModel.AvgRT = 0; }
                boxfigures.Add(boxfigure2A);
                boxfigures.Add(boxfigure2B);
                if (viewModel.corboxes[0] == 0) crossfigure = crossfigure2A;
                else crossfigure = crossfigure2B;
            }
            else if (viewModel.boxes == 4)
            {
                if (viewModel.ss4_trialcnt >= 10 && (float)viewModel.ss4_cortrialcnt / viewModel.ss4_trialcnt > 0.9) { viewModel.AvgRT = (float)viewModel.ss4_cumcorrt / viewModel.ss4_cortrialcnt; } else { viewModel.AvgRT = 0; }
                boxfigures.Add(boxfigure4A);
                boxfigures.Add(boxfigure4B);
                boxfigures.Add(boxfigure4C);
                boxfigures.Add(boxfigure4D);
                if (viewModel.corboxes[viewModel.blocktrialctr] == 0) { crossfigure = crossfigure4A; }
                else if (viewModel.corboxes[viewModel.blocktrialctr] == 1) { crossfigure = crossfigure4B; }
                else if (viewModel.corboxes[viewModel.blocktrialctr] == 2) { crossfigure = crossfigure4C; }
                else { crossfigure = crossfigure4D; }
            }

            timedout = false;
            clicked = false;
            firstshown = false;
            showbox = true;
            blockcumrt = 0;
            _stopWatch.Restart();
            Device.StartTimer(ts, TimerLoop);
        }

        public void ReactButton_Clicked(object sender, EventArgs e)
        {
//            Console.WriteLine("blocktrialctr = {0}", viewModel.blocktrialctr);
            if (showcross) //ignore it if it's not during a trial
            {
                double rt = _stopWatch.Elapsed.TotalMilliseconds;
                viewModel.ReactionTime = Math.Min(rt - ontime, viewModel.timeout);
                blockcumrt += viewModel.ReactionTime;
                viewModel.trialctr++;
                viewModel.blocktrialctr++;
                rtLabel.Text = "RT: " + viewModel.ReactionTime.ToString("N0", CultureInfo.InvariantCulture) + " ms";
                artLabel.Text = "Block RT: " + (blockcumrt / viewModel.blocktrialctr).ToString("N1", CultureInfo.InvariantCulture) + " ms";
                clicked = true;

                if (viewModel.boxes == 2)
                {
                    viewModel.ss2_trialcnt++;
                    if (viewModel.cor)
                    {
                        viewModel.ss2_cortrialcnt++;
                        viewModel.ss2_cumcorrt += viewModel.ReactionTime;
                        if (viewModel.ss2_trialcnt >= 10 && (float)viewModel.ss2_cortrialcnt / viewModel.ss2_trialcnt > 0.9) { viewModel.AvgRT = (float)viewModel.ss2_cumcorrt / viewModel.ss2_cortrialcnt; } else { viewModel.AvgRT = 0; }
                    }
                }
                else if (viewModel.boxes == 4)
                {
                    viewModel.ss4_trialcnt++;
                    if (viewModel.cor)
                    {
                        viewModel.ss4_cortrialcnt++;
                        viewModel.ss4_cumcorrt += viewModel.ReactionTime;
                        if (viewModel.ss4_trialcnt >= 10 && (float)viewModel.ss4_cortrialcnt / viewModel.ss4_trialcnt > 0.9) { viewModel.AvgRT = (float)viewModel.ss4_cumcorrt / viewModel.ss4_cortrialcnt; } else { viewModel.AvgRT = 0; }
                    }
                }

                crtLabel.Text = "SS Cor RT: " + (viewModel.AvgRT > 0 ? viewModel.AvgRT.ToString("N1", CultureInfo.InvariantCulture) + " ms" : "");

                if (viewModel.blocktrialctr < viewModel.trialsperset) //load up next stim
                { 
                    if (viewModel.boxes == 1)
                    {
                        crossfigure = crossfigure1;
                    }
                    else if (viewModel.boxes == 2)
                    {
                        if (viewModel.corboxes[viewModel.blocktrialctr] == 0) crossfigure = crossfigure2A;
                        else crossfigure = crossfigure2B;
                    }
                    else if (viewModel.boxes == 4)
                    {
                        if (viewModel.corboxes[viewModel.blocktrialctr] == 0) { crossfigure = crossfigure4A; }
                        else if (viewModel.corboxes[viewModel.blocktrialctr] == 1) { crossfigure = crossfigure4B; }
                        else if (viewModel.corboxes[viewModel.blocktrialctr] == 2) { crossfigure = crossfigure4C; }
                        else { crossfigure = crossfigure4D; }
                    }

                }
                if (viewModel.boxes > 1)
                {
                    if (viewModel.cor) { rtLabel.BackgroundColor = Color.ForestGreen; }
                    else { rtLabel.BackgroundColor = Color.OrangeRed; }
                }

                //                viewModel.ReactButtonCommand.Execute(null);
                if (timedout)
                {
                    viewModel.cor = false;
                }
                else if (viewModel.boxes == 1)
                {
                    viewModel.cor = true;
                    viewModel.ss1_trialcnt++;
                    viewModel.ss1_cumrt += viewModel.ReactionTime;
                    viewModel.AvgRT = viewModel.ss1_cumrt / viewModel.ss1_trialcnt;
                }
                viewModel.ReactButton(viewModel.trialctr, viewModel.ReactionTime, viewModel.AvgRT, viewModel.corboxes[viewModel.blocktrialctr -1], viewModel.cor);
            }
        }

        private bool TimerLoop()
        {
            // get the elapsed time from the stopwatch because the 1/30 timer interval is not accurate and can be off by 2 ms
            var dt = _stopWatch.Elapsed.TotalMilliseconds;

            if (viewModel.blocktrialctr < viewModel.trialsperset && dt < viewModel.pausedurarr[viewModel.blocktrialctr]) //keep orienting cue onscreen
            {
                showcross = false;
            }
            else if (viewModel.blocktrialctr < viewModel.trialsperset && dt < viewModel.pausedurarr[viewModel.blocktrialctr] + viewModel.timeout && !clicked)
            {
                showcross = true;
            }
            else //clicked or timeout, done with trial
            {
                if (!clicked)//timeout
                {
                    timedout = true;
                    ReactButton_Clicked(null, null);
                }
                //if (viewModel.blocktrialctr < viewModel.trialsperset && dt >= viewModel.pausedurarr[viewModel.blocktrialctr] + viewModel.timeout) 
                timedout = false;
                showcross = false;
                firstshown = false;
                _stopWatch.Restart();
                clicked = false;
                if (viewModel.blocktrialctr == viewModel.trialsperset) //done with block
                {
                    showbox = false;
                    canvasView.InvalidateSurface();
                    viewModel.IsRunning = false;
                    viewModel.ShowReact1 = false;
                    viewModel.ShowReact2 = false;
                    viewModel.ShowReact4 = false;
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

        void box1opt_CheckedChanged(System.Object sender, Xamarin.Forms.CheckedChangedEventArgs e)
        {
        }
    }
}