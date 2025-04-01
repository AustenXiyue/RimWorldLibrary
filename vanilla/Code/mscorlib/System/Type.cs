using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;

namespace System;

/// <summary>Represents type declarations: class types, interface types, array types, value types, enumeration types, type parameters, generic type definitions, and open or closed constructed generic types.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
[ComDefaultInterface(typeof(_Type))]
public abstract class Type : MemberInfo, _Type, IReflect
{
	/// <summary>Represents the member filter used on attributes. This field is read-only.</summary>
	/// <filterpriority>1</filterpriority>
	public static readonly MemberFilter FilterAttribute = __Filters.Instance.FilterAttribute;

	/// <summary>Represents the case-sensitive member filter used on names. This field is read-only.</summary>
	/// <filterpriority>1</filterpriority>
	public static readonly MemberFilter FilterName = __Filters.Instance.FilterName;

	/// <summary>Represents the case-insensitive member filter used on names. This field is read-only.</summary>
	/// <filterpriority>1</filterpriority>
	public static readonly MemberFilter FilterNameIgnoreCase = __Filters.Instance.FilterIgnoreCase;

	/// <summary>Represents a missing value in the <see cref="T:System.Type" /> information. This field is read-only.</summary>
	/// <filterpriority>1</filterpriority>
	public static readonly object Missing = System.Reflection.Missing.Value;

	/// <summary>Separates names in the namespace of the <see cref="T:System.Type" />. This field is read-only.</summary>
	/// <filterpriority>1</filterpriority>
	public static readonly char Delimiter = '.';

	/// <summary>Represents an empty array of type <see cref="T:System.Type" />. This field is read-only.</summary>
	/// <filterpriority>1</filterpriority>
	public static readonly Type[] EmptyTypes = EmptyArray<Type>.Value;

	private static Binder defaultBinder;

	private const BindingFlags DefaultLookup = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;

	internal const BindingFlags DeclaredOnlyLookup = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

	internal RuntimeTypeHandle _impl;

	/// <summary>Gets a <see cref="T:System.Reflection.MemberTypes" /> value indicating that this member is a type or a nested type.</summary>
	/// <returns>A <see cref="T:System.Reflection.MemberTypes" /> value indicating that this member is a type or a nested type.</returns>
	/// <filterpriority>1</filterpriority>
	public override MemberTypes MemberType => MemberTypes.TypeInfo;

	/// <summary>Gets the type that declares the current nested type or generic type parameter.</summary>
	/// <returns>A <see cref="T:System.Type" /> object representing the enclosing type, if the current type is a nested type; or the generic type definition, if the current type is a type parameter of a generic type; or the type that declares the generic method, if the current type is a type parameter of a generic method; otherwise, null.</returns>
	/// <filterpriority>1</filterpriority>
	public override Type DeclaringType => null;

	/// <summary>Gets a <see cref="T:System.Reflection.MethodBase" /> that represents the declaring method, if the current <see cref="T:System.Type" /> represents a type parameter of a generic method.</summary>
	/// <returns>If the current <see cref="T:System.Type" /> represents a type parameter of a generic method, a <see cref="T:System.Reflection.MethodBase" /> that represents declaring method; otherwise, null.</returns>
	/// <filterpriority>1</filterpriority>
	public virtual MethodBase DeclaringMethod => null;

	/// <summary>Gets the class object that was used to obtain this member. </summary>
	/// <returns>The Type object through which this <see cref="T:System.Type" /> object was obtained. </returns>
	/// <filterpriority>1</filterpriority>
	public override Type ReflectedType => null;

	/// <summary>Gets a <see cref="T:System.Runtime.InteropServices.StructLayoutAttribute" /> that describes the layout of the current type.</summary>
	/// <returns>Gets a <see cref="T:System.Runtime.InteropServices.StructLayoutAttribute" /> that describes the gross layout features of the current type.</returns>
	/// <exception cref="T:System.NotSupportedException">The invoked method is not supported in the base class.</exception>
	/// <filterpriority>1</filterpriority>
	public virtual StructLayoutAttribute StructLayoutAttribute
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	/// <summary>Gets the GUID associated with the <see cref="T:System.Type" />.</summary>
	/// <returns>The GUID associated with the <see cref="T:System.Type" />.</returns>
	/// <filterpriority>1</filterpriority>
	public abstract Guid GUID { get; }

	/// <summary>Gets a reference to the default binder, which implements internal rules for selecting the appropriate members to be called by <see cref="M:System.Type.InvokeMember(System.String,System.Reflection.BindingFlags,System.Reflection.Binder,System.Object,System.Object[],System.Reflection.ParameterModifier[],System.Globalization.CultureInfo,System.String[])" />.</summary>
	/// <returns>A reference to the default binder used by the system.</returns>
	/// <filterpriority>1</filterpriority>
	public static Binder DefaultBinder
	{
		get
		{
			if (defaultBinder == null)
			{
				CreateBinder();
			}
			return defaultBinder;
		}
	}

	/// <summary>Gets the module (the DLL) in which the current <see cref="T:System.Type" /> is defined.</summary>
	/// <returns>The module in which the current <see cref="T:System.Type" /> is defined.</returns>
	/// <filterpriority>1</filterpriority>
	public abstract override Module Module { get; }

	/// <summary>Gets the <see cref="T:System.Reflection.Assembly" /> in which the type is declared. For generic types, gets the <see cref="T:System.Reflection.Assembly" /> in which the generic type is defined.</summary>
	/// <returns>An <see cref="T:System.Reflection.Assembly" /> instance that describes the assembly containing the current type. For generic types, the instance describes the assembly that contains the generic type definition, not the assembly that creates and uses a particular constructed type.</returns>
	/// <filterpriority>1</filterpriority>
	public abstract Assembly Assembly { get; }

	/// <summary>Gets the handle for the current <see cref="T:System.Type" />.</summary>
	/// <returns>The handle for the current <see cref="T:System.Type" />.</returns>
	/// <exception cref="T:System.NotSupportedException">The .NET Compact Framework does not currently support this property.</exception>
	/// <filterpriority>1</filterpriority>
	public virtual RuntimeTypeHandle TypeHandle
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	/// <summary>Gets the fully qualified name of the <see cref="T:System.Type" />, including the namespace of the <see cref="T:System.Type" /> but not the assembly.</summary>
	/// <returns>The fully qualified name of the <see cref="T:System.Type" />, including the namespace of the <see cref="T:System.Type" /> but not the assembly; or null if the current instance represents a generic type parameter, an array type, pointer type, or byref type based on a type parameter, or a generic type that is not a generic type definition but contains unresolved type parameters.</returns>
	/// <filterpriority>1</filterpriority>
	public abstract string FullName { get; }

	/// <summary>Gets the namespace of the <see cref="T:System.Type" />.</summary>
	/// <returns>The namespace of the <see cref="T:System.Type" />; null if the current instance has no namespace or represents a generic parameter.</returns>
	/// <filterpriority>2</filterpriority>
	public abstract string Namespace { get; }

	/// <summary>Gets the assembly-qualified name of the <see cref="T:System.Type" />, which includes the name of the assembly from which the <see cref="T:System.Type" /> was loaded.</summary>
	/// <returns>The assembly-qualified name of the <see cref="T:System.Type" />, which includes the name of the assembly from which the <see cref="T:System.Type" /> was loaded, or null if the current instance represents a generic type parameter.</returns>
	/// <filterpriority>2</filterpriority>
	public abstract string AssemblyQualifiedName { get; }

	/// <summary>Gets the type from which the current <see cref="T:System.Type" /> directly inherits.</summary>
	/// <returns>The <see cref="T:System.Type" /> from which the current <see cref="T:System.Type" /> directly inherits, or null if the current Type represents the <see cref="T:System.Object" /> class or an interface.</returns>
	/// <filterpriority>2</filterpriority>
	public abstract Type BaseType { get; }

	/// <summary>Gets the initializer for the <see cref="T:System.Type" />.</summary>
	/// <returns>A <see cref="T:System.Reflection.ConstructorInfo" /> containing the name of the class constructor for the <see cref="T:System.Type" />.</returns>
	/// <filterpriority>2</filterpriority>
	[ComVisible(true)]
	public ConstructorInfo TypeInitializer => GetConstructorImpl(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, CallingConventions.Any, EmptyTypes, null);

	/// <summary>Gets a value indicating whether the current <see cref="T:System.Type" /> object represents a type whose definition is nested inside the definition of another type.</summary>
	/// <returns>true if the <see cref="T:System.Type" /> is nested inside another type; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsNested => DeclaringType != null;

	/// <summary>Gets the attributes associated with the <see cref="T:System.Type" />.</summary>
	/// <returns>A <see cref="T:System.Reflection.TypeAttributes" /> object representing the attribute set of the <see cref="T:System.Type" />, unless the <see cref="T:System.Type" /> represents a generic type parameter, in which case the value is unspecified. </returns>
	/// <filterpriority>2</filterpriority>
	public TypeAttributes Attributes => GetAttributeFlagsImpl();

	/// <summary>Gets a combination of <see cref="T:System.Reflection.GenericParameterAttributes" /> flags that describe the covariance and special constraints of the current generic type parameter. </summary>
	/// <returns>A bitwise combination of <see cref="T:System.Reflection.GenericParameterAttributes" /> values that describes the covariance and special constraints of the current generic type parameter.</returns>
	/// <exception cref="T:System.InvalidOperationException">The current <see cref="T:System.Type" /> object is not a generic type parameter. That is, the <see cref="P:System.Type.IsGenericParameter" /> property returns false.</exception>
	/// <exception cref="T:System.NotSupportedException">The invoked method is not supported in the base class.</exception>
	/// <filterpriority>1</filterpriority>
	public virtual GenericParameterAttributes GenericParameterAttributes
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	/// <summary>Gets a value indicating whether the <see cref="T:System.Type" /> can be accessed by code outside the assembly.</summary>
	/// <returns>true if the current <see cref="T:System.Type" /> is a public type or a public nested type such that all the enclosing types are public; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsVisible
	{
		get
		{
			if (IsGenericParameter)
			{
				return true;
			}
			if (HasElementType)
			{
				return GetElementType().IsVisible;
			}
			Type type = this;
			while (type.IsNested)
			{
				if (!type.IsNestedPublic)
				{
					return false;
				}
				type = type.DeclaringType;
			}
			if (!type.IsPublic)
			{
				return false;
			}
			if (IsGenericType && !IsGenericTypeDefinition)
			{
				Type[] genericArguments = GetGenericArguments();
				for (int i = 0; i < genericArguments.Length; i++)
				{
					if (!genericArguments[i].IsVisible)
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	/// <summary>Gets a value indicating whether the <see cref="T:System.Type" /> is not declared public.</summary>
	/// <returns>true if the <see cref="T:System.Type" /> is not declared public and is not a nested type; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsNotPublic => (GetAttributeFlagsImpl() & TypeAttributes.VisibilityMask) == 0;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Type" /> is declared public.</summary>
	/// <returns>true if the <see cref="T:System.Type" /> is declared public and is not a nested type; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsPublic => (GetAttributeFlagsImpl() & TypeAttributes.VisibilityMask) == TypeAttributes.Public;

	/// <summary>Gets a value indicating whether a class is nested and declared public.</summary>
	/// <returns>true if the class is nested and declared public; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsNestedPublic => (GetAttributeFlagsImpl() & TypeAttributes.VisibilityMask) == TypeAttributes.NestedPublic;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Type" /> is nested and declared private.</summary>
	/// <returns>true if the <see cref="T:System.Type" /> is nested and declared private; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsNestedPrivate => (GetAttributeFlagsImpl() & TypeAttributes.VisibilityMask) == TypeAttributes.NestedPrivate;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Type" /> is nested and visible only within its own family.</summary>
	/// <returns>true if the <see cref="T:System.Type" /> is nested and visible only within its own family; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsNestedFamily => (GetAttributeFlagsImpl() & TypeAttributes.VisibilityMask) == TypeAttributes.NestedFamily;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Type" /> is nested and visible only within its own assembly.</summary>
	/// <returns>true if the <see cref="T:System.Type" /> is nested and visible only within its own assembly; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsNestedAssembly => (GetAttributeFlagsImpl() & TypeAttributes.VisibilityMask) == TypeAttributes.NestedAssembly;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Type" /> is nested and visible only to classes that belong to both its own family and its own assembly.</summary>
	/// <returns>true if the <see cref="T:System.Type" /> is nested and visible only to classes that belong to both its own family and its own assembly; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsNestedFamANDAssem => (GetAttributeFlagsImpl() & TypeAttributes.VisibilityMask) == TypeAttributes.NestedFamANDAssem;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Type" /> is nested and visible only to classes that belong to either its own family or to its own assembly.</summary>
	/// <returns>true if the <see cref="T:System.Type" /> is nested and visible only to classes that belong to its own family or to its own assembly; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsNestedFamORAssem => (GetAttributeFlagsImpl() & TypeAttributes.VisibilityMask) == TypeAttributes.VisibilityMask;

	/// <summary>Gets a value indicating whether the fields of the current type are laid out automatically by the common language runtime.</summary>
	/// <returns>true if the <see cref="P:System.Type.Attributes" /> property of the current type includes <see cref="F:System.Reflection.TypeAttributes.AutoLayout" />; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsAutoLayout => (GetAttributeFlagsImpl() & TypeAttributes.LayoutMask) == 0;

	/// <summary>Gets a value indicating whether the fields of the current type are laid out sequentially, in the order that they were defined or emitted to the metadata.</summary>
	/// <returns>true if the <see cref="P:System.Type.Attributes" /> property of the current type includes <see cref="F:System.Reflection.TypeAttributes.SequentialLayout" />; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsLayoutSequential => (GetAttributeFlagsImpl() & TypeAttributes.LayoutMask) == TypeAttributes.SequentialLayout;

	/// <summary>Gets a value indicating whether the fields of the current type are laid out at explicitly specified offsets.</summary>
	/// <returns>true if the <see cref="P:System.Type.Attributes" /> property of the current type includes <see cref="F:System.Reflection.TypeAttributes.ExplicitLayout" />; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsExplicitLayout => (GetAttributeFlagsImpl() & TypeAttributes.LayoutMask) == TypeAttributes.ExplicitLayout;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Type" /> is a class; that is, not a value type or interface.</summary>
	/// <returns>true if the <see cref="T:System.Type" /> is a class; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsClass
	{
		get
		{
			if ((GetAttributeFlagsImpl() & TypeAttributes.ClassSemanticsMask) == 0)
			{
				return !IsValueType;
			}
			return false;
		}
	}

	/// <summary>Gets a value indicating whether the <see cref="T:System.Type" /> is an interface; that is, not a class or a value type.</summary>
	/// <returns>true if the <see cref="T:System.Type" /> is an interface; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsInterface
	{
		[SecuritySafeCritical]
		get
		{
			RuntimeType runtimeType = this as RuntimeType;
			if (runtimeType != null)
			{
				return RuntimeTypeHandle.IsInterface(runtimeType);
			}
			return (GetAttributeFlagsImpl() & TypeAttributes.ClassSemanticsMask) == TypeAttributes.ClassSemanticsMask;
		}
	}

	/// <summary>Gets a value indicating whether the <see cref="T:System.Type" /> is a value type.</summary>
	/// <returns>true if the <see cref="T:System.Type" /> is a value type; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsValueType => IsValueTypeImpl();

	/// <summary>Gets a value indicating whether the <see cref="T:System.Type" /> is abstract and must be overridden.</summary>
	/// <returns>true if the <see cref="T:System.Type" /> is abstract; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsAbstract => (GetAttributeFlagsImpl() & TypeAttributes.Abstract) != 0;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Type" /> is declared sealed.</summary>
	/// <returns>true if the <see cref="T:System.Type" /> is declared sealed; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsSealed => (GetAttributeFlagsImpl() & TypeAttributes.Sealed) != 0;

	/// <summary>Gets a value indicating whether the current <see cref="T:System.Type" /> represents an enumeration.</summary>
	/// <returns>true if the current <see cref="T:System.Type" /> represents an enumeration; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual bool IsEnum => IsSubclassOf(RuntimeType.EnumType);

	/// <summary>Gets a value indicating whether the <see cref="T:System.Type" /> has a name that requires special handling.</summary>
	/// <returns>true if the <see cref="T:System.Type" /> has a name that requires special handling; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsSpecialName => (GetAttributeFlagsImpl() & TypeAttributes.SpecialName) != 0;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Type" /> has a <see cref="T:System.Runtime.InteropServices.ComImportAttribute" /> attribute applied, indicating that it was imported from a COM type library.</summary>
	/// <returns>true if the <see cref="T:System.Type" /> has a <see cref="T:System.Runtime.InteropServices.ComImportAttribute" />; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsImport => (GetAttributeFlagsImpl() & TypeAttributes.Import) != 0;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Type" /> is serializable.</summary>
	/// <returns>true if the <see cref="T:System.Type" /> is serializable; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual bool IsSerializable
	{
		get
		{
			if ((GetAttributeFlagsImpl() & TypeAttributes.Serializable) != 0)
			{
				return true;
			}
			RuntimeType runtimeType = UnderlyingSystemType as RuntimeType;
			if (runtimeType != null)
			{
				return runtimeType.IsSpecialSerializableType();
			}
			return false;
		}
	}

	/// <summary>Gets a value indicating whether the string format attribute AnsiClass is selected for the <see cref="T:System.Type" />.</summary>
	/// <returns>true if the string format attribute AnsiClass is selected for the <see cref="T:System.Type" />; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsAnsiClass => (GetAttributeFlagsImpl() & TypeAttributes.StringFormatMask) == 0;

	/// <summary>Gets a value indicating whether the string format attribute UnicodeClass is selected for the <see cref="T:System.Type" />.</summary>
	/// <returns>true if the string format attribute UnicodeClass is selected for the <see cref="T:System.Type" />; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsUnicodeClass => (GetAttributeFlagsImpl() & TypeAttributes.StringFormatMask) == TypeAttributes.UnicodeClass;

	/// <summary>Gets a value indicating whether the string format attribute AutoClass is selected for the <see cref="T:System.Type" />.</summary>
	/// <returns>true if the string format attribute AutoClass is selected for the <see cref="T:System.Type" />; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsAutoClass => (GetAttributeFlagsImpl() & TypeAttributes.StringFormatMask) == TypeAttributes.AutoClass;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Type" /> is an array.</summary>
	/// <returns>true if the <see cref="T:System.Type" /> is an array; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsArray => IsArrayImpl();

	internal virtual bool IsSzArray => false;

	/// <summary>Gets a value indicating whether the current type is a generic type.</summary>
	/// <returns>true if the current type is a generic type; otherwise, false.</returns>
	public virtual bool IsGenericType => false;

	/// <summary>Gets a value indicating whether the current <see cref="T:System.Type" /> represents a generic type definition, from which other generic types can be constructed.</summary>
	/// <returns>true if the <see cref="T:System.Type" /> object represents a generic type definition; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual bool IsGenericTypeDefinition => false;

	/// <summary>Gets a value that indicates whether this object represents a constructed generic type. You can create instances of a constructed generic type. </summary>
	/// <returns>true if this object represents a constructed generic type; otherwise, false.</returns>
	public virtual bool IsConstructedGenericType
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets a value indicating whether the current <see cref="T:System.Type" /> represents a type parameter in the definition of a generic type or method.</summary>
	/// <returns>true if the <see cref="T:System.Type" /> object represents a type parameter of a generic type definition or generic method definition; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual bool IsGenericParameter => false;

	/// <summary>Gets the position of the type parameter in the type parameter list of the generic type or method that declared the parameter, when the <see cref="T:System.Type" /> object represents a type parameter of a generic type or a generic method.</summary>
	/// <returns>The position of a type parameter in the type parameter list of the generic type or method that defines the parameter. Position numbers begin at 0.</returns>
	/// <exception cref="T:System.InvalidOperationException">The current type does not represent a type parameter. That is, <see cref="P:System.Type.IsGenericParameter" /> returns false. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual int GenericParameterPosition
	{
		get
		{
			throw new InvalidOperationException(Environment.GetResourceString("Method may only be called on a Type for which Type.IsGenericParameter is true."));
		}
	}

	/// <summary>Gets a value indicating whether the current <see cref="T:System.Type" /> object has type parameters that have not been replaced by specific types.</summary>
	/// <returns>true if the <see cref="T:System.Type" /> object is itself a generic type parameter or has type parameters for which specific types have not been supplied; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual bool ContainsGenericParameters
	{
		get
		{
			if (HasElementType)
			{
				return GetRootElementType().ContainsGenericParameters;
			}
			if (IsGenericParameter)
			{
				return true;
			}
			if (!IsGenericType)
			{
				return false;
			}
			Type[] genericArguments = GetGenericArguments();
			for (int i = 0; i < genericArguments.Length; i++)
			{
				if (genericArguments[i].ContainsGenericParameters)
				{
					return true;
				}
			}
			return false;
		}
	}

	/// <summary>Gets a value indicating whether the <see cref="T:System.Type" /> is passed by reference.</summary>
	/// <returns>true if the <see cref="T:System.Type" /> is passed by reference; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsByRef => IsByRefImpl();

	/// <summary>Gets a value indicating whether the <see cref="T:System.Type" /> is a pointer.</summary>
	/// <returns>true if the <see cref="T:System.Type" /> is a pointer; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsPointer => IsPointerImpl();

	/// <summary>Gets a value indicating whether the <see cref="T:System.Type" /> is one of the primitive types.</summary>
	/// <returns>true if the <see cref="T:System.Type" /> is one of the primitive types; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsPrimitive => IsPrimitiveImpl();

	/// <summary>Gets a value indicating whether the <see cref="T:System.Type" /> is a COM object.</summary>
	/// <returns>true if the <see cref="T:System.Type" /> is a COM object; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsCOMObject => IsCOMObjectImpl();

	internal bool IsWindowsRuntimeObject => IsWindowsRuntimeObjectImpl();

	internal bool IsExportedToWindowsRuntime => IsExportedToWindowsRuntimeImpl();

	/// <summary>Gets a value indicating whether the current <see cref="T:System.Type" /> encompasses or refers to another type; that is, whether the current <see cref="T:System.Type" /> is an array, a pointer, or is passed by reference.</summary>
	/// <returns>true if the <see cref="T:System.Type" /> is an array, a pointer, or is passed by reference; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool HasElementType => HasElementTypeImpl();

	/// <summary>Gets a value indicating whether the <see cref="T:System.Type" /> can be hosted in a context.</summary>
	/// <returns>true if the <see cref="T:System.Type" /> can be hosted in a context; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsContextful => IsContextfulImpl();

	/// <summary>Gets a value indicating whether the <see cref="T:System.Type" /> is marshaled by reference.</summary>
	/// <returns>true if the <see cref="T:System.Type" /> is marshaled by reference; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsMarshalByRef => IsMarshalByRefImpl();

	internal bool HasProxyAttribute => HasProxyAttributeImpl();

	/// <summary>Gets an array of the generic type arguments for this type.</summary>
	/// <returns>An array of the generic type arguments for this type.</returns>
	public virtual Type[] GenericTypeArguments
	{
		get
		{
			if (IsGenericType && !IsGenericTypeDefinition)
			{
				return GetGenericArguments();
			}
			return EmptyTypes;
		}
	}

	/// <summary>Gets a value that indicates whether the current type is security-critical or security-safe-critical at the current trust level, and therefore can perform critical operations. </summary>
	/// <returns>true if the current type is security-critical or security-safe-critical at the current trust level; false if it is transparent. </returns>
	public virtual bool IsSecurityCritical
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets a value that indicates whether the current type is security-safe-critical at the current trust level; that is, whether it can perform critical operations and can be accessed by transparent code. </summary>
	/// <returns>true if the current type is security-safe-critical at the current trust level; false if it is security-critical or transparent.</returns>
	public virtual bool IsSecuritySafeCritical
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets a value that indicates whether the current type is transparent at the current trust level, and therefore cannot perform critical operations.</summary>
	/// <returns>true if the type is security-transparent at the current trust level; otherwise, false.</returns>
	public virtual bool IsSecurityTransparent
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	internal bool NeedsReflectionSecurityCheck
	{
		get
		{
			if (!IsVisible)
			{
				return true;
			}
			if (IsSecurityCritical && !IsSecuritySafeCritical)
			{
				return true;
			}
			if (IsGenericType)
			{
				Type[] genericArguments = GetGenericArguments();
				for (int i = 0; i < genericArguments.Length; i++)
				{
					if (genericArguments[i].NeedsReflectionSecurityCheck)
					{
						return true;
					}
				}
			}
			else if (IsArray || IsPointer)
			{
				return GetElementType().NeedsReflectionSecurityCheck;
			}
			return false;
		}
	}

	/// <summary>Indicates the type provided by the common language runtime that represents this type.</summary>
	/// <returns>The underlying system type for the <see cref="T:System.Type" />.</returns>
	/// <filterpriority>2</filterpriority>
	public abstract Type UnderlyingSystemType { get; }

	public virtual bool IsSZArray
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	internal virtual bool IsUserType => true;

	/// <summary>Initializes a new instance of the <see cref="T:System.Type" /> class.</summary>
	protected Type()
	{
	}

	/// <summary>Gets the type with the specified name, optionally providing custom methods to resolve the assembly and the type.</summary>
	/// <returns>The type with the specified name, or null if the type is not found. </returns>
	/// <param name="typeName">The name of the type to get. If the <paramref name="typeResolver" /> parameter is provided, the type name can be any string that <paramref name="typeResolver" /> is capable of resolving. If the <paramref name="assemblyResolver" /> parameter is provided or if standard type resolution is used, <paramref name="typeName" /> must be an assembly-qualified name (see <see cref="P:System.Type.AssemblyQualifiedName" />), unless the type is in the currently executing assembly or in Mscorlib.dll, in which case it is sufficient to supply the type name qualified by its namespace.</param>
	/// <param name="assemblyResolver">A method that locates and returns the assembly that is specified in <paramref name="typeName" />. The assembly name is passed to <paramref name="assemblyResolver" /> as an <see cref="T:System.Reflection.AssemblyName" /> object. If <paramref name="typeName" /> does not contain the name of an assembly, <paramref name="assemblyResolver" /> is not called. If <paramref name="assemblyResolver" /> is not supplied, standard assembly resolution is performed. Caution   Do not pass methods from unknown or untrusted callers. Doing so could result in elevation of privilege for malicious code. Use only methods that you provide or that you are familiar with. </param>
	/// <param name="typeResolver">A method that locates and returns the type that is specified by <paramref name="typeName" /> from the assembly that is returned by <paramref name="assemblyResolver" /> or by standard assembly resolution. If no assembly is provided, the <paramref name="typeResolver" /> method can provide one. The method also takes a parameter that specifies whether to perform a case-insensitive search; false is passed to that parameter. Caution   Do not pass methods from unknown or untrusted callers. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="typeName" /> is null. </exception>
	/// <exception cref="T:System.Reflection.TargetInvocationException">A class initializer is invoked and throws an exception. </exception>
	/// <exception cref="T:System.ArgumentException">An error occurs when <paramref name="typeName" /> is parsed into a type name and an assembly name (for example, when the simple type name includes an unescaped special character).-or-<paramref name="typeName" /> represents a generic type that has a pointer type, a ByRef type, or <see cref="T:System.Void" /> as one of its type arguments.-or-<paramref name="typeName" /> represents a generic type that has an incorrect number of type arguments.-or-<paramref name="typeName" /> represents a generic type, and one of its type arguments does not satisfy the constraints for the corresponding type parameter.</exception>
	/// <exception cref="T:System.TypeLoadException">
	///   <paramref name="typeName" /> represents an array of <see cref="T:System.TypedReference" />. </exception>
	/// <exception cref="T:System.IO.FileLoadException">The assembly or one of its dependencies was found, but could not be loaded. -or-<paramref name="typeName" /> contains an invalid assembly name.-or-<paramref name="typeName" /> is a valid assembly name without a type name.</exception>
	/// <exception cref="T:System.BadImageFormatException">The assembly or one of its dependencies is not valid. -or-The assembly was compiled with a later version of the common language runtime than the version that is currently loaded.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public static Type GetType(string typeName, Func<AssemblyName, Assembly> assemblyResolver, Func<Assembly, string, bool, Type> typeResolver)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return TypeNameParser.GetType(typeName, assemblyResolver, typeResolver, throwOnError: false, ignoreCase: false, ref stackMark);
	}

	/// <summary>Gets the type with the specified name, specifying whether to throw an exception if the type is not found, and optionally providing custom methods to resolve the assembly and the type.</summary>
	/// <returns>The type with the specified name. If the type is not found, the <paramref name="throwOnError" /> parameter specifies whether null is returned or an exception is thrown. In some cases, an exception is thrown regardless of the value of <paramref name="throwOnError" />. See the Exceptions section. </returns>
	/// <param name="typeName">The name of the type to get. If the <paramref name="typeResolver" /> parameter is provided, the type name can be any string that <paramref name="typeResolver" /> is capable of resolving. If the <paramref name="assemblyResolver" /> parameter is provided or if standard type resolution is used, <paramref name="typeName" /> must be an assembly-qualified name (see <see cref="P:System.Type.AssemblyQualifiedName" />), unless the type is in the currently executing assembly or in Mscorlib.dll, in which case it is sufficient to supply the type name qualified by its namespace.</param>
	/// <param name="assemblyResolver">A method that locates and returns the assembly that is specified in <paramref name="typeName" />. The assembly name is passed to <paramref name="assemblyResolver" /> as an <see cref="T:System.Reflection.AssemblyName" /> object. If <paramref name="typeName" /> does not contain the name of an assembly, <paramref name="assemblyResolver" /> is not called. If <paramref name="assemblyResolver" /> is not supplied, standard assembly resolution is performed. Caution   Do not pass methods from unknown or untrusted callers. Doing so could result in elevation of privilege for malicious code. Use only methods that you provide or that you are familiar with. </param>
	/// <param name="typeResolver">A method that locates and returns the type that is specified by <paramref name="typeName" /> from the assembly that is returned by <paramref name="assemblyResolver" /> or by standard assembly resolution. If no assembly is provided, the method can provide one. The method also takes a parameter that specifies whether to perform a case-insensitive search; false is passed to that parameter. Caution   Do not pass methods from unknown or untrusted callers. </param>
	/// <param name="throwOnError">true to throw an exception if the type cannot be found; false to return null. Specifying false also suppresses some other exception conditions, but not all of them. See the Exceptions section.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="typeName" /> is null. </exception>
	/// <exception cref="T:System.Reflection.TargetInvocationException">A class initializer is invoked and throws an exception. </exception>
	/// <exception cref="T:System.TypeLoadException">
	///   <paramref name="throwOnError" /> is true and the type is not found. -or-<paramref name="throwOnError" /> is true and <paramref name="typeName" /> contains invalid characters, such as an embedded tab.-or-<paramref name="throwOnError" /> is true and <paramref name="typeName" /> is an empty string.-or-<paramref name="throwOnError" /> is true and <paramref name="typeName" /> represents an array type with an invalid size. -or-<paramref name="typeName" /> represents an array of <see cref="T:System.TypedReference" />. </exception>
	/// <exception cref="T:System.ArgumentException">An error occurs when <paramref name="typeName" /> is parsed into a type name and an assembly name (for example, when the simple type name includes an unescaped special character).-or-<paramref name="throwOnError" /> is true and <paramref name="typeName" /> contains invalid syntax (for example, "MyType[,*,]").-or- <paramref name="typeName" /> represents a generic type that has a pointer type, a ByRef type, or <see cref="T:System.Void" /> as one of its type arguments.-or-<paramref name="typeName" /> represents a generic type that has an incorrect number of type arguments.-or-<paramref name="typeName" /> represents a generic type, and one of its type arguments does not satisfy the constraints for the corresponding type parameter.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="throwOnError" /> is true and the assembly or one of its dependencies was not found. -or-<paramref name="typeName" /> contains an invalid assembly name.-or-<paramref name="typeName" /> is a valid assembly name without a type name.</exception>
	/// <exception cref="T:System.IO.FileLoadException">The assembly or one of its dependencies was found, but could not be loaded. </exception>
	/// <exception cref="T:System.BadImageFormatException">The assembly or one of its dependencies is not valid. -or-The assembly was compiled with a later version of the common language runtime than the version that is currently loaded.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public static Type GetType(string typeName, Func<AssemblyName, Assembly> assemblyResolver, Func<Assembly, string, bool, Type> typeResolver, bool throwOnError)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return TypeNameParser.GetType(typeName, assemblyResolver, typeResolver, throwOnError, ignoreCase: false, ref stackMark);
	}

	/// <summary>Gets the type with the specified name, specifying whether to perform a case-sensitive search and whether to throw an exception if the type is not found, and optionally providing custom methods to resolve the assembly and the type.</summary>
	/// <returns>The type with the specified name. If the type is not found, the <paramref name="throwOnError" /> parameter specifies whether null is returned or an exception is thrown. In some cases, an exception is thrown regardless of the value of <paramref name="throwOnError" />. See the Exceptions section. </returns>
	/// <param name="typeName">The name of the type to get. If the <paramref name="typeResolver" /> parameter is provided, the type name can be any string that <paramref name="typeResolver" /> is capable of resolving. If the <paramref name="assemblyResolver" /> parameter is provided or if standard type resolution is used, <paramref name="typeName" /> must be an assembly-qualified name (see <see cref="P:System.Type.AssemblyQualifiedName" />), unless the type is in the currently executing assembly or in Mscorlib.dll, in which case it is sufficient to supply the type name qualified by its namespace.</param>
	/// <param name="assemblyResolver">A method that locates and returns the assembly that is specified in <paramref name="typeName" />. The assembly name is passed to <paramref name="assemblyResolver" /> as an <see cref="T:System.Reflection.AssemblyName" /> object. If <paramref name="typeName" /> does not contain the name of an assembly, <paramref name="assemblyResolver" /> is not called. If <paramref name="assemblyResolver" /> is not supplied, standard assembly resolution is performed. Caution   Do not pass methods from unknown or untrusted callers. Doing so could result in elevation of privilege for malicious code. Use only methods that you provide or that you are familiar with.</param>
	/// <param name="typeResolver">A method that locates and returns the type that is specified by <paramref name="typeName" /> from the assembly that is returned by <paramref name="assemblyResolver" /> or by standard assembly resolution. If no assembly is provided, the method can provide one. The method also takes a parameter that specifies whether to perform a case-insensitive search; the value of <paramref name="ignoreCase" /> is passed to that parameter. Caution   Do not pass methods from unknown or untrusted callers. </param>
	/// <param name="throwOnError">true to throw an exception if the type cannot be found; false to return null. Specifying false also suppresses some other exception conditions, but not all of them. See the Exceptions section.</param>
	/// <param name="ignoreCase">true to perform a case-insensitive search for <paramref name="typeName" />, false to perform a case-sensitive search for <paramref name="typeName" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="typeName" /> is null. </exception>
	/// <exception cref="T:System.Reflection.TargetInvocationException">A class initializer is invoked and throws an exception. </exception>
	/// <exception cref="T:System.TypeLoadException">
	///   <paramref name="throwOnError" /> is true and the type is not found. -or-<paramref name="throwOnError" /> is true and <paramref name="typeName" /> contains invalid characters, such as an embedded tab.-or-<paramref name="throwOnError" /> is true and <paramref name="typeName" /> is an empty string.-or-<paramref name="throwOnError" /> is true and <paramref name="typeName" /> represents an array type with an invalid size. -or-<paramref name="typeName" /> represents an array of <see cref="T:System.TypedReference" />. </exception>
	/// <exception cref="T:System.ArgumentException">An error occurs when <paramref name="typeName" /> is parsed into a type name and an assembly name (for example, when the simple type name includes an unescaped special character).-or-<paramref name="throwOnError" /> is true and <paramref name="typeName" /> contains invalid syntax (for example, "MyType[,*,]").-or- <paramref name="typeName" /> represents a generic type that has a pointer type, a ByRef type, or <see cref="T:System.Void" /> as one of its type arguments.-or-<paramref name="typeName" /> represents a generic type that has an incorrect number of type arguments.-or-<paramref name="typeName" /> represents a generic type, and one of its type arguments does not satisfy the constraints for the corresponding type parameter.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="throwOnError" /> is true and the assembly or one of its dependencies was not found. </exception>
	/// <exception cref="T:System.IO.FileLoadException">The assembly or one of its dependencies was found, but could not be loaded. -or-<paramref name="typeName" /> contains an invalid assembly name.-or-<paramref name="typeName" /> is a valid assembly name without a type name.</exception>
	/// <exception cref="T:System.BadImageFormatException">The assembly or one of its dependencies is not valid. -or-The assembly was compiled with a later version of the common language runtime than the version that is currently loaded.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public static Type GetType(string typeName, Func<AssemblyName, Assembly> assemblyResolver, Func<Assembly, string, bool, Type> typeResolver, bool throwOnError, bool ignoreCase)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return TypeNameParser.GetType(typeName, assemblyResolver, typeResolver, throwOnError, ignoreCase, ref stackMark);
	}

	/// <summary>Returns a <see cref="T:System.Type" /> object that represents a pointer to the current type.</summary>
	/// <returns>A <see cref="T:System.Type" /> object that represents a pointer to the current type.</returns>
	/// <exception cref="T:System.NotSupportedException">The invoked method is not supported in the base class.</exception>
	/// <exception cref="T:System.TypeLoadException">The current type is <see cref="T:System.TypedReference" />.-or-The current type is a ByRef type. That is, <see cref="P:System.Type.IsByRef" /> returns true. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual Type MakePointerType()
	{
		throw new NotSupportedException();
	}

	/// <summary>Returns a <see cref="T:System.Type" /> object that represents the current type when passed as a ref parameter (ByRef parameter in Visual Basic).</summary>
	/// <returns>A <see cref="T:System.Type" /> object that represents the current type when passed as a ref parameter (ByRef parameter in Visual Basic).</returns>
	/// <exception cref="T:System.NotSupportedException">The invoked method is not supported in the base class.</exception>
	/// <exception cref="T:System.TypeLoadException">The current type is <see cref="T:System.TypedReference" />.-or-The current type is a ByRef type. That is, <see cref="P:System.Type.IsByRef" /> returns true. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual Type MakeByRefType()
	{
		throw new NotSupportedException();
	}

	/// <summary>Returns a <see cref="T:System.Type" /> object representing a one-dimensional array of the current type, with a lower bound of zero.</summary>
	/// <returns>A <see cref="T:System.Type" /> object representing a one-dimensional array of the current type, with a lower bound of zero.</returns>
	/// <exception cref="T:System.NotSupportedException">The invoked method is not supported in the base class. Derived classes must provide an implementation.</exception>
	/// <exception cref="T:System.TypeLoadException">The current type is <see cref="T:System.TypedReference" />.-or-The current type is a ByRef type. That is, <see cref="P:System.Type.IsByRef" /> returns true. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual Type MakeArrayType()
	{
		throw new NotSupportedException();
	}

	/// <summary>Returns a <see cref="T:System.Type" /> object representing an array of the current type, with the specified number of dimensions.</summary>
	/// <returns>An object representing an array of the current type, with the specified number of dimensions.</returns>
	/// <param name="rank">The number of dimensions for the array. This number must be less than or equal to 32.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">
	///   <paramref name="rank" /> is invalid. For example, 0 or negative.</exception>
	/// <exception cref="T:System.NotSupportedException">The invoked method is not supported in the base class.</exception>
	/// <exception cref="T:System.TypeLoadException">The current type is <see cref="T:System.TypedReference" />.-or-The current type is a ByRef type. That is, <see cref="P:System.Type.IsByRef" /> returns true. -or-<paramref name="rank" /> is greater than 32.</exception>
	/// <filterpriority>2</filterpriority>
	public virtual Type MakeArrayType(int rank)
	{
		throw new NotSupportedException();
	}

	/// <summary>Gets the type associated with the specified program identifier (ProgID), returning null if an error is encountered while loading the <see cref="T:System.Type" />.</summary>
	/// <returns>The type associated with the specified ProgID, if <paramref name="progID" /> is a valid entry in the registry and a type is associated with it; otherwise, null.</returns>
	/// <param name="progID">The ProgID of the type to get. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="progID" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[SecurityCritical]
	public static Type GetTypeFromProgID(string progID)
	{
		return RuntimeType.GetTypeFromProgIDImpl(progID, null, throwOnError: false);
	}

	/// <summary>Gets the type associated with the specified program identifier (ProgID), specifying whether to throw an exception if an error occurs while loading the type.</summary>
	/// <returns>The type associated with the specified program identifier (ProgID), if <paramref name="progID" /> is a valid entry in the registry and a type is associated with it; otherwise, null.</returns>
	/// <param name="progID">The ProgID of the type to get. </param>
	/// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="progID" /> is null. </exception>
	/// <exception cref="T:System.Runtime.InteropServices.COMException">The specified ProgID is not registered. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[SecurityCritical]
	public static Type GetTypeFromProgID(string progID, bool throwOnError)
	{
		return RuntimeType.GetTypeFromProgIDImpl(progID, null, throwOnError);
	}

	/// <summary>Gets the type associated with the specified program identifier (progID) from the specified server, returning null if an error is encountered while loading the type.</summary>
	/// <returns>The type associated with the specified program identifier (progID), if <paramref name="progID" /> is a valid entry in the registry and a type is associated with it; otherwise, null.</returns>
	/// <param name="progID">The progID of the type to get. </param>
	/// <param name="server">The server from which to load the type. If the server name is null, this method automatically reverts to the local machine. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="prodID" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[SecurityCritical]
	public static Type GetTypeFromProgID(string progID, string server)
	{
		return RuntimeType.GetTypeFromProgIDImpl(progID, server, throwOnError: false);
	}

	/// <summary>Gets the type associated with the specified program identifier (progID) from the specified server, specifying whether to throw an exception if an error occurs while loading the type.</summary>
	/// <returns>The type associated with the specified program identifier (progID), if <paramref name="progID" /> is a valid entry in the registry and a type is associated with it; otherwise, null.</returns>
	/// <param name="progID">The progID of the <see cref="T:System.Type" /> to get. </param>
	/// <param name="server">The server from which to load the type. If the server name is null, this method automatically reverts to the local machine. </param>
	/// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="progID" /> is null. </exception>
	/// <exception cref="T:System.Runtime.InteropServices.COMException">The specified progID is not registered. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[SecurityCritical]
	public static Type GetTypeFromProgID(string progID, string server, bool throwOnError)
	{
		return RuntimeType.GetTypeFromProgIDImpl(progID, server, throwOnError);
	}

	/// <summary>Gets the type associated with the specified class identifier (CLSID).</summary>
	/// <returns>System.__ComObject regardless of whether the CLSID is valid.</returns>
	/// <param name="clsid">The CLSID of the type to get. </param>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public static Type GetTypeFromCLSID(Guid clsid)
	{
		return RuntimeType.GetTypeFromCLSIDImpl(clsid, null, throwOnError: false);
	}

	/// <summary>Gets the type associated with the specified class identifier (CLSID), specifying whether to throw an exception if an error occurs while loading the type.</summary>
	/// <returns>System.__ComObject regardless of whether the CLSID is valid.</returns>
	/// <param name="clsid">The CLSID of the type to get. </param>
	/// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs. </param>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public static Type GetTypeFromCLSID(Guid clsid, bool throwOnError)
	{
		return RuntimeType.GetTypeFromCLSIDImpl(clsid, null, throwOnError);
	}

	/// <summary>Gets the type associated with the specified class identifier (CLSID) from the specified server.</summary>
	/// <returns>System.__ComObject regardless of whether the CLSID is valid.</returns>
	/// <param name="clsid">The CLSID of the type to get. </param>
	/// <param name="server">The server from which to load the type. If the server name is null, this method automatically reverts to the local machine. </param>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public static Type GetTypeFromCLSID(Guid clsid, string server)
	{
		return RuntimeType.GetTypeFromCLSIDImpl(clsid, server, throwOnError: false);
	}

	/// <summary>Gets the type associated with the specified class identifier (CLSID) from the specified server, specifying whether to throw an exception if an error occurs while loading the type.</summary>
	/// <returns>System.__ComObject regardless of whether the CLSID is valid.</returns>
	/// <param name="clsid">The CLSID of the type to get. </param>
	/// <param name="server">The server from which to load the type. If the server name is null, this method automatically reverts to the local machine. </param>
	/// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs. </param>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public static Type GetTypeFromCLSID(Guid clsid, string server, bool throwOnError)
	{
		return RuntimeType.GetTypeFromCLSIDImpl(clsid, server, throwOnError);
	}

	/// <summary>Gets the underlying type code of the specified <see cref="T:System.Type" />.</summary>
	/// <returns>The code of the underlying type, or <see cref="F:System.TypeCode.Empty" /> if <paramref name="type" /> is null.</returns>
	/// <param name="type">The type whose underlying type code to get. </param>
	/// <filterpriority>1</filterpriority>
	public static TypeCode GetTypeCode(Type type)
	{
		if (type == null)
		{
			return TypeCode.Empty;
		}
		return type.GetTypeCodeImpl();
	}

	/// <summary>Returns the underlying type code of the specified <see cref="T:System.Type" />.</summary>
	/// <returns>The code of the underlying type.</returns>
	protected virtual TypeCode GetTypeCodeImpl()
	{
		if (this != UnderlyingSystemType && UnderlyingSystemType != null)
		{
			return GetTypeCode(UnderlyingSystemType);
		}
		return TypeCode.Object;
	}

	private static void CreateBinder()
	{
		if (defaultBinder == null)
		{
			DefaultBinder value = new DefaultBinder();
			Interlocked.CompareExchange(ref defaultBinder, value, null);
		}
	}

	/// <summary>When overridden in a derived class, invokes the specified member, using the specified binding constraints and matching the specified argument list, modifiers and culture.</summary>
	/// <returns>An object representing the return value of the invoked member.</returns>
	/// <param name="name">The string containing the name of the constructor, method, property, or field member to invoke.-or- An empty string ("") to invoke the default member. -or-For IDispatch members, a string representing the DispID, for example "[DispID=3]".</param>
	/// <param name="invokeAttr">A bitmask comprised of one or more <see cref="T:System.Reflection.BindingFlags" /> that specify how the search is conducted. The access can be one of the BindingFlags such as Public, NonPublic, Private, InvokeMethod, GetField, and so on. The type of lookup need not be specified. If the type of lookup is omitted, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static are used. </param>
	/// <param name="binder">An object that defines a set of properties and enables binding, which can involve selection of an overloaded method, coercion of argument types, and invocation of a member through reflection.-or- A null reference (Nothing in Visual Basic), to use the <see cref="P:System.Type.DefaultBinder" />. Note that explicitly defining a <see cref="T:System.Reflection.Binder" /> object may be required for successfully invoking method overloads with variable arguments.</param>
	/// <param name="target">The object on which to invoke the specified member. </param>
	/// <param name="args">An array containing the arguments to pass to the member to invoke. </param>
	/// <param name="modifiers">An array of <see cref="T:System.Reflection.ParameterModifier" /> objects representing the attributes associated with the corresponding element in the <paramref name="args" /> array. A parameter's associated attributes are stored in the member's signature. The default binder processes this parameter only when calling a COM component. </param>
	/// <param name="culture">The <see cref="T:System.Globalization.CultureInfo" /> object representing the globalization locale to use, which may be necessary for locale-specific conversions, such as converting a numeric String to a Double.-or- A null reference (Nothing in Visual Basic) to use the current thread's <see cref="T:System.Globalization.CultureInfo" />. </param>
	/// <param name="namedParameters">An array containing the names of the parameters to which the values in the <paramref name="args" /> array are passed. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="invokeAttr" /> does not contain CreateInstance and <paramref name="name" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="args" /> and <paramref name="modifiers" /> do not have the same length.-or- <paramref name="invokeAttr" /> is not a valid <see cref="T:System.Reflection.BindingFlags" /> attribute.-or- <paramref name="invokeAttr" /> does not contain one of the following binding flags: InvokeMethod, CreateInstance, GetField, SetField, GetProperty, or SetProperty.-or- <paramref name="invokeAttr" /> contains CreateInstance combined with InvokeMethod, GetField, SetField, GetProperty, or SetProperty.-or- <paramref name="invokeAttr" /> contains both GetField and SetField.-or- <paramref name="invokeAttr" /> contains both GetProperty and SetProperty.-or- <paramref name="invokeAttr" /> contains InvokeMethod combined with SetField or SetProperty.-or- <paramref name="invokeAttr" /> contains SetField and <paramref name="args" /> has more than one element.-or- The named parameter array is larger than the argument array.-or- This method is called on a COM object and one of the following binding flags was not passed in: BindingFlags.InvokeMethod, BindingFlags.GetProperty, BindingFlags.SetProperty, BindingFlags.PutDispProperty, or BindingFlags.PutRefDispProperty.-or- One of the named parameter arrays contains a string that is null. </exception>
	/// <exception cref="T:System.MethodAccessException">The specified member is a class initializer. </exception>
	/// <exception cref="T:System.MissingFieldException">The field or property cannot be found. </exception>
	/// <exception cref="T:System.MissingMethodException">No method can be found that matches the arguments in <paramref name="args" />.-or- No member can be found that has the argument names supplied in <paramref name="namedParameters" />.-or- The current <see cref="T:System.Type" /> object represents a type that contains open type parameters, that is, <see cref="P:System.Type.ContainsGenericParameters" /> returns true. </exception>
	/// <exception cref="T:System.Reflection.TargetException">The specified member cannot be invoked on <paramref name="target" />. </exception>
	/// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one method matches the binding criteria. </exception>
	/// <exception cref="T:System.InvalidOperationException">The method represented by <paramref name="name" /> has one or more unspecified generic type parameters. That is, the method's <see cref="P:System.Reflection.MethodInfo.ContainsGenericParameters" /> property returns true.</exception>
	/// <filterpriority>2</filterpriority>
	public abstract object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters);

	/// <summary>Invokes the specified member, using the specified binding constraints and matching the specified argument list and culture.</summary>
	/// <returns>An object representing the return value of the invoked member.</returns>
	/// <param name="name">The string containing the name of the constructor, method, property, or field member to invoke.-or- An empty string ("") to invoke the default member. -or-For IDispatch members, a string representing the DispID, for example "[DispID=3]".</param>
	/// <param name="invokeAttr">A bitmask comprised of one or more <see cref="T:System.Reflection.BindingFlags" /> that specify how the search is conducted. The access can be one of the BindingFlags such as Public, NonPublic, Private, InvokeMethod, GetField, and so on. The type of lookup need not be specified. If the type of lookup is omitted, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static are used. </param>
	/// <param name="binder">An object that defines a set of properties and enables binding, which can involve selection of an overloaded method, coercion of argument types, and invocation of a member through reflection.-or- A null reference (Nothing in Visual Basic), to use the <see cref="P:System.Type.DefaultBinder" />. Note that explicitly defining a <see cref="T:System.Reflection.Binder" /> object may be required for successfully invoking method overloads with variable arguments.</param>
	/// <param name="target">The object on which to invoke the specified member. </param>
	/// <param name="args">An array containing the arguments to pass to the member to invoke. </param>
	/// <param name="culture">The object representing the globalization locale to use, which may be necessary for locale-specific conversions, such as converting a numeric <see cref="T:System.String" /> to a <see cref="T:System.Double" />.-or- A null reference (Nothing in Visual Basic) to use the current thread's <see cref="T:System.Globalization.CultureInfo" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="invokeAttr" /> does not contain CreateInstance and <paramref name="name" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="invokeAttr" /> is not a valid <see cref="T:System.Reflection.BindingFlags" /> attribute. -or- <paramref name="invokeAttr" /> does not contain one of the following binding flags: InvokeMethod, CreateInstance, GetField, SetField, GetProperty, or SetProperty.-or- <paramref name="invokeAttr" /> contains CreateInstance combined with InvokeMethod, GetField, SetField, GetProperty, or SetProperty.-or- <paramref name="invokeAttr" /> contains both GetField and SetField.-or- <paramref name="invokeAttr" /> contains both GetProperty and SetProperty.-or- <paramref name="invokeAttr" /> contains InvokeMethod combined with SetField or SetProperty.-or- <paramref name="invokeAttr" /> contains SetField and <paramref name="args" /> has more than one element.-or- This method is called on a COM object and one of the following binding flags was not passed in: BindingFlags.InvokeMethod, BindingFlags.GetProperty, BindingFlags.SetProperty, BindingFlags.PutDispProperty, or BindingFlags.PutRefDispProperty.-or- One of the named parameter arrays contains a string that is null. </exception>
	/// <exception cref="T:System.MethodAccessException">The specified member is a class initializer. </exception>
	/// <exception cref="T:System.MissingFieldException">The field or property cannot be found. </exception>
	/// <exception cref="T:System.MissingMethodException">No method can be found that matches the arguments in <paramref name="args" />.-or- The current <see cref="T:System.Type" /> object represents a type that contains open type parameters, that is, <see cref="P:System.Type.ContainsGenericParameters" /> returns true. </exception>
	/// <exception cref="T:System.Reflection.TargetException">The specified member cannot be invoked on <paramref name="target" />. </exception>
	/// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one method matches the binding criteria. </exception>
	/// <exception cref="T:System.InvalidOperationException">The method represented by <paramref name="name" /> has one or more unspecified generic type parameters. That is, the method's <see cref="P:System.Reflection.MethodInfo.ContainsGenericParameters" /> property returns true.</exception>
	/// <filterpriority>2</filterpriority>
	[DebuggerHidden]
	[DebuggerStepThrough]
	public object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, CultureInfo culture)
	{
		return InvokeMember(name, invokeAttr, binder, target, args, null, culture, null);
	}

	/// <summary>Invokes the specified member, using the specified binding constraints and matching the specified argument list.</summary>
	/// <returns>An object representing the return value of the invoked member.</returns>
	/// <param name="name">The string containing the name of the constructor, method, property, or field member to invoke.-or- An empty string ("") to invoke the default member. -or-For IDispatch members, a string representing the DispID, for example "[DispID=3]".</param>
	/// <param name="invokeAttr">A bitmask comprised of one or more <see cref="T:System.Reflection.BindingFlags" /> that specify how the search is conducted. The access can be one of the BindingFlags such as Public, NonPublic, Private, InvokeMethod, GetField, and so on. The type of lookup need not be specified. If the type of lookup is omitted, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static are used. </param>
	/// <param name="binder">An object that defines a set of properties and enables binding, which can involve selection of an overloaded method, coercion of argument types, and invocation of a member through reflection.-or- A null reference (Nothing in Visual Basic), to use the <see cref="P:System.Type.DefaultBinder" />. Note that explicitly defining a <see cref="T:System.Reflection.Binder" /> object may be required for successfully invoking method overloads with variable arguments.</param>
	/// <param name="target">The object on which to invoke the specified member. </param>
	/// <param name="args">An array containing the arguments to pass to the member to invoke. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="invokeAttr" /> does not contain CreateInstance and <paramref name="name" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="invokeAttr" /> is not a valid <see cref="T:System.Reflection.BindingFlags" /> attribute. -or- <paramref name="invokeAttr" /> does not contain one of the following binding flags: InvokeMethod, CreateInstance, GetField, SetField, GetProperty, or SetProperty. -or- <paramref name="invokeAttr" /> contains CreateInstance combined with InvokeMethod, GetField, SetField, GetProperty, or SetProperty.-or- <paramref name="invokeAttr" /> contains both GetField and SetField.-or- <paramref name="invokeAttr" /> contains both GetProperty and SetProperty.-or- <paramref name="invokeAttr" /> contains InvokeMethod combined with SetField or SetProperty.-or- <paramref name="invokeAttr" /> contains SetField and <paramref name="args" /> has more than one element.-or- This method is called on a COM object and one of the following binding flags was not passed in: BindingFlags.InvokeMethod, BindingFlags.GetProperty, BindingFlags.SetProperty, BindingFlags.PutDispProperty, or BindingFlags.PutRefDispProperty.-or- One of the named parameter arrays contains a string that is null. </exception>
	/// <exception cref="T:System.MethodAccessException">The specified member is a class initializer. </exception>
	/// <exception cref="T:System.MissingFieldException">The field or property cannot be found. </exception>
	/// <exception cref="T:System.MissingMethodException">No method can be found that matches the arguments in <paramref name="args" />.-or- The current <see cref="T:System.Type" /> object represents a type that contains open type parameters, that is, <see cref="P:System.Type.ContainsGenericParameters" /> returns true. </exception>
	/// <exception cref="T:System.Reflection.TargetException">The specified member cannot be invoked on <paramref name="target" />. </exception>
	/// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one method matches the binding criteria. </exception>
	/// <exception cref="T:System.NotSupportedException">The .NET Compact Framework does not currently support this method.</exception>
	/// <exception cref="T:System.InvalidOperationException">The method represented by <paramref name="name" /> has one or more unspecified generic type parameters. That is, the method's <see cref="P:System.Reflection.MethodInfo.ContainsGenericParameters" /> property returns true.</exception>
	/// <filterpriority>2</filterpriority>
	[DebuggerStepThrough]
	[DebuggerHidden]
	public object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args)
	{
		return InvokeMember(name, invokeAttr, binder, target, args, null, null, null);
	}

	internal virtual RuntimeTypeHandle GetTypeHandleInternal()
	{
		return TypeHandle;
	}

	/// <summary>Gets the handle for the <see cref="T:System.Type" /> of a specified object.</summary>
	/// <returns>The handle for the <see cref="T:System.Type" /> of the specified <see cref="T:System.Object" />.</returns>
	/// <param name="o">The object for which to get the type handle. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="o" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public static RuntimeTypeHandle GetTypeHandle(object o)
	{
		if (o == null)
		{
			throw new ArgumentNullException(null, Environment.GetResourceString("Invalid handle."));
		}
		return new RuntimeTypeHandle((RuntimeType)o.GetType());
	}

	/// <summary>Gets the number of dimensions in an <see cref="T:System.Array" />.</summary>
	/// <returns>An <see cref="T:System.Int32" /> containing the number of dimensions in the current Type.</returns>
	/// <exception cref="T:System.NotSupportedException">The functionality of this method is unsupported in the base class and must be implemented in a derived class instead. </exception>
	/// <exception cref="T:System.ArgumentException">The current Type is not an array. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual int GetArrayRank()
	{
		throw new NotSupportedException(Environment.GetResourceString("Derived classes must provide an implementation."));
	}

	/// <summary>Searches for a constructor whose parameters match the specified argument types and modifiers, using the specified binding constraints and the specified calling convention.</summary>
	/// <returns>An object representing the constructor that matches the specified requirements, if found; otherwise, null.</returns>
	/// <param name="bindingAttr">A bitmask comprised of one or more <see cref="T:System.Reflection.BindingFlags" /> that specify how the search is conducted.-or- Zero, to return null. </param>
	/// <param name="binder">An object that defines a set of properties and enables binding, which can involve selection of an overloaded method, coercion of argument types, and invocation of a member through reflection.-or- A null reference (Nothing in Visual Basic), to use the <see cref="P:System.Type.DefaultBinder" />. </param>
	/// <param name="callConvention">The object that specifies the set of rules to use regarding the order and layout of arguments, how the return value is passed, what registers are used for arguments, and the stack is cleaned up. </param>
	/// <param name="types">An array of <see cref="T:System.Type" /> objects representing the number, order, and type of the parameters for the constructor to get.-or- An empty array of the type <see cref="T:System.Type" /> (that is, Type[] types = new Type[0]) to get a constructor that takes no parameters. </param>
	/// <param name="modifiers">An array of <see cref="T:System.Reflection.ParameterModifier" /> objects representing the attributes associated with the corresponding element in the <paramref name="types" /> array. The default binder does not process this parameter. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="types" /> is null.-or- One of the elements in <paramref name="types" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="types" /> is multidimensional.-or- <paramref name="modifiers" /> is multidimensional.-or- <paramref name="types" /> and <paramref name="modifiers" /> do not have the same length. </exception>
	/// <filterpriority>2</filterpriority>
	[ComVisible(true)]
	public ConstructorInfo GetConstructor(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
	{
		if (types == null)
		{
			throw new ArgumentNullException("types");
		}
		for (int i = 0; i < types.Length; i++)
		{
			if (types[i] == null)
			{
				throw new ArgumentNullException("types");
			}
		}
		return GetConstructorImpl(bindingAttr, binder, callConvention, types, modifiers);
	}

	/// <summary>Searches for a constructor whose parameters match the specified argument types and modifiers, using the specified binding constraints.</summary>
	/// <returns>A <see cref="T:System.Reflection.ConstructorInfo" /> object representing the constructor that matches the specified requirements, if found; otherwise, null.</returns>
	/// <param name="bindingAttr">A bitmask comprised of one or more <see cref="T:System.Reflection.BindingFlags" /> that specify how the search is conducted.-or- Zero, to return null. </param>
	/// <param name="binder">An object that defines a set of properties and enables binding, which can involve selection of an overloaded method, coercion of argument types, and invocation of a member through reflection.-or- A null reference (Nothing in Visual Basic), to use the <see cref="P:System.Type.DefaultBinder" />. </param>
	/// <param name="types">An array of <see cref="T:System.Type" /> objects representing the number, order, and type of the parameters for the constructor to get.-or- An empty array of the type <see cref="T:System.Type" /> (that is, Type[] types = new Type[0]) to get a constructor that takes no parameters.-or- <see cref="F:System.Type.EmptyTypes" />. </param>
	/// <param name="modifiers">An array of <see cref="T:System.Reflection.ParameterModifier" /> objects representing the attributes associated with the corresponding element in the parameter type array. The default binder does not process this parameter. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="types" /> is null.-or- One of the elements in <paramref name="types" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="types" /> is multidimensional.-or- <paramref name="modifiers" /> is multidimensional.-or- <paramref name="types" /> and <paramref name="modifiers" /> do not have the same length. </exception>
	/// <filterpriority>2</filterpriority>
	[ComVisible(true)]
	public ConstructorInfo GetConstructor(BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers)
	{
		if (types == null)
		{
			throw new ArgumentNullException("types");
		}
		for (int i = 0; i < types.Length; i++)
		{
			if (types[i] == null)
			{
				throw new ArgumentNullException("types");
			}
		}
		return GetConstructorImpl(bindingAttr, binder, CallingConventions.Any, types, modifiers);
	}

	/// <summary>Searches for a public instance constructor whose parameters match the types in the specified array.</summary>
	/// <returns>An object representing the public instance constructor whose parameters match the types in the parameter type array, if found; otherwise, null.</returns>
	/// <param name="types">An array of <see cref="T:System.Type" /> objects representing the number, order, and type of the parameters for the desired constructor.-or- An empty array of <see cref="T:System.Type" /> objects, to get a constructor that takes no parameters. Such an empty array is provided by the static field <see cref="F:System.Type.EmptyTypes" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="types" /> is null.-or- One of the elements in <paramref name="types" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="types" /> is multidimensional. </exception>
	/// <filterpriority>2</filterpriority>
	[ComVisible(true)]
	public ConstructorInfo GetConstructor(Type[] types)
	{
		return GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, types, null);
	}

	/// <summary>When overridden in a derived class, searches for a constructor whose parameters match the specified argument types and modifiers, using the specified binding constraints and the specified calling convention.</summary>
	/// <returns>A <see cref="T:System.Reflection.ConstructorInfo" /> object representing the constructor that matches the specified requirements, if found; otherwise, null.</returns>
	/// <param name="bindingAttr">A bitmask comprised of one or more <see cref="T:System.Reflection.BindingFlags" /> that specify how the search is conducted.-or- Zero, to return null. </param>
	/// <param name="binder">An object that defines a set of properties and enables binding, which can involve selection of an overloaded method, coercion of argument types, and invocation of a member through reflection.-or- A null reference (Nothing in Visual Basic), to use the <see cref="P:System.Type.DefaultBinder" />. </param>
	/// <param name="callConvention">The object that specifies the set of rules to use regarding the order and layout of arguments, how the return value is passed, what registers are used for arguments, and the stack is cleaned up. </param>
	/// <param name="types">An array of <see cref="T:System.Type" /> objects representing the number, order, and type of the parameters for the constructor to get.-or- An empty array of the type <see cref="T:System.Type" /> (that is, Type[] types = new Type[0]) to get a constructor that takes no parameters. </param>
	/// <param name="modifiers">An array of <see cref="T:System.Reflection.ParameterModifier" /> objects representing the attributes associated with the corresponding element in the <paramref name="types" /> array. The default binder does not process this parameter. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="types" /> is null.-or- One of the elements in <paramref name="types" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="types" /> is multidimensional.-or- <paramref name="modifiers" /> is multidimensional.-or- <paramref name="types" /> and <paramref name="modifiers" /> do not have the same length. </exception>
	/// <exception cref="T:System.NotSupportedException">The current type is a <see cref="T:System.Reflection.Emit.TypeBuilder" /> or <see cref="T:System.Reflection.Emit.GenericTypeParameterBuilder" />.</exception>
	protected abstract ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers);

	/// <summary>Returns all the public constructors defined for the current <see cref="T:System.Type" />.</summary>
	/// <returns>An array of <see cref="T:System.Reflection.ConstructorInfo" /> objects representing all the public instance constructors defined for the current <see cref="T:System.Type" />, but not including the type initializer (static constructor). If no public instance constructors are defined for the current <see cref="T:System.Type" />, or if the current <see cref="T:System.Type" /> represents a type parameter in the definition of a generic type or generic method, an empty array of type <see cref="T:System.Reflection.ConstructorInfo" /> is returned.</returns>
	/// <filterpriority>2</filterpriority>
	[ComVisible(true)]
	public ConstructorInfo[] GetConstructors()
	{
		return GetConstructors(BindingFlags.Instance | BindingFlags.Public);
	}

	/// <summary>When overridden in a derived class, searches for the constructors defined for the current <see cref="T:System.Type" />, using the specified BindingFlags.</summary>
	/// <returns>An array of <see cref="T:System.Reflection.ConstructorInfo" /> objects representing all constructors defined for the current <see cref="T:System.Type" /> that match the specified binding constraints, including the type initializer if it is defined. Returns an empty array of type <see cref="T:System.Reflection.ConstructorInfo" /> if no constructors are defined for the current <see cref="T:System.Type" />, if none of the defined constructors match the binding constraints, or if the current <see cref="T:System.Type" /> represents a type parameter in the definition of a generic type or generic method.</returns>
	/// <param name="bindingAttr">A bitmask comprised of one or more <see cref="T:System.Reflection.BindingFlags" /> that specify how the search is conducted.-or- Zero, to return null. </param>
	/// <filterpriority>2</filterpriority>
	[ComVisible(true)]
	public abstract ConstructorInfo[] GetConstructors(BindingFlags bindingAttr);

	/// <summary>Searches for the specified method whose parameters match the specified argument types and modifiers, using the specified binding constraints and the specified calling convention.</summary>
	/// <returns>An object representing the method that matches the specified requirements, if found; otherwise, null.</returns>
	/// <param name="name">The string containing the name of the method to get. </param>
	/// <param name="bindingAttr">A bitmask comprised of one or more <see cref="T:System.Reflection.BindingFlags" /> that specify how the search is conducted.-or- Zero, to return null. </param>
	/// <param name="binder">An object that defines a set of properties and enables binding, which can involve selection of an overloaded method, coercion of argument types, and invocation of a member through reflection.-or- A null reference (Nothing in Visual Basic), to use the <see cref="P:System.Type.DefaultBinder" />. </param>
	/// <param name="callConvention">The object that specifies the set of rules to use regarding the order and layout of arguments, how the return value is passed, what registers are used for arguments, and how the stack is cleaned up. </param>
	/// <param name="types">An array of <see cref="T:System.Type" /> objects representing the number, order, and type of the parameters for the method to get.-or- An empty array of <see cref="T:System.Type" /> objects (as provided by the <see cref="F:System.Type.EmptyTypes" /> field) to get a method that takes no parameters. </param>
	/// <param name="modifiers">An array of <see cref="T:System.Reflection.ParameterModifier" /> objects representing the attributes associated with the corresponding element in the <paramref name="types" /> array. To be only used when calling through COM interop, and only parameters that are passed by reference are handled. The default binder does not process this parameter. </param>
	/// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one method is found with the specified name and matching the specified binding constraints. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null.-or- <paramref name="types" /> is null.-or- One of the elements in <paramref name="types" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="types" /> is multidimensional.-or- <paramref name="modifiers" /> is multidimensional. </exception>
	/// <filterpriority>2</filterpriority>
	public MethodInfo GetMethod(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (types == null)
		{
			throw new ArgumentNullException("types");
		}
		for (int i = 0; i < types.Length; i++)
		{
			if (types[i] == null)
			{
				throw new ArgumentNullException("types");
			}
		}
		return GetMethodImpl(name, bindingAttr, binder, callConvention, types, modifiers);
	}

	/// <summary>Searches for the specified method whose parameters match the specified argument types and modifiers, using the specified binding constraints.</summary>
	/// <returns>An object representing the method that matches the specified requirements, if found; otherwise, null.</returns>
	/// <param name="name">The string containing the name of the method to get. </param>
	/// <param name="bindingAttr">A bitmask comprised of one or more <see cref="T:System.Reflection.BindingFlags" /> that specify how the search is conducted.-or- Zero, to return null. </param>
	/// <param name="binder">An object that defines a set of properties and enables binding, which can involve selection of an overloaded method, coercion of argument types, and invocation of a member through reflection.-or- A null reference (Nothing in Visual Basic), to use the <see cref="P:System.Type.DefaultBinder" />. </param>
	/// <param name="types">An array of <see cref="T:System.Type" /> objects representing the number, order, and type of the parameters for the method to get.-or- An empty array of <see cref="T:System.Type" /> objects (as provided by the <see cref="F:System.Type.EmptyTypes" /> field) to get a method that takes no parameters. </param>
	/// <param name="modifiers">An array of <see cref="T:System.Reflection.ParameterModifier" /> objects representing the attributes associated with the corresponding element in the <paramref name="types" /> array. To be only used when calling through COM interop, and only parameters that are passed by reference are handled. The default binder does not process this parameter.</param>
	/// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one method is found with the specified name and matching the specified binding constraints. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null.-or- <paramref name="types" /> is null.-or- One of the elements in <paramref name="types" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="types" /> is multidimensional.-or- <paramref name="modifiers" /> is multidimensional. </exception>
	/// <filterpriority>2</filterpriority>
	public MethodInfo GetMethod(string name, BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (types == null)
		{
			throw new ArgumentNullException("types");
		}
		for (int i = 0; i < types.Length; i++)
		{
			if (types[i] == null)
			{
				throw new ArgumentNullException("types");
			}
		}
		return GetMethodImpl(name, bindingAttr, binder, CallingConventions.Any, types, modifiers);
	}

	/// <summary>Searches for the specified public method whose parameters match the specified argument types and modifiers.</summary>
	/// <returns>An object representing the public method that matches the specified requirements, if found; otherwise, null.</returns>
	/// <param name="name">The string containing the name of the public method to get. </param>
	/// <param name="types">An array of <see cref="T:System.Type" /> objects representing the number, order, and type of the parameters for the method to get.-or- An empty array of <see cref="T:System.Type" /> objects (as provided by the <see cref="F:System.Type.EmptyTypes" /> field) to get a method that takes no parameters. </param>
	/// <param name="modifiers">An array of <see cref="T:System.Reflection.ParameterModifier" /> objects representing the attributes associated with the corresponding element in the <paramref name="types" /> array. To be only used when calling through COM interop, and only parameters that are passed by reference are handled. The default binder does not process this parameter.  </param>
	/// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one method is found with the specified name and specified parameters. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null.-or- <paramref name="types" /> is null.-or- One of the elements in <paramref name="types" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="types" /> is multidimensional.-or- <paramref name="modifiers" /> is multidimensional. </exception>
	/// <filterpriority>2</filterpriority>
	public MethodInfo GetMethod(string name, Type[] types, ParameterModifier[] modifiers)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (types == null)
		{
			throw new ArgumentNullException("types");
		}
		for (int i = 0; i < types.Length; i++)
		{
			if (types[i] == null)
			{
				throw new ArgumentNullException("types");
			}
		}
		return GetMethodImpl(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public, null, CallingConventions.Any, types, modifiers);
	}

	/// <summary>Searches for the specified public method whose parameters match the specified argument types.</summary>
	/// <returns>An object representing the public method whose parameters match the specified argument types, if found; otherwise, null.</returns>
	/// <param name="name">The string containing the name of the public method to get. </param>
	/// <param name="types">An array of <see cref="T:System.Type" /> objects representing the number, order, and type of the parameters for the method to get.-or- An empty array of <see cref="T:System.Type" /> objects (as provided by the <see cref="F:System.Type.EmptyTypes" /> field) to get a method that takes no parameters. </param>
	/// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one method is found with the specified name and specified parameters. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null.-or- <paramref name="types" /> is null.-or- One of the elements in <paramref name="types" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="types" /> is multidimensional. </exception>
	/// <filterpriority>2</filterpriority>
	public MethodInfo GetMethod(string name, Type[] types)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (types == null)
		{
			throw new ArgumentNullException("types");
		}
		for (int i = 0; i < types.Length; i++)
		{
			if (types[i] == null)
			{
				throw new ArgumentNullException("types");
			}
		}
		return GetMethodImpl(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public, null, CallingConventions.Any, types, null);
	}

	/// <summary>Searches for the specified method, using the specified binding constraints.</summary>
	/// <returns>An object representing the method that matches the specified requirements, if found; otherwise, null.</returns>
	/// <param name="name">The string containing the name of the method to get. </param>
	/// <param name="bindingAttr">A bitmask comprised of one or more <see cref="T:System.Reflection.BindingFlags" /> that specify how the search is conducted.-or- Zero, to return null. </param>
	/// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one method is found with the specified name and matching the specified binding constraints. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <filterpriority>2</filterpriority>
	public MethodInfo GetMethod(string name, BindingFlags bindingAttr)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		return GetMethodImpl(name, bindingAttr, null, CallingConventions.Any, null, null);
	}

	/// <summary>Searches for the public method with the specified name.</summary>
	/// <returns>An object that represents the public method with the specified name, if found; otherwise, null.</returns>
	/// <param name="name">The string containing the name of the public method to get. </param>
	/// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one method is found with the specified name. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <filterpriority>2</filterpriority>
	public MethodInfo GetMethod(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		return GetMethodImpl(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public, null, CallingConventions.Any, null, null);
	}

	/// <summary>When overridden in a derived class, searches for the specified method whose parameters match the specified argument types and modifiers, using the specified binding constraints and the specified calling convention.</summary>
	/// <returns>An object representing the method that matches the specified requirements, if found; otherwise, null.</returns>
	/// <param name="name">The string containing the name of the method to get. </param>
	/// <param name="bindingAttr">A bitmask comprised of one or more <see cref="T:System.Reflection.BindingFlags" /> that specify how the search is conducted.-or- Zero, to return null. </param>
	/// <param name="binder">An object that defines a set of properties and enables binding, which can involve selection of an overloaded method, coercion of argument types, and invocation of a member through reflection.-or- A null reference (Nothing in Visual Basic), to use the <see cref="P:System.Type.DefaultBinder" />. </param>
	/// <param name="callConvention">The object that specifies the set of rules to use regarding the order and layout of arguments, how the return value is passed, what registers are used for arguments, and what process cleans up the stack. </param>
	/// <param name="types">An array of <see cref="T:System.Type" /> objects representing the number, order, and type of the parameters for the method to get.-or- An empty array of the type <see cref="T:System.Type" /> (that is, Type[] types = new Type[0]) to get a method that takes no parameters.-or- null. If <paramref name="types" /> is null, arguments are not matched. </param>
	/// <param name="modifiers">An array of <see cref="T:System.Reflection.ParameterModifier" /> objects representing the attributes associated with the corresponding element in the <paramref name="types" /> array. The default binder does not process this parameter. </param>
	/// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one method is found with the specified name and matching the specified binding constraints. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="types" /> is multidimensional.-or- <paramref name="modifiers" /> is multidimensional.-or- <paramref name="types" /> and <paramref name="modifiers" /> do not have the same length. </exception>
	/// <exception cref="T:System.NotSupportedException">The current type is a <see cref="T:System.Reflection.Emit.TypeBuilder" /> or <see cref="T:System.Reflection.Emit.GenericTypeParameterBuilder" />.</exception>
	protected abstract MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers);

	/// <summary>Returns all the public methods of the current <see cref="T:System.Type" />.</summary>
	/// <returns>An array of <see cref="T:System.Reflection.MethodInfo" /> objects representing all the public methods defined for the current <see cref="T:System.Type" />.-or- An empty array of type <see cref="T:System.Reflection.MethodInfo" />, if no public methods are defined for the current <see cref="T:System.Type" />.</returns>
	/// <filterpriority>2</filterpriority>
	public MethodInfo[] GetMethods()
	{
		return GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
	}

	/// <summary>When overridden in a derived class, searches for the methods defined for the current <see cref="T:System.Type" />, using the specified binding constraints.</summary>
	/// <returns>An array of <see cref="T:System.Reflection.MethodInfo" /> objects representing all methods defined for the current <see cref="T:System.Type" /> that match the specified binding constraints.-or- An empty array of type <see cref="T:System.Reflection.MethodInfo" />, if no methods are defined for the current <see cref="T:System.Type" />, or if none of the defined methods match the binding constraints.</returns>
	/// <param name="bindingAttr">A bitmask comprised of one or more <see cref="T:System.Reflection.BindingFlags" /> that specify how the search is conducted.-or- Zero, to return null. </param>
	/// <filterpriority>2</filterpriority>
	public abstract MethodInfo[] GetMethods(BindingFlags bindingAttr);

	/// <summary>Searches for the specified field, using the specified binding constraints.</summary>
	/// <returns>An object representing the field that matches the specified requirements, if found; otherwise, null.</returns>
	/// <param name="name">The string containing the name of the data field to get. </param>
	/// <param name="bindingAttr">A bitmask comprised of one or more <see cref="T:System.Reflection.BindingFlags" /> that specify how the search is conducted.-or- Zero, to return null. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <filterpriority>2</filterpriority>
	public abstract FieldInfo GetField(string name, BindingFlags bindingAttr);

	/// <summary>Searches for the public field with the specified name.</summary>
	/// <returns>An object representing the public field with the specified name, if found; otherwise, null.</returns>
	/// <param name="name">The string containing the name of the data field to get. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <exception cref="T:System.NotSupportedException">This <see cref="T:System.Type" /> object is a <see cref="T:System.Reflection.Emit.TypeBuilder" /> whose <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" /> method has not yet been called. </exception>
	/// <filterpriority>2</filterpriority>
	public FieldInfo GetField(string name)
	{
		return GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
	}

	/// <summary>Returns all the public fields of the current <see cref="T:System.Type" />.</summary>
	/// <returns>An array of <see cref="T:System.Reflection.FieldInfo" /> objects representing all the public fields defined for the current <see cref="T:System.Type" />.-or- An empty array of type <see cref="T:System.Reflection.FieldInfo" />, if no public fields are defined for the current <see cref="T:System.Type" />.</returns>
	/// <filterpriority>2</filterpriority>
	public FieldInfo[] GetFields()
	{
		return GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
	}

	/// <summary>When overridden in a derived class, searches for the fields defined for the current <see cref="T:System.Type" />, using the specified binding constraints.</summary>
	/// <returns>An array of <see cref="T:System.Reflection.FieldInfo" /> objects representing all fields defined for the current <see cref="T:System.Type" /> that match the specified binding constraints.-or- An empty array of type <see cref="T:System.Reflection.FieldInfo" />, if no fields are defined for the current <see cref="T:System.Type" />, or if none of the defined fields match the binding constraints.</returns>
	/// <param name="bindingAttr">A bitmask comprised of one or more <see cref="T:System.Reflection.BindingFlags" /> that specify how the search is conducted.-or- Zero, to return null. </param>
	/// <filterpriority>2</filterpriority>
	public abstract FieldInfo[] GetFields(BindingFlags bindingAttr);

	/// <summary>Searches for the interface with the specified name.</summary>
	/// <returns>An object representing the interface with the specified name, implemented or inherited by the current <see cref="T:System.Type" />, if found; otherwise, null.</returns>
	/// <param name="name">The string containing the name of the interface to get. For generic interfaces, this is the mangled name.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <exception cref="T:System.Reflection.AmbiguousMatchException">The current <see cref="T:System.Type" /> represents a type that implements the same generic interface with different type arguments. </exception>
	/// <filterpriority>2</filterpriority>
	public Type GetInterface(string name)
	{
		return GetInterface(name, ignoreCase: false);
	}

	/// <summary>When overridden in a derived class, searches for the specified interface, specifying whether to do a case-insensitive search for the interface name.</summary>
	/// <returns>An object representing the interface with the specified name, implemented or inherited by the current <see cref="T:System.Type" />, if found; otherwise, null.</returns>
	/// <param name="name">The string containing the name of the interface to get. For generic interfaces, this is the mangled name.</param>
	/// <param name="ignoreCase">true to ignore the case of that part of <paramref name="name" /> that specifies the simple interface name (the part that specifies the namespace must be correctly cased).-or- false to perform a case-sensitive search for all parts of <paramref name="name" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <exception cref="T:System.Reflection.AmbiguousMatchException">The current <see cref="T:System.Type" /> represents a type that implements the same generic interface with different type arguments. </exception>
	/// <filterpriority>2</filterpriority>
	public abstract Type GetInterface(string name, bool ignoreCase);

	/// <summary>When overridden in a derived class, gets all the interfaces implemented or inherited by the current <see cref="T:System.Type" />.</summary>
	/// <returns>An array of <see cref="T:System.Type" /> objects representing all the interfaces implemented or inherited by the current <see cref="T:System.Type" />.-or- An empty array of type <see cref="T:System.Type" />, if no interfaces are implemented or inherited by the current <see cref="T:System.Type" />.</returns>
	/// <exception cref="T:System.Reflection.TargetInvocationException">A static initializer is invoked and throws an exception. </exception>
	/// <filterpriority>2</filterpriority>
	public abstract Type[] GetInterfaces();

	/// <summary>Returns an array of <see cref="T:System.Type" /> objects representing a filtered list of interfaces implemented or inherited by the current <see cref="T:System.Type" />.</summary>
	/// <returns>An array of <see cref="T:System.Type" /> objects representing a filtered list of the interfaces implemented or inherited by the current <see cref="T:System.Type" />, or an empty array of type <see cref="T:System.Type" /> if no interfaces matching the filter are implemented or inherited by the current <see cref="T:System.Type" />.</returns>
	/// <param name="filter">The delegate that compares the interfaces against <paramref name="filterCriteria" />. </param>
	/// <param name="filterCriteria">The search criteria that determines whether an interface should be included in the returned array. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="filter" /> is null. </exception>
	/// <exception cref="T:System.Reflection.TargetInvocationException">A static initializer is invoked and throws an exception. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual Type[] FindInterfaces(TypeFilter filter, object filterCriteria)
	{
		if (filter == null)
		{
			throw new ArgumentNullException("filter");
		}
		Type[] interfaces = GetInterfaces();
		int num = 0;
		for (int i = 0; i < interfaces.Length; i++)
		{
			if (!filter(interfaces[i], filterCriteria))
			{
				interfaces[i] = null;
			}
			else
			{
				num++;
			}
		}
		if (num == interfaces.Length)
		{
			return interfaces;
		}
		Type[] array = new Type[num];
		num = 0;
		for (int j = 0; j < interfaces.Length; j++)
		{
			if (interfaces[j] != null)
			{
				array[num++] = interfaces[j];
			}
		}
		return array;
	}

	/// <summary>Returns the <see cref="T:System.Reflection.EventInfo" /> object representing the specified public event.</summary>
	/// <returns>The object representing the specified public event that is declared or inherited by the current <see cref="T:System.Type" />, if found; otherwise, null.</returns>
	/// <param name="name">The string containing the name of an event that is declared or inherited by the current <see cref="T:System.Type" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <filterpriority>2</filterpriority>
	public EventInfo GetEvent(string name)
	{
		return GetEvent(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
	}

	/// <summary>When overridden in a derived class, returns the <see cref="T:System.Reflection.EventInfo" /> object representing the specified event, using the specified binding constraints.</summary>
	/// <returns>The object representing the specified event that is declared or inherited by the current <see cref="T:System.Type" />, if found; otherwise, null.</returns>
	/// <param name="name">The string containing the name of an event which is declared or inherited by the current <see cref="T:System.Type" />. </param>
	/// <param name="bindingAttr">A bitmask comprised of one or more <see cref="T:System.Reflection.BindingFlags" /> that specify how the search is conducted.-or- Zero, to return null. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <filterpriority>2</filterpriority>
	public abstract EventInfo GetEvent(string name, BindingFlags bindingAttr);

	/// <summary>Returns all the public events that are declared or inherited by the current <see cref="T:System.Type" />.</summary>
	/// <returns>An array of <see cref="T:System.Reflection.EventInfo" /> objects representing all the public events which are declared or inherited by the current <see cref="T:System.Type" />.-or- An empty array of type <see cref="T:System.Reflection.EventInfo" />, if the current <see cref="T:System.Type" /> does not have public events.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual EventInfo[] GetEvents()
	{
		return GetEvents(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
	}

	/// <summary>When overridden in a derived class, searches for events that are declared or inherited by the current <see cref="T:System.Type" />, using the specified binding constraints.</summary>
	/// <returns>An array of <see cref="T:System.Reflection.EventInfo" /> objects representing all events that are declared or inherited by the current <see cref="T:System.Type" /> that match the specified binding constraints.-or- An empty array of type <see cref="T:System.Reflection.EventInfo" />, if the current <see cref="T:System.Type" /> does not have events, or if none of the events match the binding constraints.</returns>
	/// <param name="bindingAttr">A bitmask comprised of one or more <see cref="T:System.Reflection.BindingFlags" /> that specify how the search is conducted.-or- Zero, to return null. </param>
	/// <filterpriority>2</filterpriority>
	public abstract EventInfo[] GetEvents(BindingFlags bindingAttr);

	/// <summary>Searches for the specified property whose parameters match the specified argument types and modifiers, using the specified binding constraints.</summary>
	/// <returns>An object representing the property that matches the specified requirements, if found; otherwise, null.</returns>
	/// <param name="name">The string containing the name of the property to get. </param>
	/// <param name="bindingAttr">A bitmask comprised of one or more <see cref="T:System.Reflection.BindingFlags" /> that specify how the search is conducted.-or- Zero, to return null. </param>
	/// <param name="binder">An object that defines a set of properties and enables binding, which can involve selection of an overloaded method, coercion of argument types, and invocation of a member through reflection.-or- A null reference (Nothing in Visual Basic), to use the <see cref="P:System.Type.DefaultBinder" />. </param>
	/// <param name="returnType">The return type of the property. </param>
	/// <param name="types">An array of <see cref="T:System.Type" /> objects representing the number, order, and type of the parameters for the indexed property to get.-or- An empty array of the type <see cref="T:System.Type" /> (that is, Type[] types = new Type[0]) to get a property that is not indexed. </param>
	/// <param name="modifiers">An array of <see cref="T:System.Reflection.ParameterModifier" /> objects representing the attributes associated with the corresponding element in the <paramref name="types" /> array. The default binder does not process this parameter. </param>
	/// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one property is found with the specified name and matching the specified binding constraints. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null.-or- <paramref name="types" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="types" /> is multidimensional.-or- <paramref name="modifiers" /> is multidimensional.-or- <paramref name="types" /> and <paramref name="modifiers" /> do not have the same length. </exception>
	/// <exception cref="T:System.NullReferenceException">An element of <paramref name="types" /> is null.</exception>
	/// <filterpriority>2</filterpriority>
	public PropertyInfo GetProperty(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (types == null)
		{
			throw new ArgumentNullException("types");
		}
		return GetPropertyImpl(name, bindingAttr, binder, returnType, types, modifiers);
	}

	/// <summary>Searches for the specified public property whose parameters match the specified argument types and modifiers.</summary>
	/// <returns>An object representing the public property that matches the specified requirements, if found; otherwise, null.</returns>
	/// <param name="name">The string containing the name of the public property to get. </param>
	/// <param name="returnType">The return type of the property. </param>
	/// <param name="types">An array of <see cref="T:System.Type" /> objects representing the number, order, and type of the parameters for the indexed property to get.-or- An empty array of the type <see cref="T:System.Type" /> (that is, Type[] types = new Type[0]) to get a property that is not indexed. </param>
	/// <param name="modifiers">An array of <see cref="T:System.Reflection.ParameterModifier" /> objects representing the attributes associated with the corresponding element in the <paramref name="types" /> array. The default binder does not process this parameter. </param>
	/// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one property is found with the specified name and matching the specified argument types and modifiers. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null.-or- <paramref name="types" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="types" /> is multidimensional.-or- <paramref name="modifiers" /> is multidimensional.-or- <paramref name="types" /> and <paramref name="modifiers" /> do not have the same length. </exception>
	/// <exception cref="T:System.NullReferenceException">An element of <paramref name="types" /> is null.</exception>
	/// <filterpriority>2</filterpriority>
	public PropertyInfo GetProperty(string name, Type returnType, Type[] types, ParameterModifier[] modifiers)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (types == null)
		{
			throw new ArgumentNullException("types");
		}
		return GetPropertyImpl(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public, null, returnType, types, modifiers);
	}

	/// <summary>Searches for the specified property, using the specified binding constraints.</summary>
	/// <returns>An object representing the property that matches the specified requirements, if found; otherwise, null.</returns>
	/// <param name="name">The string containing the name of the property to get. </param>
	/// <param name="bindingAttr">A bitmask comprised of one or more <see cref="T:System.Reflection.BindingFlags" /> that specify how the search is conducted.-or- Zero, to return null. </param>
	/// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one property is found with the specified name and matching the specified binding constraints. See Remarks.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <filterpriority>2</filterpriority>
	public PropertyInfo GetProperty(string name, BindingFlags bindingAttr)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		return GetPropertyImpl(name, bindingAttr, null, null, null, null);
	}

	/// <summary>Searches for the specified public property whose parameters match the specified argument types.</summary>
	/// <returns>An object representing the public property whose parameters match the specified argument types, if found; otherwise, null.</returns>
	/// <param name="name">The string containing the name of the public property to get. </param>
	/// <param name="returnType">The return type of the property. </param>
	/// <param name="types">An array of <see cref="T:System.Type" /> objects representing the number, order, and type of the parameters for the indexed property to get.-or- An empty array of the type <see cref="T:System.Type" /> (that is, Type[] types = new Type[0]) to get a property that is not indexed. </param>
	/// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one property is found with the specified name and matching the specified argument types. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null.-or- <paramref name="types" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="types" /> is multidimensional. </exception>
	/// <exception cref="T:System.NullReferenceException">An element of <paramref name="types" /> is null.</exception>
	/// <filterpriority>2</filterpriority>
	public PropertyInfo GetProperty(string name, Type returnType, Type[] types)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (types == null)
		{
			throw new ArgumentNullException("types");
		}
		return GetPropertyImpl(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public, null, returnType, types, null);
	}

	/// <summary>Searches for the specified public property whose parameters match the specified argument types.</summary>
	/// <returns>An object representing the public property whose parameters match the specified argument types, if found; otherwise, null.</returns>
	/// <param name="name">The string containing the name of the public property to get. </param>
	/// <param name="types">An array of <see cref="T:System.Type" /> objects representing the number, order, and type of the parameters for the indexed property to get.-or- An empty array of the type <see cref="T:System.Type" /> (that is, Type[] types = new Type[0]) to get a property that is not indexed. </param>
	/// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one property is found with the specified name and matching the specified argument types. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null.-or- <paramref name="types" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="types" /> is multidimensional. </exception>
	/// <exception cref="T:System.NullReferenceException">An element of <paramref name="types" /> is null.</exception>
	/// <filterpriority>2</filterpriority>
	public PropertyInfo GetProperty(string name, Type[] types)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (types == null)
		{
			throw new ArgumentNullException("types");
		}
		return GetPropertyImpl(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public, null, null, types, null);
	}

	/// <summary>Searches for the public property with the specified name and return type.</summary>
	/// <returns>An object representing the public property with the specified name, if found; otherwise, null.</returns>
	/// <param name="name">The string containing the name of the public property to get. </param>
	/// <param name="returnType">The return type of the property. </param>
	/// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one property is found with the specified name. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null, or <paramref name="returnType" /> is null. </exception>
	/// <filterpriority>2</filterpriority>
	public PropertyInfo GetProperty(string name, Type returnType)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (returnType == null)
		{
			throw new ArgumentNullException("returnType");
		}
		return GetPropertyImpl(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public, null, returnType, null, null);
	}

	internal PropertyInfo GetProperty(string name, BindingFlags bindingAttr, Type returnType)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (returnType == null)
		{
			throw new ArgumentNullException("returnType");
		}
		return GetPropertyImpl(name, bindingAttr, null, returnType, null, null);
	}

	/// <summary>Searches for the public property with the specified name.</summary>
	/// <returns>An object representing the public property with the specified name, if found; otherwise, null.</returns>
	/// <param name="name">The string containing the name of the public property to get. </param>
	/// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one property is found with the specified name. See Remarks.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <filterpriority>2</filterpriority>
	public PropertyInfo GetProperty(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		return GetPropertyImpl(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public, null, null, null, null);
	}

	/// <summary>When overridden in a derived class, searches for the specified property whose parameters match the specified argument types and modifiers, using the specified binding constraints.</summary>
	/// <returns>An object representing the property that matches the specified requirements, if found; otherwise, null.</returns>
	/// <param name="name">The string containing the name of the property to get. </param>
	/// <param name="bindingAttr">A bitmask comprised of one or more <see cref="T:System.Reflection.BindingFlags" /> that specify how the search is conducted.-or- Zero, to return null. </param>
	/// <param name="binder">An object that defines a set of properties and enables binding, which can involve selection of an overloaded member, coercion of argument types, and invocation of a member through reflection.-or- A null reference (Nothing in Visual Basic), to use the <see cref="P:System.Type.DefaultBinder" />. </param>
	/// <param name="returnType">The return type of the property. </param>
	/// <param name="types">An array of <see cref="T:System.Type" /> objects representing the number, order, and type of the parameters for the indexed property to get.-or- An empty array of the type <see cref="T:System.Type" /> (that is, Type[] types = new Type[0]) to get a property that is not indexed. </param>
	/// <param name="modifiers">An array of <see cref="T:System.Reflection.ParameterModifier" /> objects representing the attributes associated with the corresponding element in the <paramref name="types" /> array. The default binder does not process this parameter. </param>
	/// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one property is found with the specified name and matching the specified binding constraints. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null.-or- <paramref name="types" /> is null.-or- One of the elements in <paramref name="types" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="types" /> is multidimensional.-or- <paramref name="modifiers" /> is multidimensional.-or- <paramref name="types" /> and <paramref name="modifiers" /> do not have the same length. </exception>
	/// <exception cref="T:System.NotSupportedException">The current type is a <see cref="T:System.Reflection.Emit.TypeBuilder" />, <see cref="T:System.Reflection.Emit.EnumBuilder" />, or <see cref="T:System.Reflection.Emit.GenericTypeParameterBuilder" />.</exception>
	protected abstract PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers);

	/// <summary>When overridden in a derived class, searches for the properties of the current <see cref="T:System.Type" />, using the specified binding constraints.</summary>
	/// <returns>An array of <see cref="T:System.Reflection.PropertyInfo" /> objects representing all properties of the current <see cref="T:System.Type" /> that match the specified binding constraints.-or- An empty array of type <see cref="T:System.Reflection.PropertyInfo" />, if the current <see cref="T:System.Type" /> does not have properties, or if none of the properties match the binding constraints.</returns>
	/// <param name="bindingAttr">A bitmask comprised of one or more <see cref="T:System.Reflection.BindingFlags" /> that specify how the search is conducted.-or- Zero, to return null. </param>
	/// <filterpriority>2</filterpriority>
	public abstract PropertyInfo[] GetProperties(BindingFlags bindingAttr);

	/// <summary>Returns all the public properties of the current <see cref="T:System.Type" />.</summary>
	/// <returns>An array of <see cref="T:System.Reflection.PropertyInfo" /> objects representing all public properties of the current <see cref="T:System.Type" />.-or- An empty array of type <see cref="T:System.Reflection.PropertyInfo" />, if the current <see cref="T:System.Type" /> does not have public properties.</returns>
	/// <filterpriority>2</filterpriority>
	public PropertyInfo[] GetProperties()
	{
		return GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
	}

	/// <summary>Returns the public types nested in the current <see cref="T:System.Type" />.</summary>
	/// <returns>An array of <see cref="T:System.Type" /> objects representing the public types nested in the current <see cref="T:System.Type" /> (the search is not recursive), or an empty array of type <see cref="T:System.Type" /> if no public types are nested in the current <see cref="T:System.Type" />.</returns>
	/// <filterpriority>2</filterpriority>
	public Type[] GetNestedTypes()
	{
		return GetNestedTypes(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
	}

	/// <summary>When overridden in a derived class, searches for the types nested in the current <see cref="T:System.Type" />, using the specified binding constraints.</summary>
	/// <returns>An array of <see cref="T:System.Type" /> objects representing all the types nested in the current <see cref="T:System.Type" /> that match the specified binding constraints (the search is not recursive), or an empty array of type <see cref="T:System.Type" />, if no nested types are found that match the binding constraints.</returns>
	/// <param name="bindingAttr">A bitmask comprised of one or more <see cref="T:System.Reflection.BindingFlags" /> that specify how the search is conducted.-or- Zero, to return null. </param>
	/// <filterpriority>2</filterpriority>
	public abstract Type[] GetNestedTypes(BindingFlags bindingAttr);

	/// <summary>Searches for the public nested type with the specified name.</summary>
	/// <returns>An object representing the public nested type with the specified name, if found; otherwise, null.</returns>
	/// <param name="name">The string containing the name of the nested type to get. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <filterpriority>2</filterpriority>
	public Type GetNestedType(string name)
	{
		return GetNestedType(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
	}

	/// <summary>When overridden in a derived class, searches for the specified nested type, using the specified binding constraints.</summary>
	/// <returns>An object representing the nested type that matches the specified requirements, if found; otherwise, null.</returns>
	/// <param name="name">The string containing the name of the nested type to get. </param>
	/// <param name="bindingAttr">A bitmask comprised of one or more <see cref="T:System.Reflection.BindingFlags" /> that specify how the search is conducted.-or- Zero, to return null. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <filterpriority>2</filterpriority>
	public abstract Type GetNestedType(string name, BindingFlags bindingAttr);

	/// <summary>Searches for the public members with the specified name.</summary>
	/// <returns>An array of <see cref="T:System.Reflection.MemberInfo" /> objects representing the public members with the specified name, if found; otherwise, an empty array.</returns>
	/// <param name="name">The string containing the name of the public members to get. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <filterpriority>2</filterpriority>
	public MemberInfo[] GetMember(string name)
	{
		return GetMember(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
	}

	/// <summary>Searches for the specified members, using the specified binding constraints.</summary>
	/// <returns>An array of <see cref="T:System.Reflection.MemberInfo" /> objects representing the public members with the specified name, if found; otherwise, an empty array.</returns>
	/// <param name="name">The string containing the name of the members to get. </param>
	/// <param name="bindingAttr">A bitmask comprised of one or more <see cref="T:System.Reflection.BindingFlags" /> that specify how the search is conducted.-or- Zero, to return an empty array. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual MemberInfo[] GetMember(string name, BindingFlags bindingAttr)
	{
		return GetMember(name, MemberTypes.All, bindingAttr);
	}

	/// <summary>Searches for the specified members of the specified member type, using the specified binding constraints.</summary>
	/// <returns>An array of <see cref="T:System.Reflection.MemberInfo" /> objects representing the public members with the specified name, if found; otherwise, an empty array.</returns>
	/// <param name="name">The string containing the name of the members to get. </param>
	/// <param name="type">The value to search for. </param>
	/// <param name="bindingAttr">A bitmask comprised of one or more <see cref="T:System.Reflection.BindingFlags" /> that specify how the search is conducted.-or- Zero, to return an empty array. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <exception cref="T:System.NotSupportedException">A derived class must provide an implementation. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual MemberInfo[] GetMember(string name, MemberTypes type, BindingFlags bindingAttr)
	{
		throw new NotSupportedException(Environment.GetResourceString("Derived classes must provide an implementation."));
	}

	/// <summary>Returns all the public members of the current <see cref="T:System.Type" />.</summary>
	/// <returns>An array of <see cref="T:System.Reflection.MemberInfo" /> objects representing all the public members of the current <see cref="T:System.Type" />.-or- An empty array of type <see cref="T:System.Reflection.MemberInfo" />, if the current <see cref="T:System.Type" /> does not have public members.</returns>
	/// <filterpriority>2</filterpriority>
	public MemberInfo[] GetMembers()
	{
		return GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
	}

	/// <summary>When overridden in a derived class, searches for the members defined for the current <see cref="T:System.Type" />, using the specified binding constraints.</summary>
	/// <returns>An array of <see cref="T:System.Reflection.MemberInfo" /> objects representing all members defined for the current <see cref="T:System.Type" /> that match the specified binding constraints.-or- An empty array of type <see cref="T:System.Reflection.MemberInfo" />, if no members are defined for the current <see cref="T:System.Type" />, or if none of the defined members match the binding constraints.</returns>
	/// <param name="bindingAttr">A bitmask comprised of one or more <see cref="T:System.Reflection.BindingFlags" /> that specify how the search is conducted.-or- Zero (<see cref="F:System.Reflection.BindingFlags.Default" />), to return an empty array. </param>
	/// <filterpriority>2</filterpriority>
	public abstract MemberInfo[] GetMembers(BindingFlags bindingAttr);

	/// <summary>Searches for the members defined for the current <see cref="T:System.Type" /> whose <see cref="T:System.Reflection.DefaultMemberAttribute" /> is set.</summary>
	/// <returns>An array of <see cref="T:System.Reflection.MemberInfo" /> objects representing all default members of the current <see cref="T:System.Type" />.-or- An empty array of type <see cref="T:System.Reflection.MemberInfo" />, if the current <see cref="T:System.Type" /> does not have default members.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual MemberInfo[] GetDefaultMembers()
	{
		throw new NotImplementedException();
	}

	/// <summary>Returns a filtered array of <see cref="T:System.Reflection.MemberInfo" /> objects of the specified member type.</summary>
	/// <returns>A filtered array of <see cref="T:System.Reflection.MemberInfo" /> objects of the specified member type.-or- An empty array of type <see cref="T:System.Reflection.MemberInfo" />, if the current <see cref="T:System.Type" /> does not have members of type <paramref name="memberType" /> that match the filter criteria.</returns>
	/// <param name="memberType">An object that indicates the type of member to search for. </param>
	/// <param name="bindingAttr">A bitmask comprised of one or more <see cref="T:System.Reflection.BindingFlags" /> that specify how the search is conducted.-or- Zero, to return null. </param>
	/// <param name="filter">The delegate that does the comparisons, returning true if the member currently being inspected matches the <paramref name="filterCriteria" /> and false otherwise. You can use the FilterAttribute, FilterName, and FilterNameIgnoreCase delegates supplied by this class. The first uses the fields of FieldAttributes, MethodAttributes, and MethodImplAttributes as search criteria, and the other two delegates use String objects as the search criteria. </param>
	/// <param name="filterCriteria">The search criteria that determines whether a member is returned in the array of MemberInfo objects.The fields of FieldAttributes, MethodAttributes, and MethodImplAttributes can be used in conjunction with the FilterAttribute delegate supplied by this class. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="filter" /> is null. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual MemberInfo[] FindMembers(MemberTypes memberType, BindingFlags bindingAttr, MemberFilter filter, object filterCriteria)
	{
		MethodInfo[] array = null;
		ConstructorInfo[] array2 = null;
		FieldInfo[] array3 = null;
		PropertyInfo[] array4 = null;
		EventInfo[] array5 = null;
		Type[] array6 = null;
		int num = 0;
		int num2 = 0;
		if ((memberType & MemberTypes.Method) != 0)
		{
			array = GetMethods(bindingAttr);
			if (filter != null)
			{
				for (num = 0; num < array.Length; num++)
				{
					if (!filter(array[num], filterCriteria))
					{
						array[num] = null;
					}
					else
					{
						num2++;
					}
				}
			}
			else
			{
				num2 += array.Length;
			}
		}
		if ((memberType & MemberTypes.Constructor) != 0)
		{
			array2 = GetConstructors(bindingAttr);
			if (filter != null)
			{
				for (num = 0; num < array2.Length; num++)
				{
					if (!filter(array2[num], filterCriteria))
					{
						array2[num] = null;
					}
					else
					{
						num2++;
					}
				}
			}
			else
			{
				num2 += array2.Length;
			}
		}
		if ((memberType & MemberTypes.Field) != 0)
		{
			array3 = GetFields(bindingAttr);
			if (filter != null)
			{
				for (num = 0; num < array3.Length; num++)
				{
					if (!filter(array3[num], filterCriteria))
					{
						array3[num] = null;
					}
					else
					{
						num2++;
					}
				}
			}
			else
			{
				num2 += array3.Length;
			}
		}
		if ((memberType & MemberTypes.Property) != 0)
		{
			array4 = GetProperties(bindingAttr);
			if (filter != null)
			{
				for (num = 0; num < array4.Length; num++)
				{
					if (!filter(array4[num], filterCriteria))
					{
						array4[num] = null;
					}
					else
					{
						num2++;
					}
				}
			}
			else
			{
				num2 += array4.Length;
			}
		}
		if ((memberType & MemberTypes.Event) != 0)
		{
			array5 = GetEvents(bindingAttr);
			if (filter != null)
			{
				for (num = 0; num < array5.Length; num++)
				{
					if (!filter(array5[num], filterCriteria))
					{
						array5[num] = null;
					}
					else
					{
						num2++;
					}
				}
			}
			else
			{
				num2 += array5.Length;
			}
		}
		if ((memberType & MemberTypes.NestedType) != 0)
		{
			array6 = GetNestedTypes(bindingAttr);
			if (filter != null)
			{
				for (num = 0; num < array6.Length; num++)
				{
					if (!filter(array6[num], filterCriteria))
					{
						array6[num] = null;
					}
					else
					{
						num2++;
					}
				}
			}
			else
			{
				num2 += array6.Length;
			}
		}
		MemberInfo[] array7 = new MemberInfo[num2];
		num2 = 0;
		if (array != null)
		{
			for (num = 0; num < array.Length; num++)
			{
				if (array[num] != null)
				{
					array7[num2++] = array[num];
				}
			}
		}
		if (array2 != null)
		{
			for (num = 0; num < array2.Length; num++)
			{
				if (array2[num] != null)
				{
					array7[num2++] = array2[num];
				}
			}
		}
		if (array3 != null)
		{
			for (num = 0; num < array3.Length; num++)
			{
				if (array3[num] != null)
				{
					array7[num2++] = array3[num];
				}
			}
		}
		if (array4 != null)
		{
			for (num = 0; num < array4.Length; num++)
			{
				if (array4[num] != null)
				{
					array7[num2++] = array4[num];
				}
			}
		}
		if (array5 != null)
		{
			for (num = 0; num < array5.Length; num++)
			{
				if (array5[num] != null)
				{
					array7[num2++] = array5[num];
				}
			}
		}
		if (array6 != null)
		{
			for (num = 0; num < array6.Length; num++)
			{
				if (array6[num] != null)
				{
					array7[num2++] = array6[num];
				}
			}
		}
		return array7;
	}

	/// <summary>Returns an array of <see cref="T:System.Type" /> objects that represent the constraints on the current generic type parameter. </summary>
	/// <returns>An array of <see cref="T:System.Type" /> objects that represent the constraints on the current generic type parameter.</returns>
	/// <exception cref="T:System.InvalidOperationException">The current <see cref="T:System.Type" /> object is not a generic type parameter. That is, the <see cref="P:System.Type.IsGenericParameter" /> property returns false.</exception>
	/// <filterpriority>1</filterpriority>
	public virtual Type[] GetGenericParameterConstraints()
	{
		if (!IsGenericParameter)
		{
			throw new InvalidOperationException(Environment.GetResourceString("Method may only be called on a Type for which Type.IsGenericParameter is true."));
		}
		throw new InvalidOperationException();
	}

	/// <summary>Implements the <see cref="P:System.Type.IsValueType" /> property and determines whether the <see cref="T:System.Type" /> is a value type; that is, not a class or an interface.</summary>
	/// <returns>true if the <see cref="T:System.Type" /> is a value type; otherwise, false.</returns>
	protected virtual bool IsValueTypeImpl()
	{
		return IsSubclassOf(RuntimeType.ValueType);
	}

	/// <summary>When overridden in a derived class, implements the <see cref="P:System.Type.Attributes" /> property and gets a bitmask indicating the attributes associated with the <see cref="T:System.Type" />.</summary>
	/// <returns>A <see cref="T:System.Reflection.TypeAttributes" /> object representing the attribute set of the <see cref="T:System.Type" />.</returns>
	protected abstract TypeAttributes GetAttributeFlagsImpl();

	/// <summary>When overridden in a derived class, implements the <see cref="P:System.Type.IsArray" /> property and determines whether the <see cref="T:System.Type" /> is an array.</summary>
	/// <returns>true if the <see cref="T:System.Type" /> is an array; otherwise, false.</returns>
	protected abstract bool IsArrayImpl();

	/// <summary>When overridden in a derived class, implements the <see cref="P:System.Type.IsByRef" /> property and determines whether the <see cref="T:System.Type" /> is passed by reference.</summary>
	/// <returns>true if the <see cref="T:System.Type" /> is passed by reference; otherwise, false.</returns>
	protected abstract bool IsByRefImpl();

	/// <summary>When overridden in a derived class, implements the <see cref="P:System.Type.IsPointer" /> property and determines whether the <see cref="T:System.Type" /> is a pointer.</summary>
	/// <returns>true if the <see cref="T:System.Type" /> is a pointer; otherwise, false.</returns>
	protected abstract bool IsPointerImpl();

	/// <summary>When overridden in a derived class, implements the <see cref="P:System.Type.IsPrimitive" /> property and determines whether the <see cref="T:System.Type" /> is one of the primitive types.</summary>
	/// <returns>true if the <see cref="T:System.Type" /> is one of the primitive types; otherwise, false.</returns>
	protected abstract bool IsPrimitiveImpl();

	/// <summary>When overridden in a derived class, implements the <see cref="P:System.Type.IsCOMObject" /> property and determines whether the <see cref="T:System.Type" /> is a COM object.</summary>
	/// <returns>true if the <see cref="T:System.Type" /> is a COM object; otherwise, false.</returns>
	protected abstract bool IsCOMObjectImpl();

	internal virtual bool IsWindowsRuntimeObjectImpl()
	{
		throw new NotImplementedException();
	}

	internal virtual bool IsExportedToWindowsRuntimeImpl()
	{
		throw new NotImplementedException();
	}

	/// <summary>Substitutes the elements of an array of types for the type parameters of the current generic type definition and returns a <see cref="T:System.Type" /> object representing the resulting constructed type.</summary>
	/// <returns>A <see cref="T:System.Type" /> representing the constructed type formed by substituting the elements of <paramref name="typeArguments" /> for the type parameters of the current generic type.</returns>
	/// <param name="typeArguments">An array of types to be substituted for the type parameters of the current generic type.</param>
	/// <exception cref="T:System.InvalidOperationException">The current type does not represent a generic type definition. That is, <see cref="P:System.Type.IsGenericTypeDefinition" /> returns false. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="typeArguments" /> is null.-or- Any element of <paramref name="typeArguments" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">The number of elements in <paramref name="typeArguments" /> is not the same as the number of type parameters in the current generic type definition.-or- Any element of <paramref name="typeArguments" /> does not satisfy the constraints specified for the corresponding type parameter of the current generic type. -or- <paramref name="typeArguments" /> contains an element that is a pointer type (<see cref="P:System.Type.IsPointer" /> returns true), a by-ref type (<see cref="P:System.Type.IsByRef" /> returns true), or <see cref="T:System.Void" />.</exception>
	/// <exception cref="T:System.NotSupportedException">The invoked method is not supported in the base class. Derived classes must provide an implementation.</exception>
	public virtual Type MakeGenericType(params Type[] typeArguments)
	{
		throw new NotSupportedException(Environment.GetResourceString("Derived classes must provide an implementation."));
	}

	/// <summary>Implements the <see cref="P:System.Type.IsContextful" /> property and determines whether the <see cref="T:System.Type" /> can be hosted in a context.</summary>
	/// <returns>true if the <see cref="T:System.Type" /> can be hosted in a context; otherwise, false.</returns>
	protected virtual bool IsContextfulImpl()
	{
		return typeof(ContextBoundObject).IsAssignableFrom(this);
	}

	/// <summary>Implements the <see cref="P:System.Type.IsMarshalByRef" /> property and determines whether the <see cref="T:System.Type" /> is marshaled by reference.</summary>
	/// <returns>true if the <see cref="T:System.Type" /> is marshaled by reference; otherwise, false.</returns>
	protected virtual bool IsMarshalByRefImpl()
	{
		return typeof(MarshalByRefObject).IsAssignableFrom(this);
	}

	internal virtual bool HasProxyAttributeImpl()
	{
		return false;
	}

	/// <summary>When overridden in a derived class, returns the <see cref="T:System.Type" /> of the object encompassed or referred to by the current array, pointer or reference type.</summary>
	/// <returns>The <see cref="T:System.Type" /> of the object encompassed or referred to by the current array, pointer, or reference type, or null if the current <see cref="T:System.Type" /> is not an array or a pointer, or is not passed by reference, or represents a generic type or a type parameter in the definition of a generic type or generic method.</returns>
	/// <filterpriority>2</filterpriority>
	public abstract Type GetElementType();

	/// <summary>Returns an array of <see cref="T:System.Type" /> objects that represent the type arguments of a generic type or the type parameters of a generic type definition.</summary>
	/// <returns>An array of <see cref="T:System.Type" /> objects that represent the type arguments of a generic type. Returns an empty array if the current type is not a generic type.</returns>
	/// <exception cref="T:System.NotSupportedException">The invoked method is not supported in the base class. Derived classes must provide an implementation.</exception>
	/// <filterpriority>2</filterpriority>
	public virtual Type[] GetGenericArguments()
	{
		throw new NotSupportedException(Environment.GetResourceString("Derived classes must provide an implementation."));
	}

	/// <summary>Returns a <see cref="T:System.Type" /> object that represents a generic type definition from which the current generic type can be constructed.</summary>
	/// <returns>A <see cref="T:System.Type" /> object representing a generic type from which the current type can be constructed.</returns>
	/// <exception cref="T:System.InvalidOperationException">The current type is not a generic type.  That is, <see cref="P:System.Type.IsGenericType" /> returns false. </exception>
	/// <exception cref="T:System.NotSupportedException">The invoked method is not supported in the base class. Derived classes must provide an implementation.</exception>
	/// <filterpriority>2</filterpriority>
	public virtual Type GetGenericTypeDefinition()
	{
		throw new NotSupportedException(Environment.GetResourceString("Derived classes must provide an implementation."));
	}

	/// <summary>When overridden in a derived class, implements the <see cref="P:System.Type.HasElementType" /> property and determines whether the current <see cref="T:System.Type" /> encompasses or refers to another type; that is, whether the current <see cref="T:System.Type" /> is an array, a pointer, or is passed by reference.</summary>
	/// <returns>true if the <see cref="T:System.Type" /> is an array, a pointer, or is passed by reference; otherwise, false.</returns>
	protected abstract bool HasElementTypeImpl();

	internal Type GetRootElementType()
	{
		Type type = this;
		while (type.HasElementType)
		{
			type = type.GetElementType();
		}
		return type;
	}

	/// <summary>Returns the names of the members of the current enumeration type.</summary>
	/// <returns>An array that contains the names of the members of the enumeration.</returns>
	/// <exception cref="T:System.ArgumentException">The current type is not an enumeration.</exception>
	public virtual string[] GetEnumNames()
	{
		if (!IsEnum)
		{
			throw new ArgumentException(Environment.GetResourceString("Type provided must be an Enum."), "enumType");
		}
		GetEnumData(out var enumNames, out var _);
		return enumNames;
	}

	/// <summary>Returns an array of the values of the constants in the current enumeration type.</summary>
	/// <returns>An array that contains the values. The elements of the array are sorted by the binary values (that is, the unsigned values) of the enumeration constants.</returns>
	/// <exception cref="T:System.ArgumentException">The current type is not an enumeration.</exception>
	public virtual Array GetEnumValues()
	{
		if (!IsEnum)
		{
			throw new ArgumentException(Environment.GetResourceString("Type provided must be an Enum."), "enumType");
		}
		throw new NotImplementedException();
	}

	private Array GetEnumRawConstantValues()
	{
		GetEnumData(out var _, out var enumValues);
		return enumValues;
	}

	private void GetEnumData(out string[] enumNames, out Array enumValues)
	{
		FieldInfo[] fields = GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		object[] array = new object[fields.Length];
		string[] array2 = new string[fields.Length];
		for (int i = 0; i < fields.Length; i++)
		{
			array2[i] = fields[i].Name;
			array[i] = fields[i].GetRawConstantValue();
		}
		IComparer @default = Comparer.Default;
		for (int j = 1; j < array.Length; j++)
		{
			int num = j;
			string text = array2[j];
			object obj = array[j];
			bool flag = false;
			while (@default.Compare(array[num - 1], obj) > 0)
			{
				array2[num] = array2[num - 1];
				array[num] = array[num - 1];
				num--;
				flag = true;
				if (num == 0)
				{
					break;
				}
			}
			if (flag)
			{
				array2[num] = text;
				array[num] = obj;
			}
		}
		enumNames = array2;
		enumValues = array;
	}

	/// <summary>Returns the underlying type of the current enumeration type. </summary>
	/// <returns>The underlying type of the current enumeration.</returns>
	/// <exception cref="T:System.ArgumentException">The current type is not an enumeration.-or-The enumeration type is not valid, because it contains more than one instance field.</exception>
	public virtual Type GetEnumUnderlyingType()
	{
		if (!IsEnum)
		{
			throw new ArgumentException(Environment.GetResourceString("Type provided must be an Enum."), "enumType");
		}
		FieldInfo[] fields = GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (fields == null || fields.Length != 1)
		{
			throw new ArgumentException(Environment.GetResourceString("The Enum type should contain one and only one instance field."), "enumType");
		}
		return fields[0].FieldType;
	}

	/// <summary>Returns a value that indicates whether the specified value exists in the current enumeration type.</summary>
	/// <returns>true if the specified value is a member of the current enumeration type; otherwise, false.</returns>
	/// <param name="value">The value to be tested.</param>
	/// <exception cref="T:System.ArgumentException">The current type is not an enumeration.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="value" /> is of a type that cannot be the underlying type of an enumeration.</exception>
	public virtual bool IsEnumDefined(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!IsEnum)
		{
			throw new ArgumentException(Environment.GetResourceString("Type provided must be an Enum."), "enumType");
		}
		Type type = value.GetType();
		if (type.IsEnum)
		{
			if (!type.IsEquivalentTo(this))
			{
				throw new ArgumentException(Environment.GetResourceString("Object must be the same type as the enum. The type passed in was '{0}'; the enum type was '{1}'.", type.ToString(), ToString()));
			}
			type = type.GetEnumUnderlyingType();
		}
		if (type == typeof(string))
		{
			if (Array.IndexOf(GetEnumNames(), value) >= 0)
			{
				return true;
			}
			return false;
		}
		if (IsIntegerType(type))
		{
			Type enumUnderlyingType = GetEnumUnderlyingType();
			if (enumUnderlyingType.GetTypeCodeImpl() != type.GetTypeCodeImpl())
			{
				throw new ArgumentException(Environment.GetResourceString("Enum underlying type and the object must be same type or object must be a String. Type passed in was '{0}'; the enum underlying type was '{1}'.", type.ToString(), enumUnderlyingType.ToString()));
			}
			return BinarySearch(GetEnumRawConstantValues(), value) >= 0;
		}
		if (CompatibilitySwitches.IsAppEarlierThanWindowsPhone8)
		{
			throw new ArgumentException(Environment.GetResourceString("Enum underlying type and the object must be same type or object must be a String. Type passed in was '{0}'; the enum underlying type was '{1}'.", type.ToString(), GetEnumUnderlyingType()));
		}
		throw new InvalidOperationException(Environment.GetResourceString("Unknown enum type."));
	}

	/// <summary>Returns the name of the constant that has the specified value, for the current enumeration type.</summary>
	/// <returns>The name of the member of the current enumeration type that has the specified value, or null if no such constant is found.</returns>
	/// <param name="value">The value whose name is to be retrieved.</param>
	/// <exception cref="T:System.ArgumentException">The current type is not an enumeration.-or-<paramref name="value" /> is neither of the current type nor does it have the same underlying type as the current type.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	public virtual string GetEnumName(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!IsEnum)
		{
			throw new ArgumentException(Environment.GetResourceString("Type provided must be an Enum."), "enumType");
		}
		Type type = value.GetType();
		if (!type.IsEnum && !IsIntegerType(type))
		{
			throw new ArgumentException(Environment.GetResourceString("The value passed in must be an enum base or an underlying type for an enum, such as an Int32."), "value");
		}
		int num = BinarySearch(GetEnumRawConstantValues(), value);
		if (num >= 0)
		{
			return GetEnumNames()[num];
		}
		return null;
	}

	private static int BinarySearch(Array array, object value)
	{
		ulong[] array2 = new ulong[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array2[i] = Enum.ToUInt64(array.GetValue(i));
		}
		ulong value2 = Enum.ToUInt64(value);
		return Array.BinarySearch(array2, value2);
	}

	internal static bool IsIntegerType(Type t)
	{
		if (!(t == typeof(int)) && !(t == typeof(short)) && !(t == typeof(ushort)) && !(t == typeof(byte)) && !(t == typeof(sbyte)) && !(t == typeof(uint)) && !(t == typeof(long)) && !(t == typeof(ulong)) && !(t == typeof(char)))
		{
			return t == typeof(bool);
		}
		return true;
	}

	/// <summary>Determines whether the class represented by the current <see cref="T:System.Type" /> derives from the class represented by the specified <see cref="T:System.Type" />.</summary>
	/// <returns>true if the Type represented by the <paramref name="c" /> parameter and the current Type represent classes, and the class represented by the current Type derives from the class represented by <paramref name="c" />; otherwise, false. This method also returns false if <paramref name="c" /> and the current Type represent the same class.</returns>
	/// <param name="c">The type to compare with the current type. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="c" /> parameter is null. </exception>
	/// <filterpriority>2</filterpriority>
	[ComVisible(true)]
	public virtual bool IsSubclassOf(Type c)
	{
		Type type = this;
		if (type == c)
		{
			return false;
		}
		while (type != null)
		{
			if (type == c)
			{
				return true;
			}
			type = type.BaseType;
		}
		return false;
	}

	/// <summary>Determines whether the specified object is an instance of the current <see cref="T:System.Type" />.</summary>
	/// <returns>true if the current Type is in the inheritance hierarchy of the object represented by <paramref name="o" />, or if the current Type is an interface that <paramref name="o" /> supports. false if neither of these conditions is the case, or if <paramref name="o" /> is null, or if the current Type is an open generic type (that is, <see cref="P:System.Type.ContainsGenericParameters" /> returns true).</returns>
	/// <param name="o">The object to compare with the current type. </param>
	/// <filterpriority>2</filterpriority>
	public virtual bool IsInstanceOfType(object o)
	{
		if (o == null)
		{
			return false;
		}
		return IsAssignableFrom(o.GetType());
	}

	/// <summary>Determines whether an instance of the current <see cref="T:System.Type" /> can be assigned from an instance of the specified Type.</summary>
	/// <returns>true if <paramref name="c" /> and the current Type represent the same type, or if the current Type is in the inheritance hierarchy of <paramref name="c" />, or if the current Type is an interface that <paramref name="c" /> implements, or if <paramref name="c" /> is a generic type parameter and the current Type represents one of the constraints of <paramref name="c" />, or if <paramref name="c" /> represents a value type and the current Type represents Nullable&lt;c&gt; (Nullable(Of c) in Visual Basic). false if none of these conditions are true, or if <paramref name="c" /> is null.</returns>
	/// <param name="c">The type to compare with the current type. </param>
	/// <filterpriority>2</filterpriority>
	public virtual bool IsAssignableFrom(Type c)
	{
		if (c == null)
		{
			return false;
		}
		if (this == c)
		{
			return true;
		}
		RuntimeType runtimeType = UnderlyingSystemType as RuntimeType;
		if (runtimeType != null)
		{
			return runtimeType.IsAssignableFrom(c);
		}
		if (c.IsSubclassOf(this))
		{
			return true;
		}
		if (IsInterface)
		{
			return c.ImplementInterface(this);
		}
		if (IsGenericParameter)
		{
			Type[] genericParameterConstraints = GetGenericParameterConstraints();
			for (int i = 0; i < genericParameterConstraints.Length; i++)
			{
				if (!genericParameterConstraints[i].IsAssignableFrom(c))
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	/// <summary>Determines whether two COM types have the same identity and are eligible for type equivalence.</summary>
	/// <returns>true if the COM types are equivalent; otherwise, false. This method also returns false if one type is in an assembly that is loaded for execution, and the other is in an assembly that is loaded into the reflection-only context.</returns>
	/// <param name="other">The COM type that is tested for equivalence with the current type.</param>
	public virtual bool IsEquivalentTo(Type other)
	{
		return this == other;
	}

	internal bool ImplementInterface(Type ifaceType)
	{
		Type type = this;
		while (type != null)
		{
			Type[] interfaces = type.GetInterfaces();
			if (interfaces != null)
			{
				for (int i = 0; i < interfaces.Length; i++)
				{
					if (interfaces[i] == ifaceType || (interfaces[i] != null && interfaces[i].ImplementInterface(ifaceType)))
					{
						return true;
					}
				}
			}
			type = type.BaseType;
		}
		return false;
	}

	internal string FormatTypeName()
	{
		return FormatTypeName(serialization: false);
	}

	internal virtual string FormatTypeName(bool serialization)
	{
		throw new NotImplementedException();
	}

	/// <summary>Returns a String representing the name of the current Type.</summary>
	/// <returns>A <see cref="T:System.String" /> representing the name of the current <see cref="T:System.Type" />.</returns>
	/// <filterpriority>2</filterpriority>
	public override string ToString()
	{
		return "Type: " + Name;
	}

	/// <summary>Gets the types of the objects in the specified array.</summary>
	/// <returns>An array of <see cref="T:System.Type" /> objects representing the types of the corresponding elements in <paramref name="args" />.</returns>
	/// <param name="args">An array of objects whose types to determine. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="args" /> is null. </exception>
	/// <exception cref="T:System.Reflection.TargetInvocationException">The class initializers are invoked and at least one throws an exception. </exception>
	/// <filterpriority>1</filterpriority>
	public static Type[] GetTypeArray(object[] args)
	{
		if (args == null)
		{
			throw new ArgumentNullException("args");
		}
		Type[] array = new Type[args.Length];
		for (int i = 0; i < array.Length; i++)
		{
			if (args[i] == null)
			{
				throw new ArgumentNullException();
			}
			array[i] = args[i].GetType();
		}
		return array;
	}

	/// <summary>Determines if the underlying system type of the current <see cref="T:System.Type" /> is the same as the underlying system type of the specified <see cref="T:System.Object" />.</summary>
	/// <returns>true if the underlying system type of <paramref name="o" /> is the same as the underlying system type of the current <see cref="T:System.Type" />; otherwise, false. This method also returns false if the object specified by the <paramref name="o" /> parameter is not a Type.</returns>
	/// <param name="o">The object whose underlying system type is to be compared with the underlying system type of the current <see cref="T:System.Type" />. </param>
	/// <filterpriority>2</filterpriority>
	public override bool Equals(object o)
	{
		if (o == null)
		{
			return false;
		}
		return Equals(o as Type);
	}

	/// <summary>Determines if the underlying system type of the current <see cref="T:System.Type" /> is the same as the underlying system type of the specified <see cref="T:System.Type" />.</summary>
	/// <returns>true if the underlying system type of <paramref name="o" /> is the same as the underlying system type of the current <see cref="T:System.Type" />; otherwise, false.</returns>
	/// <param name="o">The object whose underlying system type is to be compared with the underlying system type of the current <see cref="T:System.Type" />. </param>
	/// <filterpriority>2</filterpriority>
	public virtual bool Equals(Type o)
	{
		if ((object)o == null)
		{
			return false;
		}
		return (object)UnderlyingSystemType == o.UnderlyingSystemType;
	}

	/// <summary>Indicates whether two <see cref="T:System.Type" /> objects are equal.</summary>
	/// <returns>true if <paramref name="left" /> is equal to <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">The first object to compare.</param>
	/// <param name="right">The second object to compare.</param>
	public static bool operator ==(Type left, Type right)
	{
		return (object)left == right;
	}

	/// <summary>Indicates whether two <see cref="T:System.Type" /> objects are not equal.</summary>
	/// <returns>true if <paramref name="left" /> is not equal to <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">The first object to compare.</param>
	/// <param name="right">The second object to compare.</param>
	public static bool operator !=(Type left, Type right)
	{
		return (object)left != right;
	}

	/// <summary>Returns the hash code for this instance.</summary>
	/// <returns>The hash code for this instance.</returns>
	/// <filterpriority>2</filterpriority>
	public override int GetHashCode()
	{
		Type underlyingSystemType = UnderlyingSystemType;
		if ((object)underlyingSystemType != this)
		{
			return underlyingSystemType.GetHashCode();
		}
		return base.GetHashCode();
	}

	/// <summary>Returns an interface mapping for the specified interface type.</summary>
	/// <returns>An object that represents the interface mapping for <paramref name="interfaceType" />.</returns>
	/// <param name="interfaceType">The interface type to retrieve a mapping for. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="interfaceType" /> is not implemented by the current type. -or-The <paramref name="interfaceType" /> parameter does not refer to an interface. -or-<paramref name="interfaceType" /> is a generic interface, and the current type is an array type. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="interfaceType" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The current <see cref="T:System.Type" /> represents a generic type parameter; that is, <see cref="P:System.Type.IsGenericParameter" /> is true. </exception>
	/// <exception cref="T:System.NotSupportedException">The invoked method is not supported in the base class. Derived classes must provide an implementation.</exception>
	/// <filterpriority>2</filterpriority>
	[ComVisible(true)]
	public virtual InterfaceMapping GetInterfaceMap(Type interfaceType)
	{
		throw new NotSupportedException(Environment.GetResourceString("Derived classes must provide an implementation."));
	}

	/// <summary>Gets the current <see cref="T:System.Type" />.</summary>
	/// <returns>The current <see cref="T:System.Type" />.</returns>
	/// <exception cref="T:System.Reflection.TargetInvocationException">A class initializer is invoked and throws an exception. </exception>
	/// <filterpriority>2</filterpriority>
	public new Type GetType()
	{
		return base.GetType();
	}

	/// <summary>Retrieves the number of type information interfaces that an object provides (either 0 or 1).</summary>
	/// <param name="pcTInfo">Points to a location that receives the number of type information interfaces provided by the object.</param>
	/// <exception cref="T:System.NotImplementedException">Late-bound access using the COM IDispatch interface is not supported.</exception>
	void _Type.GetTypeInfoCount(out uint pcTInfo)
	{
		throw new NotImplementedException();
	}

	/// <summary>Retrieves the type information for an object, which can then be used to get the type information for an interface.</summary>
	/// <param name="iTInfo">The type information to return.</param>
	/// <param name="lcid">The locale identifier for the type information.</param>
	/// <param name="ppTInfo">A pointer to the requested type information object.</param>
	/// <exception cref="T:System.NotImplementedException">Late-bound access using the COM IDispatch interface is not supported.</exception>
	void _Type.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
	{
		throw new NotImplementedException();
	}

	/// <summary>Maps a set of names to a corresponding set of dispatch identifiers.</summary>
	/// <param name="riid">Reserved for future use. Must be IID_NULL.</param>
	/// <param name="rgszNames">Passed-in array of names to be mapped.</param>
	/// <param name="cNames">Count of the names to be mapped.</param>
	/// <param name="lcid">The locale context in which to interpret the names.</param>
	/// <param name="rgDispId">Caller-allocated array which receives the IDs corresponding to the names.</param>
	/// <exception cref="T:System.NotImplementedException">Late-bound access using the COM IDispatch interface is not supported.</exception>
	void _Type.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
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
	void _Type.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
	{
		throw new NotImplementedException();
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern Type internal_from_name(string name, bool throwOnError, bool ignoreCase);

	/// <summary>Gets the <see cref="T:System.Type" /> with the specified name, performing a case-sensitive search.</summary>
	/// <returns>The type with the specified name, if found; otherwise, null.</returns>
	/// <param name="typeName">The assembly-qualified name of the type to get. See <see cref="P:System.Type.AssemblyQualifiedName" />. If the type is in the currently executing assembly or in Mscorlib.dll, it is sufficient to supply the type name qualified by its namespace.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="typeName" /> is null. </exception>
	/// <exception cref="T:System.Reflection.TargetInvocationException">A class initializer is invoked and throws an exception. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="typeName" /> represents a generic type that has a pointer type, a ByRef type, or <see cref="T:System.Void" /> as one of its type arguments.-or-<paramref name="typeName" /> represents a generic type that has an incorrect number of type arguments.-or-<paramref name="typeName" /> represents a generic type, and one of its type arguments does not satisfy the constraints for the corresponding type parameter.</exception>
	/// <exception cref="T:System.TypeLoadException">
	///   <paramref name="typeName" /> represents an array of <see cref="T:System.TypedReference" />. </exception>
	/// <exception cref="T:System.IO.FileLoadException">NoteIn the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.IO.IOException" />, instead.The assembly or one of its dependencies was found, but could not be loaded. </exception>
	/// <exception cref="T:System.BadImageFormatException">The assembly or one of its dependencies is not valid. -or-Version 2.0 or later of the common language runtime is currently loaded, and the assembly was compiled with a later version.</exception>
	/// <filterpriority>1</filterpriority>
	public static Type GetType(string typeName)
	{
		return GetType(typeName, throwOnError: false, ignoreCase: false);
	}

	/// <summary>Gets the <see cref="T:System.Type" /> with the specified name, performing a case-sensitive search and specifying whether to throw an exception if the type is not found.</summary>
	/// <returns>The type with the specified name. If the type is not found, the <paramref name="throwOnError" /> parameter specifies whether null is returned or an exception is thrown. In some cases, an exception is thrown regardless of the value of <paramref name="throwOnError" />. See the Exceptions section. </returns>
	/// <param name="typeName">The assembly-qualified name of the type to get. See <see cref="P:System.Type.AssemblyQualifiedName" />. If the type is in the currently executing assembly or in Mscorlib.dll, it is sufficient to supply the type name qualified by its namespace.</param>
	/// <param name="throwOnError">true to throw an exception if the type cannot be found; false to return null. Specifying false also suppresses some other exception conditions, but not all of them. See the Exceptions section.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="typeName" /> is null. </exception>
	/// <exception cref="T:System.Reflection.TargetInvocationException">A class initializer is invoked and throws an exception. </exception>
	/// <exception cref="T:System.TypeLoadException">
	///   <paramref name="throwOnError" /> is true and the type is not found. -or-<paramref name="throwOnError" /> is true and <paramref name="typeName" /> contains invalid characters, such as an embedded tab.-or-<paramref name="throwOnError" /> is true and <paramref name="typeName" /> is an empty string.-or-<paramref name="throwOnError" /> is true and <paramref name="typeName" /> represents an array type with an invalid size. -or-<paramref name="typeName" /> represents an array of <see cref="T:System.TypedReference" />. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="throwOnError" /> is true and <paramref name="typeName" /> contains invalid syntax. For example, "MyType[,*,]".-or- <paramref name="typeName" /> represents a generic type that has a pointer type, a ByRef type, or <see cref="T:System.Void" /> as one of its type arguments.-or-<paramref name="typeName" /> represents a generic type that has an incorrect number of type arguments.-or-<paramref name="typeName" /> represents a generic type, and one of its type arguments does not satisfy the constraints for the corresponding type parameter.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="throwOnError" /> is true and the assembly or one of its dependencies was not found. </exception>
	/// <exception cref="T:System.IO.FileLoadException">NoteIn the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.IO.IOException" />, instead.The assembly or one of its dependencies was found, but could not be loaded. </exception>
	/// <exception cref="T:System.BadImageFormatException">The assembly or one of its dependencies is not valid. -or-Version 2.0 or later of the common language runtime is currently loaded, and the assembly was compiled with a later version.</exception>
	/// <filterpriority>1</filterpriority>
	public static Type GetType(string typeName, bool throwOnError)
	{
		return GetType(typeName, throwOnError, ignoreCase: false);
	}

	/// <summary>Gets the <see cref="T:System.Type" /> with the specified name, specifying whether to perform a case-sensitive search and whether to throw an exception if the type is not found.</summary>
	/// <returns>The type with the specified name. If the type is not found, the <paramref name="throwOnError" /> parameter specifies whether null is returned or an exception is thrown. In some cases, an exception is thrown regardless of the value of <paramref name="throwOnError" />. See the Exceptions section. </returns>
	/// <param name="typeName">The assembly-qualified name of the type to get. See <see cref="P:System.Type.AssemblyQualifiedName" />. If the type is in the currently executing assembly or in Mscorlib.dll, it is sufficient to supply the type name qualified by its namespace.</param>
	/// <param name="throwOnError">true to throw an exception if the type cannot be found; false to return null.Specifying false also suppresses some other exception conditions, but not all of them. See the Exceptions section.</param>
	/// <param name="ignoreCase">true to perform a case-insensitive search for <paramref name="typeName" />, false to perform a case-sensitive search for <paramref name="typeName" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="typeName" /> is null. </exception>
	/// <exception cref="T:System.Reflection.TargetInvocationException">A class initializer is invoked and throws an exception. </exception>
	/// <exception cref="T:System.TypeLoadException">
	///   <paramref name="throwOnError" /> is true and the type is not found. -or-<paramref name="throwOnError" /> is true and <paramref name="typeName" /> contains invalid characters, such as an embedded tab.-or-<paramref name="throwOnError" /> is true and <paramref name="typeName" /> is an empty string.-or-<paramref name="throwOnError" /> is true and <paramref name="typeName" /> represents an array type with an invalid size. -or-<paramref name="typeName" /> represents an array of <see cref="T:System.TypedReference" />. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="throwOnError" /> is true and <paramref name="typeName" /> contains invalid syntax. For example, "MyType[,*,]".-or- <paramref name="typeName" /> represents a generic type that has a pointer type, a ByRef type, or <see cref="T:System.Void" /> as one of its type arguments.-or-<paramref name="typeName" /> represents a generic type that has an incorrect number of type arguments.-or-<paramref name="typeName" /> represents a generic type, and one of its type arguments does not satisfy the constraints for the corresponding type parameter.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="throwOnError" /> is true and the assembly or one of its dependencies was not found. </exception>
	/// <exception cref="T:System.IO.FileLoadException">The assembly or one of its dependencies was found, but could not be loaded. </exception>
	/// <exception cref="T:System.BadImageFormatException">The assembly or one of its dependencies is not valid. -or-Version 2.0 or later of the common language runtime is currently loaded, and the assembly was compiled with a later version.</exception>
	/// <filterpriority>1</filterpriority>
	public static Type GetType(string typeName, bool throwOnError, bool ignoreCase)
	{
		if (typeName == null)
		{
			throw new ArgumentNullException("TypeName");
		}
		if (typeName == string.Empty)
		{
			if (throwOnError)
			{
				throw new TypeLoadException("A null or zero length string does not represent a valid Type.");
			}
			return null;
		}
		Type type = internal_from_name(typeName, throwOnError, ignoreCase);
		if (throwOnError && type == null)
		{
			throw new TypeLoadException("Error loading '" + typeName + "'");
		}
		return type;
	}

	/// <summary>Gets the <see cref="T:System.Type" /> with the specified name, specifying whether to perform a case-sensitive search and whether to throw an exception if the type is not found. The type is loaded for reflection only, not for execution.</summary>
	/// <returns>The type with the specified name, if found; otherwise, null. If the type is not found, the <paramref name="throwIfNotFound" /> parameter specifies whether null is returned or an exception is thrown. In some cases, an exception is thrown regardless of the value of <paramref name="throwIfNotFound" />. See the Exceptions section.</returns>
	/// <param name="typeName">The assembly-qualified name of the <see cref="T:System.Type" /> to get. </param>
	/// <param name="throwIfNotFound">true to throw a <see cref="T:System.TypeLoadException" /> if the type cannot be found; false to return null if the type cannot be found. Specifying false also suppresses some other exception conditions, but not all of them. See the Exceptions section.</param>
	/// <param name="ignoreCase">true to perform a case-insensitive search for <paramref name="typeName" />; false to perform a case-sensitive search for <paramref name="typeName" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="typeName" /> is null. </exception>
	/// <exception cref="T:System.Reflection.TargetInvocationException">A class initializer is invoked and throws an exception. </exception>
	/// <exception cref="T:System.TypeLoadException">
	///   <paramref name="throwIfNotFound" /> is true and the type is not found. -or-<paramref name="throwIfNotFound" /> is true and <paramref name="typeName" /> contains invalid characters, such as an embedded tab.-or-<paramref name="throwIfNotFound" /> is true and <paramref name="typeName" /> is an empty string.-or-<paramref name="throwIfNotFound" /> is true and <paramref name="typeName" /> represents an array type with an invalid size. -or-<paramref name="typeName" /> represents an array of <see cref="T:System.TypedReference" /> objects. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="typeName" /> does not include the assembly name.-or-<paramref name="throwIfNotFound" /> is true and <paramref name="typeName" /> contains invalid syntax; for example, "MyType[,*,]".-or-<paramref name="typeName" /> represents a generic type that has a pointer type, a ByRef type, or <see cref="T:System.Void" /> as one of its type arguments.-or-<paramref name="typeName" /> represents a generic type that has an incorrect number of type arguments.-or-<paramref name="typeName" /> represents a generic type, and one of its type arguments does not satisfy the constraints for the corresponding type parameter.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="throwIfNotFound" /> is true and the assembly or one of its dependencies was not found. </exception>
	/// <exception cref="T:System.IO.FileLoadException">The assembly or one of its dependencies was found, but could not be loaded. </exception>
	/// <exception cref="T:System.BadImageFormatException">The assembly or one of its dependencies is not valid. -or-The assembly was compiled with a later version of the common language runtime than the version that is currently loaded.</exception>
	/// <filterpriority>1</filterpriority>
	public static Type ReflectionOnlyGetType(string typeName, bool throwIfNotFound, bool ignoreCase)
	{
		if (typeName == null)
		{
			throw new ArgumentNullException("typeName");
		}
		if (typeName == string.Empty && throwIfNotFound)
		{
			throw new TypeLoadException("A null or zero length string does not represent a valid Type");
		}
		int num = typeName.IndexOf(',');
		if (num < 0 || num == 0 || num == typeName.Length - 1)
		{
			throw new ArgumentException("Assembly qualifed type name is required", "typeName");
		}
		string assemblyString = typeName.Substring(num + 1);
		Assembly assembly;
		try
		{
			assembly = Assembly.ReflectionOnlyLoad(assemblyString);
		}
		catch
		{
			if (throwIfNotFound)
			{
				throw;
			}
			return null;
		}
		return assembly.GetType(typeName.Substring(0, num), throwIfNotFound, ignoreCase);
	}

	internal virtual Type InternalResolve()
	{
		return UnderlyingSystemType;
	}

	internal virtual Type RuntimeResolve()
	{
		throw new NotImplementedException();
	}

	internal virtual MethodInfo GetMethod(MethodInfo fromNoninstanciated)
	{
		throw new InvalidOperationException("can only be called in generic type");
	}

	internal virtual ConstructorInfo GetConstructor(ConstructorInfo fromNoninstanciated)
	{
		throw new InvalidOperationException("can only be called in generic type");
	}

	internal virtual FieldInfo GetField(FieldInfo fromNoninstanciated)
	{
		throw new InvalidOperationException("can only be called in generic type");
	}

	/// <summary>Gets the type referenced by the specified type handle.</summary>
	/// <returns>The type referenced by the specified <see cref="T:System.RuntimeTypeHandle" />, or null if the <see cref="P:System.RuntimeTypeHandle.Value" /> property of <paramref name="handle" /> is null.</returns>
	/// <param name="handle">The object that refers to the type. </param>
	/// <exception cref="T:System.Reflection.TargetInvocationException">A class initializer is invoked and throws an exception. </exception>
	/// <filterpriority>1</filterpriority>
	public static Type GetTypeFromHandle(RuntimeTypeHandle handle)
	{
		if (handle.Value == IntPtr.Zero)
		{
			return null;
		}
		return internal_from_handle(handle.Value);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern Type internal_from_handle(IntPtr handle);
}
