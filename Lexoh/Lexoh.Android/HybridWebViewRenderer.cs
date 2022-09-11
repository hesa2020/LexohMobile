using Lexoh;
using Lexoh.Droid;
using Android.Widget;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using static Android.Webkit.WebSettings;

[assembly: ExportRenderer(typeof(HybridWebView), typeof(HybridWebViewRenderer))]
namespace Lexoh.Droid
{
    public class HybridWebViewRenderer : ViewRenderer<HybridWebView, Android.Webkit.WebView>
    {
        const string JavaScriptFunction = "function invokeCSharpAction(data){jsBridge.invokeAction(data);}; window.print = function () { jsBridge.invokeAction('printpage'); };";

        public static Android.Webkit.WebView webView;

        protected override void OnElementChanged(ElementChangedEventArgs<HybridWebView> e)
        {
            base.OnElementChanged(e);

            if (Element != null && Element is HybridWebView && Element.OnExecuteJavascriptCode == null)
            {
                Element.OnExecuteJavascriptCode += OnExecuteJavascriptCode;
                Element.PropertyChanged += OnPropertyChanged;
            }

            if (Control == null)
            {
                webView = new Android.Webkit.WebView(this.Context);
                webView.SetWebViewClient(new HybridWebViewClient(this));
                HybridWebViewChromeClient _webChromeClient = new HybridWebViewChromeClient(this);
                _webChromeClient.SetContext(Context as Android.App.Activity);
                webView.SetWebChromeClient(_webChromeClient);
                webView.Settings.JavaScriptEnabled = true;
                webView.VerticalScrollBarEnabled = true;
                webView.Settings.SetGeolocationEnabled(true);
                webView.Settings.SetPluginState(PluginState.On);
                webView.Settings.MediaPlaybackRequiresUserGesture = false;
                webView.Settings.AllowContentAccess = true;
                webView.Settings.AllowFileAccess = true;
                webView.Settings.AllowFileAccessFromFileURLs = true;
                webView.Settings.AllowUniversalAccessFromFileURLs = true;
                webView.Settings.DatabaseEnabled = true;
                webView.Settings.DomStorageEnabled = true;
                webView.Settings.JavaScriptCanOpenWindowsAutomatically = true;
                webView.Settings.LoadsImagesAutomatically = true;
                
                webView.LayoutParameters = new LinearLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent);

                //webView.SetInitialScale(1);
                //webView.Settings.DefaultZoom = ZoomDensity.Far;
                //webView.Settings.SetSupportZoom(false);
                //webView.Settings.LoadWithOverviewMode = true;
                //webView.Settings.UseWideViewPort = true;

                SetNativeControl(webView);
            }
            if (e.OldElement != null)
            {
                Control.RemoveJavascriptInterface("jsBridge");
                var hybridWebView = e.OldElement as HybridWebView;
                hybridWebView.Cleanup();
            }
            if (e.NewElement != null)
            {
                Control.AddJavascriptInterface(new JSBridge(this), "jsBridge");

                if (Element.Uri.StartsWith("http://") || Element.Uri.StartsWith("https://") || Element.Uri.ToLower() == "about:blank")
                {
                    Control.LoadUrl(Element.Uri);
                }
                else
                {
                    Control.LoadUrl(string.Format("file:///android_asset/Content/{0}", Element.Uri));
                }
                InjectJS(JavaScriptFunction);
            }
        }
        
        void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Uri":
                {
                    if (Element.Uri.StartsWith("http://") || Element.Uri.StartsWith("https://") || Element.Uri.ToLower() == "about:blank")
                    {
                        Control.LoadUrl(Element.Uri);
                    }
                    else
                    {
                        Control.LoadUrl(string.Format("file:///android_asset/Content/{0}", Element.Uri));
                    }
                }
                break;
            }
        }
        
        public void OnExecuteJavascriptCode(string code)
        {
            if (Control != null)
            {
                Control.LoadUrl(string.Format("javascript: {0}", code));
            }
        }

        void InjectJS(string script)
        {
            if (Control != null)
            {
                Control.LoadUrl(string.Format("javascript: {0}", script));
            }
        }
    }
}