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
    public sealed class VisualBody : VisualStatement, IEnumerable<IVisualStatement> {

        public IList<IVisualStatement> Statements { get; set; } = new List<IVisualStatement>();

        public VisualBody() { }
        public VisualBody(params IVisualStatement[] statements) {
            foreach (var statement in statements)
                Add(statement);
        }

        public override Expression GetStatement(VisualProgram context) => Expression.Block(Statements.Select(s => s.GetStatement(context)));

        public static implicit operator VisualBody(IVisualStatement[] statements) => new VisualBody(statements);

        public void Add(IVisualStatement statement) => Statements.Add(statement);
        public IEnumerator<IVisualStatement> GetEnumerator() => Statements.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Statements.GetEnumerator();
    }
}
