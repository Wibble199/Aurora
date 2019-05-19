using Aurora.Settings;
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

namespace Aurora.Profiles.Payday_2.Layers
{
    /// <summary>
    /// Interaction logic for Control_CSGOBackgroundLayer.xaml
    /// </summary>
    public partial class Control_PD2BackgroundLayer : UserControl
    {
        public Control_PD2BackgroundLayer()
        {
            InitializeComponent();
        }

        public Control_PD2BackgroundLayer(PD2BackgroundLayerHandler datacontext)
        {
            this.DataContext = datacontext.Properties;
            InitializeComponent();
        }

        private void sldAssaultSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.lblAssaultSpeed.Content = $"x {sldAssaultSpeed.Value.ToString("0.00")}";
        }
    }
}
