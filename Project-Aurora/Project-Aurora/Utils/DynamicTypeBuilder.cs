using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using static System.Reflection.Emit.OpCodes;

namespace Aurora.Utils {

	public class DynamicTypeBuilder {

		// Note: We use a static name for the assembly (as opposed to a GUID for example) so that saving-loading workds properly (e.g. dynamic GSI node's enum type)
		private static readonly AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("DynamicTypeBuilderTypes"), AssemblyBuilderAccess.Run);
		private static readonly ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");

		public TypeBuilder TypeBuilder { get; }
		private TypeInfo builtType;

		public DynamicTypeBuilder() : this(null, null) { }
		public DynamicTypeBuilder(Type extends) : this(extends, null) { }
		public DynamicTypeBuilder(IEnumerable<Type> interfaces) : this(null, interfaces) { }
		public DynamicTypeBuilder(Type extends, IEnumerable<Type> interfaces) {
			TypeBuilder = moduleBuilder.DefineType("DynamicType_" + Guid.NewGuid().ToString(), TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout, extends);
			if (interfaces != null)
				foreach (var @interface in interfaces)
					TypeBuilder.AddInterfaceImplementation(@interface);
		}

		/// <summary>
		/// Allows creation of the code for this type's constructor. Can be called multiple times with different <paramref name="args"/> arrays if required.
		/// </summary>
		public DynamicTypeBuilder CreateConstructor(Action<ILGenerator> configure, Type[] args = null) {
			AssertNotBuilt();
			var ctor = TypeBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.HideBySig, CallingConventions.Standard, args ?? Type.EmptyTypes);
			configure(ctor.GetILGenerator());
			return this;
		}

		/// <summary>
		/// Creates an empty default parameterless constructor.
		/// </summary>
		public DynamicTypeBuilder CreateConstructor() {
			AssertNotBuilt();
			TypeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
			return this;
		}

		/// <summary>
		/// Creates an automatic property with a backing field and a default getter and setter.
		/// </summary>
		public DynamicTypeBuilder CreateAutoProperty(string name, Type type, AccessModifier getterAccess = AccessModifier.Public, AccessModifier setterAccess = AccessModifier.Public) {
			AssertNotBuilt();

			// Check it can actually be accessed
			if (getterAccess == AccessModifier.None && setterAccess == AccessModifier.None)
				throw new ArgumentException($"Cannot create a property (name: '{name}') that cannot be accessed.");

			// Define the backing field for the auto-property
			var field = TypeBuilder.DefineField("backingField__" + name, type, FieldAttributes.Private);

			// Define the actual property
			var prop = TypeBuilder.DefineProperty(name, PropertyAttributes.None, type, null);

			// Build the getter
			if (GetAccess(getterAccess, out var getterAMod)) {
				var getter = TypeBuilder.DefineMethod("get_" + name, getterAMod | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.HideBySig, type, Type.EmptyTypes);
				var il = getter.GetILGenerator();
				il.Emit(Ldarg_0);
				il.Emit(Ldfld, field);
				il.Emit(Ret);
				prop.SetGetMethod(getter);
			}

			// Build the setter
			if (GetAccess(getterAccess, out var setterAMod)) {
				var setter = TypeBuilder.DefineMethod("set_" + name, setterAMod | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.HideBySig, null, new[] { type });
				var il = setter.GetILGenerator();
				il.Emit(Ldarg_0);
				il.Emit(Ldarg_1);
				il.Emit(Stfld, field);
				il.Emit(Ret);
				prop.SetSetMethod(setter);
			}

			return this;
		}

		/// <summary>
		/// Creates a readonly property with the specified IL code.
		/// </summary>
		public DynamicTypeBuilder CreateReadOnlyProperty(string name, Type type, Action<ILGenerator> configure, AccessModifier access = AccessModifier.Public) {
			AssertNotBuilt();
			if (!GetAccess(access, out var accessMod)) throw new ArgumentException($"Cannot create a readonly property (name: '{name}') that has no getter.", nameof(access));

			var prop = TypeBuilder.DefineProperty(name, PropertyAttributes.None, type, null);
			var getter = TypeBuilder.DefineMethod("get_" + name, accessMod | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.HideBySig, type, Type.EmptyTypes);
			var il = getter.GetILGenerator();
			configure(il);
			prop.SetGetMethod(getter);

			return this;
		}

		/// <summary>
		/// Creates a readonly property that uses a backing field to lazily instantiate the value, then hold it in memory.<para/>
		/// Note this only works for types that can hold a null value.
		/// </summary>
		public DynamicTypeBuilder CreateLazyProperty(string name, Type type, Action<ILGenerator> valueFactory, AccessModifier access = AccessModifier.Public) {
			AssertNotBuilt();
			if (!GetAccess(access, out var accessMod)) throw new ArgumentException($"Cannot create a readonly property (name: '{name}') that has no getter.", nameof(access));

			// Create field, property and getter
			var field = TypeBuilder.DefineField("backingField__" + name, type, FieldAttributes.Private);
			var prop = TypeBuilder.DefineProperty(name, PropertyAttributes.None, type, null);
			var getter = TypeBuilder.DefineMethod("get_" + name, accessMod | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.HideBySig, type, Type.EmptyTypes);
			var il = getter.GetILGenerator();

			var returnFieldValue = il.DefineLabel();

			// First, check if the field on the instance is null
			il.Emit(Ldarg_0);
			il.Emit(Ldfld, field);
			il.Emit(Brtrue_S, returnFieldValue); // If not equal to false, skip over the value init

			// Initialise the field (if null)
			il.Emit(Ldarg_0);
			valueFactory(il);
			il.Emit(Stfld, field);

			// Return the field value
			il.MarkLabel(returnFieldValue);
			il.Emit(Ldarg_0);
			il.Emit(Ldfld, field);
			il.Emit(Ret);

			prop.SetGetMethod(getter);

			return this;
		}

		/// <summary>
		/// Creates a field with the given name and type on the type.
		/// </summary>
		public DynamicTypeBuilder CreateField(string name, Type type, out FieldBuilder field, AccessModifier access = AccessModifier.Private) {
			AssertNotBuilt();
			if (access == AccessModifier.None) throw new ArgumentException("Cannot create a field with no access.", nameof(access));
			field = TypeBuilder.DefineField(name, type, access == AccessModifier.Public ? FieldAttributes.Public : FieldAttributes.Private);
			return this;
		}

		/// <summary>
		/// Creates a field with the given name and type on the type.
		/// </summary>
		public DynamicTypeBuilder CreateField(string name, Type type, AccessModifier access = AccessModifier.Private) => CreateField(name, type, out _, access);


		/// <summary>
		/// Gets the <see cref="System.Reflection.TypeInfo"/> for this builder. Note that this will build the type and it will no longer be editable.
		/// </summary>
		public TypeInfo BuiltTypeInfo => builtType ?? (builtType = TypeBuilder.CreateTypeInfo());

		/// <summary>
		/// Gets the <see cref="System.Type"/> for this builder. Note that this will build the type and it will no longer be editable.
		/// </summary>
		public Type BuiltType => (builtType ?? (builtType = TypeBuilder.CreateTypeInfo())).AsType();


		/// <summary>
		/// Creates an enum with the specified value names.
		/// </summary>
		public static Type BuildEnum(string name, IEnumerable<string> values) => BuildEnum(name, values.Select((v, i) => new KeyValuePair<string, int>(v, i)));

		/// <summary>
		/// Creates an enum with the specified value names and underlying values.
		/// </summary>
		public static Type BuildEnum(string name, IEnumerable<KeyValuePair<string, int>> values) {
			var enumBuilder = moduleBuilder.DefineEnum(name, TypeAttributes.Public, typeof(int));
			foreach (var kvp in values)
				enumBuilder.DefineLiteral(kvp.Key, kvp.Value);
			return enumBuilder.CreateTypeInfo().AsType();
		}

		private void AssertNotBuilt() {
			if (builtType != null)
				throw new Exception("Cannot modify this class further because it has already been built.");
		}

		/// <summary>
		/// Returns true if <paramref name="mod"/> is not <see cref="AccessModifier.None"/>.
		/// If true, will also output a <see cref="MethodAttributes"/> indicating the desired access modifier.
		/// </summary>
		private bool GetAccess(AccessModifier mod, out MethodAttributes modifier) {
			modifier = mod == AccessModifier.Private ? MethodAttributes.Private
					 : mod == AccessModifier.Public ? MethodAttributes.Public
					 : (MethodAttributes)0;
			return mod != AccessModifier.None;
		}

		/// <summary>
		/// Determines the visibility of 
		/// </summary>
		public enum AccessModifier {
			None,
			Public,
			Private
		}
	}
}
