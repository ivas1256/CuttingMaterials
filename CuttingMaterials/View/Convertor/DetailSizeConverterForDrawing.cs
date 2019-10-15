using CuttingMaterials.Data.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CuttingMaterials.View.Convertor
{
    class DetailSizeConverterForDrawing : IValueConverter
    {
        DetailSize obj;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var prj = (Project) value;
            int detailIndex = (int)parameter.GetType().GetProperty("detailIndex").GetValue(parameter, null);
            int sizesCount = (int)parameter.GetType().GetProperty("sizesCount").GetValue(parameter, null);

            obj = prj.Details.ElementAt(detailIndex).Sizes.ElementAt(sizesCount - 1);
            return obj.GetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            obj.GetValue = (int)value;
            return value;
        }
    }
}
