using System.IO;
using Lexoh;
using Lexoh.iOS;
using Foundation;
using WebKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using UIKit;
using System;
using System.ComponentModel;
using CoreGraphics;

[assembly: ExportRenderer(typeof(HybridWebView), typeof(HybridWebViewRenderer))]
namespace Lexoh.iOS
{
    public class HybridWebViewRenderer : ViewRenderer<HybridWebView, WKWebView>, IWKScriptMessageHandler, IWKUIDelegate
    {
        const string JavaScriptFunction = "function invokeCSharpAction(data){window.webkit.messageHandlers.invokeAction.postMessage(data);}";
        WKUserContentController userController;
        FormsNavigationDelegate _navigationDelegate;
        public static WKWebView webView = null;

        protected override void OnElementChanged(ElementChangedEventArgs<HybridWebView> e)
        {
            base.OnElementChanged(e);

            if(Element != null && Element is HybridWebView && Element.OnExecuteJavascriptCode == null)
            {
                Element.OnExecuteJavascriptCode += OnExecuteJavascriptCode;
                Element.PropertyChanged += OnPropertyChanged;
            }

            if (Control == null)
            {

                _navigationDelegate = new FormsNavigationDelegate(this);
                userController = new WKUserContentController();
                var script = new WKUserScript(new NSString(JavaScriptFunction), WKUserScriptInjectionTime.AtDocumentEnd, false);
                userController.AddUserScript(script);
                userController.AddUserScript(new WKUserScript(new NSString("window.open = function (open) { return function  (url, name, features) { window.location.href = url; return window; }; } (window.open);"), WKUserScriptInjectionTime.AtDocumentStart, false));
                userController.AddUserScript(new WKUserScript(new NSString("window.print = function () { window.webkit.messageHandlers.invokeAction.postMessage('printpage'); } "), WKUserScriptInjectionTime.AtDocumentStart, false));
                userController.AddScriptMessageHandler(this, "invokeAction");

                var config = new WKWebViewConfiguration {
                    UserContentController = userController,
                };
                webView = new WKWebView(Frame, config)
                {
                    UIDelegate = this,
                    NavigationDelegate = _navigationDelegate
                };

                //webView.ScrollView.MultipleTouchEnabled = false;

                webView.TranslatesAutoresizingMaskIntoConstraints = false;
                webView.ScrollView.Bounces = false;
                SetNativeControl(webView);
            }
            if (e.OldElement != null)
            {
                userController.RemoveAllUserScripts();
                userController.RemoveScriptMessageHandler("invokeAction");
                var hybridWebView = e.OldElement as HybridWebView;
                hybridWebView.Cleanup();
            }
            if (e.NewElement != null)
            {
                try
                {
                    if (Element.Uri.StartsWith("http://") || Element.Uri.StartsWith("https://") || Element.Uri.ToLower() == "about:blank")
                    {
                        Control.LoadRequest(new NSUrlRequest(new NSUrl(Element.Uri)));
                    }
                    else
                    {
                        string fileName = Path.Combine(NSBundle.MainBundle.BundlePath, string.Format("Content/{0}", Element.Uri));
                        Control.LoadRequest(new NSUrlRequest(new NSUrl(fileName, false)));
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                }
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
                        Control.LoadRequest(new NSUrlRequest(new NSUrl(Element.Uri)));
                    }
                    else
                    {
                        string fileName = Path.Combine(NSBundle.MainBundle.BundlePath, string.Format("Content/{0}", Element.Uri));
                        Control.LoadRequest(new NSUrlRequest(new NSUrl(fileName, false)));
                    }
                }
                break;
            }
        }

        public void OnExecuteJavascriptCode(string code)
        {
            Control.EvaluateJavaScript(code, null);
        }

        public void DidReceiveScriptMessage(WKUserContentController userContentController, WKScriptMessage message)
        {
            Element.InvokeAction(message.Body.ToString());
        }

        /*
         * UI Delegate methods from: https://developer.xamarin.com/recipes/ios/content_controls/web_view/handle_javascript_alerts/
         */
        [Export("webView:runJavaScriptAlertPanelWithMessage:initiatedByFrame:completionHandler:")]
        public void RunJavaScriptAlertPanel(WebKit.WKWebView webView, string message, WKFrameInfo frame, Action completionHandler)
        {
            var alertController = UIAlertController.Create(null, message, UIAlertControllerStyle.Alert);
            alertController.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(alertController, true, null);

            completionHandler();
        }

        [Export("webView:runJavaScriptConfirmPanelWithMessage:initiatedByFrame:completionHandler:")]
        public void RunJavaScriptConfirmPanel(WKWebView webView, string message, WKFrameInfo frame, Action<bool> completionHandler)
        {
            var alertController = UIAlertController.Create(null, message, UIAlertControllerStyle.Alert);

            alertController.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, okAction => {

                completionHandler(true);

            }));

            alertController.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Default, cancelAction => {

                completionHandler(false);

            }));

            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(alertController, true, null);
        }

        [Export("webView:runJavaScriptTextInputPanelWithPrompt:defaultText:initiatedByFrame:completionHandler:")]
        public void RunJavaScriptTextInputPanel(WKWebView webView, string prompt, string defaultText, WebKit.WKFrameInfo frame, System.Action<string> completionHandler)
        {
            var alertController = UIAlertController.Create(null, prompt, UIAlertControllerStyle.Alert);

            UITextField alertTextField = null;
            alertController.AddTextField(textField => {
                textField.Placeholder = defaultText;
                alertTextField = textField;
            });

            alertController.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, okAction => {

                completionHandler(alertTextField.Text);

            }));

            alertController.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Default, cancelAction => {

                completionHandler(null);

            }));

            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(alertController, true, null);
        }


    }
}