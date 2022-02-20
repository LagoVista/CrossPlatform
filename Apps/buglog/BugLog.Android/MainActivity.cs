using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using LagoVista.XPlat.Core.Services;
using AndroidX.Core.App;
using Android;
using LagoVista.Client.Core.Models;
using Android.Bluetooth;
using Android.Content;

namespace SeaWolf.Droid
{
    [Activity(Label = "SeaWolf", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private const int REQUEST_ENABLE_BT = 900;
        private const int REQUEST_PERMISSIONS = 901;

        private bool _hasBluetoothPermissions;
        private bool _hasBluetoothAdminPermissions;
        private bool _hasCameraPermissions;
        private bool _hasNfc;
        private bool _hasNfcTransactionEvent;
        private bool _hasBindNfcEvent;
        private bool _hasCourseLocation;
        private bool _hasFineLocation;

        public void verifyAppPermissions()
        {
            _hasBluetoothPermissions = ActivityCompat.CheckSelfPermission(this, Manifest.Permission.Bluetooth) == Permission.Granted;
            _hasBluetoothAdminPermissions = ActivityCompat.CheckSelfPermission(this, Manifest.Permission.BluetoothAdmin) == Permission.Granted;

            _hasCameraPermissions = ActivityCompat.CheckSelfPermission(this, Manifest.Permission.Camera) == Permission.Granted;

            _hasNfc = ActivityCompat.CheckSelfPermission(this, Manifest.Permission.Nfc) == Permission.Granted;
            _hasNfcTransactionEvent = ActivityCompat.CheckSelfPermission(this, Manifest.Permission.NfcTransactionEvent) == Permission.Granted;
            _hasBindNfcEvent = ActivityCompat.CheckSelfPermission(this, Manifest.Permission.BindNfcService) == Permission.Granted;

            _hasFineLocation = ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) == Permission.Granted;
            _hasCourseLocation = ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) == Permission.Granted;

            if (!_hasBindNfcEvent || !_hasBluetoothAdminPermissions || !_hasBluetoothPermissions || !_hasCameraPermissions ||
                !_hasCourseLocation || !_hasFineLocation || !_hasNfc || !_hasNfcTransactionEvent)
            {
                var permissions = new string[]
                {
                    Manifest.Permission.Nfc,
                    Manifest.Permission.BindNfcService,
                    Manifest.Permission.BindNfcService,
                    Manifest.Permission.Bluetooth,
                    Manifest.Permission.BluetoothAdmin,
                    Manifest.Permission.AccessCoarseLocation,
                    Manifest.Permission.AccessFineLocation,
                    Manifest.Permission.Camera,
                };

                // We don't have permission so prompt the user
                ActivityCompat.RequestPermissions(
                        this,
                        permissions,
                        REQUEST_PERMISSIONS
                );
            }

            if (_hasBluetoothAdminPermissions && _hasBluetoothPermissions) AppPermissions.Instance.SetGranted(AppPermissions.PermissionType.Bluetooth);
            if (_hasBindNfcEvent && _hasNfc && _hasNfcTransactionEvent) AppPermissions.Instance.SetGranted(AppPermissions.PermissionType.NFC);
            if (_hasCameraPermissions) AppPermissions.Instance.SetGranted(AppPermissions.PermissionType.Camera);
            if (_hasFineLocation) AppPermissions.Instance.SetGranted(AppPermissions.PermissionType.FineLocation);
            if (_hasCourseLocation) AppPermissions.Instance.SetGranted(AppPermissions.PermissionType.CourseLocation);

            if (AppPermissions.Instance.HasPermission(AppPermissions.PermissionType.Bluetooth))
            {
                var enableBtIntent = new Intent(BluetoothAdapter.ActionRequestEnable);
                StartActivityForResult(enableBtIntent, REQUEST_ENABLE_BT);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            switch (requestCode)
            {
                case REQUEST_PERMISSIONS:
                    {
                        for (int idx = 0; idx < permissions.Length; ++idx)
                        {
                            if (permissions[idx] == Manifest.Permission.Bluetooth && grantResults[idx] == Permission.Granted) _hasBluetoothPermissions = true;
                            if (permissions[idx] == Manifest.Permission.BluetoothAdmin && grantResults[idx] == Permission.Granted) _hasBluetoothAdminPermissions = true;
                            if (permissions[idx] == Manifest.Permission.Camera && grantResults[idx] == Permission.Granted) _hasCameraPermissions = true;
                            if (permissions[idx] == Manifest.Permission.Nfc && grantResults[idx] == Permission.Granted) _hasNfc = true;
                            if (permissions[idx] == Manifest.Permission.NfcTransactionEvent && grantResults[idx] == Permission.Granted) _hasNfcTransactionEvent = true;
                            if (permissions[idx] == Manifest.Permission.BindNfcService && grantResults[idx] == Permission.Granted) _hasBindNfcEvent = true;
                            if (permissions[idx] == Manifest.Permission.AccessFineLocation && grantResults[idx] == Permission.Granted) _hasCourseLocation = true;
                            if (permissions[idx] == Manifest.Permission.AccessCoarseLocation && grantResults[idx] == Permission.Granted) _hasFineLocation = true;
                        }
                    }
                    break;
            };

            if (_hasBluetoothAdminPermissions && _hasBluetoothPermissions) AppPermissions.Instance.SetGranted(AppPermissions.PermissionType.Bluetooth);
            if (_hasBindNfcEvent && _hasNfc && _hasNfcTransactionEvent) AppPermissions.Instance.SetGranted(AppPermissions.PermissionType.NFC);
            if (_hasCameraPermissions) AppPermissions.Instance.SetGranted(AppPermissions.PermissionType.Camera);
            if (_hasFineLocation) AppPermissions.Instance.SetGranted(AppPermissions.PermissionType.FineLocation);
            if (_hasCourseLocation) AppPermissions.Instance.SetGranted(AppPermissions.PermissionType.CourseLocation);

            if (!BluetoothAdapter.DefaultAdapter.IsEnabled && AppPermissions.Instance.HasPermission(AppPermissions.PermissionType.Bluetooth))
            {
                var enableBtIntent = new Intent(BluetoothAdapter.ActionRequestEnable);
                StartActivityForResult(enableBtIntent, REQUEST_ENABLE_BT);
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            DeviceInfo.Register();
            LagoVista.XPlat.Droid.Startup.Init(this.ApplicationContext, "0d1c2f7d-9c09-41e0-9d78-e0334f206ac2");

            LoadApplication(new App());

            verifyAppPermissions();
        }  
    }
}