using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Aurora.Core.Overrides.Visual.Base;

namespace Aurora.Core.Overrides.Visual {
    public class VariableGetter<T> : VisualExpression<T> {

        private static MethodInfo dictionaryGetter = typeof(Dictionary<string, object>).GetMethod("get_Item");

        public string VariableName { get; set; }

        public override Expression GetExpression(VisualProgram context) =>
            Expression.Convert(
                Expression.Call(
                    Expression.Constant(context.VariableValues), // The instance on which the method will be called (in this case, the variable dictionary)
                    dictionaryGetter, // The method that will be called on the instance (the dictionary's get_Item method which is what dict[x] uses)
                    Expression.Constant(VariableName) // The name of the custom variable in the dictionary
                ),
                typeof(T) // Convert the value from an object into the type that is requested (e.g. string)
            );
    }
}
