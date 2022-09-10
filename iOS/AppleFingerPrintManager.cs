using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Foundation;
using LocalAuthentication;
using System.Threading.Tasks;

[assembly: Dependency(typeof(Lexoh.iOS.AppleFingerPrintManager))]
namespace Lexoh.iOS
{
    public class AppleFingerPrintManager : IFingerprintManager
    {
        static List<Action<int>> Callbacks = new List<Action<int>>();

        bool isSupported = false;
        bool initialized = false;

        public AppleFingerPrintManager()
        {
            Initialize();
        }

        void Initialize()
        {
            initialized = true;
            var context = new LAContext();
            NSError error;
            if (context.CanEvaluatePolicy(LAPolicy.DeviceOwnerAuthenticationWithBiometrics, out error))
            {
                isSupported = true;
            }
        }

        public bool Supported()
        {
            if (initialized) Initialize();
            return isSupported;
        }

        static bool hasShownAlready = false;

        public void RequireFingerprint()
        {
            if (hasShownAlready) return;
            hasShownAlready = true;
            App.IsFingerPrintOpen = true;
            var context = new LAContext();
            NSError error;
            if (context.CanEvaluatePolicy(LAPolicy.DeviceOwnerAuthenticationWithBiometrics, out error))
            {
                bool cancelled = false;
                var replyHandler = new LAContextReplyHandler((success, replyError) =>
                {
                    cancelled = replyError != null && (replyError.Code == -2 || (replyError.LocalizedDescription.ToLower().Contains("canceled") || replyError.Description.ToLower().Contains("canceled")));
                    if (success)
                    {
                        Callback(1);
                    }
                    else if(cancelled)
                    {
                        try
                        {
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                context.Invalidate();
                            });
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                        Callback(-1);
                    }
                    else
                    {
                        Callback(0);
                    }
                    hasShownAlready = false;
                    Task.Factory.StartNew(async () =>
                    {
                        await Task.Delay(1);
                        App.IsFingerPrintOpen = false;
                    });
                });

                if (!cancelled)
                    context.EvaluatePolicy(LAPolicy.DeviceOwnerAuthenticationWithBiometrics, Settings.Current.Username, replyHandler);
            }
            else
            {
                isSupported = false;
                Callback(1);
            }
        }

        void Callback(int status)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                foreach (var callback in Callbacks)
                {
                    try
                    {
                        if (callback != null)
                            callback.Invoke(status);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            });            
        }

        public void SubscribeToResult(Action<int> callback)
        {
            if (callback != null)
                Callbacks.Add(callback);
        }
    }
}
