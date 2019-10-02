using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Aurora.Core.Overrides.Visual.Base;

namespace Aurora.Core.Overrides.Visual
{
    public sealed class NumericComparison : VisualExpression<bool>
    {
        public IVisualExpression<double> LHS { get; set; }
        public IVisualExpression<double> RHS { get; set; }
        public NumericComparsionOperator Operator { get; set; }

        public override Expression GetExpression(VisualProgram context) => exprMap[Operator](LHS.GetExpression(context), RHS.GetExpression(context));

        private static Dictionary<NumericComparsionOperator, Func<Expression, Expression, BinaryExpression>> exprMap = new Dictionary<NumericComparsionOperator, Func<Expression, Expression, BinaryExpression>> {
            { NumericComparsionOperator.EQ, Expression.Equal },
            { NumericComparsionOperator.NEQ, Expression.NotEqual },
            { NumericComparsionOperator.LT, Expression.LessThan },
            { NumericComparsionOperator.LTE, Expression.LessThanOrEqual },
            { NumericComparsionOperator.GT, Expression.GreaterThan },
            { NumericComparsionOperator.GTE, Expression.GreaterThanOrEqual }
        };
    }

    public enum NumericComparsionOperator
    {
        EQ,
        NEQ,
        LT,
        LTE,
        GT,
        GTE
    }
}
