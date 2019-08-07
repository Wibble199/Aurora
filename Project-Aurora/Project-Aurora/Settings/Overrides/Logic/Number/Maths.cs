using System;
using System.Windows.Data;
using Aurora.Profiles;
using Aurora.Utils;
using Xceed.Wpf.Toolkit;

namespace Aurora.Settings.Overrides.Logic
{

    /// <summary>
    /// Evaluatable that performs a binary mathematical operation on two operands.
    /// </summary>
    [Evaluatable("Arithmetic Operation", category: OverrideLogicCategory.Maths)]
    public class NumberMathsOperation : Evaluatable<double, Control_BinaryOperationHolder> {

        /// <summary>Creates a new maths operation that has no values pre-set.</summary>
        public NumberMathsOperation() { }
        /// <summary>Creates a new evaluatable that returns the result of the two given numbers added together.</summary>
        public NumberMathsOperation(double value1, double value2) { Operand1 = new NumberConstant(value1); Operand2 = new NumberConstant(value2); }
        /// <summary>Creates a new evaluatable that returns the result of the two given numbers with the given operator.</summary>
        public NumberMathsOperation(double value1, MathsOperator op, double value2) { Operand1 = new NumberConstant(value1); Operand2 = new NumberConstant(value2); Operator = op; }
        /// <summary>Creates a new evaluatable that returns the result of the given evaluatable and given number added together.</summary>
        public NumberMathsOperation(IEvaluatable<double> eval, double value) { Operand1 = eval; Operand2 = new NumberConstant(value); }
        /// <summary>Creates a new evaluatable that returns the result of the given evaluatable and given number with the given operator.</summary>
        public NumberMathsOperation(IEvaluatable<double> eval, MathsOperator op, double value) { Operand1 = eval; Operand2 = new NumberConstant(value); Operator = op; }
        /// <summary>Creates a new evaluatable that returns the result of the two given evaluatables added together.</summary>
        public NumberMathsOperation(IEvaluatable<double> eval1, IEvaluatable<double> eval2) { Operand1 = eval1; Operand2 = eval2; }
        /// <summary>Creates a new evaluatable that returns the result of the two given evaluatables with the given operator.</summary>
        public NumberMathsOperation(IEvaluatable<double> eval1, MathsOperator op, IEvaluatable<double> eval2) { Operand1 = eval1; Operand2 = eval2; Operator = op; }

        // The operands and the operator
        public IEvaluatable<double> Operand1 { get; set; } = new NumberConstant();
        public IEvaluatable<double> Operand2 { get; set; } = new NumberConstant();
        public MathsOperator Operator { get; set; } = MathsOperator.Add;
        
        // The control allowing the user to edit the evaluatable
        public override Control_BinaryOperationHolder CreateControl() => new Control_BinaryOperationHolder(typeof(double), typeof(MathsOperator))
            .WithBinding(Control_BinaryOperationHolder.Operand1Property, new Binding("Operand1") { Source = this, Mode = BindingMode.TwoWay })
            .WithBinding(Control_BinaryOperationHolder.Operand2Property, new Binding("Operand2") { Source = this, Mode = BindingMode.TwoWay })
            .WithBinding(Control_BinaryOperationHolder.SelectedOperatorProperty, new Binding("Operator") { Source = this, Mode = BindingMode.TwoWay });

        /// <summary>Resolves the two operands and then compares them using the user specified operator</summary>
        public override double Evaluate(IGameState gameState) {
            var op1 = Operand1.Evaluate(gameState);
            var op2 = Operand2.Evaluate(gameState);
            switch (Operator) {
                case MathsOperator.Add: return op1 + op2;
                case MathsOperator.Sub: return op1 - op2;
                case MathsOperator.Mul: return op1 * op2;
                case MathsOperator.Div: return op2 == 0 ? 0 : op1 / op2; // Return 0 if user tried to divide by zero. Easier than having to deal with Infinity (which C# returns).
                case MathsOperator.Mod: return op2 == 0 ? 0 : op1 % op2;
                default: return 0;
            }
        }

        /// <summary>Updates the user control and the operands with a new application context.</summary>
        public override void SetApplication(Application application) {
            Control?.SetApplication(application);
            Operand1?.SetApplication(application);
            Operand2?.SetApplication(application);
        }
    }


    
    /// <summary>
    /// Returns the absolute value of the given evaluatable.
    /// </summary>
    [Evaluatable("Absolute", category: OverrideLogicCategory.Maths)]
    public class NumberAbsValue : Evaluatable<double, Control_NumericUnaryOpHolder> {

        /// <summary>Creates a new absolute operation with the default operand.</summary>
        public NumberAbsValue() { }
        /// <summary>Creates a new absolute evaluatable with the given operand.</summary>
        public NumberAbsValue(IEvaluatable<double> op) { Operand = op; }

        /// <summary>The operand to absolute.</summary>
        public IEvaluatable<double> Operand { get; set; } = new NumberConstant();

        // Get the control allowing the user to set the operand
        public override Control_NumericUnaryOpHolder CreateControl() => new Control_NumericUnaryOpHolder("Absolute")
            .WithBinding(Control_NumericUnaryOpHolder.OperandProperty, new Binding("Operand") { Source = this, Mode = BindingMode.TwoWay });

        /// <summary>Evaluate the operand and return the absolute value of it.</summary>
        public override double Evaluate(IGameState gameState) => Math.Abs(Operand.Evaluate(gameState));

        public override void SetApplication(Application application) {
            Control.SetApplication(application);
            Operand?.SetApplication(application);
        }
    }



    /// <summary>
    /// Evaluatable that compares two numerical evaluatables and returns a boolean depending on the comparison.
    /// </summary>
    [Evaluatable("Arithmetic Comparison", category: OverrideLogicCategory.Maths)]
    public class BooleanMathsComparison : Evaluatable<bool, Control_BinaryOperationHolder> {

        /// <summary>Creates a new maths comparison that has no values pre-set.</summary>
        public BooleanMathsComparison() { }
        /// <summary>Creates a new evaluatable that returns whether or not the two given numbers are equal.</summary>
        public BooleanMathsComparison(double value1, double value2) { Operand1 = new NumberConstant(value1); Operand2 = new NumberConstant(value2); }
        /// <summary>Creates a new evaluatable that returns the result of the two given numbers compared using the given operator.</summary>
        public BooleanMathsComparison(double value1, ComparisonOperator op, double value2) { Operand1 = new NumberConstant(value1); Operand2 = new NumberConstant(value2); Operator = op; }
        /// <summary>Creates a new evaluatable that returns whether or not the given evaluatable and given number are equal.</summary>
        public BooleanMathsComparison(IEvaluatable<double> eval, double value) { Operand1 = eval; Operand2 = new NumberConstant(value); }
        /// <summary>Creates a new evaluatable that returns the result of the given evaluatable and given number when compared using the given operator.</summary>
        public BooleanMathsComparison(IEvaluatable<double> eval, ComparisonOperator op, double value) { Operand1 = eval; Operand2 = new NumberConstant(value); Operator = op; }
        /// <summary>Creates a new evaluatable that returns the whether or not the two given evaluatables are equal.</summary>
        public BooleanMathsComparison(IEvaluatable<double> eval1, IEvaluatable<double> eval2) { Operand1 = eval1; Operand2 = eval2; }
        /// <summary>Creates a new evaluatable that returns the result of the two given evaluatables when compared using the given operator.</summary>
        public BooleanMathsComparison(IEvaluatable<double> eval1, ComparisonOperator op, IEvaluatable<double> eval2) { Operand1 = eval1; Operand2 = eval2; Operator = op; }

        // The operands and the operator
        public IEvaluatable<double> Operand1 { get; set; } = new NumberConstant();
        public IEvaluatable<double> Operand2 { get; set; } = new NumberConstant();
        public ComparisonOperator Operator { get; set; } = ComparisonOperator.EQ;

        // The control allowing the user to edit the evaluatable
        public override Control_BinaryOperationHolder CreateControl() => new Control_BinaryOperationHolder(typeof(double), typeof(ComparisonOperator))
            .WithBinding(Control_BinaryOperationHolder.Operand1Property, new Binding("Operand1") { Source = this, Mode = BindingMode.TwoWay })
            .WithBinding(Control_BinaryOperationHolder.Operand2Property, new Binding("Operand2") { Source = this, Mode = BindingMode.TwoWay })
            .WithBinding(Control_BinaryOperationHolder.SelectedOperatorProperty, new Binding("Operator") { Source = this, Mode = BindingMode.TwoWay });

        /// <summary>Resolves the two operands and then compares them with the user-specified operator.</summary>
        public override bool Evaluate(IGameState gameState) {
            var op1 = Operand1.Evaluate(gameState);
            var op2 = Operand2.Evaluate(gameState);
            switch (Operator) {
                case ComparisonOperator.EQ: return op1 == op2;
                case ComparisonOperator.NEQ: return op1 != op2;
                case ComparisonOperator.GT: return op1 > op2;
                case ComparisonOperator.GTE: return op1 >= op2;
                case ComparisonOperator.LT: return op1 < op2;
                case ComparisonOperator.LTE: return op1 <= op2;
                default: return false;
            }
        }

        /// <summary>Updates the user control and the operands with a new application context.</summary>
        public override void SetApplication(Application application) {
            Control?.SetApplication(application);
            Operand1?.SetApplication(application);
            Operand2?.SetApplication(application);
        }
    }



    /// <summary>
    /// Evaluatable that takes a number in a given range and maps it onto another range.
    /// </summary>
    [Evaluatable("Linear Interpolation", category: OverrideLogicCategory.Maths)]
    public class NumberMap : Evaluatable<double, Control_NumericMap> {

        /// <summary>Creates a new numeric map with the default constant parameters.</summary>
        public NumberMap() { }
        /// <summary>Creates a new numeric map to map the given value with the given constant range onto the range 0 → 1.</summary>
        public NumberMap(IEvaluatable<double> value, double fromMin, double fromMax) : this(value, new NumberConstant(fromMin), new NumberConstant(fromMax)) { }
        /// <summary>Creates a new numeric map to map the given value with the given dynamic range onto the range 0 → 1.</summary>
        public NumberMap(IEvaluatable<double> value, IEvaluatable<double> fromMin, IEvaluatable<double> fromMax) { Value = value; FromMin = fromMin; FromMax = fromMax; }
        /// <summary>Creates a new numeric map to map the given value with the given constant from range onto the given constant to range.</summary>
        public NumberMap(IEvaluatable<double> value, double fromMin, double fromMax, double toMin, double toMax) : this(value, new NumberConstant(fromMin), new NumberConstant(fromMax), new NumberConstant(toMin), new NumberConstant(toMax)) { }
        /// <summary>Creates a new numeric map to map the given value with the given dynamic from range onto the given constant to range.</summary>
        public NumberMap(IEvaluatable<double> value, IEvaluatable<double> fromMin, IEvaluatable<double> fromMax, double toMin, double toMax) : this(value, fromMin, fromMax, new NumberConstant(toMin), new NumberConstant(toMax)) { }
        /// <summary>Creates a new numeric map to map the given value with the given dynamic from range onto the given dynamic to range.</summary>
        public NumberMap(IEvaluatable<double> value, IEvaluatable<double> fromMin, IEvaluatable<double> fromMax, IEvaluatable<double> toMin, IEvaluatable<double> toMax) { Value = value; FromMin = fromMin; FromMax = fromMax; ToMin = toMin; ToMax = toMax; }

        // The value to run through the map
        public IEvaluatable<double> Value { get; set; } = new NumberConstant(25);
        // The values representing the starting range of the map
        public IEvaluatable<double> FromMin { get; set; } = new NumberConstant(0);
        public IEvaluatable<double> FromMax { get; set; } = new NumberConstant(100);
        // The values representing the end range of the map
        public IEvaluatable<double> ToMin { get; set; } = new NumberConstant(0);
        public IEvaluatable<double> ToMax { get; set; } = new NumberConstant(1);

        // The control to edit the map parameters
        public override Control_NumericMap CreateControl() => new Control_NumericMap(this);

        /// <summary>Evaluate the from range and to range and return the value in the new range.</summary>
        public override double Evaluate(IGameState gameState) {
            // Evaluate all components
            double value = Value.Evaluate(gameState);
            double fromMin = FromMin.Evaluate(gameState), fromMax = FromMax.Evaluate(gameState);
            double toMin = ToMin.Evaluate(gameState), toMax = ToMax.Evaluate(gameState);

            // Perform actual equation
            return Utils.MathUtils.Clamp((value - fromMin) * ((toMax - toMin) / (fromMax - fromMin)) + toMin, Math.Min(toMin, toMax), Math.Max(toMin, toMax));
            // Here is an example of it running: https://www.desmos.com/calculator/nzbiiz7vxv
        }

        /// <summary> Updates the applications on all sub evaluatables.</summary>
        public override void SetApplication(Application application) {
            Value?.SetApplication(application);
            FromMin?.SetApplication(application);
            ToMin?.SetApplication(application);
            FromMax?.SetApplication(application);
            ToMax?.SetApplication(application);
            Control?.SetApplication(application);
        }
    }



    /// <summary>
    /// Evaluatable that resolves to a numerical constant.
    /// </summary>
    [Evaluatable("Number Constant", category: OverrideLogicCategory.Maths)]
    public class NumberConstant : Evaluatable<double, DoubleUpDown> {

        /// <summary>Creates a new constant with the zero as the constant value.</summary>
        public NumberConstant() { }
        /// <summary>Creats a new constant with the given value as the constant value.</summary>
        public NumberConstant(double value) { Value = value; }

        // The constant value
        public double Value { get; set; }

        // The control allowing the user to edit the number value
        public override DoubleUpDown CreateControl() => new DoubleUpDown().WithBinding(DoubleUpDown.ValueProperty, new Binding("Value") { Source = this });

        /// <summary>Simply returns the constant value specified by the user</summary>
        public override double Evaluate(IGameState gameState) => Value;
    }
}
