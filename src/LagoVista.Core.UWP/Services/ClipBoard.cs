using LagoVista.Client.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace LagoVista.Core.UWP.Services
{
    public class ClipBoard : IClipBoard
    {
        public void Copy(string content)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(content);
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            Clipboard.SetContent(dataPackage);
        }
    }
}
