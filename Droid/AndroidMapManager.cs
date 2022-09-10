using Android.Content;
using Uri = Android.Net.Uri;
using Android.Provider;
using Android.Webkit;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(Lexoh.Droid.AndroidMapManager))]
namespace Lexoh.Droid
{
    public class AndroidMapManager : IUrlManager
    {
        public Task<bool> OpenUrl(string url)
        {
            bool result = false;
            try
            {
                var aUri = Uri.Parse(url);
                var intent = new Intent(Intent.ActionView, aUri);
                Forms.Context.StartActivity(intent);
                result = true;
            }
            catch (ActivityNotFoundException)
            {
                result = false;
            }
            return Task.FromResult(result);
        }

        public void ClearCache()
        {
            try
            {
                Browser.ClearSearches(MainActivity.Instance.ContentResolver);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
            try
            {
                Browser.ClearHistory(MainActivity.Instance.ContentResolver);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            try
            {
                if(Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.LollipopMr1)
                {
                    CookieManager.Instance.RemoveAllCookies(null);
                    CookieManager.Instance.Flush();
                }
                else
                {
                    CookieSyncManager cookieSyncMngr = CookieSyncManager.CreateInstance(MainActivity.Instance.ApplicationContext);
                    cookieSyncMngr.StartSync();
                    CookieManager cookieManager = CookieManager.Instance;
                    cookieManager.RemoveAllCookie();
                    cookieManager.RemoveSessionCookie();
                    cookieSyncMngr.StopSync();
                    cookieSyncMngr.Sync();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void InstallLexohRootCertificate()
        {
            
        }
    }
}