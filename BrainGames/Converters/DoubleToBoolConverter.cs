using System;
using System.Globalization;
using Xamarin.Forms;

namespace BrainGames.Converters
{
    public class DoubleToBoolConverter : IValueConverter
    {
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value == true) { return 1.0; }
            else { return 0.0; }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((double)value == 0.0) { return false; }
            else { return true; }
        }
    }
}
