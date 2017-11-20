using Android.App;
using Android.Widget;
using Android.OS;

namespace LagoVista.XPlat.Sample.Droid
{
    [Activity(Label = "LagoVista.XPlat.Sample.Droid", MainLauncher = true)]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
        }
    }
}

