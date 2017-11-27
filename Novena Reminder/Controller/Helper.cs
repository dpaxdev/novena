using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Foundation.Collections;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using System.Globalization;
using Novena_Reminder.Model;

namespace Novena_Reminder.Controller
{
    public static class Helper
    {
        public static ToastNotifier tn;
        public static bool IsMobile
        {
            get
            {
                var qualifiers = Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().QualifierValues;
                return (qualifiers.ContainsKey("DeviceFamily") && qualifiers["DeviceFamily"] == "Mobile");
            }
        }
        public static ResourceLoader loader = new ResourceLoader();
        public static string _t(string stringName)
        {

            return loader.GetString(stringName);
        }

        public static async Task<bool> ShowNovenaDeleteDialog(Novena nov)
        {

            ContentDialog deleteDialog = new ContentDialog
            {
                Title = _t("s0029"),//"Stergere novena",
                Content = _t("s0025"),// "Sigur doriti sa stergeti aceasta novena?",
                PrimaryButtonText = _t("s0027"),//"Sterge",
                SecondaryButtonText = _t("s0021")//"Nu"
            };

            if (nov.IsOngoing)
                deleteDialog.Content = _t("s0026") + "\n" + deleteDialog.Content;//Aceasta novena este in desfasurare.
            ContentDialogResult result = await deleteDialog.ShowAsync();

            // Delete the novena if the user clicked the primary button.

            if (result == ContentDialogResult.Primary)
            {
                return true;
            }
            else
            {
                return false;
            }

        }



        public static async Task<bool> ShowNovenaMassDeleteDialog(int count)
        {

            ContentDialog deleteDialog = new ContentDialog
            {
                Title = count > 1 ? String.Format(_t("s0024"), count) : _t("s0030"),//"Stergere " + count + " novene selectate" : "Stergere novena selectata",
                Content = count > 1 ? _t("s0023") : _t("s0025"), //"Sigur doriti sa stergeti " + (count > 1 ? "aceste novene" : "aceasta novena?"),
                PrimaryButtonText = _t("s0027"), //"Sterge",
                SecondaryButtonText = _t("s0021") //"Nu"
            };

            ContentDialogResult result = await deleteDialog.ShowAsync();

            // Delete the novena if the user clicked the primary button.

            if (result == ContentDialogResult.Primary)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public static void DoGeneralMaintenace()
        {
            //read novenas
            var Novenas = Storage.GetCollection();

            //perform maintainance
            foreach (Novena nov in Novenas)
            {
                if (nov.DoMaintenance())
                    Storage.SaveNovena(nov);
                //also manage alarms 
                if (nov.Alarm)
                {
                    Helper.ManageAlarms(nov);
                }
                Helper.UpdateLiveTile();
            }

            //if we got to here and task has not been stopped we can register the maintenance as done and save a timestamp to avoid running again today.
            Storage.WriteSetting("LastGeneralMaintenanceTime", DateTime.UtcNow.ToString());
        }


        public static void UpdateLiveTile()
        {
            // Create a tile update manager for the specified syndication feed.
            var updater = TileUpdateManager.CreateTileUpdaterForApplication();
            updater.EnableNotificationQueue(true);
            updater.Clear();

            // Keep track of the number feed items that get tile notifications.
           
            XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150Text01);
            //var xmlstr = tileXml.GetXml(); //debugging variable
            var novs = Storage.GetCollection();

            // Create a tile notification for each feed item.
            int i = 0;
            foreach (var nov in novs)
            {

                //get the next alarm
                var TextRow = ""; //this is where we build the notification string for each individual novena
                if (tn == null)
                    tn = ToastNotificationManager.CreateToastNotifier();

                var Notifications = tn.GetScheduledToastNotifications();
                //Step 1: delete all notifications for this Novena if any

                ScheduledToastNotification firstAlarm = null;
                foreach (ScheduledToastNotification notif in Notifications)
                {
                    if (notif.Group == nov.ID)
                    {
                        firstAlarm = notif;
                        break;
                    }
                }


                if (firstAlarm != null)
                {
                    DateTime dt = firstAlarm.DeliveryTime.DateTime;
                    if (dt.Day != DateTime.Now.Day)
                        TextRow += String.Format("{0:dd.MM} ", dt);
                    else
                        TextRow += String.Format("{0:HH:mm} ", dt);

                }

                //  var AlarmTime = nov.AlarmTime;
                 TextRow += nov.CurrentProgress + " - " + nov.Name;
                if (!nov.IsOngoing)
                    continue;
                tileXml.GetElementsByTagName("text")[i++].InnerText = TextRow;
              
                if (i == 4) //we're only interested in the first 4 novenas since the output space of the tile is limited to 4 lines
                    break;                  
            }                          
            
            // Create a new tile notification.
            updater.Update(new TileNotification(tileXml));
        }

       public static int CountDays(DayOfWeek day, DateTime start, DateTime end)
        {
            TimeSpan ts = end - start;                       // Total duration
            int count = (int)Math.Floor(ts.TotalDays / 7);   // Number of whole weeks
            int remainder = (int)(ts.TotalDays % 7);         // Number of remaining days
            int sinceLastDay = (int)(end.DayOfWeek - day);   // Number of days since last [day]
            if (sinceLastDay < 0) sinceLastDay += 7;         // Adjust for negative days since last [day]

            // If the days in excess of an even week are greater than or equal to the number days since the last [day], then count this one, too.
            if (remainder >= sinceLastDay) count++;

            return count;
        }

        public static int GetTotalDurationOfWeekdaysOccurence(int weekday, int occurences, int startAt)
        {

            int total = 7 * occurences;
            int dif = weekday - startAt;
            if (dif < 0)
                dif += 7;
            total += dif;

                return total;
          
        }

        public static void DeleteNovena(Novena nov)
        {
            nov.Deactivate();
            ManageAlarms(nov);
            Storage.DeleteNovena(nov.ID);
        }
        public static void ManageAlarms(Novena nov)
        {
            if (tn == null)
                tn = ToastNotificationManager.CreateToastNotifier();

            var Notifications = tn.GetScheduledToastNotifications();
            //Step 1: delete all notifications for this Novena if any
            foreach (ScheduledToastNotification notif in Notifications)
            {
                if (notif.Group == nov.ID)
                    tn.RemoveFromSchedule(notif);
            }
            //Step 2: decide if we need to add the notifications back;
            if (nov.IsActive && nov.Alarm)
            {

                DateTime CurrentDate = new DateTime(
                                       DateTime.Today.Year,
                                       DateTime.Today.Month,
                                       DateTime.Today.Day,
                                       nov.AlarmTime.Hour,
                                       nov.AlarmTime.Minute,
                                       0
                                   );

                //We schedule alarms for the whole next run of the Novena 

                var AlarmsCount = Math.Min(nov.Duration, 10);
                for (int x = nov.CurrentProgress; x < AlarmsCount; x++)
                {
                    int AlarmOffset = x - nov.CurrentProgress;

                    DateTime AlarmTime;

                    if (nov.IsOngoing)
                    {
                        if (nov.Weekday == 0)
                        {
                            AlarmTime = CurrentDate.AddDays(AlarmOffset);
                        }
                        else
                        {
                            int firstWeekday = GetDaysTillFirstOccurenceOfWeekday(nov.Weekday - 1);
                            AlarmTime = CurrentDate.AddDays(firstWeekday + AlarmOffset * 7);
                        }

                    }
                    else
                    {
                        //so we have a scheduled start in the future
                        //determine the first day
                        var ScheduledAlarmDate = new DateTime(nov.ScheduledStartDate.Year, nov.ScheduledStartDate.Month, nov.ScheduledStartDate.Day, CurrentDate.Hour, CurrentDate.Minute, 0);
                        var Delay = ScheduledAlarmDate.Subtract(CurrentDate);
                        if (nov.Weekday == 0)
                        {
                            AlarmTime = CurrentDate.AddDays(AlarmOffset + Delay.Days);                           
                        }
                        else
                        {
                            int firstWeekday = GetDaysTillFirstOccurenceOfWeekday(nov.Weekday - 1, Delay.Days);
                            AlarmTime = CurrentDate.AddDays(firstWeekday + AlarmOffset * 7);
                        }                       
                    }
                    AddScheduledToastNotification(nov.ID, nov.Name, String.Format(_t("s0031"), nov.CurrentProgress.ToString()), AlarmTime, nov.AlarmSound);
                }
            }
        }
        public static int GetDaysTillFirstOccurenceOfWeekday(int weekday)
        {
            return GetDaysTillFirstOccurenceOfWeekday(weekday, 0);             
        }
        public static int GetDaysTillFirstOccurenceOfWeekday(int weekday, int offset)
        {
            int weekdayIn = weekday - (int)(DateTime.Now.AddDays(offset).DayOfWeek);

            if (weekdayIn < 0)
                weekdayIn += 7;

            return weekdayIn + offset;
        }

        private static void AddScheduledToastNotification(string group, string title, string contentText, DateTime time, string alarmTone = "")
        {

            if (time < DateTime.Now) return; //cannot schedule a notification in the past, app would crash...
            if (group != null && group != "")
            {
                ToastContent content = new ToastContent()
                {
                    Duration = ToastDuration.Long,


                    Visual = new ToastVisual()
                    {

                        AddImageQuery = false,
                        BindingGeneric = new ToastBindingGeneric()
                        {
                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = title,
                                    HintMaxLines = 1
                                },

                                new AdaptiveText()
                                {
                                    Text = contentText
                                }
                            }
                        }
                    },
                    Scenario = ToastScenario.Alarm,
                    Audio = new ToastAudio()
                    {
                        Src = alarmTone == "" ? null : GetSoundUriFromDisplayName(alarmTone),
                        Loop = true,// new List<string>() { "Default", "IM", "Mail", "Reminder", "SMS" }.Contains(alarmTone) ? false : true,
                        Silent = alarmTone == "" ? true : false
                    },
                    Actions = new ToastActionsCustom()
                    {
                        Inputs =
                        {
                            new ToastSelectionBox("snoozeTime")
                            {
                                DefaultSelectionBoxItemId = "10",
                                Items =
                                {
                                    new ToastSelectionBoxItem("5", String.Format(_t("s0003"), 5)),
                                    new ToastSelectionBoxItem("10", String.Format(_t("s0003"), 10)),
                                    new ToastSelectionBoxItem("30", String.Format(_t("s0003"), 30)),
                                    new ToastSelectionBoxItem("60", _t("s0004")),
                                    new ToastSelectionBoxItem("120", String.Format(_t("s0005"), 2)),
                                }
                            }
                        },
                        Buttons =
                        {
                            new ToastButtonSnooze()
                            {
                                SelectionBoxId = "snoozeTime"
                            },
                            new ToastButtonDismiss()
                        }
                    }
                };
                var xml = content.GetXml();
                var scheduledToast = new ScheduledToastNotification(xml, new DateTimeOffset(time))
                {
                    Group = group
                };
                tn.AddToSchedule(scheduledToast);
            }
        }

        public static BackgroundTaskRegistration RegisterBackgroundTask(String taskEntryPoint, String name, IBackgroundTrigger trigger, IBackgroundCondition condition, bool needAccess = true)
        {
            if (needAccess)
            {
                // If the user denies access, the task will not run.
                var requestTask = BackgroundExecutionManager.RequestAccessAsync();
            }

            var builder = new BackgroundTaskBuilder()
            {
                Name = name,
                TaskEntryPoint = taskEntryPoint
            };
            builder.SetTrigger(trigger);

            if (condition != null)
            {
                builder.AddCondition(condition);
            }

            BackgroundTaskRegistration task = builder.Register();            

            return task;
        }

        public static void ShowDialog(string title, string content)
        {
            ContentDialog WarningDialog = new ContentDialog
            {
                Title = title,
                Content = content,
                PrimaryButtonText = _t("OK")//"OK"
            };
            WarningDialog.ShowAsync().GetResults();
        }
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

        public static Uri GetSoundUriFromDisplayName(string displayName)
        {
            List<string> Exceptions = new List<string>() { "Default", "IM", "Mail", "Reminder", "SMS" };
            var uriString = "";
            if (Exceptions.Contains(displayName))
                uriString = "ms-winsoundevent:Notification." + displayName;
            else
                uriString = "ms-winsoundevent:Notification.Looping." + displayName;
            return new Uri(uriString);
        }
        public static string ConvertIntToWeekday(int value, string language)
        {
            if(value == 0)
                return _t("s0006");
            CultureInfo ci; 
            if (language == null ||  new CultureInfo(language) == null)
                ci = CultureInfo.CurrentCulture;
            else
                ci = new CultureInfo(language);
            return ci.DateTimeFormat.DayNames[value-1];
        }

        public static int ConvertWeekdayToInt(string value, string language)
        {
            CultureInfo ci;
            if (language == null || new CultureInfo(language) == null)
                ci = CultureInfo.CurrentCulture;
            else
                ci = new CultureInfo(language);
            int ret = Array.IndexOf(ci.DateTimeFormat.DayNames, value);
            if (ret <= 0)
                return 0;
            else
                return ret+1;
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

    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        private object GetVisibility(object value)
        {
            if (!(value is bool))
                return Visibility.Collapsed;
            bool objValue = (bool)value;
            if (!objValue)
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
            if ((value is bool && (bool)value == true) || (value is bool? && (bool?)value == true))
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

    public class RecurrenceToVisibilityConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                var rec = (Novena.RecurrencePattern)value;
                Novena.RecurrencePattern param = (Novena.RecurrencePattern)Enum.Parse(typeof(Novena.RecurrencePattern), parameter.ToString());
                return rec == param ? Visibility.Visible : Visibility.Collapsed;
            }
            catch
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }

    public class IntToWeekdayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {

            if (value != null && value is int)
            {
                return Helper.ConvertIntToWeekday((int)value, language);
            }
            return Helper._t("s0006");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value != null && value is string)
            {

               return  Helper.ConvertWeekdayToInt((string)value, language);
              
              
            }
            return 0;
        }
    }
}





