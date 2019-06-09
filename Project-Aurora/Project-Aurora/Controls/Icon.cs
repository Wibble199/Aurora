using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Aurora.Controls {

    /// <summary>
    /// An icon is similar to an image however will fill non-transparent pixels with the foreground brush. This can be used to change the
    /// color of icons without needing to have them on disk in another color, and also allows themes to control icon colors.
    /// </summary>
    public class Icon : Control {

        static Icon() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Icon), new FrameworkPropertyMetadata(typeof(Icon)));
        }

        /// <summary>
        /// The source of the image whose color will be filled.
        /// </summary>
        public ImageSource Source {
            get => (ImageSource)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(ImageSource), typeof(Icon), new PropertyMetadata(null));
    }
}
