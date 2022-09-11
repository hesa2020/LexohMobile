//using Firebase.CloudMessaging;
using Foundation;
using System.Linq;
using System.Threading.Tasks;
using UIKit;
using UserNotifications;

namespace Lexoh.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate, IUNUserNotificationCenterDelegate//, IMessagingDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            //ZXing.Net.Mobile.Forms.iOS.Platform.Init();Zone
            
            global::Xamarin.Forms.Forms.Init();

            // Code for starting up the Xamarin Test Cloud Agent
#if DEBUG
			//Xamarin.Calabash.Start();
#endif
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                // iOS 10 or later
                var authOptions = UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound;
                UNUserNotificationCenter.Current.RequestAuthorization(authOptions, (granted, error) =>
                {
                    if (granted)
                    {
                        InvokeOnMainThread(() => {
                            UIApplication.SharedApplication.RegisterForRemoteNotifications();
                        });
                    }
                });

                // For iOS 10 display notification (sent via APNS)
                UNUserNotificationCenter.Current.Delegate = this;
            }
            else
            {
                // iOS 9 or before
                var allNotificationTypes = UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound;
                var settings = UIUserNotificationSettings.GetSettingsForTypes(allNotificationTypes, null);
                UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
            }
            //UIApplication.SharedApplication.RegisterForRemoteNotifications();

            System.Console.WriteLine("Loading Application");

            LoadApplication(new App());

            if (options != null)
            {
                if (options.ContainsKey(UIApplication.LaunchOptionsRemoteNotificationKey))
                {
                    NSDictionary userInfo = (NSDictionary)options[UIApplication.LaunchOptionsRemoteNotificationKey];
                    // reset our badge
                    UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;
                    if (userInfo != null && userInfo.ContainsKey(new NSString("type")))
                    {
                        var type = userInfo["type"] as NSString;
                        var type_id = userInfo["type_id"] as NSString;
                        System.Console.WriteLine("Received Notification Inside FInishedLaunching");
                        LexohPage.Instance.OpenNotification(type, type_id);
                    }
                }
            }

            return base.FinishedLaunching(app, options);
        }

        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations(UIApplication application, UIWindow forWindow)
        {
            //var mainPage = Xamarin.Forms.Application.Current.MainPage;
            //if (mainPage.Navigation.NavigationStack.Count != 0 && mainPage.Navigation.NavigationStack.Last() is SignaturePage)
            //{
            //    return UIInterfaceOrientationMask.Portrait;
            //}
            return UIInterfaceOrientationMask.All;
        }

        public override void DidEnterBackground(UIApplication uiApplication)
        {
            System.Console.WriteLine("DidEnterBackground");
            //Messaging.SharedInstance.Disconnect();
        }

        public override void OnActivated(UIApplication uiApplication)
        {
            System.Console.WriteLine("OnActivated");
            ConnectFCM();
            base.OnActivated(uiApplication);
        }

        public override void WillTerminate(UIApplication uiApplication)
        {
            UrlManager.Manager.ClearCache();
            base.WillTerminate(uiApplication);
        }

        public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
        {
            System.Console.WriteLine("RegisteredForRemoteNotifications");
//#if DEBUG
//            Firebase.InstanceID.InstanceId.SharedInstance.SetApnsToken(deviceToken, Firebase.InstanceID.ApnsTokenType.Sandbox);
//#endif
//#if RELEASE
//			Firebase.InstanceID.InstanceId.SharedInstance.SetApnsToken(deviceToken, Firebase.InstanceID.ApnsTokenType.Prod);
//#endif
        }

        // iOS 9 <=, fire when recieve notification foreground
        public override void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, System.Action<UIBackgroundFetchResult> completionHandler)
        {
            try
            {
                System.Console.WriteLine("DidReceiveRemoteNotification");
                //Messaging.SharedInstance.AppDidReceiveMessage(userInfo);

                var count = UIApplication.SharedApplication.ApplicationIconBadgeNumber - 1;
                UIApplication.SharedApplication.ApplicationIconBadgeNumber = count < 0 ? 0 : count;

                //if(application.ApplicationState != UIApplicationState.Inactive)
                {
                    if (userInfo.ContainsKey(new NSString("type")))
                    {
                        var type = userInfo["type"] as NSString;
                        var type_id = userInfo["type_id"] as NSString;
                        //ShowAlert("Notification!", "We are inside DidReceiveRemoteNotification and State is not inactive!?");
                        LexohPage.Instance.OpenNotification(type, type_id);
                    }
                }                
            }
            catch(System.Exception ex)
            {
                System.Console.WriteLine(ex);
            }
        }

        // iOS 10, fire when recieve notification foreground
        [Export("userNotificationCenter:willPresentNotification:withCompletionHandler:")]
        public void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, System.Action<UNNotificationPresentationOptions> completionHandler)
        {
            /*System.Console.WriteLine("WillPresentNotification");
            var test = notification as NSDictionary;
            var title = notification.Request.Content.Title;
            var body = notification.Request.Content.Body;
            ShowAlert(title, body);*/
            completionHandler(UNNotificationPresentationOptions.Alert | UNNotificationPresentationOptions.Sound | UNNotificationPresentationOptions.Badge);
        }

        private void ConnectFCM()
        {
            //Messaging.SharedInstance.Connect((error) =>
            //{
            //    if (error == null)
            //    {
            //        System.Console.WriteLine("ConnectFCM: Connected");
            //        //TODO: Change Topic to what is required
            //        Messaging.SharedInstance.Subscribe("/topics/all");
            //    }
            //    System.Diagnostics.Debug.WriteLine(error != null ? "error occured" : "connect success");
            //});
        }

        private void ShowAlert(string title, string message)
        {
            System.Console.WriteLine("ShowAlert");
            if (string.IsNullOrEmpty(message)) return;
            var alertController = UIAlertController.Create(title ?? "Alert", message, UIAlertControllerStyle.Alert);
            alertController.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(alertController, true, null);
        }

        //public void DidRefreshRegistrationToken(Messaging messaging, string fcmToken)
        //{
        //    System.Console.WriteLine("DidRefreshRegistrationToken");
        //    System.Console.WriteLine("FCM Token = " + fcmToken);
        //}

        public override void ReceivedLocalNotification(UIApplication application, UILocalNotification notification)
        {
            // show an alert
            UIAlertController okayAlertController = UIAlertController.Create(notification.AlertAction, notification.AlertBody, UIAlertControllerStyle.Alert);
            okayAlertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(okayAlertController, true, null);

            // reset our badge
            UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;
        }
    }
}
