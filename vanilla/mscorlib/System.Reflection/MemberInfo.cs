using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Reflection;

/// <summary>Obtains information about the attributes of a member and provides access to member metadata.</summary>
[Serializable]
[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
[ComDefaultInterface(typeof(_MemberInfo))]
[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
public abstract class MemberInfo : ICustomAttributeProvider, _MemberInfo
{
	/// <summary>When overridden in a derived class, gets a <see cref="T:System.Reflection.MemberTypes" /> value indicating the type of the member — method, constructor, event, and so on.</summary>
	/// <returns>A <see cref="T:System.Reflection.MemberTypes" /> value indicating the type of member.</returns>
	public abstract MemberTypes MemberType { get; }

	/// <summary>Gets the name of the current member.</summary>
	/// <returns>A <see cref="T:System.String" /> containing the name of this member.</returns>
	public abstract string Name { get; }

	/// <summary>Gets the class that declares this member.</summary>
	/// <returns>The Type object for the class that declares this member.</returns>
	public abstract Type DeclaringType { get; }

	/// <summary>Gets the class object that was used to obtain this instance of MemberInfo.</summary>
	/// <returns>The Type object through which this MemberInfo object was obtained.</returns>
	public abstract Type ReflectedType { get; }

	/// <summary>Gets a collection that contains this member's custom attributes.</summary>
	/// <returns>A collection that contains this member's custom attributes.</returns>
	public virtual IEnumerable<CustomAttributeData> CustomAttributes => GetCustomAttributesData();

	/// <summary>Gets a value that identifies a metadata element.</summary>
	/// <returns>A value which, in combination with <see cref="P:System.Reflection.MemberInfo.Module" />, uniquely identifies a metadata element.</returns>
	/// <exception cref="T:System.InvalidOperationException">The current <see cref="T:System.Reflection.MemberInfo" /> represents an array method, such as Address, on an array type whose element type is a dynamic type that has not been completed. To get a metadata token in this case, pass the <see cref="T:System.Reflection.MemberInfo" /> object to the <see cref="M:System.Reflection.Emit.ModuleBuilder.GetMethodToken(System.Reflection.MethodInfo)" /> method; or use the <see cref="M:System.Reflection.Emit.ModuleBuilder.GetArrayMethodToken(System.Type,System.String,System.Reflection.CallingConventions,System.Type,System.Type[])" />  method to get the token directly, instead of using the <see cref="M:System.Reflection.Emit.ModuleBuilder.GetArrayMethod(System.Type,System.String,System.Reflection.CallingConventions,System.Type,System.Type[])" /> method to get a <see cref="T:System.Reflection.MethodInfo" /> first.</exception>
	public virtual extern int MetadataToken
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	/// <summary>Gets the module in which the type that declares the member represented by the current <see cref="T:System.Reflection.MemberInfo" /> is defined.</summary>
	/// <returns>The <see cref="T:System.Reflection.Module" /> in which the type that declares the member represented by the current <see cref="T:System.Reflection.MemberInfo" /> is defined.</returns>
	/// <exception cref="T:System.NotImplementedException">This method is not implemented.</exception>
	public virtual Module Module
	{
		get
		{
			if (this is Type)
			{
				return ((Type)this).Module;
			}
			throw new NotImplementedException();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Reflection.MemberInfo" /> class.</summary>
	protected MemberInfo()
	{
	}

	internal virtual bool CacheEquals(object o)
	{
		throw new NotImplementedException();
	}

	/// <summary>When overridden in a derived class, returns an array of all custom attributes applied to this member. </summary>
	/// <returns>An array that contains all the custom attributes applied to this member, or an array with zero elements if no attributes are defined.</returns>
	/// <param name="inherit">true to search this member's inheritance chain to find the attributes; otherwise, false. This parameter is ignored for properties and events; see Remarks.</param>
	/// <exception cref="T:System.InvalidOperationException">This member belongs to a type that is loaded into the reflection-only context. See How to: Load Assemblies into the Reflection-Only Context.</exception>
	/// <exception cref="T:System.TypeLoadException">A custom attribute type could not be loaded. </exception>
	public abstract object[] GetCustomAttributes(bool inherit);

	/// <summary>When overridden in a derived class, returns an array of custom attributes applied to this member and identified by <see cref="T:System.Type" />.</summary>
	/// <returns>An array of custom attributes applied to this member, or an array with zero elements if no attributes assignable to <paramref name="attributeType" /> have been applied.</returns>
	/// <param name="attributeType">The type of attribute to search for. Only attributes that are assignable to this type are returned. </param>
	/// <param name="inherit">true to search this member's inheritance chain to find the attributes; otherwise, false. This parameter is ignored for properties and events; see Remarks. </param>
	/// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded. </exception>
	/// <exception cref="T:System.ArgumentNullException">If <paramref name="attributeType" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">This member belongs to a type that is loaded into the reflection-only context. See How to: Load Assemblies into the Reflection-Only Context.</exception>
	public abstract object[] GetCustomAttributes(Type attributeType, bool inherit);

	/// <summary>When overridden in a derived class, indicates whether one or more attributes of the specified type or of its derived types is applied to this member.</summary>
	/// <returns>true if one or more instances of <paramref name="attributeType" /> or any of its derived types is applied to this member; otherwise, false.</returns>
	/// <param name="attributeType">The type of custom attribute to search for. The search includes derived types. </param>
	/// <param name="inherit">true to search this member's inheritance chain to find the attributes; otherwise, false. This parameter is ignored for properties and events; see Remarks.</param>
	public abstract bool IsDefined(Type attributeType, bool inherit);

	/// <summary>Returns a list of <see cref="T:System.Reflection.CustomAttributeData" /> objects representing data about the attributes that have been applied to the target member.</summary>
	/// <returns>A generic list of <see cref="T:System.Reflection.CustomAttributeData" /> objects representing data about the attributes that have been applied to the target member.</returns>
	public virtual IList<CustomAttributeData> GetCustomAttributesData()
	{
		throw new NotImplementedException();
	}

	/// <summary>Indicates whether two <see cref="T:System.Reflection.MemberInfo" /> objects are equal.</summary>
	/// <returns>true if <paramref name="left" /> is equal to <paramref name="right" />; otherwise false.</returns>
	/// <param name="left">The <see cref="T:System.Reflection.MemberInfo" /> to compare to <paramref name="right" />.</param>
	/// <param name="right">The <see cref="T:System.Reflection.MemberInfo" /> to compare to <paramref name="left" />.</param>
	public static bool operator ==(MemberInfo left, MemberInfo right)
	{
		if ((object)left == right)
		{
			return true;
		}
		if ((object)left == null || (object)right == null)
		{
			return false;
		}
		Type type;
		Type type2;
		if ((type = left as Type) != null && (type2 = right as Type) != null)
		{
			return type == type2;
		}
		MethodBase methodBase;
		MethodBase methodBase2;
		if ((methodBase = left as MethodBase) != null && (methodBase2 = right as MethodBase) != null)
		{
			return methodBase == methodBase2;
		}
		FieldInfo fieldInfo;
		FieldInfo fieldInfo2;
		if ((fieldInfo = left as FieldInfo) != null && (fieldInfo2 = right as FieldInfo) != null)
		{
			return fieldInfo == fieldInfo2;
		}
		EventInfo eventInfo;
		EventInfo eventInfo2;
		if ((eventInfo = left as EventInfo) != null && (eventInfo2 = right as EventInfo) != null)
		{
			return eventInfo == eventInfo2;
		}
		PropertyInfo propertyInfo;
		PropertyInfo propertyInfo2;
		if ((propertyInfo = left as PropertyInfo) != null && (propertyInfo2 = right as PropertyInfo) != null)
		{
			return propertyInfo == propertyInfo2;
		}
		return false;
	}

	/// <summary>Indicates whether two <see cref="T:System.Reflection.MemberInfo" /> objects are not equal.</summary>
	/// <returns>true if <paramref name="left" /> is not equal to <paramref name="right" />; otherwise false.</returns>
	/// <param name="left">The <see cref="T:System.Reflection.MemberInfo" /> to compare to <paramref name="right" />.</param>
	/// <param name="right">The <see cref="T:System.Reflection.MemberInfo" /> to compare to <paramref name="left" />.</param>
	public static bool operator !=(MemberInfo left, MemberInfo right)
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

	/// <summary>Gets a <see cref="T:System.Type" /> object representing the <see cref="T:System.Reflection.MemberInfo" /> class.</summary>
	/// <returns>A <see cref="T:System.Type" /> object representing the <see cref="T:System.Reflection.MemberInfo" /> class.</returns>
	Type _MemberInfo.GetType()
	{
		return GetType();
	}

	/// <summary>Retrieves the number of type information interfaces that an object provides (either 0 or 1).</summary>
	/// <param name="pcTInfo">Points to a location that receives the number of type information interfaces provided by the object.</param>
	/// <exception cref="T:System.NotImplementedException">The method is called late-bound using the COM IDispatch interface.</exception>
	void _MemberInfo.GetTypeInfoCount(out uint pcTInfo)
	{
		throw new NotImplementedException();
	}

	/// <summary>Retrieves the type information for an object, which can then be used to get the type information for an interface.</summary>
	/// <param name="iTInfo">The type information to return.</param>
	/// <param name="lcid">The locale identifier for the type information.</param>
	/// <param name="ppTInfo">Receives a pointer to the requested type information object.</param>
	/// <exception cref="T:System.NotImplementedException">The method is called late-bound using the COM IDispatch interface.</exception>
	void _MemberInfo.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
	{
		throw new NotImplementedException();
	}

	/// <summary>Maps a set of names to a corresponding set of dispatch identifiers.</summary>
	/// <param name="riid">Reserved for future use. Must be IID_NULL.</param>
	/// <param name="rgszNames">Passed-in array of names to be mapped.</param>
	/// <param name="cNames">Count of the names to be mapped.</param>
	/// <param name="lcid">The locale context in which to interpret the names.</param>
	/// <param name="rgDispId">Caller-allocated array which receives the IDs corresponding to the names.</param>
	/// <exception cref="T:System.NotImplementedException">The method is called late-bound using the COM IDispatch interface.</exception>
	void _MemberInfo.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
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
	/// <exception cref="T:System.NotImplementedException">The method is called late-bound using the COM IDispatch interface.</exception>
	void _MemberInfo.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
	{
		throw new NotImplementedException();
	}
}
