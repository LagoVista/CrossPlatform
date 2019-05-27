using Android.Content;
using LagoVista.Client.Core.Interfaces;

namespace LagoVista.XPlat.Droid.Services
{
    public class ClipBoard : IClipBoard
    {
        public void Copy(string content)
        {
            var clipboardManager = (ClipboardManager)Android.App.Application.Context.GetSystemService(Context.ClipboardService);
            ClipData clip = ClipData.NewPlainText("Android Clipboard", content);
            clipboardManager.PrimaryClip = clip;
        }
    }
}