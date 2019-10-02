using System.Linq.Expressions;

namespace Aurora.Core.Overrides.Visual.Base
{

    /// <summary>
    /// A statement-expression is a node that can be used as both an expression and a statement depending on it's context.
    /// An example of this is the ++ operator, which can be used as a statement to just increment the value, or as an expression
    /// to increment and return the value.
    /// </summary>
    public abstract class VisualStatementExpression<TOut> : VisualAtomic, IVisualStatement, IVisualExpression<TOut> {
        public abstract Expression GetExpression(VisualProgram context);
        public virtual Expression GetStatement(VisualProgram context) => GetExpression(context);
    }
}
