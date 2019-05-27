using LagoVista.Client.Core.Interfaces;
using UIKit;

namespace LagoVista.XPlat.iOS.Services
{
    public class ClipBoard : IClipBoard
    {
        public void Copy(string content)
        {
            UIPasteboard clipboard = UIPasteboard.General;
            clipboard.String = content;
        }
    }
}