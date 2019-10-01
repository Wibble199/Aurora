using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Aurora.Core.Overrides.Visual.Base;

namespace Aurora.Core.Overrides.Visual {
    public class VariableSetter<T> : VisualStatement {        

        public string VariableName { get; set; }
        public VisualExpression<T> Value { get; set; }

        public override Expression GetStatement(VisualProgram context) =>VariableSetter.GetStatement(context, VariableName, Value.GetExpression(context));
    }

    internal static class VariableSetter {

        private static MethodInfo dictionarySetter = typeof(Dictionary<string, object>).GetMethod("set_Item");

        internal static Expression GetStatement(VisualProgram context, string variableName, Expression value) =>
            Expression.Call(
                Expression.Constant(context.VariableValues), // The instance on which the method will be called (in this case, the variable dictionary)
                dictionarySetter, // The method that will be called on the instance (the dictionary's set_Item method which is what dict[x] = y uses)
                Expression.Constant(variableName), // The name of the custom variable in the dictionary
                Expression.Convert(value, typeof(object)) // We need to manually convert the value to an object to be able to set it in the dictionary
            );
    }
}
