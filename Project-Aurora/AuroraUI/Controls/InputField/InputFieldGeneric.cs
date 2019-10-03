using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace AuroraUI.Controls.InputField {

    /// <summary>
    /// Represents a control that has a particular (variable) type and presents a relevant control to the user.
    /// <para/>
    /// This is the generic form of the InputField for use when the type of data being edited IS known at compile time.
    /// Unlike it's non-generic counterpart, this component can be bound using @bind-Value to a property of the type being
    /// edited (e.g. int), you are not restricted to only being able to bind to object properties.
    /// <para/>
    /// Use the <see cref="InputField"/> component instead if the type is not known at compile time.
    /// </summary>
    public sealed class InputFieldGeneric<TValue> : InputFieldBase {

        [Parameter] public TValue Value { get; set; }
        [Parameter] public EventCallback<TValue> ValueChanged { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder) {

            if (GetControlFor(typeof(TValue)) is { } control) {
                // If a control exists, create markup for this control, adding the relevant Value and ValueChanged attributes
                builder.OpenComponent(0, control);
                builder.AddAttribute(1, "Value", Value);
                builder.AddAttribute(2, "ValueChanged", EventCallback.Factory.Create<TValue>(this, OnInputValueChanged));
                builder.CloseComponent();

            } else if (typeof(TValue).IsEnum) {
                // For enums, if a control exists for this specific enum use that (in the above branch), else use the default enum combo
                builder.OpenComponent<EnumCombobox<TValue>>(0);
                builder.AddAttribute(1, "SelectedItem", Value);
                builder.AddAttribute(2, "SelectedItemChanged", EventCallback.Factory.Create<TValue>(this, OnInputValueChanged));
                builder.CloseComponent();

            } else
                // If no control exists for this datatype, show a warning message
                builder.AddMarkupContent(0, $"<div class='editor-type-error'>Editor for the data type '{typeof(TValue).Name}' is unavailable.</div>");
        }

        private void OnInputValueChanged(TValue newValue) {
            Value = newValue;
            ValueChanged.InvokeAsync(Value);
        }
    }
}
