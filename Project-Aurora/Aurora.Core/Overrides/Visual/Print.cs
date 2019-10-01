using Aurora.Core.Overrides.Visual.Base;
using System.Linq.Expressions;
using System.Reflection;

namespace Aurora.Core.Overrides.Visual
{
    public class Print : VisualStatement
    {

        public string Output { get; set; }
        private static MethodInfo method = typeof(System.Console).GetMethod("WriteLine", new[] { typeof(object) });

        public override Expression GetStatement() => Expression.Call(method, Expression.Constant(Output));
    }
}
