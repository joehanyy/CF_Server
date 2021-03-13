using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CF_Server.Network.Sockets
{
    public class ServerSocket
    {
        public event Action<ClientWrapper> OnClientConnect, OnClientDisconnect;
        public event Action<byte[], int, ClientWrapper> OnClientReceive;
        private const int TimeLimit = 1000 * 15;
        private object SyncRoot;

        private Socket Connection;
        public ushort port;
        private string ipString;
        private bool enabled;
        private System.Threading.Thread thread;
        public ServerSocket()
        {
            this.Connection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.SyncRoot = new object();
            thread = new System.Threading.Thread(doSyncAccept);
            thread.Start();
        }

        public void Enable(ushort port, string ip)
        {
            this.ipString = ip;
            this.port = port;
            bool firstTime = false;
            while (true)
            {
                try
                {
                    this.Connection.Bind(new IPEndPoint(IPAddress.Parse(ipString), this.port));
                    this.Connection.Listen((int)SocketOptionName.MaxConnections);
                    break;
                }
                catch
                {
                    if (!firstTime)
                    {
                        Console.WriteLine("Please close any other application which using the same port!", ConsoleColor.DarkRed);
                    }
                    firstTime = true;
                    System.Threading.Thread.Sleep(5000);
                    continue;
                }
            }
            this.enabled = true;
        }

        public void Enable(ushort port)
        {
            this.ipString = "0.0.0.0";
            this.port = port;
            bool firstTime = false;
            while (true)
            {
                try
                {
                    this.Connection.Bind(new IPEndPoint(IPAddress.Parse(ipString), this.port));
                    this.Connection.Listen((int)SocketOptionName.MaxConnections);
                    break;
                }
                catch
                {
                    if (!firstTime)
                    {
                        Console.WriteLine("Please close any other application which using the same port!", ConsoleColor.DarkRed);
                    }
                    firstTime = true;
                    System.Threading.Thread.Sleep(5000);
                    continue;
                }
            }
            this.enabled = true;
        }

        public bool PrintoutIPs = false;
        private void doSyncAccept()
        {
            while (true)
            {
                if (this.enabled)
                {
                    try
                    {
                        processSocket(this.Connection.Accept());
                    }
                    catch { }
                }
                System.Threading.Thread.Sleep(1);
            }
        }
        private void doAsyncAccept(IAsyncResult res)
        {
            try
            {
                Socket socket = this.Connection.EndAccept(res);
                processSocket(socket);
                this.Connection.BeginAccept(doAsyncAccept, null);
            }
            catch
            {

            }
        }

        private void processSocket(Socket socket)
        {
            try
            {
                string ip = (socket.RemoteEndPoint as IPEndPoint).Address.ToString();
                ip.GetHashCode();
                ClientWrapper wrapper = new ClientWrapper();
                wrapper.Alive = true;
                wrapper.IP = ip;
                wrapper.Create(socket, this, this.OnClientReceive);
                if (this.OnClientConnect != null) this.OnClientConnect(wrapper);
            }
            catch
            {

            }
        }

        public void Reset()
        {
            this.Disable();
            this.Enable();
        }

        public void Disable()
        {
            this.enabled = false;
            this.Connection.Close(1);
        }

        public void Enable()
        {
            if (!this.enabled)
            {
                this.Connection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.Connection.Bind(new IPEndPoint(IPAddress.Parse(ipString), this.port));
                this.Connection.Listen((int)SocketOptionName.MaxConnections);
                this.enabled = true;
            }
        }

        public void InvokeDisconnect(ClientWrapper Client)
        {
            if (this.OnClientDisconnect != null)
                this.OnClientDisconnect(Client);
        }

        public bool Enabled
        {
            get
            {
                return this.enabled;
            }
        }
    }
}
