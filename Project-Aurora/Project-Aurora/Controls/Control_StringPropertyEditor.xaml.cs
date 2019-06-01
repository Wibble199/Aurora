using Aurora.Profiles;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Aurora.Controls {

    /// <summary>
    /// Provides an control that automatically generates fields for any class that implements <see cref="IStringProperty"/> correctly.
    /// </summary>
    public partial class Control_StringPropertyEditor : UserControl {

        public Control_StringPropertyEditor() {
            InitializeComponent();

            TargetInstance = new TestClass();

            ((FrameworkElement)Content).DataContext = this;
        }

        /// <summary>
        /// Gets or sets the <see cref="IStringProperty"/> that is being edited by this control.
        /// </summary>
        public IStringProperty TargetInstance {
            get => (IStringProperty)GetValue(TargetInstanceProperty);
            set => SetValue(TargetInstanceProperty, value);
        }
        public static readonly DependencyProperty TargetInstanceProperty =
            DependencyProperty.Register("TargetInstance", typeof(IStringProperty), typeof(Control_StringPropertyEditor), new PropertyMetadata(null));
    }

    /// <summary>
    /// Specialised Multi-Binding that is capable of binding to a specific key in the bound <see cref="IStringProperty"/> target. The ctor
    /// should be passed a binding indicating which property should be accessed. This binding MUST be <see cref="BindingMode.OneWay"/>.
    /// </summary>
    public class PropertyAccessProxyBinding : MultiBinding {
        
        public PropertyAccessProxyBinding(BindingBase propNameBinding) {
            Bindings.Add(propNameBinding);
            Bindings.Add(new Binding("DataContext.TargetInstance") {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Grid), 1),
                Mode = BindingMode.OneWay
            });
            Mode = BindingMode.TwoWay;
            Converter = new PropertyAccessProxyBindingMultiValueConverter();
        }

        class PropertyAccessProxyBindingMultiValueConverter : IMultiValueConverter {

            private IStringProperty target;
            private string key;

            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
                // Store the key and the target IStringProperty so it can be retrieved during the convert back
                key = values[0] as string;
                target = values[1] as IStringProperty;

                // Get the target property's value and return it
                return target.GetValueFromString(key);
            }
            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
                // ConvertBack is called when the view control changes, so write the value back into the cached target.
                target.SetValueFromString(key, value);

                // Since both individual bindings that make up the PropertyAccessProxyBinding Multibinding are OneWay, we can just return an empty array.
                return new object[2];
            }
        }
    }

    public class TestClass : StringPropertyNotify<TestClass> {

        private string hello = "Hello!";
        public string Hello { get => hello; set => SetAndNotify(ref hello, value); }

        public int World { get; set; } = 5;
    }
}
