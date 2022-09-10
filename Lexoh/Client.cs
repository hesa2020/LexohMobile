using System;
using WebSocketSharp;

namespace Lexoh
{
    public class Client
    {
        public Action OnError;
        public Action OnConnected;
        public string ErrorMessage = "";
        private bool shouldBeConnected = false;

        WebSocket SocketClient;
        public Client(string server)
        {
            try
            {
                SocketClient = new WebSocket(server);
                SocketClient.SslConfiguration.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => { return true; };
                SocketClient.OnError += SocketClient_OnError;
                SocketClient.OnMessage += SocketClient_OnMessage;
                SocketClient.OnOpen += SocketClient_OnOpen;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void SocketClient_OnOpen(object sender, EventArgs e)
        {
            if (OnConnected != null)
                OnConnected.Invoke();
        }

        private void SocketClient_OnMessage(object sender, MessageEventArgs e)
        {
            ErrorMessage = e.Data;
        }

        public void Connect()
        {
            try
            {
                shouldBeConnected = true;
                SocketClient.Connect();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void Dispose()
        {
            try
            {
                shouldBeConnected = false;
                SocketClient.Close();
                SocketClient = null;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void SendData(string data)
        {
            SocketClient.Send(data);
        }

        private void SocketClient_OnError(object sender, ErrorEventArgs e)
        {
            if (OnError == null || !shouldBeConnected)
                return;
            OnError.Invoke();
        }
    }
}