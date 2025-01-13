using LagoVista.Core.PlatformSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.XPlat.Maui.Services
{
    internal class StorageService : IStorageService
    {
        public Task ClearAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task ClearKVP(string key)
        {
            throw new NotImplementedException();
        }

        public Task<Stream> Get(Uri rui)
        {
            throw new NotImplementedException();
        }

        public Task<Stream> Get(string fileName, Locations location = Locations.Default, string folder = "")
        {
            throw new NotImplementedException();
        }

        public Task<TObject> GetAsync<TObject>(string fileName) where TObject : class
        {
            throw new NotImplementedException();
        }

        public Task<T> GetKVPAsync<T>(string key, T defaultValue = null) where T : class
        {
            throw new NotImplementedException();
        }

        public Task<bool> HasKVPAsync(string key)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> ReadAllBytesAsync(string fileName)
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> ReadAllLinesAsync(string fileName)
        {
            throw new NotImplementedException();
        }

        public Task<string> ReadAllTextAsync(string fileName)
        {
            throw new NotImplementedException();
        }

        public Task<string> StoreAsync(Stream stream, string fileName, Locations location = Locations.Default, string folder = "")
        {
            throw new NotImplementedException();
        }

        public Task<string> StoreAsync<TObject>(TObject instance, string fileName) where TObject : class
        {
            throw new NotImplementedException();
        }

        public Task StoreKVP<T>(string key, T value) where T : class
        {
            throw new NotImplementedException();
        }

        public Task<string> WriteAllBytesAsync(string fileName, byte[] buffer)
        {
            throw new NotImplementedException();
        }

        public Task<string> WriteAllLinesAsync(string fileName, List<string> text)
        {
            throw new NotImplementedException();
        }

        public Task<string> WriteAllTextAsync(string fileName, string text)
        {
            throw new NotImplementedException();
        }
    }
}
