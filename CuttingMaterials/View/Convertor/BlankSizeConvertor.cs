using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CuttingMaterials.View.Convertor
{
    class BlankSizeConvertor : IValueConverter
    {
        string[] blankSize = new string[2];

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var arr = ((string)value).Split('x');
            if(arr.Count() != 0)
                blankSize = arr;
            switch ((string)parameter)
            {
                case "width":
                    if (arr.Count() == 0)
                        return 0;
                    else
                        return arr[0];
                case "height":
                    if (arr.Count() == 0)
                        return 0;
                    else
                        return arr[1];
                default:
                    return 0;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((string)parameter)
            {
                case "width":
                    blankSize[0] = (string)value;
                    return string.Join("x", blankSize);
                case "height":
                    blankSize[1] = (string)value;
                    return string.Join("x", blankSize);
                default:
                    return 0;
            }
        }
    }
}
