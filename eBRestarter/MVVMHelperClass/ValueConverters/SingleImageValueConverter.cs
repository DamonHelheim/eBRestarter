using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace eBRestarter.MVVMHelperClass.ValueConverters
{
    public class SingleImageValueConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            bool trueOrfalse  = (bool)value[0];

            string ImagePath1 = (string)value[1];
            string ImagePath2 = (string)value[2];

            if (trueOrfalse is false)
            {
                return _ = new BitmapImage(new Uri(ImagePath1, UriKind.RelativeOrAbsolute));

            } else
            {
                return _ = new BitmapImage(new Uri(ImagePath2, UriKind.RelativeOrAbsolute));
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
