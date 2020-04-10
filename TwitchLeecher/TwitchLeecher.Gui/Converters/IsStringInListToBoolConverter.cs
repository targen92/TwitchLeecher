using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using TwitchLeecher.Core.Enums;

namespace TwitchLeecher.Gui.Converters
{
    public class IsStringInListToBoolConverter : IMultiValueConverter
    {
        #region IValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2)
            {
                throw new ApplicationException("Values has to have 2 items!");
            }

            if (!(values[0] is string))
            {
                throw new ApplicationException("First value has to be of type '" + typeof(string).FullName + "'!");
            }

            if (!(values[1] is IEnumerable<string>))
            {
                throw new ApplicationException("Second value has to be of type '" + typeof(IEnumerable<string>).FullName + "'!");
            }

            string id = (string)values[0];
            IEnumerable<string> idList = (IEnumerable<string>)values[1];

            foreach(var tempId in idList)
            {
                if (tempId == id)
                {
                    return true;
                }
            }

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("This converter can only convert!");
        }

        #endregion IValueConverter Members
    }
}