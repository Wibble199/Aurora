using Aurora.Core.Overrides.Visual.Base;
using System;
using System.Collections.Generic;

namespace Aurora.Core.Overrides.Visual {

    /// <summary>
    /// Provides the user a way of visually crafting code to generate the overrides in a block-based manner.
    /// Stores a collection of entry points (e.g. event handlers) into the visual program.
    /// </summary>
    public class VisualProgram : IOverrideLogic {

        public IList<VisualEntry> Entries { get; set; } = new List<VisualEntry>();
        public IDictionary<string, (Type type, object @default)> Variables { get; set; } = new Dictionary<string, (Type, object)>();

    }
}
