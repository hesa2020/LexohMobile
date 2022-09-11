using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using System.Threading.Tasks;
using Xamarin.Forms.Platform.Android;

namespace Lexoh.Droid
{
    [Activity(Label = "Lexoh", Icon = "@drawable/icon", Theme = "@style/MainTheme.Splash", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static MainActivity Instance = null;

        private static bool ResetToken = false;//DEBUG ONLY<-----

        protected override void OnCreate(Bundle bundle)
        {
            Instance = this;
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            
            base.SetTheme(Resource.Style.MainTheme);
            
            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);

            LoadApplication(new App());
            
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

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);            
            ActivityResultCallbackRegistry.InvokeCallback(requestCode, resultCode, data);
        }
    }
}
