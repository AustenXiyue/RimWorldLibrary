using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Reflection;

/// <summary>Discovers the attributes of a method and provides access to method metadata.</summary>
[Serializable]
[ComDefaultInterface(typeof(_MethodInfo))]
[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
public abstract class MethodInfo : MethodBase, _MethodInfo
{
	/// <summary>Gets a <see cref="T:System.Reflection.MemberTypes" /> value indicating that this member is a method.</summary>
	/// <returns>A <see cref="T:System.Reflection.MemberTypes" /> value indicating that this member is a method.</returns>
	public override MemberTypes MemberType => MemberTypes.Method;

	/// <summary>Gets the return type of this method.</summary>
	/// <returns>The return type of this method.</returns>
	public virtual Type ReturnType
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets a <see cref="T:System.Reflection.ParameterInfo" /> object that contains information about the return type of the method, such as whether the return type has custom modifiers. </summary>
	/// <returns>A <see cref="T:System.Reflection.ParameterInfo" /> object that contains information about the return type.</returns>
	/// <exception cref="T:System.NotImplementedException">This method is not implemented.</exception>
	public virtual ParameterInfo ReturnParameter
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets the custom attributes for the return type.</summary>
	/// <returns>An ICustomAttributeProvider object representing the custom attributes for the return type.</returns>
	public abstract ICustomAttributeProvider ReturnTypeCustomAttributes { get; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Reflection.MethodInfo" /> class.</summary>
	protected MethodInfo()
	{
	}

	/// <summary>Indicates whether two <see cref="T:System.Reflection.MethodInfo" /> objects are equal.</summary>
	/// <returns>true if <paramref name="left" /> is equal to <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">The first object to compare.</param>
	/// <param name="right">The second object to compare.</param>
	public static bool operator ==(MethodInfo left, MethodInfo right)
	{
		if ((object)left == right)
		{
			return true;
		}
		if ((object)left == null || (object)right == null || left is RuntimeMethodInfo || right is RuntimeMethodInfo)
		{
			return false;
		}
		return left.Equals(right);
	}

	/// <summary>Indicates whether two <see cref="T:System.Reflection.MethodInfo" /> objects are not equal.</summary>
	/// <returns>true if <paramref name="left" /> is not equal to <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">The first object to compare.</param>
	/// <param name="right">The second object to compare.</param>
	public static bool operator !=(MethodInfo left, MethodInfo right)
	{
		return !(left == right);
	}

	/// <summary>Returns a value that indicates whether this instance is equal to a specified object.</summary>
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

	/// <summary>When overridden in a derived class, returns the MethodInfo object for the method on the direct or indirect base class in which the method represented by this instance was first declared.</summary>
	/// <returns>A MethodInfo object for the first implementation of this method.</returns>
	public abstract MethodInfo GetBaseDefinition();

	/// <summary>Returns an array of <see cref="T:System.Type" /> objects that represent the type arguments of a generic method or the type parameters of a generic method definition.</summary>
	/// <returns>An array of <see cref="T:System.Type" /> objects that represent the type arguments of a generic method or the type parameters of a generic method definition. Returns an empty array if the current method is not a generic method.</returns>
	/// <exception cref="T:System.NotSupportedException">This method is not supported.</exception>
	[ComVisible(true)]
	public override Type[] GetGenericArguments()
	{
		throw new NotSupportedException(Environment.GetResourceString("Derived classes must provide an implementation."));
	}

	/// <summary>Returns a <see cref="T:System.Reflection.MethodInfo" /> object that represents a generic method definition from which the current method can be constructed.</summary>
	/// <returns>A <see cref="T:System.Reflection.MethodInfo" /> object representing a generic method definition from which the current method can be constructed.</returns>
	/// <exception cref="T:System.InvalidOperationException">The current method is not a generic method. That is, <see cref="P:System.Reflection.MethodInfo.IsGenericMethod" /> returns false. </exception>
	/// <exception cref="T:System.NotSupportedException">This method is not supported.</exception>
	[ComVisible(true)]
	public virtual MethodInfo GetGenericMethodDefinition()
	{
		throw new NotSupportedException(Environment.GetResourceString("Derived classes must provide an implementation."));
	}

	/// <summary>Substitutes the elements of an array of types for the type parameters of the current generic method definition, and returns a <see cref="T:System.Reflection.MethodInfo" /> object representing the resulting constructed method.</summary>
	/// <returns>A <see cref="T:System.Reflection.MethodInfo" /> object that represents the constructed method formed by substituting the elements of <paramref name="typeArguments" /> for the type parameters of the current generic method definition.</returns>
	/// <param name="typeArguments">An array of types to be substituted for the type parameters of the current generic method definition.</param>
	/// <exception cref="T:System.InvalidOperationException">The current <see cref="T:System.Reflection.MethodInfo" /> does not represent a generic method definition. That is, <see cref="P:System.Reflection.MethodInfo.IsGenericMethodDefinition" /> returns false.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="typeArguments" /> is null.-or- Any element of <paramref name="typeArguments" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">The number of elements in <paramref name="typeArguments" /> is not the same as the number of type parameters of the current generic method definition.-or- An element of <paramref name="typeArguments" /> does not satisfy the constraints specified for the corresponding type parameter of the current generic method definition. </exception>
	/// <exception cref="T:System.NotSupportedException">This method is not supported.</exception>
	public virtual MethodInfo MakeGenericMethod(params Type[] typeArguments)
	{
		throw new NotSupportedException(Environment.GetResourceString("Derived classes must provide an implementation."));
	}

	/// <summary>Creates a delegate of the specified type from this method.</summary>
	/// <returns>The delegate for this method.</returns>
	/// <param name="delegateType">The type of the delegate to create.</param>
	public virtual Delegate CreateDelegate(Type delegateType)
	{
		throw new NotSupportedException(Environment.GetResourceString("Derived classes must provide an implementation."));
	}

	/// <summary>Creates a delegate of the specified type with the specified target from this method.</summary>
	/// <returns>The delegate for this method.</returns>
	/// <param name="delegateType">The type of the delegate to create.</param>
	/// <param name="target">The object targeted by the delegate.</param>
	public virtual Delegate CreateDelegate(Type delegateType, object target)
	{
		throw new NotSupportedException(Environment.GetResourceString("Derived classes must provide an implementation."));
	}

	/// <summary>Provides access to the <see cref="M:System.Object.GetType" /> method from COM.</summary>
	/// <returns>A <see cref="T:System.Type" /> object representing the <see cref="T:System.Reflection.MethodInfo" /> type.</returns>
	Type _MethodInfo.GetType()
	{
		return GetType();
	}

	/// <summary>Retrieves the number of type information interfaces that an object provides (either 0 or 1).</summary>
	/// <param name="pcTInfo">Points to a location that receives the number of type information interfaces provided by the object.</param>
	/// <exception cref="T:System.NotImplementedException">Late-bound access using the COM IDispatch interface is not supported.</exception>
	void _MethodInfo.GetTypeInfoCount(out uint pcTInfo)
	{
		throw new NotImplementedException();
	}

	/// <summary>Retrieves the type information for an object, which can be used to get the type information for an interface.</summary>
	/// <param name="iTInfo">The type information to return.</param>
	/// <param name="lcid">The locale identifier for the type information.</param>
	/// <param name="ppTInfo">Receives a pointer to the requested type information object.</param>
	/// <exception cref="T:System.NotImplementedException">Late-bound access using the COM IDispatch interface is not supported.</exception>
	void _MethodInfo.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
	{
		throw new NotImplementedException();
	}

	/// <summary>Maps a set of names to a corresponding set of dispatch identifiers.</summary>
	/// <param name="riid">Reserved for future use. Must be IID_NULL.</param>
	/// <param name="rgszNames">Passed-in array of names to be mapped.</param>
	/// <param name="cNames">Count of the names to be mapped.</param>
	/// <param name="lcid">The locale context in which to interpret the names.</param>
	/// <param name="rgDispId">Caller-allocated array that receives the IDs corresponding to the names.</param>
	/// <exception cref="T:System.NotImplementedException">Late-bound access using the COM IDispatch interface is not supported.</exception>
	void _MethodInfo.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
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
	void _MethodInfo.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
	{
		throw new NotImplementedException();
	}

	internal virtual MethodInfo GetBaseMethod()
	{
		return this;
	}
}
