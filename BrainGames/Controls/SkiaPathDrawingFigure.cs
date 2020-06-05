using System;
using Xamarin.Forms;
using SkiaSharp;

namespace BrainGames.Controls
{
    public class SkiaPathDrawingFigure : SkiaFigure
    {
        SKPoint pt1, pt2;

        public SkiaPathDrawingFigure()
        {
            Type = SkiaFigureType.Path;
            FigurePaint = new SKPaint();
            IsSelected = false;
        }

        public SKPath Path;

        private bool _eraser = false;
        public bool Eraser
        {
            get { return _eraser; }
            set
            {
                _eraser = value;
            }
        }

        public override SKPoint StartPoint
        {
            get { return pt1; }
            set
            {
                pt1 = value;
            }
        }

        public override SKPoint EndPoint
        {
            get { return pt2; }
            set
            {
                pt2 = value;
            }
        }

        public override SkiaFigure MakeCopy()
        {
            SkiaPathDrawingFigure repfig = new SkiaPathDrawingFigure();
            repfig.Rectangle = this.Rectangle;
            repfig.LastFingerLocation = this.LastFingerLocation;
            repfig.PenultimateFingerLocation = this.PenultimateFingerLocation;
            repfig.StartPoint = this.StartPoint;
            repfig.EndPoint = this.EndPoint;
            repfig.Type = this.Type;
            repfig.FigurePaint = this.FigurePaint;
            repfig.IsSelected = this.IsSelected;
            repfig.Path = this.Path;
            repfig.Eraser = this.Eraser;
            return repfig;
        }

        // For dragging operations
        public override Point LastFingerLocation { set; get; }
        public override Point PenultimateFingerLocation { set; get; }

        // For the dragging hit-test
        public override bool IsInBounds(SKPoint pt)
        {
            if (Aura == 40) return (pt.Y >= this.Rectangle.Top - Aura && pt.Y <= this.Rectangle.Bottom + Aura && pt.X >= this.Rectangle.Left - Aura && pt.X <= this.Rectangle.Right + Aura);//this is for dragSymbols
            bool hit = false;
            for (int i = 0; i <= Aura; i++)
            {
                for (int j = 0; j <= Aura; j++)
                {
                    if (Path.Contains(pt.X + i, pt.Y + j) || Path.Contains(pt.X + i, pt.Y - j) || Path.Contains(pt.X - i, pt.Y + j) || Path.Contains(pt.X - i, pt.Y - j))
                    {
                        hit = true;
                        break;
                    }
                }
                if (hit) break;
            }
            return hit;
        }
    }
}