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

            if (cb != null && cb.SelectedValue != null)
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

        public static long DecodeId(string s)
        {


            byte[] data2 = new byte[8];
            // add back in all the characters removed during encoding
            Convert.FromBase64String("AAAAA" + s + "=").CopyTo(data2, 0);
            // reverse again from big to little-endian
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(data2);
            }
            long decoded = BitConverter.ToInt64(data2, 0);

            return decoded;

        }

        public static string EncodeId(long id)
        {
            byte[] data = BitConverter.GetBytes(id);
            // make data big-endian if needed
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(data);
            }
            // first 5 base-64 character always "A" (as first 30 bits always zero)
            // only need to keep the 6 characters (36 bits) at the end 
            string base64 = Convert.ToBase64String(data, 0, 8).Substring(5, 6);

            return base64;
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

    public class DateTimeToDateTimeOffsetConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                DateTime date = (DateTime)value;
                return new DateTimeOffset(date);
            }
            catch 
            {
                return DateTimeOffset.MinValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            try
            {
                DateTimeOffset dto = (DateTimeOffset)value;
                return dto.DateTime;
            }
            catch 
            {
                return DateTime.MinValue;
            }
        }
    }

    public class DateTimeToTimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null && value is DateTime)
            {
                DateTime v = (DateTime)value;
                TimeSpan x = new TimeSpan(v.Ticks);
                return x;
            }
            return new TimeSpan();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value != null && value is TimeSpan)
            {
                TimeSpan x = (TimeSpan)value;
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

    public class InvertBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((value is bool && (bool)value == true) || (value is bool? && (bool?)value==true ))
            {
                return false;
            }
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is bool)
                return (bool)value;
            return false;
        }

    }

    public class ComboBoxItemConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value as ComboBoxItem;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }

    public class RecurrenceToCheckConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                var rec = (Novena.RecurrencePattern)value;
                Novena.RecurrencePattern param = (Novena.RecurrencePattern)Enum.Parse(typeof(Novena.RecurrencePattern), parameter.ToString());
                return rec == param;
            }
            catch
            {
                return false;
            }           
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }

}



