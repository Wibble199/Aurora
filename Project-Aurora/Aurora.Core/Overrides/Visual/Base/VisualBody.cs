using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Aurora.Core.Overrides.Visual.Base {

    /// <summary>
    /// Class that is capable of storing a collection of statements and producing a block expression from them.
    /// Other statements that need to use a statement body (such as if statements) should not extend this class,
    /// but have an instance of it as a property.
    /// </summary>
    public sealed class VisualBody : VisualStatement, IEnumerable<VisualStatement> {

        public IList<VisualStatement> Statements { get; set; } = new List<VisualStatement>();

        public override Expression GetStatement() => Expression.Block(Statements.Select(s => s.GetStatement()));

        public void Add(VisualStatement statement) => Statements.Add(statement);
        public IEnumerator<VisualStatement> GetEnumerator() => Statements.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Statements.GetEnumerator();
    }
}
