using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Aurora.Controls {

    public partial class LoadingSpinner : UserControl {
        public LoadingSpinner() {
            InitializeComponent();
            ((FrameworkElement)Content).DataContext = this;
            SpinnerColor ??= (Brush)FindResource("BaseTextBrush");
        }

        public Brush SpinnerColor {
            get => (Brush)GetValue(SpinnerColorProperty);
            set => SetValue(SpinnerColorProperty, value);
        }
        public static readonly DependencyProperty SpinnerColorProperty =
            DependencyProperty.Register("SpinnerColor", typeof(Brush), typeof(LoadingSpinner), new PropertyMetadata(null));
    }
}
