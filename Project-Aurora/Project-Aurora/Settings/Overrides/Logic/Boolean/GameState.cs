using Aurora.Controls;
using Aurora.Profiles;
using Aurora.Utils;
using Newtonsoft.Json;
using System;
using System.Windows.Data;

namespace Aurora.Settings.Overrides.Logic {

    /// <summary>
    /// Condition that accesses a specific game state variable (of boolean type) and returns the state.
    /// </summary>
    [Evaluatable("Boolean State Variable", category: OverrideLogicCategory.State)]
    public class BooleanGSIBoolean : Evaluatable<bool, Control_GameStateParameterPicker> {

        /// <summary>Creates an empty boolean state variable lookup.</summary>
        public BooleanGSIBoolean() { }
        /// <summary>Creates a evaluatable that returns the boolean variable at the given path.</summary>
        public BooleanGSIBoolean(string variablePath) { VariablePath = variablePath; }

        /// <summary>The path to the variable the user wants to evaluate.</summary>
        public string VariablePath { get; set; } = "";

        /// <summary>Creates a new control for this Boolean GSI.</summary>
        public override Control_GameStateParameterPicker CreateControl() => new Control_GameStateParameterPicker { PropertyType = PropertyType.Boolean }
            .WithBinding(Control_GameStateParameterPicker.SelectedPathProperty, new Binding("VariablePath") { Source = this, Mode = BindingMode.TwoWay });

        /// <summary>Fetches the given boolean value from the game state and returns it.</summary>
        public override bool Evaluate(IGameState gameState) {
            bool result = false;
            if (VariablePath.Length > 0)
                try {
                    result = (bool)Utils.GameStateUtils.RetrieveGameStateParameter(gameState, VariablePath);
                } catch { }
            return result;
        }

        /// <summary>Update the assigned control with the new application.</summary>
        public override void SetApplication(Application application) {
            Control.Application = application;

            // Check to ensure the variable path is valid
            if (application != null && !string.IsNullOrWhiteSpace(VariablePath) && !application.ParameterLookup.IsValidParameter(VariablePath))
                VariablePath = string.Empty;
        }

        public override IEvaluatable<bool> Clone() => new BooleanGSIBoolean { VariablePath = VariablePath };
    }
       


    /// <summary>
    /// Condition that accesses a specified game state variable (of any enum type) and returns a comparison between it and a static enum of the same type.
    /// </summary>
    [Evaluatable("Enum State Variable", category: OverrideLogicCategory.State)]
    public class BooleanGSIEnum : Evaluatable<bool, Control_BooleanGSIEnum> {

        /// <summary>Creates a blank enum game state lookup evaluatable.</summary>
        public BooleanGSIEnum() { }
        /// <summary>Creates an enum game state lookup that returns true when the variable at the given path equals the given enum.</summary>
        public BooleanGSIEnum(string path, Enum val) { StatePath = path; EnumValue = val; }

        // The path of the game state enum
        public string StatePath { get; set; }

        // The value to compare the GSI enum against.
        // Has to be converted using the TypeAnnotatedObjectConverter else the type won't be stored, only the number (which JSON then doesn't know how to serialise back)
        [JsonConverter(typeof(TypeAnnotatedObjectConverter))]
        public Enum EnumValue { get; set; }

        // Control assigned to this condition
        public override Control_BooleanGSIEnum CreateControl() => new Control_BooleanGSIEnum(this);

        /// <summary>Parses the numbers, compares the result, and returns the result.</summary>
        public override bool Evaluate(IGameState gameState) {
            var @enum = GameStateUtils.TryGetEnumFromState(gameState, StatePath);
            return @enum != null && @enum.Equals(EnumValue);
        }

        /// <summary>Update the assigned control with the new application.</summary>
        public override void SetApplication(Application application) {
            Control?.SetApplication(application);

            // Check to ensure the variable paths are valid
            if (application != null && !string.IsNullOrWhiteSpace(StatePath) && !application.ParameterLookup.ContainsKey(StatePath))
                StatePath = string.Empty;
        }

        public override IEvaluatable<bool> Clone() => new BooleanGSIEnum { StatePath = StatePath, EnumValue = EnumValue };
    }
}