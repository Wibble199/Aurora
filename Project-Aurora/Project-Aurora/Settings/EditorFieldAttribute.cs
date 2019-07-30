using System;
using System.Runtime.CompilerServices;
using TS = Aurora.Settings.Localization.TranslationSource;

namespace Aurora.Settings {

    /// <summary>
    /// Attribute that can be applied to properties of <see cref="Profiles.IStringProperty"/> classes to  define metadata about
    /// the property. This metadata will then be read by <see cref="Controls.StringPropertyEditor"/> to determine how
    /// that property will be visually diplayed to the user.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class EditorFieldAttribute : Attribute {

        public EditorFieldAttribute([CallerMemberName] string _name = null, [CallerLineNumber] int _order = 0) {
            Name = _name;
            Order = _order;
        }

        /// <summary>The non-localized name given to this property. Will be ignored when <see cref="LocName"/> has a value.</summary>
        public string Name { get; set; }
        /// <summary>The non-localized description given to this property. Will be ignored when <see cref="LocDescription"/> has a value.</summary>
        public string Description { get; set; }

        /// <summary>The ID of the category this property is grouped under.</summary>
        public string CategoryID { get; set; }

        /// <summary>The key to use as a lookup when fetching the localized name for this property.</summary>
        public string LocName { get; set; }
        /// <summary>The package to pass to the lookup when fetching the localized name for this property.</summary>
        public string LocNamePackage { get; set; } = TS.DefaultPackage;

        /// <summary>The key to use as a lookup when fetching the localized description for this property.</summary>
        public string LocDescription { get; set; }
        /// <summary>The package to pass to the lookup when fetching the localized description for this property.</summary>
        public string LocDescriptionPackage { get; set; } = TS.DefaultPackage;

        /// <summary>Shortcut to set both <see cref="LocNamePackage"/> and <see cref="LocDescriptionPackage"/> at the same time.</summary>
        public string LocPackage { set => LocNamePackage = LocDescriptionPackage = value; }

        /// <summary>A value used to sort the properties and determine the order in which they will appear, lower values being first.
        /// The constructor for this attrbiute gives the Order a default value equal to the line number, making properties sort in the order
        /// they were defined in the code.</summary>
        public int Order { get; set; }

        /// <summary>A value that will be compared to before the value of the property is set, ensuring it remains within the constraints. The
        /// type must implement <see cref="IComparable{T}"/> for this to be used.</summary>
        public object Min { get; set; }
        /// <summary>A value that will be compared to before the value of the property is set, ensuring it remains within the constraints. The
        /// type must implement <see cref="IComparable{T}"/> for this to be used.</summary>
        public object Max { get; set; }

        /// <summary>For numeric types, this value will determine the step size of the numeric stepper or slider that is displayed to the user.</summary>
        public double Step { get; set; }

        /// <summary>A value that will be compared to before teh value of the property is set. Only applies to string types.</summary>
        public int MaxLength { get; set; }
        /// <summary>A value that will be compared to before teh value of the property is set. Only applies to string types.</summary>
        public int MinLength { get; set; }

        /// <summary>Gets the label that should be used to display this field. Will perform localization if required.</summary>
        public string DisplayName => string.IsNullOrWhiteSpace(LocName) ? Name : TS.Instance[LocName, LocNamePackage];
        /// <summary>Gets the description that will be displayed next to this field. Will perform localization if required.</summary>
        public string DisplayDescription => string.IsNullOrWhiteSpace(LocDescription) ? Description : TS.Instance[LocDescription, LocDescriptionPackage];
    }


    /// <summary>
    /// Attribute that can be applied to <see cref="Profiles.IStringProperty"/> classes to define metadata for categories that are used to
    /// categories the properties in the class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class CategoryMetadataAttribute : Attribute {

        public CategoryMetadataAttribute(string categoryId) {
            CategoryID = categoryId;
        }

        /// <summary>An identifying ID for this category. This is the value used for <see cref="EditorFieldAttribute.CategoryID"/> field.</summary>
        public string CategoryID { get; set; }

        /// <summary>The key to use as a lookup when fetching the localized name for this category.</summary>
        public string LocName { get; set; }
        /// <summary>The key to use as a lookup when fetching the localized description for this category.</summary>
        public string LocNamePackage { get; set; } = TS.DefaultPackage;

        /// <summary>A value used to sort the categories for the grouping. Lower 'order' numbers will appear earlier.</summary>
        public int Order { get; set; } = int.MaxValue;

        /// <summary>Gets the label that should be used for this category. Will perform localization if required.</summary>
        public string DisplayName => string.IsNullOrWhiteSpace(LocName) ? CategoryID : TS.Instance[LocName, LocNamePackage];
    }


    /// <summary>
    /// Attribute that can be applied to <see cref="Profiles.IStringProperty"/> classes to allow adding or updating metadata for a property in
    /// the class.
    /// </summary>
    /// <seealso cref="EditorFieldAttribute"/>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class EditorFieldAttributeFor : EditorFieldAttribute {
        /// <summary>The name (as set in code) of the target property that this metadata is for.</summary>
        public string PropertyName { get; set; }
    }
}
