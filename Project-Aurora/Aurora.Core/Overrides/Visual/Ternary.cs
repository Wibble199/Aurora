using System.Linq.Expressions;
using Aurora.Core.Overrides.Visual.Base;

namespace Aurora.Core.Overrides.Visual
{
    public sealed class Ternary<TOut> : VisualExpression<TOut>
    {
        public IVisualExpression<bool> Condition { get; set; }
        public IVisualExpression<TOut> TrueExpr { get; set; }
        public IVisualExpression<TOut> FalseExpr { get; set; }

        public override Expression GetExpression(VisualProgram c) => Expression.Condition(Condition.GetExpression(c), TrueExpr.GetExpression(c), FalseExpr.GetExpression(c));
    }
}
