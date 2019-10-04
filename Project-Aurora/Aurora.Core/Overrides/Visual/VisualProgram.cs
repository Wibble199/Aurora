using Aurora.Core.Overrides.Visual.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aurora.Core.Overrides.Visual {

    /// <summary>
    /// Provides the user a way of visually crafting code to generate the overrides in a block-based manner.
    /// Stores a collection of entry points (e.g. event handlers) into the visual program.
    /// </summary>
    public sealed class VisualProgram : IOverrideLogic {

        /// <summary>
        /// All programmable entry points to the application.
        /// </summary>
        public List<VisualEntry> Entries { get; set; } = new List<VisualEntry>();

        /// <summary>
        /// Contains the definitions for all the variables in the program.
        /// </summary>
        public Dictionary<string, (Type type, object @default)> VariableDefinitions { get; set; } = new Dictionary<string, (Type, object)>();

        /// <summary>
        /// Contains the values for all the variables in the program. This should not be serialized.
        /// </summary>
        internal Dictionary<string, object> VariableValues { get; private set; } = new Dictionary<string, object>();

        /// <summary>
        /// Resets the variable values to their default values.
        /// </summary>
        public void ResetVariableValues() => VariableValues = VariableDefinitions.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.@default);

        /// <summary>
        /// Compiles all the entry points into delegate methods.
        /// </summary>
        public IEnumerable<Delegate> CompileAll() => Entries.Select(e => e.GetLambda(this).Compile());
    }
}
