using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using UserElements;

namespace PNetwork
{
	public enum PORTS:int{MAIN_SERVER_PORT=9800,BROADCAST_SERVER_PORT=9801}
	public delegate void Received(Packet packet, Student sender);
	public delegate void Error(string message);
	public delegate void Connections(Student Student);
	public class NET_Server
	{
		public event Connections OnConnected, OnDisconnected;
		public event Received OnRecieved;
		public event Error Error;
		Socket SERVER;
		StudentsList CLIENTS;
		IPEndPoint ADR;
		public bool? FullLog = true;

        public NET_Server(StudentsList Students, int port = (int)PORTS.MAIN_SERVER_PORT)
		{                                
			CLIENTS = Students;
            NET_Server_Broadcast nsb = new NET_Server_Broadcast();
            nsb.Start_Broadcasting();
			SERVER = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			ADR = new IPEndPoint(IPAddress.Any, port);
		}

		public void Listen()
		{
			SERVER.Bind(ADR);
			SERVER.Listen(64);
			SERVER.BeginAccept(Accept, SERVER);
		}

		void Accept(IAsyncResult result)
		{
			byte[] DATA = new byte[1024];
			Socket handler = SERVER.EndAccept(result);
            CLIENTS.Add(new Student() { Socket = handler });

			handler.BeginReceive(DATA, 0, DATA.Length, SocketFlags.None, Receive, new object[] { handler, DATA });
			SERVER.BeginAccept(Accept, SERVER);
		}
		public void Send(Packet packet, Socket Student,bool log=false)
		{
			try
			{
				if (Student != null && Student.RemoteEndPoint != null)
				{
					byte[] data = Serializator.Serialize<Packet>(packet);
					Student.Send(data);
				}
			}
			catch (Exception ex)
			{
				Local_Error(ex.Message + " - " + ex.TargetSite);
				On_Disconnected(Student);
			}
		}
		public void Send(Packet packet, Student Student)
		{
			if (Student == null||Student.Socket==null)
				return;
			Send(packet,Student.Socket);
		}

		public void Send(object data, Commands command, Student Student)
		{
			Send(new Packet(data, command), Student);
		}

		public void SendAll(object data, Commands command)
		{
			SendAll(new Packet(data, command));
		}

		public void SendAll(Packet packet)
		{
			foreach (Student Student in CLIENTS)
				Send(packet, Student);
		}

		void Receive(IAsyncResult result)
		{
			byte[] DATA = ((object[])result.AsyncState)[1] as byte[];
			int recieved = 0;
			Socket handler = ((object[])result.AsyncState)[0] as Socket;
			try
			{
				recieved = handler.EndReceive(result);
				handler.BeginReceive(DATA, 0, DATA.Length, SocketFlags.None, Receive, result.AsyncState);
			}
			catch (Exception ex) { Local_Error(ex.Message + " - " + ex.StackTrace); }
			Thread.Sleep(2);
			if (recieved > 0)
				On_Received(DATA, handler);
			else
				On_Disconnected(handler);

		}

		void Local_Error(string message, params object[] args)
		{
			if (Error != null)
				Error(string.Format(message, args));
		}

		void On_Received(byte[] data, Socket sender)
		{
            Student Student = CLIENTS.FirstOrDefault(n => n.Socket == sender);
			try
			{
				Packet packet = Serializator.Deserialize<Packet>(data);
				switch (packet.Command)
				{
						case Commands.Registration: On_Connected(packet, sender); break;
						case Commands.Ping: Send(new Packet(null, Commands.Ping), sender,true); break;
						default: if (OnRecieved != null) OnRecieved(packet, Student); return;
				}
			}
			catch
			{
			}

		}

		void On_Connected(Packet packet, Socket sender)
		{
            Student current_student = null;
            Registration_Info information = (Registration_Info)packet.Data;
            switch (Params.Mode)
            {
                case Test_Mode.Default:
                    current_student = new Student(information.Name,information.Group,information.Type) { Status = StatusList.Тестируется };
                    CLIENTS.Add(current_student);
                    //Dispatcher.Invoke(new Action<Student>(Students.Add), current_student); 
                    break;
                case Test_Mode.Username: current_student = CLIENTS.FirstOrDefault(e => e.Name == information.Name && e.Group == information.Group);
                    if (current_student == null)
                        return;
                    current_student.Socket = sender; break;
                case Test_Mode.Password: current_student = CLIENTS.FirstOrDefault(e => e.ID == (packet.Data as string));
                    if (current_student == null)
                        return;
                    current_student.Socket = sender; break;
            }
            if (current_student!=null&&current_student.Socket != null)
                On_Disconnected(sender);
            current_student = CLIENTS.FirstOrDefault(c => c.ID == (string)packet.Data);
            current_student.Socket = sender;
            Send(current_student.ID, Commands.Registration, current_student);
            if (current_student.Socket == null)
				return;
			if (OnConnected != null)
                OnConnected(current_student);
		}


		void On_Disconnected(Student Student)
		{
			if (Student.Socket == null)
				return;
			try
			{
				Student.Socket.Shutdown(SocketShutdown.Both);
				Student.Socket.Disconnect(false);
				Student.Socket.Dispose();
				Student.Socket = null;
			}
			catch { }
			if (OnDisconnected != null)
				OnDisconnected(Student);
		}

		void On_Disconnected(Socket sender)
		{
            //if(ConnectionList.Contains(sender))
            //    ConnectionList.Remove(sender);
			Student Student = CLIENTS.FirstOrDefault(c => sender.Equals(c.Socket));
			if (Student != null)
			{
				On_Disconnected(Student);
				return;
			}
			try
			{
				sender.Shutdown(SocketShutdown.Both);
				sender.Disconnect(false);
				sender.Dispose();
				sender = null;
			}
			catch { }
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
			this.EndPoint = new IPEndPoint(IPAddress.Broadcast, (int)PORTS.BROADCAST_SERVER_PORT);
			this.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
		}
		public void Start_Broadcasting()
		{
			byte[] Message = Encoding.UTF8.GetBytes("ELECTRONIC BROADCAST SERVER");
			this.Broadcast = new Thread(
				delegate()
				{
					while (true)
					{
						this.Server.SendTo(Message, this.EndPoint);
						Thread.Sleep(5000);
					}
				});
			this.Broadcast.IsBackground=true;
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
