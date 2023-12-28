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

namespace Electronic
{
    
    public partial class Registration : Window
    {
        public Registration()
        {
            InitializeComponent();
        }
        PNetwork.Test_Mode Mode;
        PNetwork.Registration_Info Info;
        public PNetwork.Registration_Info ShowDialog(PNetwork.Test_Mode mode)
        {
            if (mode == PNetwork.Test_Mode.Password)
                DEFAULT_PANEL.Visibility = System.Windows.Visibility.Collapsed;
            this.ShowDialog();
            return Info;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (PASSWORD_TEXTBOX.Text.Length > 0)
            {
                Info = new PNetwork.Registration_Info(PASSWORD_TEXTBOX.Text,string.Empty, PNetwork.Test_Type.Triggers);
                this.Close(); return;
            }
            if (NAME_TEXTBOX.Text.Length < 1 || GROUP_TEXTBOX.Text.Length < 1)
                return;
            Info = new PNetwork.Registration_Info(NAME_TEXTBOX.Text, GROUP_TEXTBOX.Text, PNetwork.Test_Type.Triggers);
            this.Close();
        }
    }
}
