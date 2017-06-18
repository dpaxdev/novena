using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Windows.Storage;

namespace Novena_Reminder.Model
{
    static class Storage
    {


        static StorageFolder folder = ApplicationData.Current.LocalFolder;
        static string DataFileName = "Data.txt";

        public static ObservableCollection<Novena> GetCollection()
        {
            ObservableCollection<Novena> temp = new ObservableCollection<Novena>();

            StorageFile file = GetFile(folder, DataFileName);
            if (file != null)
            {             
                using (Stream stream = Task.Run(() => file.OpenStreamForReadAsync()).Result)
                {
                    if (stream.Length == 0) return temp;
                    DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(ObservableCollection<Novena>));
                    temp = (ObservableCollection<Novena>)json.ReadObject(stream);
                }
            }

            return temp;
        }

        private static async Task<StorageFile> GetFileAsyncWrapper(StorageFolder folder, string DataFileName)
        {

            StorageFile result;

            var exists = await folder.TryGetItemAsync(DataFileName);
            if (exists != null)
            {
                result = await folder.GetFileAsync(DataFileName);
            }
            else
            {
                result = null;
            }
            return result;
        }

        public static StorageFile GetFile(StorageFolder folder, string DataFileName)
        {

            var result = Task.Run(() => GetFileAsyncWrapper(folder, DataFileName)).Result;
            return result;

        }

        public static Novena GetNovenaById(Guid id)
        {
            var Novenas = GetCollection();
            if (Novenas != null && Novenas.Count > 0)
            {
                foreach (Novena nov in Novenas)
                {
                    if (nov.ID == id)
                    {
                        return nov;
                    }
                }
            }
            return null;
        }

        public static bool SaveCollection(ObservableCollection<Novena> Novenas)
        {
            StorageFile file = CreateFile(folder, DataFileName);

            if (Novenas.Count == 0) return false;

            using (Stream stream = file.OpenStreamForWriteAsync().Result)
            {
                try
                {
                    DataContractJsonSerializer json = new DataContractJsonSerializer(Novenas.GetType());
                    json.WriteObject(stream, Novenas);
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }

        private static StorageFile CreateFile(StorageFolder folder, string dataFileName)
        {
            return CreateFileAsyncWrapper(folder, dataFileName).Result;
        }

        private static async Task<StorageFile> CreateFileAsyncWrapper(StorageFolder folder, string dataFileName)
        {
            return await folder.CreateFileAsync(DataFileName, CreationCollisionOption.ReplaceExisting);
        }


    }
}
