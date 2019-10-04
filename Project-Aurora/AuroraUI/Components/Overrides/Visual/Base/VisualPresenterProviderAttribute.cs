using System;

namespace AuroraUI.Components.Overrides.Visual.Base {

    public class VisualPresenterProviderAttribute : Attribute {

        /// <summary>
        /// The VisualAtomic type that the component being annotated represents.
        /// </summary>
        public Type PresenterFor { get; set; }

        /// <summary></summary>
        /// <param name="presenterFor">The VisualAtomic type that the component being annotated represents.</param>
        public VisualPresenterProviderAttribute(Type presenterFor) {
            PresenterFor = presenterFor;
        }
    }
}
