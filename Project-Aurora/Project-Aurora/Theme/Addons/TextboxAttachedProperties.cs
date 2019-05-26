using System.Windows;

namespace Aurora.Theme.Addons {

    public static class TextboxAddons {
            
        // Icon that appears on the side of the textbox and can be used to indicate the type of data for the textbox
        public static string GetIcon(DependencyObject obj) => (string)obj.GetValue(IconProperty);
        public static void SetIcon(DependencyObject obj, string value) => obj.SetValue(IconProperty, value);

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.RegisterAttached("Icon", typeof(string), typeof(TextboxAddons), new PropertyMetadata(null));
               

        // Placeholder text that appears when the textbox does not contain any text
        public static string GetPlaceholder(DependencyObject obj) => (string)obj.GetValue(PlaceholderProperty);
        public static void SetPlaceholder(DependencyObject obj, string value) => obj.SetValue(PlaceholderProperty, value);

        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.RegisterAttached("Placeholder", typeof(string), typeof(TextboxAddons), new PropertyMetadata(""));
    }
}
