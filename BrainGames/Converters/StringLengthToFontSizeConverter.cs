using System;
using System.Globalization;
using Xamarin.Forms;
using System.Collections.Generic;
using System.Linq;

namespace BrainGames.Converters
{
    public class StringLengthToFontSizeConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int decorations = ((string)value).Count(x => x.Equals('`'));
            string[] s = new string[] { };
            s = ((string)parameter).Split('~');
            double fontsize = double.Parse(s[0]);
            double fontmultiplier = double.Parse(s[1]);
            s = ((string)value).Split('/');
            for (int i = 1; i < s.Length; i++)
            {
                fontsize *= fontmultiplier;
            }
            for (int i = 0; i < decorations; i+=2)
            {
                fontsize *= (1-(1-fontmultiplier)/2);
            }
            return (float)fontsize;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Only one way bindings are supported with this converter");
        }
    }
}
