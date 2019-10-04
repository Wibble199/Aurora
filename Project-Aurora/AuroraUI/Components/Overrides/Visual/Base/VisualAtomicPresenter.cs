using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AuroraUI.Components.Overrides.Visual.Base {

    /// <summary>
    /// A base class for any atomic based presenters (expression or statement presenters).
    /// </summary>
    public abstract class VisualAtomicPresenter : ComponentBase {

        /// <summary>
        /// A dictionary of all atomic types with their assigned controls.
        /// </summary>
        private static Dictionary<Type, Type> availableControls =
            Aurora.Utils.TypeUtils.GetTypesWithCustomAttribute<VisualPresenterProviderAttribute>()
                .Where(x => typeof(IComponent).IsAssignableFrom(x.Key))
                .ToDictionary(x => x.Value.PresenterFor, x => x.Key);

        /// <summary>
        /// Gets the presenter type for the target atomic type.
        /// </summary>
        protected static Type GetPresenterTypeFor(Type visualAtomicType)
            => availableControls.TryGetValue(visualAtomicType, out var t) ? t // Try get the exact type (this allows overriding generic types with a specific bound type)
             : visualAtomicType.IsGenericType && availableControls.TryGetValue(visualAtomicType.GetGenericTypeDefinition(), out t) ? t // If there is no specific type and the type is generic, see if there's an unbound type for it
             : null; // Otherwise, no control available

        /// <summary>
        /// Builds a render tree for the given atomic.
        /// </summary>
        /// <param name="builder">The render tree builder.</param>
        /// <param name="sequence">A reference to the sequence counter integer. This will be incremented for each created markup node.</param>
        /// <param name="atomic">The atomic to be presented.</param>
        protected static void BuildPresenterFor<TAtomic>(RenderTreeBuilder builder, ref int sequence, TAtomic atomic) where TAtomic : class {
            if (atomic != null && GetPresenterTypeFor(atomic.GetType()) is { } control) {
                builder.OpenComponent(sequence++, control);
                builder.AddAttribute(sequence++, "Atomic", atomic);
                builder.CloseComponent();
            } else if (atomic == null)
                builder.AddContent(sequence++, "[Empty]");
            else
                builder.AddMarkupContent(sequence++, $"<div class='editor-type-error'>Editor for the override atomic node of type '{atomic.GetType().Name}' is unavailable.</div>");
        }
    }
}
