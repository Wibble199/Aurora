using System.Linq.Expressions;
using Aurora.Core.Overrides.Visual.Base;

namespace Aurora.Core.Overrides.Visual
{
    public class IfExpression : VisualStatement
    {
        public VisualExpression<bool> Condition { get; set; }
        public VisualBody TrueBody { get; set; } = new VisualBody();
        public VisualBody FalseBody { get; set; } = new VisualBody();

        public override Expression GetStatement() => Expression.IfThenElse(Condition.GetExpression(), TrueBody.GetStatement(), FalseBody.GetStatement());
    }
}
