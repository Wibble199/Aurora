using Aurora.Core.Overrides.Visual.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Aurora.Core.Overrides.Visual.Base {

    /// <summary>
    /// Represents an entry-point for the visual programming tool.
    /// </summary>
    public sealed class VisualEntry : VisualAtomic {

        /// <summary>
        /// A list of definitions of all parameters that will be passed to the compiled delegate during runtime.
        /// </summary>
        /// <remarks>This is readonly because the parameters will be pre-defined based on the context the compiled delegate will be run.</remarks>
        public IReadOnlyList<(string name, Type type)> Parameters { get; }

        /// <summary>
        /// A set of pairs that map a parameter onto a variable. When an entry is called, it will set the value of the relevant variable to the value of the parameter.
        /// </summary>
        public HashSet<(string parameterName, string variableName)> ParameterMap { get; set; } = new HashSet<(string parameterName, string variableName)>();
        
        /// <summary>
        /// The main body of statements that are executed when this entry point is run.
        /// </summary>
        public VisualBody Body { get; set; } = new VisualBody();

        #region Constructors
        public VisualEntry() : this((IEnumerable<(string, Type)>)null) { }
        public VisualEntry(IEnumerable<(string name, Type type)> parameters = null) { Parameters = (parameters ?? new (string, Type)[0]).ToList(); }
        public VisualEntry(params (string name, Type type)[] parameters) : this(parameters.AsEnumerable()) { }
        #endregion

        /// <summary>
        /// Creates a <see cref="LambdaExpression"/> that represents the expression tree to be invoked by
        /// this entry, including defining the parameters to be passed to the entry.
        /// </summary>
        public LambdaExpression GetLambda(VisualProgram context) {
            // Create a Linq ParameterExpression for each expected parameter that will be passed to the compiled delegate
            var @params = Parameters.Select(p => (p.name, expression: Expression.Parameter(p.type, p.name)));
            // Note: A dictionary was not used here (although we do need to get an expression by name later and name would make a suitable key) because a
            // dictionary's order is not guaranteed and so when the ParameterExpressions were passed to the lambda method, they may end up in a different
            // order than they are defined, meaning the expected signature of the compiled delegate would not match the compiled signature.

            return Expression.Lambda(
                Expression.Block(
                    ParameterMap.Select((kvp, i) =>
                        VariableAccessorFactory.CreateSetterExpression(context, kvp.variableName, @params.Single(p => p.name == kvp.parameterName).expression)  // Create setters that copy any desired parameters from the entry into their specified variables
                    ).Concat(
                    Body.Statements.Select(s => s.GetStatement(context))) // Get the statements that make up the main code body
                ),
                @params.Select(p => p.expression) // We need to pass the expressions to the Lambda method so it knows what parameters to expect
            );
        }
    }
}
