using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Novena_Reminder.Controller
{
    public static class Helper
    {
        public static int Combobox2Int(ComboBox cb)
        {
            int ret = 0;

            if (cb != null && cb.SelectedValue !=null )
            {
                int.TryParse(cb.SelectedValue.ToString(), out ret);
            }

            return ret;
        }

        public static void PopulateComboboxWithIntInterval(ComboBox cb, int min, int max, int interval = 1)
        {

            var values = new List<int>();
            for (int x = min; x <= max; x = x + interval)
            {
                values.Add(x);
            }
            cb.ItemsSource = values;
        }
    }

    public class BooleanToVisibilityConverter : IValueConverter
    {
        private object GetVisibility(object value)
        {
            if (!(value is bool))
                return Visibility.Collapsed;
            bool objValue = (bool)value;
            if (objValue)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return GetVisibility(value);
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

    }

    public class DateTimeToTimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime)
            {
                DateTime v = (DateTime)value;
                TimeSpan x = new TimeSpan(v.Ticks);
                return x;
            }
            return new TimeSpan();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is TimeSpan)
            {
                TimeSpan x  = (TimeSpan)value;
                DateTime v = new DateTime(x.Ticks);
                return v;
            }
            return new DateTime();
        }
    }

    public class NullableBooleanToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool?)
            {
                return (bool)value;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is bool)
                return (bool)value;
            return false;
        }
    }
}



