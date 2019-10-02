using System.Linq.Expressions;
using Aurora.Core.Overrides.Visual.Base;
using Aurora.Core.Overrides.Visual.Utils;

namespace Aurora.Core.Overrides.Visual {

    public sealed class OperatorAssign : VisualStatementExpression<double> {
        public string VariableName { get; set; }
        public IVisualExpression<double> Value { get; set; }

        public override Expression GetExpression(VisualProgram context) => // Can also use DivideAssign, PowAssign, ModuloAssign, etc.
            VariableAccessorFactory.CreateAssignmentExpression(context, VariableName, v => Expression.AddAssign(v, Value.GetExpression(context)));
    }
}
