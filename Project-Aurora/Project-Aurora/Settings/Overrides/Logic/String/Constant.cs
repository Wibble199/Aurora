using Aurora.Profiles;
using Aurora.Utils;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Aurora.Settings.Overrides.Logic {
    /// <summary>
    /// Represents a constant string value that will always evaluate to the same value.
    /// </summary>
    [Evaluatable("String Constant", category: OverrideLogicCategory.String)]
    public class StringConstant : Evaluatable<string, TextBox> {

        /// <summary>The value of the constant.</summary>
        public string Value { get; set; } = "";

        public override TextBox CreateControl() => new TextBox()
            .WithBinding(TextBox.TextProperty, new Binding("Value") { Source = this, Mode = BindingMode.TwoWay });

        /// <summary>Simply return the constant value.</summary>
        public override string Evaluate(IGameState gameState) => Value;

        /// <summary>Clones this constant string value.</summary>
        public override IEvaluatable<string> Clone() => new StringConstant { Value = Value };
    }
}
