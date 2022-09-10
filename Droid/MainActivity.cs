using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Common;
using Android.OS;
using Firebase;
using Firebase.Iid;
using System.Threading.Tasks;

namespace Lexoh.Droid
{
    [Activity(Label = "Lexoh", Icon = "@drawable/icon", Theme = "@style/MyTheme.Splash", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static MainActivity Instance = null;

        private static bool ResetToken = false;//DEBUG ONLY<-----

        protected override void OnCreate(Bundle bundle)
        {
            Instance = this;
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            
            base.SetTheme(Resource.Style.MyTheme);
            
            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);

            LoadApplication(new App());
            //try
            //{
            //    var options = new FirebaseOptions.Builder()
            //         .SetApplicationId("1:1083453337327:android:c1ae9ba768f16c3a")
            //         .SetApiKey("AIzaSyCPq7SL0azkjIWRboW39uM2UAaTl7gpOVU")
            //         .SetDatabaseUrl("https://api-8301870164865522546-387983.firebaseio.com")
            //         .SetGcmSenderId("1083453337327")
            //         .SetStorageBucket("api-8301870164865522546-387983.appspot.com").Build();

            //    FirebaseApp app = FirebaseApp.InitializeApp(Application.Context, options);
            //}
            //catch (System.Exception ex)
            //{
            //    System.Console.WriteLine(ex);
            //}

            // Generate token in background thread
            //Task.Factory.StartNew(() => {
            //    var token = FirebaseInstanceId.Instance.GetToken("1083453337327", "FCM");
            //    if(ResetToken)
            //    {
            //        FirebaseInstanceId.Instance.DeleteInstanceId();
            //        var token2 = FirebaseInstanceId.Instance.GetToken("1083453337327", "FCM");
            //        System.Console.WriteLine("Firebase Token : " + token2);
            //    }
            //    else                
            //        System.Console.WriteLine("Firebase Token : " + token);
            //});

            if(IsPlayServicesAvailable())
            {
                
            }
            
            string type = Intent.GetStringExtra("type") ?? "";
            string type_id = Intent.GetStringExtra("type_id") ?? "";

            if(!string.IsNullOrEmpty(type) && !string.IsNullOrEmpty(type_id) && LexohPage.Instance != null)
            {
                LexohPage.Instance.OpenNotification(type, type_id);
            }
        }

        protected override void OnResume()
        {
            base.OnResume();//Doesnt seems to be needed but i am leaving this here in case its needed for a specific device...
            string type = Intent.GetStringExtra("type") ?? "";
            string type_id = Intent.GetStringExtra("type_id") ?? "";
            
            if (!string.IsNullOrEmpty(type) && !string.IsNullOrEmpty(type_id) && LexohPage.Instance != null)
            {
                LexohPage.Instance.OpenNotification(type, type_id);
            }
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            string type = intent.GetStringExtra("type") ?? "";
            string type_id = intent.GetStringExtra("type_id") ?? "";

            if (!string.IsNullOrEmpty(type) && !string.IsNullOrEmpty(type_id) && LexohPage.Instance != null)
            {
                LexohPage.Instance.OpenNotification(type, type_id);
            }
        }

		protected override void OnDestroy()
		{
            System.Console.WriteLine("DEstroy!!!");
            UrlManager.Manager.ClearCache();
            base.OnDestroy();
		}

        public bool IsPlayServicesAvailable()
        {
            int resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
            if (resultCode != ConnectionResult.Success)
            {
                if (GoogleApiAvailability.Instance.IsUserResolvableError(resultCode))
                    System.Console.WriteLine(GoogleApiAvailability.Instance.GetErrorString(resultCode));
                else
                {
                    System.Console.WriteLine("This device is not supported");
                    Finish();
                }
                return false;
            }
            else
            {
                System.Console.WriteLine("Google Play Services is available.");
                return true;
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);            
            ActivityResultCallbackRegistry.InvokeCallback(requestCode, resultCode, data);
        }
    }
}
