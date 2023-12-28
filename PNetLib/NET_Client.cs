using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Threading;

namespace PNetwork
{
	public delegate void Connect_on_server();
	public delegate void Received_from_server(Packet packet);
	public class NET_Client
	{
		public event Connect_on_server OnConnected, OnDisconnected;
		public event Received_from_server OnRecieved;
		public event Error Error;
		Queue<Packet> Sending = new Queue<Packet>(15);
		Socket CLIENT;
		public bool IsConnected { get; set; }
		public string ID = string.Empty;
		Thread Connected;
		public NET_Client()
		{
			IsConnected = false;
			Pinger = new Thread(Ping) { IsBackground=true};
			Pinger.Start();
		}
		public void Connect()
		{
			try
			{
				CLIENT = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				IPEndPoint end = NET_Receiver_Broadcast.Receive();
				CLIENT.Connect(end);
				byte[] DATA = new byte[1024];
				CLIENT.BeginReceive(DATA, 0, DATA.Length, SocketFlags.None, Receive, DATA);
				On_Connected();
			}
			catch { Connect(); }
		}

		public void Disconnect() { CLIENT.Disconnect(false); CLIENT.Close(); }

		public void Send(Packet packet)
		{
			try
			{
				if (!IsConnected)
				{
					Sending.Enqueue(packet);
					return;
				}
				byte[] sending = Serializator.Serialize<Packet>(packet);
				CLIENT.Send(sending);
			}
			catch
			{
				On_Disconnected();
			}
		}

		public void Send(object data, Commands command)
		{
			Packet packet = new Packet(data, command);
			Send(packet);
		}

		void Receive(IAsyncResult result)
		{
			int recieved = 0;
			Socket handler = CLIENT;
			byte[] DATA = result.AsyncState as byte[],data = new byte[1024];
			try
			{
				recieved = handler.EndReceive(result);
				handler.BeginReceive(data, 0, data.Length, SocketFlags.None, Receive, data);
			}
			catch (Exception ex)
			{
				Local_Error(ex.Message + " - " + ex.TargetSite);
			}
			if (recieved > 0)
				On_Received(DATA);
			else
				On_Disconnected();
		}

		void Local_Error(string message, params object[] args)
		{
			if (Error != null)
				Error(string.Format(message, args));
		}

		void On_Received(byte[] data)
		{
			Packet packet = Serializator.Deserialize<Packet>(data);
			switch (packet.Command)
			{
				case Commands.Registration:
                    ID = packet.Data.ToString();
                    if (OnConnected != null)
                        OnConnected();
					return;
					case Commands.Ping_OK: ConnectedFlag = true; return;
					default: if (OnRecieved != null) OnRecieved(packet); return;
			}

		}

		void On_Connected()
		{
			IsConnected = true;
			if (ID.Length > 0)
				Send(ID, Commands.Registration);
			lock (Sending)
			{
				while (Sending.Count>0)
				{
					Send(Sending.Dequeue());
				}
				Sending.Clear();
			}
		}

		void On_Disconnected()
		{
			if (!IsConnected)
				return;
			IsConnected = false;
			try
			{
				CLIENT.Shutdown(SocketShutdown.Both);
				CLIENT.Disconnect(false);
				CLIENT.Dispose();
			}
			catch { }
			if (OnDisconnected != null)
				OnDisconnected();
			Thread.Sleep(5000);
			Connect();
		}

		bool ConnectedFlag;
		Thread Pinger;
		void Ping()
		{
			Thread.Sleep(30000);
			while (true)
			{
				ConnectedFlag = false;
				Send(null, Commands.Ping);
				Thread.Sleep(30000);
				if (!ConnectedFlag)
				{
					On_Disconnected();
					Thread.Sleep(1000);
				}
			}
		}
	}

	public static class NET_Receiver_Broadcast
	{
		static EndPoint iep = new IPEndPoint(IPAddress.Any, (int)PORTS.BROADCAST_SERVER_PORT);
		static Socket receiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		static NET_Receiver_Broadcast()
		{
			receiver.Bind(iep);
		}

		public static IPEndPoint Receive()
		{
			byte[] data = new byte[1024];
			receiver.ReceiveFrom(data, ref iep);
			string text= Encoding.Default.GetString(data);
			IPEndPoint endpoint = iep as IPEndPoint;
			endpoint.Port = (int)PORTS.MAIN_SERVER_PORT;
			return endpoint;
		}
	}
}
