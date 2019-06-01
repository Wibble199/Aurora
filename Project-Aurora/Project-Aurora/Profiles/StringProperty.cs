using Aurora.Settings.Localization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Aurora.Profiles {

    /// <summary>
    /// Indicates that any implementing classes have the ability to have their properties inspected or written by name.
    /// </summary>
    /// <seealso cref="StringProperty{TBase}"/>
    public interface IStringProperty {
        IReadOnlyDictionary<string, IMember> PropertyLookup { get; }
        object GetValueFromString(string name, object input = null);
        void SetValueFromString(string name, object value);
        IStringProperty Clone();
    }


    /// <summary>
    /// Provides a property lookup for all properties on the given TBase class. Also provides the ability to get and set these properties
    /// by name quickly using a compiled expression (faster than using <see cref="PropertyInfo.SetValue(object, object)"/>).
    /// </summary>
    public abstract class StringProperty<TBase> : IStringProperty where TBase : StringProperty<TBase> {
        public static IReadOnlyDictionary<string, Member<TBase>> PropertyLookup { get; set; } = null;
        public static object DictLock = new object();

        public StringProperty() {
            PropertyLookup = GeneratePropertyLookup<TBase>();
        }

        // Interface member
        IReadOnlyDictionary<string, IMember> IStringProperty.PropertyLookup => new ReadOnlyDictionary<string, IMember>(PropertyLookup.ToDictionary(kvp => kvp.Key, kvp => (IMember)kvp.Value));

        /// <summary>
        /// Generates a property lookup dictionary for the given type, `T`.
        /// </summary>
        /// <typeparam name="T">The type to create the dictionary from.</typeparam>
        public static IReadOnlyDictionary<string, Member<T>> GeneratePropertyLookup<T>() {
            lock (DictLock) {

                var lookup = new Dictionary<string, Member<T>>();
                var thisType = typeof(T);

                // For every member in the type extending this class
                foreach (var member in thisType.GetMembers()) {
                    ParameterExpression paramExpression = Expression.Parameter(thisType);

                    // Ignore anything not a field or property (methods, ctors, etc.)
                    if (member.MemberType != MemberTypes.Property && member.MemberType != MemberTypes.Field)
                        continue;

                    // Get the type represented by this member
                    var memberType = member.MemberType == MemberTypes.Property ? ((PropertyInfo)member).PropertyType : ((FieldInfo)member).FieldType;


                    // Create a getter lambda that will take an instance of type T and return the value of this member in that instance
                    var getter = Expression.Lambda<Func<T, object>>(
                        // Body
                        Expression.Convert(
                            Expression.PropertyOrField(paramExpression, member.Name),
                            typeof(object)
                        ),
                        // Params
                        paramExpression
                    ).Compile();


                    // Create a setter lambda that will take an instance of type T and a value and set the instance's member to value
                    Action<T, object> setter = null;
                    if (!(member.MemberType == MemberTypes.Property && ((PropertyInfo)member).SetMethod == null)) {
                        ParameterExpression objectTypeParam = Expression.Parameter(typeof(object));
                        MemberExpression propertyGetterExpression = Expression.PropertyOrField(paramExpression, member.Name);

                        setter = Expression.Lambda<Action<T, object>>(
                            // Body
                            Expression.Assign(
                                propertyGetterExpression,
                                Expression.ConvertChecked(objectTypeParam, memberType)
                            ),
                            // Params
                            paramExpression, objectTypeParam
                        ).Compile();
                    }

                    // Add this member to the property lookup dictionary. Now with named class instead of nameless tuples :)
                    var lna = member.GetCustomAttribute<LocalizedNameAttribute>();
                    var lda = member.GetCustomAttribute<LocalizedDescriptionAttribute>();
                    if (!lookup.ContainsKey(member.Name))
                        lookup.Add(member.Name, new Member<T> {
                            Name = member.Name,
                            DisplayName = member.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? member.Name,
                            NameLocalizationKey = lna?.Key,
                            NameLocalizationPackage = lna?.Package,
                            Description = member.GetCustomAttribute<DescriptionAttribute>()?.Description ?? "",
                            DescriptionLocalizationKey = lda?.Key,
                            DescriptionLocalizationPackage = lda?.Package,
                            Get = getter,
                            Set = setter,
                            MemberType = memberType,
                            NonNullableMemberType = Utils.TypeUtils.GetNonNullableOf(memberType)
                        });
                }

                return lookup;
            }
        }

        public object GetValueFromString(string name, object input = null) =>
            PropertyLookup.ContainsKey(name) ? PropertyLookup[name].Get((TBase)(object)this) : null;

        public void SetValueFromString(string name, object value) {
            if (PropertyLookup.ContainsKey(name))
                // Convert the value to the right type, though if null is given, don't try to cast it
                PropertyLookup[name].Set((TBase)(object)this, value == null ? null : Convert.ChangeType(value, PropertyLookup[name].NonNullableMemberType));
        }

        public IStringProperty Clone() => (IStringProperty)MemberwiseClone();
    }

    /// <summary>
    /// Functionally identical to the <see cref="StringProperty{TBase}"/>, but implenting <see cref="INotifyPropertyChanged"/> and providing
    /// a method to raise the <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
    /// </summary>
    public class StringPropertyNotify<TBase> : StringProperty<TBase>, INotifyPropertyChanged where TBase : StringPropertyNotify<TBase> {

        /// <summary>Fired when a property in this instance changes.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>Sets the given referenced field to the given value. If the field does not equal the value, the <see cref="PropertyChanged"/> event is fired.</summary>
        internal void SetAndNotify<TField>(ref TField field, TField value, [CallerMemberName] string propertyName = null) {
            if (Comparer<TField>.Default.Compare(field, value) != 0) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }


    /// <summary>
    /// Class that holds pre-compiled delegates for getting/setting a particular property of by name.
    /// </summary>
    public class Member<T> : IMember {
        /// <summary>The code name of the member.</summary>
        public string Name { get; internal set; }

        /// <summary>The static (unlocalized) display name of the member.</summary>
        public string DisplayName { get; internal set; }

        /// <summary>A localization key for the description provided by a <see cref="LocalizedNameAttribute"/> on the this member.</summary>
        public string NameLocalizationKey { get; internal set; }
        public string NameLocalizationPackage { get; internal set; }

        /// <summary>An unlocalized description provided by a <see cref="DescriptionAttribute"/> on the this member.</summary>
        public string Description { get; internal set; }

        /// <summary>A localization key for the description provided by a <see cref="LocalizedDescriptionAttribute"/> on the this member.</summary>
        public string DescriptionLocalizationKey { get; internal set; }
        public string DescriptionLocalizationPackage { get; internal set; }

        /// <summary>A function that when called and provided with an instance of T, returns the value of this member.</summary>
        public Func<T, object> Get { get; internal set; }
        Func<object, object> IMember.Get => new Func<object, object>((target) => Get((T)target));

        /// <summary>An action that when called and provided with an instance of T and an object, will set the value of this member on T to the value.</summary>
        public Action<T, object> Set { get; internal set; }
        Action<object, object> IMember.Set => new Action<object, object>((target, value) => Set((T)target, value));

        /// <summary>The type of member as it appears in the T definition.</summary>
        public Type MemberType { get; internal set; }

        /// <summary>The type of member, coerced to be non-nullable. If <see cref="MemberType"/> is non-nullable, this will be the same.</summary>
        public Type NonNullableMemberType { get; internal set; }
    }

    /// <summary>Type-less <see cref="Member{T}"/> interface.</summary>
    public interface IMember {
        public string Name { get; }
        public string DisplayName { get; }
        public string NameLocalizationKey { get; }
        public string NameLocalizationPackage { get; }
        public string Description { get; }
        public string DescriptionLocalizationKey { get; }
        public string DescriptionLocalizationPackage { get; }
        public Func<object, object> Get { get; }
        public Action<object, object> Set { get; }
        public Type MemberType { get; }
        public Type NonNullableMemberType { get; }
    }
}
