using Aurora.Settings.Layers;
using Aurora.Settings.Overrides.Logic;
using Aurora.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Aurora.Settings.Overrides {
    /// <summary>
    /// Interaction logic for Control_OverridesEditor.xaml
    /// </summary>
    public partial class Control_OverridesEditor : UserControl, INotifyPropertyChanged {

        public Control_OverridesEditor() {
            // Setup UI and databinding stuff
            InitializeComponent();
            ((FrameworkElement)Content).DataContext = this;
        }

        #region PropertyChanged Event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(params string[] affectedProperties) {
            foreach (var prop in affectedProperties)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        #endregion

        #region Properties
        /// <summary>
        /// For the given layer, returns a list of all properties on the handler of that layer that have the OverridableAttribute 
        /// applied (i.e. have been marked overridable for the overrides system).
        /// </summary>
        public List<Tuple<string, string, Type>> AvailableLayerProperties {
            get {
                // Get a list of any members that should be ignored as per the LogicOverrideIgnorePropertyAttribute on the properties class
                var ignoredProperties = Layer?.Handler.GetType().GetCustomAttributes(typeof(LogicOverrideIgnorePropertyAttribute), false)
                    .Cast<LogicOverrideIgnorePropertyAttribute>()
                    .Select(attr => attr.PropertyName);

                return Layer?.Handler.Properties.GetType().GetProperties() // Get all properties on the layer handler's property list
                    .Where(prop => prop.GetCustomAttributes(typeof(LogicOverridableAttribute), true).Length > 0) // Filter to only return the PropertyInfos that have Overridable
                    .Where(prop => !ignoredProperties.Contains(prop.Name)) // Only select things that are NOT on the ignored properties list
                    .Select(prop => new Tuple<string, string, Type>( // Return the name and type of these properties.
                        prop.Name, // The actual C# property name
                        ((LogicOverridableAttribute)prop.GetCustomAttributes(typeof(LogicOverridableAttribute), true)[0]).Name, // Get the name specified in the attribute (so it is prettier for the user)
                        Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType // If the property is a nullable type (e.g. bool?), will instead return the non-nullable type (bool)
                    ))
                    .OrderBy(tup => tup.Item2)
                    .ToList();
            }
        }

        /// <summary>The layer being edited by this control.</summary>
        public Layer Layer {
            get => (Layer)GetValue(LayerProperty);
            set => SetValue(LayerProperty, value);
        }
        public static readonly DependencyProperty LayerProperty =
            DependencyProperty.Register("Layer", typeof(Layer), typeof(Control_OverridesEditor), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnLayerChange));

        // The name of the selected property that is being edited
        private Tuple<string, string, Type> _selectedProperty;
        public Tuple<string, string, Type> SelectedProperty {
            get => _selectedProperty;
            set {
                _selectedProperty = value;
                OnPropertyChanged("SelectedProperty", "OverridesEnabledForProperty", "CurrentEvaluatable");
            }
        }

        public bool OverridesEnabledForProperty {
            get => SelectedProperty != null && CurrentEvaluatable != null;
            set {
                if (Layer == null || value == OverridesEnabledForProperty) return;
                if (value) {
                    Layer.OverrideLogic[SelectedProperty.Item1] = EvaluatableDefaults.Get(SelectedProperty.Item3);
                } else {
                    Layer.OverrideLogic.Remove(SelectedProperty.Item1);
                    ((IValueOverridable)Layer.Handler.Properties).Overrides.SetValueFromString(_selectedProperty.Item1, null);
                }
                OnPropertyChanged("CurrentEvaluatable");
            }
        }

        public IEvaluatable CurrentEvaluatable {
            get => (Layer != null && SelectedProperty != null && Layer.OverrideLogic.TryGetValue(SelectedProperty.Item1, out var eval)) ? eval : null;
            set {
                if (Layer != null)
                    Layer.OverrideLogic[SelectedProperty.Item1] = value;
            }
        }
        #endregion

        private static void OnLayerChange(DependencyObject overridesEditor, DependencyPropertyChangedEventArgs eventArgs) {
            var control = (Control_OverridesEditor)overridesEditor;
            var layer = (Layer)eventArgs.NewValue;
            // Ensure the layer has the property-override map
            if (layer.OverrideLogic == null)
                layer.OverrideLogic = new ObservableDictionary<string, IEvaluatable>();
            control.SelectedProperty = null;
            control.OnPropertyChanged("Layer", "AvailableLayerProperties");
        }

        #region Methods
        // Inform bindings that the available properties list has changed (by raising "" has changed).
        // This may also change the selected property, and therefore the selected logic etc.
        public void ForcePropertyListUpdate() => OnPropertyChanged("");

        /// <summary>Open the overrides page on the documentation page</summary>
        private void HelpButton_Click(object sender, RoutedEventArgs e) =>
            Process.Start(new ProcessStartInfo(@"https://project-aurora.gitbook.io/guide/advanced-topics/overrides-system"));
        #endregion
    }



    /// <summary>
    /// Simple converter to convert a type to it's name (instead of using ToString beacuse that gives the fully qualified name).
    /// </summary>
    public class PrettyTypeNameConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => ((Type)value).Name;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    /// <summary>
    /// Simple converter to convert a type to a pretty icon used in the list.
    /// </summary>
    public class TypeToIconConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var imageName = new Dictionary<Type, string> {
                { typeof(bool), "checked-checkbox-30.png" },
                { typeof(int), "numbers-30.png" },
                { typeof(long), "numbers-30.png" },
                { typeof(float), "numbers-30.png" },
                { typeof(double), "numbers-30.png" },
                { typeof(Color), "paint-palette-30.png" },
                { typeof(KeySequence), "keyboard-30.png" }
            }.TryGetValue((Type)value, out string val) ? val : "diamonds-30.png";
            return new BitmapImage(new Uri($"/Aurora;component/Resources/UIIcons/{imageName}", UriKind.Relative));
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    /// <summary>
    /// Simple converter that returns true if the given property has a value or false if it is null.
    /// </summary>
    public class HasValueToBoolConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value != null;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
