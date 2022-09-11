﻿using Xamarin.Forms;
using System;
using System.Reflection;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Device = Xamarin.Forms.Device;

namespace Lexoh
{
    public partial class App : Application
    {
        static DateTime SleepAt;
        public static bool IsFingerPrintOpen;
        public static bool PreventFingerprint;
        public static bool IsDev = false;

        public App()
        {
            InitializeComponent();

            //TEST DRAWINGS...
            //MainPage = new SignaturePage();
            //return;

            Settings.LoadSettings();

            MainPage = new NavigationPage(new LexohPage());
        }

        public byte[] FileFromResource(string r)
        {
            // Ensure "this" is an object that is part of your implementation within your Xamarin forms project
            var assembly = this.GetType().GetTypeInfo().Assembly;
            byte[] buffer = null;
            using (System.IO.Stream s = assembly.GetManifestResourceStream(r))
            {
                if (s != null)
                {
                    long length = s.Length;
                    buffer = new byte[length];
                    s.Read(buffer, 0, (int)length);
                }
            }
            return buffer;
        }

        public string GetNamespace()
        {
            return "Lexoh." + Device.RuntimePlatform + ".Embedded.";
        }

        protected override void OnStart()
        {
            // Handle when your app starts
            //"b1bcef9f-5d84-40c4-a202-e9943d88f77a"
            AppCenter.Start("ios=b1bcef9f-5d84-40c4-a202-e9943d88f77a;" +
                  "uwp={Your UWP App secret here};" +
                  "android={Your Android App secret here};" +
                  "macos={Your macOS App secret here};",
                  typeof(Analytics), typeof(Crashes));
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
            SleepAt = DateTime.Now;
            if (IsFingerPrintOpen) PreventFingerprint = true;
            Console.WriteLine("App is sleeping");
            if (LexohPage.Instance != null)
                LexohPage.Instance.EnableView(false);
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
            if (LexohPage.Instance != null)
                LexohPage.Instance.EnableView(true);
            Console.WriteLine("App is resume");
            //LexohPage.IsLogged = false;
            //LexohPage.LoggedOut = false;
            if (LexohPage.IsLogged)
            {
                TimeSpan elapsed = DateTime.Now - SleepAt;
                if (elapsed.TotalMinutes >= 30)
                {
                    if (LexohPage.Instance != null)
                        LexohPage.Instance.Restart();
                }
                else if (elapsed.TotalMinutes >= 10)
                {
                    /*if (Settings.Current.UseFingerprint && FingerprintManager.Manager.Supported())
                    {
                        if (LexohPage.Instance != null)
                            LexohPage.Instance.RequireFingerprint();
                    }
                    else*/ if (LexohPage.Instance != null)
                        LexohPage.Instance.Restart();
                }
            } 
            else if (!PreventFingerprint)
            {
                LexohPage.Instance?.RequireFingerprint();
            }
            PreventFingerprint = false;
        }
    }
}
