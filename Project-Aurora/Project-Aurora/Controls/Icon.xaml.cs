using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Aurora.Controls {

    /// <summary>
    /// An icon is similar to an image however will fill non-transparent pixels with a given brush. This can be used to change the color
    /// of icons without needing to have them on disk in another color, and also allows themes to control icon colors.
    /// </summary>
    public partial class Icon : UserControl {

        public Icon() {
            InitializeComponent();
            ((FrameworkElement)Content).DataContext = this;
        }

        /// <summary>
        /// The brush that will be used to set the fill color of the icon.
        /// </summary>
        public Brush IconColor {
            get => (Brush)GetValue(IconColorProperty);
            set => SetValue(IconColorProperty, value);
        }

        /// <summary>Identifies the <see cref="IconColor"/> property.</summary>
        public static readonly DependencyProperty IconColorProperty =
            DependencyProperty.Register("IconColor", typeof(Brush), typeof(Icon), new PropertyMetadata(Brushes.Black));

        /// <summary>
        /// The source of the image whose color will be filled.
        /// </summary>
        public ImageSource Source {
            get => (ImageSource)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        /// <summary>Identifies the <see cref="Source"/> property.</summary>
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(ImageSource), typeof(Icon), new PropertyMetadata(null));
    }
}
