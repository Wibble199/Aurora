using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Aurora.Utils {

    /// <summary>
    /// Converter that returns a boolean based on whether or not the given value is null or not.
    /// Does not support "ConvertBack".
    /// </summary>
    public class IsNullToBooleanConverter : IValueConverter {

        /// <summary>This is the value to return when the given value is null. Will return the opposite if the value is non-null.</summary>
        public bool ReturnValWhenNull { get; set; } = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => !(value == null ^ ReturnValWhenNull);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    /// <summary>Simple converter that returns true if the given value is non-null.</summary>
    public class IsNullToVisibilityConverter : IValueConverter {

        public Visibility ReturnValWhenNull { get; set; } = Visibility.Collapsed;
        public Visibility ReturnValWhenNonNull { get; set; } = Visibility.Visible;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value == null ? ReturnValWhenNull : ReturnValWhenNonNull;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    /// <summary>Simple 2-way converter that inverts the given boolean.</summary>
    public class BooleanInverterConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => !((bool)value);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => !((bool)value);
    }

    /// <summary>
    /// Converter that allows specification of multiple other converters.
    /// Does not support "ConvertBack".
    /// </summary>
    /// <remarks>Code by Garath Evans (https://bit.ly/2HAdFvW)</remarks>
    public class ValueConverterGroup : System.Collections.Generic.List<IValueConverter>, IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => this.Aggregate(value, (current, converter) => converter.Convert(current, targetType, parameter, culture));
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    /// <summary>
    /// MultiConverter that takes the self (use "&lt;Binding RelativeSource="{RelativeSource Self}" /&gt;") element and a string name of a
    /// style and returns the actual style resource.
    /// </summary>
    /// <remarks>Code adapted from https://stackoverflow.com/a/410681/1305670 </remarks>
    public class StyleConverter : IMultiValueConverter {

        /// <summary>A default style to resolve if the one from the binding does not exist.</summary>
        public string DefaultStyleName { get; set; } = "";

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            var targetElement = values[0] as FrameworkElement;
            if (!(values[1] is string styleName)) return null;
            return (Style)targetElement.TryFindResource(styleName) ?? (Style)targetElement.TryFindResource(DefaultStyleName);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
