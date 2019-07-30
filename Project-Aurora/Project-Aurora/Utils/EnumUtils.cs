using Aurora.Devices;
using Aurora.Profiles.Desktop;
using Aurora.Profiles.GTA5;
using Aurora.Profiles.Payday_2.GSI.Nodes;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Aurora.Settings.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Markup;

namespace Aurora.Utils {
    public static class EnumUtils {
        
        /// <summary>Returns the attribute of the given type for this enum.</summary>
        public static TAttr GetCustomAttribute<TAttr>(this Enum enumObj) where TAttr : Attribute =>
            enumObj.GetType().GetField(enumObj.ToString()).GetCustomAttribute(typeof(TAttr), false) as TAttr;

        /// <summary>Gets the description for this enum value.</summary>
        public static string GetDescription(this Enum enumObj) => GetCustomAttribute<DescriptionAttribute>(enumObj)?.Description ?? enumObj.ToString();

        /// <summary>Takes a particular type of enum and returns all values of the enum and their associated description in a list suitable for use as an ItemsSource.
        /// Returns an enumerable of KeyValuePairs where the key is the description/name and the value is the enum's value.</summary>
        /// <typeparam name="T">The type of enum whose values to fetch.</typeparam>
        public static IEnumerable<KeyValuePair<string, T>> GetEnumItemsSource<T>() =>
            typeof(T).GetEnumValues().Cast<T>().Select(@enum => new KeyValuePair<string, T>(
                typeof(T).GetMember(@enum.ToString()).FirstOrDefault()?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? @enum.ToString(),
                @enum
            ));

        /// <summary>Takes a particular type of enum and returns all values of the enum and their associated description in a list suitable for use as an ItemsSource.
        /// Returns an enumerable of KeyValuePairs where the key is the description/name and the value is the enum's value.</summary>
        /// <param name="enumType">The type of enum whose values to fetch.</param>
        public static IEnumerable<KeyValuePair<string, object>> GetEnumItemsSource(Type enumType) =>
            enumType.GetEnumValues().Cast<object>().Select(@enum => new KeyValuePair<string, object>(
                enumType.GetMember(@enum.ToString()).FirstOrDefault()?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? @enum.ToString(),
                @enum
            ));
    }

    public static class IValueConverterExt {
        public static Enum StringToEnum(this IValueConverter conv, Type t, string name) {
            return (Enum)Enum.Parse(t, name);
        }
    }

    /// <summary>
    /// Markup Extension that takes an enum type and returns a collection of anonymous objects containing all the enum values, with "Text"
    /// as the <see cref="DescriptionAttribute"/> of the enum item, "Value" as the enum value itself and "Group" as the <see cref="CategoryAttribute"/>.
    /// <para>Set the <see cref="System.Windows.Controls.ItemsControl.DisplayMemberPath"/> to "Text" and
    /// <see cref="System.Windows.Controls.Primitives.Selector.SelectedValuePath"/> to "Value", or use the StaticResource templates.</para>
    /// </summary>
    public class EnumToItemsSourceExtension : MarkupExtension {

        private readonly Type enumType;

        public bool DoGroup { get; set; } = false;

        public EnumToItemsSourceExtension(Type enumType) {
            this.enumType = enumType;
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => enumType == null ? (object)new { } : GetListFor(enumType, DoGroup);

        /// <summary>
        /// Creates a <see cref="ListCollectionView"/> for the given enum type. The items in the collection have properties: 'Text', 'Value',
        /// 'Group', 'LocalizationKey' and 'LocalizationPackage'.
        /// </summary>
        public static ListCollectionView GetListFor(Type enumType, bool doGroup = false) {
            var lcv = new ListCollectionView(Enum.GetValues(enumType)
                .Cast<Enum>()
                .Select(e => new {
                    Text = e.GetDescription(),
                    Value = e,
                    Group = e.GetCustomAttribute<CategoryAttribute>()?.Category ?? "",
                    LocalizationKey = e.GetCustomAttribute<LocalizedDescriptionAttribute>()?.Key,
                    LocalizationPackage = e.GetCustomAttribute<LocalizedDescriptionAttribute>()?.Package ?? TranslationSource.DefaultPackage
                })
                .ToList()
            );
            if (doGroup) lcv.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
            return lcv;
        }
    }

    /// <summary>
    /// A class that provides a list of all enum values for the given enum. Similar to the <see cref="EnumToItemsSourceExtension"/>, but can accept a binding.
    /// </summary>
    public class EnumBindingToItemsSource : Binding {

        public EnumBindingToItemsSource() : base() {
            Converter = new EnumListConverter();
        }

        public EnumBindingToItemsSource(string path) : base(path) {
            Converter = new EnumListConverter();
        }

        class EnumListConverter : IValueConverter {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => EnumToItemsSourceExtension.GetListFor((Type)value, true);
            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
        }
    }
}