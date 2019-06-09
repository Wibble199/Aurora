using System.Windows;
using System.Windows.Media;

namespace Aurora.Theme.Addons {

    public static class TextboxAddons {
            
        // Icon that appears on the side of the textbox and can be used to indicate the type of data for the textbox
        public static ImageSource GetIcon(DependencyObject obj) => (ImageSource)obj.GetValue(IconProperty);
        public static void SetIcon(DependencyObject obj, ImageSource value) => obj.SetValue(IconProperty, value);

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.RegisterAttached("Icon", typeof(ImageSource), typeof(TextboxAddons), new PropertyMetadata(null));
               

        // Placeholder text that appears when the textbox does not contain any text
        public static string GetPlaceholder(DependencyObject obj) => (string)obj.GetValue(PlaceholderProperty);
        public static void SetPlaceholder(DependencyObject obj, string value) => obj.SetValue(PlaceholderProperty, value);

        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.RegisterAttached("Placeholder", typeof(string), typeof(TextboxAddons), new PropertyMetadata(""));
    }
}
