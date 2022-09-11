using System;
using Android.Webkit;

namespace Actif.Droid
{
    public class JavascriptValueCallback : Java.Lang.Object, IValueCallback
    {

        public Java.Lang.Object Value { get; private set; }

        readonly WeakReference<Xamarin.Forms.WebView> Reference;

        public JavascriptValueCallback(Xamarin.Forms.WebView renderer)
        {
            Reference = new WeakReference<Xamarin.Forms.WebView>(renderer);
        }

        public void OnReceiveValue(Java.Lang.Object value)
        {
            if (Reference == null || !Reference.TryGetTarget(out Xamarin.Forms.WebView renderer)) return;
            Value = value;
        }

        public void Reset()
        {
            Value = null;
        }
    }
}