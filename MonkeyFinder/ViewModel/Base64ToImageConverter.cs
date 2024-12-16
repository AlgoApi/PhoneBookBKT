using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.IO;
using Microsoft.Maui.Controls;
using Microsoft.Extensions.Primitives;

namespace MonkeyFinder.ViewModel
{
    public class Base64ToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string base64String && !string.IsNullOrWhiteSpace(base64String))
            {
                if (Uri.TryCreate(base64String, UriKind.Absolute, out _))
                {
                    // If the value is a valid URI, load the image from the URI
                    return ImageSource.FromUri(new Uri(base64String));
                }
                try
                {
                    var imageBytes = System.Convert.FromBase64String(base64String);
                    return ImageSource.FromStream(() => new MemoryStream(imageBytes));
                }
                catch
                {
                    return null; // В случае ошибки вернём пустое изображение
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
