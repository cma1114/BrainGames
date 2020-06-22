using System;
using System.Globalization;
using Xamarin.Forms;

namespace BrainGames.Converters
{
    public class IntToBoolConverter : IValueConverter
    {
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value == true) { return 1; }
            else { return 0; }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((int)value == 0) { return false; }
            else { return true; }
        }
    }
}
