using Aurora.Settings;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Aurora.Controls {
    /// <summary>
    /// Interaction logic for KeyBindList.xaml
    /// </summary>
    public partial class KeyBindList : UserControl {

        public KeyBindList() {
            InitializeComponent();
            ((FrameworkElement)Content).DataContext = this;
        }

        #region Keybinds Property
        public ObservableCollection<Keybind> Keybinds {
            get => (ObservableCollection<Keybind>)GetValue(KeybindsProperty);
            set => SetValue(KeybindsProperty, value);
        }
        public static readonly DependencyProperty KeybindsProperty =
            DependencyProperty.Register("Keybinds", typeof(ObservableCollection<Keybind>), typeof(KeyBindList), new PropertyMetadata(null));
        #endregion

        private void AddNewButton_Click(object sender, RoutedEventArgs e) {
            Keybinds.Add(new Keybind());
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e) {
            Keybinds.Remove((Keybind)((Button)sender).DataContext);
        }
    }
}
