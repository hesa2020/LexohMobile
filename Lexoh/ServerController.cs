using System;
using WebSocketSharp;
using WebSocketSharp.Server;
using Newtonsoft.Json.Linq;

namespace Lexoh
{
    public class ServerController : WebSocketBehavior
    {
        public static ServerController Instance = null;

        public ServerController()
        {
            Instance = this;
        }

        public void SendMessage(string msg)
        {
            Send(msg);
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            try
            {
                if (e.Data.StartsWith("SaveCredentials:"))
                {
                    try
                    {
                        var json = System.Web.HttpUtility.UrlDecode(e.Data.Replace("SaveCredentials:", ""));
                        JObject obj = JObject.Parse(json);
                        var username = obj["Username"].Value<string>();
                        var password = obj["Password"].Value<string>();
                        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                        {
                            if (LexohPage.Instance != null)
                                LexohPage.Instance.SaveCredentials(username, password);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
                else if (e.Data.ToLower() == "deletesettings")
                {
                    if (LexohPage.Instance != null)
                        LexohPage.Instance.DeleteSettings();
                }
                else if (e.Data.ToLower() == "refreshwithoutcache")
                {
                    if (LexohPage.Instance != null)
                        LexohPage.Instance.RefreshWithoutCache();
                }
                else if (e.Data.ToLower() == "deletesettingsandreload")
                {
                    if (LexohPage.Instance != null)
                        LexohPage.Instance.DeleteSettingsAndReload();
                }
                else if (e.Data.ToLower() == "opencodebarscanner")
                {
                    Console.WriteLine("Received 'OpenCodebarScanner' command.");
                    if (LexohPage.Instance != null)
                        LexohPage.Instance.OpenCodebarScanner();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
