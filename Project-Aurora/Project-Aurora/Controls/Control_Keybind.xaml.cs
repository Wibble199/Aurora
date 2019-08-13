using Aurora.Settings;
using System.Windows;
using System.Windows.Controls;
using SharpDX.RawInput;
using System.Windows.Input;
using System.Windows.Data;
using System;
using System.Globalization;

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
            Global.InputEvents.KeyUp += InputEventsKeyUp;
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

        #region IsActive Property
        /// <summary>Whether or not this keybind control is the active one.</summary>
        public bool IsActive {
            get => (bool)GetValue(IsActiveProperty);
            private set => SetValue(IsActiveProperty, value);
        }
        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), typeof(Control_Keybind), new PropertyMetadata(false, IsActiveChanged));
        private static void IsActiveChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            if (!(bool)e.NewValue)
                ActiveInstance = null;
            if ((bool)e.NewValue && ActiveInstance != null)
                ActiveInstance.IsActive = false;
            if ((bool)e.NewValue)
                ActiveInstance = (Control_Keybind)sender;
        }
        #endregion

        private static void InputEventsKeyDown(object sender, KeyboardInputEventArgs e) {
            ActiveInstance?.Dispatcher.Invoke(() => {
                if (ActiveInstance.Keybind == null)
                    ActiveInstance.Keybind = new Keybind(Global.InputEvents.PressedKeys);
                else
                    ActiveInstance.Keybind.SetKeys(Global.InputEvents.PressedKeys);
            });
        }

        private static void InputEventsKeyUp(object sender, KeyboardInputEventArgs e) {
            if (ActiveInstance != null && Global.InputEvents.PressedKeys.Length == 0)
                ActiveInstance?.Dispatcher.Invoke(() => ActiveInstance.IsActive = false);
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e) => Keybind?.SetKeys(new System.Windows.Forms.Keys[0]);
    }

    public class ControlKeybindTextboxPaddingConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => new Thickness(0, 0, (double)value, 0);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
