using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Aurora.Controls {
    /// <summary>
    /// Class that defines an AlertBox which can be used similar to how <see cref="MessageBox"/> is used.
    /// <para>Provides a dialog box that can have any number of buttons and returns the user's response.</para>
    /// </summary>
    public partial class AlertBox : UserControl {

        /// <summary>Used to create a task that will be completed when the user presses one of the alert's buttons.</summary>
        private readonly TaskCompletionSource<int> buttonClickCompletionSource = new TaskCompletionSource<int>();

        /// <summary>A bool to track whether or not the alert has been created with it's own dedicated window.</summary>
        private bool isDedicatedWindow = false;

        /// <summary>
        /// Creates a new <see cref="AlertBox"/> and sets the DataContext.
        /// </summary>
        private AlertBox() {
            InitializeComponent();
            ((FrameworkElement)Content).DataContext = this;
            (Resources["AnimationIn"] as Storyboard).Begin(this, true);
        }

        #region Properties
        /// <summary>Gets or sets the text that is displayed inside the alert box.</summary>
        public string Text {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(AlertBox), new PropertyMetadata(""));

        /// <summary>Gets or sets the title that is displayed inside the alert box.</summary>
        public string Title {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(AlertBox), new PropertyMetadata(""));

        /// <summary>Gets or sets the icon that is displayed inside the alert box. Setting to <see cref="AlertBoxIcon.None"/> will collapse the icon.</summary>
        public AlertBoxIcon Icon {
            get => (AlertBoxIcon)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(AlertBoxIcon), typeof(AlertBox), new PropertyMetadata(AlertBoxIcon.None));
        #endregion

        /// <summary>
        /// Generic event handler that is assigned to the Click event of all buttons that appear in the alert box.
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e) {
            var btn = (Button)sender;
            var stackpanel = VisualTreeHelper.GetParent(btn) as StackPanel; // Get the parent element
            buttonClickCompletionSource.SetResult(stackpanel.Children.IndexOf(btn)); // Find the clicked button's index in parent
            Close();
        }

        /// <summary>
        /// Causes the alert box to close.
        /// In the case of a dedicated window, will close the window by setting the <see cref="Window.DialogResult"/> to true.
        /// In the case of a mounted alert box, will simply remove the box from the parent window.
        /// </summary>
        private async void Close() {
            // Complete the close animation
            await CloseAnimation();

            // If we created a window dedicated for this alert box, set the dialog result (releasing the wait on `ShowDialog`)
            if (isDedicatedWindow && Parent is Window w)
                w.DialogResult = true;

            // Else if it's not a dedicated window, we need to remove it from the panel it is residing in
            else {
                if (VisualTreeHelper.GetParent(this) is Panel p)
                    p.Children.Remove(this);
            }
        }

        private Task CloseAnimation() {
            var tcs = new TaskCompletionSource<bool>();
            var animationOut = Resources["AnimationOut"] as Storyboard;
            animationOut.Completed += (sender, e) => tcs.SetResult(true);
            animationOut.Begin(this, true);
            return tcs.Task;
        }

        /// <summary>
        /// Core function that attaches a <see cref="AlertBox"/> to the given panel, or creates a new dedicated window if panel is null.
        /// <para>Populates the controls of the alert with the given values.</para>
        /// <para>Returns a task that should be awaited. The task will complete when the user chooses an option and the index of the pressed
        /// button will be the resolution value of the task.</para>
        /// </summary>
        private static Task<int> ShowCore(Panel panel, string content, string title, AlertBoxIcon icon, object[] buttons) {
            // Create the alert
            var msg = new AlertBox { Title = title, Text = content, Icon = icon };

            if (panel != null)
                panel.Children.Add(msg);

            // If not parent panel is provided, create a separate window for the alert
            else {
                var w = new Window {
                    ShowInTaskbar = false,
                    WindowStyle = WindowStyle.ToolWindow,
                    ResizeMode = ResizeMode.NoResize,
                    Content = msg,
                    SizeToContent = SizeToContent.WidthAndHeight
                };
                // Add a close event that checks if the button task is complete, if not it returns -1. This means -1
                // will be returned if there wasn't a button that was clicked.
                w.Closed += (sender, e) => {
                    if (!msg.buttonClickCompletionSource.Task.IsCompleted)
                        msg.buttonClickCompletionSource.SetResult(-1);
                };
                msg.isDedicatedWindow = true;
                w.ShowDialog();
            }

            // At this point in the code, the child has been added to the target window, but is not in a custom separate window

            return msg.buttonClickCompletionSource.Task; // Return the task that will complete when a button is clicked.
        }

        #region Public Show Methods
        /// <summary>
        /// Shows an alert box that will be placed at the root of the given <see cref="Window"/>. Uses the provided content, title, icon and buttons.
        /// Note that the window must have a <see cref="Panel"/> type child (e.g. Grid) at the root or 1-level down. If this is not the case, a new
        /// dedicated window for the alert will be created.
        /// <para>Returns a task that should be awaited. The task will complete when the user chooses an option and the index of the pressed
        /// button will be the resolution value of the task.</para>
        /// </summary>
        public static Task<int> Show(Window parent, string content, string title, AlertBoxIcon icon, object[] buttons) {
            Panel panel = null;

            // Attempt to attach the MessageBox to front content of the targetted window
            if (parent != null && parent.Content is Panel) panel = (Panel)parent.Content;

            // Attempt to attach MessageBox to the first control's collection (e.g. if the root element is a border with a grid inside, we can attach to grid)
            // Do not go any deeper than this otherwise we may end up in a very nested panel
            else if (parent != null && parent.Content is ContentControl c && c.Content is Panel) panel = (Panel)c.Content;
            else if (parent != null && parent.Content is Decorator d && d.Child is Panel) panel = (Panel)d.Child;

            return ShowCore(panel, content, title, icon, buttons);
        }

        /// <summary>
        /// Shows an alert box that will be placed at the root of the given <see cref="Window"/>. Uses the provided content, title and icon and has a single "Okay" button.
        /// <para>Returns a task that should be awaited.</para>
        /// </summary>
        public static Task<int> Show(Window parent, string content, string title, AlertBoxIcon icon)
            => Show(parent, content, title, icon, new[] { "Okay" });

        /// <summary>
        /// Shows an alert box that will be placed at the root of the given <see cref="Window"/>. Uses the provided content and title and has a no icon and a single "Okay" button.
        /// <para>Returns a task that should be awaited.</para>
        /// </summary>
        public static Task<int> Show(Window parent, string content, string title)
            => Show(parent, content, title, AlertBoxIcon.None, new[] { "Okay" });

        /// <summary>
        /// Will attempt to search for the parent <see cref="Window"/> of the given <see cref="DependencyObject"/>. This allows for use of the
        /// <see cref="AlertBox"/> within custom controls that do not normally have direct access to the <see cref="Window" /> reference.
        /// Displays the provided content, title, icon and buttons.
        /// <para>Returns a task that should be awaited. The task will complete when the user chooses an option and the index of the pressed
        /// button will be the resolution value of the task.</para>
        /// </summary>
        public static Task<int> Show(DependencyObject obj, string content, string title, AlertBoxIcon icon, object[] buttons) {
            while (obj != null && !(obj is Window))
                obj = VisualTreeHelper.GetParent(obj);
            return Show(obj as Window, content, title, icon, buttons);
        }

        /// <summary>
        /// Will attempt to search for the parent <see cref="Window"/> of the given <see cref="DependencyObject"/>. This allows for use of the
        /// <see cref="AlertBox"/> within custom controls that do not normally have direct access to the <see cref="Window" /> reference.
        /// Displays the provided content, title and icon. Has a single "Okay" button.
        /// <para>Returns a task that should be awaited.</para>
        /// </summary>
        public static Task<int> Show(DependencyObject obj, string content, string title, AlertBoxIcon icon)
            => Show(obj, content, title, icon, new[] { "Okay" });

        /// <summary>
        /// Will attempt to search for the parent <see cref="Window"/> of the given <see cref="DependencyObject"/>. This allows for use of the
        /// <see cref="AlertBox"/> within custom controls that do not normally have direct access to the <see cref="Window" /> reference.
        /// Displays the provided content and title. Has no icon and a single "Okay" button.
        /// <para>Returns a task that should be awaited.</para>
        /// </summary>
        public static Task<int> Show(DependencyObject obj, string content, string title)
            => Show(obj, content, title, AlertBoxIcon.None, new[] { "Okay" });

        /// <summary>
        /// Shows an alert box that will appear in a dedicated new window.
        /// Displays the provided content, title, icon and buttons.
        /// <para>Returns a task that should be awaited. The task will complete when the user chooses an option and the index of the pressed
        /// button will be the resolution value of the task.</para>
        /// </summary>
        public static Task<int> Show(string content, string title, AlertBoxIcon icon, object[] buttons)
            => Show(null, content, title, icon, buttons);

        /// <summary>
        /// Shows an alert box that will appear in a dedicated new window.
        /// Displays the provided content, title and icon. Has a single "Okay" button.
        /// <para>Returns a task that should be awaited.</para>
        /// </summary>
        public static Task<int> Show(string content, string title, AlertBoxIcon icon)
            => Show(null, content, title, icon, new[] { "Okay" });

        /// <summary>
        /// Shows an alert box that will appear in a dedicated new window.
        /// Displays the provided content and title. Has no icon and a single "Okay" button.
        /// <para>Returns a task that should be awaited.</para>
        /// </summary>
        public static Task<int> Show(string content, string title)
            => Show(null, content, title, AlertBoxIcon.None, new[] { "Okay" });
        #endregion
    }


    /// <summary>
    /// Class that converts a <see cref="AlertBoxIcon"/> value into it's relative icon as a <see cref="BitmapImage"/>.
    /// </summary>
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
            return new BitmapImage(new Uri($"/Aurora;component/Resources/UIIcons/{name}-50.png", UriKind.Relative));
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    /// <summary>
    /// <para>Class that converts a <see cref="AlertBoxIcon"/> value into a <see cref="Visibility"/> value.</para>
    /// Returns <see cref="Visibility.Collapsed"/> if given <see cref="AlertBoxIcon.None"/>, <see cref="Visibility.Visible"/> otherwise.
    /// </summary>
    public class AlertBoxIconToVisibilityConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => ((AlertBoxIcon)value) == AlertBoxIcon.None ? Visibility.Collapsed : Visibility.Visible;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }


    /// <summary>
    /// An enum that contains possible icons that can be displayed by the <see cref="AlertBox"/>.
    /// </summary>
    public enum AlertBoxIcon { None, Success, Info, Question, Warning, Error }
}
