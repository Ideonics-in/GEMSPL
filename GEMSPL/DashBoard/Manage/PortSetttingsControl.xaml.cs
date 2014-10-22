using GEMSPL.Entity;
using System;
using System.Collections.Generic;
using System.IO.Ports;
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

namespace GEMSPL.DashBoard.Manage
{
    /// <summary>
    /// Interaction logic for PortSetttingsControl.xaml
    /// </summary>
    public partial class PortSetttingsControl : UserControl
    {
        public event EventHandler<SaveEventArgs> SaveEvent;
        public event EventHandler<CancelEventArgs> CancelEvent;
        public static String[] BAUD_RATES = { "9600", "19200", "57600", "115200" };
        public static String[] HANDSHAKING = { "None", "RequestToSend" };


        String Port;
        String Baud;
        String Handshake;
        List<String> portList;
        public PortSetttingsControl(PortSettings settings)
        {
            Port = settings.ComPort;
            Baud = settings.BaudRate;
            Handshake = settings.HandShake;
            InitializeComponent();
            portList = SerialPort.GetPortNames().ToList();
            int comIndex = portList.FindIndex(x => x == Port);

            ComPort.ItemsSource = portList;
            ComPort.SelectedIndex = comIndex;

            BaudRate.ItemsSource = BAUD_RATES;
            BaudRate.SelectedIndex = Array.FindIndex(BAUD_RATES, x => x == Baud);

            Handshaking.ItemsSource = HANDSHAKING;
            Handshaking.SelectedIndex = Array.FindIndex(HANDSHAKING, x => x == Handshake);
        }

        private void Cancel_Click_1(object sender, RoutedEventArgs e)
        {
            if (CancelEvent != null)
                CancelEvent(this, new CancelEventArgs());

        }

        private void Save_Click_1(object sender, RoutedEventArgs e)
        {
            if( SaveEvent != null )
                SaveEvent(this, new SaveEventArgs(new PortSettings{ BaudRate= BAUD_RATES[BaudRate.SelectedIndex], ComPort = portList[ComPort.SelectedIndex]
                    , HandShake = HANDSHAKING[Handshaking.SelectedIndex]}));

        }
    }
    public class SaveEventArgs : EventArgs
    {
        public PortSettings Settings { get; set; }

        public SaveEventArgs(PortSettings settings)
        {
            Settings = settings;
        }
    }
}
