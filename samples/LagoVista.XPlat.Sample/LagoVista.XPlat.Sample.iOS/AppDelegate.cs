using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;
using LagoVista.XPlat.iOS;
using LagoVista.Core.Models;

namespace LagoVista.XPlat.Sample.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();

            var version = NSBundle.MainBundle.InfoDictionary[new NSString("CFBundleVersion")].ToString();
            Console.WriteLine($"NSLog Version {version}");

            UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.LightContent;

            app.StatusBarHidden = false;

            Startup.Init(app, "dontcare");

            var formsApp = new App();

            LoadApplication(formsApp);
            
            return base.FinishedLaunching(app, options);
        }
    }
}
