using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using static System.Linq.Expressions.Expression;

namespace Aurora.Core.Overrides.Visual.Utils {

    internal static class VariableAccessorFactory {

        private static MethodInfo dictionaryGetter = typeof(Dictionary<string, object>).GetMethod("get_Item");
        private static MethodInfo dictionarySetter = typeof(Dictionary<string, object>).GetMethod("set_Item");

        /// <summary>
        /// Creates an expression that gets the value of a visual program variable.
        /// </summary>
        /// <param name="context">The program context whose variable dictionary will be used.</param>
        /// <param name="variableName">The name of the variable in the dictionary.</param>
        /// <param name="type">The type that the variable should be cast to.</param>
        internal static Expression CreateGetterExpression(VisualProgram context, string variableName, Type type) =>
            Convert(
                Call(
                    Constant(context.VariableValues), // The instance on which the method will be called (in this case, the variable dictionary)
                    dictionaryGetter, // The method that will be called on the instance (the dictionary's get_Item method which is what dict[x] uses)
                    Constant(variableName) // The name of the custom variable in the dictionary
                ),
                type // Convert the value from an object into the type that is requested (e.g. string)
            );

        /// <summary>
        /// Creates an expression that sets the variable to the given expression value.
        /// </summary>
        /// <param name="context">The program context whose variable dictionary will be used.</param>
        /// <param name="variableName">The name of the variable in the dictionary.</param>
        /// <param name="value">The expression that represents the new value of the variable.</param>
        internal static Expression CreateSetterExpression(VisualProgram context, string variableName, Expression value) =>
            Call(
                Constant(context.VariableValues), // The instance on which the method will be called (in this case, the variable dictionary)
                dictionarySetter, // The method that will be called on the instance (the dictionary's set_Item method which is what dict[x] = y uses)
                Constant(variableName), // The name of the custom variable in the dictionary
                Convert(value, typeof(object)) // We need to manually convert the value to an object to be able to set it in the dictionary
            );

        /// <summary>
        /// Creates an expression that gets the target variable, performs an action on it, sets it back to the variable store and returns the new value.
        /// This can be used to implement nodes such as increment, addassign, etc.
        /// </summary>
        /// <param name="context">The program context whose variable dictionary will be used.</param>
        /// <param name="VariableName">The name of the variable in the dictionary.</param>
        /// <param name="action">Factory function to generate the expression that will be performed between reading and writing the variable. It is passed
        /// the parameter expression that can be read/written to.</param>
        internal static Expression CreateAssignmentExpression(VisualProgram context, string VariableName, Func<ParameterExpression, Expression> action) {
            var returnLabel = Label(typeof(double));
            var temp = Parameter(typeof(double), "temp");
            return Block(
                typeof(double),
                new[] { temp },
                Assign(temp, CreateGetterExpression(context, VariableName, typeof(double))),
                action(temp),
                CreateSetterExpression(context, VariableName, temp),
                Return(returnLabel, temp),
                Label(returnLabel, temp)
            );
        }
    }
}
