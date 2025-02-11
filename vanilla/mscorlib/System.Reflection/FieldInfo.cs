using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Reflection;

/// <summary>Discovers the attributes of a field and provides access to field metadata. </summary>
[Serializable]
[ClassInterface(ClassInterfaceType.None)]
[ComDefaultInterface(typeof(_FieldInfo))]
[ComVisible(true)]
public abstract class FieldInfo : MemberInfo, _FieldInfo
{
	/// <summary>Gets the attributes associated with this field.</summary>
	/// <returns>The FieldAttributes for this field.</returns>
	public abstract FieldAttributes Attributes { get; }

	/// <summary>Gets a RuntimeFieldHandle, which is a handle to the internal metadata representation of a field.</summary>
	/// <returns>A handle to the internal metadata representation of a field.</returns>
	public abstract RuntimeFieldHandle FieldHandle { get; }

	/// <summary>Gets the type of this field object.</summary>
	/// <returns>The type of this field object.</returns>
	public abstract Type FieldType { get; }

	/// <summary>Gets a <see cref="T:System.Reflection.MemberTypes" /> value indicating that this member is a field.</summary>
	/// <returns>A <see cref="T:System.Reflection.MemberTypes" /> value indicating that this member is a field.</returns>
	public override MemberTypes MemberType => MemberTypes.Field;

	/// <summary>Gets a value indicating whether the value is written at compile time and cannot be changed.</summary>
	/// <returns>true if the field has the Literal attribute set; otherwise, false.</returns>
	public bool IsLiteral => (Attributes & FieldAttributes.Literal) != 0;

	/// <summary>Gets a value indicating whether the field is static.</summary>
	/// <returns>true if this field is static; otherwise, false.</returns>
	public bool IsStatic => (Attributes & FieldAttributes.Static) != 0;

	/// <summary>Gets a value indicating whether the field can only be set in the body of the constructor.</summary>
	/// <returns>true if the field has the InitOnly attribute set; otherwise, false.</returns>
	public bool IsInitOnly => (Attributes & FieldAttributes.InitOnly) != 0;

	/// <summary>Gets a value indicating whether the field is public.</summary>
	/// <returns>true if this field is public; otherwise, false.</returns>
	public bool IsPublic => (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Public;

	/// <summary>Gets a value indicating whether the field is private.</summary>
	/// <returns>true if the field is private; otherwise; false.</returns>
	public bool IsPrivate => (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Private;

	/// <summary>Gets a value indicating whether the visibility of this field is described by <see cref="F:System.Reflection.FieldAttributes.Family" />; that is, the field is visible only within its class and derived classes.</summary>
	/// <returns>true if access to this field is exactly described by <see cref="F:System.Reflection.FieldAttributes.Family" />; otherwise, false.</returns>
	public bool IsFamily => (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Family;

	/// <summary>Gets a value indicating whether the potential visibility of this field is described by <see cref="F:System.Reflection.FieldAttributes.Assembly" />; that is, the field is visible at most to other types in the same assembly, and is not visible to derived types outside the assembly.</summary>
	/// <returns>true if the visibility of this field is exactly described by <see cref="F:System.Reflection.FieldAttributes.Assembly" />; otherwise, false.</returns>
	public bool IsAssembly => (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Assembly;

	/// <summary>Gets a value indicating whether the visibility of this field is described by <see cref="F:System.Reflection.FieldAttributes.FamANDAssem" />; that is, the field can be accessed from derived classes, but only if they are in the same assembly.</summary>
	/// <returns>true if access to this field is exactly described by <see cref="F:System.Reflection.FieldAttributes.FamANDAssem" />; otherwise, false.</returns>
	public bool IsFamilyAndAssembly => (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.FamANDAssem;

	/// <summary>Gets a value indicating whether the potential visibility of this field is described by <see cref="F:System.Reflection.FieldAttributes.FamORAssem" />; that is, the field can be accessed by derived classes wherever they are, and by classes in the same assembly.</summary>
	/// <returns>true if access to this field is exactly described by <see cref="F:System.Reflection.FieldAttributes.FamORAssem" />; otherwise, false.</returns>
	public bool IsFamilyOrAssembly => (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.FamORAssem;

	/// <summary>Gets a value indicating whether the corresponding PinvokeImpl attribute is set in <see cref="T:System.Reflection.FieldAttributes" />.</summary>
	/// <returns>true if the PinvokeImpl attribute is set in <see cref="T:System.Reflection.FieldAttributes" />; otherwise, false.</returns>
	public bool IsPinvokeImpl => (Attributes & FieldAttributes.PinvokeImpl) == FieldAttributes.PinvokeImpl;

	/// <summary>Gets a value indicating whether the corresponding SpecialName attribute is set in the <see cref="T:System.Reflection.FieldAttributes" /> enumerator.</summary>
	/// <returns>true if the SpecialName attribute is set in <see cref="T:System.Reflection.FieldAttributes" />; otherwise, false.</returns>
	public bool IsSpecialName => (Attributes & FieldAttributes.SpecialName) == FieldAttributes.SpecialName;

	/// <summary>Gets a value indicating whether this field has the NotSerialized attribute.</summary>
	/// <returns>true if the field has the NotSerialized attribute set; otherwise, false.</returns>
	public bool IsNotSerialized => (Attributes & FieldAttributes.NotSerialized) == FieldAttributes.NotSerialized;

	/// <summary>Gets a value that indicates whether the current field is security-critical or security-safe-critical at the current trust level. </summary>
	/// <returns>true if the current field is security-critical or security-safe-critical at the current trust level; false if it is transparent. </returns>
	public virtual bool IsSecurityCritical
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	/// <summary>Gets a value that indicates whether the current field is security-safe-critical at the current trust level. </summary>
	/// <returns>true if the current field is security-safe-critical at the current trust level; false if it is security-critical or transparent.</returns>
	public virtual bool IsSecuritySafeCritical
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	/// <summary>Gets a value that indicates whether the current field is transparent at the current trust level.</summary>
	/// <returns>true if the field is security-transparent at the current trust level; otherwise, false.</returns>
	public virtual bool IsSecurityTransparent
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	/// <summary>Initializes a new instance of the FieldInfo class.</summary>
	protected FieldInfo()
	{
	}

	/// <summary>When overridden in a derived class, returns the value of a field supported by a given object.</summary>
	/// <returns>An object containing the value of the field reflected by this instance.</returns>
	/// <param name="obj">The object whose field value will be returned. </param>
	/// <exception cref="T:System.Reflection.TargetException">NoteIn the .NET for Windows Store apps or the Portable Class Library, catch <see cref="T:System.Exception" /> instead.The field is non-static and <paramref name="obj" /> is null. </exception>
	/// <exception cref="T:System.NotSupportedException">A field is marked literal, but the field does not have one of the accepted literal types. </exception>
	/// <exception cref="T:System.FieldAccessException">NoteIn the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.MemberAccessException" />, instead.The caller does not have permission to access this field. </exception>
	/// <exception cref="T:System.ArgumentException">The method is neither declared nor inherited by the class of <paramref name="obj" />. </exception>
	public abstract object GetValue(object obj);

	/// <summary>When overridden in a derived class, sets the value of the field supported by the given object.</summary>
	/// <param name="obj">The object whose field value will be set. </param>
	/// <param name="value">The value to assign to the field. </param>
	/// <param name="invokeAttr">A field of Binder that specifies the type of binding that is desired (for example, Binder.CreateInstance or Binder.ExactBinding). </param>
	/// <param name="binder">A set of properties that enables the binding, coercion of argument types, and invocation of members through reflection. If <paramref name="binder" /> is null, then Binder.DefaultBinding is used. </param>
	/// <param name="culture">The software preferences of a particular culture. </param>
	/// <exception cref="T:System.FieldAccessException">The caller does not have permission to access this field. </exception>
	/// <exception cref="T:System.Reflection.TargetException">The <paramref name="obj" /> parameter is null and the field is an instance field. </exception>
	/// <exception cref="T:System.ArgumentException">The field does not exist on the object.-or- The <paramref name="value" /> parameter cannot be converted and stored in the field. </exception>
	public abstract void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture);

	/// <summary>Sets the value of the field supported by the given object.</summary>
	/// <param name="obj">The object whose field value will be set. </param>
	/// <param name="value">The value to assign to the field. </param>
	/// <exception cref="T:System.FieldAccessException">NoteIn the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.MemberAccessException" />, instead.The caller does not have permission to access this field. </exception>
	/// <exception cref="T:System.Reflection.TargetException">NoteIn the .NET for Windows Store apps or the Portable Class Library, catch <see cref="T:System.Exception" /> instead.The <paramref name="obj" /> parameter is null and the field is an instance field. </exception>
	/// <exception cref="T:System.ArgumentException">The field does not exist on the object.-or- The <paramref name="value" /> parameter cannot be converted and stored in the field. </exception>
	[DebuggerHidden]
	[DebuggerStepThrough]
	public void SetValue(object obj, object value)
	{
		SetValue(obj, value, BindingFlags.Default, null, null);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern FieldInfo internal_from_handle_type(IntPtr field_handle, IntPtr type_handle);

	/// <summary>Gets a <see cref="T:System.Reflection.FieldInfo" /> for the field represented by the specified handle.</summary>
	/// <returns>A <see cref="T:System.Reflection.FieldInfo" /> object representing the field specified by <paramref name="handle" />.</returns>
	/// <param name="handle">A <see cref="T:System.RuntimeFieldHandle" /> structure that contains the handle to the internal metadata representation of a field. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="handle" /> is invalid.</exception>
	public static FieldInfo GetFieldFromHandle(RuntimeFieldHandle handle)
	{
		if (handle.Value == IntPtr.Zero)
		{
			throw new ArgumentException("The handle is invalid.");
		}
		return internal_from_handle_type(handle.Value, IntPtr.Zero);
	}

	/// <summary>Gets a <see cref="T:System.Reflection.FieldInfo" /> for the field represented by the specified handle, for the specified generic type.</summary>
	/// <returns>A <see cref="T:System.Reflection.FieldInfo" /> object representing the field specified by <paramref name="handle" />, in the generic type specified by <paramref name="declaringType" />.</returns>
	/// <param name="handle">A <see cref="T:System.RuntimeFieldHandle" /> structure that contains the handle to the internal metadata representation of a field.</param>
	/// <param name="declaringType">A <see cref="T:System.RuntimeTypeHandle" /> structure that contains the handle to the generic type that defines the field.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="handle" /> is invalid.-or-<paramref name="declaringType" /> is not compatible with <paramref name="handle" />. For example, <paramref name="declaringType" /> is the runtime type handle of the generic type definition, and <paramref name="handle" /> comes from a constructed type. See Remarks.</exception>
	[ComVisible(false)]
	public static FieldInfo GetFieldFromHandle(RuntimeFieldHandle handle, RuntimeTypeHandle declaringType)
	{
		if (handle.Value == IntPtr.Zero)
		{
			throw new ArgumentException("The handle is invalid.");
		}
		FieldInfo fieldInfo = internal_from_handle_type(handle.Value, declaringType.Value);
		if (fieldInfo == null)
		{
			throw new ArgumentException("The field handle and the type handle are incompatible.");
		}
		return fieldInfo;
	}

	internal virtual int GetFieldOffset()
	{
		throw new SystemException("This method should not be called");
	}

	/// <summary>Returns the value of a field supported by a given object.</summary>
	/// <returns>An Object containing a field value.</returns>
	/// <param name="obj">A <see cref="T:System.TypedReference" /> structure that encapsulates a managed pointer to a location and a runtime representation of the type that might be stored at that location. </param>
	/// <exception cref="T:System.NotSupportedException">The caller requires the Common Language Specification (CLS) alternative, but called this method instead. </exception>
	[CLSCompliant(false)]
	public virtual object GetValueDirect(TypedReference obj)
	{
		throw new NotSupportedException(Environment.GetResourceString("This non-CLS method is not implemented."));
	}

	/// <summary>Sets the value of the field supported by the given object.</summary>
	/// <param name="obj">A <see cref="T:System.TypedReference" /> structure that encapsulates a managed pointer to a location and a runtime representation of the type that can be stored at that location. </param>
	/// <param name="value">The value to assign to the field. </param>
	/// <exception cref="T:System.NotSupportedException">The caller requires the Common Language Specification (CLS) alternative, but called this method instead. </exception>
	[CLSCompliant(false)]
	public virtual void SetValueDirect(TypedReference obj, object value)
	{
		throw new NotSupportedException(Environment.GetResourceString("This non-CLS method is not implemented."));
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern MarshalAsAttribute get_marshal_info();

	internal object[] GetPseudoCustomAttributes()
	{
		int num = 0;
		if (IsNotSerialized)
		{
			num++;
		}
		if (DeclaringType.IsExplicitLayout)
		{
			num++;
		}
		MarshalAsAttribute marshal_info = get_marshal_info();
		if (marshal_info != null)
		{
			num++;
		}
		if (num == 0)
		{
			return null;
		}
		object[] array = new object[num];
		num = 0;
		if (IsNotSerialized)
		{
			array[num++] = new NonSerializedAttribute();
		}
		if (DeclaringType.IsExplicitLayout)
		{
			array[num++] = new FieldOffsetAttribute(GetFieldOffset());
		}
		if (marshal_info != null)
		{
			array[num++] = marshal_info;
		}
		return array;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern Type[] GetTypeModifiers(bool optional);

	/// <summary>Gets an array of types that identify the optional custom modifiers of the field.</summary>
	/// <returns>An array of <see cref="T:System.Type" /> objects that identify the optional custom modifiers of the current field, such as <see cref="T:System.Runtime.CompilerServices.IsConst" />.</returns>
	public virtual Type[] GetOptionalCustomModifiers()
	{
		Type[] typeModifiers = GetTypeModifiers(optional: true);
		if (typeModifiers == null)
		{
			return Type.EmptyTypes;
		}
		return typeModifiers;
	}

	/// <summary>Gets an array of types that identify the required custom modifiers of the property.</summary>
	/// <returns>An array of <see cref="T:System.Type" /> objects that identify the required custom modifiers of the current property, such as <see cref="T:System.Runtime.CompilerServices.IsConst" /> or <see cref="T:System.Runtime.CompilerServices.IsImplicitlyDereferenced" />.</returns>
	public virtual Type[] GetRequiredCustomModifiers()
	{
		Type[] typeModifiers = GetTypeModifiers(optional: false);
		if (typeModifiers == null)
		{
			return Type.EmptyTypes;
		}
		return typeModifiers;
	}

	/// <summary>Returns a literal value associated with the field by a compiler. </summary>
	/// <returns>An <see cref="T:System.Object" /> that contains the literal value associated with the field. If the literal value is a class type with an element value of zero, the return value is null.</returns>
	/// <exception cref="T:System.InvalidOperationException">The Constant table in unmanaged metadata does not contain a constant value for the current field.</exception>
	/// <exception cref="T:System.FormatException">The type of the value is not one of the types permitted by the Common Language Specification (CLS). See the ECMA Partition II specification Metadata Logical Format: Other Structures, Element Types used in Signatures. </exception>
	/// <exception cref="T:System.NotSupportedException">The constant value for the field is not set. </exception>
	public virtual object GetRawConstantValue()
	{
		throw new NotSupportedException("This non-CLS method is not implemented.");
	}

	/// <summary>Returns a value that indicates whether this instance is equal to a specified object.</summary>
	/// <returns>true if <paramref name="obj" /> equals the type and value of this instance; otherwise, false.</returns>
	/// <param name="obj">An object to compare with this instance, or null.</param>
	public override bool Equals(object obj)
	{
		return obj == this;
	}

	/// <summary>Returns the hash code for this instance.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	/// <summary>Indicates whether two <see cref="T:System.Reflection.FieldInfo" /> objects are equal.</summary>
	/// <returns>true if <paramref name="left" /> is equal to <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">The first object to compare.</param>
	/// <param name="right">The second object to compare.</param>
	public static bool operator ==(FieldInfo left, FieldInfo right)
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

	/// <summary>Indicates whether two <see cref="T:System.Reflection.FieldInfo" /> objects are not equal.</summary>
	/// <returns>true if <paramref name="left" /> is not equal to <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">The first object to compare.</param>
	/// <param name="right">The second object to compare.</param>
	public static bool operator !=(FieldInfo left, FieldInfo right)
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

	/// <summary>Maps a set of names to a corresponding set of dispatch identifiers.</summary>
	/// <param name="riid">Reserved for future use. Must be IID_NULL.</param>
	/// <param name="rgszNames">Passed-in array of names to be mapped.</param>
	/// <param name="cNames">Count of the names to be mapped.</param>
	/// <param name="lcid">The locale context in which to interpret the names.</param>
	/// <param name="rgDispId">Caller-allocated array which receives the IDs corresponding to the names.</param>
	/// <exception cref="T:System.NotImplementedException">Late-bound access using the COM IDispatch interface is not supported.</exception>
	void _FieldInfo.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
	{
		throw new NotImplementedException();
	}

	/// <summary>Gets a <see cref="T:System.Type" /> object representing the <see cref="T:System.Reflection.FieldInfo" /> type.</summary>
	/// <returns>A <see cref="T:System.Type" /> object representing the <see cref="T:System.Reflection.FieldInfo" /> type.</returns>
	Type _FieldInfo.GetType()
	{
		return GetType();
	}

	/// <summary>Retrieves the type information for an object, which can then be used to get the type information for an interface.</summary>
	/// <param name="iTInfo">The type information to return.</param>
	/// <param name="lcid">The locale identifier for the type information.</param>
	/// <param name="ppTInfo">Receives a pointer to the requested type information object.</param>
	/// <exception cref="T:System.NotImplementedException">Late-bound access using the COM IDispatch interface is not supported.</exception>
	void _FieldInfo.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
	{
		throw new NotImplementedException();
	}

	/// <summary>Retrieves the number of type information interfaces that an object provides (either 0 or 1).</summary>
	/// <param name="pcTInfo">Points to a location that receives the number of type information interfaces provided by the object.</param>
	/// <exception cref="T:System.NotImplementedException">Late-bound access using the COM IDispatch interface is not supported.</exception>
	void _FieldInfo.GetTypeInfoCount(out uint pcTInfo)
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
	void _FieldInfo.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
	{
		throw new NotImplementedException();
	}
}
