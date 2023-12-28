using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace PNetwork
{
    public delegate void Connect_on_server();
    public delegate void Received_from_server(Packet packet);
    public class NET_Client
    {
        public event Connect_on_server OnConnected, OnDisconnected;
        public event Received_from_server OnRecieved;
        public event Error Error;
        Socket CLIENT;
        IPEndPoint EndPoint;
        byte[] DATA;

        /// <summary>
        /// Создает экземпляр клиента
        /// </summary>
        /// <param name="port">Порт сервера</param>
        public NET_Client(IPEndPoint endpoint)
        {
            EndPoint = endpoint;
            CLIENT = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            DATA = new byte[1024];
        }
        /// <summary>
        /// Совершает попытку подключения к серверу
        /// </summary>
        public void Connect()
        {
            CLIENT.Connect(EndPoint);
            CLIENT.BeginReceive(DATA, 0, DATA.Length, SocketFlags.None, Receive, CLIENT);
            On_Connected();
        }
        public void Disconnect() { CLIENT.Disconnect(false); CLIENT.Close(); }
        public void Send(string header, object data, Commands command)
        {
            byte[] sending = Serializator.Serialize<Packet>(new Packet(header, data, command));
            CLIENT.Send(sending);
        }
        void Receive(IAsyncResult result)
        {
            Socket handler = result.AsyncState as Socket;
            try
            {
                int recieved = handler.EndReceive(result);
                if (recieved > 0)
                {
                    On_Received(DATA);
                    handler.BeginReceive(DATA, 0, DATA.Length, SocketFlags.None, Receive, handler);
                }
            }
            catch (Exception ex) { Local_Error(ex.Message); }

        }
        void Local_Error(string message)
        {
            if(Error!=null)
            Error(message);
        }
        void On_Received(byte[] data)
        {
            try { if (OnRecieved != null) 
                OnRecieved(Serializator.Deserialize<Packet>(data)); }
            catch { Local_Error("!Ошибка приема пакета."); }
        }
        void On_Connected()
        {
            if(OnConnected!=null)
            OnConnected();
        }
    }

    public class Receiver
    {
        public static IPEndPoint Receive()
        {
            Socket receiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            EndPoint iep = new IPEndPoint(IPAddress.Any, 4701);
            receiver.Bind(iep);
            byte[] data = new byte[1024];
            receiver.ReceiveFrom(data,ref iep);        
            receiver.Close();
            IPEndPoint endpoint = iep as IPEndPoint;
            endpoint.Port = 4700;
            return endpoint;

        }
    }
}
