﻿using System;
using System.Linq.Expressions;

namespace Aurora.Core.Overrides.Visual.Base {

    /// <summary>
    /// A statement is an instruction that can be executed. It does not return anything, but may use one
    /// or more <see cref="VisualExpression{TOut}"/>s during execution.
    /// </summary>
    public abstract class VisualStatement : VisualAtomic, IVisualStatement {
        public abstract Expression GetStatement(VisualProgram context);
        public new VisualStatement Clone() => (VisualStatement)base.Clone();
    }

    /// <summary>
    /// A statement is an instruction that can be executed. It does not return anything, but may use one
    /// or more <see cref="VisualExpression{TOut}"/>s during execution.
    /// </summary>
    public interface IVisualStatement : ICloneable {
        Expression GetStatement(VisualProgram context);
    }
}
