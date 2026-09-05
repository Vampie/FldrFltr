using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FldrFltr
{
    /// <summary>Hides a button's icon Image entirely when Tag (the icon path) isn't set.
    /// (Copied from FldrSrtr's App.UI/NullToVisibilityConverter.cs.)</summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        public static readonly NullToVisibilityConverter Instance = new NullToVisibilityConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is string s && !string.IsNullOrEmpty(s) ? Visibility.Visible : Visibility.Collapsed;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
