using LagoVista.Core.PlatformSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.XPlat.Maui.Services
{
    public class PopupService : IPopupServices
    {
        public Task<bool> ConfirmAsync(string title, string prompt)
        {
            throw new NotImplementedException();
        }

        public Task<double?> PromptForDoubleAsync(string label, double? defaultvalue = null, string help = "", bool isRequired = false)
        {
            throw new NotImplementedException();
        }

        public Task<int?> PromptForIntAsync(string label, int? defaultvalue = null, string help = "", bool isRequired = false)
        {
            throw new NotImplementedException();
        }

        public Task<string> PromptForStringAsync(string label, string defaultvalue = null, string help = "", bool isRequired = false)
        {
            throw new NotImplementedException();
        }

        public Task ShowAsync(string title, string message)
        {
            throw new NotImplementedException();
        }

        public Task ShowAsync(string message)
        {
            throw new NotImplementedException();
        }

        public Task<string> ShowOpenFileAsync(string fileMask = "")
        {
            throw new NotImplementedException();
        }

        public Task<string> ShowSaveFileAsync(string fileMask = "", string defaultFileName = "")
        {
            throw new NotImplementedException();
        }
    }
}
