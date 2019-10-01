using Aurora.Core.Overrides.Visual.Base;
using System.Linq.Expressions;
using System.Reflection;

namespace Aurora.Core.Overrides.Visual
{
    public class Print : VisualStatement
    {

        public VisualExpression<int> Output { get; set; }
        private static MethodInfo method = typeof(System.Console).GetMethod("WriteLine", new[] { typeof(object) });

        public override Expression GetStatement(VisualProgram context) => Expression.Call(method, Expression.Convert(Output.GetExpression(context), typeof(object)));
    }
}
