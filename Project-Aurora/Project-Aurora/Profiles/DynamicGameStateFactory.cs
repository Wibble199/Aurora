using Aurora.Profiles.Generic;
using Aurora.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using static System.Reflection.Emit.OpCodes;

namespace Aurora.Profiles {

    /// <summary>
    /// Dynamically creates a <see cref="GameState"/> based on the supplied JSON schema.
    /// </summary>
    public static class DynamicGameStateFactory {

        // The directory that stores the JSON schemas for the different nodes
        private readonly static string SchemaDirectory = System.IO.Path.Combine(Global.ExecutingDirectory, "Schemas");

        /// <summary>
        /// Creates a <see cref="GameState{T}"/> for the schema with the specified name.
        /// </summary>
        public static Type CreateGameState(string schemaName) {
            // Deserialize the schema
            using (var stream = new FileStream(Path.Combine(SchemaDirectory, schemaName), FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream, Encoding.UTF8)) {
                var serializer = new JsonSerializer();
                var schema = (JObject)serializer.Deserialize(reader, typeof(JObject));
                return CreateNode(typeof(GameState<>), schema);
            }
        }

        // Reflection info needed for the ILGenerator
        public static readonly MethodInfo jObjectGetValueMethod = typeof(JObject).GetMethod(nameof(JObject.GetValue), new[] { typeof(string), typeof(StringComparison) });
        public static readonly MethodInfo objectToStringMethod = typeof(object).GetMethod(nameof(object.ToString));

        /// <summary>
        /// Creates the <see cref="Node{TClass}"/> type for a specific object in the schema.
        /// </summary>
        /// <param name="baseType">The base type. Should either be <see cref="Node{TClass}"/> or <see cref="GameState{T}"/>.</param>
        private static Type CreateNode(Type baseType, JObject jObj) {
            // Guard the parent type
            if (baseType != typeof(Node<>) && baseType != typeof(GameState<>))
                throw new ArgumentException("Parent type should either be Node<> or GameState<>.", nameof(baseType));

            // Create type builder
            var builder = new DynamicTypeBuilder();

            // Create a type that represents Node<T> or GameState<T> where T is our new dynamic type which our new type needs to extend.
            // We set the Reflection.Emit.TypeBuilder's parent to be a new node based on the TypeBuilder type.
            // This works because Reflection.Emit.TypeBuilder extends System.Type.
            // This is equivilent to doing "class MyClass : Node<MyClass> { }"
            var parentType = baseType.MakeGenericType(builder.TypeBuilder);
            builder.TypeBuilder.SetParent(parentType);


            // Create a multicast delegate that will create the IL for the new Node class's constructor
            Action<ILGenerator> constructor = il => {
                // Call the base Node constructor (passing it the JSON string [Ldarg_1]).
                // Need to use TypeBuilder GetConstructor because we cannot get the constructor of BaseType (which is Node<OurCustomType>) since OurCustomType doesn't exist yet
                // Thus, trying to do baseType.GetConstructor(new[] { typeof(string) }) throws an exception, but using TypeBuilder.GetConstructor works.
                il.Emit(Ldarg_0);
                il.Emit(Ldarg_1);
                il.Emit(Call, TypeBuilder.GetConstructor(parentType, baseType.GetConstructor(new[] { typeof(string) })));
            };


            #region Reused helper Methods
            // Create a lambda which emits IL code onto the given ILGenerator which sets the target field to the relevant getter for the target type and name.
            Action<ILGenerator> CreateConstructorFieldSetterLambda(FieldBuilder field, Type type, string jsonPathName) => il => {
                il.Emit(Ldarg_0);
                il.Emit(Ldarg_0);
                il.Emit(Ldstr, jsonPathName); // String to pass to the Get method
                il.Emit(Call, TypeBuilder.GetMethod(parentType, GetGetterFor(type))); // Call the relevant method for this type, e.g. GetInt, GetString etc.
                il.Emit(Stfld, field);
            };

            // Creates a lazy property for the given Node<T> type, including the lazy initialisation.
            // Running this effectively adds the following to the builder:
            //      private T backingField__Name;
            //      public T Name => backingField__Name ?? (backingField__Name = new T(_ParsedData["Name"]?.ToString() ?? ""));
            void CreateLazyNodePropertyOnBuilder(string propName, Type type, string jsonPath) {
                // Create a lazily evaluated property of this type, with the following value initialisation
                builder.CreateLazyProperty(propName, type, il => {
                    // This is IL equivilent of new NodeType(_ParsedData["JSONKEY"]?.ToString() ?? "")
                    var toStringLabel = il.DefineLabel();
                    var checkNotNullLabel = il.DefineLabel();
                    var returnValueLabel = il.DefineLabel();

                    // Load the _ParsedData field, which contains the JSON object for the current 
                    // Note that on GameState<T>, _ParsedData is a property, but on Node<T> is is a field. Yay for consistency..... Means we either need to call the getter or load the field depending on the type we are creating
                    il.Emit(Ldarg_0);
                    if (baseType == typeof(GameState<>))
                        il.Emit(Call, TypeBuilder.GetMethod(parentType, typeof(GameState<>).GetMethod("get__ParsedData", Type.EmptyTypes)));
                    else
                        il.Emit(Ldfld, TypeBuilder.GetField(parentType, typeof(Node<>).GetField("_ParsedData", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)));
                    
                    il.Emit(Ldstr, jsonPath);
                    il.Emit(Ldc_I4_S, (Int32)StringComparison.InvariantCultureIgnoreCase);
                    il.Emit(Callvirt, jObjectGetValueMethod);
                    il.Emit(Dup);
                    il.Emit(Brtrue_S, toStringLabel);
                    il.Emit(Pop);
                    il.Emit(Ldnull);
                    il.Emit(Br_S, checkNotNullLabel);
                    il.MarkLabel(toStringLabel);
                    il.Emit(Callvirt, objectToStringMethod);
                    il.MarkLabel(checkNotNullLabel);
                    il.Emit(Dup);
                    il.Emit(Brtrue_S, returnValueLabel);
                    il.Emit(Pop);
                    il.Emit(Ldstr, "");
                    il.MarkLabel(returnValueLabel);
                    il.Emit(Newobj, type.GetConstructor(new[] { typeof(string) }));
                });
            }
            #endregion

            
            // Loop over all properties in the schema
            foreach (var prop in jObj) {

                // Special name handling
                string name, jsonKey;
                if (prop.Key.Contains('~')) {
                    name = prop.Key.Split('~')[0];
                    jsonKey = prop.Key.Split('~')[1];
                } else {
                    name = jsonKey = prop.Key;
                }

                // If the property is an array of strings, treat it as an enum (create an enum for it)
                if (prop.Value is JArray enumVals) {
                    // Create the dynamic enum
                    var type = DynamicTypeBuilder.BuildEnum("DynamicEnum_" + name, enumVals.Select(x => x.ToString()));
                    builder.CreateField(name, type, out var field, DynamicTypeBuilder.AccessModifier.Public);

                    // Add code to the constructor to set the value of this field
                    constructor += CreateConstructorFieldSetterLambda(field, type, jsonKey);
                }

                // If the property is an object, treat it as a nested Node and create a new node
                else if (prop.Value is JObject nested) {
                    // Create another node from this nested information.
                    var nodeType = CreateNode(typeof(Node<>), nested);

                    // Create the lazy node
                    CreateLazyNodePropertyOnBuilder(name, nodeType, jsonKey);
                }

                // Otherwise the property is something like an int or string
                else {
                    // Create a field for this value
                    var type = Type.GetType(prop.Value.ToString());
                    builder.CreateField(name, type, out var field, DynamicTypeBuilder.AccessModifier.Public);

                    // Add code to the constructor to set the value of this field
                    constructor += CreateConstructorFieldSetterLambda(field, type, jsonKey);
                }
            }


            // If this is a GameState, we want to also add the provider node to it
            if (baseType == typeof(GameState<>))
                CreateLazyNodePropertyOnBuilder("Provider", typeof(ProviderNode), "provider");


            // Need to add a return instruction at the end of the ctor
            constructor += (ILGenerator il) => il.Emit(Ret);

            builder.CreateConstructor(constructor, new[] { typeof(string) });
            builder.CreateConstructor();

            return builder.BuiltType;
        }


        #region Node Get Methods
        /// <summary>
        /// Returns the relevant method on Node for the specific type.<para/>
        /// E.G. if <paramref name="type"/> is <see cref="int"/>, this will return <see cref="Node{TClass}.GetInt(string)"/>.
        /// </summary>
        private static MethodInfo GetGetterFor(Type type) {
            if (type.IsEnum)
                return typeof(Node<>).GetMethod(nameof(DummyNode.GetEnum), BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(type);
            else if (type.IsArray)
                return typeof(Node<>).GetMethod(nameof(DummyNode.GetArray), BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(type.GetElementType());
            else if (methodMap.TryGetValue(type, out var inf))
                return inf;
            throw new NotSupportedException($"The type {type.FullName} is not supported.");
        }

        // A map of types onto their relevant methods (excluding Enum and Array)
        private static readonly Dictionary<Type, MethodInfo> methodMap = new Dictionary<Type, string> {
            [typeof(bool)] = nameof(DummyNode.GetBool),
            [typeof(float)] = nameof(DummyNode.GetFloat),
            [typeof(int)] = nameof(DummyNode.GetInt),
            [typeof(long)] = nameof(DummyNode.GetLong),
            [typeof(string)] = nameof(DummyNode.GetString)
        }.ToDictionary(t => t.Key, t => typeof(Node<>).GetMethod(t.Value, BindingFlags.Instance | BindingFlags.NonPublic));

        // Dummy node only used to access the method infos (since we cannot do nameof on an unbound generic such as Node<>, and using nameof provides extra safety should the method be refactored)
        private class DummyNode : Node<DummyNode> { }
        #endregion
    }
}
