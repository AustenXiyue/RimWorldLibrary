using System.Reflection;

namespace System.Runtime.InteropServices;

/// <summary>Exposes the public members of the <see cref="T:System.Reflection.EventInfo" /> class to unmanaged code.</summary>
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("9DE59C64-D889-35A1-B897-587D74469E5B")]
[ComVisible(true)]
[CLSCompliant(false)]
[TypeLibImportClass(typeof(EventInfo))]
public interface _EventInfo
{
	/// <summary>Provides COM objects with version-independent access to the <see cref="P:System.Reflection.EventInfo.Attributes" /> property.</summary>
	/// <returns>The read-only attributes for this event.</returns>
	EventAttributes Attributes { get; }

	/// <summary>Provides COM objects with version-independent access to the <see cref="P:System.Reflection.MemberInfo.DeclaringType" /> property.</summary>
	/// <returns>The Type object for the class that declares this member.</returns>
	Type DeclaringType { get; }

	/// <summary>Provides COM objects with version-independent access to the <see cref="P:System.Reflection.EventInfo.EventHandlerType" /> property.</summary>
	/// <returns>A read-only <see cref="T:System.Type" /> object representing the delegate event handler.</returns>
	Type EventHandlerType { get; }

	/// <summary>Provides COM objects with version-independent access to the <see cref="P:System.Reflection.EventInfo.IsMulticast" /> property.</summary>
	/// <returns>true if the delegate is an instance of a multicast delegate; otherwise, false.</returns>
	bool IsMulticast { get; }

	/// <summary>Provides COM objects with version-independent access to the <see cref="P:System.Reflection.EventInfo.IsSpecialName" /> property.</summary>
	/// <returns>true if this event has a special name; otherwise, false.</returns>
	bool IsSpecialName { get; }

	/// <summary>Provides COM objects with version-independent access to the <see cref="P:System.Reflection.EventInfo.MemberType" /> property.</summary>
	/// <returns>A <see cref="T:System.Reflection.MemberTypes" /> value indicating that this member is an event.</returns>
	MemberTypes MemberType { get; }

	/// <summary>Provides COM objects with version-independent access to the <see cref="P:System.Reflection.MemberInfo.Name" /> property.</summary>
	/// <returns>A <see cref="T:System.String" /> object containing the name of this member.</returns>
	string Name { get; }

	/// <summary>Provides COM objects with version-independent access to the <see cref="P:System.Reflection.MemberInfo.ReflectedType" /> property.</summary>
	/// <returns>The <see cref="T:System.Type" /> object that was used to obtain this object.</returns>
	Type ReflectedType { get; }

	/// <summary>Provides COM objects with version-independent access to the <see cref="M:System.Reflection.EventInfo.AddEventHandler(System.Object,System.Delegate)" /> method.</summary>
	/// <param name="target">The event source. </param>
	/// <param name="handler">A method or methods to be invoked when the event is raised by the target. </param>
	void AddEventHandler(object target, Delegate handler);

	/// <summary>Provides COM objects with version-independent access to the <see cref="M:System.Object.Equals(System.Object)" /> method.</summary>
	/// <returns>true if the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />; otherwise, false.</returns>
	/// <param name="other">The <see cref="T:System.Object" /> to compare with the current <see cref="T:System.Object" />.</param>
	new bool Equals(object other);

	/// <summary>Provides COM objects with version-independent access to the <see cref="M:System.Reflection.EventInfo.GetAddMethod" /> method.</summary>
	/// <returns>A <see cref="T:System.Reflection.MethodInfo" /> object representing the method used to add an event-handler delegate to the event source.</returns>
	MethodInfo GetAddMethod();

	/// <summary>Provides COM objects with version-independent access to the <see cref="M:System.Reflection.EventInfo.GetAddMethod(System.Boolean)" /> method.</summary>
	/// <returns>A <see cref="T:System.Reflection.MethodInfo" /> object representing the method used to add an event-handler delegate to the event source.</returns>
	/// <param name="nonPublic">true to return non-public methods; otherwise, false.</param>
	MethodInfo GetAddMethod(bool nonPublic);

	/// <summary>Provides COM objects with version-independent access to the <see cref="M:System.Reflection.MemberInfo.GetCustomAttributes(System.Boolean)" /> method.</summary>
	/// <returns>An array that contains all the custom attributes, or an array with zero (0) elements if no attributes are defined.</returns>
	/// <param name="inherit">true to search a member's inheritance chain to find the attributes; otherwise, false.</param>
	object[] GetCustomAttributes(bool inherit);

	/// <summary>Provides COM objects with version-independent access to the <see cref="M:System.Reflection.MemberInfo.GetCustomAttributes(System.Type,System.Boolean)" /> method.</summary>
	/// <returns>An array of custom attributes applied to this member, or an array with zero (0) elements if no attributes have been applied.</returns>
	/// <param name="attributeType">The type of attribute to search for. Only attributes that are assignable to this type are returned. </param>
	/// <param name="inherit">true to search this member's inheritance chain to find the attributes; otherwise, false. </param>
	object[] GetCustomAttributes(Type attributeType, bool inherit);

	/// <summary>Provides COM objects with version-independent access to the <see cref="M:System.Object.GetHashCode" /> method.</summary>
	/// <returns>The hash code for the current instance.</returns>
	new int GetHashCode();

	/// <summary>Maps a set of names to a corresponding set of dispatch identifiers.</summary>
	/// <param name="riid">Reserved for future use. Must be IID_NULL.</param>
	/// <param name="rgszNames">An array of names to be mapped.</param>
	/// <param name="cNames">The count of the names to be mapped.</param>
	/// <param name="lcid">The locale context in which to interpret the names.</param>
	/// <param name="rgDispId">An array allocated by the caller that receives the identifiers corresponding to the names.</param>
	void GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId);

	/// <summary>Retrieves the type information for an object, which can be used to get the type information for an interface.</summary>
	/// <param name="iTInfo">The type information to return.</param>
	/// <param name="lcid">The locale identifier for the type information.</param>
	/// <param name="ppTInfo">A pointer to the requested type information object.</param>
	void GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo);

	/// <summary>Retrieves the number of type information interfaces that an object provides (either 0 or 1).</summary>
	/// <param name="pcTInfo">When this method returns, contains a pointer to a location that receives the number of type information interfaces provided by the object. This parameter is passed uninitialized.</param>
	void GetTypeInfoCount(out uint pcTInfo);

	/// <summary>Provides access to properties and methods exposed by an object.</summary>
	/// <param name="dispIdMember">An identifier for the member.</param>
	/// <param name="riid">Reserved for future use. Must be IID_NULL.</param>
	/// <param name="lcid">The locale context in which to interpret arguments.</param>
	/// <param name="wFlags">Flags describing the context of the call.</param>
	/// <param name="pDispParams">A pointer to a structure containing an array of arguments, an array of argument DISPIDs for named arguments, and counts for the number of elements in the arrays.</param>
	/// <param name="pVarResult">A pointer to the location where the result will be stored.</param>
	/// <param name="pExcepInfo">A pointer to a structure that contains exception information.</param>
	/// <param name="puArgErr">The index of the first argument that has an error.</param>
	void Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr);

	/// <summary>Provides COM objects with version-independent access to the <see cref="M:System.Reflection.EventInfo.GetRaiseMethod" /> method.</summary>
	/// <returns>The method that is called when the event is raised.</returns>
	MethodInfo GetRaiseMethod();

	/// <summary>Provides COM objects with version-independent access to the <see cref="M:System.Reflection.EventInfo.GetRaiseMethod(System.Boolean)" /> method.</summary>
	/// <returns>The <see cref="T:System.Reflection.MethodInfo" /> object that was called when the event was raised.</returns>
	/// <param name="nonPublic">true to return non-public methods; otherwise, false.</param>
	MethodInfo GetRaiseMethod(bool nonPublic);

	/// <summary>Provides COM objects with version-independent access to the <see cref="M:System.Reflection.EventInfo.GetRemoveMethod" /> method.</summary>
	/// <returns>A <see cref="T:System.Reflection.MethodInfo" /> object representing the method used to remove an event-handler delegate from the event source.</returns>
	MethodInfo GetRemoveMethod();

	/// <summary>Provides COM objects with version-independent access to the <see cref="M:System.Reflection.EventInfo.GetRemoveMethod(System.Boolean)" /> method.</summary>
	/// <returns>A <see cref="T:System.Reflection.MethodInfo" /> object representing the method used to remove an event-handler delegate from the event source.</returns>
	/// <param name="nonPublic">true to return non-public methods; otherwise, false.</param>
	MethodInfo GetRemoveMethod(bool nonPublic);

	/// <summary>Provides COM objects with version-independent access to the <see cref="M:System.Object.GetType" /> method.</summary>
	/// <returns>A <see cref="T:System.Type" /> object.</returns>
	new Type GetType();

	/// <summary>Provides COM objects with version-independent access to the <see cref="M:System.Reflection.MemberInfo.IsDefined(System.Type,System.Boolean)" /> method.</summary>
	/// <returns>true if one or more instance of the <paramref name="attributeType" /> parameter is applied to this member; otherwise, false.</returns>
	/// <param name="attributeType">The Type object to which the custom attributes are applied. </param>
	/// <param name="inherit">true to search this member's inheritance chain to find the attributes; otherwise, false. </param>
	bool IsDefined(Type attributeType, bool inherit);

	/// <summary>Provides COM objects with version-independent access to the <see cref="M:System.Reflection.EventInfo.RemoveEventHandler(System.Object,System.Delegate)" /> method.</summary>
	/// <param name="target">The event source. </param>
	/// <param name="handler">The delegate to be disassociated from the events raised by target. </param>
	void RemoveEventHandler(object target, Delegate handler);

	/// <summary>Provides COM objects with version-independent access to the <see cref="M:System.Object.ToString" /> method.</summary>
	/// <returns>A string that represents the current <see cref="T:System.Object" />.</returns>
	new string ToString();
}
