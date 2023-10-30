using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFapp
{
    public partial class MainWindow : Window
    {
        TelegramClient client;

        public MainWindow()
        {
            InitializeComponent();

            client = new TelegramClient(this);

            logList.ItemsSource = client.botUpdates;
        }
        
        private void btnMsgSend_Click(object sender, RoutedEventArgs e)
        {
            client.SendMessage(txtMsgSend.Text, TargetSend.Text);
        }

        private void btnMsgAllSend_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in client.botIds)
            {
                client.SendMessage(txtMsgSend.Text, item.ToString());
            }
        }

        private void btnMsgAllClear_Click(object sender, RoutedEventArgs e)
        {
            client.botUpdates.Clear();

        }
    }  
}
