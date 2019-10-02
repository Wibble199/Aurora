using Aurora.Core.Overrides.Visual.Base;
using System.Linq.Expressions;
using System.Reflection;

namespace Aurora.Core.Overrides.Visual
{
    public sealed class Print : VisualStatement
    {

        public IVisualExpression<double> Output { get; set; }
        private static MethodInfo method = typeof(System.Console).GetMethod("WriteLine", new[] { typeof(object) });

        public override Expression GetStatement(VisualProgram context) => Expression.Call(method, Expression.Convert(Output.GetExpression(context), typeof(object)));
    }
}
