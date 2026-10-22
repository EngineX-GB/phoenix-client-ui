using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace phoenix_prototype
{
    public class AvailabilityToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int v)
            {
                if (v <= 20) return Brushes.DarkBlue;
                if (v <= 40) return Brushes.DarkSlateBlue;
                if (v <= 60) return Brushes.MediumBlue;
                if (v <= 80) return Brushes.Blue;
                return Brushes.Green;
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
