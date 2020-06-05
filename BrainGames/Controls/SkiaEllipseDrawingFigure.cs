using System;
using Xamarin.Forms;
using SkiaSharp;

namespace BrainGames.Controls
{
    class SkiaEllipseDrawingFigure : SkiaFigure
    {
        SKPoint pt1, pt2;

        public SkiaEllipseDrawingFigure()
        {
            Type = SkiaFigureType.Ellipse;
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


        // For dragging operations
        public override Point LastFingerLocation { set; get; }
        public override Point PenultimateFingerLocation { set; get; }

        // For the dragging hit-test
        public override bool IsInBounds(SKPoint pt)
        {
            SKRect rect = Rectangle;

            return (Math.Pow(pt.X - rect.MidX, 2) / Math.Pow(rect.Width / 2, 2) +
                    Math.Pow(pt.Y - rect.MidY, 2) / Math.Pow(rect.Height / 2, 2)) < 1;
        }
    }
}