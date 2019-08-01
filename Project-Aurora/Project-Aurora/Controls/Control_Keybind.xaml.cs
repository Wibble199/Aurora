using Aurora.Settings;
using System.Windows;
using System.Windows.Controls;
using SharpDX.RawInput;
using System.Windows.Input;

namespace Aurora.Controls {

    public partial class Control_Keybind : UserControl {

        public static Control_Keybind ActiveInstance { get; private set; } = null; //Makes sure that only one keybind can be set at a time

        public Control_Keybind() {
            InitializeComponent();
            ((FrameworkElement)Content).DataContext = this;
        }

        static Control_Keybind() {
            // Since there is only ever a single static active instance, we only need one event handler which can use that static reference,
            // instead of needing to add events to each control instance.
            Global.InputEvents.KeyDown += InputEventsKeyDown;
        }

        #region Keybind Property
        /// <summary>The keybind that this control is presenting to the user.</summary>
        public Keybind Keybind {
            get => (Keybind)GetValue(KeybindProperty);
            set => SetValue(KeybindProperty, value);
        }

        public static readonly DependencyProperty KeybindProperty =
            DependencyProperty.Register("Keybind", typeof(Keybind), typeof(Control_Keybind), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        #endregion

        #region CanClear Property
        /// <summary>Whether or not the control should show a button allowing the user to clear the keybind.</summary>
        public bool CanClear {
            get => (bool)GetValue(CanClearProperty);
            set => SetValue(CanClearProperty, value);
        }
        public static readonly DependencyProperty CanClearProperty =
            DependencyProperty.Register("CanClear", typeof(bool), typeof(Control_Keybind), new PropertyMetadata(false));
        #endregion

        private static void InputEventsKeyDown(object sender, KeyboardInputEventArgs e) {
            ActiveInstance?.Dispatcher.Invoke(() => {
                if (ActiveInstance.Keybind == null)
                    ActiveInstance.Keybind = new Keybind(Global.InputEvents.PressedKeys);
                else
                    ActiveInstance.Keybind.SetKeys(Global.InputEvents.PressedKeys);
            });
        }

        public void Start() {
            ActiveInstance?.Stop();
            ActiveInstance = this;
        }

        public void Stop() {
            ActiveInstance = null;
        }

        private void TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) => Start();
        private void TextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) => Stop();
        private void Textbox_KeyDown(object sender, KeyEventArgs e) => e.Handled = true;
        private void ClearButton_Click(object sender, RoutedEventArgs e) => Keybind?.SetKeys(new System.Windows.Forms.Keys[0]);
    }
}
