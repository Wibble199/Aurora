using Aurora.Core.Overrides.Visual.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace AuroraUI.Components.Overrides.Visual.Base {

    public class VisualStatementPresenter : VisualAtomicPresenter {

        [Parameter] public IVisualStatement Statement { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder) {
            var i = 0;
            builder.OpenElement(i++, "div");
            builder.AddAttribute(i++, "class", "override-node atomic-statement");
            BuildPresenterFor(builder, ref i, Statement);
            builder.CloseElement();
        }
    }
}
