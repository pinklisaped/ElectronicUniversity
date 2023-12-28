using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using PNetwork;
using UserElements;
using ElectronicElements;
using Microsoft.Win32;
using System.IO;
using System.Collections.Generic;

namespace Server_Application
{
    public partial class MainWindow : Window
    {
        //DebugConsole console = new DebugConsole();
        bool Test_State = true;
        NET_Server Server; 
        NET_Server_Broadcast Broadcast_Server = new NET_Server_Broadcast();
        TriggerCircuit[] Trigger_List;
        StudentsList Students;
        public MainWindow()
        {
            InitializeComponent();
            //new Settings().ShowDialog();
            this.Students = ((StudentsList)TABLE.Resources["Students"]);
            Server = new NET_Server(Students);    
            this.Server.Listen();
            this.Students.Students_Changed += (o) => { TEST_END.Text = string.Format("Завершено {0}/{1}",Students.END_TEST,Students.Count); };
            this.Server.OnRecieved += Server_OnRecieved;
            this.Server.OnConnected += Server_OnConnected;
            //Server.Error += (e) => { console.Add(e); };   
            this.Loaded += (o, e) =>
            {
                Trigger_List = TriggerCircuit.Read();
                //console.Owner = this;
                //console.Show();
            };
            TABLE.Drop += TABLE_Drop;
        }

        void Server_OnConnected(Student client)
        {
            Server.Send(Params.Mode, Commands.Ready, client);
        }

        void Server_OnRecieved(Packet packet, Student sender)
        {
            //console.Add(packet.ToString());
            if (sender.Status == StatusList.Удалён)
                return;
            switch (packet.Command)
            {
                case Commands.Answer:
                    this.Server.Send(new ElectronicCircuit(new Input[] { new Input("Q", CircuitInputType.direct) { States = sender.Statistic.Last_Answer.True_Answer } }), Commands.Answer, sender);
                    this.Dispatcher.BeginInvoke(new Action<bool?[]>(sender.Add_Answer), packet.Data as bool?[]);
                    break;
                case Commands.End_Test: this.Server.Send(sender.Total_Score, Commands.End_Test, sender); sender.Status = StatusList.Завершил; break;
                case Commands.Next:
                    if (!Test_State)
                        return;
                    if (sender.Status == StatusList.Завершил)
                    {
                        this.Server.Send(sender.Total_Score, Commands.End_Test, sender); return;
                    }
                    TriggerCircuit temp = Trigger_List[sender.Current_Task - 1];
                    sender.Add_Answer(temp.Output);
                    this.Server.Send(new object[]{ temp.Cut(),string.Format("Задание {0}/{1}", sender.Current_Task, Trigger_List.Length)}, Commands.New_Task, sender);
                    temp.States = TriggerCircuit.Generate(temp.Inputs.Length); break;
            }
        }    

        void TABLE_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                Open_Statistics(files[0]);
            }
        }

        #region Commands

        OpenFileDialog opendialog = new OpenFileDialog() { Filter = "Файлы статистики|*.stat|Все файлы|*.*" };
        SaveFileDialog savedialog = new SaveFileDialog() { Filter = "Файлы статистики|*.stat|Все файлы|*.*" };

        #region Statistic Commands
        private void Open_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            bool? result = opendialog.ShowDialog();
            if (result == false)
                return;
            Open_Statistics(opendialog.FileName);
        }
        void Open_Statistics(string path)
        {
           
            Cryptography_RSA rsa = new Cryptography_RSA();
            rsa.Load_Key(Load_Key());
            try
            {
                try
                {
                    byte[] data = File.ReadAllBytes(path);
                    Student[] temp = Serializator.Deserialize<Student[]>(rsa.Decrypt(data)) as Student[];
                    foreach (Student st in temp)
                        Students.Add(st);
                }
                catch
                {
                    byte[] data = File.ReadAllBytes(path);
                    Students.Add(Serializator.Deserialize<Student>(rsa.Decrypt(data)));
                }
            }
            catch { MessageBox.Show("Файл статистики не содержит данных или поврежден"); }
        }
        private void Save_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {

            if (savedialog.FileName.Length > 1)
            {
                Write_Statistics();
            }
            else
                SaveAs_Executed(sender, e);

        }
        private void SaveAs_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            bool? result = savedialog.ShowDialog();
            if (result == true)
                Write_Statistics();
        }

        private void Write_Statistics()
        {
            Cryptography_RSA rsa = new Cryptography_RSA();
            string key = Load_Key();
            if (key == null)
                return;
            rsa.Load_Key(key);
            byte[] data = Serializator.Serialize<Student[]>(Students.ToArray());
            data = rsa.Encrypt(data);
            File.WriteAllBytes(savedialog.FileName, data);
        }
        private string Load_Key()
        {
            string key = "private.key";
            if (!File.Exists(key))
            {
                if (MessageBox.Show("Ключ не найден, хотите указать путь?", "Отсутствует ключ", MessageBoxButton.YesNo) == MessageBoxResult.No)
                    return null;
                else
                {
                    OpenFileDialog ofd = new OpenFileDialog() { Filter = "Файлы ключа|*.key|Все файлы|*.*", Multiselect = false };
                    if (ofd.ShowDialog() == true)
                        key = ofd.FileName;
                    else return null;
                }
            }
            return File.ReadAllText(key);
        }
        #endregion

        #region Group Commands
        private void Open_Group_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string temp = opendialog.Filter;
            opendialog.Filter = "Текстовые файлы|*.txt|Все файлы|*.*";
            bool? dialogresult = opendialog.ShowDialog();
            opendialog.Filter = temp;
            if (dialogresult == false) return; 
            string[] students = File.ReadAllLines(opendialog.FileName, Encoding.Default);
            foreach(string student in students)
            {
                string[] name = student.Split('|');
                Students.Add(new Student(name[0], name[1]) { Status = StatusList.Ожидает });
            }
        }

        private void OpenPass_Group_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //Random rnd = new Random();
            //int size = rnd.Next(4, 12);
            //bool[][] ans = new bool[2][];
            //ans[0] = new bool[size];
            //ans[1] = new bool[size];
            //for (int i = 0; i < size; i++)
            //{
            //    ans[0][i] = rnd.Next(0, 10) > 5 ? true : false;
            //    ans[1][i] = rnd.Next(0, 10) > 5 ? true : false;
            //}
            //Students[0].Add_Answer(ans);
        }
        private void Clear_Group_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter.ToString() == "One")
                (TABLE.SelectedItem as Student).Status = StatusList.Удалён;
            else
                Students.Clear();
        }  
        #endregion

        #endregion
    }
    class Local_Commands
    {
        public static RoutedUICommand Open { get; private set; }
        public static RoutedUICommand OpenPasswords { get; private set; }
        public static RoutedUICommand Clear { get; private set; }

        static Local_Commands()
        {
            InputGestureCollection igc = new InputGestureCollection(){new KeyGesture(Key.G, ModifierKeys.Control| ModifierKeys.Shift,"Ctrl+Shift+G")};
            Open = new RoutedUICommand("Открыть", "Open", typeof(Local_Commands),igc);
            igc = new InputGestureCollection() { new KeyGesture(Key.P, ModifierKeys.Control | ModifierKeys.Shift, "Ctrl+Shift+P") };
            OpenPasswords = new RoutedUICommand("Открыть с паролями", "OpenPass", typeof(Local_Commands), igc);
            Clear = new RoutedUICommand("Очистить", "Clear", typeof(Local_Commands));
        }

    }
}
