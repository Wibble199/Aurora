using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Aurora.Core.Overrides.Visual.Base {

    /// <summary>
    /// Represents an entry-point for the visual programming tool.
    /// </summary>
    public class VisualEntry : VisualAtomic {

        public IEnumerable<(string name, Type type)> Parameters { get; }
        public Dictionary<string, string> ParameterMap { get; set; } = new Dictionary<string, string>();
        public VisualBody Body { get; set; } = new VisualBody();

        public VisualEntry(IEnumerable<(string name, Type type)> parameters = null) {
            Parameters = parameters ?? new (string, Type)[0];
        }

        /// <summary>
        /// Creates a <see cref="LambdaExpression"/> that represents the expression tree to be invoked by
        /// this entry, including defining the parameters to be passed to the entry.
        /// </summary>
        public LambdaExpression GetLambda(VisualProgram context) {
            var @params = Parameters.Select(p => Expression.Parameter(p.type, p.name)).ToList();
            return Expression.Lambda(
                Expression.Block(
                    ParameterMap.Select((kvp, i) => VariableSetter.GetStatement(context, kvp.Value, @params[i])).Concat( // Copy any required parameters into their specified parameters
                    Body.Statements.Select(s => s.GetStatement(context))) // Run the main code body
                ),
                @params
            );
        }
    }
}
