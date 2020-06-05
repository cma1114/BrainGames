using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace BrainGames.Controls
{
    public class SkiaFrame : Frame
    {
        //        public Color Circle_color = Color.White;
        public Color Circle_color
        {
            get { return (Color)GetValue(Circle_colorProperty); }
            set { SetValue(Circle_colorProperty, value); }
        }
        //        public static readonly BindableProperty Circle_colorProperty = BindableProperty.Create(nameof(Circle_color), typeof(Color), typeof(SkiaFrame), null, BindingMode.OneWay, null, OnCircleColorSeqChanged);
        public static readonly BindableProperty Circle_colorProperty = BindableProperty.Create("Circle_color", typeof(Color), typeof(SkiaFrame), Color.White);

        public void DragLabel_PaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();
            SKPaint paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = Circle_color.ToSKColor(),
                StrokeWidth = 6
            };
            canvas.DrawCircle(info.Width / 2, info.Height / 2, info.Height / 2, paint);
        }
/*
        private static void OnCircleColorSeqChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            var control = (SkiaFrame)bindable;
            if (control != null)
            {
                if (newvalue is Color _circle_color)
                {
                    control.Circle_color = _circle_color;
                }
            }
        }
        */
    }
}
