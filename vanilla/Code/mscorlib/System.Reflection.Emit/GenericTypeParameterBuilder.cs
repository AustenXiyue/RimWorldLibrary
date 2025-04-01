using System.Globalization;
using System.Runtime.InteropServices;
using Unity;

namespace System.Reflection.Emit;

/// <summary>Defines and creates generic type parameters for dynamically defined generic types and methods. This class cannot be inherited. </summary>
[StructLayout(LayoutKind.Sequential)]
[ComVisible(true)]
public sealed class GenericTypeParameterBuilder : TypeInfo
{
	private TypeBuilder tbuilder;

	private MethodBuilder mbuilder;

	private string name;

	private int index;

	private Type base_type;

	private Type[] iface_constraints;

	private CustomAttributeBuilder[] cattrs;

	private GenericParameterAttributes attrs;

	/// <summary>Gets the current generic type parameter.</summary>
	/// <returns>The current <see cref="T:System.Reflection.Emit.GenericTypeParameterBuilder" /> object.</returns>
	public override Type UnderlyingSystemType => this;

	/// <summary>Gets an <see cref="T:System.Reflection.Assembly" /> object representing the dynamic assembly that contains the generic type definition the current type parameter belongs to.</summary>
	/// <returns>An <see cref="T:System.Reflection.Assembly" /> object representing the dynamic assembly that contains the generic type definition the current type parameter belongs to.</returns>
	public override Assembly Assembly => tbuilder.Assembly;

	/// <summary>Gets null in all cases.</summary>
	/// <returns>A null reference (Nothing in Visual Basic) in all cases.</returns>
	public override string AssemblyQualifiedName => null;

	/// <summary>Gets the base type constraint of the current generic type parameter.</summary>
	/// <returns>A <see cref="T:System.Type" /> object that represents the base type constraint of the generic type parameter, or null if the type parameter has no base type constraint.</returns>
	public override Type BaseType => base_type;

	/// <summary>Gets null in all cases.</summary>
	/// <returns>A null reference (Nothing in Visual Basic) in all cases.</returns>
	public override string FullName => null;

	/// <summary>Not supported for incomplete generic type parameters.</summary>
	/// <returns>Not supported for incomplete generic type parameters.</returns>
	/// <exception cref="T:System.NotSupportedException">In all cases. </exception>
	public override Guid GUID
	{
		get
		{
			throw not_supported();
		}
	}

	/// <summary>Gets the name of the generic type parameter.</summary>
	/// <returns>The name of the generic type parameter.</returns>
	public override string Name => name;

	/// <summary>Gets null in all cases.</summary>
	/// <returns>A null reference (Nothing in Visual Basic) in all cases.</returns>
	public override string Namespace => null;

	/// <summary>Gets the dynamic module that contains the generic type parameter.</summary>
	/// <returns>A <see cref="T:System.Reflection.Module" /> object that represents the dynamic module that contains the generic type parameter.</returns>
	public override Module Module => tbuilder.Module;

	/// <summary>Gets the generic type definition or generic method definition to which the generic type parameter belongs.</summary>
	/// <returns>If the type parameter belongs to a generic type, a <see cref="T:System.Type" /> object representing that generic type; if the type parameter belongs to a generic method, a <see cref="T:System.Type" /> object representing that type that declared that generic method.</returns>
	public override Type DeclaringType
	{
		get
		{
			if (!(mbuilder != null))
			{
				return tbuilder;
			}
			return mbuilder.DeclaringType;
		}
	}

	/// <summary>Gets the <see cref="T:System.Type" /> object that was used to obtain the <see cref="T:System.Reflection.Emit.GenericTypeParameterBuilder" />.</summary>
	/// <returns>The <see cref="T:System.Type" /> object that was used to obtain the <see cref="T:System.Reflection.Emit.GenericTypeParameterBuilder" />.</returns>
	public override Type ReflectedType => DeclaringType;

	/// <summary>Not supported for incomplete generic type parameters.</summary>
	/// <returns>Not supported for incomplete generic type parameters.</returns>
	/// <exception cref="T:System.NotSupportedException">In all cases.</exception>
	public override RuntimeTypeHandle TypeHandle
	{
		get
		{
			throw not_supported();
		}
	}

	/// <summary>Gets true in all cases.</summary>
	/// <returns>true in all cases.</returns>
	public override bool ContainsGenericParameters => true;

	/// <summary>Gets true in all cases.</summary>
	/// <returns>true in all cases.</returns>
	public override bool IsGenericParameter => true;

	/// <summary>Returns false in all cases.</summary>
	/// <returns>false in all cases.</returns>
	public override bool IsGenericType => false;

	/// <summary>Gets false in all cases.</summary>
	/// <returns>false in all cases.</returns>
	public override bool IsGenericTypeDefinition => false;

	public override GenericParameterAttributes GenericParameterAttributes
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	/// <summary>Gets the position of the type parameter in the type parameter list of the generic type or method that declared the parameter.</summary>
	/// <returns>The position of the type parameter in the type parameter list of the generic type or method that declared the parameter.</returns>
	public override int GenericParameterPosition => index;

	/// <summary>Gets a <see cref="T:System.Reflection.MethodInfo" /> that represents the declaring method, if the current <see cref="T:System.Reflection.Emit.GenericTypeParameterBuilder" /> represents a type parameter of a generic method.</summary>
	/// <returns>A <see cref="T:System.Reflection.MethodInfo" /> that represents the declaring method, if the current <see cref="T:System.Reflection.Emit.GenericTypeParameterBuilder" /> represents a type parameter of a generic method; otherwise, null.</returns>
	public override MethodBase DeclaringMethod => mbuilder;

	internal override bool IsUserType => false;

	/// <summary>Sets the base type that a type must inherit in order to be substituted for the type parameter.</summary>
	/// <param name="baseTypeConstraint">The <see cref="T:System.Type" /> that must be inherited by any type that is to be substituted for the type parameter.</param>
	public void SetBaseTypeConstraint(Type baseTypeConstraint)
	{
		base_type = baseTypeConstraint ?? typeof(object);
	}

	/// <summary>Sets the interfaces a type must implement in order to be substituted for the type parameter. </summary>
	/// <param name="interfaceConstraints">An array of <see cref="T:System.Type" /> objects that represent the interfaces a type must implement in order to be substituted for the type parameter.</param>
	[ComVisible(true)]
	public void SetInterfaceConstraints(params Type[] interfaceConstraints)
	{
		iface_constraints = interfaceConstraints;
	}

	/// <summary>Sets the variance characteristics and special constraints of the generic parameter, such as the parameterless constructor constraint.</summary>
	/// <param name="genericParameterAttributes">A bitwise combination of <see cref="T:System.Reflection.GenericParameterAttributes" /> values that represent the variance characteristics and special constraints of the generic type parameter.</param>
	public void SetGenericParameterAttributes(GenericParameterAttributes genericParameterAttributes)
	{
		attrs = genericParameterAttributes;
	}

	internal GenericTypeParameterBuilder(TypeBuilder tbuilder, MethodBuilder mbuilder, string name, int index)
	{
		this.tbuilder = tbuilder;
		this.mbuilder = mbuilder;
		this.name = name;
		this.index = index;
	}

	internal override Type InternalResolve()
	{
		if (mbuilder != null)
		{
			return MethodBase.GetMethodFromHandle(mbuilder.MethodHandleInternal, mbuilder.TypeBuilder.InternalResolve().TypeHandle).GetGenericArguments()[index];
		}
		return tbuilder.InternalResolve().GetGenericArguments()[index];
	}

	internal override Type RuntimeResolve()
	{
		if (mbuilder != null)
		{
			return MethodBase.GetMethodFromHandle(mbuilder.MethodHandleInternal, mbuilder.TypeBuilder.RuntimeResolve().TypeHandle).GetGenericArguments()[index];
		}
		return tbuilder.RuntimeResolve().GetGenericArguments()[index];
	}

	/// <summary>Not supported for incomplete generic type parameters.</summary>
	/// <returns>Not supported for incomplete generic type parameters.</returns>
	/// <param name="c">Not supported.</param>
	/// <exception cref="T:System.NotSupportedException">In all cases.</exception>
	[ComVisible(true)]
	public override bool IsSubclassOf(Type c)
	{
		throw not_supported();
	}

	protected override TypeAttributes GetAttributeFlagsImpl()
	{
		return TypeAttributes.Public;
	}

	protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
	{
		throw not_supported();
	}

	/// <summary>Not supported for incomplete generic type parameters.</summary>
	/// <returns>Not supported for incomplete generic type parameters.</returns>
	/// <param name="bindingAttr">Not supported.</param>
	/// <exception cref="T:System.NotSupportedException">In all cases. </exception>
	[ComVisible(true)]
	public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
	{
		throw not_supported();
	}

	/// <summary>Not supported for incomplete generic type parameters.</summary>
	/// <returns>Not supported for incomplete generic type parameters.</returns>
	/// <param name="name">Not supported.</param>
	/// <param name="bindingAttr">Not supported. </param>
	/// <exception cref="T:System.NotSupportedException">In all cases.</exception>
	public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
	{
		throw not_supported();
	}

	/// <summary>Not supported for incomplete generic type parameters.</summary>
	/// <returns>Not supported for incomplete generic type parameters.</returns>
	/// <exception cref="T:System.NotSupportedException">In all cases.</exception>
	public override EventInfo[] GetEvents()
	{
		throw not_supported();
	}

	/// <summary>Not supported for incomplete generic type parameters.</summary>
	/// <returns>Not supported for incomplete generic type parameters.</returns>
	/// <param name="bindingAttr">Not supported.</param>
	/// <exception cref="T:System.NotSupportedException">In all cases.</exception>
	public override EventInfo[] GetEvents(BindingFlags bindingAttr)
	{
		throw not_supported();
	}

	/// <summary>Not supported for incomplete generic type parameters.</summary>
	/// <returns>Not supported for incomplete generic type parameters.</returns>
	/// <param name="name">Not supported.</param>
	/// <param name="bindingAttr">Not supported.</param>
	/// <exception cref="T:System.NotSupportedException">In all cases.</exception>
	public override FieldInfo GetField(string name, BindingFlags bindingAttr)
	{
		throw not_supported();
	}

	/// <summary>Not supported for incomplete generic type parameters.</summary>
	/// <returns>Not supported for incomplete generic type parameters.</returns>
	/// <param name="bindingAttr">Not supported.</param>
	/// <exception cref="T:System.NotSupportedException">In all cases.</exception>
	public override FieldInfo[] GetFields(BindingFlags bindingAttr)
	{
		throw not_supported();
	}

	/// <summary>Not supported for incomplete generic type parameters.</summary>
	/// <returns>Not supported for incomplete generic type parameters.</returns>
	/// <param name="name">The name of the interface.</param>
	/// <param name="ignoreCase">true to search without regard for case; false to make a case-sensitive search.</param>
	/// <exception cref="T:System.NotSupportedException">In all cases.</exception>
	public override Type GetInterface(string name, bool ignoreCase)
	{
		throw not_supported();
	}

	/// <summary>Not supported for incomplete generic type parameters.</summary>
	/// <returns>Not supported for incomplete generic type parameters.</returns>
	/// <exception cref="T:System.NotSupportedException">In all cases.</exception>
	public override Type[] GetInterfaces()
	{
		throw not_supported();
	}

	/// <summary>Not supported for incomplete generic type parameters.</summary>
	/// <returns>Not supported for incomplete generic type parameters.</returns>
	/// <param name="bindingAttr">Not supported.</param>
	/// <exception cref="T:System.NotSupportedException">In all cases.</exception>
	public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
	{
		throw not_supported();
	}

	/// <summary>Not supported for incomplete generic type parameters.</summary>
	/// <returns>Not supported for incomplete generic type parameters.</returns>
	/// <param name="name">Not supported.</param>
	/// <param name="type">Not supported.</param>
	/// <param name="bindingAttr">Not supported.</param>
	/// <exception cref="T:System.NotSupportedException">In all cases.</exception>
	public override MemberInfo[] GetMember(string name, MemberTypes type, BindingFlags bindingAttr)
	{
		throw not_supported();
	}

	/// <summary>Not supported for incomplete generic type parameters.</summary>
	/// <returns>Not supported for incomplete generic type parameters.</returns>
	/// <param name="bindingAttr">Not supported.</param>
	/// <exception cref="T:System.NotSupportedException">In all cases.</exception>
	public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
	{
		throw not_supported();
	}

	protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
	{
		throw not_supported();
	}

	/// <summary>Not supported for incomplete generic type parameters.</summary>
	/// <returns>Not supported for incomplete generic type parameters.</returns>
	/// <param name="name">Not supported.</param>
	/// <param name="bindingAttr">Not supported.</param>
	/// <exception cref="T:System.NotSupportedException">In all cases.</exception>
	public override Type GetNestedType(string name, BindingFlags bindingAttr)
	{
		throw not_supported();
	}

	/// <summary>Not supported for incomplete generic type parameters.</summary>
	/// <returns>Not supported for incomplete generic type parameters.</returns>
	/// <param name="bindingAttr">Not supported.</param>
	/// <exception cref="T:System.NotSupportedException">In all cases.</exception>
	public override Type[] GetNestedTypes(BindingFlags bindingAttr)
	{
		throw not_supported();
	}

	/// <summary>Not supported for incomplete generic type parameters.</summary>
	/// <returns>Not supported for incomplete generic type parameters.</returns>
	/// <param name="bindingAttr">Not supported.</param>
	/// <exception cref="T:System.NotSupportedException">In all cases.</exception>
	public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
	{
		throw not_supported();
	}

	protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
	{
		throw not_supported();
	}

	protected override bool HasElementTypeImpl()
	{
		return false;
	}

	/// <summary>Throws a <see cref="T:System.NotSupportedException" /> exception in all cases.</summary>
	/// <returns>Throws a <see cref="T:System.NotSupportedException" /> exception in all cases.</returns>
	/// <param name="c">The object to test.</param>
	/// <exception cref="T:System.NotSupportedException">In all cases.</exception>
	public override bool IsAssignableFrom(Type c)
	{
		throw not_supported();
	}

	/// <summary>Throws a <see cref="T:System.NotSupportedException" /> exception in all cases.</summary>
	/// <returns>Throws a <see cref="T:System.NotSupportedException" /> exception in all cases.</returns>
	/// <param name="typeInfo">The object to test.</param>
	/// <exception cref="T:System.NotSupportedException">In all cases.</exception>
	public override bool IsAssignableFrom(TypeInfo typeInfo)
	{
		if (typeInfo == null)
		{
			return false;
		}
		return IsAssignableFrom(typeInfo.AsType());
	}

	public override bool IsInstanceOfType(object o)
	{
		throw not_supported();
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
		return false;
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
		if (!(base_type != null))
		{
			return false;
		}
		return base_type.IsValueType;
	}

	/// <summary>Not supported for incomplete generic type parameters.</summary>
	/// <returns>Not supported for incomplete generic type parameters.</returns>
	/// <param name="name">Not supported. </param>
	/// <param name="invokeAttr">Not supported.</param>
	/// <param name="binder">Not supported.</param>
	/// <param name="target">Not supported.</param>
	/// <param name="args">Not supported.</param>
	/// <param name="modifiers">Not supported.</param>
	/// <param name="culture">Not supported.</param>
	/// <param name="namedParameters">Not supported.</param>
	/// <exception cref="T:System.NotSupportedException">In all cases.</exception>
	public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
	{
		throw not_supported();
	}

	/// <summary>Throws a <see cref="T:System.NotSupportedException" /> in all cases. </summary>
	/// <returns>The type referred to by the current array type, pointer type, or ByRef type; or null if the current type is not an array type, is not a pointer type, and is not passed by reference.</returns>
	/// <exception cref="T:System.NotSupportedException">In all cases.</exception>
	public override Type GetElementType()
	{
		throw not_supported();
	}

	/// <summary>Not supported for incomplete generic type parameters.</summary>
	/// <returns>Not supported for incomplete generic type parameters.</returns>
	/// <param name="attributeType">Not supported.</param>
	/// <param name="inherit">Not supported.</param>
	/// <exception cref="T:System.NotSupportedException">In all cases.</exception>
	public override bool IsDefined(Type attributeType, bool inherit)
	{
		throw not_supported();
	}

	/// <summary>Not supported for incomplete generic type parameters.</summary>
	/// <returns>Not supported for incomplete generic type parameters.</returns>
	/// <param name="inherit">Specifies whether to search this member's inheritance chain to find the attributes.</param>
	/// <exception cref="T:System.NotSupportedException">In all cases.</exception>
	public override object[] GetCustomAttributes(bool inherit)
	{
		throw not_supported();
	}

	/// <summary>Not supported for incomplete generic type parameters.</summary>
	/// <returns>Not supported for incomplete generic type parameters.</returns>
	/// <param name="attributeType">The type of attribute to search for. Only attributes that are assignable to this type are returned.</param>
	/// <param name="inherit">Specifies whether to search this member's inheritance chain to find the attributes.</param>
	/// <exception cref="T:System.NotSupportedException">In all cases.</exception>
	public override object[] GetCustomAttributes(Type attributeType, bool inherit)
	{
		throw not_supported();
	}

	/// <summary>Not supported for incomplete generic type parameters.</summary>
	/// <returns>Not supported for incomplete generic type parameters.</returns>
	/// <param name="interfaceType">A <see cref="T:System.Type" /> object that represents the interface type for which the mapping is to be retrieved.</param>
	/// <exception cref="T:System.NotSupportedException">In all cases.</exception>
	[ComVisible(true)]
	public override InterfaceMapping GetInterfaceMap(Type interfaceType)
	{
		throw not_supported();
	}

	/// <summary>Not valid for generic type parameters.</summary>
	/// <returns>Not valid for generic type parameters.</returns>
	/// <exception cref="T:System.InvalidOperationException">In all cases.</exception>
	public override Type[] GetGenericArguments()
	{
		throw new InvalidOperationException();
	}

	/// <summary>Not valid for generic type parameters.</summary>
	/// <returns>Not valid for generic type parameters.</returns>
	/// <exception cref="T:System.InvalidOperationException">In all cases.</exception>
	public override Type GetGenericTypeDefinition()
	{
		throw new InvalidOperationException();
	}

	public override Type[] GetGenericParameterConstraints()
	{
		throw new InvalidOperationException();
	}

	/// <summary>Set a custom attribute using a custom attribute builder.</summary>
	/// <param name="customBuilder">An instance of a helper class that defines the custom attribute.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="customBuilder" /> is null.</exception>
	public void SetCustomAttribute(CustomAttributeBuilder customBuilder)
	{
		if (customBuilder == null)
		{
			throw new ArgumentNullException("customBuilder");
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
	/// <param name="con">The constructor for the custom attribute.</param>
	/// <param name="binaryAttribute">A byte blob representing the attribute.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="con" /> is null.-or-<paramref name="binaryAttribute" /> is a null reference.</exception>
	[MonoTODO("unverified implementation")]
	public void SetCustomAttribute(ConstructorInfo con, byte[] binaryAttribute)
	{
		SetCustomAttribute(new CustomAttributeBuilder(con, binaryAttribute));
	}

	private Exception not_supported()
	{
		return new NotSupportedException();
	}

	/// <summary>Returns a string representation of the current generic type parameter.</summary>
	/// <returns>A string that contains the name of the generic type parameter.</returns>
	public override string ToString()
	{
		return name;
	}

	/// <summary>Tests whether the given object is an instance of EventToken and is equal to the current instance.</summary>
	/// <returns>Returns true if <paramref name="o" /> is an instance of EventToken and equals the current instance; otherwise, false.</returns>
	/// <param name="o">The object to be compared with the current instance.</param>
	[MonoTODO]
	public override bool Equals(object o)
	{
		return base.Equals(o);
	}

	/// <summary>Returns a 32-bit integer hash code for the current instance.</summary>
	/// <returns>A 32-bit integer hash code.</returns>
	[MonoTODO]
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	/// <summary>Returns the type of a one-dimensional array whose element type is the generic type parameter.</summary>
	/// <returns>A <see cref="T:System.Type" /> object that represents the type of a one-dimensional array whose element type is the generic type parameter.</returns>
	public override Type MakeArrayType()
	{
		return new ArrayType(this, 0);
	}

	/// <summary>Returns the type of an array whose element type is the generic type parameter, with the specified number of dimensions.</summary>
	/// <returns>A <see cref="T:System.Type" /> object that represents the type of an array whose element type is the generic type parameter, with the specified number of dimensions.</returns>
	/// <param name="rank">The number of dimensions for the array.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">
	///   <paramref name="rank" /> is not a valid number of dimensions. For example, its value is less than 1.</exception>
	public override Type MakeArrayType(int rank)
	{
		if (rank < 1)
		{
			throw new IndexOutOfRangeException();
		}
		return new ArrayType(this, rank);
	}

	/// <summary>Returns a <see cref="T:System.Type" /> object that represents the current generic type parameter when passed as a reference parameter.</summary>
	/// <returns>A <see cref="T:System.Type" /> object that represents the current generic type parameter when passed as a reference parameter.</returns>
	public override Type MakeByRefType()
	{
		return new ByRefType(this);
	}

	/// <summary>Not valid for incomplete generic type parameters.</summary>
	/// <returns>This method is invalid for incomplete generic type parameters.</returns>
	/// <param name="typeArguments">An array of type arguments.</param>
	/// <exception cref="T:System.InvalidOperationException">In all cases.</exception>
	public override Type MakeGenericType(params Type[] typeArguments)
	{
		throw new InvalidOperationException(Environment.GetResourceString("{0} is not a GenericTypeDefinition. MakeGenericType may only be called on a type for which Type.IsGenericTypeDefinition is true."));
	}

	/// <summary>Returns a <see cref="T:System.Type" /> object that represents a pointer to the current generic type parameter.</summary>
	/// <returns>A <see cref="T:System.Type" /> object that represents a pointer to the current generic type parameter.</returns>
	public override Type MakePointerType()
	{
		return new PointerType(this);
	}

	internal GenericTypeParameterBuilder()
	{
		ThrowStub.ThrowNotSupportedException();
	}
}
