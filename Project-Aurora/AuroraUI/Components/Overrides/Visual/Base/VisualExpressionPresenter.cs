using Aurora.Core.Overrides.Visual.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;

namespace AuroraUI.Components.Overrides.Visual.Base {

    public class VisualExpressionPresenter : VisualAtomicPresenter {

        [Parameter] public IVisualExpression Expression { get; set; }
        [Parameter] public Type ExpressionType { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder) {
            var i = 0;
            builder.OpenElement(i++, "div");
            BuildPresenterFor(builder, ref i, Expression);
            builder.CloseElement();
        }
    }
}
