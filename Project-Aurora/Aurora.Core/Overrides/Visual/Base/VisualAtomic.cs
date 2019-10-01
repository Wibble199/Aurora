using System;

namespace Aurora.Core.Overrides.Visual.Base {

    public abstract class VisualAtomic : ICloneable {
        /// <summary>
        /// A default cloning implementation that creates a new instance of this type and copies the value
        /// of any properties on this type (any property that implements ICloneable will also be cloned).
        /// </summary>
        /// <remarks>
        /// This method should be overriden if there are any other properties/fields/behaviours that need
        /// to be handled differently during cloning.
        /// </remarks>
        public virtual VisualAtomic Clone()
        {
            var @new = (VisualAtomic)Activator.CreateInstance(GetType());
            foreach (var prop in GetType().GetProperties())
            {
                var val = prop.GetValue(this);
                if (val is ICloneable c) val = c.Clone();
                prop.SetValue(@new, val);
            }
            return @new;
        }
        object ICloneable.Clone() => Clone();
    }
}
