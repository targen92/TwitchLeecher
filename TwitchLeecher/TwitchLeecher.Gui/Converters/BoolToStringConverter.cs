using System;
using System.Globalization;
using System.Windows.Data;

namespace TwitchLeecher.Gui.Converters
{
    public class BoolToStringConverter : IValueConverter
    {
        public string FalseValue { get; set; }
        public string TrueValue { get; set; }

        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return FalseValue;
            else
                return (bool)value ? TrueValue : FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value != null ? value.Equals(TrueValue) : false;
        }
        #endregion IValueConverter Members
    }
}