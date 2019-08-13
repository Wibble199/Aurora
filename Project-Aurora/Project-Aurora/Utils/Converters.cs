using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

    /// <summary>Returns true if the given value compares in such a way to the supplied <see cref="CompareTo"/> property.</summary>
    public class NumericComparisonConverter : IValueConverter {
        public object CompareTo { get; set; }
        public ComparisonType Comparison { get; set; }
        public Type Type { set => CompareTo = (IComparable)System.ComponentModel.TypeDescriptor.GetConverter(value).ConvertFrom(CompareTo); }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (!(value is IComparable val) || !(CompareTo is IComparable compTo)) return false;
            var comparison = compTo.CompareTo(val);
            return (Comparison == ComparisonType.EQ && comparison == 0)
                || (Comparison == ComparisonType.GT && comparison < 0)
                || (Comparison == ComparisonType.LT && comparison > 0);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

        public enum ComparisonType { EQ, GT, LT }
    }

    /// <summary>Simple converter that takes a double (0-1) and returns a string showing it as a percentage.</summary>
    public class AsPercentageConverter : IValueConverter {
        public int NumDecimals { get; set; } = 0;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => Math.Round((double)value * 100, NumDecimals) + "%";
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
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

    /// <summary>
    /// Calculates the value that represents the percentage of the first value. The percentage is given by the second value out of the third value.
    /// I.E. returns values[0] * (values[1] / values[2]).
    /// <para>For using with a width/height, set the first value to be the parent's ActualWidth/ActualHeight, second value to the the value and the
    /// third value to be the maximum value.</para>
    /// </summary>
    public class PercentageDoubleConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            double actualWidth = (double)values[0], curVal = (double)values[1];
            double maxVal = values.Length >= 3 ? (double)values[2] : 0, minVal = values.Length >= 4 ? (double)values[3] : 0;
            return actualWidth * ((curVal - minVal) / (maxVal - minVal));
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    /// <summary>
    /// Converts an string path into an ImageSource representing that resource. The path should be relative to the 'Resources' directory.
    /// </summary>
    public class StringToImageSourceConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value == null ? null : new BitmapImage(new Uri($"/Aurora;component/Resources/{value.ToString()}", UriKind.Relative));
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    /// <summary>
    /// Converter that adds all the given thicknesses together.
    /// </summary>
    public class ThicknessAdditionConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (!(value is Thickness t1 && parameter is Thickness t2)) return null;
            return new Thickness(t1.Left + t2.Left, t1.Top + t2.Top, t1.Right + t2.Right, t1.Bottom + t2.Bottom);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
