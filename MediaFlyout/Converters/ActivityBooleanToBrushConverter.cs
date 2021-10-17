using System;
using System.Globalization;
using System.Windows.Data;
using SourceChord.FluentWPF;

namespace MediaFlyout.Converters
{
    class ActivityBooleanToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null && (bool)value ? AccentColors.ImmersiveSystemAccentBrush :
                System.Windows.Application.Current.Resources.MergedDictionaries[0]["SystemListMediumColorBrush"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
