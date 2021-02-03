using LagoVista.Core.PlatformSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LagoVista.AppLoader.Services
{
    public class PopupService : IPopupServices
    {
        public Task<bool> ConfirmAsync(string title, string prompt)
        {
            return Task.FromResult(MessageBox.Show(prompt, title, MessageBoxButton.YesNo) == MessageBoxResult.Yes);
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
            MessageBox.Show(message, title);
            return Task.CompletedTask;
        }

        public Task ShowAsync(string message)
        {
            MessageBox.Show(message);
            return Task.CompletedTask;
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
