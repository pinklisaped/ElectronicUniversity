using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace PNetwork
{
    /// <summary>
    /// Реализует класс пакетов для обмена между клиент-сервером
    /// </summary>
    [Serializable]
    public struct Packet
    {
        public object Data { get; private set; }
        public string Header { get; private set; }
        public Commands Command { get; private set; }
        /// <summary>
        /// Созданный пакет неизменяем!
        /// </summary>
        /// <param name="header">Заголовок пакета, можно использовать null</param>
        /// <param name="data">Данные для передачи</param>
        /// <param name="command">Команда сервера</param>
        public Packet(string header, object data, Commands command)
            : this()
        {
            this.Header = header; this.Data = data; this.Command = command;
        }
        public override string ToString()
        {
            return string.Format("///Заголовок пакета {2}/// Пакет содержит {0}, команда {1}", Data, Command.ToString(), Header);
        }
    }
    /// <summary>
    /// Набор команд для сервера
    /// </summary>
    public enum Commands : byte 
    {   
        Reconnect, End_Test,Local_State, Pause_Test, Ready,Message ,Restart, Answer ,New_Task, Next
    }
    /// <summary>
    /// Содержит информацию о режиме тестирования
    /// </summary>
    public enum Test_Mode:byte { Default, Username, Password }
    /// <summary>
    /// Класс физического клиента, содержит данные о его имени и подключении
    /// </summary>   
    public class Client
    {
        /// <summary>
        /// Отображаемое имя на сервере в списке подключенных
        /// </summary>
        public string Name { get; set; }
        public Socket Socket { get; set; }
        public Client(string name, Socket socket)
        {
            this.Name = name; this.Socket = socket;     
        }
    }
}
