using LagoVista.XPlat.Core.Services;
using System.IO;
using System.Threading.Tasks;

namespace LagoVista.XPlat.Droid.Services
{
    public class StorageService : StorageServiceBase
    {
        protected override Task<Stream> GetStreamAsync(string file)
        {
            var data = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            var fullFileName = Path.Combine(data, file);

            if (System.IO.File.Exists(fullFileName))
            {
                return Task.FromResult(System.IO.File.Open(fullFileName, FileMode.OpenOrCreate) as Stream);
            }
            else
            {
                return null;
            }
        }

        protected override Task PutAllTextAsync(string fileName, string contents)
        {
            var tcs = new System.Threading.Tasks.TaskCompletionSource<object>();
            Task.Run(() =>
            {
                var data = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
                if (!System.IO.Directory.Exists(data))
                {
                    System.IO.Directory.CreateDirectory(data);
                }

                var fullFileName = Path.Combine(data, fileName);

                System.IO.File.WriteAllText(fullFileName, contents);
                tcs.SetResult(0);
            });
            return tcs.Task;
        }

        protected override Task PutStreamAsync(string fileName, Stream stream)
        {
            var tcs = new System.Threading.Tasks.TaskCompletionSource<object>();
            Task.Run(() =>
            {
                var data = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
                if (!System.IO.Directory.Exists(data))
                {
                    System.IO.Directory.CreateDirectory(data);
                }

                var fullFileName = Path.Combine(data, fileName);

                using (var outputStream = System.IO.File.Open(fullFileName, FileMode.OpenOrCreate))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(outputStream);
                    tcs.SetResult(0);
                }
            });

            return tcs.Task;
        }

        protected override Task<string> ReadAllTextAsync(string fileName)
        {
            var data = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            var fullFileName = Path.Combine(data, fileName);

            if (System.IO.File.Exists(fullFileName))
            {

                return Task.FromResult(System.IO.File.ReadAllText(fullFileName));
            }
            else
            {
                return Task.FromResult("");
            }
        }
    }
}