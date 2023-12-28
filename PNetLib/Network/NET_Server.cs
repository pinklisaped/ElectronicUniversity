using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace PNetwork
{
    public delegate void Received(Packet packet, Client sender);
    public delegate void Error(string message);
    public delegate void Connections(Client client);
    public class NET_Server
    {
        public event Connections OnConnected, OnDisconnected;
        public event Received OnRecieved;
        public event Error Error;
        Socket SERVER;
        List<Client> CLIENTS;
        IPEndPoint ADR;
        byte[] DATA;
        public NET_Server(int port)
        {
            SERVER = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            CLIENTS = new List<Client>(20);
            ADR = new IPEndPoint(IPAddress.Any, port);
            DATA = new byte[1024];
        }
        public void Listen()
        {
            SERVER.Bind(ADR);
            SERVER.Listen(64);
            SERVER.BeginAccept(Accept, SERVER);
        }
        void Accept(IAsyncResult result)
        {
            Socket handler = SERVER.EndAccept(result);
            handler.BeginReceive(DATA, 0, DATA.Length, SocketFlags.None, Receive, handler);
            SERVER.BeginAccept(Accept, SERVER);
            On_Connected(handler);
        }
        public void Send(Packet packet,Client client)
        {
            client.Socket.Send(Serializator.Serialize<Packet>(packet));
        }
        public void Send(string header, object data, Commands command, Client client)
        {
            Send(new Packet(header, data, command), client);
        }
        public void SendAll(Packet packet)
        {
            byte[] data = Serializator.Serialize<Packet>(packet);
            foreach (Client client in CLIENTS)
                client.Socket.Send(data);
        }
        void Receive(IAsyncResult result)
        {
            Socket handler = result.AsyncState as Socket;
            int recieved=0;
            try
            {
                recieved = handler.EndReceive(result);    
            }
            catch (Exception ex) { Local_Error(ex.Message+" - "+ex.TargetSite); }
            if (recieved > 0)
            {
                On_Received(DATA, handler);
                handler.BeginReceive(DATA, 0, DATA.Length, SocketFlags.None, Receive, handler);
            }

        }
        void Local_Error(string message)
        {
            if(Error!=null)
            Error(message);
        }
        void On_Received(byte[] data,Socket sender)
        {
            //try
            {
                if (OnRecieved != null)
                    OnRecieved(Serializator.Deserialize<Packet>(data), CLIENTS.Find(n => n.Socket.RemoteEndPoint == sender.RemoteEndPoint));
            }
            //catch { Local_Error("!Ошибка приема пакета."); }
        }
        void On_Connected(Socket client)
        {
            Client c = new Client(client.GetHashCode().ToString(), client);
            CLIENTS.Add(c);
            if(OnConnected!=null)
            OnConnected(c);
        }
        void On_Disonnected(Client client)
        {
            CLIENTS.Remove(client);
            if(OnDisconnected!=null)
            OnDisconnected(client);
        }
    }
    public class NET_Server_Broadcast
    {
        Socket Server;
        IPEndPoint EndPoint;
        Thread Broadcast;
        public NET_Server_Broadcast()
        {
            this.Server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            this.EndPoint = new IPEndPoint(IPAddress.Broadcast, 4701);
            this.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
        }
        public void Start_Broadcasting()
        {
            byte[] Message = Encoding.UTF8.GetBytes("This Server Potok");
            this.Broadcast = new Thread(delegate()
                {
                    while (true)
                    {
                        this.Server.SendTo(Message, this.EndPoint); 
                        Thread.Sleep(5000);
                    }
                });
            this.Broadcast.IsBackground = true;
            this.Broadcast.Start();
        }
        public void Stop_Broadcasting()
        {
            if (this.Broadcast != null)
                this.Broadcast.Abort();
            Server.Close();
        }

    }
}
