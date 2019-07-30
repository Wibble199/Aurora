using Aurora.Profiles;
using Aurora.Settings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Aurora.Controls {

    /// <summary>
    /// Provides an control that automatically generates fields for any class that implements <see cref="IStringProperty"/> correctly.
    /// </summary>
    public partial class StringPropertyEditor : UserControl {

        public StringPropertyEditor() {
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
            DependencyProperty.Register("TargetInstance", typeof(IStringProperty), typeof(StringPropertyEditor), new PropertyMetadata(null));
    }


    /// <summary>
    /// Converter that takes a <see cref="IStringProperty"/> instance and returns a <see cref="IEnumerable{T}"/> of <see cref="PropertyProxyAccessProvider"/>s
    /// that provide proxy access to all of the <see cref="IMember"/>s in the <see cref="StringProperty{TBase}.PropertyLookup"/> dictionary.
    /// </summary>
    public class PropertyLookupToProxyConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var target = value as IStringProperty;

            // Get a dictionary of any categories defined on this type
            var categories = target.GetType().GetCustomAttributes<CategoryMetadataAttribute>(inherit: true).ToList();
            categories.Add(new CategoryMetadataAttribute("ungrouped") { LocName = "other", Order = int.MaxValue });

            // Categories all the properties
            var categorised = from p in target.PropertyLookup
                              let ppap = new PropertyProxyAccessProvider { Target = target, Member = p.Value, Type = p.Value.NonNullableMemberType }
                              join c in categories on ppap.Member.Metadata.CategoryID ?? "ungrouped" equals c.CategoryID
                              orderby c.Order, ppap.Member.Metadata.Order
                              select new { Prop = ppap, Category = c };

            // Create a ListCollectionView to sort and group the properties
            var lcv = new ListCollectionView(categorised.ToList());
            lcv.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
            return lcv;
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
        public Type Type { get; set; }
        public object Value {
            get => Member.Get(Target);
            set => Member.Set(Target, value);
        }
    }


    [CategoryMetadata("someCat", LocName = "cancel", Order = 1)]
    public class TestClass : StringProperty<TestClass> {

        [EditorField(Name = "Non-localized name", Description = "Non-localized description", Order = 2)]
        public virtual string Hello { get; set; }

        [EditorField(LocName = "cancel", LocDescription = "restart_required", Order = 1)]
        public virtual int World { get; set; } = 5;

        [EditorField(CategoryID = "someCat")]
        public virtual bool Yes { get; set; }
    }
}
