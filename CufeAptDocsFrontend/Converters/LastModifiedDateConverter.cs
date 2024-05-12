using System.Globalization;
using System.Windows.Data;

namespace MRK
{
    public class LastModifiedDateConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime lastModificationDate)
            {
                TimeSpan timeSpan = DateTime.Now - lastModificationDate;
                if (timeSpan.Days > 0)
                {
                    return $"{timeSpan.Days} days ago";
                }
                else if (timeSpan.Hours > 0)
                {
                    return $"{timeSpan.Hours} hours ago";
                }
                else if (timeSpan.Minutes > 0)
                {
                    return $"{timeSpan.Minutes} mins ago";
                }
                else
                {
                    return "Just now";
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
