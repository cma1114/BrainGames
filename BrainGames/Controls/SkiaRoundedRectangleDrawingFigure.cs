using System;
using Xamarin.Forms;
using SkiaSharp;


namespace BrainGames.Controls
{
    class SkiaRoundedRectangleDrawingFigure : SkiaFigure
    {
        SKPoint pt1, pt2;

        public SkiaRoundedRectangleDrawingFigure()
        {
            Type = SkiaFigureType.RoundedRectangle;
            FigurePaint = new SKPaint();
            IsSelected = false;
        }

        public override SKPoint StartPoint
        {
            get { return pt1; }
            set
            {
                pt1 = value;
                MakeRectangle();
            }
        }

        public override SKPoint EndPoint
        {
            get { return pt2; }
            set
            {
                pt2 = value;
                MakeRectangle();
            }
        }

        void MakeRectangle()
        {
            Rectangle = new SKRect(pt1.X, pt1.Y, pt2.X, pt2.Y).Standardized;
        }

        public override SkiaFigure MakeCopy()
        {
            SkiaRectangleDrawingFigure repfig = new SkiaRectangleDrawingFigure();
            repfig.Rectangle = new SKRect(this.Rectangle.Left, this.Rectangle.Top, this.Rectangle.Right, this.Rectangle.Bottom);
            repfig.LastFingerLocation = this.LastFingerLocation;
            repfig.PenultimateFingerLocation = this.PenultimateFingerLocation;
            repfig.StartPoint = this.StartPoint;
            repfig.EndPoint = this.EndPoint;
            repfig.Type = this.Type;
            repfig.FigurePaint = new SKPaint();
            repfig.FigurePaint = this.FigurePaint;
            repfig.IsSelected = this.IsSelected;
            return repfig;
        }

        // For dragging operations
        public override Point LastFingerLocation { set; get; }
        public override Point PenultimateFingerLocation { set; get; }

        // For the dragging hit-test
        public override bool IsInBounds(SKPoint pt)
        {
            SKRect rect = this.Rectangle;

            return (pt.Y >= rect.Top && pt.Y <= rect.Bottom && pt.X >= rect.Left && pt.X <= rect.Right);

        }
    }
}
