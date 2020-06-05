using System;
using Xamarin.Forms;
using SkiaSharp;

namespace BrainGames.Controls
{
    public class SkiaArrowheadFigure : SkiaFigure
    {

        public SkiaArrowheadFigure(SKPaint tp, String txt)
        {
            Type = SkiaFigureType.Arrowhead;
            LastFingerLocation = Point.Zero;
            Text = txt;
            FigurePaint = tp;
            // Find the text bounds
            FigurePaint.MeasureText(Text, ref _textBounds);
            IsSelected = false;
        }

        private SKRect _textBounds;
        public SKRect textBounds
        {
            set { _textBounds = value; }
            get { return _textBounds; }
        }

        public String Text { set; get; }

        public float angle;

        // For dragging operations
        public override Point LastFingerLocation { set; get; }
        public override Point PenultimateFingerLocation { set; get; }

        // For the dragging hit-test
        public override bool IsInBounds(SKPoint pt)
        {
            //float textWidth = TextPaint.MeasureText(Text)/2;
            /*
             *return pt.X > LastFingerLocation.X + textBounds.Left && pt.X < LastFingerLocation.X + textBounds.Left + textBounds.Width && 
                            pt.Y > LastFingerLocation.Y + textBounds.Top && pt.Y < LastFingerLocation.Y + textBounds.Top + textBounds.Height;
                            */
            float aura = 20;
            float charloffset = textBounds.Left;
            float chartoffset = textBounds.Top;
            return pt.X > this.Rectangle.Left + charloffset - aura && pt.X < this.Rectangle.Left + charloffset + this.Rectangle.Width + aura && pt.Y > this.Rectangle.Top + chartoffset - aura && pt.Y < this.Rectangle.Top + chartoffset + this.Rectangle.Height + aura;
        }
    }
}