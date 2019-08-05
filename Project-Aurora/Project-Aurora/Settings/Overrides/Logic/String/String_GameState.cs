using Aurora.Controls;
using Aurora.Profiles;
using Aurora.Utils;
using System.Windows.Data;

namespace Aurora.Settings.Overrides.Logic {

    [Evaluatable("String State Variable", category: OverrideLogicCategory.State)]
    public class StringGSIString : Evaluatable<string, Control_GameStateParameterPicker> {

        /// <summary>Path to the GSI variable</summary>
        public string VariablePath { get; set; } = "";

        /// <summary>Control assigned to this logic node.</summary>
        public override Control_GameStateParameterPicker CreateControl() => new Control_GameStateParameterPicker { PropertyType = PropertyType.String }
            .WithBinding(Control_GameStateParameterPicker.SelectedPathProperty, new Binding("VariablePath") { Source = this, Mode = BindingMode.TwoWay });

        /// <summary>Attempts to return the string at the given state variable.</summary>
        public override string Evaluate(IGameState gameState) {
            if (VariablePath.Length > 0)
                try { return (string)GameStateUtils.RetrieveGameStateParameter(gameState, VariablePath); }
                catch { }
            return "";
        }

        /// <summary>Update the assigned combobox with the new application context.</summary>
        public override void SetApplication(Application application) {
            Control.Application = application;

            // Check to ensure var path is valid
            if (application != null && !string.IsNullOrWhiteSpace(VariablePath) && !application.ParameterLookup.IsValidParameter(VariablePath))
                VariablePath = string.Empty;
        }

        /// <summary>Clones this StringGSIString.</summary>
        public override IEvaluatable<string> Clone() => new StringGSIString { VariablePath = VariablePath };
    }
}
