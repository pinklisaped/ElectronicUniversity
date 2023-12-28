using System;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ElectronicElements;
using UserElements;
using PNetwork;
using System.Threading.Tasks;
using System.Text;

namespace Electronic
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>

    public enum Testing : byte { Training, Testing, Server_Testing }
    public partial class MainWindow : Window
    {
        Action Compare,End_Test;
        public Testing Testing;
        public Student student;
        TriggerCircuit[] TRIGGERS;
        Timer TEST_TIME;
        int _index=0;
        bool exitflag;
        NET_Client client;
        int CURRENT_TRIGGER
        {
            get { return _index; }
            set
            {
                if (value < TRIGGERS.Length)
                    _index = value;
                else
                    return;
                Next();
            }
        }
        public MainWindow()
        {
            InitializeComponent();  
            Compare = Local_Compared;
            End_Test = Local_End_Test;
            string[] args = Environment.GetCommandLineArgs();
            TRIGGERS = TriggerCircuit.Read();
            if (MessageBox.Show("Запустить режим обучения", "Выбор режима", MessageBoxButton.YesNo) == MessageBoxResult.No)
            //if (args.Length > 1 && args[1].ToLower() == "-test")
            {

                Action timer = delegate { TEST_TIME_VALUE.Value++; };
                Testing = Electronic.Testing.Testing;
                MAINMENUGRID.Visibility = Visibility.Collapsed;
                GETTASKGRID.Visibility = Visibility.Visible;
                TEST_TIME_VALUE.Visibility = Visibility.Visible;

                TEST_TIME = new Timer(1000);
                TEST_TIME.Elapsed += (o, e) => { this.Dispatcher.BeginInvoke(timer, null); };
                System.Threading.Thread loading = new System.Threading.Thread(Loading) { IsBackground = true };
                Timer local_loader = new Timer(30000) { AutoReset = false };
                local_loader.Elapsed += (o, e) =>
                {
                    loading.Abort();
                    this.Dispatcher.BeginInvoke(new Action(Local_Test_Load));
                    local_loader.Dispose();
                };
                this.Loaded += (o, e) =>
                {
                   
                    local_loader.Start();
                    loading.Start(local_loader);
                };
                return;
            }
            this.Loaded += (o, e) =>
            {
                MAINMENUCIRCUITBOX.Circuit = TRIGGERS[0];
                MAINMENULISTBOX.ItemsSource = TRIGGERS;
                MAINMENULISTBOX.SelectionChanged += MAINMENULISTBOX_SelectionChanged;
                student = new Student();
            };
            
        }
        void Loading(object result)
        {
            client = new NET_Client();
            client.Connect();
            Timer local_loader = result as Timer;
            local_loader.Stop(); local_loader.Dispose();
            client.OnRecieved += Data_Received;
            Compare = Online_Compared;
            End_Test = Online_End;
        }
        void Local_Test_Load()
        {
           // string[] args = Environment.GetCommandLineArgs();
            //student = new Student(string.Format("{0} {1} {2}", args[2], args[3], args[4]), args[5]);
            MessageBox.Show("Сервер в локальной сети не найден\nтест будет проведен в оффлайн режиме","Ошибка поиска сервера", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            Registration(Test_Mode.Default);
            student.Statistic = new Statistics(5, TRIGGERS.Length);
            CURRENT_TRIGGER = 0;
            this.GETTASKGRID.IsEnabled = true;
        }
        Registration_Info Registration(Test_Mode mode)
        {
            Registration_Info info=new Registration_Info();
            while (student == null)
               info = new Registration().ShowDialog(mode);
            student = new Student(info.Name,info.Group);
            this.TEST_NAME.Content = student.Name;
            return info;
        }
        #region Events
        private void TEST_TIME_VALUE_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TEST_TIME_VALUE.Value == 0)
            {
                TEST_TIME_LABEL.Content = "Осталось 2 минуты"; return;
            }
            string text = string.Empty;
            if (TEST_TIME_VALUE.Value < 60)
                text = "Осталась 1 минута " + (TEST_TIME_VALUE.Maximum - TEST_TIME_VALUE.Value - 60) + " секунд";
            else
                text = "Осталось " + (TEST_TIME_VALUE.Maximum - TEST_TIME_VALUE.Value) + " секунд";

            if (TEST_TIME_VALUE.Value == TEST_TIME_VALUE.Maximum)
            {
                text = "Время истекло";
                TEST_TIME.Stop();
                Compare();
            }
            TEST_TIME_LABEL.Content = text;
        }
        private void TEST_BUTTON_END_Click(object sender, RoutedEventArgs e)
        {
            End_Test();
        }
        private void Next_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Compare();
        }
        #endregion

        #region LOCAL TEST
        void Next()
        {
            if (_index == TRIGGERS.Length - 1)
                TEST_BUTTON_NEXT.Visibility = Visibility.Collapsed;
            else
                TEST_BUTTON_NEXT.Visibility = Visibility.Visible;
            TEST_CIRCUIT_BOX.Circuit = TRIGGERS[_index];
            TEST_CHALLENGE.Content = string.Format("ЗАДАНИЕ {0}/{1}", _index + 1, TRIGGERS.Length);
            TEST_OUTPUT.Circuit = TRIGGERS[_index];
            TEST_INPUT.Initialize(TRIGGERS[_index].Initial_Value);
            TEST_TIME_VALUE.Value = 0;
            TEST_INPUT.Focus();
        }
        void Local_Compared()
        {          
            bool?[] input_results = TEST_INPUT.States;
            bool[] correct_results = TEST_OUTPUT.Circuit.Output;
            ElectronicCircuit ANSWER = new ElectronicCircuit(new Input[]{new Input("Q",CircuitInputType.direct){ States=correct_results}});
            TEST_OUT_VALID.Circuit = ANSWER;
            TEST_OUT_VALID.Visibility = Visibility.Visible; 
            ushort errors = 0;
            for (ushort i = 0; i < input_results.Length; i++)
                if (correct_results[i] != input_results[i])
                    errors++;
            TRIGGERS[CURRENT_TRIGGER].States = TriggerCircuit.Generate(TEST_OUTPUT.Circuit.Inputs.Length);
            if (Testing == Electronic.Testing.Training&&!exitflag)
            {   
                MessageBoxResult repeat = MessageBox.Show(string.Format("Ваш результат {0} из {1}, повторить?", correct_results.Length - errors, correct_results.Length), "Отчет", MessageBoxButton.YesNo);
                if (repeat == MessageBoxResult.Yes)
                {
                    TEST_OUTPUT.Initialize();
                    TEST_OUT_VALID.Visibility = Visibility.Hidden;
                    TEST_INPUT.Initialize(TRIGGERS[CURRENT_TRIGGER].Initial_Value);
                    TEST_INPUT.Focus();
                    return;
                }
            }
            else
            {
                if (TEST_TIME != null)
                TEST_TIME.Stop();
                MessageBox.Show(string.Format("Ваш результат {0} из {1} - {2:P0}.", correct_results.Length - errors, correct_results.Length, (float)(correct_results.Length - errors) / Params.Steps), "Отчет");
                student.Add_Answer(input_results, correct_results);
                if (TEST_TIME != null)
                TEST_TIME.Start();
            } 
            TEST_OUT_VALID.Visibility = Visibility.Hidden;
            if (CURRENT_TRIGGER == TRIGGERS.Length - 1&&!exitflag)
            {
                End_Test();
                return;
            }
            else if (CURRENT_TRIGGER < 0)
                return;
            CURRENT_TRIGGER++;  
            TEST_INPUT.Initialize(TRIGGERS[CURRENT_TRIGGER].Initial_Value) ;
        }
        void Local_End_Test()
        {
            if (!exitflag)
            {
                exitflag = true;
                Compare();
            }
            exitflag = false;
            TESTGRID.Visibility = Visibility.Collapsed;
            if (Testing == Electronic.Testing.Training)
            {
                MAINMENUGRID.Visibility = Visibility.Visible;
            }
            else
            {
                MessageBox.Show(string.Format("Результат тестирования: {0} (баллы)", student.Statistic.Total_Score), "Отчет");
                Application.Current.Shutdown();
                Cryptography_RSA rsa = new Cryptography_RSA();
                rsa.Load_Key(Encoding.Default.GetString(Properties.Resources._public));
                byte[] save = rsa.Encrypt(Serializator.Serialize(student));
                System.IO.File.WriteAllBytes(student.ID + ".stat", save);
            }
        }
        #endregion

        #region ONLINE TEST

        void Data_Received(Packet packet)
        { 
            switch(packet.Command)
            {
                case Commands.Ready:
                    Test_Mode mode = (Test_Mode)packet.Data;
                    Registration_Info info=(Registration_Info)Dispatcher.Invoke(new Func<Test_Mode,Registration_Info>(Registration),mode);
                    client.Send(info, Commands.Registration); break;break;
                case Commands.Answer: 
                     this.Dispatcher.BeginInvoke(new Action<ElectronicCircuit>(Receive_Result), packet.Data as ElectronicCircuit); 
                     break;
                case Commands.New_Task:
                    Dispatcher.Invoke(new Action<ElectronicCircuit,string>(
                    Online_Next), ((packet.Data as object[])[0] as ElectronicCircuit),(packet.Data as object[])[0].ToString()); break;
                case Commands.End_Test:this.Dispatcher.BeginInvoke(new Action(delegate{MessageBox.Show(this, string.Format("Тест окончен\nрезультат - {0:F1}", packet.Data, 0)); Application.Current.Shutdown();})); break;
            }
        }
        void Online_Compared()
        {
            client.Send(TEST_INPUT.States, Commands.Answer);
            TEST_TIME.Stop();
        }

        void Receive_Result(ElectronicCircuit c)
        {
            TEST_OUT_VALID.Circuit = c;
            TEST_OUT_VALID.Visibility = System.Windows.Visibility.Visible;
            ushort result = 0;
            for (int i = 0; i < TEST_INPUT.States.Length; i++)
                if (TEST_INPUT.States[i] == TEST_OUT_VALID.Circuit.Inputs[0].States[i])
                    result++;
            MessageBox.Show(string.Format("Ваш результат {0} из {1} - {2:P0}.", result, TEST_INPUT.States.Length, (float)(result) / Params.Steps), "Отчет");
            client.Send(null, Commands.Next);
        }
        void Online_Next(ElectronicCircuit c, string s)
        {
            if(!this.GETTASKGRID.IsEnabled)
                this.GETTASKGRID.IsEnabled = true;
            TEST_CHALLENGE.Content = s;
            TEST_OUT_VALID.Visibility = System.Windows.Visibility.Hidden;
            TEST_OUTPUT.Circuit = c;
            TEST_CIRCUIT_BOX.Circuit = c;
            TEST_INPUT.Initialize(c.Initial_Value);
            TEST_TIME_VALUE.Value = 0;
            if(TESTGRID.Visibility==Visibility.Visible)
            TEST_TIME.Start();
            TEST_INPUT.Focus();
        }

        void Online_End()
        {
            client.Send(TEST_INPUT.States, Commands.End_Test);
        }
        #endregion

        #region MAIN MENU
        void MAINMENULISTBOX_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MAINMENULISTBOX.SelectedIndex == -1)
                return;
            TESTGRID.Visibility = Visibility.Visible;      
            MAINMENUGRID.Visibility = Visibility.Collapsed;
            CURRENT_TRIGGER = MAINMENULISTBOX.SelectedIndex;
            MAINMENULISTBOX.SelectedIndex = -1;
        }
        void listBox1_ListBoxItem_MouseEnter(object sender, MouseEventArgs e)
        {
            MAINMENUCIRCUITBOX.Circuit = (TriggerCircuit)(sender as ListBoxItem).Content;
        }
        private void MAINMENU_BUTTON_BACK_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        } 
        void Start_Click(object sender, RoutedEventArgs e)
        {
            //WindowStyle = System.Windows.WindowStyle.None;
            //WindowState = System.Windows.WindowState.Maximized;     
            //this.Topmost = true;
            GETTASKGRID.Visibility = Visibility.Collapsed;           
            TESTGRID.Visibility = Visibility.Visible;
            TEST_INPUT.Focus();
            TEST_TIME.Start();
        }
        #endregion

    }

    public class Local_Commands 
    {
        public static RoutedUICommand Start { get; private set; }
        public static RoutedUICommand Next { get; private set; }
        public static RoutedUICommand Exit { get; private set; }
        static Local_Commands()
        {
            Start = new RoutedUICommand();
            Next = new RoutedUICommand("Далее", "NEXT", typeof(Local_Commands), new InputGestureCollection() { new KeyGesture(Key.Enter) });
            Exit = new RoutedUICommand("Выход", "EXIT", typeof(Local_Commands), new InputGestureCollection() { new KeyGesture(Key.F4, ModifierKeys.Alt) });
        }
    }
}
