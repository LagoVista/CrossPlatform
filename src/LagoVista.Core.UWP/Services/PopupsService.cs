using LagoVista.Core.PlatformSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace LagoVista.Core.UWP.Services
{
    public class PopupsService : IPopupServices
    {
        public async Task<bool> ConfirmAsync(string title, string prompt)
        {
            try
            {
                var result = false;
                var dlg = new MessageDialog(prompt, title);
                dlg.Commands.Add(new UICommand("Yes") { Invoked = (IUICommand) => { result = true; } });
                dlg.Commands.Add(new UICommand("No") { Invoked = (IUICommand) => { result = false; } });

                await dlg.ShowAsync();
                return result;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Task<double?> PromptForDoubleAsync(string label, double? defaultvalue = default(double?), string help = "", bool isRequired = false)
        {
            throw new NotImplementedException();
        }

        public Task<int?> PromptForIntAsync(string label, int? defaultvalue = default(int?), string help = "", bool isRequired = false)
        {
            throw new NotImplementedException();
        }

        public async Task<string> PromptForStringAsync(string label, string defaultvalue = null, string help = "", bool isRequired = false)
        {
            var cntDlg = new ContentDialog1(label);
            if (!String.IsNullOrEmpty(defaultvalue))
            {
                cntDlg.Text = defaultvalue;
            }

            cntDlg.Required = isRequired;

            var result = await cntDlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                return cntDlg.Text;
            }
            else
            {
                return null;
            }
        }

        public async Task ShowAsync(string message)
        {
            try
            {
                var dlg = new MessageDialog(message);
                await dlg.ShowAsync();
            }
            catch (Exception)
            {

            }
        }

        public async Task ShowAsync(string title, string message)
        {
            try
            {
                var dlg = new MessageDialog(message, title);
                await dlg.ShowAsync();
            }
            catch (Exception)
            {

            }
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

    public sealed partial class ContentDialog1 : ContentDialog
    {
        TextBox _txtBox;
        TextBlock _requiredMessage;

        public ContentDialog1(string label)
        {
            var grd = new Grid();
            grd.RowDefinitions.Add(new RowDefinition() { Height = new Windows.UI.Xaml.GridLength(1, Windows.UI.Xaml.GridUnitType.Star) });
            grd.RowDefinitions.Add(new RowDefinition() { Height = new Windows.UI.Xaml.GridLength(1, Windows.UI.Xaml.GridUnitType.Auto) });
            grd.RowDefinitions.Add(new RowDefinition() { Height = new Windows.UI.Xaml.GridLength(1, Windows.UI.Xaml.GridUnitType.Auto) });

            _requiredMessage = new TextBlock();
            _requiredMessage.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Red);
            _requiredMessage.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            _requiredMessage.SetValue(Grid.RowProperty, 2);
            _requiredMessage.Text = "* Required";

            this.Title = label;

            _txtBox = new TextBox();
            _txtBox.SetValue(Grid.RowProperty, 1);

            grd.Children.Add(_txtBox);
            grd.Children.Add(_requiredMessage);

            PrimaryButtonText = "OK";
            SecondaryButtonText = "Cancel";
            PrimaryButtonClick += ContentDialog1_PrimaryButtonClick;
            Content = grd;
        }

        private void ContentDialog1_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (String.IsNullOrEmpty(Text) && Required)
            {
                _requiredMessage.Visibility = Windows.UI.Xaml.Visibility.Visible;
                args.Cancel = true;
            }
        }

        public bool Required { get; set; }


        public string Text
        {
            get { return _txtBox.Text; }
            set { _txtBox.Text = value; ; }
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
