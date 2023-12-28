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
        public Commands Command { get; private set; }
        /// <summary>
        /// Созданный пакет неизменяем!
        /// </summary>
        /// <param name="data">Данные для передачи</param>
        /// <param name="command">Команда сервера</param>
        public Packet(object data, Commands command)
            : this()
        {
            this.Data = data; this.Command = command;
        }
        public override string ToString()
        {
            return string.Format("Пакет содержит {0}, команда {1}", Data, Command.ToString());
        }
    }
    /// <summary>
    /// Набор команд для сервера
    /// </summary>
    public enum Commands : byte 
    {   
        Registration,Reconnect, Ping, Ping_OK, End_Test,Local_State, Pause_Test, Ready,Message ,Restart, Answer ,New_Task, Next
    }
    /// <summary>
    /// Содержит информацию о режиме тестирования
    /// </summary>
    public enum Test_Mode : byte { Default, Username, Password }
    public enum Test_Type : byte { Triggers, NoRAM }  
    [Serializable]
    public struct Registration_Info
    {
        public string Name { get; private set; }
        public string Group { get; private set; }
        public Test_Type Type { get; private set; }
        public Registration_Info(string name, string group, Test_Type type)
            : this()
        {
            this.Name = name; this.Group = group; this.Type = type;
        }

    }
}
