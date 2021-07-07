using Android.App;
using Android.OS;
using Android.Content.PM;
using static LagoVista.PlatformManager.Droid.Resource;
using LagoVista.Core.Models;
using System;

namespace LagoVista.PlatformManager.Droid
{
    [Activity(Label = "Platform Manager", Icon = "@mipmap/icon", Theme = "@style/MainTheme", ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public const string MOBILE_CENTER_KEY = "1276e0bd-4c34-4b88-81d1-61c999e0dd7e";

        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Layout.Tabbar;
            ToolbarResource = Layout.Toolbar;

            //https://console.firebase.google.com

            //https://play.google.com/apps/publish/?dev_acc=12258406958683843289
            LagoVista.XPlat.Droid.Startup.Init(BaseContext, MOBILE_CENTER_KEY);

            base.OnCreate(bundle);

            var packageInfo = PackageManager.GetPackageInfo(PackageName, 0);

            var versionParts = packageInfo.VersionName.Split('.');
            var versionInfo = new VersionInfo();
            if (versionParts.Length != 4)
            {
                throw new Exception("Expecting android:versionName in AndroidManifest.xml to be a version conisting of four parts 1.0.218.1231 [Major].[Minor].[Build].[Revision]");
            }

            /* if this blows up our versionName in AndroidManaifest.xml is borked...make sure all version numbers are intergers like 1.0.218.1231 */
            versionInfo.Major = Convert.ToInt32(versionParts[0]);
            versionInfo.Minor = Convert.ToInt32(versionParts[1]);
            versionInfo.Build = Convert.ToInt32(versionParts[2]);
            versionInfo.Revision = Convert.ToInt32(versionParts[3]);

            global::Xamarin.Forms.Forms.Init(this, bundle);

            var app = new App();
            app.SetVersionInfo(versionInfo);
            
            LoadApplication(app);
        }
    }
}

