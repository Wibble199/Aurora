using System.Linq.Expressions;
using Aurora.Core.Overrides.Visual.Base;
using Aurora.Core.Overrides.Visual.Utils;

namespace Aurora.Core.Overrides.Visual
{
    public sealed class VariableSetter<T> : VisualStatement {        

        public string VariableName { get; set; }
        public IVisualExpression<T> Value { get; set; }

        public override Expression GetStatement(VisualProgram context) => VariableAccessorFactory.CreateSetterExpression(context, VariableName, Value.GetExpression(context));
    }
}
