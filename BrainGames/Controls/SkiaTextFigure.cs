using System;
using Xamarin.Forms;
using SkiaSharp;

namespace BrainGames.Controls
{
    class SkiaTextFigure : SkiaFigure
    {

        public SkiaTextFigure(SKPaint tp, String txt)
        {
            Type = SkiaFigureType.Character;
            LastFingerLocation = Point.Zero;
            Text = txt;
            FigurePaint = tp;
            // Find the text bounds
            FigurePaint.MeasureText(Text, ref _textBounds);
            IsSelected = false;
        }

        private SKRect _textBounds;
        public SKRect textBounds { 
            set { _textBounds = value; }
            get { return _textBounds; }
            }

        public String Text { set; get; }

        public SKRect RenderedRectangle { set; get; }

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

            float charloffset = textBounds.Left;
            float chartoffset = textBounds.Top;
//            return pt.X > this.Rectangle.Left + charloffset - Aura && pt.X < this.Rectangle.Left + charloffset + this.Rectangle.Width + Aura && pt.Y > this.Rectangle.Top + chartoffset - Aura && pt.Y < this.Rectangle.Top + chartoffset + this.Rectangle.Height + Aura;
            return pt.X > this.Rectangle.Left - Aura / 2 && pt.X < this.Rectangle.Left + this.Rectangle.Width + Aura / 2 && pt.Y > this.Rectangle.Top - Aura / 2 && pt.Y < this.Rectangle.Top + this.Rectangle.Height + Aura / 2;
        }
    }
}