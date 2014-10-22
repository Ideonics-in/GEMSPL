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

namespace GEMSPL.DashBoard.Feedback
{
    /// <summary>
    /// Interaction logic for InitializationFB.xaml
    /// </summary>
    public partial class InitializationFB : UserControl
    {
        public event EventHandler<CancelEventArgs> CancelEvent;
        public String TagContent { get; set; }
        public InitializationFB(String tag,String header)
        {
            InitializeComponent();
            FeedBackTextBox.Text = header;
            FeedBackProgress.Value = 0;
            
            TagContent = tag;
            DataContext = this;

        }

        public void DisableCancellation()
        {
            CancelButton.Visibility = System.Windows.Visibility.Hidden;
        }

        public void EnableCancellation()
        {
            CancelButton.Visibility = System.Windows.Visibility.Visible;
        }

        public void SetHeader(String header)
        {
            FeedBackTextBox.Text = header;
        }

        public void UpdateHeader(String text)
        {
            FeedBackTextBox.Text += text;
        }

        public void SetTag(String tag)
        {
            TagContent = tag;
            DataContext = null;
            DataContext = this;
        }

        public void Clear()
        {
            FeedBackProgress.Value = 0;
            FeedBackTextBox.Text = "";
            TagContent = "";
            DataContext = this;
        }

        public void ProgessUpdate(double percentage)
        {
            FeedBackProgress.Value = percentage;
        }

        private void CancelButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (CancelEvent != null)
                CancelEvent(this, new CancelEventArgs());
        }
    }

    public class CancelEventArgs : EventArgs
    {

    }
}
