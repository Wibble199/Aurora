using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Aurora.Settings.Overrides {
    public class EvaluatableDecorator : ContentControl {

        private readonly Border border = new Border {
            Background = Brushes.Red
        };

        public EvaluatableDecorator() {
            base.Content = border;
        }
        
        public new UIElement Content {
            get => (UIElement)GetValue(EvaluatableContentProperty);
            set => SetValue(EvaluatableContentProperty, value);
        }
        public static readonly DependencyProperty EvaluatableContentProperty =
            DependencyProperty.Register("Content", typeof(UIElement), typeof(EvaluatableDecorator), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, ContentChangedHandler));

        public static void ContentChangedHandler(DependencyObject target, DependencyPropertyChangedEventArgs e) {
            ((EvaluatableDecorator)target).border.Child = (UIElement)e.NewValue;
        }
    }
}
