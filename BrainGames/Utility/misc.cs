using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace BrainGames.Utility
{

    public class TextChangedBehavior : Behavior<SearchBar>
    {
        protected override void OnAttachedTo(SearchBar bindable)
        {
            base.OnAttachedTo(bindable);
            bindable.TextChanged += Bindable_TextChanged;
        }

        protected override void OnDetachingFrom(SearchBar bindable)
        {
            base.OnDetachingFrom(bindable);
            bindable.TextChanged -= Bindable_TextChanged;
        }

        private void Bindable_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((SearchBar)sender).SearchCommand?.Execute(e.NewTextValue);
        }
    }

    public class misc
    {

        public static Color getgradient(Color c1, Color c2, double p)
        {
            double rStep = (c2.R - c1.R);
            double gStep = (c2.G - c1.G);
            double bStep = (c2.B - c1.B);

            var r = c1.R + (rStep * p);
            var g = c1.G + (gStep * p);
            var b = c1.B + (bStep * p);

            return Color.FromRgb(r, g, b);
        }

        public misc()
        {
        }
    }
}
