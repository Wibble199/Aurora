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

        /// <summary>A collection of buttons to display on the alert box.</summary>
        public string[] Buttons {
            get => (string[])GetValue(ButtonsProperty);
            set => SetValue(ButtonsProperty, value);
        }
        public static readonly DependencyProperty ButtonsProperty =
            DependencyProperty.Register("Buttons", typeof(string[]), typeof(AlertBox), new PropertyMetadata(new[] { "Okay" }));

        /// <summary>Gets or sets the icon that is displayed inside the alert box. Setting to <see cref="AlertBoxIcon.None"/> will collapse the icon.</summary>
        public AlertBoxIcon Icon {
            get => (AlertBoxIcon)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(AlertBoxIcon), typeof(AlertBox), new PropertyMetadata(AlertBoxIcon.None));

        /// <summary>Indicates whether or not the messagebox has a close button and whether a click on the backdrop will close it.
        /// Has no effect on alerts inside a dedicated window.</summary>
        public bool AllowClose {
            get => (bool)GetValue(AllowCloseProperty);
            set => SetValue(AllowCloseProperty, value);
        }
        public static readonly DependencyProperty AllowCloseProperty =
            DependencyProperty.Register("AllowClose", typeof(bool), typeof(AlertBox), new PropertyMetadata(true));


        #endregion

        /// <summary>
        /// When the control is loaded, we play the animation.
        /// </summary>
        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            // If the window is not in it's own dedicated one (i.e. it's been attached to an existing window), play an animation.
            // We don't play one in a dedicated window since it looks weird: the window appears immediately and then the contents fade in.
            if (!isDedicatedWindow)
                (Resources["AnimationIn"] as Storyboard).Begin(this, true);
        }

        /// <summary>
        /// Generic event handler that is assigned to the Click event of all buttons that appear in the alert box.
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e) {
            var btn = (Button)sender;
            var buttonContainer = VisualTreeHelper.GetParent(btn) as ContentPresenter; // Get the element that wraps the button
            var stackpanel = VisualTreeHelper.GetParent(buttonContainer) as StackPanel; // Get the wrapper's parent element
            Close(stackpanel.Children.IndexOf(buttonContainer)); // Find the clicked button's index (wrapper's index in parent)
        }

        /// <summary>
        /// Closes the button when the backdrop or small 'X' button is pressed. Results in a -1 result from the Task.
        /// </summary>
        private void Backdrop_Click(object sender, RoutedEventArgs e) {
            if (AllowClose)
                Close(-1);
        }

        /// <summary>
        /// Causes the alert box to close.
        /// In the case of a dedicated window, will close the window by setting the <see cref="Window.DialogResult"/> to true.
        /// In the case of a mounted alert box, will simply remove the box from the parent window.
        /// </summary>
        private async void Close(int result) {
            buttonClickCompletionSource.SetResult(result);

            // If we created a window dedicated for this alert box, set the dialog result (releasing the wait on `ShowDialog`)
            if (isDedicatedWindow && Parent is Window w)
                w.DialogResult = true;

            // Else if it's not a dedicated window, we need to remove it from the panel it is residing in
            else {
                // Complete the close animation
                await PlayCloseAnimation();

                // Then remove the AlertBox from the visual tree
                if (VisualTreeHelper.GetParent(this) is Panel p)
                    p.Children.Remove(this);

                // In case that didn't work for some reason, ensure that the box doesn't block mouse clicks
                IsHitTestVisible = false;
            }
        }

        /// <summary>
        /// Plays the close animation. Returns a <see cref="Task"/> that will complete when the animation is finished playing.
        /// </summary>
        private Task PlayCloseAnimation() {
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
        private static Task<int> ShowCore(Panel panel, string content, string title, string[] buttons, AlertBoxIcon icon, bool allowClose) {
            // Default buttons
            buttons ??= new[] { "Okay" };

            // Create the alert
            var msg = new AlertBox { Title = title, Text = content, Buttons = buttons, Icon = icon, AllowClose = allowClose };

            // If a panel is provided, add the alert to the panel
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

            // Return the task that will complete when a button is clicked.
            return msg.buttonClickCompletionSource.Task;
        }

        #region Public Show Methods
        /// <summary>
        /// Shows an alert box that will be placed at the root of the given <see cref="Window"/>. Uses the provided content, title, icon and buttons.
        /// Note that the window must have a <see cref="Panel"/> type child (e.g. Grid) at the root or 1-level down. If this is not the case, a new
        /// dedicated window for the alert will be created.
        /// <para>Returns a task that should be awaited. The task will complete when the user chooses an option and the index of the pressed
        /// button will be the resolution value of the task. Will return -1 if the alert was closed without choosing an option.</para>
        /// </summary>
        public static Task<int> Show(Window parent, string content, string title, string[] buttons = null, AlertBoxIcon icon = AlertBoxIcon.None, bool allowClose = true) {
            Panel panel = null;

            // Attempt to attach the MessageBox to front content of the targetted window
            if (parent != null && parent.Content is Panel) panel = (Panel)parent.Content;

            // Attempt to attach MessageBox to the first control's collection (e.g. if the root element is a border with a grid inside, we can attach to grid)
            // Do not go any deeper than this otherwise we may end up in a very nested panel
            else if (parent != null && parent.Content is ContentControl c && c.Content is Panel) panel = (Panel)c.Content;
            else if (parent != null && parent.Content is Decorator d && d.Child is Panel) panel = (Panel)d.Child;

            return ShowCore(panel, content, title, buttons, icon, allowClose);
        }

        /// <summary>
        /// Will attempt to search for the parent <see cref="Window"/> of the given <see cref="DependencyObject"/>. This allows for use of the
        /// <see cref="AlertBox"/> within custom controls that do not normally have direct access to the <see cref="Window" /> reference.
        /// <para>Returns a task that should be awaited. The task will complete when the user chooses an option and the index of the pressed
        /// button will be the resolution value of the task. Will return -1 if the alert was closed without choosing an option.</para>
        /// </summary>
        public static Task<int> Show(DependencyObject obj, string content, string title, string[] buttons = null, AlertBoxIcon icon = AlertBoxIcon.None, bool allowClose = true) {
            while (obj != null && !(obj is Window))
                obj = VisualTreeHelper.GetParent(obj);
            return Show(obj as Window, content, title, buttons, icon, allowClose);
        }

        /// <summary>
        /// Shows an alert box that will appear in a dedicated new window.
        /// <para>Returns a task that should be awaited. The task will complete when the user chooses an option and the index of the pressed
        /// button will be the resolution value of the task. Will return -1 if the alert was closed without choosing an option.</para>
        /// </summary>
        public static Task<int> Show(string content, string title, string[] buttons = null, AlertBoxIcon icon = AlertBoxIcon.None, bool allowClose = true)
            => ShowCore(null, content, title, buttons, icon, allowClose);
        #endregion
        
        #region Preset Show Methods
        /// <summary>
        /// Helper method that uses the `AlertBox.Show` method to show a delete window, asking the user if they want to delete a certain item.
        /// </summary>
        /// <param name="itemType">The type of item to delete. E.G. "layer".</param>
        /// <param name="itemName">An identifying name of the item to delete. E.G. "My Layer".</param>
        public async static Task<bool> ShowDelete(Window wnd, string itemType, string itemName, bool allowClose = true) =>
            (await Show(wnd, $"Are you sure you wish to delete {itemType} '{itemName}'? You cannot undo this action.", $"Delete {itemType}?", new[] { "Don't delete", "Delete" }, AlertBoxIcon.Delete, allowClose)) == 1;
        public async static Task<bool> ShowDelete(DependencyObject obj, string itemType, string itemName, bool allowClose = true) =>
            (await Show(obj, $"Are you sure you wish to delete {itemType} '{itemName}'? You cannot undo this action.", $"Delete {itemType}?", new[] { "Don't delete", "Delete" }, AlertBoxIcon.Delete, allowClose)) == 1;
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
                AlertBoxIcon.Delete => "trash-can",
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
    public enum AlertBoxIcon { None, Success, Info, Question, Warning, Error, Delete }
}
