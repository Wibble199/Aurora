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
        public VisualBody Body { get; set; } = new VisualBody();

        public VisualEntry(IEnumerable<(string name, Type type)> parameters = null) {
            Parameters = parameters ?? new (string, Type)[0];
        }

        /// <summary>
        /// Creates a <see cref="LambdaExpression"/> that represents the expression tree to be invoked by
        /// this entry, including defining the parameters to be passed to the entry.
        /// </summary>
        public LambdaExpression GetLambda() => Expression.Lambda(
            Body.GetStatement(),
            Parameters.Select(p => Expression.Parameter(p.type, p.name))
        );
    }
}
