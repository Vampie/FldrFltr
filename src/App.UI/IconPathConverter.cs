using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using App.Infrastructure.Configuration;

namespace FldrFltr
{
    /// <summary>Resolves a button's Tag (e.g. "Icons/icon-save.png" — the folder prefix is
    /// cosmetic, only the filename matters) to the actual PNG under IconSets\Default\ next to the
    /// exe. No pack-switching/override system like FldrSrtr's IconSetProvider — FldrFltr ships a
    /// single bundled set, adding an icon is dropping a PNG in IconSets\Default\.</summary>
    public class IconPathConverter : IValueConverter
    {
        public static readonly IconPathConverter Instance = new IconPathConverter();

        private static string IconSetFolder => Path.Combine(PortablePaths.BaseDirectory, "IconSets", "Default");

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string path) || string.IsNullOrEmpty(path))
            {
                return null;
            }

            string fullPath = Path.Combine(IconSetFolder, Path.GetFileName(path));
            return File.Exists(fullPath) ? fullPath : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
