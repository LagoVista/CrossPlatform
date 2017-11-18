using LagoVista.Core.PlatformSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;

namespace LagoVista.Core.UWP.Services
{
    public class Imaging : IImaging
    {
        static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        static async Task<IRandomAccessStream> ConvertToRandomAccessStream(Stream inputStream)
        {
            var randomAccessStream = new InMemoryRandomAccessStream();
            var outputStream = randomAccessStream.GetOutputStreamAt(0);
            using (var dw = new DataWriter(outputStream))
            {
                var task = Task.Factory.StartNew(() => dw.WriteBytes(ReadFully(inputStream)));
                await task;
                await dw.StoreAsync();
                await outputStream.FlushAsync();
                return randomAccessStream;
            }
        }

        public async Task<Stream> ResizeImage(Stream sourceStream, uint maxWidth, uint maxHeight)
        {
            sourceStream.Seek(0, SeekOrigin.Begin);

            using (var randomAccessStream = await ConvertToRandomAccessStream(sourceStream))
            {
                var decoder = await BitmapDecoder.CreateAsync(randomAccessStream);

                uint height =  maxHeight;
                uint width = maxWidth;

                if (decoder.PixelWidth > decoder.PixelHeight)
                    height = Convert.ToUInt32(width * (Convert.ToDouble(decoder.PixelHeight) / Convert.ToDouble(decoder.PixelWidth)));
                else
                    width = Convert.ToUInt32(height * (Convert.ToDouble(decoder.PixelWidth) / Convert.ToDouble(decoder.PixelHeight)));

                var destinationStream = new InMemoryRandomAccessStream();

                var encoder =await BitmapEncoder.CreateForTranscodingAsync(destinationStream, decoder);
                encoder.BitmapTransform.ScaledHeight = height;
                encoder.BitmapTransform.ScaledWidth = width;

                try
                {
                    await encoder.FlushAsync();

                    return destinationStream.AsStream();
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
    }
}
