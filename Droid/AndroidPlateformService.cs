using Android.Content;
using Android.Print;
using Xamarin.Forms;

[assembly: Dependency(typeof(Lexoh.Droid.AndroidPlateformService))]
namespace Lexoh.Droid
{
    public class AndroidPlateformService : IPlatformService
    {
        public void ClearWebViewCache()
        {
            //Not implemented.
        }

        public void PrintWebPage()
        {
            PrintManager printManager = (PrintManager)HybridWebViewRenderer.webView.Context.GetSystemService(Context.PrintService);
            printManager.Print("Lexoh", HybridWebViewRenderer.webView.CreatePrintDocumentAdapter("Lexoh"), null);
            //Need to check if the above call is asynchronous or not.
            LexohPage.Instance?.ExecuteJavascript("$('.printIFrame').remove();");//
        }
    }
}