using LagoVista.Core.PlatformSupport;
using LagoVista.Core;
using System;
using System.Threading.Tasks;
using System.IO;
using Windows.Storage;
using Windows.Foundation.Collections;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Collections.Generic;

namespace LagoVista.Core.UWP.Services
{
    public class StorageService : IStorageService
    {
        PropertySet _iotSettings;

        private IPropertySet AppSettings
        {
            get
            {
                if (IsIoT)
                {
                    return _iotSettings;
                }
                else
                    return Windows.Storage.ApplicationData.Current.RoamingSettings.Values;
            }
        }

        private async Task LoadSettingsIfRequired()
        {
            if (_iotSettings == null && IsIoT)
            {
                _iotSettings = await GetAsync<PropertySet>("IOTSETTINGS.json");
                if (_iotSettings == null)
                {
                    _iotSettings = new PropertySet();
                }
            }
        }

        private bool IsIoT
        {
            get { return Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.IoT"; }
        }

        private async Task SaveSettingsIfRequired()
        {
            if (_iotSettings != null)
            {
                await StoreAsync(_iotSettings, "IOTSETTINGS.json");
            }
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
            try
            {
                IStorageFolder folder = null;

                switch (location)
                {
                    case Locations.Roaming: folder = Windows.Storage.ApplicationData.Current.RoamingFolder; break;
                    case Locations.Local: folder = Windows.Storage.ApplicationData.Current.LocalFolder; break;
                    case Locations.Temp: folder = Windows.Storage.ApplicationData.Current.TemporaryFolder; break;
                }

                var storageFile = await folder.GetFileAsync(fileName);

                return await storageFile.OpenStreamForReadAsync();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<TObject> GetAsync<TObject>(string fileName) where TObject : class
        {
            var location = IsIoT ? Locations.Local : Locations.Roaming;
            using (var inputStream = await Get(location, fileName))
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
            StorageFolder folder = null;

            switch (location)
            {
                case Locations.Roaming: folder = Windows.Storage.ApplicationData.Current.RoamingFolder; break;
                case Locations.Local: folder = Windows.Storage.ApplicationData.Current.LocalFolder; break;
                case Locations.Temp: folder = Windows.Storage.ApplicationData.Current.TemporaryFolder; break;
            }

            if (!String.IsNullOrEmpty(folderName))
            {
                StorageFolder childFolder;
                try
                {
                    childFolder = await folder.GetFolderAsync(folderName);
                }
                catch (Exception)
                {
                    childFolder = await folder.CreateFolderAsync(folderName);
                }

                folder = childFolder;
            }

            var storageItem = await folder.TryGetItemAsync(fileName);
            if (storageItem != null)
                await storageItem.DeleteAsync();

            var storageFile = await folder.CreateFileAsync(fileName);

            using (var outputStream = await storageFile.OpenStreamForWriteAsync())
            {
                stream.CopyTo(outputStream);
            }

            return new Uri(storageFile.Path);
        }

        //TODO: Need to figure out why we are returning a string.
        public async Task<string> StoreAsync<TObject>(TObject instance, string fileName) where TObject : class
        {
            var json = JsonConvert.SerializeObject(instance);
            var outputFolder = IsIoT ? Windows.Storage.ApplicationData.Current.LocalFolder : Windows.Storage.ApplicationData.Current.RoamingFolder;

            try
            {
                var file = await outputFolder.GetFileAsync(fileName);
                await file.DeleteAsync();
                
            }
            catch (Exception)
            {
                Debug.WriteLine("Could not delete file, may not exist.");
            }

            try
            {
                var outputFile = await outputFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                using (var outputStream = await outputFile.OpenStreamForWriteAsync())
                {
                    var buffer = json.ToUTF8ByteArray();
                    await outputStream.WriteAsync(buffer, 0, buffer.Length);
                    return outputFile.Name;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("EXCEPTION SAVING SETTINGS: " + ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }

            return null;
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
            throw new NotImplementedException();
        }

        public Task<string> WriteAllTextAsync(string fileName, string text)
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> ReadAllLinesAsync(string fileName)
        {
            throw new NotImplementedException();
        }

        public Task<string> WriteAllLinesAsync(string fileName, List<string> text)
        {
            throw new NotImplementedException();
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
