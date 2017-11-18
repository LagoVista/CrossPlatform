using LagoVista.Core.Interfaces;
using LagoVista.Core.IOC;
using LagoVista.Core.PlatformSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace LagoVista.XPlat.Core.Views
{
    public class LagoVistaNavigationPage : NavigationPage
    {
        public LagoVistaNavigationPage(Page root) : base(root)
        {
            BarBackgroundColor = AppStyle.TitleBarBackground.ToXamFormsColor();
            BarTextColor = AppStyle.TitleBarText.ToXamFormsColor();
            BackgroundColor = AppStyle.PageBackground.ToXamFormsColor();
        }

        public void HandleURIActivation(Uri uri)
        {
            var logger = SLWIOC.Get<ILogger>();
            if (this.CurrentPage == null)
            {
                logger.AddCustomEvent(LogLevel.Error, "LagoVistaNavigationPage_HandleURIActivation", "Main Page Null");
            }
            else
            {
                var contentPage = this.CurrentPage as LagoVistaContentPage;
                if (contentPage != null)
                {
                    contentPage.HandleURIActivation(uri);
                }
                else
                {
                    logger.AddCustomEvent(LogLevel.Error, "App_OnActivated", "EventArgs Not ProtocolActivatedEventArgs", new System.Collections.Generic.KeyValuePair<string, string>("type", this.CurrentPage.GetType().Name));
                }
            }
        }

        private IAppStyle AppStyle { get { return SLWIOC.Get<IAppStyle>(); } }
    }
}
