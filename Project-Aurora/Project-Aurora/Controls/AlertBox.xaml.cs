using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Aurora.Controls {
    /// <summary>
    /// Interaction logic for MessageBox.xaml
    /// </summary>
    public partial class AlertBox : UserControl {

        private readonly TaskCompletionSource<int> buttonClickCompletionSource = new TaskCompletionSource<int>();
        private bool isDedicatedWindow = false;

        private AlertBox() {
            InitializeComponent();
            ((FrameworkElement)Content).DataContext = this;
        }

        public string Text {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(AlertBox), new PropertyMetadata(""));

        public string Title {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(AlertBox), new PropertyMetadata(""));

        public AlertBoxIcon Icon {
            get => (AlertBoxIcon)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(AlertBoxIcon), typeof(AlertBox), new PropertyMetadata(AlertBoxIcon.None));


        private void Button_Click(object sender, RoutedEventArgs e) {
            var btn = (Button)sender;
            var stackpanel = VisualTreeHelper.GetParent(btn) as StackPanel;
            buttonClickCompletionSource.SetResult(stackpanel.Children.IndexOf(btn));
            Close();
        }

        private void Close() {
            // If we created a window dedicated for this alert box, set the dialog result (releasing the wait on `ShowDialog`)
            if (isDedicatedWindow && Parent is Window w)
                w.DialogResult = true;

            // Else if it's not a dedicated window, we need to remove it from the panel it is residing in
            else {
                if (VisualTreeHelper.GetParent(this) is Panel p)
                    p.Children.Remove(this);
            }
        }


        public static async Task<int> Show(Window parent, string content, string title, AlertBoxIcon icon, object[] buttons) {
            var msg = new AlertBox { Title = title, Text = content, Icon = icon };

            // Attempt to attach the MessageBox to front content of the targetted window
            if (parent != null && parent.Content is Panel container) container.Children.Add(msg);

            // Attempt to attach MessageBox to the first control's collection (e.g. if the root element is a border with a grid inside, we can attach to grid)
            else if (parent != null && parent.Content is ContentControl c && c.Content is Panel container2) container2.Children.Add(msg);
            else if (parent != null && parent.Content is Decorator d && d.Child is Panel container3) container3.Children.Add(msg);

            // Don't go any deeper, so if we fail that (or no parent is provided), create a separate window for it
            else {
                var w = new Window {
                    ShowInTaskbar = false,
                    WindowStyle = WindowStyle.None,
                    ResizeMode = ResizeMode.NoResize,
                    Content = msg,
                    Width = double.NaN,
                    Height = double.NaN
                };
                msg.isDedicatedWindow = true;
                w.ShowDialog();
            }

            // At this point in the code, the child has been added to the target window, but is not in a custom separate window

            return await msg.buttonClickCompletionSource.Task; // Wait for a buttont to be clicked, and return the result
        }

        public static Task<int> Show(Window parent, string content, string title, AlertBoxIcon icon)
            => Show(parent, content, title, icon, new[] { "Okay" });

        public static Task<int> Show(Window parent, string content, string title)
            => Show(parent, content, title, AlertBoxIcon.None, new[] { "Okay" });

        public static Task<int> Show(string content, string title, AlertBoxIcon icon, object[] buttons)
            => Show(null, content, title, icon, buttons);

        public static Task<int> Show(string content, string title, AlertBoxIcon icon)
            => Show(null, content, title, icon, new[] { "Okay" });

        public static Task<int> Show(string content, string title)
            => Show(null, content, title, AlertBoxIcon.None, new[] { "Okay" });

        private class AlertBoxAwaiter {
        }
    }


    public class AlertBoxIconConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var name = (AlertBoxIcon)value switch {
                AlertBoxIcon.Success => "ok",
                AlertBoxIcon.Info => "info",
                AlertBoxIcon.Question => "help",
                AlertBoxIcon.Warning => "warning",
                AlertBoxIcon.Error => "error",
                _ => ""
            };
            return new System.Windows.Media.Imaging.BitmapImage(new Uri($"/Aurora;component/Resources/UIIcons/{name}-50.png", UriKind.Relative));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public enum AlertBoxIcon {
        None,
        Success,
        Info,
        Question,
        Warning,
        Error
    }
}
