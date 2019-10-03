using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AuroraUI.Controls.InputField {

    /// <summary>
    /// Base class for <see cref="InputField"/> and <see cref="InputFieldGeneric{TValue}"/> providing them a list of available controls that can be used.
    /// </summary>
    public abstract class InputFieldBase : ComponentBase {

        // A list of all available inner editor controls
        protected static Dictionary<(Type dataType, string specialName), Type> availableControls =
            Aurora.Utils.TypeUtils.GetTypesWithCustomAttribute<InputFieldControlAttribute>()
                .Where(x => typeof(ComponentBase).IsAssignableFrom(x.Key))
                .ToDictionary(x => (x.Value.Type, x.Value.SpecialName?.ToLower() ?? ""), x => x.Key);

        /// <summary>Gets the editor type that will be used for the given data type.</summary>
        protected static Type GetControlFor(Type dataType, string specialName = "")
            => availableControls.ContainsKey((dataType, specialName.ToLower())) ? availableControls[(dataType, specialName)] // Try find a matching type and special name
             : availableControls.ContainsKey((dataType, "")) ? availableControls[(dataType, specialName)] // If not, try find the type with no special name
             : null; // Found no matching control
    }
}
