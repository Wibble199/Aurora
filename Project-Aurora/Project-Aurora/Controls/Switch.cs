using System.Windows;
using System.Windows.Controls;

namespace Aurora.Controls {
    /// <summary>
    /// Syntactic sugar for creating a checkbox with the "CheckboxSwitch" style applied.
    /// </summary>
    public class Switch : CheckBox {

        public Switch() : base() {
            Style = App.Current.FindResource("CheckboxSwitch") as Style;
            HorizontalContentAlignment = HorizontalAlignment.Left;
        }
    }
}
