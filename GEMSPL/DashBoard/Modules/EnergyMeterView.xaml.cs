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

namespace GEMSPL.DashBoard.Modules
{
    /// <summary>
    /// Interaction logic for EnergyMeterView.xaml
    /// </summary>
    public partial class EnergyMeterView : UserControl
    {

        public event EventHandler<CancelEventArgs> CancelEvent;

        public EnergyMeterView(ICollection<EnergyParameter> parameters)
        {
            InitializeComponent();
            this.DataContext = parameters;
        }
        private void Cancel_Click_1(object sender, RoutedEventArgs e)
        {
            if (CancelEvent != null)
                CancelEvent(this, new CancelEventArgs());

        }
    }

    public class EnergyParameter
    {
        public String Name { get; set; }
        public String Value { get; set; }

        public EnergyParameter(String name, String value)
        {
            Name = name;
            Value = value;
        }


    }

}
