using System.Windows;
using System.Windows.Data;

namespace Aurora.Utils {
    public static class FrameworkElementExtension {
        /// <summary>
        /// Tiny extension for the FrameworkElement that allows to set a binding on an element and return that element (so it can be chained).
        /// Used in the TypeControlMap to shorten the code.
        /// </summary>
        public static T WithBinding<T>(this T self, DependencyProperty dp, Binding binding, IValueConverter converter = null, BindingMode? bindingMode = null) where T : FrameworkElement {
            if (converter != null)
                binding.Converter = converter;
            if (bindingMode.HasValue)
                binding.Mode = bindingMode.Value;
            self.SetBinding(dp, binding);
            return self;
        }
    }
}
