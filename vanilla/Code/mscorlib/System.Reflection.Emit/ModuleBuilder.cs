using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity;

namespace System.Reflection.Emit;

/// <summary>Defines and represents a module in a dynamic assembly.</summary>
[StructLayout(LayoutKind.Sequential)]
[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
[ComDefaultInterface(typeof(_ModuleBuilder))]
public class ModuleBuilder : Module, _ModuleBuilder
{
	private UIntPtr dynamic_image;

	private int num_types;

	private TypeBuilder[] types;

	private CustomAttributeBuilder[] cattrs;

	private byte[] guid;

	private int table_idx;

	internal AssemblyBuilder assemblyb;

	private MethodBuilder[] global_methods;

	private FieldBuilder[] global_fields;

	private bool is_main;

	private MonoResource[] resources;

	private IntPtr unparented_classes;

	private TypeBuilder global_type;

	private Type global_type_created;

	private Dictionary<TypeName, TypeBuilder> name_cache;

	private Dictionary<string, int> us_string_cache;

	private int[] table_indexes;

	private bool transient;

	private ModuleBuilderTokenGenerator token_gen;

	private Hashtable resource_writers;

	private ISymbolWriter symbolWriter;

	private static bool has_warned_about_symbolWriter;

	private static int typeref_tokengen = 33554431;

	private static int typedef_tokengen = 50331647;

	private static int typespec_tokengen = 469762047;

	private static int memberref_tokengen = 184549375;

	private static int methoddef_tokengen = 117440511;

	private Dictionary<MemberInfo, int> inst_tokens;

	private Dictionary<MemberInfo, int> inst_tokens_open;

	/// <summary>Gets a String representing the fully qualified name and path to this module.</summary>
	/// <returns>The fully qualified module name.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override string FullyQualifiedName => fqname;

	internal string FileName => fqname;

	internal bool IsMain
	{
		set
		{
			is_main = value;
		}
	}

	/// <summary>Gets the dynamic assembly that defined this instance of <see cref="T:System.Reflection.Emit.ModuleBuilder" />.</summary>
	/// <returns>The dynamic assembly that defined the current dynamic module.</returns>
	public override Assembly Assembly => assemblyb;

	/// <summary>A string that indicates that this is an in-memory module.</summary>
	/// <returns>Text that indicates that this is an in-memory module.</returns>
	public override string Name => name;

	/// <summary>Gets a string that represents the name of the dynamic module.</summary>
	/// <returns>The name of the dynamic module.</returns>
	public override string ScopeName => name;

	/// <summary>Gets a universally unique identifier (UUID) that can be used to distinguish between two versions of a module.</summary>
	/// <returns>A <see cref="T:System.Guid" /> that can be used to distinguish between two versions of a module.</returns>
	public override Guid ModuleVersionId => GetModuleVersionId();

	/// <summary>Gets a token that identifies the current dynamic module in metadata.</summary>
	/// <returns>An integer token that identifies the current module in metadata.</returns>
	public override int MetadataToken => Module.get_MetadataToken(this);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void basic_init(ModuleBuilder ab);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void set_wrappers_type(ModuleBuilder mb, Type ab);

	internal ModuleBuilder(AssemblyBuilder assb, string name, string fullyqname, bool emitSymbolInfo, bool transient)
	{
		base.name = (scopename = name);
		fqname = fullyqname;
		base.assembly = (assemblyb = assb);
		this.transient = transient;
		guid = Guid.FastNewGuidArray();
		table_idx = get_next_table_index(this, 0, inc: true);
		name_cache = new Dictionary<TypeName, TypeBuilder>();
		us_string_cache = new Dictionary<string, int>(512);
		basic_init(this);
		CreateGlobalType();
		if (assb.IsRun)
		{
			Type ab = new TypeBuilder(this, TypeAttributes.Abstract, 16777215).CreateType();
			set_wrappers_type(this, ab);
		}
		if (!emitSymbolInfo)
		{
			return;
		}
		Assembly assembly = Assembly.LoadWithPartialName("Mono.CompilerServices.SymbolWriter");
		Type type = null;
		if (assembly != null)
		{
			type = assembly.GetType("Mono.CompilerServices.SymbolWriter.SymbolWriterImpl");
		}
		if (type == null)
		{
			WarnAboutSymbolWriter("Failed to load the default Mono.CompilerServices.SymbolWriter assembly");
		}
		else
		{
			try
			{
				symbolWriter = (ISymbolWriter)Activator.CreateInstance(type, this);
			}
			catch (MissingMethodException)
			{
				WarnAboutSymbolWriter("The default Mono.CompilerServices.SymbolWriter is not available on this platform");
				return;
			}
		}
		string text = fqname;
		if (assemblyb.AssemblyDir != null)
		{
			text = Path.Combine(assemblyb.AssemblyDir, text);
		}
		symbolWriter.Initialize(IntPtr.Zero, text, fFullBuild: true);
	}

	private static void WarnAboutSymbolWriter(string message)
	{
		if (!has_warned_about_symbolWriter)
		{
			has_warned_about_symbolWriter = true;
			Console.Error.WriteLine("WARNING: {0}", message);
		}
	}

	/// <summary>Returns a value that indicates whether this dynamic module is transient.</summary>
	/// <returns>true if this dynamic module is transient; otherwise, false.</returns>
	public bool IsTransient()
	{
		return transient;
	}

	/// <summary>Completes the global function definitions and global data definitions for this dynamic module.</summary>
	/// <exception cref="T:System.InvalidOperationException">This method was called previously. </exception>
	public void CreateGlobalFunctions()
	{
		if (global_type_created != null)
		{
			throw new InvalidOperationException("global methods already created");
		}
		if (global_type != null)
		{
			global_type_created = global_type.CreateType();
		}
	}

	/// <summary>Defines an initialized data field in the .sdata section of the portable executable (PE) file.</summary>
	/// <returns>A field to reference the data.</returns>
	/// <param name="name">The name used to refer to the data. <paramref name="name" /> cannot contain embedded nulls. </param>
	/// <param name="data">The binary large object (BLOB) of data. </param>
	/// <param name="attributes">The attributes for the field. The default is Static. </param>
	/// <exception cref="T:System.ArgumentException">The length of <paramref name="name" /> is zero.-or- The size of <paramref name="data" /> is less than or equal to zero or greater than or equal to 0x3f0000. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> or <paramref name="data" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Reflection.Emit.ModuleBuilder.CreateGlobalFunctions" /> has been previously called. </exception>
	public FieldBuilder DefineInitializedData(string name, byte[] data, FieldAttributes attributes)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		FieldAttributes fieldAttributes = attributes & ~FieldAttributes.ReservedMask;
		FieldBuilder fieldBuilder = DefineDataImpl(name, data.Length, fieldAttributes | FieldAttributes.HasFieldRVA);
		fieldBuilder.SetRVAData(data);
		return fieldBuilder;
	}

	/// <summary>Defines an uninitialized data field in the .sdata section of the portable executable (PE) file.</summary>
	/// <returns>A field to reference the data.</returns>
	/// <param name="name">The name used to refer to the data. <paramref name="name" /> cannot contain embedded nulls. </param>
	/// <param name="size">The size of the data field. </param>
	/// <param name="attributes">The attributes for the field. </param>
	/// <exception cref="T:System.ArgumentException">The length of <paramref name="name" /> is zero.-or- <paramref name="size" /> is less than or equal to zero, or greater than or equal to 0x003f0000. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Reflection.Emit.ModuleBuilder.CreateGlobalFunctions" /> has been previously called. </exception>
	public FieldBuilder DefineUninitializedData(string name, int size, FieldAttributes attributes)
	{
		return DefineDataImpl(name, size, attributes & ~FieldAttributes.ReservedMask);
	}

	private FieldBuilder DefineDataImpl(string name, int size, FieldAttributes attributes)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (name == string.Empty)
		{
			throw new ArgumentException("name cannot be empty", "name");
		}
		if (global_type_created != null)
		{
			throw new InvalidOperationException("global fields already created");
		}
		if (size <= 0 || size >= 4128768)
		{
			throw new ArgumentException("Data size must be > 0 and < 0x3f0000", (string)null);
		}
		CreateGlobalType();
		string className = "$ArrayType$" + size;
		Type type = GetType(className, throwOnError: false, ignoreCase: false);
		if (type == null)
		{
			TypeBuilder typeBuilder = DefineType(className, TypeAttributes.Public | TypeAttributes.ExplicitLayout | TypeAttributes.Sealed, assemblyb.corlib_value_type, null, PackingSize.Size1, size);
			typeBuilder.CreateType();
			type = typeBuilder;
		}
		FieldBuilder fieldBuilder = global_type.DefineField(name, type, attributes | FieldAttributes.Static);
		if (global_fields != null)
		{
			FieldBuilder[] array = new FieldBuilder[global_fields.Length + 1];
			Array.Copy(global_fields, array, global_fields.Length);
			array[global_fields.Length] = fieldBuilder;
			global_fields = array;
		}
		else
		{
			global_fields = new FieldBuilder[1];
			global_fields[0] = fieldBuilder;
		}
		return fieldBuilder;
	}

	private void addGlobalMethod(MethodBuilder mb)
	{
		if (global_methods != null)
		{
			MethodBuilder[] array = new MethodBuilder[global_methods.Length + 1];
			Array.Copy(global_methods, array, global_methods.Length);
			array[global_methods.Length] = mb;
			global_methods = array;
		}
		else
		{
			global_methods = new MethodBuilder[1];
			global_methods[0] = mb;
		}
	}

	/// <summary>Defines a global method with the specified name, attributes, return type, and parameter types.</summary>
	/// <returns>The defined global method.</returns>
	/// <param name="name">The name of the method. <paramref name="name" /> cannot contain embedded nulls. </param>
	/// <param name="attributes">The attributes of the method. <paramref name="attributes" /> must include <see cref="F:System.Reflection.MethodAttributes.Static" />. </param>
	/// <param name="returnType">The return type of the method. </param>
	/// <param name="parameterTypes">The types of the method's parameters. </param>
	/// <exception cref="T:System.ArgumentException">The method is not static. That is, <paramref name="attributes" /> does not include <see cref="F:System.Reflection.MethodAttributes.Static" />.-or- The length of <paramref name="name" /> is zero -or-An element in the <see cref="T:System.Type" /> array is null.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Reflection.Emit.ModuleBuilder.CreateGlobalFunctions" /> has been previously called. </exception>
	public MethodBuilder DefineGlobalMethod(string name, MethodAttributes attributes, Type returnType, Type[] parameterTypes)
	{
		return DefineGlobalMethod(name, attributes, CallingConventions.Standard, returnType, parameterTypes);
	}

	/// <summary>Defines a global method with the specified name, attributes, calling convention, return type, and parameter types.</summary>
	/// <returns>The defined global method.</returns>
	/// <param name="name">The name of the method. <paramref name="name" /> cannot contain embedded nulls. </param>
	/// <param name="attributes">The attributes of the method. <paramref name="attributes" /> must include <see cref="F:System.Reflection.MethodAttributes.Static" />.</param>
	/// <param name="callingConvention">The calling convention for the method. </param>
	/// <param name="returnType">The return type of the method. </param>
	/// <param name="parameterTypes">The types of the method's parameters. </param>
	/// <exception cref="T:System.ArgumentException">The method is not static. That is, <paramref name="attributes" /> does not include <see cref="F:System.Reflection.MethodAttributes.Static" />.-or-An element in the <see cref="T:System.Type" /> array is null.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Reflection.Emit.ModuleBuilder.CreateGlobalFunctions" /> has been previously called. </exception>
	public MethodBuilder DefineGlobalMethod(string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes)
	{
		return DefineGlobalMethod(name, attributes, callingConvention, returnType, null, null, parameterTypes, null, null);
	}

	/// <summary>Defines a global method with the specified name, attributes, calling convention, return type, custom modifiers for the return type, parameter types, and custom modifiers for the parameter types.</summary>
	/// <returns>The defined global method.</returns>
	/// <param name="name">The name of the method. <paramref name="name" /> cannot contain embedded null characters. </param>
	/// <param name="attributes">The attributes of the method. <paramref name="attributes" /> must include <see cref="F:System.Reflection.MethodAttributes.Static" />.</param>
	/// <param name="callingConvention">The calling convention for the method. </param>
	/// <param name="returnType">The return type of the method. </param>
	/// <param name="requiredReturnTypeCustomModifiers">An array of types representing the required custom modifiers for the return type, such as <see cref="T:System.Runtime.CompilerServices.IsConst" /> or <see cref="T:System.Runtime.CompilerServices.IsBoxed" />. If the return type has no required custom modifiers, specify null. </param>
	/// <param name="optionalReturnTypeCustomModifiers">An array of types representing the optional custom modifiers for the return type, such as <see cref="T:System.Runtime.CompilerServices.IsConst" /> or <see cref="T:System.Runtime.CompilerServices.IsBoxed" />. If the return type has no optional custom modifiers, specify null. </param>
	/// <param name="parameterTypes">The types of the method's parameters. </param>
	/// <param name="requiredParameterTypeCustomModifiers">An array of arrays of types. Each array of types represents the required custom modifiers for the corresponding parameter of the global method. If a particular argument has no required custom modifiers, specify null instead of an array of types. If the global method has no arguments, or if none of the arguments have required custom modifiers, specify null instead of an array of arrays.</param>
	/// <param name="optionalParameterTypeCustomModifiers">An array of arrays of types. Each array of types represents the optional custom modifiers for the corresponding parameter. If a particular argument has no optional custom modifiers, specify null instead of an array of types. If the global method has no arguments, or if none of the arguments have optional custom modifiers, specify null instead of an array of arrays.</param>
	/// <exception cref="T:System.ArgumentException">The method is not static. That is, <paramref name="attributes" /> does not include <see cref="F:System.Reflection.MethodAttributes.Static" />.-or-An element in the <see cref="T:System.Type" /> array is null.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="M:System.Reflection.Emit.ModuleBuilder.CreateGlobalFunctions" /> method has been previously called. </exception>
	public MethodBuilder DefineGlobalMethod(string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] requiredReturnTypeCustomModifiers, Type[] optionalReturnTypeCustomModifiers, Type[] parameterTypes, Type[][] requiredParameterTypeCustomModifiers, Type[][] optionalParameterTypeCustomModifiers)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if ((attributes & MethodAttributes.Static) == 0)
		{
			throw new ArgumentException("global methods must be static");
		}
		if (global_type_created != null)
		{
			throw new InvalidOperationException("global methods already created");
		}
		CreateGlobalType();
		MethodBuilder methodBuilder = global_type.DefineMethod(name, attributes, callingConvention, returnType, requiredReturnTypeCustomModifiers, optionalReturnTypeCustomModifiers, parameterTypes, requiredParameterTypeCustomModifiers, optionalParameterTypeCustomModifiers);
		addGlobalMethod(methodBuilder);
		return methodBuilder;
	}

	/// <summary>Defines a PInvoke method with the specified name, the name of the DLL in which the method is defined, the attributes of the method, the calling convention of the method, the return type of the method, the types of the parameters of the method, and the PInvoke flags.</summary>
	/// <returns>The defined PInvoke method.</returns>
	/// <param name="name">The name of the PInvoke method. <paramref name="name" /> cannot contain embedded nulls. </param>
	/// <param name="dllName">The name of the DLL in which the PInvoke method is defined. </param>
	/// <param name="attributes">The attributes of the method. </param>
	/// <param name="callingConvention">The method's calling convention. </param>
	/// <param name="returnType">The method's return type. </param>
	/// <param name="parameterTypes">The types of the method's parameters. </param>
	/// <param name="nativeCallConv">The native calling convention. </param>
	/// <param name="nativeCharSet">The method's native character set. </param>
	/// <exception cref="T:System.ArgumentException">The method is not static or if the containing type is an interface.-or- The method is abstract.-or- The method was previously defined. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> or <paramref name="dllName" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The containing type has been previously created using <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" /></exception>
	public MethodBuilder DefinePInvokeMethod(string name, string dllName, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, CallingConvention nativeCallConv, CharSet nativeCharSet)
	{
		return DefinePInvokeMethod(name, dllName, name, attributes, callingConvention, returnType, parameterTypes, nativeCallConv, nativeCharSet);
	}

	/// <summary>Defines a PInvoke method with the specified name, the name of the DLL in which the method is defined, the attributes of the method, the calling convention of the method, the return type of the method, the types of the parameters of the method, and the PInvoke flags.</summary>
	/// <returns>The defined PInvoke method.</returns>
	/// <param name="name">The name of the PInvoke method. <paramref name="name" /> cannot contain embedded nulls. </param>
	/// <param name="dllName">The name of the DLL in which the PInvoke method is defined. </param>
	/// <param name="entryName">The name of the entry point in the DLL. </param>
	/// <param name="attributes">The attributes of the method. </param>
	/// <param name="callingConvention">The method's calling convention. </param>
	/// <param name="returnType">The method's return type. </param>
	/// <param name="parameterTypes">The types of the method's parameters. </param>
	/// <param name="nativeCallConv">The native calling convention. </param>
	/// <param name="nativeCharSet">The method's native character set. </param>
	/// <exception cref="T:System.ArgumentException">The method is not static or if the containing type is an interface or if the method is abstract of if the method was previously defined. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> or <paramref name="dllName" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The containing type has been previously created using <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" /></exception>
	public MethodBuilder DefinePInvokeMethod(string name, string dllName, string entryName, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, CallingConvention nativeCallConv, CharSet nativeCharSet)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if ((attributes & MethodAttributes.Static) == 0)
		{
			throw new ArgumentException("global methods must be static");
		}
		if (global_type_created != null)
		{
			throw new InvalidOperationException("global methods already created");
		}
		CreateGlobalType();
		MethodBuilder methodBuilder = global_type.DefinePInvokeMethod(name, dllName, entryName, attributes, callingConvention, returnType, parameterTypes, nativeCallConv, nativeCharSet);
		addGlobalMethod(methodBuilder);
		return methodBuilder;
	}

	/// <summary>Constructs a TypeBuilder for a private type with the specified name in this module. </summary>
	/// <returns>A private type with the specified name.</returns>
	/// <param name="name">The full path of the type, including the namespace. <paramref name="name" /> cannot contain embedded nulls. </param>
	/// <exception cref="T:System.ArgumentException">A type with the given name exists in the parent assembly of this module.-or- Nested type attributes are set on a type that is not nested. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	public TypeBuilder DefineType(string name)
	{
		return DefineType(name, TypeAttributes.NotPublic);
	}

	/// <summary>Constructs a TypeBuilder given the type name and the type attributes.</summary>
	/// <returns>A TypeBuilder created with all of the requested attributes.</returns>
	/// <param name="name">The full path of the type. <paramref name="name" /> cannot contain embedded nulls. </param>
	/// <param name="attr">The attributes of the defined type. </param>
	/// <exception cref="T:System.ArgumentException">A type with the given name exists in the parent assembly of this module.-or- Nested type attributes are set on a type that is not nested. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	public TypeBuilder DefineType(string name, TypeAttributes attr)
	{
		if ((attr & TypeAttributes.ClassSemanticsMask) != 0)
		{
			return DefineType(name, attr, null, null);
		}
		return DefineType(name, attr, typeof(object), null);
	}

	/// <summary>Constructs a TypeBuilder given type name, its attributes, and the type that the defined type extends.</summary>
	/// <returns>A TypeBuilder created with all of the requested attributes.</returns>
	/// <param name="name">The full path of the type. <paramref name="name" /> cannot contain embedded nulls. </param>
	/// <param name="attr">The attribute to be associated with the type. </param>
	/// <param name="parent">The type that the defined type extends. </param>
	/// <exception cref="T:System.ArgumentException">A type with the given name exists in the parent assembly of this module.-or- Nested type attributes are set on a type that is not nested. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	public TypeBuilder DefineType(string name, TypeAttributes attr, Type parent)
	{
		return DefineType(name, attr, parent, null);
	}

	private void AddType(TypeBuilder tb)
	{
		if (types != null)
		{
			if (types.Length == num_types)
			{
				TypeBuilder[] destinationArray = new TypeBuilder[types.Length * 2];
				Array.Copy(types, destinationArray, num_types);
				types = destinationArray;
			}
		}
		else
		{
			types = new TypeBuilder[1];
		}
		types[num_types] = tb;
		num_types++;
	}

	private TypeBuilder DefineType(string name, TypeAttributes attr, Type parent, Type[] interfaces, PackingSize packingSize, int typesize)
	{
		if (name == null)
		{
			throw new ArgumentNullException("fullname");
		}
		TypeIdentifier key = TypeIdentifiers.FromInternal(name);
		if (name_cache.ContainsKey(key))
		{
			throw new ArgumentException("Duplicate type name within an assembly.");
		}
		TypeBuilder typeBuilder = new TypeBuilder(this, name, attr, parent, interfaces, packingSize, typesize, null);
		AddType(typeBuilder);
		name_cache.Add(key, typeBuilder);
		return typeBuilder;
	}

	internal void RegisterTypeName(TypeBuilder tb, TypeName name)
	{
		name_cache.Add(name, tb);
	}

	internal TypeBuilder GetRegisteredType(TypeName name)
	{
		TypeBuilder value = null;
		name_cache.TryGetValue(name, out value);
		return value;
	}

	/// <summary>Constructs a TypeBuilder given the type name, attributes, the type that the defined type extends, and the interfaces that the defined type implements.</summary>
	/// <returns>A TypeBuilder created with all of the requested attributes.</returns>
	/// <param name="name">The full path of the type. <paramref name="name" /> cannot contain embedded nulls. </param>
	/// <param name="attr">The attributes to be associated with the type. </param>
	/// <param name="parent">The type that the defined type extends. </param>
	/// <param name="interfaces">The list of interfaces that the type implements. </param>
	/// <exception cref="T:System.ArgumentException">A type with the given name exists in the parent assembly of this module.-or- Nested type attributes are set on a type that is not nested. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	[ComVisible(true)]
	public TypeBuilder DefineType(string name, TypeAttributes attr, Type parent, Type[] interfaces)
	{
		return DefineType(name, attr, parent, interfaces, PackingSize.Unspecified, 0);
	}

	/// <summary>Constructs a TypeBuilder given the type name, the attributes, the type that the defined type extends, and the total size of the type.</summary>
	/// <returns>A TypeBuilder object.</returns>
	/// <param name="name">The full path of the type. <paramref name="name" /> cannot contain embedded nulls. </param>
	/// <param name="attr">The attributes of the defined type. </param>
	/// <param name="parent">The type that the defined type extends. </param>
	/// <param name="typesize">The total size of the type. </param>
	/// <exception cref="T:System.ArgumentException">A type with the given name exists in the parent assembly of this module.-or- Nested type attributes are set on a type that is not nested. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	public TypeBuilder DefineType(string name, TypeAttributes attr, Type parent, int typesize)
	{
		return DefineType(name, attr, parent, null, PackingSize.Unspecified, typesize);
	}

	/// <summary>Constructs a TypeBuilder given the type name, the attributes, the type that the defined type extends, and the packing size of the type.</summary>
	/// <returns>A TypeBuilder object.</returns>
	/// <param name="name">The full path of the type. <paramref name="name" /> cannot contain embedded nulls. </param>
	/// <param name="attr">The attributes of the defined type. </param>
	/// <param name="parent">The type that the defined type extends. </param>
	/// <param name="packsize">The packing size of the type. </param>
	/// <exception cref="T:System.ArgumentException">A type with the given name exists in the parent assembly of this module.-or- Nested type attributes are set on a type that is not nested. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	public TypeBuilder DefineType(string name, TypeAttributes attr, Type parent, PackingSize packsize)
	{
		return DefineType(name, attr, parent, null, packsize, 0);
	}

	/// <summary>Constructs a TypeBuilder given the type name, attributes, the type that the defined type extends, the packing size of the defined type, and the total size of the defined type.</summary>
	/// <returns>A TypeBuilder created with all of the requested attributes.</returns>
	/// <param name="name">The full path of the type. <paramref name="name" /> cannot contain embedded nulls. </param>
	/// <param name="attr">The attributes of the defined type. </param>
	/// <param name="parent">The type that the defined type extends. </param>
	/// <param name="packingSize">The packing size of the type. </param>
	/// <param name="typesize">The total size of the type. </param>
	/// <exception cref="T:System.ArgumentException">A type with the given name exists in the parent assembly of this module.-or- Nested type attributes are set on a type that is not nested. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	public TypeBuilder DefineType(string name, TypeAttributes attr, Type parent, PackingSize packingSize, int typesize)
	{
		return DefineType(name, attr, parent, null, packingSize, typesize);
	}

	/// <summary>Returns the named method on an array class.</summary>
	/// <returns>The named method on an array class.</returns>
	/// <param name="arrayClass">An array class. </param>
	/// <param name="methodName">The name of a method on the array class. </param>
	/// <param name="callingConvention">The method's calling convention. </param>
	/// <param name="returnType">The return type of the method. </param>
	/// <param name="parameterTypes">The types of the method's parameters. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="arrayClass" /> is not an array. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="arrayClass" /> or <paramref name="methodName" /> is null. </exception>
	public MethodInfo GetArrayMethod(Type arrayClass, string methodName, CallingConventions callingConvention, Type returnType, Type[] parameterTypes)
	{
		return new MonoArrayMethod(arrayClass, methodName, callingConvention, returnType, parameterTypes);
	}

	/// <summary>Defines an enumeration type that is a value type with a single non-static field called <paramref name="value__" /> of the specified type.</summary>
	/// <returns>The defined enumeration.</returns>
	/// <param name="name">The full path of the enumeration type. <paramref name="name" /> cannot contain embedded nulls. </param>
	/// <param name="visibility">The type attributes for the enumeration. The attributes are any bits defined by <see cref="F:System.Reflection.TypeAttributes.VisibilityMask" />. </param>
	/// <param name="underlyingType">The underlying type for the enumeration. This must be a built-in integer type. </param>
	/// <exception cref="T:System.ArgumentException">Attributes other than visibility attributes are provided.-or- An enumeration with the given name exists in the parent assembly of this module.-or- The visibility attributes do not match the scope of the enumeration. For example, <see cref="F:System.Reflection.TypeAttributes.NestedPublic" /> is specified for <paramref name="visibility" />, but the enumeration is not a nested type. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	public EnumBuilder DefineEnum(string name, TypeAttributes visibility, Type underlyingType)
	{
		TypeIdentifier key = TypeIdentifiers.FromInternal(name);
		if (name_cache.ContainsKey(key))
		{
			throw new ArgumentException("Duplicate type name within an assembly.");
		}
		EnumBuilder enumBuilder = new EnumBuilder(this, name, visibility, underlyingType);
		TypeBuilder typeBuilder = enumBuilder.GetTypeBuilder();
		AddType(typeBuilder);
		name_cache.Add(key, typeBuilder);
		return enumBuilder;
	}

	/// <summary>Gets the named type defined in the module.</summary>
	/// <returns>The requested type, if the type is defined in this module; otherwise, null.</returns>
	/// <param name="className">The name of the <see cref="T:System.Type" /> to get. </param>
	/// <exception cref="T:System.ArgumentException">Length of <paramref name="className" /> is zero or is greater than 1023. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="className" /> is null. </exception>
	/// <exception cref="T:System.Security.SecurityException">The requested <see cref="T:System.Type" /> is non-public and the caller does not have <see cref="T:System.Security.Permissions.ReflectionPermission" /> to reflect non-public objects outside the current assembly. </exception>
	/// <exception cref="T:System.Reflection.TargetInvocationException">A class initializer is invoked and throws an exception. </exception>
	/// <exception cref="T:System.TypeLoadException">An error is encountered while loading the <see cref="T:System.Type" />. </exception>
	[ComVisible(true)]
	public override Type GetType(string className)
	{
		return GetType(className, throwOnError: false, ignoreCase: false);
	}

	/// <summary>Gets the named type defined in the module, optionally ignoring the case of the type name.</summary>
	/// <returns>The requested type, if the type is defined in this module; otherwise, null.</returns>
	/// <param name="className">The name of the <see cref="T:System.Type" /> to get. </param>
	/// <param name="ignoreCase">If true, the search is case-insensitive. If false, the search is case-sensitive. </param>
	/// <exception cref="T:System.ArgumentException">Length of <paramref name="className" /> is zero or is greater than 1023. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="className" /> is null. </exception>
	/// <exception cref="T:System.Security.SecurityException">The requested <see cref="T:System.Type" /> is non-public and the caller does not have <see cref="T:System.Security.Permissions.ReflectionPermission" /> to reflect non-public objects outside the current assembly. </exception>
	/// <exception cref="T:System.Reflection.TargetInvocationException">A class initializer is invoked and throws an exception. </exception>
	[ComVisible(true)]
	public override Type GetType(string className, bool ignoreCase)
	{
		return GetType(className, throwOnError: false, ignoreCase);
	}

	private TypeBuilder search_in_array(TypeBuilder[] arr, int validElementsInArray, TypeName className)
	{
		for (int i = 0; i < validElementsInArray; i++)
		{
			if (string.Compare(className.DisplayName, arr[i].FullName, ignoreCase: true, CultureInfo.InvariantCulture) == 0)
			{
				return arr[i];
			}
		}
		return null;
	}

	private TypeBuilder search_nested_in_array(TypeBuilder[] arr, int validElementsInArray, TypeName className)
	{
		for (int i = 0; i < validElementsInArray; i++)
		{
			if (string.Compare(className.DisplayName, arr[i].Name, ignoreCase: true, CultureInfo.InvariantCulture) == 0)
			{
				return arr[i];
			}
		}
		return null;
	}

	private TypeBuilder GetMaybeNested(TypeBuilder t, IEnumerable<TypeName> nested)
	{
		TypeBuilder typeBuilder = t;
		foreach (TypeName item in nested)
		{
			if (typeBuilder.subtypes == null)
			{
				return null;
			}
			typeBuilder = search_nested_in_array(typeBuilder.subtypes, typeBuilder.subtypes.Length, item);
			if (typeBuilder == null)
			{
				return null;
			}
		}
		return typeBuilder;
	}

	/// <summary>Gets the named type defined in the module, optionally ignoring the case of the type name. Optionally throws an exception if the type is not found.</summary>
	/// <returns>The specified type, if the type is declared in this module; otherwise, null.</returns>
	/// <param name="className">The name of the <see cref="T:System.Type" /> to get. </param>
	/// <param name="throwOnError">true to throw an exception if the type cannot be found; false to return null. </param>
	/// <param name="ignoreCase">If true, the search is case-insensitive. If false, the search is case-sensitive. </param>
	/// <exception cref="T:System.ArgumentException">Length of <paramref name="className" /> is zero or is greater than 1023. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="className" /> is null. </exception>
	/// <exception cref="T:System.Security.SecurityException">The requested <see cref="T:System.Type" /> is non-public and the caller does not have <see cref="T:System.Security.Permissions.ReflectionPermission" /> to reflect non-public objects outside the current assembly. </exception>
	/// <exception cref="T:System.Reflection.TargetInvocationException">A class initializer is invoked and throws an exception. </exception>
	/// <exception cref="T:System.TypeLoadException">
	///   <paramref name="throwOnError" /> is true and the specified type is not found. </exception>
	[ComVisible(true)]
	public override Type GetType(string className, bool throwOnError, bool ignoreCase)
	{
		if (className == null)
		{
			throw new ArgumentNullException("className");
		}
		if (className.Length == 0)
		{
			throw new ArgumentException("className");
		}
		TypeBuilder value = null;
		if (types == null && throwOnError)
		{
			throw new TypeLoadException(className);
		}
		TypeSpec typeSpec = TypeSpec.Parse(className);
		if (!ignoreCase)
		{
			TypeName key = typeSpec.TypeNameWithoutModifiers();
			name_cache.TryGetValue(key, out value);
		}
		else
		{
			if (types != null)
			{
				value = search_in_array(types, num_types, typeSpec.Name);
			}
			if (!typeSpec.IsNested && value != null)
			{
				value = GetMaybeNested(value, typeSpec.Nested);
			}
		}
		if (value == null && throwOnError)
		{
			throw new TypeLoadException(className);
		}
		if (value != null && (typeSpec.HasModifiers || typeSpec.IsByRef))
		{
			Type type = value;
			if ((object)value != null)
			{
				TypeBuilder typeBuilder = value;
				if (typeBuilder.is_created)
				{
					type = typeBuilder.CreateType();
				}
			}
			foreach (ModifierSpec modifier in typeSpec.Modifiers)
			{
				if (modifier is PointerSpec)
				{
					type = type.MakePointerType();
				}
				else if (modifier is ArraySpec)
				{
					ArraySpec arraySpec = modifier as ArraySpec;
					if (arraySpec.IsBound)
					{
						return null;
					}
					type = ((arraySpec.Rank != 1) ? type.MakeArrayType(arraySpec.Rank) : type.MakeArrayType());
				}
			}
			if (typeSpec.IsByRef)
			{
				type = type.MakeByRefType();
			}
			value = type as TypeBuilder;
			if (value == null)
			{
				return type;
			}
		}
		if (value != null && value.is_created)
		{
			return value.CreateType();
		}
		return value;
	}

	internal int get_next_table_index(object obj, int table, bool inc)
	{
		if (table_indexes == null)
		{
			table_indexes = new int[64];
			for (int i = 0; i < 64; i++)
			{
				table_indexes[i] = 1;
			}
			table_indexes[2] = 2;
		}
		if (inc)
		{
			return table_indexes[table]++;
		}
		return table_indexes[table];
	}

	/// <summary>Applies a custom attribute to this module by using a custom attribute builder.</summary>
	/// <param name="customBuilder">An instance of a helper class that specifies the custom attribute to apply. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="customBuilder" /> is null. </exception>
	public void SetCustomAttribute(CustomAttributeBuilder customBuilder)
	{
		if (cattrs != null)
		{
			CustomAttributeBuilder[] array = new CustomAttributeBuilder[cattrs.Length + 1];
			cattrs.CopyTo(array, 0);
			array[cattrs.Length] = customBuilder;
			cattrs = array;
		}
		else
		{
			cattrs = new CustomAttributeBuilder[1];
			cattrs[0] = customBuilder;
		}
	}

	/// <summary>Applies a custom attribute to this module by using a specified binary large object (BLOB) that represents the attribute.</summary>
	/// <param name="con">The constructor for the custom attribute. </param>
	/// <param name="binaryAttribute">A byte BLOB representing the attribute. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="con" /> or <paramref name="binaryAttribute" /> is null. </exception>
	[ComVisible(true)]
	public void SetCustomAttribute(ConstructorInfo con, byte[] binaryAttribute)
	{
		SetCustomAttribute(new CustomAttributeBuilder(con, binaryAttribute));
	}

	/// <summary>Returns the symbol writer associated with this dynamic module.</summary>
	/// <returns>The symbol writer associated with this dynamic module.</returns>
	public ISymbolWriter GetSymWriter()
	{
		return symbolWriter;
	}

	/// <summary>Defines a document for source.</summary>
	/// <returns>The defined document.</returns>
	/// <param name="url">The URL for the document. </param>
	/// <param name="language">The GUID that identifies the document language. This can be <see cref="F:System.Guid.Empty" />. </param>
	/// <param name="languageVendor">The GUID that identifies the document language vendor. This can be <see cref="F:System.Guid.Empty" />. </param>
	/// <param name="documentType">The GUID that identifies the document type. This can be <see cref="F:System.Guid.Empty" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="url" /> is null. This is a change from earlier versions of the .NET Framework.</exception>
	/// <exception cref="T:System.InvalidOperationException">This method is called on a dynamic module that is not a debug module. </exception>
	public ISymbolDocumentWriter DefineDocument(string url, Guid language, Guid languageVendor, Guid documentType)
	{
		if (symbolWriter != null)
		{
			return symbolWriter.DefineDocument(url, language, languageVendor, documentType);
		}
		return null;
	}

	/// <summary>Returns all the classes defined within this module.</summary>
	/// <returns>An array that contains the types defined within the module that is reflected by this instance.</returns>
	/// <exception cref="T:System.Reflection.ReflectionTypeLoadException">One or more classes in a module could not be loaded. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	public override Type[] GetTypes()
	{
		if (types == null)
		{
			return Type.EmptyTypes;
		}
		int num = num_types;
		Type[] array = new Type[num];
		Array.Copy(types, array, num);
		for (int i = 0; i < array.Length; i++)
		{
			if (types[i].is_created)
			{
				array[i] = types[i].CreateType();
			}
		}
		return array;
	}

	/// <summary>Defines the named managed embedded resource with the given attributes that is to be stored in this module.</summary>
	/// <returns>A resource writer for the defined resource.</returns>
	/// <param name="name">The name of the resource. <paramref name="name" /> cannot contain embedded nulls. </param>
	/// <param name="description">The description of the resource. </param>
	/// <param name="attribute">The resource attributes. </param>
	/// <exception cref="T:System.ArgumentException">Length of <paramref name="name" /> is zero. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">This module is transient.-or- The containing assembly is not persistable. </exception>
	public IResourceWriter DefineResource(string name, string description, ResourceAttributes attribute)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (name == string.Empty)
		{
			throw new ArgumentException("name cannot be empty");
		}
		if (transient)
		{
			throw new InvalidOperationException("The module is transient");
		}
		if (!assemblyb.IsSave)
		{
			throw new InvalidOperationException("The assembly is transient");
		}
		ResourceWriter resourceWriter = new ResourceWriter(new MemoryStream());
		if (resource_writers == null)
		{
			resource_writers = new Hashtable();
		}
		resource_writers[name] = resourceWriter;
		if (resources != null)
		{
			MonoResource[] destinationArray = new MonoResource[resources.Length + 1];
			Array.Copy(resources, destinationArray, resources.Length);
			resources = destinationArray;
		}
		else
		{
			resources = new MonoResource[1];
		}
		int num = resources.Length - 1;
		resources[num].name = name;
		resources[num].attrs = attribute;
		return resourceWriter;
	}

	/// <summary>Defines the named managed embedded resource to be stored in this module.</summary>
	/// <returns>A resource writer for the defined resource.</returns>
	/// <param name="name">The name of the resource. <paramref name="name" /> cannot contain embedded nulls. </param>
	/// <param name="description">The description of the resource. </param>
	/// <exception cref="T:System.ArgumentException">Length of <paramref name="name" /> is zero. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">This module is transient.-or- The containing assembly is not persistable. </exception>
	public IResourceWriter DefineResource(string name, string description)
	{
		return DefineResource(name, description, ResourceAttributes.Public);
	}

	/// <summary>Defines an unmanaged embedded resource given an opaque binary large object (BLOB) of bytes.</summary>
	/// <param name="resource">An opaque BLOB that represents an unmanaged resource </param>
	/// <exception cref="T:System.ArgumentException">An unmanaged resource has already been defined in the module's assembly. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="resource" /> is null. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[MonoTODO]
	public void DefineUnmanagedResource(byte[] resource)
	{
		if (resource == null)
		{
			throw new ArgumentNullException("resource");
		}
		throw new NotImplementedException();
	}

	/// <summary>Defines an unmanaged resource given the name of Win32 resource file.</summary>
	/// <param name="resourceFileName">The name of the unmanaged resource file. </param>
	/// <exception cref="T:System.ArgumentException">An unmanaged resource has already been defined in the module's assembly.-or- <paramref name="resourceFileName" /> is the empty string (""). </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="resourceFileName" /> is null. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="resourceFileName" /> is not found. -or- <paramref name="resourceFileName" /> is a directory. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[MonoTODO]
	public void DefineUnmanagedResource(string resourceFileName)
	{
		if (resourceFileName == null)
		{
			throw new ArgumentNullException("resourceFileName");
		}
		if (resourceFileName == string.Empty)
		{
			throw new ArgumentException("resourceFileName");
		}
		if (!File.Exists(resourceFileName) || Directory.Exists(resourceFileName))
		{
			throw new FileNotFoundException("File '" + resourceFileName + "' does not exist or is a directory.");
		}
		throw new NotImplementedException();
	}

	/// <summary>Defines a binary large object (BLOB) that represents a manifest resource to be embedded in the dynamic assembly.</summary>
	/// <param name="name">The case-sensitive name for the resource.</param>
	/// <param name="stream">A stream that contains the bytes for the resource.</param>
	/// <param name="attribute">An enumeration value that specifies whether the resource is public or private.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null.-or-<paramref name="stream" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name" /> is a zero-length string.</exception>
	/// <exception cref="T:System.InvalidOperationException">The dynamic assembly that contains the current module is transient; that is, no file name was specified when <see cref="M:System.Reflection.Emit.AssemblyBuilder.DefineDynamicModule(System.String,System.String)" /> was called.</exception>
	public void DefineManifestResource(string name, Stream stream, ResourceAttributes attribute)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (name == string.Empty)
		{
			throw new ArgumentException("name cannot be empty");
		}
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (transient)
		{
			throw new InvalidOperationException("The module is transient");
		}
		if (!assemblyb.IsSave)
		{
			throw new InvalidOperationException("The assembly is transient");
		}
		if (resources != null)
		{
			MonoResource[] destinationArray = new MonoResource[resources.Length + 1];
			Array.Copy(resources, destinationArray, resources.Length);
			resources = destinationArray;
		}
		else
		{
			resources = new MonoResource[1];
		}
		int num = resources.Length - 1;
		resources[num].name = name;
		resources[num].attrs = attribute;
		resources[num].stream = stream;
	}

	/// <summary>This method does nothing.</summary>
	/// <param name="name">The name of the custom attribute </param>
	/// <param name="data">An opaque binary large object (BLOB) of bytes that represents the value of the custom attribute. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="url" /> is null. </exception>
	[MonoTODO]
	public void SetSymCustomAttribute(string name, byte[] data)
	{
		throw new NotImplementedException();
	}

	/// <summary>Sets the user entry point.</summary>
	/// <param name="entryPoint">The user entry point. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="entryPoint" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">This method is called on a dynamic module that is not a debug module.-or- <paramref name="entryPoint" /> is not contained in this dynamic module. </exception>
	[MonoTODO]
	public void SetUserEntryPoint(MethodInfo entryPoint)
	{
		if (entryPoint == null)
		{
			throw new ArgumentNullException("entryPoint");
		}
		if (entryPoint.DeclaringType.Module != this)
		{
			throw new InvalidOperationException("entryPoint is not contained in this module");
		}
		throw new NotImplementedException();
	}

	/// <summary>Returns the token used to identify the specified method within this module.</summary>
	/// <returns>The token used to identify the specified method within this module.</returns>
	/// <param name="method">The method to get a token for. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="method" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The declaring type for the method is not in this module. </exception>
	public MethodToken GetMethodToken(MethodInfo method)
	{
		if (method == null)
		{
			throw new ArgumentNullException("method");
		}
		return new MethodToken(GetToken(method));
	}

	/// <summary>Returns the token used to identify the method that has the specified attributes and parameter types within this module.</summary>
	/// <returns>The token used to identify the specified method within this module.</returns>
	/// <param name="method">The method to get a token for.</param>
	/// <param name="optionalParameterTypes">A collection of the types of the optional parameters to the method.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="method" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The declaring type for the method is not in this module. </exception>
	public MethodToken GetMethodToken(MethodInfo method, IEnumerable<Type> optionalParameterTypes)
	{
		if (method == null)
		{
			throw new ArgumentNullException("method");
		}
		return new MethodToken(GetToken(method, optionalParameterTypes));
	}

	/// <summary>Returns the token for the named method on an array class.</summary>
	/// <returns>The token for the named method on an array class.</returns>
	/// <param name="arrayClass">The object for the array. </param>
	/// <param name="methodName">A string that contains the name of the method. </param>
	/// <param name="callingConvention">The calling convention for the method. </param>
	/// <param name="returnType">The return type of the method. </param>
	/// <param name="parameterTypes">The types of the parameters of the method. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="arrayClass" /> is not an array.-or- The length of <paramref name="methodName" /> is zero. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="arrayClass" /> or <paramref name="methodName" /> is null. </exception>
	public MethodToken GetArrayMethodToken(Type arrayClass, string methodName, CallingConventions callingConvention, Type returnType, Type[] parameterTypes)
	{
		return GetMethodToken(GetArrayMethod(arrayClass, methodName, callingConvention, returnType, parameterTypes));
	}

	/// <summary>Returns the token used to identify the specified constructor within this module.</summary>
	/// <returns>The token used to identify the specified constructor within this module.</returns>
	/// <param name="con">The constructor to get a token for. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="con" /> is null. </exception>
	[ComVisible(true)]
	public MethodToken GetConstructorToken(ConstructorInfo con)
	{
		if (con == null)
		{
			throw new ArgumentNullException("con");
		}
		return new MethodToken(GetToken(con));
	}

	/// <summary>Returns the token used to identify the constructor that has the specified attributes and parameter types within this module.</summary>
	/// <returns>The token used to identify the specified constructor within this module.</returns>
	/// <param name="constructor">The constructor to get a token for.</param>
	/// <param name="optionalParameterTypes">A collection of the types of the optional parameters to the constructor.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="constructor" /> is null. </exception>
	public MethodToken GetConstructorToken(ConstructorInfo constructor, IEnumerable<Type> optionalParameterTypes)
	{
		if (constructor == null)
		{
			throw new ArgumentNullException("constructor");
		}
		return new MethodToken(GetToken(constructor, optionalParameterTypes));
	}

	/// <summary>Returns the token used to identify the specified field within this module.</summary>
	/// <returns>The token used to identify the specified field within this module.</returns>
	/// <param name="field">The field to get a token for. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="field" /> is null. </exception>
	public FieldToken GetFieldToken(FieldInfo field)
	{
		if (field == null)
		{
			throw new ArgumentNullException("field");
		}
		return new FieldToken(GetToken(field));
	}

	/// <summary>Defines a token for the signature that has the specified character array and signature length.</summary>
	/// <returns>A token for the specified signature.</returns>
	/// <param name="sigBytes">The signature binary large object (BLOB). </param>
	/// <param name="sigLength">The length of the signature BLOB. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="sigBytes" /> is null. </exception>
	[MonoTODO]
	public SignatureToken GetSignatureToken(byte[] sigBytes, int sigLength)
	{
		throw new NotImplementedException();
	}

	/// <summary>Defines a token for the signature that is defined by the specified <see cref="T:System.Reflection.Emit.SignatureHelper" />.</summary>
	/// <returns>A token for the defined signature.</returns>
	/// <param name="sigHelper">The signature. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="sigHelper" /> is null. </exception>
	public SignatureToken GetSignatureToken(SignatureHelper sigHelper)
	{
		if (sigHelper == null)
		{
			throw new ArgumentNullException("sigHelper");
		}
		return new SignatureToken(GetToken(sigHelper));
	}

	/// <summary>Returns the token of the given string in the module’s constant pool.</summary>
	/// <returns>The token of the string in the constant pool.</returns>
	/// <param name="str">The string to add to the module's constant pool. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="str" /> is null. </exception>
	public StringToken GetStringConstant(string str)
	{
		if (str == null)
		{
			throw new ArgumentNullException("str");
		}
		return new StringToken(GetToken(str));
	}

	/// <summary>Returns the token used to identify the specified type within this module.</summary>
	/// <returns>The token used to identify the given type within this module.</returns>
	/// <param name="type">The type object that represents the class type. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="type" /> is a ByRef type. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="type" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">This is a non-transient module that references a transient module. </exception>
	public TypeToken GetTypeToken(Type type)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (type.IsByRef)
		{
			throw new ArgumentException("type can't be a byref type", "type");
		}
		if (!IsTransient() && type.Module is ModuleBuilder && ((ModuleBuilder)type.Module).IsTransient())
		{
			throw new InvalidOperationException("a non-transient module can't reference a transient module");
		}
		return new TypeToken(GetToken(type));
	}

	/// <summary>Returns the token used to identify the type with the specified name.</summary>
	/// <returns>The token used to identify the type with the specified name within this module.</returns>
	/// <param name="name">The name of the class, including the namespace. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name" /> is the empty string ("").-or-<paramref name="name" /> represents a ByRef type. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. -or-The type specified by <paramref name="name" /> could not be found.</exception>
	/// <exception cref="T:System.InvalidOperationException">This is a non-transient module that references a transient module. </exception>
	public TypeToken GetTypeToken(string name)
	{
		return GetTypeToken(GetType(name));
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern int getUSIndex(ModuleBuilder mb, string str);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern int getToken(ModuleBuilder mb, object obj, bool create_open_instance);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern int getMethodToken(ModuleBuilder mb, MethodBase method, Type[] opt_param_types);

	internal int GetToken(string str)
	{
		if (!us_string_cache.TryGetValue(str, out var value))
		{
			value = getUSIndex(this, str);
			us_string_cache[str] = value;
		}
		return value;
	}

	private int GetPseudoToken(MemberInfo member, bool create_open_instance)
	{
		Dictionary<MemberInfo, int> dictionary = (create_open_instance ? inst_tokens_open : inst_tokens);
		int value;
		if (dictionary == null)
		{
			dictionary = new Dictionary<MemberInfo, int>(ReferenceEqualityComparer<MemberInfo>.Instance);
			if (create_open_instance)
			{
				inst_tokens_open = dictionary;
			}
			else
			{
				inst_tokens = dictionary;
			}
		}
		else if (dictionary.TryGetValue(member, out value))
		{
			return value;
		}
		if (member is TypeBuilderInstantiation || member is SymbolType)
		{
			value = typespec_tokengen--;
		}
		else if (member is FieldOnTypeBuilderInst)
		{
			value = memberref_tokengen--;
		}
		else if (member is ConstructorOnTypeBuilderInst)
		{
			value = memberref_tokengen--;
		}
		else if (member is MethodOnTypeBuilderInst)
		{
			value = memberref_tokengen--;
		}
		else if (member is FieldBuilder)
		{
			value = memberref_tokengen--;
		}
		else if (member is TypeBuilder)
		{
			value = ((create_open_instance && (member as TypeBuilder).ContainsGenericParameters) ? typespec_tokengen-- : ((!(member.Module == this)) ? typeref_tokengen-- : typedef_tokengen--));
		}
		else
		{
			if (member is EnumBuilder)
			{
				return dictionary[member] = GetPseudoToken((member as EnumBuilder).GetTypeBuilder(), create_open_instance);
			}
			if (member is ConstructorBuilder)
			{
				value = ((!(member.Module == this) || (member as ConstructorBuilder).TypeBuilder.ContainsGenericParameters) ? memberref_tokengen-- : methoddef_tokengen--);
			}
			else if (member is MethodBuilder)
			{
				MethodBuilder methodBuilder = member as MethodBuilder;
				value = ((!(member.Module == this) || methodBuilder.TypeBuilder.ContainsGenericParameters || methodBuilder.IsGenericMethodDefinition) ? memberref_tokengen-- : methoddef_tokengen--);
			}
			else
			{
				if (!(member is GenericTypeParameterBuilder))
				{
					throw new NotImplementedException();
				}
				value = typespec_tokengen--;
			}
		}
		dictionary[member] = value;
		RegisterToken(member, value);
		return value;
	}

	internal int GetToken(MemberInfo member)
	{
		if (member is ConstructorBuilder || member is MethodBuilder)
		{
			return GetPseudoToken(member, create_open_instance: false);
		}
		return getToken(this, member, create_open_instance: true);
	}

	internal int GetToken(MemberInfo member, bool create_open_instance)
	{
		if (member is TypeBuilderInstantiation || member is FieldOnTypeBuilderInst || member is ConstructorOnTypeBuilderInst || member is MethodOnTypeBuilderInst || member is SymbolType || member is FieldBuilder || member is TypeBuilder || member is ConstructorBuilder || member is MethodBuilder || member is GenericTypeParameterBuilder || member is EnumBuilder)
		{
			return GetPseudoToken(member, create_open_instance);
		}
		return getToken(this, member, create_open_instance);
	}

	internal int GetToken(MethodBase method, IEnumerable<Type> opt_param_types)
	{
		if (method is ConstructorBuilder || method is MethodBuilder)
		{
			return GetPseudoToken(method, create_open_instance: false);
		}
		if (opt_param_types == null)
		{
			return getToken(this, method, create_open_instance: true);
		}
		List<Type> list = new List<Type>(opt_param_types);
		return getMethodToken(this, method, list.ToArray());
	}

	internal int GetToken(MethodBase method, Type[] opt_param_types)
	{
		if (method is ConstructorBuilder || method is MethodBuilder)
		{
			return GetPseudoToken(method, create_open_instance: false);
		}
		return getMethodToken(this, method, opt_param_types);
	}

	internal int GetToken(SignatureHelper helper)
	{
		return getToken(this, helper, create_open_instance: true);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal extern void RegisterToken(object obj, int token);

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal extern object GetRegisteredToken(int token);

	internal TokenGenerator GetTokenGenerator()
	{
		if (token_gen == null)
		{
			token_gen = new ModuleBuilderTokenGenerator(this);
		}
		return token_gen;
	}

	internal static object RuntimeResolve(object obj)
	{
		if (obj is MethodBuilder)
		{
			return (obj as MethodBuilder).RuntimeResolve();
		}
		if (obj is ConstructorBuilder)
		{
			return (obj as ConstructorBuilder).RuntimeResolve();
		}
		if (obj is FieldBuilder)
		{
			return (obj as FieldBuilder).RuntimeResolve();
		}
		if (obj is GenericTypeParameterBuilder)
		{
			return (obj as GenericTypeParameterBuilder).RuntimeResolve();
		}
		if (obj is FieldOnTypeBuilderInst)
		{
			return (obj as FieldOnTypeBuilderInst).RuntimeResolve();
		}
		if (obj is MethodOnTypeBuilderInst)
		{
			return (obj as MethodOnTypeBuilderInst).RuntimeResolve();
		}
		if (obj is ConstructorOnTypeBuilderInst)
		{
			return (obj as ConstructorOnTypeBuilderInst).RuntimeResolve();
		}
		if (obj is Type)
		{
			return (obj as Type).RuntimeResolve();
		}
		throw new NotImplementedException(obj.GetType().FullName);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void build_metadata(ModuleBuilder mb);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void WriteToFile(IntPtr handle);

	private void FixupTokens(Dictionary<int, int> token_map, Dictionary<int, MemberInfo> member_map, Dictionary<MemberInfo, int> inst_tokens, bool open)
	{
		foreach (KeyValuePair<MemberInfo, int> inst_token in inst_tokens)
		{
			MemberInfo key = inst_token.Key;
			int value = inst_token.Value;
			MemberInfo memberInfo = null;
			if (key is TypeBuilderInstantiation || key is SymbolType)
			{
				memberInfo = (key as Type).RuntimeResolve();
			}
			else if (key is FieldOnTypeBuilderInst)
			{
				memberInfo = (key as FieldOnTypeBuilderInst).RuntimeResolve();
			}
			else if (key is ConstructorOnTypeBuilderInst)
			{
				memberInfo = (key as ConstructorOnTypeBuilderInst).RuntimeResolve();
			}
			else if (key is MethodOnTypeBuilderInst)
			{
				memberInfo = (key as MethodOnTypeBuilderInst).RuntimeResolve();
			}
			else if (key is FieldBuilder)
			{
				memberInfo = (key as FieldBuilder).RuntimeResolve();
			}
			else if (key is TypeBuilder)
			{
				memberInfo = (key as TypeBuilder).RuntimeResolve();
			}
			else if (key is EnumBuilder)
			{
				memberInfo = (key as EnumBuilder).RuntimeResolve();
			}
			else if (key is ConstructorBuilder)
			{
				memberInfo = (key as ConstructorBuilder).RuntimeResolve();
			}
			else if (key is MethodBuilder)
			{
				memberInfo = (key as MethodBuilder).RuntimeResolve();
			}
			else
			{
				if (!(key is GenericTypeParameterBuilder))
				{
					throw new NotImplementedException();
				}
				memberInfo = (key as GenericTypeParameterBuilder).RuntimeResolve();
			}
			int value2 = GetToken(memberInfo, open);
			token_map[value] = value2;
			member_map[value] = memberInfo;
			RegisterToken(memberInfo, value);
		}
	}

	private void FixupTokens()
	{
		Dictionary<int, int> token_map = new Dictionary<int, int>();
		Dictionary<int, MemberInfo> member_map = new Dictionary<int, MemberInfo>();
		if (inst_tokens != null)
		{
			FixupTokens(token_map, member_map, inst_tokens, open: false);
		}
		if (inst_tokens_open != null)
		{
			FixupTokens(token_map, member_map, inst_tokens_open, open: true);
		}
		if (types != null)
		{
			for (int i = 0; i < num_types; i++)
			{
				types[i].FixupTokens(token_map, member_map);
			}
		}
	}

	internal void Save()
	{
		if (transient && !is_main)
		{
			return;
		}
		if (types != null)
		{
			for (int i = 0; i < num_types; i++)
			{
				if (!types[i].is_created)
				{
					throw new NotSupportedException("Type '" + types[i].FullName + "' was not completed.");
				}
			}
		}
		FixupTokens();
		if (global_type != null && global_type_created == null)
		{
			global_type_created = global_type.CreateType();
		}
		if (resources != null)
		{
			for (int j = 0; j < resources.Length; j++)
			{
				if (resource_writers != null && resource_writers[resources[j].name] is IResourceWriter resourceWriter)
				{
					ResourceWriter obj = (ResourceWriter)resourceWriter;
					obj.Generate();
					MemoryStream memoryStream = (MemoryStream)obj._output;
					resources[j].data = new byte[memoryStream.Length];
					memoryStream.Seek(0L, SeekOrigin.Begin);
					memoryStream.Read(resources[j].data, 0, (int)memoryStream.Length);
					continue;
				}
				Stream stream = resources[j].stream;
				if (stream != null)
				{
					try
					{
						long length = stream.Length;
						resources[j].data = new byte[length];
						stream.Seek(0L, SeekOrigin.Begin);
						stream.Read(resources[j].data, 0, (int)length);
					}
					catch
					{
					}
				}
			}
		}
		build_metadata(this);
		string text = fqname;
		if (assemblyb.AssemblyDir != null)
		{
			text = Path.Combine(assemblyb.AssemblyDir, text);
		}
		try
		{
			File.Delete(text);
		}
		catch
		{
		}
		using (FileStream fileStream = new FileStream(text, FileMode.Create, FileAccess.Write))
		{
			WriteToFile(fileStream.Handle);
		}
		File.SetAttributes(text, (FileAttributes)(-2147483648));
		if (types != null && symbolWriter != null)
		{
			for (int k = 0; k < num_types; k++)
			{
				types[k].GenerateDebugInfo(symbolWriter);
			}
			symbolWriter.Close();
		}
	}

	internal void CreateGlobalType()
	{
		if (global_type == null)
		{
			global_type = new TypeBuilder(this, TypeAttributes.NotPublic, 1);
		}
	}

	internal override Guid GetModuleVersionId()
	{
		return new Guid(guid);
	}

	internal static Guid Mono_GetGuid(ModuleBuilder mb)
	{
		return mb.GetModuleVersionId();
	}

	/// <summary>For a description of this member, see <see cref="M:System.Runtime.InteropServices._ModuleBuilder.GetIDsOfNames(System.Guid@,System.IntPtr,System.UInt32,System.UInt32,System.IntPtr)" />. </summary>
	/// <param name="riid">Reserved for future use. Must be IID_NULL.</param>
	/// <param name="rgszNames">Passed-in array of names to be mapped.</param>
	/// <param name="cNames">Count of the names to be mapped.</param>
	/// <param name="lcid">The locale context in which to interpret the names.</param>
	/// <param name="rgDispId">Caller-allocated array which receives the IDs corresponding to the names.</param>
	/// <exception cref="T:System.NotImplementedException">The method is called late-bound using the COM IDispatch interface.</exception>
	void _ModuleBuilder.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
	{
		throw new NotImplementedException();
	}

	/// <summary>For a description of this member, see <see cref="M:System.Runtime.InteropServices._ModuleBuilder.GetTypeInfo(System.UInt32,System.UInt32,System.IntPtr)" />.</summary>
	/// <param name="iTInfo">The type information to return.</param>
	/// <param name="lcid">The locale identifier for the type information.</param>
	/// <param name="ppTInfo">A pointer to the requested type information object.</param>
	/// <exception cref="T:System.NotImplementedException">The method is called late-bound using the COM IDispatch interface.</exception>
	void _ModuleBuilder.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
	{
		throw new NotImplementedException();
	}

	/// <summary>For a description of this member, see <see cref="M:System.Runtime.InteropServices._ModuleBuilder.GetTypeInfoCount(System.UInt32@)" />.</summary>
	/// <param name="pcTInfo">The location that receives the number of type information interfaces provided by the object.</param>
	/// <exception cref="T:System.NotImplementedException">The method is called late-bound using the COM IDispatch interface.</exception>
	void _ModuleBuilder.GetTypeInfoCount(out uint pcTInfo)
	{
		throw new NotImplementedException();
	}

	/// <summary>For a description of this member, see <see cref="M:System.Runtime.InteropServices._ModuleBuilder.Invoke(System.UInt32,System.Guid@,System.UInt32,System.Int16,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)" />.</summary>
	/// <param name="dispIdMember">The member ID.</param>
	/// <param name="riid">Reserved for future use. Must be IID_NULL.</param>
	/// <param name="lcid">The locale context in which to interpret arguments.</param>
	/// <param name="wFlags">Flags describing the context of the call.</param>
	/// <param name="pDispParams">Pointer to a structure containing an array of arguments, an array of argument DISPIDs for named arguments, and counts for the number of elements in the arrays.</param>
	/// <param name="pVarResult">Pointer to the location where the result is to be stored.</param>
	/// <param name="pExcepInfo">Pointer to a structure that contains exception information.</param>
	/// <param name="puArgErr">The index of the first argument that has an error.</param>
	/// <exception cref="T:System.NotImplementedException">The method is called late-bound using the COM IDispatch interface.</exception>
	void _ModuleBuilder.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
	{
		throw new NotImplementedException();
	}

	/// <summary>Gets a value indicating whether the object is a resource.</summary>
	/// <returns>true if the object is a resource; otherwise, false.</returns>
	public override bool IsResource()
	{
		return false;
	}

	/// <summary>Returns the module-level method that matches the specified criteria.</summary>
	/// <returns>A method that is defined at the module level, and matches the specified criteria; or null if such a method does not exist.</returns>
	/// <param name="name">The method name. </param>
	/// <param name="bindingAttr">A combination of BindingFlags bit flags used to control the search. </param>
	/// <param name="binder">An object that implements Binder, containing properties related to this method. </param>
	/// <param name="callConvention">The calling convention for the method. </param>
	/// <param name="types">The parameter types of the method. </param>
	/// <param name="modifiers">An array of parameter modifiers used to make binding work with parameter signatures in which the types have been modified. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null, <paramref name="types" /> is null, or an element of <paramref name="types" /> is null. </exception>
	protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
	{
		if (global_type_created == null)
		{
			return null;
		}
		if (types == null)
		{
			return global_type_created.GetMethod(name);
		}
		return global_type_created.GetMethod(name, bindingAttr, binder, callConvention, types, modifiers);
	}

	/// <summary>Returns the field identified by the specified metadata token, in the context defined by the specified generic type parameters.</summary>
	/// <returns>A <see cref="T:System.Reflection.FieldInfo" /> object representing the field that is identified by the specified metadata token.</returns>
	/// <param name="metadataToken">A metadata token that identifies a field in the module.</param>
	/// <param name="genericTypeArguments">An array of <see cref="T:System.Type" /> objects representing the generic type arguments of the type where the token is in scope, or null if that type is not generic. </param>
	/// <param name="genericMethodArguments">An array of <see cref="T:System.Type" /> objects representing the generic type arguments of the method where the token is in scope, or null if that method is not generic.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="metadataToken" /> is not a token for a field in the scope of the current module.-or-<paramref name="metadataToken" /> identifies a field whose parent TypeSpec has a signature containing element type var (a type parameter of a generic type) or mvar (a type parameter of a generic method), and the necessary generic type arguments were not supplied for either or both of <paramref name="genericTypeArguments" /> and <paramref name="genericMethodArguments" />.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="metadataToken" /> is not a valid token in the scope of the current module.</exception>
	public override FieldInfo ResolveField(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
	{
		ResolveTokenError error;
		IntPtr intPtr = Module.ResolveFieldToken(_impl, metadataToken, ptrs_from_types(genericTypeArguments), ptrs_from_types(genericMethodArguments), out error);
		if (intPtr == IntPtr.Zero)
		{
			throw resolve_token_exception(metadataToken, error, "Field");
		}
		return FieldInfo.GetFieldFromHandle(new RuntimeFieldHandle(intPtr));
	}

	/// <summary>Returns the type or member identified by the specified metadata token, in the context defined by the specified generic type parameters.</summary>
	/// <returns>A <see cref="T:System.Reflection.MemberInfo" /> object representing the type or member that is identified by the specified metadata token.</returns>
	/// <param name="metadataToken">A metadata token that identifies a type or member in the module.</param>
	/// <param name="genericTypeArguments">An array of <see cref="T:System.Type" /> objects representing the generic type arguments of the type where the token is in scope, or null if that type is not generic. </param>
	/// <param name="genericMethodArguments">An array of <see cref="T:System.Type" /> objects representing the generic type arguments of the method where the token is in scope, or null if that method is not generic.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="metadataToken" /> is not a token for a type or member in the scope of the current module.-or-<paramref name="metadataToken" /> is a MethodSpec or TypeSpec whose signature contains element type var (a type parameter of a generic type) or mvar (a type parameter of a generic method), and the necessary generic type arguments were not supplied for either or both of <paramref name="genericTypeArguments" /> and <paramref name="genericMethodArguments" />.-or-<paramref name="metadataToken" /> identifies a property or event.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="metadataToken" /> is not a valid token in the scope of the current module.</exception>
	public override MemberInfo ResolveMember(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
	{
		ResolveTokenError error;
		MemberInfo memberInfo = Module.ResolveMemberToken(_impl, metadataToken, ptrs_from_types(genericTypeArguments), ptrs_from_types(genericMethodArguments), out error);
		if (memberInfo == null)
		{
			throw resolve_token_exception(metadataToken, error, "MemberInfo");
		}
		return memberInfo;
	}

	internal MemberInfo ResolveOrGetRegisteredToken(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
	{
		MemberInfo memberInfo = Module.ResolveMemberToken(_impl, metadataToken, ptrs_from_types(genericTypeArguments), ptrs_from_types(genericMethodArguments), out var error);
		if (memberInfo != null)
		{
			return memberInfo;
		}
		memberInfo = GetRegisteredToken(metadataToken) as MemberInfo;
		if (memberInfo == null)
		{
			throw resolve_token_exception(metadataToken, error, "MemberInfo");
		}
		return memberInfo;
	}

	/// <summary>Returns the method or constructor identified by the specified metadata token, in the context defined by the specified generic type parameters. </summary>
	/// <returns>A <see cref="T:System.Reflection.MethodBase" /> object representing the method that is identified by the specified metadata token.</returns>
	/// <param name="metadataToken">A metadata token that identifies a method or constructor in the module.</param>
	/// <param name="genericTypeArguments">An array of <see cref="T:System.Type" /> objects representing the generic type arguments of the type where the token is in scope, or null if that type is not generic. </param>
	/// <param name="genericMethodArguments">An array of <see cref="T:System.Type" /> objects representing the generic type arguments of the method where the token is in scope, or null if that method is not generic.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="metadataToken" /> is not a token for a method or constructor in the scope of the current module.-or-<paramref name="metadataToken" /> is a MethodSpec whose signature contains element type var (a type parameter of a generic type) or mvar (a type parameter of a generic method), and the necessary generic type arguments were not supplied for either or both of <paramref name="genericTypeArguments" /> and <paramref name="genericMethodArguments" />.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="metadataToken" /> is not a valid token in the scope of the current module.</exception>
	public override MethodBase ResolveMethod(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
	{
		ResolveTokenError error;
		IntPtr intPtr = Module.ResolveMethodToken(_impl, metadataToken, ptrs_from_types(genericTypeArguments), ptrs_from_types(genericMethodArguments), out error);
		if (intPtr == IntPtr.Zero)
		{
			throw resolve_token_exception(metadataToken, error, "MethodBase");
		}
		return MethodBase.GetMethodFromHandleNoGenericCheck(new RuntimeMethodHandle(intPtr));
	}

	/// <summary>Returns the string identified by the specified metadata token.</summary>
	/// <returns>A <see cref="T:System.String" /> containing a string value from the metadata string heap.</returns>
	/// <param name="metadataToken">A metadata token that identifies a string in the string heap of the module.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="metadataToken" /> is not a token for a string in the scope of the current module. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="metadataToken" /> is not a valid token in the scope of the current module.</exception>
	public override string ResolveString(int metadataToken)
	{
		ResolveTokenError error;
		string text = Module.ResolveStringToken(_impl, metadataToken, out error);
		if (text == null)
		{
			throw resolve_token_exception(metadataToken, error, "string");
		}
		return text;
	}

	/// <summary>Returns the signature blob identified by a metadata token.</summary>
	/// <returns>An array of bytes representing the signature blob.</returns>
	/// <param name="metadataToken">A metadata token that identifies a signature in the module.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="metadataToken" /> is not a valid MemberRef, MethodDef, TypeSpec, signature, or FieldDef token in the scope of the current module.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="metadataToken" /> is not a valid token in the scope of the current module.</exception>
	public override byte[] ResolveSignature(int metadataToken)
	{
		ResolveTokenError error;
		byte[] array = Module.ResolveSignature(_impl, metadataToken, out error);
		if (array == null)
		{
			throw resolve_token_exception(metadataToken, error, "signature");
		}
		return array;
	}

	/// <summary>Returns the type identified by the specified metadata token, in the context defined by the specified generic type parameters.</summary>
	/// <returns>A <see cref="T:System.Type" /> object representing the type that is identified by the specified metadata token.</returns>
	/// <param name="metadataToken">A metadata token that identifies a type in the module.</param>
	/// <param name="genericTypeArguments">An array of <see cref="T:System.Type" /> objects representing the generic type arguments of the type where the token is in scope, or null if that type is not generic. </param>
	/// <param name="genericMethodArguments">An array of <see cref="T:System.Type" /> objects representing the generic type arguments of the method where the token is in scope, or null if that method is not generic.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="metadataToken" /> is not a token for a type in the scope of the current module.-or-<paramref name="metadataToken" /> is a TypeSpec whose signature contains element type var (a type parameter of a generic type) or mvar (a type parameter of a generic method), and the necessary generic type arguments were not supplied for either or both of <paramref name="genericTypeArguments" /> and <paramref name="genericMethodArguments" />.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="metadataToken" /> is not a valid token in the scope of the current module.</exception>
	public override Type ResolveType(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
	{
		ResolveTokenError error;
		IntPtr intPtr = Module.ResolveTypeToken(_impl, metadataToken, ptrs_from_types(genericTypeArguments), ptrs_from_types(genericMethodArguments), out error);
		if (intPtr == IntPtr.Zero)
		{
			throw resolve_token_exception(metadataToken, error, "Type");
		}
		return Type.GetTypeFromHandle(new RuntimeTypeHandle(intPtr));
	}

	/// <summary>Returns a value that indicates whether this instance is equal to the specified object.</summary>
	/// <returns>true if <paramref name="obj" /> equals the type and value of this instance; otherwise, false.</returns>
	/// <param name="obj">An object to compare with this instance, or null.</param>
	public override bool Equals(object obj)
	{
		return base.Equals(obj);
	}

	/// <summary>Returns the hash code for this instance.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	/// <summary>Returns a value that indicates whether the specified attribute type has been applied to this module.</summary>
	/// <returns>true if one or more instances of <paramref name="attributeType" /> have been applied to this module; otherwise, false.</returns>
	/// <param name="attributeType">The type of custom attribute to test for.</param>
	/// <param name="inherit">This argument is ignored for objects of this type.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="attributeType" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="attributeType" /> is not a <see cref="T:System.Type" /> object supplied by the runtime. For example, <paramref name="attributeType" /> is a <see cref="T:System.Reflection.Emit.TypeBuilder" /> object.</exception>
	public override bool IsDefined(Type attributeType, bool inherit)
	{
		return base.IsDefined(attributeType, inherit);
	}

	/// <summary>Returns all the custom attributes that have been applied to the current <see cref="T:System.Reflection.Emit.ModuleBuilder" />.</summary>
	/// <returns>An array that contains the custom attributes; the array is empty if there are no attributes.</returns>
	/// <param name="inherit">This argument is ignored for objects of this type.</param>
	public override object[] GetCustomAttributes(bool inherit)
	{
		return GetCustomAttributes(null, inherit);
	}

	/// <summary>Returns all the custom attributes that have been applied to the current <see cref="T:System.Reflection.Emit.ModuleBuilder" />, and that derive from a specified attribute type.</summary>
	/// <returns>An array that contains the custom attributes that are derived, at any level, from <paramref name="attributeType" />; the array is empty if there are no such attributes.</returns>
	/// <param name="attributeType">The base type from which attributes derive.</param>
	/// <param name="inherit">This argument is ignored for objects of this type.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="attributeType" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="attributeType" /> is not a <see cref="T:System.Type" /> object supplied by the runtime. For example, <paramref name="attributeType" /> is a <see cref="T:System.Reflection.Emit.TypeBuilder" /> object.</exception>
	public override object[] GetCustomAttributes(Type attributeType, bool inherit)
	{
		if (cattrs == null || cattrs.Length == 0)
		{
			return Array.Empty<object>();
		}
		if (attributeType is TypeBuilder)
		{
			throw new InvalidOperationException("First argument to GetCustomAttributes can't be a TypeBuilder");
		}
		List<object> list = new List<object>();
		for (int i = 0; i < cattrs.Length; i++)
		{
			Type type = cattrs[i].Ctor.GetType();
			if (type is TypeBuilder)
			{
				throw new InvalidOperationException("Can't construct custom attribute for TypeBuilder type");
			}
			if (attributeType == null || attributeType.IsAssignableFrom(type))
			{
				list.Add(cattrs[i].Invoke());
			}
		}
		return list.ToArray();
	}

	/// <summary>Returns a module-level field, defined in the .sdata region of the portable executable (PE) file, that has the specified name and binding attributes.</summary>
	/// <returns>A field that has the specified name and binding attributes, or null if the field does not exist.</returns>
	/// <param name="name">The field name. </param>
	/// <param name="bindingAttr">A combination of the BindingFlags bit flags used to control the search. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="name" /> parameter is null. </exception>
	public override FieldInfo GetField(string name, BindingFlags bindingAttr)
	{
		if (global_type_created == null)
		{
			throw new InvalidOperationException("Module-level fields cannot be retrieved until after the CreateGlobalFunctions method has been called for the module.");
		}
		return global_type_created.GetField(name, bindingAttr);
	}

	/// <summary>Returns all fields defined in the .sdata region of the portable executable (PE) file that match the specified binding flags.</summary>
	/// <returns>An array of fields that match the specified flags; the array is empty if no such fields exist.</returns>
	/// <param name="bindingFlags">A combination of the BindingFlags bit flags used to control the search.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="name" /> parameter is null. </exception>
	public override FieldInfo[] GetFields(BindingFlags bindingFlags)
	{
		if (global_type_created == null)
		{
			throw new InvalidOperationException("Module-level fields cannot be retrieved until after the CreateGlobalFunctions method has been called for the module.");
		}
		return global_type_created.GetFields(bindingFlags);
	}

	/// <summary>Returns all the methods that have been defined at the module level for the current <see cref="T:System.Reflection.Emit.ModuleBuilder" />, and that match the specified binding flags.</summary>
	/// <returns>An array that contains all the module-level methods that match <paramref name="bindingFlags" />.</returns>
	/// <param name="bindingFlags">A combination of BindingFlags bit flags used to control the search.</param>
	public override MethodInfo[] GetMethods(BindingFlags bindingFlags)
	{
		if (global_type_created == null)
		{
			throw new InvalidOperationException("Module-level methods cannot be retrieved until after the CreateGlobalFunctions method has been called for the module.");
		}
		return global_type_created.GetMethods(bindingFlags);
	}

	internal ModuleBuilder()
	{
		ThrowStub.ThrowNotSupportedException();
	}
}
