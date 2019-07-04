using Aurora.Settings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace Aurora.Controls {
    public partial class KeySequence : UserControl {

        #region Title Property
        public string Title {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(KeySequence), new PropertyMetadata("Affected Keys"));
        #endregion

        #region Recording Tag Property
        public string RecordingTag {
            get => (string)GetValue(RecordingTagProperty);
            set => SetValue(RecordingTagProperty, value);
        }

        public static readonly DependencyProperty RecordingTagProperty =
            DependencyProperty.Register("RecordingTag", typeof(string), typeof(KeySequence), new PropertyMetadata());
        #endregion

        #region Sequence Property
        public Settings.KeySequence Sequence {
            get => (Settings.KeySequence)GetValue(SequenceProperty);
            set {
                if (!Equals(value, Sequence)) {
                    sequence_removeFromLayerEditor();
                }

                SetValue(SequenceProperty, value);

                sequence_updateToLayerEditor();

                SequenceUpdated?.Invoke(this, new EventArgs());
            }
        }

        public static readonly DependencyProperty SequenceProperty = DependencyProperty.Register("Sequence", typeof(Settings.KeySequence), typeof(KeySequence), new PropertyMetadata(null));
        #endregion

        #region Freestyle Enabled Property
        public bool FreestyleEnabled {
            get => (bool)GetValue(FreestyleEnabledProperty);
            set => SetValue(FreestyleEnabledProperty, value);
        }
        public List<Devices.DeviceKeys> SelectedItems { get; internal set; }

        public static readonly DependencyProperty FreestyleEnabledProperty = DependencyProperty.Register("FreestyleEnabled", typeof(bool), typeof(KeySequence), new PropertyMetadata(true));
        #endregion

        #region Freestyle Key Light
        public SolidColorBrush FreestyleKeyLight {
            get => (SolidColorBrush)GetValue(FreestyleKeyLightProperty);
            set => SetValue(FreestyleKeyLightProperty, value);
        }
        public static readonly DependencyProperty FreestyleKeyLightProperty =
            DependencyProperty.Register("FreestyleKeyLight", typeof(SolidColorBrush), typeof(KeySequence), new PropertyMetadata(Brushes.Black));
        #endregion


        /// <summary>Fired whenever the KeySequence object is changed or re-created. Does NOT trigger when keys are changed.</summary>
        public event EventHandler SequenceUpdated;
        /// <summary>Fired whenever keys are changed.</summary>
        public event EventHandler SequenceKeysChange;
        public event SelectionChangedEventHandler SelectionChanged;

        public KeySequence() {
            InitializeComponent();
            ((FrameworkElement)Content).DataContext = this;
        }

        public void sequence_updateToLayerEditor() {
            if (Sequence != null && IsInitialized && IsVisible && IsEnabled) {
                if (Sequence.Type == Settings.KeySequenceType.FreeForm) {
                    Sequence.Freeform.ValuesChanged += freeform_updated;
                    FreestyleKeyLight = LayerEditor.AddKeySequenceElement(Sequence.Freeform, Title);
                } else {
                    Sequence.Freeform.ValuesChanged -= freeform_updated;
                    LayerEditor.RemoveKeySequenceElement(Sequence.Freeform);
                }
            }
        }

        private void freeform_updated(Settings.FreeFormObject newfreeform) {
            if (newfreeform != null) {
                Sequence.Freeform = newfreeform;
                if (SequenceUpdated != null)
                    SequenceUpdated(this, new EventArgs());
            }
        }

        private void sequence_removeFromLayerEditor() {
            if (Sequence != null && IsInitialized) {
                if (Sequence.Type == Settings.KeySequenceType.FreeForm) {
                    Sequence.Freeform.ValuesChanged -= freeform_updated;
                    LayerEditor.RemoveKeySequenceElement(Sequence.Freeform);
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            sequence_updateToLayerEditor();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e) {
            sequence_removeFromLayerEditor();
        }

        private void UserControl_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (e.NewValue is bool) {
                //this.keys_keysequence.IsEnabled = (bool)e.NewValue;
                //this.sequence_record.IsEnabled = (bool)e.NewValue;
                //this.sequence_up.IsEnabled = (bool)e.NewValue;
                //this.sequence_down.IsEnabled = (bool)e.NewValue;
                //this.sequence_remove.IsEnabled = (bool)e.NewValue;
                //this.sequence_freestyle_checkbox.IsEnabled = (bool)e.NewValue && FreestyleEnabled;

                if ((bool)e.NewValue)
                    sequence_updateToLayerEditor();
                else
                    sequence_removeFromLayerEditor();
            }
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (e.NewValue is bool b && b) {
                sequence_updateToLayerEditor();
            } else
                sequence_removeFromLayerEditor();
        }


        private void RecordingButton_Checked(object sender, RoutedEventArgs e) {
            // If there is no recording in process, start recording, otherwise uncheck the button immediately
            if (!Global.key_recorder.IsRecording())
                Global.key_recorder.StartRecording(RecordingTag);
            else
                ((ToggleButton)sender).IsChecked = false;
        }

        private void RecordingButton_Unchecked(object sender, RoutedEventArgs e) {
            // If the recording for this sequence is in progress, stop it
            if (Global.key_recorder.IsRecording(RecordingTag)) {
                Global.key_recorder.StopRecording();
                foreach (var key in Global.key_recorder.GetKeys())
                    Sequence.Keys.Add(key);
                Global.key_recorder.Reset();
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e) {
            
        }

        private void ReverseButton_Click(object sender, RoutedEventArgs e) {
            // Not using LINQ's Reverse method because it returns a NEW COLLECTION rather than doing an in-place reverse
            var c = Sequence.Keys.Count;
            for (var i = 0; i < c; i++)
                Sequence.Keys.Move(c - 1, i);
        }

        private void UseFreestyleCheckBox_Checked(object sender, RoutedEventArgs e) {
            sequence_updateToLayerEditor();
        }

        private void UseFreestyleCheckBox_Unchecked(object sender, RoutedEventArgs e) {
            sequence_updateToLayerEditor();
        }
    }

    /// <summary>
    /// Converter for binding a KeySequence's type to a "Use freestyle" checkbox's checked property.
    /// </summary>
    public class SequenceTypeToBoolConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (KeySequenceType)value == KeySequenceType.FreeForm;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => (bool)value ? KeySequenceType.FreeForm : KeySequenceType.Sequence;
    }
}