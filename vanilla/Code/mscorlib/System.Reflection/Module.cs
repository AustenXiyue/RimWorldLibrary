using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;

namespace System.Reflection;

/// <summary>Performs reflection on a module.</summary>
[Serializable]
[StructLayout(LayoutKind.Sequential)]
[ComVisible(true)]
[ComDefaultInterface(typeof(_Module))]
[ClassInterface(ClassInterfaceType.None)]
public abstract class Module : ISerializable, ICustomAttributeProvider, _Module
{
	/// <summary>A TypeFilter object that filters the list of types defined in this module based upon the name. This field is case-sensitive and read-only.</summary>
	public static readonly TypeFilter FilterTypeName = filter_by_type_name;

	/// <summary>A TypeFilter object that filters the list of types defined in this module based upon the name. This field is case-insensitive and read-only.</summary>
	public static readonly TypeFilter FilterTypeNameIgnoreCase = filter_by_type_name_ignore_case;

	internal IntPtr _impl;

	internal Assembly assembly;

	internal string fqname;

	internal string name;

	internal string scopename;

	internal bool is_resource;

	internal int token;

	private const BindingFlags defaultBindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;

	/// <summary>Gets a handle for the module.</summary>
	/// <returns>A <see cref="T:System.ModuleHandle" /> structure for the current module.</returns>
	public ModuleHandle ModuleHandle => new ModuleHandle(_impl);

	internal Guid MvId => GetModuleVersionId();

	/// <summary>Gets the appropriate <see cref="T:System.Reflection.Assembly" /> for this instance of <see cref="T:System.Reflection.Module" />.</summary>
	/// <returns>An Assembly object.</returns>
	public virtual Assembly Assembly
	{
		get
		{
			throw CreateNIE();
		}
	}

	/// <summary>Gets a String representing the name of the module with the path removed.</summary>
	/// <returns>The module name with no path.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public virtual string Name
	{
		get
		{
			throw CreateNIE();
		}
	}

	/// <summary>Gets a string representing the name of the module.</summary>
	/// <returns>The module name.</returns>
	public virtual string ScopeName
	{
		get
		{
			throw CreateNIE();
		}
	}

	/// <summary>Gets the metadata stream version.</summary>
	/// <returns>A 32-bit integer representing the metadata stream version. The high-order two bytes represent the major version number, and the low-order two bytes represent the minor version number.</returns>
	public virtual int MDStreamVersion
	{
		get
		{
			throw CreateNIE();
		}
	}

	/// <summary>Gets a universally unique identifier (UUID) that can be used to distinguish between two versions of a module.</summary>
	/// <returns>A <see cref="T:System.Guid" /> that can be used to distinguish between two versions of a module.</returns>
	public virtual Guid ModuleVersionId
	{
		get
		{
			throw CreateNIE();
		}
	}

	/// <summary>Gets a string representing the fully qualified name and path to this module.</summary>
	/// <returns>The fully qualified module name.</returns>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permissions. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public virtual string FullyQualifiedName
	{
		get
		{
			throw CreateNIE();
		}
	}

	/// <summary>Gets a token that identifies the module in metadata.</summary>
	/// <returns>An integer token that identifies the current module in metadata.</returns>
	public virtual int MetadataToken
	{
		get
		{
			throw CreateNIE();
		}
	}

	/// <summary>Gets a collection that contains this module's custom attributes.</summary>
	/// <returns>A collection that contains this module's custom attributes.</returns>
	public virtual IEnumerable<CustomAttributeData> CustomAttributes => GetCustomAttributesData();

	/// <summary>Initializes a new instance of the <see cref="T:System.Reflection.Module" /> class.</summary>
	protected Module()
	{
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern int get_MetadataToken(Module module);

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern int GetMDStreamVersion(IntPtr module_handle);

	/// <summary>Returns a field having the specified name.</summary>
	/// <returns>A FieldInfo object having the specified name, or null if the field does not exist.</returns>
	/// <param name="name">The field name. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="name" /> parameter is null. </exception>
	public FieldInfo GetField(string name)
	{
		return GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
	}

	/// <summary>Returns the global fields defined on the module.</summary>
	/// <returns>An array of <see cref="T:System.Reflection.FieldInfo" /> objects representing the global fields defined on the module; if there are no global fields, an empty array is returned.</returns>
	public FieldInfo[] GetFields()
	{
		return GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
	}

	/// <summary>Returns a method having the specified name.</summary>
	/// <returns>A MethodInfo object having the specified name, or null if the method does not exist.</returns>
	/// <param name="name">The method name. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	public MethodInfo GetMethod(string name)
	{
		return GetMethodImpl(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public, null, CallingConventions.Any, null, null);
	}

	/// <summary>Returns a method having the specified name and parameter types.</summary>
	/// <returns>A MethodInfo object in accordance with the specified criteria, or null if the method does not exist.</returns>
	/// <param name="name">The method name. </param>
	/// <param name="types">The parameter types to search for. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null, <paramref name="types" /> is null, or <paramref name="types" /> (i) is null. </exception>
	public MethodInfo GetMethod(string name, Type[] types)
	{
		if (types == null)
		{
			throw new ArgumentNullException("types");
		}
		return GetMethodImpl(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public, null, CallingConventions.Any, types, null);
	}

	/// <summary>Returns a method having the specified name, binding information, calling convention, and parameter types and modifiers.</summary>
	/// <returns>A MethodInfo object in accordance with the specified criteria, or null if the method does not exist.</returns>
	/// <param name="name">The method name. </param>
	/// <param name="bindingAttr">One of the BindingFlags bit flags used to control the search. </param>
	/// <param name="binder">An object that implements Binder, containing properties related to this method. </param>
	/// <param name="callConvention">The calling convention for the method. </param>
	/// <param name="types">The parameter types to search for. </param>
	/// <param name="modifiers">An array of parameter modifiers used to make binding work with parameter signatures in which the types have been modified. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null, <paramref name="types" /> is null, or <paramref name="types" /> (i) is null. </exception>
	public MethodInfo GetMethod(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
	{
		if (types == null)
		{
			throw new ArgumentNullException("types");
		}
		return GetMethodImpl(name, bindingAttr, binder, callConvention, types, modifiers);
	}

	/// <summary>Returns the global methods defined on the module.</summary>
	/// <returns>An array of <see cref="T:System.Reflection.MethodInfo" /> objects representing all the global methods defined on the module; if there are no global methods, an empty array is returned.</returns>
	public MethodInfo[] GetMethods()
	{
		return GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
	}

	/// <summary>Provides an <see cref="T:System.Runtime.Serialization.ISerializable" /> implementation for serialized objects.</summary>
	/// <param name="info">The information and data needed to serialize or deserialize an object. </param>
	/// <param name="context">The context for the serialization. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="info" /> is null. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter" />
	/// </PermissionSet>
	[SecurityCritical]
	[SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		throw new NotImplementedException();
	}

	/// <summary>Returns the specified type, performing a case-sensitive search.</summary>
	/// <returns>A Type object representing the given type, if the type is in this module; otherwise, null.</returns>
	/// <param name="className">The name of the type to locate. The name must be fully qualified with the namespace. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="className" /> is null. </exception>
	/// <exception cref="T:System.Reflection.TargetInvocationException">The class initializers are invoked and an exception is thrown. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="className" /> is a zero-length string. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="className" /> requires a dependent assembly that could not be found. </exception>
	/// <exception cref="T:System.IO.FileLoadException">
	///   <paramref name="className" /> requires a dependent assembly that was found but could not be loaded.-or-The current assembly was loaded into the reflection-only context, and <paramref name="className" /> requires a dependent assembly that was not preloaded. </exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="className" /> requires a dependent assembly, but the file is not a valid assembly. -or-<paramref name="className" /> requires a dependent assembly which was compiled for a version of the runtime later than the currently loaded version.</exception>
	[ComVisible(true)]
	public virtual Type GetType(string className)
	{
		return GetType(className, throwOnError: false, ignoreCase: false);
	}

	/// <summary>Returns the specified type, searching the module with the specified case sensitivity.</summary>
	/// <returns>A Type object representing the given type, if the type is in this module; otherwise, null.</returns>
	/// <param name="className">The name of the type to locate. The name must be fully qualified with the namespace. </param>
	/// <param name="ignoreCase">true for case-insensitive search; otherwise, false. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="className" /> is null. </exception>
	/// <exception cref="T:System.Reflection.TargetInvocationException">The class initializers are invoked and an exception is thrown. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="className" /> is a zero-length string. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="className" /> requires a dependent assembly that could not be found. </exception>
	/// <exception cref="T:System.IO.FileLoadException">
	///   <paramref name="className" /> requires a dependent assembly that was found but could not be loaded.-or-The current assembly was loaded into the reflection-only context, and <paramref name="className" /> requires a dependent assembly that was not preloaded. </exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="className" /> requires a dependent assembly, but the file is not a valid assembly. -or-<paramref name="className" /> requires a dependent assembly which was compiled for a version of the runtime later than the currently loaded version.</exception>
	[ComVisible(true)]
	public virtual Type GetType(string className, bool ignoreCase)
	{
		return GetType(className, throwOnError: false, ignoreCase);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal extern Type[] InternalGetTypes();

	/// <summary>Returns the name of the module.</summary>
	/// <returns>A String representing the name of this module.</returns>
	public override string ToString()
	{
		return name;
	}

	internal Exception resolve_token_exception(int metadataToken, ResolveTokenError error, string tokenType)
	{
		if (error == ResolveTokenError.OutOfRange)
		{
			return new ArgumentOutOfRangeException("metadataToken", $"Token 0x{metadataToken:x} is not valid in the scope of module {name}");
		}
		return new ArgumentException($"Token 0x{metadataToken:x} is not a valid {tokenType} token in the scope of module {name}", "metadataToken");
	}

	internal IntPtr[] ptrs_from_types(Type[] types)
	{
		if (types == null)
		{
			return null;
		}
		IntPtr[] array = new IntPtr[types.Length];
		for (int i = 0; i < types.Length; i++)
		{
			if (types[i] == null)
			{
				throw new ArgumentException();
			}
			array[i] = types[i].TypeHandle.Value;
		}
		return array;
	}

	/// <summary>Returns the field identified by the specified metadata token.</summary>
	/// <returns>A <see cref="T:System.Reflection.FieldInfo" /> object representing the field that is identified by the specified metadata token.</returns>
	/// <param name="metadataToken">A metadata token that identifies a field in the module.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="metadataToken" /> is not a token for a field in the scope of the current module.-or-<paramref name="metadataToken" /> identifies a field whose parent TypeSpec has a signature containing element type var (a type parameter of a generic type) or mvar (a type parameter of a generic method).</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="metadataToken" /> is not a valid token in the scope of the current module.</exception>
	public FieldInfo ResolveField(int metadataToken)
	{
		return ResolveField(metadataToken, null, null);
	}

	/// <summary>Returns the type or member identified by the specified metadata token.</summary>
	/// <returns>A <see cref="T:System.Reflection.MemberInfo" /> object representing the type or member that is identified by the specified metadata token.</returns>
	/// <param name="metadataToken">A metadata token that identifies a type or member in the module.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="metadataToken" /> is not a token for a type or member in the scope of the current module.-or-<paramref name="metadataToken" /> is a MethodSpec or TypeSpec whose signature contains element type var (a type parameter of a generic type) or mvar (a type parameter of a generic method).-or-<paramref name="metadataToken" /> identifies a property or event.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="metadataToken" /> is not a valid token in the scope of the current module.</exception>
	public MemberInfo ResolveMember(int metadataToken)
	{
		return ResolveMember(metadataToken, null, null);
	}

	/// <summary>Returns the method or constructor identified by the specified metadata token.</summary>
	/// <returns>A <see cref="T:System.Reflection.MethodBase" /> object representing the method or constructor that is identified by the specified metadata token.</returns>
	/// <param name="metadataToken">A metadata token that identifies a method or constructor in the module.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="metadataToken" /> is not a token for a method or constructor in the scope of the current module.-or-<paramref name="metadataToken" /> is a MethodSpec whose signature contains element type var (a type parameter of a generic type) or mvar (a type parameter of a generic method).</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="metadataToken" /> is not a valid token in the scope of the current module.</exception>
	public MethodBase ResolveMethod(int metadataToken)
	{
		return ResolveMethod(metadataToken, null, null);
	}

	/// <summary>Returns the type identified by the specified metadata token.</summary>
	/// <returns>A <see cref="T:System.Type" /> object representing the type that is identified by the specified metadata token.</returns>
	/// <param name="metadataToken">A metadata token that identifies a type in the module.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="metadataToken" /> is not a token for a type in the scope of the current module.-or-<paramref name="metadataToken" /> is a TypeSpec whose signature contains element type var (a type parameter of a generic type) or mvar (a type parameter of a generic method). </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="metadataToken" /> is not a valid token in the scope of the current module.</exception>
	public Type ResolveType(int metadataToken)
	{
		return ResolveType(metadataToken, null, null);
	}

	internal static Type MonoDebugger_ResolveType(Module module, int token)
	{
		ResolveTokenError error;
		IntPtr intPtr = ResolveTypeToken(module._impl, token, null, null, out error);
		if (intPtr == IntPtr.Zero)
		{
			return null;
		}
		return Type.GetTypeFromHandle(new RuntimeTypeHandle(intPtr));
	}

	internal static Guid Mono_GetGuid(Module module)
	{
		return module.GetModuleVersionId();
	}

	internal virtual Guid GetModuleVersionId()
	{
		return new Guid(GetGuidInternal());
	}

	private static bool filter_by_type_name(Type m, object filterCriteria)
	{
		string text = (string)filterCriteria;
		if (text.Length > 0 && text[text.Length - 1] == '*')
		{
			return m.Name.StartsWithOrdinalUnchecked(text.Substring(0, text.Length - 1));
		}
		return m.Name == text;
	}

	private static bool filter_by_type_name_ignore_case(Type m, object filterCriteria)
	{
		string text = (string)filterCriteria;
		if (text.Length > 0 && text[text.Length - 1] == '*')
		{
			return m.Name.StartsWith(text.Substring(0, text.Length - 1), StringComparison.OrdinalIgnoreCase);
		}
		return string.Compare(m.Name, text, StringComparison.OrdinalIgnoreCase) == 0;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal extern IntPtr GetHINSTANCE();

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern string GetGuidInternal();

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal extern Type GetGlobalType();

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern IntPtr ResolveTypeToken(IntPtr module, int token, IntPtr[] type_args, IntPtr[] method_args, out ResolveTokenError error);

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern IntPtr ResolveMethodToken(IntPtr module, int token, IntPtr[] type_args, IntPtr[] method_args, out ResolveTokenError error);

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern IntPtr ResolveFieldToken(IntPtr module, int token, IntPtr[] type_args, IntPtr[] method_args, out ResolveTokenError error);

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern string ResolveStringToken(IntPtr module, int token, out ResolveTokenError error);

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern MemberInfo ResolveMemberToken(IntPtr module, int token, IntPtr[] type_args, IntPtr[] method_args, out ResolveTokenError error);

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern byte[] ResolveSignature(IntPtr module, int metadataToken, out ResolveTokenError error);

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern void GetPEKind(IntPtr module, out PortableExecutableKinds peKind, out ImageFileMachine machine);

	/// <summary>Maps a set of names to a corresponding set of dispatch identifiers.</summary>
	/// <param name="riid">Reserved for future use. Must be IID_NULL.</param>
	/// <param name="rgszNames">Passed-in array of names to be mapped.</param>
	/// <param name="cNames">Count of the names to be mapped.</param>
	/// <param name="lcid">The locale context in which to interpret the names.</param>
	/// <param name="rgDispId">Caller-allocated array that receives the IDs corresponding to the names.</param>
	/// <exception cref="T:System.NotImplementedException">Late-bound access using the COM IDispatch interface is not supported.</exception>
	void _Module.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
	{
		throw new NotImplementedException();
	}

	/// <summary>Retrieves the type information for an object, which can then be used to get the type information for an interface.</summary>
	/// <param name="iTInfo">The type information to return.</param>
	/// <param name="lcid">The locale identifier for the type information.</param>
	/// <param name="ppTInfo">Receives a pointer to the requested type information object.</param>
	/// <exception cref="T:System.NotImplementedException">Late-bound access using the COM IDispatch interface is not supported.</exception>
	void _Module.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
	{
		throw new NotImplementedException();
	}

	/// <summary>Retrieves the number of type information interfaces that an object provides (either 0 or 1).</summary>
	/// <param name="pcTInfo">Points to a location that receives the number of type information interfaces provided by the object.</param>
	/// <exception cref="T:System.NotImplementedException">Late-bound access using the COM IDispatch interface is not supported.</exception>
	void _Module.GetTypeInfoCount(out uint pcTInfo)
	{
		throw new NotImplementedException();
	}

	/// <summary>Provides access to properties and methods exposed by an object.</summary>
	/// <param name="dispIdMember">Identifies the member.</param>
	/// <param name="riid">Reserved for future use. Must be IID_NULL.</param>
	/// <param name="lcid">The locale context in which to interpret arguments.</param>
	/// <param name="wFlags">Flags describing the context of the call.</param>
	/// <param name="pDispParams">Pointer to a structure containing an array of arguments, an array of argument DispIDs for named arguments, and counts for the number of elements in the arrays.</param>
	/// <param name="pVarResult">Pointer to the location where the result is to be stored.</param>
	/// <param name="pExcepInfo">Pointer to a structure that contains exception information.</param>
	/// <param name="puArgErr">The index of the first argument that has an error.</param>
	/// <exception cref="T:System.NotImplementedException">Late-bound access using the COM IDispatch interface is not supported.</exception>
	void _Module.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
	{
		throw new NotImplementedException();
	}

	/// <summary>Determines whether this module and the specified object are equal.</summary>
	/// <returns>true if <paramref name="o" /> is equal to this instance; otherwise, false.</returns>
	/// <param name="o">The object to compare with this instance. </param>
	public override bool Equals(object o)
	{
		return o == this;
	}

	/// <summary>Returns the hash code for this instance.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	/// <summary>Indicates whether two <see cref="T:System.Reflection.Module" /> objects are equal.</summary>
	/// <returns>true if <paramref name="left" /> is equal to <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">The first object to compare. </param>
	/// <param name="right">The second object to compare.</param>
	public static bool operator ==(Module left, Module right)
	{
		if ((object)left == right)
		{
			return true;
		}
		if (((object)left == null) ^ ((object)right == null))
		{
			return false;
		}
		return left.Equals(right);
	}

	/// <summary>Indicates whether two <see cref="T:System.Reflection.Module" /> objects are not equal.</summary>
	/// <returns>true if <paramref name="left" /> is not equal to <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">The first object to compare.</param>
	/// <param name="right">The second object to compare.</param>
	public static bool operator !=(Module left, Module right)
	{
		if ((object)left == right)
		{
			return false;
		}
		if (((object)left == null) ^ ((object)right == null))
		{
			return true;
		}
		return !left.Equals(right);
	}

	private static Exception CreateNIE()
	{
		return new NotImplementedException("Derived classes must implement it");
	}

	/// <summary>Gets a value indicating whether the object is a resource.</summary>
	/// <returns>true if the object is a resource; otherwise, false.</returns>
	public virtual bool IsResource()
	{
		throw CreateNIE();
	}

	/// <summary>Returns an array of classes accepted by the given filter and filter criteria.</summary>
	/// <returns>An array of type Type containing classes that were accepted by the filter.</returns>
	/// <param name="filter">The delegate used to filter the classes. </param>
	/// <param name="filterCriteria">An Object used to filter the classes. </param>
	/// <exception cref="T:System.Reflection.ReflectionTypeLoadException">One or more classes in a module could not be loaded. </exception>
	public virtual Type[] FindTypes(TypeFilter filter, object filterCriteria)
	{
		throw CreateNIE();
	}

	/// <summary>Returns all custom attributes.</summary>
	/// <returns>An array of type Object containing all custom attributes.</returns>
	/// <param name="inherit">This argument is ignored for objects of this type. </param>
	public virtual object[] GetCustomAttributes(bool inherit)
	{
		throw CreateNIE();
	}

	/// <summary>Gets custom attributes of the specified type.</summary>
	/// <returns>An array of type Object containing all custom attributes of the specified type.</returns>
	/// <param name="attributeType">The type of attribute to get. </param>
	/// <param name="inherit">This argument is ignored for objects of this type. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="attributeType" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="attributeType" /> is not a <see cref="T:System.Type" /> object supplied by the runtime. For example, <paramref name="attributeType" /> is a <see cref="T:System.Reflection.Emit.TypeBuilder" /> object.</exception>
	public virtual object[] GetCustomAttributes(Type attributeType, bool inherit)
	{
		throw CreateNIE();
	}

	/// <summary>Returns a list of <see cref="T:System.Reflection.CustomAttributeData" /> objects for the current module, which can be used in the reflection-only context.</summary>
	/// <returns>A generic list of <see cref="T:System.Reflection.CustomAttributeData" /> objects representing data about the attributes that have been applied to the current module.</returns>
	public virtual IList<CustomAttributeData> GetCustomAttributesData()
	{
		throw CreateNIE();
	}

	/// <summary>Returns a field having the specified name and binding attributes.</summary>
	/// <returns>A FieldInfo object having the specified name and binding attributes, or null if the field does not exist.</returns>
	/// <param name="name">The field name. </param>
	/// <param name="bindingAttr">One of the BindingFlags bit flags used to control the search. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="name" /> parameter is null. </exception>
	public virtual FieldInfo GetField(string name, BindingFlags bindingAttr)
	{
		throw CreateNIE();
	}

	/// <summary>Returns the global fields defined on the module that match the specified binding flags.</summary>
	/// <returns>An array of type <see cref="T:System.Reflection.FieldInfo" /> representing the global fields defined on the module that match the specified binding flags; if no global fields match the binding flags, an empty array is returned.</returns>
	/// <param name="bindingFlags">A bitwise combination of <see cref="T:System.Reflection.BindingFlags" /> values that limit the search.</param>
	public virtual FieldInfo[] GetFields(BindingFlags bindingFlags)
	{
		throw CreateNIE();
	}

	/// <summary>Returns the method implementation in accordance with the specified criteria.</summary>
	/// <returns>A MethodInfo object containing implementation information as specified, or null if the method does not exist.</returns>
	/// <param name="name">The method name. </param>
	/// <param name="bindingAttr">One of the BindingFlags bit flags used to control the search. </param>
	/// <param name="binder">An object that implements Binder, containing properties related to this method. </param>
	/// <param name="callConvention">The calling convention for the method. </param>
	/// <param name="types">The parameter types to search for. </param>
	/// <param name="modifiers">An array of parameter modifiers used to make binding work with parameter signatures in which the types have been modified. </param>
	/// <exception cref="T:System.Reflection.AmbiguousMatchException">
	///   <paramref name="types" /> is null. </exception>
	protected virtual MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
	{
		throw CreateNIE();
	}

	/// <summary>Returns the global methods defined on the module that match the specified binding flags.</summary>
	/// <returns>An array of type <see cref="T:System.Reflection.MethodInfo" /> representing the global methods defined on the module that match the specified binding flags; if no global methods match the binding flags, an empty array is returned.</returns>
	/// <param name="bindingFlags">A bitwise combination of <see cref="T:System.Reflection.BindingFlags" /> values that limit the search.</param>
	public virtual MethodInfo[] GetMethods(BindingFlags bindingFlags)
	{
		throw CreateNIE();
	}

	/// <summary>Gets a pair of values indicating the nature of the code in a module and the platform targeted by the module.</summary>
	/// <param name="peKind">When this method returns, a combination of the <see cref="T:System.Reflection.PortableExecutableKinds" /> values indicating the nature of the code in the module.</param>
	/// <param name="machine">When this method returns, one of the <see cref="T:System.Reflection.ImageFileMachine" /> values indicating the platform targeted by the module.</param>
	public virtual void GetPEKind(out PortableExecutableKinds peKind, out ImageFileMachine machine)
	{
		throw CreateNIE();
	}

	/// <summary>Returns the specified type, specifying whether to make a case-sensitive search of the module and whether to throw an exception if the type cannot be found.</summary>
	/// <returns>A <see cref="T:System.Type" /> object representing the specified type, if the type is declared in this module; otherwise, null.</returns>
	/// <param name="className">The name of the type to locate. The name must be fully qualified with the namespace. </param>
	/// <param name="throwOnError">true to throw an exception if the type cannot be found; false to return null. </param>
	/// <param name="ignoreCase">true for case-insensitive search; otherwise, false. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="className" /> is null. </exception>
	/// <exception cref="T:System.Reflection.TargetInvocationException">The class initializers are invoked and an exception is thrown. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="className" /> is a zero-length string. </exception>
	/// <exception cref="T:System.TypeLoadException">
	///   <paramref name="throwOnError" /> is true, and the type cannot be found. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="className" /> requires a dependent assembly that could not be found. </exception>
	/// <exception cref="T:System.IO.FileLoadException">
	///   <paramref name="className" /> requires a dependent assembly that was found but could not be loaded.-or-The current assembly was loaded into the reflection-only context, and <paramref name="className" /> requires a dependent assembly that was not preloaded. </exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="className" /> requires a dependent assembly, but the file is not a valid assembly. -or-<paramref name="className" /> requires a dependent assembly which was compiled for a version of the runtime later than the currently loaded version.</exception>
	[ComVisible(true)]
	public virtual Type GetType(string className, bool throwOnError, bool ignoreCase)
	{
		throw CreateNIE();
	}

	/// <summary>Returns a value that indicates whether the specified attribute type has been applied to this module.</summary>
	/// <returns>true if one or more instances of <paramref name="attributeType" /> have been applied to this module; otherwise, false.</returns>
	/// <param name="attributeType">The type of custom attribute to test for. </param>
	/// <param name="inherit">This argument is ignored for objects of this type. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="attributeType" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="attributeType" /> is not a <see cref="T:System.Type" /> object supplied by the runtime. For example, <paramref name="attributeType" /> is a <see cref="T:System.Reflection.Emit.TypeBuilder" /> object.</exception>
	public virtual bool IsDefined(Type attributeType, bool inherit)
	{
		throw CreateNIE();
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
	public virtual FieldInfo ResolveField(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
	{
		throw CreateNIE();
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
	public virtual MemberInfo ResolveMember(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
	{
		throw CreateNIE();
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
	public virtual MethodBase ResolveMethod(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
	{
		throw CreateNIE();
	}

	/// <summary>Returns the signature blob identified by a metadata token.</summary>
	/// <returns>An array of bytes representing the signature blob.</returns>
	/// <param name="metadataToken">A metadata token that identifies a signature in the module.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="metadataToken" /> is not a valid MemberRef, MethodDef, TypeSpec, signature, or FieldDef token in the scope of the current module.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="metadataToken" /> is not a valid token in the scope of the current module.</exception>
	public virtual byte[] ResolveSignature(int metadataToken)
	{
		throw CreateNIE();
	}

	/// <summary>Returns the string identified by the specified metadata token.</summary>
	/// <returns>A <see cref="T:System.String" /> containing a string value from the metadata string heap.</returns>
	/// <param name="metadataToken">A metadata token that identifies a string in the string heap of the module. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="metadataToken" /> is not a token for a string in the scope of the current module. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="metadataToken" /> is not a valid token in the scope of the current module.</exception>
	public virtual string ResolveString(int metadataToken)
	{
		throw CreateNIE();
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
	public virtual Type ResolveType(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
	{
		throw CreateNIE();
	}

	/// <summary>Returns an X509Certificate object corresponding to the certificate included in the Authenticode signature of the assembly which this module belongs to. If the assembly has not been Authenticode signed, null is returned.</summary>
	/// <returns>An X509Certificate object, or null if the assembly to which this module belongs has not been Authenticode signed.</returns>
	public virtual X509Certificate GetSignerCertificate()
	{
		throw CreateNIE();
	}

	/// <summary>Returns all the types defined within this module.</summary>
	/// <returns>An array of type Type containing types defined within the module that is reflected by this instance.</returns>
	/// <exception cref="T:System.Reflection.ReflectionTypeLoadException">One or more classes in a module could not be loaded. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	public virtual Type[] GetTypes()
	{
		throw CreateNIE();
	}
}
