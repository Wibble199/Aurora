using Aurora.Core.Overrides.Visual.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace AuroraUI.Components.Overrides.Visual.Base {

    public class VisualBodyPresenter : ComponentBase {

        [Parameter] public VisualBody Body { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder) {
            var i = 0;
            builder.OpenElement(i++, "div");
            foreach (var statement in Body) {
                builder.OpenComponent<VisualStatementPresenter>(i++);
                builder.AddAttribute(i++, "Statement", statement);
                builder.CloseComponent();
            }
            builder.CloseElement();
        }
    }
}
