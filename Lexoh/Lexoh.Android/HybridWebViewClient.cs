using System;
using Android.Content;
using Android.Webkit;
using WebView = Android.Webkit.WebView;
using Xamarin.Forms;
using Android.Graphics;
using Android.Net.Http;

namespace Lexoh.Droid
{
    public class HybridWebViewClient : WebViewClient
    {
        readonly WeakReference<HybridWebViewRenderer> Reference;
        public HybridWebViewClient(HybridWebViewRenderer renderer)
        {
            Reference = new WeakReference<HybridWebViewRenderer>(renderer);
        }

        public override void OnReceivedHttpError(WebView view, IWebResourceRequest request, WebResourceResponse errorResponse)
        {
            if (Reference == null || !Reference.TryGetTarget(out HybridWebViewRenderer renderer)) return;
            if (renderer.Element == null) return;

            renderer.Element.HandleNavigationError(errorResponse.StatusCode);
            renderer.Element.HandleNavigationCompleted(request.Url.ToString());
        }

        public override void OnReceivedError(WebView view, IWebResourceRequest request, WebResourceError error)
        {
            if (Reference == null || !Reference.TryGetTarget(out HybridWebViewRenderer renderer)) return;
            if (renderer.Element == null) return;

            renderer.Element.HandleNavigationError((int)error.ErrorCode);
            renderer.Element.HandleNavigationCompleted(request.Url.ToString());
        }

        public override bool ShouldOverrideUrlLoading(WebView view, IWebResourceRequest request)
        {
            if (Reference == null || !Reference.TryGetTarget(out HybridWebViewRenderer renderer)) return true;
            if (renderer.Element == null) return true;
            string url = request.Url.ToString();
            var response = renderer.Element.HandleNavigationStartRequest(url);
            if (response.Cancel || response.OffloadOntoDevice)
            {
                if (response.OffloadOntoDevice)
                    AttemptToHandleCustomUrlScheme(view, url);
                view.StopLoading();
                return true;
            }
            return false;
        }

        bool AttemptToHandleCustomUrlScheme(WebView view, string url)
        {
            if (url.StartsWith("mailto"))
            {
                url = url.Replace("mailto:", "");

                Intent email = new Intent(Intent.ActionSendto);
                email.SetData(Android.Net.Uri.Parse("mailto:"));
                email.PutExtra(Intent.ExtraEmail, new String[] { url.Split('?')[0] });
                if (email.ResolveActivity(Forms.Context.PackageManager) != null)
                    Forms.Context.StartActivity(email);
                return true;
            }
            if (url.StartsWith("http"))
            {
                Intent webPage = new Intent(Intent.ActionView, Android.Net.Uri.Parse(url));
                if (webPage.ResolveActivity(Forms.Context.PackageManager) != null)
                    Forms.Context.StartActivity(webPage);

                return true;
            }
            return false;
        }

        public override void OnPageStarted(WebView view, string url, Bitmap favicon)
        {
            if (Reference == null || !Reference.TryGetTarget(out HybridWebViewRenderer renderer)) return;
            if (renderer.Element == null) return;
            var response = renderer.Element.HandleNavigationStartRequest(url);
            if (response.Cancel || response.OffloadOntoDevice)
            {
                if (response.OffloadOntoDevice)
                    AttemptToHandleCustomUrlScheme(view, url);
                view.StopLoading();
            }
        }
        
        public override void OnReceivedSslError(WebView view, SslErrorHandler handler, SslError error)
        {
            if (Reference == null || !Reference.TryGetTarget(out HybridWebViewRenderer renderer)) return;
            if (renderer.Element == null) return;
            
            handler.Proceed();
        }

        public override void OnPageFinished(WebView view, string url)
        {
            if (Reference == null || !Reference.TryGetTarget(out HybridWebViewRenderer renderer)) return;
            if (renderer.Element == null) return;
            
            renderer.Element.HandleNavigationCompleted(url);
        }
    }
}