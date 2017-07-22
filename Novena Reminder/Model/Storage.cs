using Novena_Reminder.Controller;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Novena_Reminder.Model
{
    public static class Storage
    {

        static string NOV_SETTINGS_PREFIX = "nov";
        static ApplicationDataContainer localSettings = ApplicationData.Current.RoamingSettings;
        static Encoding Enc = Encoding.UTF8;


        public static ObservableCollection<Novena> GetCollection()
        {
            ObservableCollection<Novena> ret = new ObservableCollection<Novena>();

            List<string> SerializedNovenas = ReadSerializedNovenas();
            foreach (string Serialized in SerializedNovenas)
                ret.Add(UnserializeNovena(Serialized));
            return ret;

        }

        public static void SaveCollection(ObservableCollection<Novena> novenas)
        {

            foreach (Novena novena in novenas)
                SaveNovena(novena);
        }

        public static void SaveNovena(Novena nov)
        {

            WriteNovena(nov);
            Helper.ManageAlarms(nov);
        }


        public static Novena GetNovenaById(string id)
        {
            String nov = ReadSerializedNovenaById(id);
            return UnserializeNovena(nov);
        }

        public static void DeleteNovena(string id)
        {
            DeleteSetting(NOV_SETTINGS_PREFIX + id.ToString());
        }

        private static Novena UnserializeNovena(string nov)
        {
            if (nov == null || nov.Length == 0)
                return null;
            Stream stream = String2Stream(nov);
            DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(Novena));
            return (Novena)json.ReadObject(stream);
        }



        private static void WriteNovena(Novena nov)
        {
            if (!nov.IsActive)
            {
                nov.StartDate = DateTime.MinValue;
            }
            /*
            PropertyInfo[] properties = typeof(Novena).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (property.GetType() == typeof(DateTime))
                {
                    if ((DateTime)property.GetValue(nov) == DateTime.MinValue)
                    {
                        property.SetValue(nov, DateTime.MinValue.ToUniversalTime());
                    }
                }
            }
            */
            var Serialized = SerializeNovena(nov);
            WriteSetting(NOV_SETTINGS_PREFIX + nov.ID, Serialized);
        }

        private static string SerializeNovena(Novena nov)
        {
            Stream stream = new MemoryStream();
            DataContractJsonSerializer json = new DataContractJsonSerializer(nov.GetType());
            json.WriteObject(stream, nov);
            return Stream2String(stream);
        }

        private static string ReadSerializedNovenaById(string id)
        {
            return ReadSetting(NOV_SETTINGS_PREFIX + id);
        }

        private static List<string> ReadSerializedNovenas()
        {

            List<string> SerializedNovenas = new List<string>();

            foreach (string key in localSettings.Values.Keys)
            {
                if (key.StartsWith(NOV_SETTINGS_PREFIX))
                {
                   // localSettings.Values.Remove(key);
                    SerializedNovenas.Add(localSettings.Values[key] as string);
                }
            }

            return SerializedNovenas;
        }


        public static void WriteSetting(string key, string value)
        {
            localSettings.Values[key] = value;
        }
        public static string ReadSetting(string v)
        {
            return localSettings.Values[v] as string;
        }

        public static void DeleteSetting(string v)
        {
            if (localSettings.Values.ContainsKey(v))
            {
                localSettings.Values.Remove(v);
            }
        }


        public static Stream String2Stream(string str)
        {
            return new MemoryStream(Enc.GetBytes(str ?? ""));
        }

        public static string Stream2String(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

    }
}
