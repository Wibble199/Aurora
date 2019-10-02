using System.Linq.Expressions;
using Aurora.Core.Overrides.Visual.Base;
using Aurora.Core.Overrides.Visual.Utils;

namespace Aurora.Core.Overrides.Visual
{

    public sealed class VariableGetter<T> : VisualExpression<T> {

        public string VariableName { get; set; }

        public override Expression GetExpression(VisualProgram context) => VariableAccessorFactory.CreateGetterExpression(context, VariableName, typeof(T));
    }
}
