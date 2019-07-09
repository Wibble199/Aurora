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
        public override Control_GameStateParameterPicker CreateControl() => new Control_GameStateParameterPicker { PropertyType = PropertyType.Boolean, Margin = new System.Windows.Thickness(0, 0, 0, 6) }
            .WithBinding(Control_GameStateParameterPicker.SelectedPathProperty, new Binding("VariablePath") { Source = this, Mode = BindingMode.TwoWay });

        /// <summary>Fetches the given boolean value from the game state and returns it.</summary>
        public override bool Evaluate(IGameState gameState) {
            bool result = false;
            if (VariablePath.Length > 0)
                try {
                    object tmp = GameStateUtils.RetrieveGameStateParameter(gameState, VariablePath);
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
    /// Condition that accesses some specified game state variables (of numeric type) and returns a comparison between them.
    /// </summary>
    [Evaluatable("Numeric State Variable", category: OverrideLogicCategory.State)]
    public class BooleanGSINumeric : Evaluatable<bool, Control_ConditionGSINumeric> {

        /// <summary>Creates a blank numeric game state lookup evaluatable.</summary>
        public BooleanGSINumeric() { }
        /// <summary>Creates a numeric game state lookup that returns true when the variable at the given path equals the given value.</summary>
        public BooleanGSINumeric(string path1, double val) { Operand1Path = path1; Operand2Path = val.ToString(); }
        /// <summary>Creates a numeric game state lookup that returns true when the variable at path1 equals the given variable at path2.</summary>
        public BooleanGSINumeric(string path1, string path2) { Operand1Path = path1; Operand2Path = path2; }
        /// <summary>Creates a numeric game state lookup that returns a boolean depending on the given operator's comparison between the variable at the given path and the value.</summary>
        public BooleanGSINumeric(string path1, ComparisonOperator op, double val) { Operand1Path = path1; Operand2Path = val.ToString(); Operator = op; }
        /// <summary>Creates a numeric game state lookup that returns a boolean depending on the given operator's comparison between the variable at path1 and the variable at path2.</summary>
        public BooleanGSINumeric(string path1, ComparisonOperator op, string path2) { Operand1Path = path1; Operand2Path = path2; Operator = op; }

        // Path to the two GSI variables (or numbers themselves) and the operator to compare them with
        public string Operand1Path { get; set; }
        public string Operand2Path { get; set; }
        public ComparisonOperator Operator { get; set; } = ComparisonOperator.EQ;

        // Control assigned to this condition
        public override Control_ConditionGSINumeric CreateControl() => new Control_ConditionGSINumeric(this);

        /// <summary>Parses the numbers, compares the result, and returns the result.</summary>
        public override bool Evaluate(IGameState gameState) {
            // Parse the operands (either as numbers or paths)
            double op1 = GameStateUtils.TryGetDoubleFromState(gameState, Operand1Path);
            double op2 = GameStateUtils.TryGetDoubleFromState(gameState, Operand2Path);

            // Evaluate the operands based on the selected operator and return the result.
            return Operator switch {
                ComparisonOperator.EQ => op1 == op2,
                ComparisonOperator.NEQ => op1 != op2,
                ComparisonOperator.LT => op1 < op2,
                ComparisonOperator.LTE => op1 <= op2,
                ComparisonOperator.GT => op1 > op2,
                ComparisonOperator.GTE => op1 >= op2,
                _ => false
            };
        }

        /// <summary>Update the assigned control with the new application.</summary>
        public override void SetApplication(Application application) {
            Control?.SetApplication(application);

            // Check to ensure the variable paths are valid
            if (application != null && !double.TryParse(Operand1Path, out _) && !string.IsNullOrWhiteSpace(Operand1Path) && !application.ParameterLookup.IsValidParameter(Operand1Path))
                Operand1Path = string.Empty;
            if (application != null && !double.TryParse(Operand2Path, out _) && !string.IsNullOrWhiteSpace(Operand2Path) && !application.ParameterLookup.IsValidParameter(Operand2Path))
                Operand2Path = string.Empty;
        }

        public override IEvaluatable<bool> Clone() => new BooleanGSINumeric { Operand1Path = Operand1Path, Operand2Path = Operand2Path, Operator = Operator };
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