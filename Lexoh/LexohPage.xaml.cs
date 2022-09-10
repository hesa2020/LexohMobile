using Xamarin.Forms;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
//using Xamarin.Essentials;
using System;

namespace Lexoh
{
    public interface IPlatformService
    {
        void ClearWebViewCache();
        void PrintWebPage();
    }

    public partial class LexohPage : ContentPage
    {
        public static string Version = "1.0.1";
        public static LexohPage Instance;
        public static string AppUrl = "about:blank";
        int errorCount;

        string NotificationUrl = "";

        static bool Initialized;

        static bool isLogged;
        public static bool IsLogged
        {
            get { return isLogged; }
            set
            {
                isLogged = value;
            }
        }

        public static bool LoggedOut;
        /*
        double width = 0;
        double height = 0;

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            if (width != this.width || height != this.height)
            {
                WebContent.HeightRequest = width;
                WebContent.WidthRequest = height;

                this.width = width;
                this.height = height;
                if (width > height)
                {
                    //outerStack.Orientation = StackOrientation.Horizontal;

                }
                else
                {
                    //outerStack.Orientation = StackOrientation.Vertical;
                }
            }
        }
        */
        public LexohPage()
        {
            Instance = this;
            InitializeComponent();
            Initialize();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            MessagingCenter.Send(this, "allowLandScapePortrait");
            NavigationPage.SetHasNavigationBar(this, false);
        }

        //during page close setting back to portrait
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MessagingCenter.Send(this, "preventLandScape");
        }

        void Initialize()
        {
            if (Initialized) return;
            Initialized = true;
            //UrlManager.Manager.ClearCache();
            try
            {
                /*if(Device.RuntimePlatform == Device.Android)
                {
                    WebContent.IsVisible = true;
                }*/

                if (Version != Settings.Current.Version)
                {
                    Settings.Current.Version = Version;
                    Settings.SaveSettings();
                    UrlManager.Manager.ClearCache();
                }

                AppUrl = "https://app.lexoh.com/";
                if (!string.IsNullOrEmpty(Settings.UrlSettings))
                {
                    if (!Settings.UrlSettings.StartsWith("http://") && !Settings.UrlSettings.StartsWith("https://"))
                    {
                        AppUrl = string.Format("http://{0}", Settings.UrlSettings);
                    }
                    else AppUrl = Settings.UrlSettings;
                }

                AppUrl = "https://app.lexoh.com/";

                WebContent.Uri = AppUrl;
                
                WebContent.RegisterAction(data =>
                {
                    try
                    {
                        Console.WriteLine(data);
                        if (data == "GetToken")
                        {
                            //string token = FirebaseManager.Manager.GetToken();
                            //Console.WriteLine("My Token: " + token);
                            //ExecuteJavascript($"SetToken('{token}');");
                            //ExecuteJavascript($"mobileApp.GetToken('{token}');");
                        }
                        if(data.ToLower() == "printpage")
                        {
                            PrintPage();
                        }
                        else if (data.StartsWith("SaveCredentials:"))
                        {
                            try
                            {
                                var json = System.Web.HttpUtility.UrlDecode(data.Replace("SaveCredentials:", "").Replace("+", "%2B"));
                                JObject obj = JObject.Parse(json);
                                var username = obj["Username"].Value<string>();
                                var password = obj["Password"].Value<string>();
                                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                                {
                                    SaveCredentials(username, password);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                            }
                        }
                        else if (data.ToLower() == "deletesettings")
                        {
                            DeleteSettings();
                        }
                        else if (data.ToLower() == "refreshwithoutcache")
                        {
                            RefreshWithoutCache();
                        }
                        else if (data.ToLower() == "deletesettingsandreload")
                        {
                            DeleteSettingsAndReload();
                        }
                        else if (data.ToLower() == "opencodebarscanner")
                        {
                            Console.WriteLine("Received 'OpenCodebarScanner' command.");
                            OpenCodebarScanner();
                        }
                        else if (data.ToLower() == "promptforfingerprint")
                        {
                            Console.WriteLine("Received 'PromptForFingerprint' command.");
                            PromptForFingerprint();
                        }
                        else if (data.ToLower() == "requirefingerprint")
                        {
                            Console.WriteLine("Received 'RequireFingerprint' command.");
                            RequireFingerprint();
                        }
                        else if (data.StartsWith("Print:"))
                        {
                            var json = data.Replace("Print:", "");
                            JObject obj = JObject.Parse(json);
                            var server = obj["Server"].Value<string>();
                            var printData = obj["Data"].Value<string>();

                            if (!string.IsNullOrEmpty(server) && !string.IsNullOrEmpty(printData))
                            {
                                Client socketClient = new Client(server);
                                socketClient.OnConnected = new Action(() =>
                                {
                                    socketClient.SendData(printData);
                                    socketClient.Dispose();
                                    socketClient = null;
                                });
                                socketClient.OnError = new Action(() =>
                                {
                                    ExecuteJavascript("mobileApp.ShowPrintConnectionError();");
                                });
                                socketClient.Connect();

                            }
                        }
                        else if (data.StartsWith("Restart"))
                        {
                            IsLogged = false;
                            LoggedOut = false;//Set to true to avoid asking fingerprint after logging out
                            Restart();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                });

                WebContent.OnNavigationStarted += WebContent_OnNavigationStarted;
                WebContent.OnNavigationCompleted += WebContent_OnNavigationCompleted;
                //FingerprintManager.Manager.SubscribeToResult(new Action<int>((result) => {
                //    if (result == 1)//Success
                //    {
                //        errorCount = 0;
                //        if (!IsLogged)
                //            ExecuteJavascript($"mobileApp.SubmitLogin('{Settings.Current.Username}', '{Settings.Current.Password}');");
                //        IsLogged = true;
                //        LoggedOut = false;
                //        ExecuteJavascript("mobileApp.ShowFingerprintRequired(false);");
                //    }
                //    else if (result == 0)//Failure
                //    {
                //        errorCount++;
                //        if (errorCount >= 3)
                //        {
                //            DeleteSettingsAndReload();
                //        }
                //        else
                //        {
                //            RequireFingerprint();
                //        }
                //    }
                //    else//Cancelled
                //    {
                //        try
                //        {
                //            if (IsLogged)
                //            {
                //                Restart();
                //                LoggedOut = false;
                //            }
                //            else
                //            {
                //                //Settings.Current.UseFingerprint = false;
                //                LoggedOut = true;
                //            }
                //            IsLogged = false;
                //            ExecuteJavascript("mobileApp.ShowFingerprintRequired(false);");
                //        }
                //        catch (Exception ex)
                //        {
                //            Console.WriteLine(ex);
                //        }
                //    }
                //}));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Initialization Exception: " + ex);
            }
        }

        protected override bool OnBackButtonPressed()
        {
            CloseWindow();
            return true;
        }

        internal void EnableView(bool enabled)
        {
           /* if(WebContent != null)
                WebContent.IsEnabled = enabled;*/
        }

        public void ExecuteJavascript(string javascript)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                WebContent.ExecuteJavascript(javascript);
            });
        }

        string lastUri = "about:blank";

        void WebContent_OnNavigationStarted(object sender, Delegates.DecisionHandlerDelegate e)
        {
            if (e.Uri.Contains("support.lexoh.com"))
            {
                e.Cancel = true;
                //Launcher.OpenAsync(e.Uri);
                Device.OpenUri(new Uri(e.Uri));
            }
            else if (e.Uri.Contains("lexoh.com") || e.Uri == AppUrl || e.Uri.Contains(AppUrl.Replace("https://", "").Replace("http://", "")))
            {
                //Process
                lastUri = e.Uri;
            }
            else if(e.Uri.Contains("amazonaws.com"))
            {
                e.Cancel = true;
                Device.OpenUri(new Uri(e.Uri));
                //Launcher.OpenAsync(e.Uri);
            }
            else if (e.Uri.Contains("maps") && !e.Uri.Contains("google.com/maps/embed") && UrlManager.Manager.OpenUrl(e.Uri).Result == true)// if(e.Uri.Contains("google.ca/maps") || e.Uri.Contains("google.com/maps") || e.Uri.Contains("google.fr/maps") || e.Uri.Contains("google.uk/maps"))
            {
                e.Cancel = true;
            }
            else
            {
                lastUri = e.Uri;
                e.Cancel = false;
            }
        }
        
        void WebContent_OnNavigationCompleted(object sender, string e)
        {
            WebContent.IsVisible = true;
            //string ios = Device.RuntimePlatform == Device.iOS ? "true" : "false";
            //ExecuteJavascript("mobileApp.Initialize(" + ios + ");");
            if(!string.IsNullOrEmpty(NotificationUrl) && lastUri != "about:blank" && lastUri != AppUrl && lastUri.Contains("lexoh.com") && !lastUri.ToLower().StartsWith("https://login.lexoh.com") && !lastUri.ToLower().StartsWith("https://login.dev.lexoh.com"))
            {
                var old = NotificationUrl;
                NotificationUrl = "";
                Redirect(old);
                Console.WriteLine("Should redirect to: " + old);
            }
            //CheckAutoLogin();
        }

        public void RequireFingerprint()
        {
            if (!AppUrl.Contains("lexoh.com")) return;
            Task.Factory.StartNew(async () =>
            {
                await Task.Delay(10);
                Device.BeginInvokeOnMainThread(() =>
                {
                    if (!string.IsNullOrEmpty(Settings.Current.Username) && !string.IsNullOrEmpty(Settings.Current.Password))
                    {
                        //if (Settings.Current.UseFingerprint && FingerprintManager.Manager.Supported())
                        //{
                        //    App.PreventFingerprint = true;
                        //    ExecuteJavascript("mobileApp.ShowFingerprintRequired(true);");
                        //    FingerprintManager.Manager.RequireFingerprint();
                        //}
                        //else
                        //{
                        //    //We do not want this anymore:
                        //    //ExecuteJavascript($"mobileApp.SubmitLogin('{Settings.Current.Username}', '{Settings.Current.Password}');");
                        //}
                    }
                });
            });
        }

        public void CheckAutoLogin()
        {
            if (!AppUrl.Contains("lexoh.com")) return;
            if ((!IsLogged && (lastUri.ToLower() == AppUrl.ToLower() || lastUri.ToLower() == AppUrl.ToLower().Remove(AppUrl.Length - 1))))
            {
                RequireFingerprint();
            }
        }

        public void SaveCredentials(string username, string password)
        {
            try
            {
                if (username != Settings.Current.Username)
                {
                    Settings.Current.UseFingerprint = false;
                }
                Settings.Current.Username = username;
                Settings.Current.Password = password;
                Settings.SaveSettings();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void PromptForFingerprint()
        {
            IsLogged = true;
            //bool supported = FingerprintManager.Manager.Supported();
            //if (supported && !Settings.Current.UseFingerprint)
            //{
            //    //Ask the user if he wants to use finger print for auto logging in.
            //    AskTouchIdSaving();
            //}
        }

        public void DeleteSettings()
        {
            IsLogged = false;
            LoggedOut = false;
            Settings.Current.UseFingerprint = false;
            Settings.Current.Password = "";
            Settings.Current.Username = "";
            Settings.SaveSettings();
        }

        public void RefreshWithoutCache()
        {
            //UrlManager.Manager.ClearCache();
            try
            {
                DependencyService.Get<IPlatformService>().ClearWebViewCache();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
            Restart();
        }

        public void PrintPage()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    DependencyService.Get<IPlatformService>().PrintWebPage();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            });
        }

        public void DeleteSettingsAndReload()
        {
            DeleteSettings();
            Restart();
        }

        public void OpenCodebarScanner()
        {
            //Device.BeginInvokeOnMainThread(async () =>
            //{
            //    var result = await new MobileBarcodeScanner().Scan();
            //    if (result == null || result.Text == null) return;
            //    //await DisplayAlert("Scanned Barcode", result.Text, "OK");
            //    ExecuteJavascript("mobileApp.SetBarcode('" + result.Text + "');");
            //});
        }

        async void AskTouchIdSaving()
        {
            Settings.Current.UseFingerprint = await DisplayAlert("Connexion automatique", "Voulez-vous autoriser l'authentification par la reconnaissance de l'empreinte digitale?", "Oui", "Non");
            Settings.SaveSettings();
        }

        public void Redirect(string url)
        {
            ExecuteJavascript("window.location.href = '" + url + "'");
        }

        public void Restart()
        {
            try
            {
                IsLogged = false;
                Redirect("/signout");
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void CloseWindow()
        {
            //ExecuteJavascript("jWindow_Close();");
        }

        public void OpenNotification(string tableName, string id)
        {
            Console.WriteLine("OpenNotification called, WebContent Uri = " + WebContent.Uri);
            //if (!AppUrl.Contains("lexoh.com")) return;//This is probably TIF, in which case we cant handle the notification click redirection...
            //if(string.IsNullOrEmpty(WebContent.Uri) || lastUri == "about:blank" || lastUri == AppUrl || lastUri.ToLower().StartsWith("https://login.lexoh.com"))
            //{
            //    NotificationUrl = "/admin.html?directpage=" + tableName + "&dpID=" + id;
            //}
            //else
            //{
            //    WebContent.IsVisible = true;
            //    Console.WriteLine("Should redirect directly to: " + "/admin.html?directpage=" + tableName + "&dpID=" + id);
            //    Redirect("/admin.html?directpage=" + tableName + "&dpID=" + id);
            //}
        }
    }
}
