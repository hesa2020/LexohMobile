using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Webkit;
using Java.IO;
using System.Collections.Generic;
using Object = Java.Lang.Object;
using Uri = Android.Net.Uri;

namespace Lexoh.Droid
{
    public class HybridWebViewChromeClient : WebChromeClient
    {
        Activity _activity;
        List<int> _requestCodes;
        Uri lastUri;
        private readonly HybridWebViewRenderer _context;

        public HybridWebViewChromeClient(HybridWebViewRenderer context)
        {
            _context = context;
        }
        
        public override void OnGeolocationPermissionsShowPrompt(string origin, GeolocationPermissions.ICallback callback)
        {
            callback.Invoke(origin, true, true);
        }

        public override void OnPermissionRequest(PermissionRequest request)
        {
            request.Grant(request.GetResources());
        }

        public override bool OnShowFileChooser(global::Android.Webkit.WebView webView, IValueCallback filePathCallback, FileChooserParams fileChooserParams)
        {
            base.OnShowFileChooser(webView, filePathCallback, fileChooserParams);
            return ChooseFile(filePathCallback, fileChooserParams.CreateIntent(), fileChooserParams.Title);
        }

        public void UnregisterCallbacks()
        {
            if (_requestCodes == null || _requestCodes.Count == 0 || _activity == null)
                return;
            foreach (int requestCode in _requestCodes)
            {
                ActivityResultCallbackRegistry.UnregisterActivityResultCallback(requestCode);
            }
            _requestCodes = null;
        }

        protected bool ChooseFile(IValueCallback filePathCallback, Intent intent, string title)
        {
            System.Action<Result, Intent> callback = (resultCode, intentData) =>
            {
                if (filePathCallback == null)
                    return;
                Object result = ParseResult(resultCode, intentData);
                filePathCallback.OnReceiveValue(result);
            };
            _requestCodes = _requestCodes ?? new List<int>();
            int newRequestCode = ActivityResultCallbackRegistry.RegisterActivityResultCallback(callback);
            _requestCodes.Add(newRequestCode);

            //_activity.StartActivityForResult(Intent.CreateChooser(intent, title), newRequestCode);
            
            var dir = new File(Environment.GetExternalStoragePublicDirectory(Environment.DirectoryPictures), "Lexoh");
            if(!dir.Exists())
            {
                dir.Mkdirs();
            }
            Intent intentCapture = new Intent(MediaStore.ActionImageCapture);

            lastUri = Uri.FromFile(new File(dir, string.Format("photo_{0}.jpg", System.Guid.NewGuid())));
            //intentCapture.PutExtra("android.intent.extra.quickCapture", true);//This disable the confirmation after taking a picture.
            intentCapture.PutExtra(MediaStore.ExtraOutput, lastUri);
            intentCapture.AddFlags(ActivityFlags.SingleTop);
            _activity.StartActivityForResult(intentCapture, newRequestCode);
            
            return true;
        }

        protected virtual Object ParseResult(Result resultCode, Intent data)
        {
            if(data == null)
            {
                Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
                mediaScanIntent.SetData(lastUri);
                _activity.SendBroadcast(mediaScanIntent);
                return FileChooserParams.ParseResult((int)resultCode, mediaScanIntent);
            }
            return FileChooserParams.ParseResult((int)resultCode, data);
        }

        internal void SetContext(Activity thisActivity)
        {
            if (thisActivity == null)
                throw new System.ArgumentNullException(nameof(thisActivity));
            _activity = thisActivity;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                UnregisterCallbacks();
            base.Dispose(disposing);
        }
    }
}