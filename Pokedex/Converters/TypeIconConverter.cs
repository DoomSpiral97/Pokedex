using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Pokedex.Converters;

public class TypeIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string typeName)
        {
            // "Fire" → "pack://application:,,,/Media/Icons/Icon Fire.png"
            return $"pack://application:,,,/Media/Icons/Icon {typeName}.png";
        }
        return string.Empty;
    }


    //nicht eingebaut
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
