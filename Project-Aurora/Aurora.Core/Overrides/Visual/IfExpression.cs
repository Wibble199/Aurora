using System.Linq.Expressions;
using Aurora.Core.Overrides.Visual.Base;

namespace Aurora.Core.Overrides.Visual
{
    public sealed class IfExpression : VisualStatement
    {
        public IVisualExpression<bool> Condition { get; set; }
        public VisualBody TrueBody { get; set; } = new VisualBody();
        public VisualBody FalseBody { get; set; } = new VisualBody();

        public override Expression GetStatement(VisualProgram c) => Expression.IfThenElse(Condition.GetExpression(c), TrueBody.GetStatement(c), FalseBody.GetStatement(c));
    }
}
