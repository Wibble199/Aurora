using System;
using System.Linq.Expressions;

namespace Aurora.Core.Overrides.Visual.Base {

    /// <summary>
    /// An expression represents an action which can be evaluated to produce a value of some type.
    /// </summary>
    /// <typeparam name="TOut">The variable type that is produced by this expression.</typeparam>
    public abstract class VisualExpression<TOut> : VisualAtomic, IVisualExpression<TOut> {
        public abstract Expression GetExpression(VisualProgram context);
        public virtual new VisualExpression<TOut> Clone() => (VisualExpression<TOut>)base.Clone();
    }

    /// <summary>
    /// An expression represents an action which can be evaluated to produce a value of some type.
    /// </summary>
    /// <typeparam name="TOut">The variable type that is produced by this expression.</typeparam>
    public interface IVisualExpression<out TOut> : ICloneable {
        Expression GetExpression(VisualProgram context);
    }
}
