using Foundation;
using LagoVista.Core.Models;
using System;
using UIKit;

namespace LagoVista.PlatformManager.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
    [Register("AppDelegate")]
    public class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        // class-level declarations

        private const string MOBILE_CENTER_KEY = "e3d48293-22fe-4bfb-9a7d-9fa41112c23f";

        public override UIWindow Window
        {
            get;
            set;
        }

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            // create a new window instance based on the screen size
            LagoVista.XPlat.iOS.Startup.Init(app, MOBILE_CENTER_KEY);

            UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.LightContent;
            app.StatusBarHidden = false;

            global::Xamarin.Forms.Forms.Init();
            var version = NSBundle.MainBundle.InfoDictionary[new NSString("CFBundleVersion")].ToString();
            Console.WriteLine($"NSLog Version {version}");

            var versionParts = version.Split('.');
            var versionInfo = new VersionInfo();
            if (versionParts.Length != 4)
            {
                throw new Exception("Expecting CFBundleVersion to be a version consisting of four parts 1.0.218.1231 [Major].[Minor].[Build].[Revision]");
            }

            var formsApp = new App();

            /* if this blows up our build version is borked...make sure all version numbers are intergers like 1.0.218.1231 */
            versionInfo.Major = Convert.ToInt32(versionParts[0]);
            versionInfo.Minor = Convert.ToInt32(versionParts[1]);
            versionInfo.Build = Convert.ToInt32(versionParts[2]);
            versionInfo.Revision = Convert.ToInt32(versionParts[3]);
            formsApp.SetVersionInfo(versionInfo);

            LoadApplication(formsApp);


            return base.FinishedLaunching(app, options);
        }

        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            App.Instance.HandleURIActivation(new Uri(url.AbsoluteString));
            return true;
        }


    }
}


