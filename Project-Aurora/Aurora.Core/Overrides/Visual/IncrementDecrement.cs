using System.Linq.Expressions;
using Aurora.Core.Overrides.Visual.Base;
using Aurora.Core.Overrides.Visual.Utils;

namespace Aurora.Core.Overrides.Visual {

    // Note that this represents pre-increment/pre-decrement (--x/++x), rather than post-increment/post-decrement (x--/x++).
    public sealed class IncrementDecrement : VisualStatementExpression<double> {
        public string VariableName { get; set; }
        public bool IsIncrement { get; set; }

        public override Expression GetExpression(VisualProgram context) =>
            VariableAccessorFactory.CreateAssignmentExpression(context, VariableName, v => Expression.AddAssign(v, Expression.Constant(IsIncrement ? 1 : -1)));
    }
}
