using Aurora.Profiles;
using Aurora.Settings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

            TestClass c;
            TargetInstance = c = TestClass.Create();
            c.PropertyChanged += (sender, e) => Console.WriteLine("Property changed");

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
    /// Converter that takes a <see cref="IStringProperty"/> instance and returns a <see cref="IEnumerable{T}"/> of <see cref="PropertyProxyAccessProvider"/>s
    /// that provide proxy access to all of the <see cref="IMember"/>s in the <see cref="StringProperty{TBase}.PropertyLookup"/> dictionary.
    /// </summary>
    public class PropertyLookupToProxyConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var target = value as IStringProperty;
            return target.PropertyLookup.Select(m => new PropertyProxyAccessProvider { Target = target, Member = m.Value });
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    /// <summary>
    /// Provides proxy access to a specific property on a specific instance of an <see cref="IStringProperty"/>. The getters and setters
    /// of the <see cref="IMember"/> are exposed using a property on this class, allowing it to be bound using WPF bindings.
    /// </summary>
    public class PropertyProxyAccessProvider {
        public IStringProperty Target { get; set; }
        public IMember Member { get; set; }
        public object Value {
            get => Member.Get(Target);
            set => Member.Set(Target, value);
        }
    }


    public class TestClass : StringProperty<TestClass> {

        [System.ComponentModel.DisplayName("Non-localized name"), System.ComponentModel.Description("Non-localized description")]
        public virtual string Hello { get; set; }

        [Settings.Localization.LocalizedName("cancel"), Settings.Localization.LocalizedDescription("restart_required")]
        public virtual int World { get; set; } = 5;
    }
}
