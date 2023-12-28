using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Server_Application
{
    /// <summary>
    /// Логика взаимодействия для DebugConsole.xaml
    /// </summary>
    public partial class DebugConsole : Window
    {
        Action<string> log;
        public DebugConsole()
        {
            InitializeComponent();
            log = new Action<string>(logger.AppendText);
        }
        public void Add(string text)
        {
            this.Dispatcher.BeginInvoke(log,text + Environment.NewLine);
        }
    }
}
