using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using Unity;

namespace System.Reflection.Emit;

/// <summary>Defines and creates new instances of classes during run time.</summary>
[StructLayout(LayoutKind.Sequential)]
[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
[ComDefaultInterface(typeof(_TypeBuilder))]
public sealed class TypeBuilder : TypeInfo, _TypeBuilder
{
	private string tname;

	private string nspace;

	private Type parent;

	private Type nesting_type;

	internal Type[] interfaces;

	internal int num_methods;

	internal MethodBuilder[] methods;

	internal ConstructorBuilder[] ctors;

	internal PropertyBuilder[] properties;

	internal int num_fields;

	internal FieldBuilder[] fields;

	internal EventBuilder[] events;

	private CustomAttributeBuilder[] cattrs;

	internal TypeBuilder[] subtypes;

	internal TypeAttributes attrs;

	private int table_idx;

	private ModuleBuilder pmodule;

	private int class_size;

	private PackingSize packing_size;

	private IntPtr generic_container;

	private GenericTypeParameterBuilder[] generic_params;

	private RefEmitPermissionSet[] permissions;

	private TypeInfo created;

	private int state;

	private TypeName fullname;

	private bool createTypeCalled;

	private Type underlying_type;

	/// <summary>Represents that total size for the type is not specified.</summary>
	public const int UnspecifiedTypeSize = 0;

	/// <summary>Retrieves the dynamic assembly that contains this type definition.</summary>
	/// <returns>Read-only. Retrieves the dynamic assembly that contains this type definition.</returns>
	public override Assembly Assembly => pmodule.Assembly;

	/// <summary>Returns the full name of this type qualified by the display name of the assembly.</summary>
	/// <returns>Read-only. The full name of this type qualified by the display name of the assembly.</returns>
	public override string AssemblyQualifiedName => fullname.DisplayName + ", " + Assembly.FullName;

	/// <summary>Retrieves the base type of this type.</summary>
	/// <returns>Read-only. Retrieves the base type of this type.</returns>
	public override Type BaseType => parent;

	/// <summary>Returns the type that declared this type.</summary>
	/// <returns>Read-only. The type that declared this type.</returns>
	public override Type DeclaringType => nesting_type;

	/// <summary>Returns the underlying system type for this TypeBuilder.</summary>
	/// <returns>Read-only. Returns the underlying system type.</returns>
	/// <exception cref="T:System.InvalidOperationException">This type is an enumeration, but there is no underlying system type. </exception>
	public override Type UnderlyingSystemType
	{
		get
		{
			if (is_created)
			{
				return created.UnderlyingSystemType;
			}
			if (IsEnum)
			{
				if (underlying_type != null)
				{
					return underlying_type;
				}
				throw new InvalidOperationException("Enumeration type is not defined.");
			}
			return this;
		}
	}

	/// <summary>Retrieves the full path of this type.</summary>
	/// <returns>Read-only. Retrieves the full path of this type.</returns>
	public override string FullName => fullname.DisplayName;

	/// <summary>Retrieves the GUID of this type.</summary>
	/// <returns>Read-only. Retrieves the GUID of this type </returns>
	/// <exception cref="T:System.NotSupportedException">This method is not currently supported for incomplete types. </exception>
	public override Guid GUID
	{
		get
		{
			check_created();
			return created.GUID;
		}
	}

	/// <summary>Retrieves the dynamic module that contains this type definition.</summary>
	/// <returns>Read-only. Retrieves the dynamic module that contains this type definition.</returns>
	public override Module Module => pmodule;

	/// <summary>Retrieves the name of this type.</summary>
	/// <returns>Read-only. Retrieves the <see cref="T:System.String" /> name of this type.</returns>
	public override string Name => tname;

	/// <summary>Retrieves the namespace where this TypeBuilder is defined.</summary>
	/// <returns>Read-only. Retrieves the namespace where this TypeBuilder is defined.</returns>
	public override string Namespace => nspace;

	/// <summary>Retrieves the packing size of this type.</summary>
	/// <returns>Read-only. Retrieves the packing size of this type.</returns>
	public PackingSize PackingSize => packing_size;

	/// <summary>Retrieves the total size of a type.</summary>
	/// <returns>Read-only. Retrieves this type’s total size.</returns>
	public int Size => class_size;

	/// <summary>Returns the type that was used to obtain this type.</summary>
	/// <returns>Read-only. The type that was used to obtain this type.</returns>
	public override Type ReflectedType => nesting_type;

	/// <summary>Not supported in dynamic modules.</summary>
	/// <returns>Read-only.</returns>
	/// <exception cref="T:System.NotSupportedException">Not supported in dynamic modules. </exception>
	public override RuntimeTypeHandle TypeHandle
	{
		get
		{
			check_created();
			return created.TypeHandle;
		}
	}

	/// <summary>Returns the type token of this type.</summary>
	/// <returns>Read-only. Returns the TypeToken of this type.</returns>
	/// <exception cref="T:System.InvalidOperationException">The type was previously created using <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" />.</exception>
	public TypeToken TypeToken => new TypeToken(0x2000000 | table_idx);

	internal bool is_created => createTypeCalled;

	public override bool ContainsGenericParameters => generic_params != null;

	/// <summary>Gets a value indicating whether the current type is a generic type parameter.</summary>
	/// <returns>true if the current <see cref="T:System.Reflection.Emit.TypeBuilder" /> object represents a generic type parameter; otherwise, false.</returns>
	public override bool IsGenericParameter => false;

	/// <summary>Gets a value that indicates the covariance and special constraints of the current generic type parameter. </summary>
	/// <returns>A bitwise combination of <see cref="T:System.Reflection.GenericParameterAttributes" /> values that describes the covariance and special constraints of the current generic type parameter.</returns>
	public override GenericParameterAttributes GenericParameterAttributes => GenericParameterAttributes.None;

	/// <summary>Gets a value indicating whether the current <see cref="T:System.Reflection.Emit.TypeBuilder" /> represents a generic type definition from which other generic types can be constructed.</summary>
	/// <returns>true if this <see cref="T:System.Reflection.Emit.TypeBuilder" /> object represents a generic type definition; otherwise, false.</returns>
	public override bool IsGenericTypeDefinition => generic_params != null;

	/// <summary>Gets a value indicating whether the current type is a generic type. </summary>
	/// <returns>true if the type represented by the current <see cref="T:System.Reflection.Emit.TypeBuilder" /> object is generic; otherwise, false.</returns>
	public override bool IsGenericType => IsGenericTypeDefinition;

	/// <summary>Gets the position of a type parameter in the type parameter list of the generic type that declared the parameter.</summary>
	/// <returns>If the current <see cref="T:System.Reflection.Emit.TypeBuilder" /> object represents a generic type parameter, the position of the type parameter in the type parameter list of the generic type that declared the parameter; otherwise, undefined.</returns>
	[MonoTODO]
	public override int GenericParameterPosition => 0;

	/// <summary>Gets the method that declared the current generic type parameter.</summary>
	/// <returns>A <see cref="T:System.Reflection.MethodBase" /> that represents the method that declared the current type, if the current type is a generic type parameter; otherwise, null.</returns>
	public override MethodBase DeclaringMethod => null;

	internal override bool IsUserType => false;

	/// <summary>Gets a value that indicates whether this object represents a constructed generic type.</summary>
	/// <returns>true if this object represents a constructed generic type; otherwise, false.</returns>
	public override bool IsConstructedGenericType => false;

	protected override TypeAttributes GetAttributeFlagsImpl()
	{
		return attrs;
	}

	internal TypeBuilder(ModuleBuilder mb, TypeAttributes attr, int table_idx)
	{
		parent = null;
		attrs = attr;
		class_size = 0;
		this.table_idx = table_idx;
		tname = ((table_idx == 1) ? "<Module>" : ("type_" + table_idx));
		nspace = string.Empty;
		fullname = TypeIdentifiers.WithoutEscape(tname);
		pmodule = mb;
	}

	internal TypeBuilder(ModuleBuilder mb, string name, TypeAttributes attr, Type parent, Type[] interfaces, PackingSize packing_size, int type_size, Type nesting_type)
	{
		this.parent = ResolveUserType(parent);
		attrs = attr;
		class_size = type_size;
		this.packing_size = packing_size;
		this.nesting_type = nesting_type;
		check_name("fullname", name);
		if (parent == null && (attr & TypeAttributes.ClassSemanticsMask) != 0 && (attr & TypeAttributes.Abstract) == 0)
		{
			throw new InvalidOperationException("Interface must be declared abstract.");
		}
		int num = name.LastIndexOf('.');
		if (num != -1)
		{
			tname = name.Substring(num + 1);
			nspace = name.Substring(0, num);
		}
		else
		{
			tname = name;
			nspace = string.Empty;
		}
		if (interfaces != null)
		{
			this.interfaces = new Type[interfaces.Length];
			Array.Copy(interfaces, this.interfaces, interfaces.Length);
		}
		pmodule = mb;
		if ((attr & TypeAttributes.ClassSemanticsMask) == 0 && parent == null)
		{
			this.parent = typeof(object);
		}
		table_idx = mb.get_next_table_index(this, 2, inc: true);
		fullname = GetFullName();
	}

	/// <summary>Determines whether this type is derived from a specified type.</summary>
	/// <returns>Read-only. Returns true if this type is the same as the type <paramref name="c" />, or is a subtype of type <paramref name="c" />; otherwise, false.</returns>
	/// <param name="c">A <see cref="T:System.Type" /> that is to be checked. </param>
	[ComVisible(true)]
	public override bool IsSubclassOf(Type c)
	{
		if (c == null)
		{
			return false;
		}
		if (c == this)
		{
			return false;
		}
		Type baseType = parent;
		while (baseType != null)
		{
			if (c == baseType)
			{
				return true;
			}
			baseType = baseType.BaseType;
		}
		return false;
	}

	private TypeName GetFullName()
	{
		TypeIdentifier typeIdentifier = TypeIdentifiers.FromInternal(tname);
		if (nesting_type != null)
		{
			return TypeNames.FromDisplay(nesting_type.FullName).NestedName(typeIdentifier);
		}
		if (nspace != null && nspace.Length > 0)
		{
			return TypeIdentifiers.FromInternal(nspace, typeIdentifier);
		}
		return typeIdentifier;
	}

	/// <summary>Adds declarative security to this type.</summary>
	/// <param name="action">The security action to be taken such as Demand, Assert, and so on. </param>
	/// <param name="pset">The set of permissions the action applies to. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="action" /> is invalid (RequestMinimum, RequestOptional, and RequestRefuse are invalid). </exception>
	/// <exception cref="T:System.InvalidOperationException">The containing type has been created using <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" />.-or- The permission set <paramref name="pset" /> contains an action that was added earlier by AddDeclarativeSecurity. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pset" /> is null. </exception>
	public void AddDeclarativeSecurity(SecurityAction action, PermissionSet pset)
	{
		if (pset == null)
		{
			throw new ArgumentNullException("pset");
		}
		if (action == SecurityAction.RequestMinimum || action == SecurityAction.RequestOptional || action == SecurityAction.RequestRefuse)
		{
			throw new ArgumentOutOfRangeException("Request* values are not permitted", "action");
		}
		check_not_created();
		if (permissions != null)
		{
			RefEmitPermissionSet[] array = permissions;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].action == action)
				{
					throw new InvalidOperationException("Multiple permission sets specified with the same SecurityAction.");
				}
			}
			RefEmitPermissionSet[] array2 = new RefEmitPermissionSet[permissions.Length + 1];
			permissions.CopyTo(array2, 0);
			permissions = array2;
		}
		else
		{
			permissions = new RefEmitPermissionSet[1];
		}
		permissions[permissions.Length - 1] = new RefEmitPermissionSet(action, pset.ToXml().ToString());
		attrs |= TypeAttributes.HasSecurity;
	}

	/// <summary>Adds an interface that this type implements.</summary>
	/// <param name="interfaceType">The interface that this type implements. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="interfaceType" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The type was previously created using <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" />. </exception>
	[ComVisible(true)]
	public void AddInterfaceImplementation(Type interfaceType)
	{
		if (interfaceType == null)
		{
			throw new ArgumentNullException("interfaceType");
		}
		check_not_created();
		if (interfaces != null)
		{
			Type[] array = interfaces;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == interfaceType)
				{
					return;
				}
			}
			Type[] array2 = new Type[interfaces.Length + 1];
			interfaces.CopyTo(array2, 0);
			array2[interfaces.Length] = interfaceType;
			interfaces = array2;
		}
		else
		{
			interfaces = new Type[1];
			interfaces[0] = interfaceType;
		}
	}

	protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
	{
		check_created();
		if (created == typeof(object))
		{
			if (ctors == null)
			{
				return null;
			}
			ConstructorBuilder constructorBuilder = null;
			int num = 0;
			ConstructorBuilder[] array = ctors;
			foreach (ConstructorBuilder constructorBuilder2 in array)
			{
				if (callConvention == CallingConventions.Any || constructorBuilder2.CallingConvention == callConvention)
				{
					constructorBuilder = constructorBuilder2;
					num++;
				}
			}
			if (num == 0)
			{
				return null;
			}
			if (types == null)
			{
				if (num > 1)
				{
					throw new AmbiguousMatchException();
				}
				return constructorBuilder;
			}
			MethodBase[] array2 = new MethodBase[num];
			if (num == 1)
			{
				array2[0] = constructorBuilder;
			}
			else
			{
				num = 0;
				array = ctors;
				foreach (ConstructorInfo constructorInfo in array)
				{
					if (callConvention == CallingConventions.Any || constructorInfo.CallingConvention == callConvention)
					{
						array2[num++] = constructorInfo;
					}
				}
			}
			if (binder == null)
			{
				binder = Type.DefaultBinder;
			}
			return (ConstructorInfo)binder.SelectMethod(bindingAttr, array2, types, modifiers);
		}
		return created.GetConstructor(bindingAttr, binder, callConvention, types, modifiers);
	}

	/// <summary>Determines whether a custom attribute is applied to the current type.</summary>
	/// <returns>true if one or more instances of <paramref name="attributeType" />, or an attribute derived from <paramref name="attributeType" />, is defined on this type; otherwise, false.</returns>
	/// <param name="attributeType">The type of attribute to search for. Only attributes that are assignable to this type are returned. </param>
	/// <param name="inherit">Specifies whether to search this member's inheritance chain to find the attributes. </param>
	/// <exception cref="T:System.NotSupportedException">This method is not currently supported for incomplete types. Retrieve the type using <see cref="M:System.Type.GetType" /> and call <see cref="M:System.Reflection.MemberInfo.IsDefined(System.Type,System.Boolean)" /> on the returned <see cref="T:System.Type" />. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="attributeType" /> is not defined.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="attributeType" /> is null.</exception>
	[SecuritySafeCritical]
	public override bool IsDefined(Type attributeType, bool inherit)
	{
		if (!is_created)
		{
			throw new NotSupportedException();
		}
		return MonoCustomAttrs.IsDefined(this, attributeType, inherit);
	}

	/// <summary>Returns all the custom attributes defined for this type.</summary>
	/// <returns>Returns an array of objects representing all the custom attributes of this type.</returns>
	/// <param name="inherit">Specifies whether to search this member's inheritance chain to find the attributes. </param>
	/// <exception cref="T:System.NotSupportedException">This method is not currently supported for incomplete types. Retrieve the type using <see cref="M:System.Type.GetType" /> and call <see cref="M:System.Reflection.MemberInfo.GetCustomAttributes(System.Boolean)" /> on the returned <see cref="T:System.Type" />. </exception>
	[SecuritySafeCritical]
	public override object[] GetCustomAttributes(bool inherit)
	{
		check_created();
		return created.GetCustomAttributes(inherit);
	}

	/// <summary>Returns all the custom attributes of the current type that are assignable to a specified type.</summary>
	/// <returns>An array of custom attributes defined on the current type.</returns>
	/// <param name="attributeType">The type of attribute to search for. Only attributes that are assignable to this type are returned.</param>
	/// <param name="inherit">Specifies whether to search this member's inheritance chain to find the attributes. </param>
	/// <exception cref="T:System.NotSupportedException">This method is not currently supported for incomplete types. Retrieve the type using <see cref="M:System.Type.GetType" /> and call <see cref="M:System.Reflection.MemberInfo.GetCustomAttributes(System.Boolean)" /> on the returned <see cref="T:System.Type" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="attributeType" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">The type must be a type provided by the underlying runtime system.</exception>
	[SecuritySafeCritical]
	public override object[] GetCustomAttributes(Type attributeType, bool inherit)
	{
		check_created();
		return created.GetCustomAttributes(attributeType, inherit);
	}

	/// <summary>Defines a nested type, given its name.</summary>
	/// <returns>The defined nested type.</returns>
	/// <param name="name">The short name of the type. <paramref name="name" /> cannot contain embedded nulls. </param>
	/// <exception cref="T:System.ArgumentException">Length of <paramref name="name" /> is zero or greater than 1023. -or-This operation would create a type with a duplicate <see cref="P:System.Reflection.Emit.TypeBuilder.FullName" /> in the current assembly.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	public TypeBuilder DefineNestedType(string name)
	{
		return DefineNestedType(name, TypeAttributes.NestedPrivate, pmodule.assemblyb.corlib_object_type, null);
	}

	/// <summary>Defines a nested type, given its name and attributes.</summary>
	/// <returns>The defined nested type.</returns>
	/// <param name="name">The short name of the type. <paramref name="name" /> cannot contain embedded nulls. </param>
	/// <param name="attr">The attributes of the type. </param>
	/// <exception cref="T:System.ArgumentException">The nested attribute is not specified.-or- This type is sealed.-or- This type is an array.-or- This type is an interface, but the nested type is not an interface.-or- The length of <paramref name="name" /> is zero or greater than 1023. -or-This operation would create a type with a duplicate <see cref="P:System.Reflection.Emit.TypeBuilder.FullName" /> in the current assembly.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	public TypeBuilder DefineNestedType(string name, TypeAttributes attr)
	{
		return DefineNestedType(name, attr, pmodule.assemblyb.corlib_object_type, null);
	}

	/// <summary>Defines a nested type, given its name, attributes, and the type that it extends.</summary>
	/// <returns>The defined nested type.</returns>
	/// <param name="name">The short name of the type. <paramref name="name" /> cannot contain embedded nulls. </param>
	/// <param name="attr">The attributes of the type. </param>
	/// <param name="parent">The type that the nested type extends. </param>
	/// <exception cref="T:System.ArgumentException">The nested attribute is not specified.-or- This type is sealed.-or- This type is an array.-or- This type is an interface, but the nested type is not an interface.-or- The length of <paramref name="name" /> is zero or greater than 1023. -or-This operation would create a type with a duplicate <see cref="P:System.Reflection.Emit.TypeBuilder.FullName" /> in the current assembly.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	public TypeBuilder DefineNestedType(string name, TypeAttributes attr, Type parent)
	{
		return DefineNestedType(name, attr, parent, null);
	}

	private TypeBuilder DefineNestedType(string name, TypeAttributes attr, Type parent, Type[] interfaces, PackingSize packSize, int typeSize)
	{
		if (interfaces != null)
		{
			for (int i = 0; i < interfaces.Length; i++)
			{
				if (interfaces[i] == null)
				{
					throw new ArgumentNullException("interfaces");
				}
			}
		}
		TypeBuilder typeBuilder = new TypeBuilder(pmodule, name, attr, parent, interfaces, packSize, typeSize, this);
		typeBuilder.fullname = typeBuilder.GetFullName();
		pmodule.RegisterTypeName(typeBuilder, typeBuilder.fullname);
		if (subtypes != null)
		{
			TypeBuilder[] array = new TypeBuilder[subtypes.Length + 1];
			Array.Copy(subtypes, array, subtypes.Length);
			array[subtypes.Length] = typeBuilder;
			subtypes = array;
		}
		else
		{
			subtypes = new TypeBuilder[1];
			subtypes[0] = typeBuilder;
		}
		return typeBuilder;
	}

	/// <summary>Defines a nested type, given its name, attributes, the type that it extends, and the interfaces that it implements.</summary>
	/// <returns>The defined nested type.</returns>
	/// <param name="name">The short name of the type. <paramref name="name" /> cannot contain embedded nulls. </param>
	/// <param name="attr">The attributes of the type. </param>
	/// <param name="parent">The type that the nested type extends. </param>
	/// <param name="interfaces">The interfaces that the nested type implements. </param>
	/// <exception cref="T:System.ArgumentException">The nested attribute is not specified.-or- This type is sealed.-or- This type is an array.-or- This type is an interface, but the nested type is not an interface.-or- The length of <paramref name="name" /> is zero or greater than 1023. -or-This operation would create a type with a duplicate <see cref="P:System.Reflection.Emit.TypeBuilder.FullName" /> in the current assembly.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null.-or-An element of the <paramref name="interfaces" /> array is null.</exception>
	[ComVisible(true)]
	public TypeBuilder DefineNestedType(string name, TypeAttributes attr, Type parent, Type[] interfaces)
	{
		return DefineNestedType(name, attr, parent, interfaces, PackingSize.Unspecified, 0);
	}

	/// <summary>Defines a nested type, given its name, attributes, the total size of the type, and the type that it extends.</summary>
	/// <returns>The defined nested type.</returns>
	/// <param name="name">The short name of the type. <paramref name="name" /> cannot contain embedded nulls. </param>
	/// <param name="attr">The attributes of the type. </param>
	/// <param name="parent">The type that the nested type extends. </param>
	/// <param name="typeSize">The total size of the type. </param>
	/// <exception cref="T:System.ArgumentException">The nested attribute is not specified.-or- This type is sealed.-or- This type is an array.-or- This type is an interface, but the nested type is not an interface.-or- The length of <paramref name="name" /> is zero or greater than 1023. -or-This operation would create a type with a duplicate <see cref="P:System.Reflection.Emit.TypeBuilder.FullName" /> in the current assembly.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	public TypeBuilder DefineNestedType(string name, TypeAttributes attr, Type parent, int typeSize)
	{
		return DefineNestedType(name, attr, parent, null, PackingSize.Unspecified, typeSize);
	}

	/// <summary>Defines a nested type, given its name, attributes, the type that it extends, and the packing size.</summary>
	/// <returns>The defined nested type.</returns>
	/// <param name="name">The short name of the type. <paramref name="name" /> cannot contain embedded nulls. </param>
	/// <param name="attr">The attributes of the type. </param>
	/// <param name="parent">The type that the nested type extends. </param>
	/// <param name="packSize">The packing size of the type. </param>
	/// <exception cref="T:System.ArgumentException">The nested attribute is not specified.-or- This type is sealed.-or- This type is an array.-or- This type is an interface, but the nested type is not an interface.-or- The length of <paramref name="name" /> is zero or greater than 1023. -or-This operation would create a type with a duplicate <see cref="P:System.Reflection.Emit.TypeBuilder.FullName" /> in the current assembly.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	public TypeBuilder DefineNestedType(string name, TypeAttributes attr, Type parent, PackingSize packSize)
	{
		return DefineNestedType(name, attr, parent, null, packSize, 0);
	}

	/// <summary>Defines a nested type, given its name, attributes, size, and the type that it extends.</summary>
	/// <returns>The defined nested type.</returns>
	/// <param name="name">The short name of the type. <paramref name="name" /> cannot contain embedded null values.</param>
	/// <param name="attr">The attributes of the type.</param>
	/// <param name="parent">The type that the nested type extends.</param>
	/// <param name="packSize">The packing size of the type.</param>
	/// <param name="typeSize">The total size of the type.</param>
	public TypeBuilder DefineNestedType(string name, TypeAttributes attr, Type parent, PackingSize packSize, int typeSize)
	{
		return DefineNestedType(name, attr, parent, null, packSize, typeSize);
	}

	/// <summary>Adds a new constructor to the type, with the given attributes and signature.</summary>
	/// <returns>The defined constructor.</returns>
	/// <param name="attributes">The attributes of the constructor. </param>
	/// <param name="callingConvention">The calling convention of the constructor. </param>
	/// <param name="parameterTypes">The parameter types of the constructor. </param>
	/// <exception cref="T:System.InvalidOperationException">The type was previously created using <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" />. </exception>
	[ComVisible(true)]
	public ConstructorBuilder DefineConstructor(MethodAttributes attributes, CallingConventions callingConvention, Type[] parameterTypes)
	{
		return DefineConstructor(attributes, callingConvention, parameterTypes, null, null);
	}

	/// <summary>Adds a new constructor to the type, with the given attributes, signature, and custom modifiers.</summary>
	/// <returns>The defined constructor.</returns>
	/// <param name="attributes">The attributes of the constructor. </param>
	/// <param name="callingConvention">The calling convention of the constructor. </param>
	/// <param name="parameterTypes">The parameter types of the constructor. </param>
	/// <param name="requiredCustomModifiers">An array of arrays of types. Each array of types represents the required custom modifiers for the corresponding parameter, such as <see cref="T:System.Runtime.CompilerServices.IsConst" />. If a particular parameter has no required custom modifiers, specify null instead of an array of types. If none of the parameters have required custom modifiers, specify null instead of an array of arrays.</param>
	/// <param name="optionalCustomModifiers">An array of arrays of types. Each array of types represents the optional custom modifiers for the corresponding parameter, such as <see cref="T:System.Runtime.CompilerServices.IsConst" />. If a particular parameter has no optional custom modifiers, specify null instead of an array of types. If none of the parameters have optional custom modifiers, specify null instead of an array of arrays.</param>
	/// <exception cref="T:System.ArgumentException">The size of <paramref name="requiredCustomModifiers" /> or <paramref name="optionalCustomModifiers" /> does not equal the size of <paramref name="parameterTypes" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">The type was previously created using <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" />.-or-For the current dynamic type, the <see cref="P:System.Reflection.Emit.TypeBuilder.IsGenericType" /> property is true, but the <see cref="P:System.Reflection.Emit.TypeBuilder.IsGenericTypeDefinition" /> property is false.</exception>
	[ComVisible(true)]
	public ConstructorBuilder DefineConstructor(MethodAttributes attributes, CallingConventions callingConvention, Type[] parameterTypes, Type[][] requiredCustomModifiers, Type[][] optionalCustomModifiers)
	{
		check_not_created();
		ConstructorBuilder constructorBuilder = new ConstructorBuilder(this, attributes, callingConvention, parameterTypes, requiredCustomModifiers, optionalCustomModifiers);
		if (ctors != null)
		{
			ConstructorBuilder[] array = new ConstructorBuilder[ctors.Length + 1];
			Array.Copy(ctors, array, ctors.Length);
			array[ctors.Length] = constructorBuilder;
			ctors = array;
		}
		else
		{
			ctors = new ConstructorBuilder[1];
			ctors[0] = constructorBuilder;
		}
		return constructorBuilder;
	}

	/// <summary>Defines the default constructor. The constructor defined here will simply call the default constructor of the parent.</summary>
	/// <returns>Returns the constructor.</returns>
	/// <param name="attributes">A MethodAttributes object representing the attributes to be applied to the constructor. </param>
	/// <exception cref="T:System.NotSupportedException">The parent type (base type) does not have a default constructor. </exception>
	/// <exception cref="T:System.InvalidOperationException">The type was previously created using <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" />.-or-For the current dynamic type, the <see cref="P:System.Reflection.Emit.TypeBuilder.IsGenericType" /> property is true, but the <see cref="P:System.Reflection.Emit.TypeBuilder.IsGenericTypeDefinition" /> property is false.</exception>
	[ComVisible(true)]
	public ConstructorBuilder DefineDefaultConstructor(MethodAttributes attributes)
	{
		Type type = ((!(parent != null)) ? pmodule.assemblyb.corlib_object_type : parent);
		Type type2 = type;
		type = type.InternalResolve();
		if (type == typeof(object) || type == typeof(ValueType))
		{
			type = type2;
		}
		ConstructorInfo constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
		if (constructor == null)
		{
			throw new NotSupportedException("Parent does not have a default constructor. The default constructor must be explicitly defined.");
		}
		ConstructorBuilder constructorBuilder = DefineConstructor(attributes, CallingConventions.Standard, Type.EmptyTypes);
		ILGenerator iLGenerator = constructorBuilder.GetILGenerator();
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Call, constructor);
		iLGenerator.Emit(OpCodes.Ret);
		return constructorBuilder;
	}

	private void append_method(MethodBuilder mb)
	{
		if (methods != null)
		{
			if (methods.Length == num_methods)
			{
				MethodBuilder[] destinationArray = new MethodBuilder[methods.Length * 2];
				Array.Copy(methods, destinationArray, num_methods);
				methods = destinationArray;
			}
		}
		else
		{
			methods = new MethodBuilder[1];
		}
		methods[num_methods] = mb;
		num_methods++;
	}

	/// <summary>Adds a new method to the type, with the specified name, method attributes, and method signature.</summary>
	/// <returns>The defined method.</returns>
	/// <param name="name">The name of the method. <paramref name="name" /> cannot contain embedded nulls. </param>
	/// <param name="attributes">The attributes of the method. </param>
	/// <param name="returnType">The return type of the method. </param>
	/// <param name="parameterTypes">The types of the parameters of the method. </param>
	/// <exception cref="T:System.ArgumentException">The length of <paramref name="name" /> is zero.-or- The type of the parent of this method is an interface, and this method is not virtual (Overridable in Visual Basic). </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The type was previously created using <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" />.-or-For the current dynamic type, the <see cref="P:System.Reflection.Emit.TypeBuilder.IsGenericType" /> property is true, but the <see cref="P:System.Reflection.Emit.TypeBuilder.IsGenericTypeDefinition" /> property is false. </exception>
	public MethodBuilder DefineMethod(string name, MethodAttributes attributes, Type returnType, Type[] parameterTypes)
	{
		return DefineMethod(name, attributes, CallingConventions.Standard, returnType, parameterTypes);
	}

	/// <summary>Adds a new method to the type, with the specified name, method attributes, calling convention, and method signature.</summary>
	/// <returns>A <see cref="T:System.Reflection.Emit.MethodBuilder" /> representing the newly defined method.</returns>
	/// <param name="name">The name of the method. <paramref name="name" /> cannot contain embedded nulls. </param>
	/// <param name="attributes">The attributes of the method. </param>
	/// <param name="callingConvention">The calling convention of the method. </param>
	/// <param name="returnType">The return type of the method. </param>
	/// <param name="parameterTypes">The types of the parameters of the method. </param>
	/// <exception cref="T:System.ArgumentException">The length of <paramref name="name" /> is zero.-or- The type of the parent of this method is an interface, and this method is not virtual (Overridable in Visual Basic). </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The type was previously created using <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" />.-or-For the current dynamic type, the <see cref="P:System.Reflection.Emit.TypeBuilder.IsGenericType" /> property is true, but the <see cref="P:System.Reflection.Emit.TypeBuilder.IsGenericTypeDefinition" /> property is false. </exception>
	public MethodBuilder DefineMethod(string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes)
	{
		return DefineMethod(name, attributes, callingConvention, returnType, null, null, parameterTypes, null, null);
	}

	/// <summary>Adds a new method to the type, with the specified name, method attributes, calling convention, method signature, and custom modifiers.</summary>
	/// <returns>A <see cref="T:System.Reflection.Emit.MethodBuilder" /> object representing the newly added method.</returns>
	/// <param name="name">The name of the method. <paramref name="name" /> cannot contain embedded nulls. </param>
	/// <param name="attributes">The attributes of the method. </param>
	/// <param name="callingConvention">The calling convention of the method. </param>
	/// <param name="returnType">The return type of the method. </param>
	/// <param name="returnTypeRequiredCustomModifiers">An array of types representing the required custom modifiers, such as <see cref="T:System.Runtime.CompilerServices.IsConst" />, for the return type of the method. If the return type has no required custom modifiers, specify null.</param>
	/// <param name="returnTypeOptionalCustomModifiers">An array of types representing the optional custom modifiers, such as <see cref="T:System.Runtime.CompilerServices.IsConst" />, for the return type of the method. If the return type has no optional custom modifiers, specify null.</param>
	/// <param name="parameterTypes">The types of the parameters of the method.</param>
	/// <param name="parameterTypeRequiredCustomModifiers">An array of arrays of types. Each array of types represents the required custom modifiers for the corresponding parameter, such as <see cref="T:System.Runtime.CompilerServices.IsConst" />. If a particular parameter has no required custom modifiers, specify null instead of an array of types. If none of the parameters have required custom modifiers, specify null instead of an array of arrays.</param>
	/// <param name="parameterTypeOptionalCustomModifiers">An array of arrays of types. Each array of types represents the optional custom modifiers for the corresponding parameter, such as <see cref="T:System.Runtime.CompilerServices.IsConst" />. If a particular parameter has no optional custom modifiers, specify null instead of an array of types. If none of the parameters have optional custom modifiers, specify null instead of an array of arrays.</param>
	/// <exception cref="T:System.ArgumentException">The length of <paramref name="name" /> is zero.-or- The type of the parent of this method is an interface, and this method is not virtual (Overridable in Visual Basic). -or-The size of <paramref name="parameterTypeRequiredCustomModifiers" /> or <paramref name="parameterTypeOptionalCustomModifiers" /> does not equal the size of <paramref name="parameterTypes" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The type was previously created using <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" />.-or-For the current dynamic type, the <see cref="P:System.Reflection.Emit.TypeBuilder.IsGenericType" /> property is true, but the <see cref="P:System.Reflection.Emit.TypeBuilder.IsGenericTypeDefinition" /> property is false.</exception>
	public MethodBuilder DefineMethod(string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers, Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers)
	{
		check_name("name", name);
		check_not_created();
		if (base.IsInterface && ((attributes & MethodAttributes.Abstract) == 0 || (attributes & MethodAttributes.Virtual) == 0) && (attributes & MethodAttributes.Static) == 0)
		{
			throw new ArgumentException("Interface method must be abstract and virtual.");
		}
		if (returnType == null)
		{
			returnType = pmodule.assemblyb.corlib_void_type;
		}
		MethodBuilder methodBuilder = new MethodBuilder(this, name, attributes, callingConvention, returnType, returnTypeRequiredCustomModifiers, returnTypeOptionalCustomModifiers, parameterTypes, parameterTypeRequiredCustomModifiers, parameterTypeOptionalCustomModifiers);
		append_method(methodBuilder);
		return methodBuilder;
	}

	/// <summary>Defines a PInvoke method given its name, the name of the DLL in which the method is defined, the name of the entry point, the attributes of the method, the calling convention of the method, the return type of the method, the types of the parameters of the method, and the PInvoke flags.</summary>
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
	/// <exception cref="T:System.ArgumentException">The method is not static.-or- The parent type is an interface.-or- The method is abstract.-or- The method was previously defined.-or- The length of <paramref name="name" />, <paramref name="dllName" />, or <paramref name="entryName" /> is zero. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" />, <paramref name="dllName" />, or <paramref name="entryName" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The containing type has been previously created using <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" />. </exception>
	public MethodBuilder DefinePInvokeMethod(string name, string dllName, string entryName, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, CallingConvention nativeCallConv, CharSet nativeCharSet)
	{
		return DefinePInvokeMethod(name, dllName, entryName, attributes, callingConvention, returnType, null, null, parameterTypes, null, null, nativeCallConv, nativeCharSet);
	}

	/// <summary>Defines a PInvoke method given its name, the name of the DLL in which the method is defined, the name of the entry point, the attributes of the method, the calling convention of the method, the return type of the method, the types of the parameters of the method, the PInvoke flags, and custom modifiers for the parameters and return type.</summary>
	/// <returns>A <see cref="T:System.Reflection.Emit.MethodBuilder" /> representing the defined PInvoke method.</returns>
	/// <param name="name">The name of the PInvoke method. <paramref name="name" /> cannot contain embedded nulls. </param>
	/// <param name="dllName">The name of the DLL in which the PInvoke method is defined. </param>
	/// <param name="entryName">The name of the entry point in the DLL. </param>
	/// <param name="attributes">The attributes of the method. </param>
	/// <param name="callingConvention">The method's calling convention. </param>
	/// <param name="returnType">The method's return type. </param>
	/// <param name="returnTypeRequiredCustomModifiers">An array of types representing the required custom modifiers, such as <see cref="T:System.Runtime.CompilerServices.IsConst" />, for the return type of the method. If the return type has no required custom modifiers, specify null.</param>
	/// <param name="returnTypeOptionalCustomModifiers">An array of types representing the optional custom modifiers, such as <see cref="T:System.Runtime.CompilerServices.IsConst" />, for the return type of the method. If the return type has no optional custom modifiers, specify null.</param>
	/// <param name="parameterTypes">The types of the method's parameters. </param>
	/// <param name="parameterTypeRequiredCustomModifiers">An array of arrays of types. Each array of types represents the required custom modifiers for the corresponding parameter, such as <see cref="T:System.Runtime.CompilerServices.IsConst" />. If a particular parameter has no required custom modifiers, specify null instead of an array of types. If none of the parameters have required custom modifiers, specify null instead of an array of arrays.</param>
	/// <param name="parameterTypeOptionalCustomModifiers">An array of arrays of types. Each array of types represents the optional custom modifiers for the corresponding parameter, such as <see cref="T:System.Runtime.CompilerServices.IsConst" />. If a particular parameter has no optional custom modifiers, specify null instead of an array of types. If none of the parameters have optional custom modifiers, specify null instead of an array of arrays.</param>
	/// <param name="nativeCallConv">The native calling convention. </param>
	/// <param name="nativeCharSet">The method's native character set. </param>
	/// <exception cref="T:System.ArgumentException">The method is not static.-or- The parent type is an interface.-or- The method is abstract.-or- The method was previously defined.-or- The length of <paramref name="name" />, <paramref name="dllName" />, or <paramref name="entryName" /> is zero. -or-The size of <paramref name="parameterTypeRequiredCustomModifiers" /> or <paramref name="parameterTypeOptionalCustomModifiers" /> does not equal the size of <paramref name="parameterTypes" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" />, <paramref name="dllName" />, or <paramref name="entryName" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The type was previously created using <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" />.-or-For the current dynamic type, the <see cref="P:System.Reflection.Emit.TypeBuilder.IsGenericType" /> property is true, but the <see cref="P:System.Reflection.Emit.TypeBuilder.IsGenericTypeDefinition" /> property is false.</exception>
	public MethodBuilder DefinePInvokeMethod(string name, string dllName, string entryName, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers, Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers, CallingConvention nativeCallConv, CharSet nativeCharSet)
	{
		check_name("name", name);
		check_name("dllName", dllName);
		check_name("entryName", entryName);
		if ((attributes & MethodAttributes.Abstract) != 0)
		{
			throw new ArgumentException("PInvoke methods must be static and native and cannot be abstract.");
		}
		if (base.IsInterface)
		{
			throw new ArgumentException("PInvoke methods cannot exist on interfaces.");
		}
		check_not_created();
		MethodBuilder methodBuilder = new MethodBuilder(this, name, attributes, callingConvention, returnType, returnTypeRequiredCustomModifiers, returnTypeOptionalCustomModifiers, parameterTypes, parameterTypeRequiredCustomModifiers, parameterTypeOptionalCustomModifiers, dllName, entryName, nativeCallConv, nativeCharSet);
		append_method(methodBuilder);
		return methodBuilder;
	}

	/// <summary>Defines a PInvoke method given its name, the name of the DLL in which the method is defined, the attributes of the method, the calling convention of the method, the return type of the method, the types of the parameters of the method, and the PInvoke flags.</summary>
	/// <returns>The defined PInvoke method.</returns>
	/// <param name="name">The name of the PInvoke method. <paramref name="name" /> cannot contain embedded nulls. </param>
	/// <param name="dllName">The name of the DLL in which the PInvoke method is defined. </param>
	/// <param name="attributes">The attributes of the method. </param>
	/// <param name="callingConvention">The method's calling convention. </param>
	/// <param name="returnType">The method's return type. </param>
	/// <param name="parameterTypes">The types of the method's parameters. </param>
	/// <param name="nativeCallConv">The native calling convention. </param>
	/// <param name="nativeCharSet">The method's native character set. </param>
	/// <exception cref="T:System.ArgumentException">The method is not static.-or- The parent type is an interface.-or- The method is abstract.-or- The method was previously defined.-or- The length of <paramref name="name" /> or <paramref name="dllName" /> is zero. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> or <paramref name="dllName" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The containing type has been previously created using <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" />. </exception>
	public MethodBuilder DefinePInvokeMethod(string name, string dllName, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, CallingConvention nativeCallConv, CharSet nativeCharSet)
	{
		return DefinePInvokeMethod(name, dllName, name, attributes, callingConvention, returnType, parameterTypes, nativeCallConv, nativeCharSet);
	}

	/// <summary>Adds a new method to the type, with the specified name and method attributes.</summary>
	/// <returns>A <see cref="T:System.Reflection.Emit.MethodBuilder" /> representing the newly defined method.</returns>
	/// <param name="name">The name of the method. <paramref name="name" /> cannot contain embedded nulls. </param>
	/// <param name="attributes">The attributes of the method. </param>
	/// <exception cref="T:System.ArgumentException">The length of <paramref name="name" /> is zero.-or- The type of the parent of this method is an interface, and this method is not virtual (Overridable in Visual Basic). </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The type was previously created using <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" />.-or-For the current dynamic type, the <see cref="P:System.Reflection.Emit.TypeBuilder.IsGenericType" /> property is true, but the <see cref="P:System.Reflection.Emit.TypeBuilder.IsGenericTypeDefinition" /> property is false. </exception>
	public MethodBuilder DefineMethod(string name, MethodAttributes attributes)
	{
		return DefineMethod(name, attributes, CallingConventions.Standard);
	}

	/// <summary>Adds a new method to the type, with the specified name, method attributes, and calling convention.</summary>
	/// <returns>A <see cref="T:System.Reflection.Emit.MethodBuilder" /> representing the newly defined method.</returns>
	/// <param name="name">The name of the method. <paramref name="name" /> cannot contain embedded nulls. </param>
	/// <param name="attributes">The attributes of the method. </param>
	/// <param name="callingConvention">The calling convention of the method. </param>
	/// <exception cref="T:System.ArgumentException">The length of <paramref name="name" /> is zero.-or- The type of the parent of this method is an interface and this method is not virtual (Overridable in Visual Basic). </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The type was previously created using <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" />.-or-For the current dynamic type, the <see cref="P:System.Reflection.Emit.TypeBuilder.IsGenericType" /> property is true, but the <see cref="P:System.Reflection.Emit.TypeBuilder.IsGenericTypeDefinition" /> property is false. </exception>
	public MethodBuilder DefineMethod(string name, MethodAttributes attributes, CallingConventions callingConvention)
	{
		return DefineMethod(name, attributes, callingConvention, null, null);
	}

	/// <summary>Specifies a given method body that implements a given method declaration, potentially with a different name.</summary>
	/// <param name="methodInfoBody">The method body to be used. This should be a MethodBuilder object. </param>
	/// <param name="methodInfoDeclaration">The method whose declaration is to be used. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="methodInfoBody" /> does not belong to this class. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="methodInfoBody" /> or <paramref name="methodInfoDeclaration" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The type was previously created using <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" />.-or- The declaring type of <paramref name="methodInfoBody" /> is not the type represented by this <see cref="T:System.Reflection.Emit.TypeBuilder" />. </exception>
	public void DefineMethodOverride(MethodInfo methodInfoBody, MethodInfo methodInfoDeclaration)
	{
		if (methodInfoBody == null)
		{
			throw new ArgumentNullException("methodInfoBody");
		}
		if (methodInfoDeclaration == null)
		{
			throw new ArgumentNullException("methodInfoDeclaration");
		}
		check_not_created();
		if (methodInfoBody.DeclaringType != this)
		{
			throw new ArgumentException("method body must belong to this type");
		}
		if (methodInfoBody is MethodBuilder)
		{
			((MethodBuilder)methodInfoBody).set_override(methodInfoDeclaration);
		}
	}

	/// <summary>Adds a new field to the type, with the given name, attributes, and field type.</summary>
	/// <returns>The defined field.</returns>
	/// <param name="fieldName">The name of the field. <paramref name="fieldName" /> cannot contain embedded nulls. </param>
	/// <param name="type">The type of the field </param>
	/// <param name="attributes">The attributes of the field. </param>
	/// <exception cref="T:System.ArgumentException">The length of <paramref name="fieldName" /> is zero.-or- <paramref name="type" /> is System.Void.-or- A total size was specified for the parent class of this field. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="fieldName" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The type was previously created using <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" />.</exception>
	public FieldBuilder DefineField(string fieldName, Type type, FieldAttributes attributes)
	{
		return DefineField(fieldName, type, null, null, attributes);
	}

	/// <summary>Adds a new field to the type, with the given name, attributes, field type, and custom modifiers.</summary>
	/// <returns>The defined field.</returns>
	/// <param name="fieldName">The name of the field. <paramref name="fieldName" /> cannot contain embedded nulls. </param>
	/// <param name="type">The type of the field </param>
	/// <param name="requiredCustomModifiers">An array of types representing the required custom modifiers for the field, such as <see cref="T:Microsoft.VisualC.IsConstModifier" />.</param>
	/// <param name="optionalCustomModifiers">An array of types representing the optional custom modifiers for the field, such as <see cref="T:Microsoft.VisualC.IsConstModifier" />.</param>
	/// <param name="attributes">The attributes of the field. </param>
	/// <exception cref="T:System.ArgumentException">The length of <paramref name="fieldName" /> is zero.-or- <paramref name="type" /> is System.Void.-or- A total size was specified for the parent class of this field. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="fieldName" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The type was previously created using <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" />. </exception>
	public FieldBuilder DefineField(string fieldName, Type type, Type[] requiredCustomModifiers, Type[] optionalCustomModifiers, FieldAttributes attributes)
	{
		check_name("fieldName", fieldName);
		if (type == typeof(void))
		{
			throw new ArgumentException("Bad field type in defining field.");
		}
		check_not_created();
		FieldBuilder fieldBuilder = new FieldBuilder(this, fieldName, type, attributes, requiredCustomModifiers, optionalCustomModifiers);
		if (fields != null)
		{
			if (fields.Length == num_fields)
			{
				FieldBuilder[] destinationArray = new FieldBuilder[fields.Length * 2];
				Array.Copy(fields, destinationArray, num_fields);
				fields = destinationArray;
			}
			fields[num_fields] = fieldBuilder;
			num_fields++;
		}
		else
		{
			fields = new FieldBuilder[1];
			fields[0] = fieldBuilder;
			num_fields++;
		}
		if (IsEnum && underlying_type == null && (attributes & FieldAttributes.Static) == 0)
		{
			underlying_type = type;
		}
		return fieldBuilder;
	}

	/// <summary>Adds a new property to the type, with the given name and property signature.</summary>
	/// <returns>The defined property.</returns>
	/// <param name="name">The name of the property. <paramref name="name" /> cannot contain embedded nulls. </param>
	/// <param name="attributes">The attributes of the property. </param>
	/// <param name="returnType">The return type of the property. </param>
	/// <param name="parameterTypes">The types of the parameters of the property. </param>
	/// <exception cref="T:System.ArgumentException">The length of <paramref name="name" /> is zero. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. -or- Any of the elements of the <paramref name="parameterTypes" /> array is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The type was previously created using <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" />. </exception>
	public PropertyBuilder DefineProperty(string name, PropertyAttributes attributes, Type returnType, Type[] parameterTypes)
	{
		return DefineProperty(name, attributes, (CallingConventions)0, returnType, null, null, parameterTypes, null, null);
	}

	/// <summary>Adds a new property to the type, with the given name, attributes, calling convention, and property signature.</summary>
	/// <returns>The defined property.</returns>
	/// <param name="name">The name of the property. <paramref name="name" /> cannot contain embedded nulls. </param>
	/// <param name="attributes">The attributes of the property. </param>
	/// <param name="callingConvention">The calling convention of the property accessors. </param>
	/// <param name="returnType">The return type of the property. </param>
	/// <param name="parameterTypes">The types of the parameters of the property.</param>
	/// <exception cref="T:System.ArgumentException">The length of <paramref name="name" /> is zero. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. -or- Any of the elements of the <paramref name="parameterTypes" /> array is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The type was previously created using <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" />. </exception>
	public PropertyBuilder DefineProperty(string name, PropertyAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes)
	{
		return DefineProperty(name, attributes, callingConvention, returnType, null, null, parameterTypes, null, null);
	}

	/// <summary>Adds a new property to the type, with the given name, property signature, and custom modifiers.</summary>
	/// <returns>The defined property.</returns>
	/// <param name="name">The name of the property. <paramref name="name" /> cannot contain embedded nulls. </param>
	/// <param name="attributes">The attributes of the property. </param>
	/// <param name="returnType">The return type of the property. </param>
	/// <param name="returnTypeRequiredCustomModifiers">An array of types representing the required custom modifiers, such as <see cref="T:System.Runtime.CompilerServices.IsConst" />, for the return type of the property. If the return type has no required custom modifiers, specify null.</param>
	/// <param name="returnTypeOptionalCustomModifiers">An array of types representing the optional custom modifiers, such as <see cref="T:System.Runtime.CompilerServices.IsConst" />, for the return type of the property. If the return type has no optional custom modifiers, specify null.</param>
	/// <param name="parameterTypes">The types of the parameters of the property. </param>
	/// <param name="parameterTypeRequiredCustomModifiers">An array of arrays of types. Each array of types represents the required custom modifiers for the corresponding parameter, such as <see cref="T:System.Runtime.CompilerServices.IsConst" />. If a particular parameter has no required custom modifiers, specify null instead of an array of types. If none of the parameters have required custom modifiers, specify null instead of an array of arrays.</param>
	/// <param name="parameterTypeOptionalCustomModifiers">An array of arrays of types. Each array of types represents the optional custom modifiers for the corresponding parameter, such as <see cref="T:System.Runtime.CompilerServices.IsConst" />. If a particular parameter has no optional custom modifiers, specify null instead of an array of types. If none of the parameters have optional custom modifiers, specify null instead of an array of arrays.</param>
	/// <exception cref="T:System.ArgumentException">The length of <paramref name="name" /> is zero. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null-or- Any of the elements of the <paramref name="parameterTypes" /> array is null</exception>
	/// <exception cref="T:System.InvalidOperationException">The type was previously created using <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" />. </exception>
	public PropertyBuilder DefineProperty(string name, PropertyAttributes attributes, Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers, Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers)
	{
		return DefineProperty(name, attributes, (CallingConventions)0, returnType, returnTypeRequiredCustomModifiers, returnTypeOptionalCustomModifiers, parameterTypes, parameterTypeRequiredCustomModifiers, parameterTypeOptionalCustomModifiers);
	}

	/// <summary>Adds a new property to the type, with the given name, calling convention, property signature, and custom modifiers.</summary>
	/// <returns>The defined property.</returns>
	/// <param name="name">The name of the property. <paramref name="name" /> cannot contain embedded nulls. </param>
	/// <param name="attributes">The attributes of the property. </param>
	/// <param name="callingConvention">The calling convention of the property accessors. </param>
	/// <param name="returnType">The return type of the property. </param>
	/// <param name="returnTypeRequiredCustomModifiers">An array of types representing the required custom modifiers, such as <see cref="T:System.Runtime.CompilerServices.IsConst" />, for the return type of the property. If the return type has no required custom modifiers, specify null.</param>
	/// <param name="returnTypeOptionalCustomModifiers">An array of types representing the optional custom modifiers, such as <see cref="T:System.Runtime.CompilerServices.IsConst" />, for the return type of the property. If the return type has no optional custom modifiers, specify null.</param>
	/// <param name="parameterTypes">The types of the parameters of the property. </param>
	/// <param name="parameterTypeRequiredCustomModifiers">An array of arrays of types. Each array of types represents the required custom modifiers for the corresponding parameter, such as <see cref="T:System.Runtime.CompilerServices.IsConst" />. If a particular parameter has no required custom modifiers, specify null instead of an array of types. If none of the parameters have required custom modifiers, specify null instead of an array of arrays.</param>
	/// <param name="parameterTypeOptionalCustomModifiers">An array of arrays of types. Each array of types represents the optional custom modifiers for the corresponding parameter, such as <see cref="T:System.Runtime.CompilerServices.IsConst" />. If a particular parameter has no optional custom modifiers, specify null instead of an array of types. If none of the parameters have optional custom modifiers, specify null instead of an array of arrays.</param>
	/// <exception cref="T:System.ArgumentException">The length of <paramref name="name" /> is zero. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. -or- Any of the elements of the <paramref name="parameterTypes" /> array is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The type was previously created using <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" />. </exception>
	public PropertyBuilder DefineProperty(string name, PropertyAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers, Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers)
	{
		check_name("name", name);
		if (parameterTypes != null)
		{
			for (int i = 0; i < parameterTypes.Length; i++)
			{
				if (parameterTypes[i] == null)
				{
					throw new ArgumentNullException("parameterTypes");
				}
			}
		}
		check_not_created();
		PropertyBuilder propertyBuilder = new PropertyBuilder(this, name, attributes, callingConvention, returnType, returnTypeRequiredCustomModifiers, returnTypeOptionalCustomModifiers, parameterTypes, parameterTypeRequiredCustomModifiers, parameterTypeOptionalCustomModifiers);
		if (properties != null)
		{
			Array.Resize(ref properties, properties.Length + 1);
			properties[properties.Length - 1] = propertyBuilder;
		}
		else
		{
			properties = new PropertyBuilder[1] { propertyBuilder };
		}
		return propertyBuilder;
	}

	/// <summary>Defines the initializer for this type.</summary>
	/// <returns>Returns a type initializer.</returns>
	/// <exception cref="T:System.InvalidOperationException">The containing type has been previously created using <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" />. </exception>
	[ComVisible(true)]
	public ConstructorBuilder DefineTypeInitializer()
	{
		return DefineConstructor(MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, CallingConventions.Standard, null);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern TypeInfo create_runtime_class();

	private bool is_nested_in(Type t)
	{
		while (t != null)
		{
			if (t == this)
			{
				return true;
			}
			t = t.DeclaringType;
		}
		return false;
	}

	private bool has_ctor_method()
	{
		MethodAttributes methodAttributes = MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
		for (int i = 0; i < num_methods; i++)
		{
			MethodBuilder methodBuilder = methods[i];
			if (methodBuilder.Name == ConstructorInfo.ConstructorName && (methodBuilder.Attributes & methodAttributes) == methodAttributes)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Creates a <see cref="T:System.Type" /> object for the class. After defining fields and methods on the class, CreateType is called in order to load its Type object.</summary>
	/// <returns>Returns the new <see cref="T:System.Type" /> object for this class.</returns>
	/// <exception cref="T:System.InvalidOperationException">The enclosing type has not been created.-or- This type is non-abstract and contains an abstract method.-or- This type is not an abstract class or an interface and has a method without a method body. </exception>
	/// <exception cref="T:System.NotSupportedException">The type contains invalid Microsoft intermediate language (MSIL) code.-or- The branch target is specified using a 1-byte offset, but the target is at a distance greater than 127 bytes from the branch. </exception>
	/// <exception cref="T:System.TypeLoadException">The type cannot be loaded. For example, it contains a static method that has the calling convention <see cref="F:System.Reflection.CallingConventions.HasThis" />.</exception>
	public Type CreateType()
	{
		return CreateTypeInfo();
	}

	/// <summary>Gets a <see cref="T:System.Reflection.TypeInfo" /> object that represents this type.</summary>
	/// <returns>An object that represents this type.</returns>
	public TypeInfo CreateTypeInfo()
	{
		if (createTypeCalled)
		{
			return created;
		}
		if (!base.IsInterface && parent == null && this != pmodule.assemblyb.corlib_object_type && FullName != "<Module>")
		{
			SetParent(pmodule.assemblyb.corlib_object_type);
		}
		if (fields != null)
		{
			FieldBuilder[] array = fields;
			foreach (FieldBuilder fieldBuilder in array)
			{
				if (fieldBuilder == null)
				{
					continue;
				}
				Type fieldType = fieldBuilder.FieldType;
				if (!fieldBuilder.IsStatic && fieldType is TypeBuilder && fieldType.IsValueType && fieldType != this && is_nested_in(fieldType))
				{
					TypeBuilder typeBuilder = (TypeBuilder)fieldType;
					if (!typeBuilder.is_created)
					{
						AppDomain.CurrentDomain.DoTypeResolve(typeBuilder);
						_ = typeBuilder.is_created;
					}
				}
			}
		}
		if (!base.IsInterface && !base.IsValueType && ctors == null && tname != "<Module>" && ((GetAttributeFlagsImpl() & TypeAttributes.Abstract) | TypeAttributes.Sealed) != (TypeAttributes.Abstract | TypeAttributes.Sealed) && !has_ctor_method())
		{
			DefineDefaultConstructor(MethodAttributes.Public);
		}
		createTypeCalled = true;
		if (parent != null && parent.IsSealed)
		{
			throw new TypeLoadException(string.Concat("Could not load type '", FullName, "' from assembly '", Assembly, "' because the parent type is sealed."));
		}
		if (parent == pmodule.assemblyb.corlib_enum_type && methods != null)
		{
			throw new TypeLoadException(string.Concat("Could not load type '", FullName, "' from assembly '", Assembly, "' because it is an enum with methods."));
		}
		if (interfaces != null)
		{
			Type[] array2 = interfaces;
			foreach (Type type in array2)
			{
				if (type.IsNestedPrivate && type.Assembly != Assembly)
				{
					throw new TypeLoadException(string.Concat("Could not load type '", FullName, "' from assembly '", Assembly, "' because it is implements the inaccessible interface '", type.FullName, "'."));
				}
			}
		}
		if (methods != null)
		{
			bool flag = !base.IsAbstract;
			for (int j = 0; j < num_methods; j++)
			{
				MethodBuilder methodBuilder = methods[j];
				if (flag && methodBuilder.IsAbstract)
				{
					throw new InvalidOperationException("Type is concrete but has abstract method " + methodBuilder);
				}
				methodBuilder.check_override();
				methodBuilder.fixup();
			}
		}
		if (ctors != null)
		{
			ConstructorBuilder[] array3 = ctors;
			for (int i = 0; i < array3.Length; i++)
			{
				array3[i].fixup();
			}
		}
		ResolveUserTypes();
		created = create_runtime_class();
		if (created != null)
		{
			return created;
		}
		return this;
	}

	private void ResolveUserTypes()
	{
		parent = ResolveUserType(parent);
		ResolveUserTypes(interfaces);
		if (fields != null)
		{
			FieldBuilder[] array = fields;
			foreach (FieldBuilder fieldBuilder in array)
			{
				if (fieldBuilder != null)
				{
					fieldBuilder.ResolveUserTypes();
				}
			}
		}
		if (methods != null)
		{
			MethodBuilder[] array2 = methods;
			foreach (MethodBuilder methodBuilder in array2)
			{
				if (methodBuilder != null)
				{
					methodBuilder.ResolveUserTypes();
				}
			}
		}
		if (ctors == null)
		{
			return;
		}
		ConstructorBuilder[] array3 = ctors;
		foreach (ConstructorBuilder constructorBuilder in array3)
		{
			if (constructorBuilder != null)
			{
				constructorBuilder.ResolveUserTypes();
			}
		}
	}

	internal static void ResolveUserTypes(Type[] types)
	{
		if (types != null)
		{
			for (int i = 0; i < types.Length; i++)
			{
				types[i] = ResolveUserType(types[i]);
			}
		}
	}

	internal static Type ResolveUserType(Type t)
	{
		if (t != null && (t.GetType().Assembly != typeof(int).Assembly || t is TypeDelegator))
		{
			t = t.UnderlyingSystemType;
			if (t != null && (t.GetType().Assembly != typeof(int).Assembly || t is TypeDelegator))
			{
				throw new NotSupportedException("User defined subclasses of System.Type are not yet supported.");
			}
			return t;
		}
		return t;
	}

	internal void FixupTokens(Dictionary<int, int> token_map, Dictionary<int, MemberInfo> member_map)
	{
		if (methods != null)
		{
			for (int i = 0; i < num_methods; i++)
			{
				methods[i].FixupTokens(token_map, member_map);
			}
		}
		if (ctors != null)
		{
			ConstructorBuilder[] array = ctors;
			for (int j = 0; j < array.Length; j++)
			{
				array[j].FixupTokens(token_map, member_map);
			}
		}
		if (subtypes != null)
		{
			TypeBuilder[] array2 = subtypes;
			for (int j = 0; j < array2.Length; j++)
			{
				array2[j].FixupTokens(token_map, member_map);
			}
		}
	}

	internal void GenerateDebugInfo(ISymbolWriter symbolWriter)
	{
		symbolWriter.OpenNamespace(Namespace);
		if (methods != null)
		{
			for (int i = 0; i < num_methods; i++)
			{
				methods[i].GenerateDebugInfo(symbolWriter);
			}
		}
		if (ctors != null)
		{
			ConstructorBuilder[] array = ctors;
			for (int j = 0; j < array.Length; j++)
			{
				array[j].GenerateDebugInfo(symbolWriter);
			}
		}
		symbolWriter.CloseNamespace();
		if (subtypes != null)
		{
			for (int k = 0; k < subtypes.Length; k++)
			{
				subtypes[k].GenerateDebugInfo(symbolWriter);
			}
		}
	}

	/// <summary>Returns an array of <see cref="T:System.Reflection.ConstructorInfo" /> objects representing the public and non-public constructors defined for this class, as specified.</summary>
	/// <returns>Returns an array of <see cref="T:System.Reflection.ConstructorInfo" /> objects representing the specified constructors defined for this class. If no constructors are defined, an empty array is returned.</returns>
	/// <param name="bindingAttr">This must be a bit flag from <see cref="T:System.Reflection.BindingFlags" /> as in InvokeMethod, NonPublic, and so on. </param>
	/// <exception cref="T:System.NotSupportedException">This method is not implemented for incomplete types. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[ComVisible(true)]
	public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
	{
		if (is_created)
		{
			return created.GetConstructors(bindingAttr);
		}
		throw new NotSupportedException();
	}

	internal ConstructorInfo[] GetConstructorsInternal(BindingFlags bindingAttr)
	{
		if (ctors == null)
		{
			return new ConstructorInfo[0];
		}
		ArrayList arrayList = new ArrayList();
		ConstructorBuilder[] array = ctors;
		foreach (ConstructorBuilder constructorBuilder in array)
		{
			bool flag = false;
			MethodAttributes attributes = constructorBuilder.Attributes;
			if ((attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Public)
			{
				if ((bindingAttr & BindingFlags.Public) != 0)
				{
					flag = true;
				}
			}
			else if ((bindingAttr & BindingFlags.NonPublic) != 0)
			{
				flag = true;
			}
			if (!flag)
			{
				continue;
			}
			flag = false;
			if ((attributes & MethodAttributes.Static) != 0)
			{
				if ((bindingAttr & BindingFlags.Static) != 0)
				{
					flag = true;
				}
			}
			else if ((bindingAttr & BindingFlags.Instance) != 0)
			{
				flag = true;
			}
			if (flag)
			{
				arrayList.Add(constructorBuilder);
			}
		}
		ConstructorInfo[] array2 = new ConstructorInfo[arrayList.Count];
		arrayList.CopyTo(array2);
		return array2;
	}

	/// <summary>Calling this method always throws <see cref="T:System.NotSupportedException" />.</summary>
	/// <returns>This method is not supported. No value is returned.</returns>
	/// <exception cref="T:System.NotSupportedException">This method is not supported. </exception>
	public override Type GetElementType()
	{
		throw new NotSupportedException();
	}

	/// <summary>Returns the event with the specified name.</summary>
	/// <returns>An <see cref="T:System.Reflection.EventInfo" /> object representing the event declared or inherited by this type with the specified name, or null if there are no matches.</returns>
	/// <param name="name">The name of the event to search for. </param>
	/// <param name="bindingAttr">A bitwise combination of <see cref="T:System.Reflection.BindingFlags" /> values that limits the search. </param>
	/// <exception cref="T:System.NotSupportedException">This method is not implemented for incomplete types. </exception>
	public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
	{
		check_created();
		return created.GetEvent(name, bindingAttr);
	}

	/// <summary>Returns the public events declared or inherited by this type.</summary>
	/// <returns>Returns an array of <see cref="T:System.Reflection.EventInfo" /> objects representing the public events declared or inherited by this type. An empty array is returned if there are no public events.</returns>
	/// <exception cref="T:System.NotSupportedException">This method is not implemented for incomplete types. </exception>
	public override EventInfo[] GetEvents()
	{
		return GetEvents(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
	}

	/// <summary>Returns the public and non-public events that are declared by this type.</summary>
	/// <returns>Returns an array of <see cref="T:System.Reflection.EventInfo" /> objects representing the events declared or inherited by this type that match the specified binding flags. An empty array is returned if there are no matching events.</returns>
	/// <param name="bindingAttr">A bitwise combination of <see cref="T:System.Reflection.BindingFlags" /> values that limits the search.</param>
	/// <exception cref="T:System.NotSupportedException">This method is not implemented for incomplete types. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public override EventInfo[] GetEvents(BindingFlags bindingAttr)
	{
		if (is_created)
		{
			return created.GetEvents(bindingAttr);
		}
		throw new NotSupportedException();
	}

	/// <summary>Returns the field specified by the given name.</summary>
	/// <returns>Returns the <see cref="T:System.Reflection.FieldInfo" /> object representing the field declared or inherited by this type with the specified name and public or non-public modifier. If there are no matches then null is returned.</returns>
	/// <param name="name">The name of the field to get. </param>
	/// <param name="bindingAttr">This must be a bit flag from <see cref="T:System.Reflection.BindingFlags" /> as in InvokeMethod, NonPublic, and so on. </param>
	/// <exception cref="T:System.NotSupportedException">This method is not implemented for incomplete types. </exception>
	public override FieldInfo GetField(string name, BindingFlags bindingAttr)
	{
		if (created != null)
		{
			return created.GetField(name, bindingAttr);
		}
		if (fields == null)
		{
			return null;
		}
		FieldBuilder[] array = fields;
		foreach (FieldInfo fieldInfo in array)
		{
			if (fieldInfo == null || fieldInfo.Name != name)
			{
				continue;
			}
			bool flag = false;
			FieldAttributes attributes = fieldInfo.Attributes;
			if ((attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Public)
			{
				if ((bindingAttr & BindingFlags.Public) != 0)
				{
					flag = true;
				}
			}
			else if ((bindingAttr & BindingFlags.NonPublic) != 0)
			{
				flag = true;
			}
			if (!flag)
			{
				continue;
			}
			flag = false;
			if ((attributes & FieldAttributes.Static) != 0)
			{
				if ((bindingAttr & BindingFlags.Static) != 0)
				{
					flag = true;
				}
			}
			else if ((bindingAttr & BindingFlags.Instance) != 0)
			{
				flag = true;
			}
			if (flag)
			{
				return fieldInfo;
			}
		}
		return null;
	}

	/// <summary>Returns the public and non-public fields that are declared by this type.</summary>
	/// <returns>Returns an array of <see cref="T:System.Reflection.FieldInfo" /> objects representing the public and non-public fields declared or inherited by this type. An empty array is returned if there are no fields, as specified.</returns>
	/// <param name="bindingAttr">This must be a bit flag from <see cref="T:System.Reflection.BindingFlags" /> : InvokeMethod, NonPublic, and so on. </param>
	/// <exception cref="T:System.NotSupportedException">This method is not implemented for incomplete types. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public override FieldInfo[] GetFields(BindingFlags bindingAttr)
	{
		if (created != null)
		{
			return created.GetFields(bindingAttr);
		}
		if (fields == null)
		{
			return new FieldInfo[0];
		}
		ArrayList arrayList = new ArrayList();
		FieldBuilder[] array = fields;
		foreach (FieldInfo fieldInfo in array)
		{
			if (fieldInfo == null)
			{
				continue;
			}
			bool flag = false;
			FieldAttributes attributes = fieldInfo.Attributes;
			if ((attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Public)
			{
				if ((bindingAttr & BindingFlags.Public) != 0)
				{
					flag = true;
				}
			}
			else if ((bindingAttr & BindingFlags.NonPublic) != 0)
			{
				flag = true;
			}
			if (!flag)
			{
				continue;
			}
			flag = false;
			if ((attributes & FieldAttributes.Static) != 0)
			{
				if ((bindingAttr & BindingFlags.Static) != 0)
				{
					flag = true;
				}
			}
			else if ((bindingAttr & BindingFlags.Instance) != 0)
			{
				flag = true;
			}
			if (flag)
			{
				arrayList.Add(fieldInfo);
			}
		}
		FieldInfo[] array2 = new FieldInfo[arrayList.Count];
		arrayList.CopyTo(array2);
		return array2;
	}

	/// <summary>Returns the interface implemented (directly or indirectly) by this class with the fully qualified name matching the given interface name.</summary>
	/// <returns>Returns a <see cref="T:System.Type" /> object representing the implemented interface. Returns null if no interface matching name is found.</returns>
	/// <param name="name">The name of the interface. </param>
	/// <param name="ignoreCase">If true, the search is case-insensitive. If false, the search is case-sensitive. </param>
	/// <exception cref="T:System.NotSupportedException">This method is not implemented for incomplete types. </exception>
	public override Type GetInterface(string name, bool ignoreCase)
	{
		check_created();
		return created.GetInterface(name, ignoreCase);
	}

	/// <summary>Returns an array of all the interfaces implemented on this type and its base types.</summary>
	/// <returns>Returns an array of <see cref="T:System.Type" /> objects representing the implemented interfaces. If none are defined, an empty array is returned.</returns>
	public override Type[] GetInterfaces()
	{
		if (is_created)
		{
			return created.GetInterfaces();
		}
		if (interfaces != null)
		{
			Type[] array = new Type[interfaces.Length];
			interfaces.CopyTo(array, 0);
			return array;
		}
		return Type.EmptyTypes;
	}

	/// <summary>Returns all the public and non-public members declared or inherited by this type, as specified.</summary>
	/// <returns>Returns an array of <see cref="T:System.Reflection.MemberInfo" /> objects representing the public and non-public members defined on this type if <paramref name="nonPublic" /> is used; otherwise, only the public members are returned.</returns>
	/// <param name="name">The name of the member. </param>
	/// <param name="type">The type of the member to return. </param>
	/// <param name="bindingAttr">This must be a bit flag from <see cref="T:System.Reflection.BindingFlags" />, as in InvokeMethod, NonPublic, and so on. </param>
	/// <exception cref="T:System.NotSupportedException">This method is not implemented for incomplete types. </exception>
	public override MemberInfo[] GetMember(string name, MemberTypes type, BindingFlags bindingAttr)
	{
		check_created();
		return created.GetMember(name, type, bindingAttr);
	}

	/// <summary>Returns the members for the public and non-public members declared or inherited by this type.</summary>
	/// <returns>Returns an array of <see cref="T:System.Reflection.MemberInfo" /> objects representing the public and non-public members declared or inherited by this type. An empty array is returned if there are no matching members.</returns>
	/// <param name="bindingAttr">This must be a bit flag from <see cref="T:System.Reflection.BindingFlags" />, such as InvokeMethod, NonPublic, and so on. </param>
	/// <exception cref="T:System.NotSupportedException">This method is not implemented for incomplete types. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
	{
		check_created();
		return created.GetMembers(bindingAttr);
	}

	private MethodInfo[] GetMethodsByName(string name, BindingFlags bindingAttr, bool ignoreCase, Type reflected_type)
	{
		MethodInfo[] array2;
		if ((bindingAttr & BindingFlags.DeclaredOnly) == 0 && parent != null)
		{
			MethodInfo[] array = parent.GetMethods(bindingAttr);
			ArrayList arrayList = new ArrayList(array.Length);
			bool flag = (bindingAttr & BindingFlags.FlattenHierarchy) != 0;
			foreach (MethodInfo methodInfo in array)
			{
				MethodAttributes attributes = methodInfo.Attributes;
				if ((!methodInfo.IsStatic || flag) && (attributes & MethodAttributes.MemberAccessMask) switch
				{
					MethodAttributes.Public => (bindingAttr & BindingFlags.Public) != 0, 
					MethodAttributes.Assembly => (bindingAttr & BindingFlags.NonPublic) != 0, 
					MethodAttributes.Private => false, 
					_ => (bindingAttr & BindingFlags.NonPublic) != 0, 
				})
				{
					arrayList.Add(methodInfo);
				}
			}
			if (methods == null)
			{
				array2 = new MethodInfo[arrayList.Count];
				arrayList.CopyTo(array2);
			}
			else
			{
				array2 = new MethodInfo[methods.Length + arrayList.Count];
				arrayList.CopyTo(array2, 0);
				methods.CopyTo(array2, arrayList.Count);
			}
		}
		else
		{
			array2 = methods;
		}
		if (array2 == null)
		{
			return new MethodInfo[0];
		}
		ArrayList arrayList2 = new ArrayList();
		MethodInfo[] array3 = array2;
		foreach (MethodInfo methodInfo2 in array3)
		{
			if (methodInfo2 == null || (name != null && string.Compare(methodInfo2.Name, name, ignoreCase) != 0))
			{
				continue;
			}
			bool flag2 = false;
			MethodAttributes attributes = methodInfo2.Attributes;
			if ((attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Public)
			{
				if ((bindingAttr & BindingFlags.Public) != 0)
				{
					flag2 = true;
				}
			}
			else if ((bindingAttr & BindingFlags.NonPublic) != 0)
			{
				flag2 = true;
			}
			if (!flag2)
			{
				continue;
			}
			flag2 = false;
			if ((attributes & MethodAttributes.Static) != 0)
			{
				if ((bindingAttr & BindingFlags.Static) != 0)
				{
					flag2 = true;
				}
			}
			else if ((bindingAttr & BindingFlags.Instance) != 0)
			{
				flag2 = true;
			}
			if (flag2)
			{
				arrayList2.Add(methodInfo2);
			}
		}
		MethodInfo[] array4 = new MethodInfo[arrayList2.Count];
		arrayList2.CopyTo(array4);
		return array4;
	}

	/// <summary>Returns all the public and non-public methods declared or inherited by this type, as specified.</summary>
	/// <returns>Returns an array of <see cref="T:System.Reflection.MethodInfo" /> objects representing the public and non-public methods defined on this type if <paramref name="nonPublic" /> is used; otherwise, only the public methods are returned.</returns>
	/// <param name="bindingAttr">This must be a bit flag from <see cref="T:System.Reflection.BindingFlags" /> as in InvokeMethod, NonPublic, and so on. </param>
	/// <exception cref="T:System.NotSupportedException">This method is not implemented for incomplete types. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
	{
		return GetMethodsByName(null, bindingAttr, ignoreCase: false, this);
	}

	protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
	{
		check_created();
		if (types == null)
		{
			return created.GetMethod(name, bindingAttr);
		}
		return created.GetMethod(name, bindingAttr, binder, callConvention, types, modifiers);
	}

	/// <summary>Returns the public and non-public nested types that are declared by this type.</summary>
	/// <returns>A <see cref="T:System.Type" /> object representing the nested type that matches the specified requirements, if found; otherwise, null.</returns>
	/// <param name="name">The <see cref="T:System.String" /> containing the name of the nested type to get. </param>
	/// <param name="bindingAttr">A bitmask comprised of one or more <see cref="T:System.Reflection.BindingFlags" /> that specify how the search is conducted.-or- Zero, to conduct a case-sensitive search for public methods. </param>
	/// <exception cref="T:System.NotSupportedException">This method is not implemented for incomplete types. </exception>
	public override Type GetNestedType(string name, BindingFlags bindingAttr)
	{
		check_created();
		if (subtypes == null)
		{
			return null;
		}
		TypeBuilder[] array = subtypes;
		foreach (TypeBuilder typeBuilder in array)
		{
			if (!typeBuilder.is_created)
			{
				continue;
			}
			if ((typeBuilder.attrs & TypeAttributes.VisibilityMask) == TypeAttributes.NestedPublic)
			{
				if ((bindingAttr & BindingFlags.Public) == 0)
				{
					continue;
				}
			}
			else if ((bindingAttr & BindingFlags.NonPublic) == 0)
			{
				continue;
			}
			if (typeBuilder.Name == name)
			{
				return typeBuilder.created;
			}
		}
		return null;
	}

	/// <summary>Returns the public and non-public nested types that are declared or inherited by this type.</summary>
	/// <returns>An array of <see cref="T:System.Type" /> objects representing all the types nested within the current <see cref="T:System.Type" /> that match the specified binding constraints.An empty array of type <see cref="T:System.Type" />, if no types are nested within the current <see cref="T:System.Type" />, or if none of the nested types match the binding constraints.</returns>
	/// <param name="bindingAttr">This must be a bit flag from <see cref="T:System.Reflection.BindingFlags" />, as in InvokeMethod, NonPublic, and so on. </param>
	/// <exception cref="T:System.NotSupportedException">This method is not implemented for incomplete types. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public override Type[] GetNestedTypes(BindingFlags bindingAttr)
	{
		if (!is_created)
		{
			throw new NotSupportedException();
		}
		ArrayList arrayList = new ArrayList();
		if (subtypes == null)
		{
			return Type.EmptyTypes;
		}
		TypeBuilder[] array = subtypes;
		foreach (TypeBuilder typeBuilder in array)
		{
			bool flag = false;
			if ((typeBuilder.attrs & TypeAttributes.VisibilityMask) == TypeAttributes.NestedPublic)
			{
				if ((bindingAttr & BindingFlags.Public) != 0)
				{
					flag = true;
				}
			}
			else if ((bindingAttr & BindingFlags.NonPublic) != 0)
			{
				flag = true;
			}
			if (flag)
			{
				arrayList.Add(typeBuilder);
			}
		}
		Type[] array2 = new Type[arrayList.Count];
		arrayList.CopyTo(array2);
		return array2;
	}

	/// <summary>Returns all the public and non-public properties declared or inherited by this type, as specified.</summary>
	/// <returns>Returns an array of PropertyInfo objects representing the public and non-public properties defined on this type if <paramref name="nonPublic" /> is used; otherwise, only the public properties are returned.</returns>
	/// <param name="bindingAttr">This invocation attribute. This must be a bit flag from <see cref="T:System.Reflection.BindingFlags" /> : InvokeMethod, NonPublic, and so on. </param>
	/// <exception cref="T:System.NotSupportedException">This method is not implemented for incomplete types. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
	{
		if (is_created)
		{
			return created.GetProperties(bindingAttr);
		}
		if (properties == null)
		{
			return new PropertyInfo[0];
		}
		ArrayList arrayList = new ArrayList();
		PropertyBuilder[] array = properties;
		foreach (PropertyInfo propertyInfo in array)
		{
			bool flag = false;
			MethodInfo methodInfo = propertyInfo.GetGetMethod(nonPublic: true);
			if (methodInfo == null)
			{
				methodInfo = propertyInfo.GetSetMethod(nonPublic: true);
			}
			if (methodInfo == null)
			{
				continue;
			}
			MethodAttributes attributes = methodInfo.Attributes;
			if ((attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Public)
			{
				if ((bindingAttr & BindingFlags.Public) != 0)
				{
					flag = true;
				}
			}
			else if ((bindingAttr & BindingFlags.NonPublic) != 0)
			{
				flag = true;
			}
			if (!flag)
			{
				continue;
			}
			flag = false;
			if ((attributes & MethodAttributes.Static) != 0)
			{
				if ((bindingAttr & BindingFlags.Static) != 0)
				{
					flag = true;
				}
			}
			else if ((bindingAttr & BindingFlags.Instance) != 0)
			{
				flag = true;
			}
			if (flag)
			{
				arrayList.Add(propertyInfo);
			}
		}
		PropertyInfo[] array2 = new PropertyInfo[arrayList.Count];
		arrayList.CopyTo(array2);
		return array2;
	}

	protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
	{
		throw not_supported();
	}

	protected override bool HasElementTypeImpl()
	{
		if (!is_created)
		{
			return false;
		}
		return created.HasElementType;
	}

	/// <summary>Invokes the specified member. The method that is to be invoked must be accessible and provide the most specific match with the specified argument list, under the constraints of the specified binder and invocation attributes.</summary>
	/// <returns>Returns the return value of the invoked member.</returns>
	/// <param name="name">The name of the member to invoke. This can be a constructor, method, property, or field. A suitable invocation attribute must be specified. Note that it is possible to invoke the default member of a class by passing an empty string as the name of the member. </param>
	/// <param name="invokeAttr">The invocation attribute. This must be a bit flag from BindingFlags. </param>
	/// <param name="binder">An object that enables the binding, coercion of argument types, invocation of members, and retrieval of MemberInfo objects using reflection. If binder is null, the default binder is used. See <see cref="T:System.Reflection.Binder" />. </param>
	/// <param name="target">The object on which to invoke the specified member. If the member is static, this parameter is ignored. </param>
	/// <param name="args">An argument list. This is an array of Objects that contains the number, order, and type of the parameters of the member to be invoked. If there are no parameters this should be null. </param>
	/// <param name="modifiers">An array of the same length as <paramref name="args" /> with elements that represent the attributes associated with the arguments of the member to be invoked. A parameter has attributes associated with it in the metadata. They are used by various interoperability services. See the metadata specs for more details. </param>
	/// <param name="culture">An instance of CultureInfo used to govern the coercion of types. If this is null, the CultureInfo for the current thread is used. (Note that this is necessary to, for example, convert a String that represents 1000 to a Double value, since 1000 is represented differently by different cultures.) </param>
	/// <param name="namedParameters">Each parameter in the <paramref name="namedParameters" /> array gets the value in the corresponding element in the <paramref name="args" /> array. If the length of <paramref name="args" /> is greater than the length of <paramref name="namedParameters" />, the remaining argument values are passed in order. </param>
	/// <exception cref="T:System.NotSupportedException">This method is not currently supported for incomplete types. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	/// </PermissionSet>
	public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
	{
		check_created();
		return created.InvokeMember(name, invokeAttr, binder, target, args, modifiers, culture, namedParameters);
	}

	protected override bool IsArrayImpl()
	{
		return false;
	}

	protected override bool IsByRefImpl()
	{
		return false;
	}

	protected override bool IsCOMObjectImpl()
	{
		return (GetAttributeFlagsImpl() & TypeAttributes.Import) != 0;
	}

	protected override bool IsPointerImpl()
	{
		return false;
	}

	protected override bool IsPrimitiveImpl()
	{
		return false;
	}

	protected override bool IsValueTypeImpl()
	{
		if (this == pmodule.assemblyb.corlib_value_type || this == pmodule.assemblyb.corlib_enum_type)
		{
			return false;
		}
		Type baseType = parent;
		while (baseType != null)
		{
			if (baseType == pmodule.assemblyb.corlib_value_type)
			{
				return true;
			}
			baseType = baseType.BaseType;
		}
		return false;
	}

	/// <summary>Returns a <see cref="T:System.Type" /> object that represents a one-dimensional array of the current type, with a lower bound of zero.</summary>
	/// <returns>A <see cref="T:System.Type" /> object representing a one-dimensional array type whose element type is the current type, with a lower bound of zero.</returns>
	public override Type MakeArrayType()
	{
		return new ArrayType(this, 0);
	}

	/// <summary>Returns a <see cref="T:System.Type" /> object that represents an array of the current type, with the specified number of dimensions.</summary>
	/// <returns>A <see cref="T:System.Type" /> object that represents a one-dimensional array of the current type.</returns>
	/// <param name="rank">The number of dimensions for the array. </param>
	/// <exception cref="T:System.IndexOutOfRangeException">
	///   <paramref name="rank" /> is not a valid array dimension.</exception>
	public override Type MakeArrayType(int rank)
	{
		if (rank < 1)
		{
			throw new IndexOutOfRangeException();
		}
		return new ArrayType(this, rank);
	}

	/// <summary>Returns a <see cref="T:System.Type" /> object that represents the current type when passed as a ref parameter (ByRef in Visual Basic).</summary>
	/// <returns>A <see cref="T:System.Type" /> object that represents the current type when passed as a ref parameter (ByRef in Visual Basic).</returns>
	public override Type MakeByRefType()
	{
		return new ByRefType(this);
	}

	/// <summary>Substitutes the elements of an array of types for the type parameters of the current generic type definition, and returns the resulting constructed type.</summary>
	/// <returns>A <see cref="T:System.Type" /> representing the constructed type formed by substituting the elements of <paramref name="typeArguments" /> for the type parameters of the current generic type. </returns>
	/// <param name="typeArguments">An array of types to be substituted for the type parameters of the current generic type definition.</param>
	/// <exception cref="T:System.InvalidOperationException">The current type does not represent the definition of a generic type. That is, <see cref="P:System.Reflection.Emit.TypeBuilder.IsGenericTypeDefinition" /> returns false. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="typeArguments" /> is null.-or- Any element of <paramref name="typeArguments" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">Any element of <paramref name="typeArguments" /> does not satisfy the constraints specified for the corresponding type parameter of the current generic type. </exception>
	public override Type MakeGenericType(params Type[] typeArguments)
	{
		if (!IsGenericTypeDefinition)
		{
			throw new InvalidOperationException("not a generic type definition");
		}
		if (typeArguments == null)
		{
			throw new ArgumentNullException("typeArguments");
		}
		if (generic_params.Length != typeArguments.Length)
		{
			throw new ArgumentException($"The type or method has {generic_params.Length} generic parameter(s) but {typeArguments.Length} generic argument(s) where provided. A generic argument must be provided for each generic parameter.", "typeArguments");
		}
		for (int i = 0; i < typeArguments.Length; i++)
		{
			if (typeArguments[i] == null)
			{
				throw new ArgumentNullException("typeArguments");
			}
		}
		Type[] array = new Type[typeArguments.Length];
		typeArguments.CopyTo(array, 0);
		return pmodule.assemblyb.MakeGenericType(this, array);
	}

	/// <summary>Returns a <see cref="T:System.Type" /> object that represents the type of an unmanaged pointer to the current type.</summary>
	/// <returns>A <see cref="T:System.Type" /> object that represents the type of an unmanaged pointer to the current type.</returns>
	public override Type MakePointerType()
	{
		return new PointerType(this);
	}

	/// <summary>Set a custom attribute using a custom attribute builder.</summary>
	/// <param name="customBuilder">An instance of a helper class to define the custom attribute. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="customBuilder" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">For the current dynamic type, the <see cref="P:System.Reflection.Emit.TypeBuilder.IsGenericType" /> property is true, but the <see cref="P:System.Reflection.Emit.TypeBuilder.IsGenericTypeDefinition" /> property is false.</exception>
	public void SetCustomAttribute(CustomAttributeBuilder customBuilder)
	{
		if (customBuilder == null)
		{
			throw new ArgumentNullException("customBuilder");
		}
		switch (customBuilder.Ctor.ReflectedType.FullName)
		{
		case "System.Runtime.InteropServices.StructLayoutAttribute":
		{
			byte[] data = customBuilder.Data;
			int num = data[2] | (data[3] << 8);
			attrs &= ~TypeAttributes.LayoutMask;
			switch ((LayoutKind)num)
			{
			case LayoutKind.Auto:
				attrs |= TypeAttributes.NotPublic;
				break;
			case LayoutKind.Explicit:
				attrs |= TypeAttributes.ExplicitLayout;
				break;
			case LayoutKind.Sequential:
				attrs |= TypeAttributes.SequentialLayout;
				break;
			default:
				throw new Exception("Error in customattr");
			}
			Type obj = ((customBuilder.Ctor is ConstructorBuilder) ? ((ConstructorBuilder)customBuilder.Ctor).parameters[0] : customBuilder.Ctor.GetParametersInternal()[0].ParameterType);
			int num2 = 6;
			if (obj.FullName == "System.Int16")
			{
				num2 = 4;
			}
			int num3 = data[num2++];
			num3 |= data[num2++] << 8;
			for (int i = 0; i < num3; i++)
			{
				num2++;
				int num4;
				if (data[num2++] == 85)
				{
					num4 = CustomAttributeBuilder.decode_len(data, num2, out num2);
					CustomAttributeBuilder.string_from_bytes(data, num2, num4);
					num2 += num4;
				}
				num4 = CustomAttributeBuilder.decode_len(data, num2, out num2);
				string text = CustomAttributeBuilder.string_from_bytes(data, num2, num4);
				num2 += num4;
				int num5 = data[num2++];
				num5 |= data[num2++] << 8;
				num5 |= data[num2++] << 16;
				num5 |= data[num2++] << 24;
				switch (text)
				{
				case "CharSet":
					switch ((CharSet)num5)
					{
					case CharSet.None:
					case CharSet.Ansi:
						attrs &= ~TypeAttributes.StringFormatMask;
						break;
					case CharSet.Unicode:
						attrs &= ~TypeAttributes.AutoClass;
						attrs |= TypeAttributes.UnicodeClass;
						break;
					case CharSet.Auto:
						attrs &= ~TypeAttributes.UnicodeClass;
						attrs |= TypeAttributes.AutoClass;
						break;
					}
					break;
				case "Pack":
					packing_size = (PackingSize)num5;
					break;
				case "Size":
					class_size = num5;
					break;
				}
			}
			return;
		}
		case "System.Runtime.CompilerServices.SpecialNameAttribute":
			attrs |= TypeAttributes.SpecialName;
			return;
		case "System.SerializableAttribute":
			attrs |= TypeAttributes.Serializable;
			return;
		case "System.Runtime.InteropServices.ComImportAttribute":
			attrs |= TypeAttributes.Import;
			return;
		case "System.Security.SuppressUnmanagedCodeSecurityAttribute":
			attrs |= TypeAttributes.HasSecurity;
			break;
		}
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

	/// <summary>Sets a custom attribute using a specified custom attribute blob.</summary>
	/// <param name="con">The constructor for the custom attribute. </param>
	/// <param name="binaryAttribute">A byte blob representing the attributes. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="con" /> or <paramref name="binaryAttribute" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">For the current dynamic type, the <see cref="P:System.Reflection.Emit.TypeBuilder.IsGenericType" /> property is true, but the <see cref="P:System.Reflection.Emit.TypeBuilder.IsGenericTypeDefinition" /> property is false.</exception>
	[ComVisible(true)]
	public void SetCustomAttribute(ConstructorInfo con, byte[] binaryAttribute)
	{
		SetCustomAttribute(new CustomAttributeBuilder(con, binaryAttribute));
	}

	/// <summary>Adds a new event to the type, with the given name, attributes and event type.</summary>
	/// <returns>The defined event.</returns>
	/// <param name="name">The name of the event. <paramref name="name" /> cannot contain embedded nulls. </param>
	/// <param name="attributes">The attributes of the event. </param>
	/// <param name="eventtype">The type of the event. </param>
	/// <exception cref="T:System.ArgumentException">The length of <paramref name="name" /> is zero. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null.-or- <paramref name="eventtype" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The type was previously created using <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" />. </exception>
	public EventBuilder DefineEvent(string name, EventAttributes attributes, Type eventtype)
	{
		check_name("name", name);
		if (eventtype == null)
		{
			throw new ArgumentNullException("type");
		}
		check_not_created();
		EventBuilder eventBuilder = new EventBuilder(this, name, attributes, eventtype);
		if (events != null)
		{
			EventBuilder[] array = new EventBuilder[events.Length + 1];
			Array.Copy(events, array, events.Length);
			array[events.Length] = eventBuilder;
			events = array;
		}
		else
		{
			events = new EventBuilder[1];
			events[0] = eventBuilder;
		}
		return eventBuilder;
	}

	/// <summary>Defines initialized data field in the .sdata section of the portable executable (PE) file.</summary>
	/// <returns>A field to reference the data.</returns>
	/// <param name="name">The name used to refer to the data. <paramref name="name" /> cannot contain embedded nulls. </param>
	/// <param name="data">The blob of data. </param>
	/// <param name="attributes">The attributes for the field. </param>
	/// <exception cref="T:System.ArgumentException">Length of <paramref name="name" /> is zero.-or- The size of the data is less than or equal to zero, or greater than or equal to 0x3f0000. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> or <paramref name="data" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" /> has been previously called. </exception>
	public FieldBuilder DefineInitializedData(string name, byte[] data, FieldAttributes attributes)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		FieldBuilder fieldBuilder = DefineUninitializedData(name, data.Length, attributes);
		fieldBuilder.SetRVAData(data);
		return fieldBuilder;
	}

	/// <summary>Defines an uninitialized data field in the .sdata section of the portable executable (PE) file.</summary>
	/// <returns>A field to reference the data.</returns>
	/// <param name="name">The name used to refer to the data. <paramref name="name" /> cannot contain embedded nulls. </param>
	/// <param name="size">The size of the data field. </param>
	/// <param name="attributes">The attributes for the field. </param>
	/// <exception cref="T:System.ArgumentException">Length of <paramref name="name" /> is zero.-or- <paramref name="size" /> is less than or equal to zero, or greater than or equal to 0x003f0000. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The type was previously created using <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" />. </exception>
	public FieldBuilder DefineUninitializedData(string name, int size, FieldAttributes attributes)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (name.Length == 0)
		{
			throw new ArgumentException("Empty name is not legal", "name");
		}
		if (size <= 0 || size > 4128768)
		{
			throw new ArgumentException("Data size must be > 0 and < 0x3f0000");
		}
		check_not_created();
		string text = "$ArrayType$" + size;
		TypeIdentifier innerName = TypeIdentifiers.WithoutEscape(text);
		Type type = pmodule.GetRegisteredType(fullname.NestedName(innerName));
		if (type == null)
		{
			TypeBuilder typeBuilder = DefineNestedType(text, TypeAttributes.NestedPrivate | TypeAttributes.ExplicitLayout | TypeAttributes.Sealed, pmodule.assemblyb.corlib_value_type, null, PackingSize.Size1, size);
			typeBuilder.CreateType();
			type = typeBuilder;
		}
		return DefineField(name, type, attributes | FieldAttributes.Static | FieldAttributes.HasFieldRVA);
	}

	/// <summary>Sets the base type of the type currently under construction.</summary>
	/// <param name="parent">The new base type. </param>
	/// <exception cref="T:System.InvalidOperationException">The type was previously created using <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" />.-or-<paramref name="parent" /> is null, and the current instance represents an interface whose attributes do not include <see cref="F:System.Reflection.TypeAttributes.Abstract" />.-or-For the current dynamic type, the <see cref="P:System.Reflection.Emit.TypeBuilder.IsGenericType" /> property is true, but the <see cref="P:System.Reflection.Emit.TypeBuilder.IsGenericTypeDefinition" /> property is false. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="parent" /> is an interface. This exception condition is new in the .NET Framework version 2.0. </exception>
	public void SetParent(Type parent)
	{
		check_not_created();
		if (parent == null)
		{
			if ((attrs & TypeAttributes.ClassSemanticsMask) != 0)
			{
				if ((attrs & TypeAttributes.Abstract) == 0)
				{
					throw new InvalidOperationException("Interface must be declared abstract.");
				}
				this.parent = null;
			}
			else
			{
				this.parent = typeof(object);
			}
		}
		else
		{
			this.parent = parent;
		}
		this.parent = ResolveUserType(this.parent);
	}

	internal int get_next_table_index(object obj, int table, bool inc)
	{
		return pmodule.get_next_table_index(obj, table, inc);
	}

	/// <summary>Returns an interface mapping for the requested interface.</summary>
	/// <returns>Returns the requested interface mapping.</returns>
	/// <param name="interfaceType">The <see cref="T:System.Type" /> of the interface for which the mapping is to be retrieved. </param>
	/// <exception cref="T:System.NotSupportedException">This method is not implemented for incomplete types. </exception>
	[ComVisible(true)]
	public override InterfaceMapping GetInterfaceMap(Type interfaceType)
	{
		if (created == null)
		{
			throw new NotSupportedException("This method is not implemented for incomplete types.");
		}
		return created.GetInterfaceMap(interfaceType);
	}

	internal override Type InternalResolve()
	{
		check_created();
		return created;
	}

	internal override Type RuntimeResolve()
	{
		check_created();
		return created;
	}

	private Exception not_supported()
	{
		return new NotSupportedException("The invoked member is not supported in a dynamic module.");
	}

	private void check_not_created()
	{
		if (is_created)
		{
			throw new InvalidOperationException("Unable to change after type has been created.");
		}
	}

	private void check_created()
	{
		if (!is_created)
		{
			throw not_supported();
		}
	}

	private void check_name(string argName, string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException(argName);
		}
		if (name.Length == 0)
		{
			throw new ArgumentException("Empty name is not legal", argName);
		}
		if (name[0] == '\0')
		{
			throw new ArgumentException("Illegal name", argName);
		}
	}

	/// <summary>Returns the name of the type excluding the namespace.</summary>
	/// <returns>Read-only. The name of the type excluding the namespace.</returns>
	public override string ToString()
	{
		return FullName;
	}

	/// <summary>Gets a value that indicates whether a specified <see cref="T:System.Type" /> can be assigned to this object.</summary>
	/// <returns>true if the <paramref name="c" /> parameter and the current type represent the same type, or if the current type is in the inheritance hierarchy of <paramref name="c" />, or if the current type is an interface that <paramref name="c" /> supports. false if none of these conditions are valid, or if <paramref name="c" /> is null.</returns>
	/// <param name="c">The object to test. </param>
	[MonoTODO]
	public override bool IsAssignableFrom(Type c)
	{
		return base.IsAssignableFrom(c);
	}

	[MonoTODO("arrays")]
	internal bool IsAssignableTo(Type c)
	{
		if (c == this)
		{
			return true;
		}
		if (c.IsInterface)
		{
			if (parent != null && is_created && c.IsAssignableFrom(parent))
			{
				return true;
			}
			if (interfaces == null)
			{
				return false;
			}
			Type[] array = interfaces;
			foreach (Type c2 in array)
			{
				if (c.IsAssignableFrom(c2))
				{
					return true;
				}
			}
			if (!is_created)
			{
				return false;
			}
		}
		if (parent == null)
		{
			return c == typeof(object);
		}
		return c.IsAssignableFrom(parent);
	}

	/// <summary>Returns a value that indicates whether the current dynamic type has been created.</summary>
	/// <returns>true if the <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" /> method has been called; otherwise, false. </returns>
	public bool IsCreated()
	{
		return is_created;
	}

	/// <summary>Returns an array of <see cref="T:System.Type" /> objects representing the type arguments of a generic type or the type parameters of a generic type definition.</summary>
	/// <returns>An array of <see cref="T:System.Type" /> objects. The elements of the array represent the type arguments of a generic type or the type parameters of a generic type definition.</returns>
	public override Type[] GetGenericArguments()
	{
		if (generic_params == null)
		{
			return null;
		}
		Type[] array = new Type[generic_params.Length];
		generic_params.CopyTo(array, 0);
		return array;
	}

	/// <summary>Returns a <see cref="T:System.Type" /> object that represents a generic type definition from which the current type can be obtained.</summary>
	/// <returns>A <see cref="T:System.Type" /> object representing a generic type definition from which the current type can be obtained.</returns>
	/// <exception cref="T:System.InvalidOperationException">The current type is not generic. That is, <see cref="P:System.Reflection.Emit.TypeBuilder.IsGenericType" /> returns false.</exception>
	public override Type GetGenericTypeDefinition()
	{
		if (generic_params == null)
		{
			throw new InvalidOperationException("Type is not generic");
		}
		return this;
	}

	/// <summary>Defines the generic type parameters for the current type, specifying their number and their names, and returns an array of <see cref="T:System.Reflection.Emit.GenericTypeParameterBuilder" /> objects that can be used to set their constraints.</summary>
	/// <returns>An array of <see cref="T:System.Reflection.Emit.GenericTypeParameterBuilder" /> objects that can be used to define the constraints of the generic type parameters for the current type.</returns>
	/// <param name="names">An array of names for the generic type parameters.</param>
	/// <exception cref="T:System.InvalidOperationException">Generic type parameters have already been defined for this type.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="names" /> is null.-or-An element of <paramref name="names" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="names" /> is an empty array.</exception>
	public GenericTypeParameterBuilder[] DefineGenericParameters(params string[] names)
	{
		if (names == null)
		{
			throw new ArgumentNullException("names");
		}
		if (names.Length == 0)
		{
			throw new ArgumentException("names");
		}
		generic_params = new GenericTypeParameterBuilder[names.Length];
		for (int i = 0; i < names.Length; i++)
		{
			string text = names[i];
			if (text == null)
			{
				throw new ArgumentNullException("names");
			}
			generic_params[i] = new GenericTypeParameterBuilder(this, null, text, i);
		}
		return generic_params;
	}

	/// <summary>Returns the constructor of the specified constructed generic type that corresponds to the specified constructor of the generic type definition. </summary>
	/// <returns>A <see cref="T:System.Reflection.ConstructorInfo" /> object that represents the constructor of <paramref name="type" /> corresponding to <paramref name="constructor" />, which specifies a constructor belonging to the generic type definition of <paramref name="type" />.</returns>
	/// <param name="type">The constructed generic type whose constructor is returned.</param>
	/// <param name="constructor">A constructor on the generic type definition of <paramref name="type" />, which specifies which constructor of <paramref name="type" /> to return.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="type" /> does not represent a generic type. -or-<paramref name="type" /> is not of type <see cref="T:System.Reflection.Emit.TypeBuilder" />.-or-The declaring type of <paramref name="constructor" /> is not a generic type definition. -or-The declaring type of <paramref name="constructor" /> is not the generic type definition of <paramref name="type" />.</exception>
	public static ConstructorInfo GetConstructor(Type type, ConstructorInfo constructor)
	{
		if (type == null)
		{
			throw new ArgumentException("Type is not generic", "type");
		}
		if (!type.IsGenericType)
		{
			throw new ArgumentException("Type is not a generic type", "type");
		}
		if (type.IsGenericTypeDefinition)
		{
			throw new ArgumentException("Type cannot be a generic type definition", "type");
		}
		if (constructor == null)
		{
			throw new NullReferenceException();
		}
		if (!constructor.DeclaringType.IsGenericTypeDefinition)
		{
			throw new ArgumentException("constructor declaring type is not a generic type definition", "constructor");
		}
		if (constructor.DeclaringType != type.GetGenericTypeDefinition())
		{
			throw new ArgumentException("constructor declaring type is not the generic type definition of type", "constructor");
		}
		ConstructorInfo constructor2 = type.GetConstructor(constructor);
		if (constructor2 == null)
		{
			throw new ArgumentException("constructor not found");
		}
		return constructor2;
	}

	private static bool IsValidGetMethodType(Type type)
	{
		if (type is TypeBuilder || type is TypeBuilderInstantiation)
		{
			return true;
		}
		if (type.Module is ModuleBuilder)
		{
			return true;
		}
		if (type.IsGenericParameter)
		{
			return false;
		}
		Type[] genericArguments = type.GetGenericArguments();
		if (genericArguments == null)
		{
			return false;
		}
		for (int i = 0; i < genericArguments.Length; i++)
		{
			if (IsValidGetMethodType(genericArguments[i]))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Returns the method of the specified constructed generic type that corresponds to the specified method of the generic type definition. </summary>
	/// <returns>A <see cref="T:System.Reflection.MethodInfo" /> object that represents the method of <paramref name="type" /> corresponding to <paramref name="method" />, which specifies a method belonging to the generic type definition of <paramref name="type" />.</returns>
	/// <param name="type">The constructed generic type whose method is returned.</param>
	/// <param name="method">A method on the generic type definition of <paramref name="type" />, which specifies which method of <paramref name="type" /> to return.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="method" /> is a generic method that is not a generic method definition.-or-<paramref name="type" /> does not represent a generic type.-or-<paramref name="type" /> is not of type <see cref="T:System.Reflection.Emit.TypeBuilder" />.-or-The declaring type of <paramref name="method" /> is not a generic type definition. -or-The declaring type of <paramref name="method" /> is not the generic type definition of <paramref name="type" />.</exception>
	public static MethodInfo GetMethod(Type type, MethodInfo method)
	{
		if (!IsValidGetMethodType(type))
		{
			throw new ArgumentException("type is not TypeBuilder but " + type.GetType(), "type");
		}
		if (type is TypeBuilder && type.ContainsGenericParameters)
		{
			type = type.MakeGenericType(type.GetGenericArguments());
		}
		if (!type.IsGenericType)
		{
			throw new ArgumentException("type is not a generic type", "type");
		}
		if (!method.DeclaringType.IsGenericTypeDefinition)
		{
			throw new ArgumentException("method declaring type is not a generic type definition", "method");
		}
		if (method.DeclaringType != type.GetGenericTypeDefinition())
		{
			throw new ArgumentException("method declaring type is not the generic type definition of type", "method");
		}
		if (method == null)
		{
			throw new NullReferenceException();
		}
		MethodInfo method2 = type.GetMethod(method);
		if (method2 == null)
		{
			throw new ArgumentException($"method {method.Name} not found in type {type}");
		}
		return method2;
	}

	/// <summary>Returns the field of the specified constructed generic type that corresponds to the specified field of the generic type definition. </summary>
	/// <returns>A <see cref="T:System.Reflection.FieldInfo" /> object that represents the field of <paramref name="type" /> corresponding to <paramref name="field" />, which specifies a field belonging to the generic type definition of <paramref name="type" />.</returns>
	/// <param name="type">The constructed generic type whose field is returned.</param>
	/// <param name="field">A field on the generic type definition of <paramref name="type" />, which specifies which field of <paramref name="type" /> to return.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="type" /> does not represent a generic type. -or-<paramref name="type" /> is not of type <see cref="T:System.Reflection.Emit.TypeBuilder" />.-or-The declaring type of <paramref name="field" /> is not a generic type definition. -or-The declaring type of <paramref name="field" /> is not the generic type definition of <paramref name="type" />.</exception>
	public static FieldInfo GetField(Type type, FieldInfo field)
	{
		if (!type.IsGenericType)
		{
			throw new ArgumentException("Type is not a generic type", "type");
		}
		if (type.IsGenericTypeDefinition)
		{
			throw new ArgumentException("Type cannot be a generic type definition", "type");
		}
		if (field is FieldOnTypeBuilderInst)
		{
			throw new ArgumentException("The specified field must be declared on a generic type definition.", "field");
		}
		if (field.DeclaringType != type.GetGenericTypeDefinition())
		{
			throw new ArgumentException("field declaring type is not the generic type definition of type", "method");
		}
		FieldInfo field2 = type.GetField(field);
		if (field2 == null)
		{
			throw new Exception("field not found");
		}
		return field2;
	}

	/// <summary>Maps a set of names to a corresponding set of dispatch identifiers.</summary>
	/// <param name="riid">Reserved for future use. Must be IID_NULL.</param>
	/// <param name="rgszNames">Passed-in array of names to be mapped.</param>
	/// <param name="cNames">Count of the names to be mapped.</param>
	/// <param name="lcid">The locale context in which to interpret the names.</param>
	/// <param name="rgDispId">Caller-allocated array which receives the IDs corresponding to the names.</param>
	/// <exception cref="T:System.NotImplementedException">Late-bound access using the COM IDispatch interface is not supported.</exception>
	void _TypeBuilder.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
	{
		throw new NotImplementedException();
	}

	/// <summary>Retrieves the type information for an object, which can then be used to get the type information for an interface.</summary>
	/// <param name="iTInfo">The type information to return.</param>
	/// <param name="lcid">The locale identifier for the type information.</param>
	/// <param name="ppTInfo">Receives a pointer to the requested type information object.</param>
	/// <exception cref="T:System.NotImplementedException">Late-bound access using the COM IDispatch interface is not supported.</exception>
	void _TypeBuilder.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
	{
		throw new NotImplementedException();
	}

	/// <summary>Retrieves the number of type information interfaces that an object provides (either 0 or 1).</summary>
	/// <param name="pcTInfo">Points to a location that receives the number of type information interfaces provided by the object.</param>
	/// <exception cref="T:System.NotImplementedException">Late-bound access using the COM IDispatch interface is not supported.</exception>
	void _TypeBuilder.GetTypeInfoCount(out uint pcTInfo)
	{
		throw new NotImplementedException();
	}

	/// <summary>Provides access to properties and methods exposed by an object.</summary>
	/// <param name="dispIdMember">Identifies the member.</param>
	/// <param name="riid">Reserved for future use. Must be IID_NULL.</param>
	/// <param name="lcid">The locale context in which to interpret arguments.</param>
	/// <param name="wFlags">Flags describing the context of the call.</param>
	/// <param name="pDispParams">Pointer to a structure containing an array of arguments, an array of argument DISPIDs for named arguments, and counts for the number of elements in the arrays.</param>
	/// <param name="pVarResult">Pointer to the location where the result is to be stored.</param>
	/// <param name="pExcepInfo">Pointer to a structure that contains exception information.</param>
	/// <param name="puArgErr">The index of the first argument that has an error.</param>
	/// <exception cref="T:System.NotImplementedException">Late-bound access using the COM IDispatch interface is not supported.</exception>
	void _TypeBuilder.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
	{
		throw new NotImplementedException();
	}

	/// <summary>Gets a value that indicates whether a specified <see cref="T:System.Reflection.TypeInfo" /> object can be assigned to this object.</summary>
	/// <returns>true if <paramref name="typeInfo" /> can be assigned to this object; otherwise, false.</returns>
	/// <param name="typeInfo">The object to test.</param>
	public override bool IsAssignableFrom(TypeInfo typeInfo)
	{
		return base.IsAssignableFrom(typeInfo);
	}

	internal TypeBuilder()
	{
		ThrowStub.ThrowNotSupportedException();
	}
}
