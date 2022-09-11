using System;
using Foundation;
using WebKit;
using UIKit;

namespace Lexoh.iOS
{
    public class FormsNavigationDelegate : WKNavigationDelegate
    {

        readonly WeakReference<HybridWebViewRenderer> Reference;

        public FormsNavigationDelegate(HybridWebViewRenderer renderer)
        {
            Reference = new WeakReference<HybridWebViewRenderer>(renderer);
        }

        public bool AttemptOpenCustomUrlScheme(NSUrl url)
        {
            var app = UIApplication.SharedApplication;

            if (app.CanOpenUrl(url))
                return app.OpenUrl(url);

            return false;
        }

        [Export("webView:decidePolicyForNavigationAction:decisionHandler:")]
        public override void DecidePolicy(WKWebView webView, WKNavigationAction navigationAction, Action<WKNavigationActionPolicy> decisionHandler)
        {
            if (Reference == null || !Reference.TryGetTarget(out HybridWebViewRenderer renderer)) return;
            if (renderer.Element == null) return;

            var response = renderer.Element.HandleNavigationStartRequest(navigationAction.Request.Url.ToString());

            if (response.Cancel || response.OffloadOntoDevice)
            {
                if (response.OffloadOntoDevice)
                    AttemptOpenCustomUrlScheme(navigationAction.Request.Url);

                decisionHandler(WKNavigationActionPolicy.Cancel);
            }
            else
            {
                decisionHandler(WKNavigationActionPolicy.Allow);
            }
        }

        [Export("webView:didFinishNavigation:")]
        public override void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
        {
			if (Reference == null || !Reference.TryGetTarget(out HybridWebViewRenderer renderer)) return;
			if (renderer.Element == null) return;

            renderer.Element.HandleNavigationCompleted(webView.Url.ToString());
        }
    }
}
