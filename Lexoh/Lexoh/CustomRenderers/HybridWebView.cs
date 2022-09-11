using Lexoh.Delegates;
using System;
using Xamarin.Forms;

namespace Lexoh
{
    public class HybridWebView : View
    {
        /// <summary>
        /// Fired when navigation begins, for example when the source is set.
        /// </summary>
        public event EventHandler<DecisionHandlerDelegate> OnNavigationStarted;

        /// <summary>
        /// Fires when navigation is completed. This can be either as the result of a valid navigation, or on an error.
        /// Returns the URL of the page navigated to.
        /// </summary>
        public event EventHandler<string> OnNavigationCompleted;

        /// <summary>
        /// Fires when navigation fires an error. By default this uses the native systems error codes.
        /// </summary>
        public event EventHandler<int> OnNavigationError;

        Action<string> action;

        public static readonly BindableProperty UriProperty = BindableProperty.Create(
            propertyName: "Uri",
            returnType: typeof(string),
            declaringType: typeof(HybridWebView),
            defaultValue: default(string));

        public string Uri
        {
            get { return (string)GetValue(UriProperty); }
            set { SetValue(UriProperty, value); }
        }

        public void RegisterAction(Action<string> callback)
        {
            action = callback;
        }

        public void Cleanup()
        {
            action = null;
        }

        public Action<string> OnExecuteJavascriptCode;

        public void ExecuteJavascript(string code)
        {
            if (OnExecuteJavascriptCode == null) return;
            OnExecuteJavascriptCode.Invoke(code);
        }

        public void InvokeAction(string data)
        {
            if (action == null || data == null)
            {
                return;
            }
            action.Invoke(data);
        }

        #region Internals

        public DecisionHandlerDelegate HandleNavigationStartRequest(string uri)
        {
            // By default, we only attempt to offload valid Uris with none http/s schemes
            bool validUri = System.Uri.TryCreate(uri, UriKind.Absolute, out Uri uriResult);
            bool validScheme = false;

            if (validUri)
                validScheme = uriResult.Scheme.StartsWith("http") || uriResult.Scheme.StartsWith("file");

            var handler = new DecisionHandlerDelegate()
            {
                Uri = uri,
                OffloadOntoDevice = validUri && !validScheme
            };

            OnNavigationStarted?.Invoke(this, handler);
            return handler;
        }

        public void HandleNavigationCompleted(string uri)
        {
            OnNavigationCompleted?.Invoke(this, uri);
        }

        public void HandleNavigationError(int errorCode)
        {
            OnNavigationError?.Invoke(this, errorCode);
        }
        #endregion

    }
}