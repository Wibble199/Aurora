using System;
using System.Linq.Expressions;
using Aurora.Core.Overrides.Visual.Base;

namespace Aurora.Core.Overrides.Visual {

    /// <summary>
    /// Expression that represents a constant/literal value.
    /// </summary>
    /// <typeparam name="T">The type of value that will be returned.</typeparam>
    public sealed class Literal<T> : VisualExpression<T>, ILiteral {
        public T Value { get; set; }
        public Literal(T value = default) => Value = value;
        public override Expression GetExpression(VisualProgram context) => Expression.Constant(Value);
        object ILiteral.Value { get => Value; set => Value = (T)value; }
    }

    public interface ILiteral : IVisualExpression {
        object Value { get; set; }
    }
}
