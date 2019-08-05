using Aurora.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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
    public abstract class StringProperty<TBase> : AutoNotifyPropertyChanged<StringProperty<TBase>>, IStringProperty where TBase : StringProperty<TBase> {
        public static IReadOnlyDictionary<string, Member<TBase>> PropertyLookup { get; set; } = null;

        public StringProperty() {
            PropertyLookup = StringProperty.GeneratePropertyLookup<TBase>();
        }

        public static new TBase Create() =>
            Global.ProxyGenerator.CreateClassProxy<TBase>(new NotifyChangedInterceptor());

        // Interface member
        IReadOnlyDictionary<string, IMember> IStringProperty.PropertyLookup => new ReadOnlyDictionary<string, IMember>(PropertyLookup.ToDictionary(kvp => kvp.Key, kvp => (IMember)kvp.Value));

        /// <summary>Gets the value of the target property from this instance.</summary>
        public object GetValueFromString(string propertyName, object input = null) =>
            PropertyLookup.ContainsKey(propertyName) ? PropertyLookup[propertyName].Get((TBase)(object)this) : null;

        /// <summary>Sets the value of the target property to the given value on this instance.</summary>
        public void SetValueFromString(string propertyName, object value) {
            if (PropertyLookup.ContainsKey(propertyName))
                // Convert the value to the right type, though if null is given, don't try to cast it
                PropertyLookup[propertyName].Set((TBase)(object)this, value == null ? null : Convert.ChangeType(value, PropertyLookup[propertyName].NonNullableMemberType));
        }

        public IStringProperty Clone() => (IStringProperty)MemberwiseClone();
    }

    public static class StringProperty {

        /// <summary>Contains a list of type-specific lock objects.</summary>
        private static Dictionary<Type, object> DictLocks = new Dictionary<Type, object>();

        /// <summary>Gets or creates the type-specific lock object for the given type.</summary>
        private static object GetLockObject(Type t) => DictLocks.TryGetValue(t, out var @lock) ? @lock : (DictLocks[t] = new object());

        /// <summary>
        /// Generates a property lookup dictionary for the given type, `T`.
        /// </summary>
        public static IReadOnlyDictionary<string, Member<T>> GeneratePropertyLookup<T>() {
            lock (GetLockObject(typeof(T))) {

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

                    // Get the metadata for this property.
                    var meta = thisType.GetCustomAttributes<EditorFieldAttributeFor>(inherit: true).FirstOrDefault(a => a.PropertyName == member.Name) // Prioritise any EditorFieldAttributeFors on the declaring class
                        ?? member.GetCustomAttribute<EditorFieldAttribute>(inherit: true) // If none are found, get the EditorFieldAttribute defined on the property
                        ?? new EditorFieldAttribute(member.Name, int.MaxValue); // If it is not provided, create a default one

                    // Add this member to the property lookup dictionary. Now with named class instead of nameless tuples :)
                    if (!lookup.ContainsKey(member.Name))
                        lookup.Add(member.Name, new Member<T> {
                            Name = member.Name,
                            Metadata = meta,
                            Get = getter,
                            Set = setter,
                            MemberType = memberType,
                            NonNullableMemberType = Utils.TypeUtils.GetNonNullableOf(memberType)
                        });
                }

                return lookup;
            }
        }
    }


    /// <summary>
    /// Class that holds pre-compiled delegates for getting/setting a particular property of by name.
    /// </summary>
    public class Member<T> : IMember {
        /// <summary>The code name of the member.</summary>
        public string Name { get; internal set; }

        /// <summary>The metadata for this member.</summary>
        public EditorFieldAttribute Metadata { get; internal set; }

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
        public EditorFieldAttribute Metadata { get; }
        public Func<object, object> Get { get; }
        public Action<object, object> Set { get; }
        public Type MemberType { get; }
        public Type NonNullableMemberType { get; }
    }
}
