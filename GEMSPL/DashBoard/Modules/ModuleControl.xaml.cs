using GEMSPL.Entity;
using Microsoft.Win32;
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

namespace GEMSPL.DashBoard.Modules
{
    /// <summary>
    /// Interaction logic for ModuleControl.xaml
    /// </summary>
    public partial class ModuleControl : UserControl
    {
        public event EventHandler<SaveEventArgs> SaveEvent;
        public event EventHandler<SaveEventArgs> EditEvent;
        public event EventHandler<CancelEventArgs> CancelEvent;
        String BasePath = String.Empty;
        bool Edit = false;
        public ModuleControl(String basePath)
        {
            InitializeComponent();
            BasePath = basePath;

        }


        public ModuleControl(Module m, String basePath)
        {
            InitializeComponent();
            BasePath = basePath;
            
            LocationTB.Text = m.Location;
            No.Text = m.Details.No;
            RR.Text = m.Details.RR;
            SIM.Text = m.Details.SIM;
            Load.Text = m.Details.Load;
            Edit = true;
            

        }


        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog imagefile = new OpenFileDialog();

            Nullable<bool> result = imagefile.ShowDialog();

            if (result == true)
                Image.Text = imagefile.FileName;




        }

        private void Save_Click_1(object sender, RoutedEventArgs e)
        {
           
            Module m = new Module();
            
            m.Details.No =
            m.Details.RR = RR.Text;
            m.Details.SIM = SIM.Text;
            m.Details.Load = Load.Text;
            m.Details.Image = Image.Text;
            if (Edit)
            {
                m.Location = BasePath.Substring(0,BasePath.LastIndexOf("\\")+1) + "\\" + LocationTB.Text;
                if (EditEvent != null)
                {
                    EditEvent(this, new SaveEventArgs(m));
                }
             
            }
            else
            {
                m.Location = BasePath + "\\" + LocationTB.Text;
                if (Directory.Exists(BasePath + "\\" + LocationTB.Text))
                {
                    MessageBox.Show("Module Already Exists", "Module Control Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    LocationTB.Clear();
                    return;
                }
                if (SaveEvent != null)
                {

                    SaveEvent(this, new SaveEventArgs(m));
                }
            }
        }

        private void Cancel_Click_1(object sender, RoutedEventArgs e)
        {
            if (CancelEvent != null)
                CancelEvent(this, new CancelEventArgs());
        }

        private void UserControl_Loaded_1(object sender, RoutedEventArgs e)
        {
            LocationTB.Focus();
        }
    }

    public class SaveEventArgs : EventArgs
    {
        public Module Module { get; set; }

        public SaveEventArgs(Module m)
        {
            Module = m;
        }
    }

    public class CancelEventArgs : EventArgs
    {

    }
}
