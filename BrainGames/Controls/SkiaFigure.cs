using System;
using Xamarin.Forms;
using SkiaSharp;

namespace BrainGames.Controls
{
    public abstract class SkiaFigure
    {

        public enum SkiaFigureType
        {
            Character,
            Arrowhead,
            Dash,
            Ellipse,
            Rectangle,
            RoundedRectangle,
            Line,
            Path
        }

        public SkiaFigureType Type;

        public SKPaint FigurePaint { set; get; }

        public virtual SKPoint StartPoint { set; get; }
        public virtual SKPoint EndPoint { set; get; }

        private float _aura = 20;
        public float Aura
        {
            set { _aura = value; }
            get { return _aura; }
        }

        public SKRect Rectangle { set; get; }

        // For dragging operations
        public virtual Point LastFingerLocation { set; get; }
        public virtual Point PenultimateFingerLocation { set; get; }

        private SKColor _selectedColor = SKColors.YellowGreen;
        private SKColor _readytodeleteColor = SKColors.Red;

        private SKColor _color;
        public void Select()
        {
            _color = FigurePaint.Color;
            FigurePaint.Color = _selectedColor;
            IsSelected = true;
        }

        public void DeSelect()
        {
            FigurePaint.Color = _color;
            IsSelected = false;
        }

        public bool IsSelected;

        public void SetReadyToDelete()
        {
            if(!IsSelected) return;
            FigurePaint.Color = _readytodeleteColor;
        }

        public void UnSetReadyToDelete()
        {
            if (!IsSelected) return;
            FigurePaint.Color = _selectedColor;
        }

        // For the dragging hit-test
        public abstract bool IsInBounds(SKPoint pt);

        public virtual SkiaFigure MakeCopy() { return null; }
    }
}