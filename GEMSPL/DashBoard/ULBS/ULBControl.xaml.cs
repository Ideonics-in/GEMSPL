using System;
using System.Collections.Generic;
using System.IO;
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

namespace GEMSPL.DashBoard.ULBS
{
    /// <summary>
    /// Interaction logic for ULBZoneAreaControl.xaml
    /// </summary>
    public partial class ULBControl : UserControl
    {
        public event EventHandler<SaveEventArgs> SaveEvent;
        public event EventHandler<CancelEventArgs> CancelEvent;
        String BasePath = String.Empty;

        public ULBControl(String basePath)
        {
            InitializeComponent();
            BasePath = basePath;

           
        }

        private void Save_Click_1(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(BasePath + "\\" + MainTextBox.Text))
            {
                MessageBox.Show("ULB Already Exists", "ULB Control Info", MessageBoxButton.OK, MessageBoxImage.Information);
                MainTextBox.Clear();
                return;
            }

            if (SaveEvent != null)
                SaveEvent(this, new SaveEventArgs(MainTextBox.Text));
        }

        private void Cancel_Click_1(object sender, RoutedEventArgs e)
        {
            if (CancelEvent != null)
                CancelEvent(this, new CancelEventArgs());
        }

        private void UserControl_Loaded_1(object sender, RoutedEventArgs e)
        {
            MainTextBox.Focus();
        }
    }

    public class SaveEventArgs : EventArgs
    {
        public String ULB { get; set; }

        public SaveEventArgs(String ulb)
        {
            ULB = ulb;
        }
    }

    public class CancelEventArgs : EventArgs
    {

    }
}
