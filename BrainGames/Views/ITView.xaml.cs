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

//http://codetips.nl/skiagameloop.html
namespace BrainGames.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class ITView : Grid
    {
        public TMDViewModel viewModel
        {
            get { return BindingContext as TMDViewModel; }
            set { BindingContext = value; }
        }

        private readonly Stopwatch _stopWatch = new Stopwatch();
        static float toplen = 120;
        static float shortleglen = 120;
        static float longleglen = 235;
        private const double _fpsWanted = 120.0;
        float centerx, centery;

        SkiaPathDrawingFigure pifigure, pifigure_l, pifigure_r, pifigure_mask, pifigure_dot;
        SKPaint skMaskPaint = new SKPaint()
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Black,
            StrokeWidth = 40,
            IsAntialias = true,
            StrokeCap = SKStrokeCap.Round
        };
        SKPaint skPiPaint = new SKPaint()
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Black,
            StrokeWidth = 15,
            IsAntialias = true,
            StrokeCap = SKStrokeCap.Round
        };
        SKPaint skDotPaint = new SKPaint()
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.Blue
        };

        private TimeSpan ts;

        public ITView(TMDViewModel _viewModel)
        {
            InitializeComponent();
            viewModel = _viewModel;
            ts = TimeSpan.FromMilliseconds(1000.0 / _fpsWanted);
        }

        protected override void OnSizeAllocated(double w, double h)
        {
            base.OnSizeAllocated(w, h);
            if (w != -1 && h != -1) Init();
            //RunIt();
        }

        public void ReadyButton_Clicked(object sender, EventArgs e)
        {
            _stopWatch.Restart();
            Device.StartTimer(ts, TimerLoop);
        }

        public void LeftButton_Clicked(object sender, EventArgs e)
        {
            if (!viewModel.ITshown) return;
            if (viewModel.ITcor_ans == TMDViewModel.ITanswertype.left)
            {
//                fpsLabel.BackgroundColor = Color.ForestGreen;
//                asdLabel.Text = "Avg Dur: " + (viewModel.cumcorstimdur / viewModel.cortrialctr).ToString("N1", CultureInfo.InvariantCulture) + " ms";
            }
            else
            {
//                fpsLabel.BackgroundColor = Color.OrangeRed;
            }
            if (Settings.IT_EstIT > 0)
            {
//                itLabel.Text = "Est IT: " + Settings.IT_EstIT.ToString("N1", CultureInfo.InvariantCulture) + " ms";
//                itLabel.TextColor = Color.Black;
            }
        }

        public void RightButton_Clicked(object sender, EventArgs e)
        {
            if (!viewModel.ITshown) return;
            if (viewModel.ITcor_ans == TMDViewModel.ITanswertype.right)
            {
//                fpsLabel.BackgroundColor = Color.ForestGreen;
//                asdLabel.Text = "Avg Dur: " + (viewModel.cumcorstimdur / viewModel.cortrialctr).ToString("N1", CultureInfo.InvariantCulture) + " ms";
            }
            else
            {
//                fpsLabel.BackgroundColor = Color.OrangeRed;
            }
            if (Settings.IT_EstIT > 0)
            {
//                itLabel.Text = "Est IT: " + Settings.IT_EstIT.ToString("N1", CultureInfo.InvariantCulture) + " ms";
//                itLabel.TextColor = Color.Black;
            }
        }

        private void Init()
        {
            centerx = canvasView.CanvasSize.Width == 0 ? (float)canvasView.Width : canvasView.CanvasSize.Width / 2;
            centery = canvasView.CanvasSize.Height == 0 ? (float)canvasView.Height : canvasView.CanvasSize.Height / 2;

            float tl_x, tl_y, bl_x, bl_y, tr_x, tr_y, br_x, br_y;

            pifigure_l = new SkiaPathDrawingFigure();
            SKPath path = new SKPath();
            tl_x = centerx - toplen / 2;
            tl_y = centery - shortleglen / 2;
            tr_x = centerx + toplen / 2;
            tr_y = centery - shortleglen / 2;
            bl_x = centerx - toplen / 2;
            br_x = centerx + toplen / 2;

            bl_y = centery - shortleglen / 2 + longleglen;
            br_y = centery - shortleglen / 2 + shortleglen;

            path.MoveTo(bl_x, bl_y);
            pifigure_l.Path = path;
            pifigure_l.FigurePaint = skPiPaint;
            pifigure_l.Path.LineTo(new SKPoint(tl_x, tl_y));
            pifigure_l.Path.LineTo(new SKPoint(tr_x, tr_y));
            pifigure_l.Path.LineTo(new SKPoint(br_x, br_y));
            pifigure_l.Rectangle = pifigure_l.Path.Bounds;

            pifigure_r = new SkiaPathDrawingFigure();
            path = new SKPath();
            bl_y = centery - shortleglen / 2 + shortleglen;
            br_y = centery - shortleglen / 2 + longleglen;

            path.MoveTo(bl_x, bl_y);
            pifigure_r.Path = path;
            pifigure_r.FigurePaint = skPiPaint;
            pifigure_r.Path.LineTo(new SKPoint(tl_x, tl_y));
            pifigure_r.Path.LineTo(new SKPoint(tr_x, tr_y));
            pifigure_r.Path.LineTo(new SKPoint(br_x, br_y));
            pifigure_r.Rectangle = pifigure_r.Path.Bounds;

            pifigure_mask = new SkiaPathDrawingFigure();
            path = new SKPath();
            float offset = (skMaskPaint.StrokeWidth - skPiPaint.StrokeWidth) / 2;
            path.MoveTo(bl_x - offset, Math.Max(bl_y - offset, br_y - offset));
            pifigure_mask.Path = path;
            pifigure_mask.FigurePaint = skMaskPaint;
            pifigure_mask.Path.LineTo(new SKPoint(tl_x - offset, tl_y - offset));
            pifigure_mask.Path.LineTo(new SKPoint(tr_x - offset, tr_y - offset));
            pifigure_mask.Path.LineTo(new SKPoint(br_x - offset, Math.Max(bl_y - offset, br_y - offset)));
            pifigure_mask.Rectangle = pifigure_mask.Path.Bounds;

            float radius = 10;
            SKRect rect = new SKRect(centerx - radius, centery - radius,
                         centerx + radius, centery + radius);
            pifigure_dot = new SkiaPathDrawingFigure();
            path = new SKPath();
            path.MoveTo(centerx, centery + shortleglen / 2);
            pifigure_dot.Path = path;
            pifigure_dot.FigurePaint = skDotPaint;
            pifigure_dot.Path.ArcTo(rect, 0, 315, false);
            pifigure_dot.Path.ArcTo(rect, 315, 45, false);
            pifigure_dot.Path.Close();
            pifigure_dot.Rectangle = pifigure_dot.Path.Bounds;
        }


        private bool TimerLoop()
        {
            if (!viewModel.IsRunning) { return true; }
            // get the elapsed time from the stopwatch because the 1/30 timer interval is not accurate and can be off by 2 ms
            var dt = _stopWatch.Elapsed.TotalMilliseconds;

            if (dt < viewModel.ITpausedur - viewModel.ITminstimdur / 2) //keep orienting cue onscreen
            {
                pifigure = pifigure_dot;
            }
            else
            {
                if (viewModel.ITemppausedur == 0) viewModel.ITemppausedur = dt;
                if (dt < viewModel.ITemppausedur + viewModel.ITcurstimdur - viewModel.ITminstimdur / 2) //keep stim onscreen
                {
                    if (pifigure == pifigure_dot) //Start of round; decide which stim to show
                    {
                        if (viewModel.ITcor_ans == TMDViewModel.ITanswertype.left)
                        {
                            pifigure = pifigure_l;
                        }
                        else
                        {
                            pifigure = pifigure_r;
                        }
                    }
                }
                else
                {
                    if (viewModel.ITempstimdur == 0)
                    {
                        viewModel.ITempstimdur = dt - viewModel.ITemppausedur;
                        viewModel.ITempstimtimearr.Add(viewModel.ITempstimdur);
//                        fpsLabel.Text = "Stim Dur: " + viewModel.ITempstimdur.ToString("N0", CultureInfo.InvariantCulture) + " ms";
                        viewModel.ITshown = true;
                    }
                    if (dt < viewModel.ITemppausedur + viewModel.ITempstimdur + viewModel.ITmaskdur - viewModel.ITminstimdur / 2) //keep mask onscreen
                    {
                        pifigure = pifigure_mask;
                    }
                    else //You've shown stim and mask; clear screen
                    {
                        viewModel.ITempmaskdur = dt - viewModel.ITemppausedur - viewModel.ITempstimdur;
                        pifigure = null;
                        canvasView.InvalidateSurface();
                        return false;
                    }
                }
            }
            // trigger the redrawing of the view
            canvasView.InvalidateSurface();

            return true;
        }

        private void OnPainting(object sender, SKPaintSurfaceEventArgs e)
        {
            var surface = e.Surface;
            var canvas = surface.Canvas;

            // clear the view with the specified background color
            canvas.Clear();
            if (pifigure is null) return;
            canvas.DrawPath(((SkiaPathDrawingFigure)pifigure).Path, pifigure.FigurePaint);
        }
    }
}