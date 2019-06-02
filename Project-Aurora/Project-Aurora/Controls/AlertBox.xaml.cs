using Aurora.Settings.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
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

        /// <summary>
        /// Creates a new <see cref="AlertBox"/> and sets the DataContext.
        /// </summary>
        private AlertBox() {
            InitializeComponent();
            ((FrameworkElement)base.Content).DataContext = this;

            WeakEventManager<AlertBox, KeyEventArgs>.AddHandler(this, "KeyDown", UserControl_KeyDown);
        }

        #region Properties
        /// <summary>Sets the content of the AlertBox to be the given content.</summary>
        public new object Content {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }
        public static new readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof(object), typeof(AlertBox), new FrameworkPropertyMetadata(null, coerceValueCallback: CoerceContentCallback));

        /// <summary>Method to coerce arrays. If an array is passed, a ItemsControl is used inside the ContentControl, which will render any UIElements or text instead.
        /// Without this, if an array was passed, the ContentControl would display something such as the literal string "object[]" instead of the objects themselves.</summary>
        public static object CoerceContentCallback(DependencyObject _, object value) =>
            value.GetType().IsArray ? new ItemsControl { ItemsSource = (Array)value, HorizontalAlignment = HorizontalAlignment.Stretch } : value;

        /// <summary>Gets or sets the title that is displayed inside the alert box.</summary>
        public string Title {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(AlertBox), new PropertyMetadata(""));

        /// <summary>A collection of buttons to display on the alert box.</summary>
        public IEnumerable<ChoiceButton> Buttons {
            get => (IEnumerable<ChoiceButton>)GetValue(ButtonsProperty);
            set => SetValue(ButtonsProperty, value);
        }
        public static readonly DependencyProperty ButtonsProperty =
            DependencyProperty.Register("Buttons", typeof(IEnumerable<ChoiceButton>), typeof(AlertBox), new PropertyMetadata(new[] { new ChoiceButton("Okay") }));

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

        /// <summary>Sets the width of the AlertBox.</summary>
        /// <remarks>This hides the normal width property, as if that's changed it affects the size of the backdrop - not what is wanted.</remarks>
        public new double Width {
            get => (double)GetValue(WidthProperty);
            set => SetValue(WidthProperty, value);
        }
        public static new readonly DependencyProperty WidthProperty =
            DependencyProperty.Register("Width", typeof(double), typeof(AlertBox), new PropertyMetadata(double.NaN));

        /// <summary>Indicates to the AlertBox whether it is running inside a dedicated window or as a child of another window.</summary>
        public bool IsDedicatedWindow {
            get => (bool)GetValue(IsDedicatedWindowProperty);
            set => SetValue(IsDedicatedWindowProperty, value);
        }
        public static readonly DependencyProperty IsDedicatedWindowProperty =
            DependencyProperty.Register("IsDedicatedWindow", typeof(bool), typeof(AlertBox), new PropertyMetadata(false));
        #endregion

        /// <summary>
        /// When the control is loaded, we play the animation.
        /// </summary>
        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            // If the window is not in it's own dedicated one (i.e. it's been attached to an existing window), play an animation.
            // We don't play one in a dedicated window since it looks weird: the window appears immediately and then the contents fade in.
            if (!IsDedicatedWindow)
                (Resources["AnimationIn"] as Storyboard).Begin(this, true);
        }

        /// <summary>
        /// Generic event handler that is assigned to the Click event of all buttons that appear in the alert box.
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e) {
            var btn = (Button)sender;
            var buttonContainer = VisualTreeHelper.GetParent(btn) as ContentPresenter; // Get the element that wraps the button
            var stackpanel = VisualTreeHelper.GetParent(buttonContainer) as StackPanel; // Get the wrapper's parent element
            var btnIndex = stackpanel.Children.IndexOf(buttonContainer); // Find the clicked button's index (wrapper's index in parent)
            PerformButtonClick(btnIndex);
        }

        /// <summary>
        /// Handles the click logic of a button, running the button's Validate method and if allowed, closing the AlertBox.
        /// </summary>
        private void PerformButtonClick(int btnIndex) {
            var valid = Buttons.ElementAtOrDefault(btnIndex)?.Validate?.Invoke(btnIndex) ?? true; // Check to see if the button has a defined `Validate` function that should be run first.
            if (valid) Close(btnIndex);
        }

        /// <summary>
        /// Closes the button when the backdrop or small 'X' button is pressed. Results in a -1 result from the Task.
        /// </summary>
        private void Backdrop_Click(object sender, RoutedEventArgs e) {
            if (AllowClose)
                Close(-1);
        }

        /// <summary>
        /// Handler for key input. Allows key presses to trigger the default choice buttons and the close.
        /// </summary>
        private void UserControl_KeyDown(object sender, KeyEventArgs e) {
            // If the return/numpad enter key are pressed, trigger the "default" button
            if (e.Key == Key.Return || e.Key == Key.Enter) {
                var defIdx = Buttons.Select((b, i) => new { b, i }).FirstOrDefault(x => x.b.IsDefault)?.i;
                if (defIdx.HasValue) PerformButtonClick(defIdx.Value);

            // If the escape key was pressed, simulate a click on the backdrop. This will then respect the `AllowClose` flag.
            } else if (e.Key == Key.Escape)
                Backdrop_Click(sender, null);
        }

        /// <summary>
        /// Causes the alert box to close.
        /// In the case of a dedicated window, will close the window by setting the <see cref="Window.DialogResult"/> to true.
        /// In the case of a mounted alert box, will simply remove the box from the parent window.
        /// </summary>
        private async void Close(int result) {
            buttonClickCompletionSource.SetResult(result);

            // If we created a window dedicated for this alert box, set the dialog result (releasing the wait on `ShowDialog`)
            if (IsDedicatedWindow && Parent is Window w)
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
        private static Task<int> ShowCore(Panel panel, object content, string title, IEnumerable<ChoiceButton> buttons, AlertBoxIcon icon, bool allowClose, double width) {
            // Default buttons
            buttons ??= new[] { new ChoiceButton("Okay") };

            // Create the alert
            var msg = new AlertBox { Title = title, Content = content, Buttons = buttons, Icon = icon, AllowClose = allowClose, Width = width };

            // If a panel is provided, add the alert to the panel
            if (panel != null) {
                panel.Children.Add(msg);
                FocusManager.SetIsFocusScope(msg, true);
                FocusManager.SetFocusedElement(panel, msg);

            // If not parent panel is provided, create a separate window for the alert
            }  else {
                var w = new Window {
                    ShowInTaskbar = false,
                    WindowStyle = WindowStyle.ToolWindow,
                    ResizeMode = ResizeMode.NoResize,
                    Content = msg,
                    SizeToContent = SizeToContent.WidthAndHeight,
                    Topmost = true
                };
                // Add a close event that checks if the button task is complete, if not it returns -1. This means -1
                // will be returned if there wasn't a button that was clicked.
                w.Closed += (sender, e) => {
                    if (!msg.buttonClickCompletionSource.Task.IsCompleted)
                        msg.buttonClickCompletionSource.SetResult(-1);
                };
                msg.IsDedicatedWindow = true;
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
        public static Task<int> Show(Window parent, object content, string title, IEnumerable<ChoiceButton> buttons = null, AlertBoxIcon icon = AlertBoxIcon.None, bool allowClose = true, double width = double.NaN) {
            Panel panel = null;

            // Attempt to attach the MessageBox to front content of the targetted window
            if (parent != null && parent.Content is Panel) panel = (Panel)parent.Content;

            // Attempt to attach MessageBox to the first control's collection (e.g. if the root element is a border with a grid inside, we can attach to grid)
            // Do not go any deeper than this otherwise we may end up in a very nested panel
            else if (parent != null && parent.Content is ContentControl c && c.Content is Panel) panel = (Panel)c.Content;
            else if (parent != null && parent.Content is Decorator d && d.Child is Panel) panel = (Panel)d.Child;

            return ShowCore(panel, content, title, buttons, icon, allowClose, width);
        }

        /// <summary>
        /// Shows an alert box that will be placed at the root of the given <see cref="Window"/>. Uses the provided content, title, icon and buttons.
        /// Note that the window must have a <see cref="Panel"/> type child (e.g. Grid) at the root or 1-level down. If this is not the case, a new
        /// dedicated window for the alert will be created.
        /// <para>Returns a task that should be awaited. The task will complete when the user chooses an option and the index of the pressed
        /// button will be the resolution value of the task. Will return -1 if the alert was closed without choosing an option.</para>
        /// </summary>
        public static Task<int> Show(Window parent, object content, string title, IEnumerable<string> buttons, AlertBoxIcon icon = AlertBoxIcon.None, bool allowClose = true, double width = double.NaN)
            => Show(parent, content, title, buttons?.Select(lbl => new ChoiceButton(lbl)), icon, allowClose, width);

        /// <summary>
        /// Will attempt to search for the parent <see cref="Window"/> of the given <see cref="DependencyObject"/>. This allows for use of the
        /// <see cref="AlertBox"/> within custom controls that do not normally have direct access to the <see cref="Window" /> reference.
        /// <para>Returns a task that should be awaited. The task will complete when the user chooses an option and the index of the pressed
        /// button will be the resolution value of the task. Will return -1 if the alert was closed without choosing an option.</para>
        /// </summary>
        public static Task<int> Show(DependencyObject obj, object content, string title, IEnumerable<ChoiceButton> buttons = null, AlertBoxIcon icon = AlertBoxIcon.None, bool allowClose = true, double width = double.NaN) {
            while (obj != null && !(obj is Window))
                obj = VisualTreeHelper.GetParent(obj);
            return Show(obj as Window, content, title, buttons, icon, allowClose, width);
        }

        /// <summary>
        /// Will attempt to search for the parent <see cref="Window"/> of the given <see cref="DependencyObject"/>. This allows for use of the
        /// <see cref="AlertBox"/> within custom controls that do not normally have direct access to the <see cref="Window" /> reference.
        /// <para>Returns a task that should be awaited. The task will complete when the user chooses an option and the index of the pressed
        /// button will be the resolution value of the task. Will return -1 if the alert was closed without choosing an option.</para>
        /// </summary>
        public static Task<int> Show(DependencyObject obj, object content, string title, IEnumerable<string> buttons, AlertBoxIcon icon = AlertBoxIcon.None, bool allowClose = true, double width = double.NaN)
            => Show(obj, content, title, buttons?.Select(lbl => new ChoiceButton(lbl)), icon, allowClose, width);

        /// <summary>
        /// Shows an alert box that will appear in a dedicated new window.
        /// <para>Returns a task that should be awaited. The task will complete when the user chooses an option and the index of the pressed
        /// button will be the resolution value of the task. Will return -1 if the alert was closed without choosing an option.</para>
        /// </summary>
        public static Task<int> Show(object content, string title, IEnumerable<ChoiceButton> buttons = null, AlertBoxIcon icon = AlertBoxIcon.None, bool allowClose = true, double width = double.NaN)
            => ShowCore(null, content, title, buttons, icon, allowClose, width);

        /// <summary>
        /// Shows an alert box that will appear in a dedicated new window.
        /// <para>Returns a task that should be awaited. The task will complete when the user chooses an option and the index of the pressed
        /// button will be the resolution value of the task. Will return -1 if the alert was closed without choosing an option.</para>
        /// </summary>
        public static Task<int> Show(object content, string title, IEnumerable<string> buttons, AlertBoxIcon icon = AlertBoxIcon.None, bool allowClose = true, double width = double.NaN)
            => ShowCore(null, content, title, buttons?.Select(lbl => new ChoiceButton(lbl)), icon, allowClose, width);
        #endregion

        #region Preset Show Methods
        /// <summary>
        /// Helper method that uses the `AlertBox.Show` method to show a localized delete window, asking the user if they want to delete a certain item.
        /// </summary>
        /// <param name="itemType">The type of item to delete. E.G. "layer".</param>
        /// <param name="itemName">An identifying name of the item to delete. E.G. "My Layer".</param>
        public async static Task<bool> ShowDelete(Window wnd, string itemType, string itemName, bool allowClose = true) =>
            (await Show(wnd, TranslationSource.Instance.GetInterpolatedString("alert_delete_text", itemType, itemName), TranslationSource.Instance.GetInterpolatedString("alert_delete_title", itemType), new[] { new ChoiceButton(TranslationSource.Instance["dont_delete"], "FlatButton"), new ChoiceButton(TranslationSource.Instance["delete"], "DangerButton", true) }, AlertBoxIcon.Delete, allowClose)) == 1;
        public async static Task<bool> ShowDelete(DependencyObject obj, string itemType, string itemName, bool allowClose = true) =>
            (await Show(obj, TranslationSource.Instance.GetInterpolatedString("alert_delete_text", itemType, itemName), TranslationSource.Instance.GetInterpolatedString("alert_delete_title", itemType), new[] { new ChoiceButton(TranslationSource.Instance["dont_delete"], "FlatButton"), new ChoiceButton(TranslationSource.Instance["delete"], "DangerButton", true) }, AlertBoxIcon.Delete, allowClose)) == 1;

        /// <summary>
        /// Helper method that creates a text input AlertBox. If provided, the validate method will be run each time the user submits the input
        /// and if it returns false, the user is asked again and the `invalidMessage` is shown.
        /// </summary>
        public async static Task<string> ShowInput(DependencyObject obj, string title, string message, string @default = "", Func<string, bool> validate = null, string invalidMessage = "") {
            var buttons = new[] { new ChoiceButton("Cancel", "FlatButton"), new ChoiceButton("Okay", isDefault: true) };
            var textbox = new TextBox { Text = @default };
            var firstRun = true;
            do {
                var result = await Show(obj, firstRun || string.IsNullOrWhiteSpace(invalidMessage) ? new object[] { message, textbox } : new object[] { message, textbox, invalidMessage }, title, buttons, AlertBoxIcon.Question, false, 9999d);
                if (result != 1) return null;
                firstRun = false;
            } while (validate != null && !validate(textbox.Text));
            return textbox.Text;
        }
        #endregion


        /// <summary>
        /// Class that defines the a button that will appear in the alertbox.
        /// </summary>
        public class ChoiceButton {

            /// <summary>The name of the default style, if none is provided.</summary>
            private const string DEFAULT_STYLE = "Panel1Button";

            /// <summary>The text label of the button.</summary>
            public string Label { get; }

            /// <summary>The name of the style this button should use.</summary>
            public string StyleName { get; }

            /// <summary>Whether or not this is the 'default' button that will accept enter keypresses.</summary>
            public bool IsDefault { get; }

            /// <summary>A function that is run once the user clicks this button. If false is returned, the alert will not close.</summary>
            public Func<int, bool> Validate { get; }

            public ChoiceButton(string label, string style = DEFAULT_STYLE, bool isDefault = false, Func<int, bool> validate = null) {
                Label = label;
                StyleName = style;
                IsDefault = isDefault;
                Validate = validate;
            }
        }
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
