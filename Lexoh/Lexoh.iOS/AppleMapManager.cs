using Foundation;
using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using UIKit;
using WebKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(Lexoh.iOS.AppleMapManager))]
namespace Lexoh.iOS
{
    public class AppleMapManager : IUrlManager
    {
        public Task<bool> OpenUrl(string url)
        {
            var canOpen = UIApplication.SharedApplication.CanOpenUrl(new NSUrl(url));
            if (!canOpen)
                return Task.FromResult(false);
            return Task.FromResult(UIApplication.SharedApplication.OpenUrl(new NSUrl(url)));
        }
        
        public void ClearCache()
        {
            try
            {
                NSUrlCache.SharedCache.RemoveAllCachedResponses();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
            try
            {
                var websiteDataTypes = new NSSet<NSString>(new[]
                {
                    //Choose which ones you want to remove
                    WKWebsiteDataType.Cookies,
                    WKWebsiteDataType.DiskCache,
                    WKWebsiteDataType.IndexedDBDatabases,
                    WKWebsiteDataType.LocalStorage,
                    WKWebsiteDataType.MemoryCache,
                    WKWebsiteDataType.OfflineWebApplicationCache,
                    WKWebsiteDataType.SessionStorage,
                    WKWebsiteDataType.WebSQLDatabases
                });
                WKWebsiteDataStore.DefaultDataStore.FetchDataRecordsOfTypes(websiteDataTypes, (NSArray records) =>
                {
                    for (nuint i = 0; i < records.Count; i++)
                    {
                        var record = records.GetItem<WKWebsiteDataRecord>(i);
                        WKWebsiteDataStore.DefaultDataStore.RemoveDataOfTypes(record.DataTypes, new[] { record }, () => {
                            Console.Write($"deleted: {record.DisplayName}");
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            /*NSHttpCookieStorage CookieStorage = NSHttpCookieStorage.SharedStorage;
            foreach (var cookie in CookieStorage.Cookies)
                CookieStorage.DeleteCookie(cookie);*/
        }

        public void InstallLexohRootCertificate()
        {
            try
            {
                var assembly = GetType().GetTypeInfo().Assembly; // you can replace "this.GetType()" with "typeof(MyType)", where MyType is any type in your assembly.
                byte[] buffer = null;
                using (Stream s = assembly.GetManifestResourceStream("Lexoh.iOS.Embedded.LexohRoot.crt"))
                {
                    if (s != null)
                    {
                        long length = s.Length;
                        buffer = new byte[length];
                        s.Read(buffer, 0, (int)length);
                    }
                }
                if(buffer != null && buffer.Length != 0)
                {
                    Security.SecCertificate cert = new Security.SecCertificate(new X509Certificate2(buffer, (string)null, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet));
                    var test = new Security.SecTrust(cert.ToX509Certificate(), Security.SecPolicy.CreateBasicX509Policy());
                    test.Evaluate();
                    var test2 = test.GetResult();
                    
                    Security.SecTrustResult result = test.GetTrustResult();
                    Console.WriteLine(result.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
