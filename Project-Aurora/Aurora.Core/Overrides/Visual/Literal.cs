using System.Linq.Expressions;
using Aurora.Core.Overrides.Visual.Base;

namespace Aurora.Core.Overrides.Visual {

    /// <summary>
    /// Expression that represents a constant/literal value.
    /// </summary>
    /// <typeparam name="T">The type of value that will be returned.</typeparam>
    public sealed class Literal<T> : VisualExpression<T> {
        public T Value { get; set; }
        public Literal(T value = default) => Value = value;
        public override Expression GetExpression(VisualProgram context) => Expression.Constant(Value);
    }
}
