using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Aurora.Utils {
    public static class FrameworkElementExtension {
        /// <summary>
        /// Tiny extension for the FrameworkElement that allows to set a binding on an element and return that element (so it can be chained).
        /// Used in the TypeControlMap to shorten the code.
        /// </summary>
        public static T WithBinding<T>(this T self, DependencyProperty dp, Binding binding, BindingMode? bindingMode = null, IValueConverter converter = null) where T : FrameworkElement {
            if (bindingMode.HasValue)
                binding.Mode = bindingMode.Value;
            if (converter != null)
                binding.Converter = converter;
            self.SetBinding(dp, binding);
            return self;
        }

        /// <summary>
        /// Tiny extension for the FrameworkElement that allows to set a binding on an element and return that element (so it can be chained).
        /// Used in the TypeControlMap to shorten the code.
        /// </summary>
        public static T WithBinding<T>(this T self, DependencyProperty dp, object source, string path, BindingMode? bindingMode = null, IValueConverter converter = null) where T : FrameworkElement
            => self.WithBinding(dp, new Binding(path) { Source = source }, bindingMode, converter);


        /// <summary>Uses the <see cref="VisualTreeHelper.GetParent(DependencyObject)"/> method to try and find the first parent of the given type. Will
        /// return null if no parents of the target type are found.</summary>
        /// <typeparam name="T">The type of parent to try to find.</typeparam>
        /// <param name="el">The element to begin the search from.</param>
        public static T GetParent<T>(this DependencyObject el) where T : DependencyObject {
            var cur = el;
            do
                cur = VisualTreeHelper.GetParent(cur);
            while (cur != null && !(cur is T));
            return cur as T;
        }


        /// <summary>Tests to see if the given 'parent' object is a parent of the given 'child' object (as according to
        /// <see cref="VisualTreeHelper.GetParent(DependencyObject)"/>).</summary>
        /// <param name="parent">The parent element. Will return true if 'child' is inside this.</param>
        /// <param name="child">The child element. Will return true if this is inside 'parent'.</param>
        public static bool IsParentOf(this DependencyObject parent, DependencyObject child) {
            var cur = child; // Starting at the child,
            while (cur != null) { // Continuing until we run out of elements
                if (cur == parent) // If the current item is the parent, return true
                    return true;
                cur = VisualTreeHelper.GetParent(cur); // Move on to the parent of the current element
            }
            return false; // If we ran out of elements, 'parent' is not a parent of 'child'.
        }
    }
}
