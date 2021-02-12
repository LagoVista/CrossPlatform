using Android.App;
using Android.Content.PM;
using Android.OS;
using LagoVista.XPlat.Sample;

namespace LagoVista.Simulator.Droid
{
    [Activity(Label = "NuvIoT Device Manager", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public const string MOBILE_CENTER_KEY = "1211d9f6-eed2-44a1-bfac-e7b080e000c1";

        protected override void OnCreate(Bundle bundle)
        { 
            //https://play.google.com/apps/publish/?dev_acc=12258406958683843289
            LagoVista.XPlat.Droid.Startup.Init(BaseContext, MOBILE_CENTER_KEY);

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());
        }
    }
}

