using LagoVista.Core;
using LagoVista.Core.PlatformSupport;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.XPlat.WPF.Services
{
    public class StorageService : IStorageService
    {
        Dictionary<string, string> _iotSettings = new Dictionary<string, string>();


        private Dictionary<string, string> AppSettings
        {
            get
            {
                return _iotSettings;
            }
        }

        private async Task LoadSettingsIfRequired()
        {
            throw new NotImplementedException();
        }

        private async Task SaveSettingsIfRequired()
        {
            await StoreAsync(_iotSettings, "IOTSETTINGS.json");
        }

        public async Task ClearKVP(string key)
        {
            await LoadSettingsIfRequired();

            if (AppSettings.ContainsKey(key))
                AppSettings.Remove(key);

            await SaveSettingsIfRequired();
        }

        public Task<Stream> Get(Uri rui)
        {
            throw new NotImplementedException();
        }

        public async Task<Stream> Get(Locations location, string fileName, string folderName = "")
        {
            var outputPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var fullFileName = Path.Combine(outputPath, fileName);
            if (System.IO.File.Exists(outputPath))
            {
                return System.IO.File.Open(fullFileName, FileMode.Open);
            }
            else
            {
                return System.IO.File.Open(fullFileName, FileMode.Create);
            }
            
        }

        public async Task<TObject> GetAsync<TObject>(string fileName) where TObject : class
        {
            using (var inputStream = await Get(Locations.Local, fileName))
            {
                if (inputStream == null)
                {
                    return null;
                }
                else
                {
                    using (var rdr = new StreamReader(inputStream))
                    {
                        var json = rdr.ReadToEnd();
                        return JsonConvert.DeserializeObject<TObject>(json);
                    }
                }
            }
        }

        public async Task<T> GetKVPAsync<T>(string key, T defaultValue = default(T)) where T : class
        {
            await LoadSettingsIfRequired();

            if (AppSettings.ContainsKey(key))
            {
                var json = AppSettings[key] as string;
                if (!String.IsNullOrEmpty(json))
                {
                    return JsonConvert.DeserializeObject<T>(json);
                }
            }

            return defaultValue;
        }

        public async Task<bool> HasKVPAsync(string key)
        {
            await LoadSettingsIfRequired();

            return AppSettings.ContainsKey(key);
        }

        public async Task<Uri> StoreAsync(Stream stream, Locations location, string fileName, string folderName = "")
        {
            var outputPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var fullOutputFile = Path.Combine(outputPath, fileName);

            using (var fileStream = new FileStream(fullOutputFile, FileMode.OpenOrCreate))
            using (var streamWriter = new StreamWriter(fileStream))
            {
            }

            return new Uri(fullOutputFile);
        }

        //TODO: Need to figure out why we are returning a string.
        public Task<string> StoreAsync<TObject>(TObject instance, string fileName) where TObject : class
        {
            var json = JsonConvert.SerializeObject(instance);
            var outputPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var fullFileName = Path.Combine(outputPath, fileName);
            try
            {
                System.IO.File.Delete(fileName);

            }
            catch (Exception)
            {
                Debug.WriteLine("Could not delete file, may not exist.");
            }

            try
            {
                System.IO.File.WriteAllText(fullFileName, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("EXCEPTION SAVING SETTINGS: " + ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }

            return Task.FromResult(fullFileName);
        }

        public async Task StoreKVP<T>(string key, T value) where T : class
        {
            try
            {
                await LoadSettingsIfRequired();

                AppSettings[key] = JsonConvert.SerializeObject(value);

                await SaveSettingsIfRequired();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("EXCEPTION SAVING SETTINGS: " + ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }
        }

        public Task<string> StoreAsync(Stream stream, string fileName, Locations location = Locations.Default, string folder = "")
        {
            throw new NotImplementedException();
        }

        public Task<Stream> Get(string fileName, Locations location = Locations.Default, string folder = "")
        {
            throw new NotImplementedException();
        }


        public Task<string> ReadAllTextAsync(string fileName)
        {
            return GetAsync<string>(fileName);
        }

        public async Task<string> WriteAllTextAsync(string fileName, string text)
        {
            await StoreAsync(text, fileName);
            return text;
        }

        public async Task<List<string>> ReadAllLinesAsync(string fileName)
        {
            var output = await ReadAllTextAsync(fileName);
            return output.Split('\r').ToList();
        }

        public async Task<string> WriteAllLinesAsync(string fileName, List<string> text)
        {
            var bldr = new StringBuilder();
            foreach (var line in text)
            {
                bldr.AppendLine(line);
            }

            await WriteAllTextAsync(fileName, bldr.ToString());

            return bldr.ToString();
        }

        public Task<byte[]> ReadAllBytesAsync(string fileName)
        {
            throw new NotImplementedException();
        }

        public Task<string> WriteAllBytesAsync(string fileName, byte[] buffer)
        {
            throw new NotImplementedException();
        }
    }
}
