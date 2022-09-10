using System;
using Android.Webkit;

namespace Actif.Droid
{
    public class FormsWebViewChromeClient : WebChromeClient
    {

        readonly WeakReference<FormsWebViewRenderer> Reference;

        public FormsWebViewChromeClient(FormsWebViewRenderer renderer)
        {
            Reference = new WeakReference<FormsWebViewRenderer>(renderer);
        }

        public override void OnGeolocationPermissionsShowPrompt(string origin, GeolocationPermissions.ICallback callback)
        {
            callback.Invoke(origin, true, true);
        }

        public override void OnPermissionRequest (PermissionRequest request)
        {
            request.Grant(request.GetResources());
        }

    }
}