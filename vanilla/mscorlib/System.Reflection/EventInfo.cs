using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mono;

namespace System.Reflection;

/// <summary>Discovers the attributes of an event and provides access to event metadata.</summary>
[Serializable]
[StructLayout(LayoutKind.Sequential)]
[ComDefaultInterface(typeof(_EventInfo))]
[ClassInterface(ClassInterfaceType.None)]
[ComVisible(true)]
public abstract class EventInfo : MemberInfo, _EventInfo
{
	private delegate void AddEventAdapter(object _this, Delegate dele);

	private delegate void AddEvent<T, D>(T _this, D dele);

	private delegate void StaticAddEvent<D>(D dele);

	private AddEventAdapter cached_add_event;

	/// <summary>Gets the attributes for this event.</summary>
	/// <returns>The read-only attributes for this event.</returns>
	public abstract EventAttributes Attributes { get; }

	/// <summary>Gets the Type object of the underlying event-handler delegate associated with this event.</summary>
	/// <returns>A read-only Type object representing the delegate event handler.</returns>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	public virtual Type EventHandlerType
	{
		get
		{
			ParameterInfo[] parametersInternal = GetAddMethod(nonPublic: true).GetParametersInternal();
			if (parametersInternal.Length != 0)
			{
				return parametersInternal[0].ParameterType;
			}
			return null;
		}
	}

	/// <summary>Gets a value indicating whether the event is multicast.</summary>
	/// <returns>true if the delegate is an instance of a multicast delegate; otherwise, false.</returns>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	public virtual bool IsMulticast => true;

	/// <summary>Gets a value indicating whether the EventInfo has a name with a special meaning.</summary>
	/// <returns>true if this event has a special name; otherwise, false.</returns>
	public bool IsSpecialName => (Attributes & EventAttributes.SpecialName) != 0;

	/// <summary>Gets a <see cref="T:System.Reflection.MemberTypes" /> value indicating that this member is an event.</summary>
	/// <returns>A <see cref="T:System.Reflection.MemberTypes" /> value indicating that this member is an event.</returns>
	public override MemberTypes MemberType => MemberTypes.Event;

	/// <summary>Gets the <see cref="T:System.Reflection.MethodInfo" /> object for the <see cref="M:System.Reflection.EventInfo.AddEventHandler(System.Object,System.Delegate)" /> method of the event, including non-public methods.</summary>
	/// <returns>The <see cref="T:System.Reflection.MethodInfo" /> object for the <see cref="M:System.Reflection.EventInfo.AddEventHandler(System.Object,System.Delegate)" /> method.</returns>
	public virtual MethodInfo AddMethod => GetAddMethod(nonPublic: true);

	/// <summary>Gets the method that is called when the event is raised, including non-public methods.</summary>
	/// <returns>The method that is called when the event is raised.</returns>
	public virtual MethodInfo RaiseMethod => GetRaiseMethod(nonPublic: true);

	/// <summary>Gets the MethodInfo object for removing a method of the event, including non-public methods.</summary>
	/// <returns>The MethodInfo object for removing a method of the event.</returns>
	public virtual MethodInfo RemoveMethod => GetRemoveMethod(nonPublic: true);

	/// <summary>Initializes a new instance of the EventInfo class.</summary>
	protected EventInfo()
	{
	}

	/// <summary>Adds an event handler to an event source.</summary>
	/// <param name="target">The event source. </param>
	/// <param name="handler">Encapsulates a method or methods to be invoked when the event is raised by the target. </param>
	/// <exception cref="T:System.InvalidOperationException">The event does not have a public add accessor.</exception>
	/// <exception cref="T:System.ArgumentException">The handler that was passed in cannot be used. </exception>
	/// <exception cref="T:System.MethodAccessException">NoteIn the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.MemberAccessException" />, instead.The caller does not have access permission to the member. </exception>
	/// <exception cref="T:System.Reflection.TargetException">NoteIn the .NET for Windows Store apps or the Portable Class Library, catch <see cref="T:System.Exception" /> instead.The <paramref name="target" /> parameter is null and the event is not static.-or- The <see cref="T:System.Reflection.EventInfo" /> is not declared on the target. </exception>
	[DebuggerStepThrough]
	[DebuggerHidden]
	public virtual void AddEventHandler(object target, Delegate handler)
	{
		if (cached_add_event == null)
		{
			MethodInfo addMethod = GetAddMethod();
			if (addMethod == null)
			{
				throw new InvalidOperationException("Cannot add a handler to an event that doesn't have a visible add method");
			}
			if (addMethod.DeclaringType.IsValueType)
			{
				if (target == null && !addMethod.IsStatic)
				{
					throw new TargetException("Cannot add a handler to a non static event with a null target");
				}
				addMethod.Invoke(target, new object[1] { handler });
				return;
			}
			cached_add_event = CreateAddEventDelegate(addMethod);
		}
		cached_add_event(target, handler);
	}

	/// <summary>Returns the method used to add an event handler delegate to the event source.</summary>
	/// <returns>A <see cref="T:System.Reflection.MethodInfo" /> object representing the method used to add an event handler delegate to the event source.</returns>
	public MethodInfo GetAddMethod()
	{
		return GetAddMethod(nonPublic: false);
	}

	/// <summary>When overridden in a derived class, retrieves the MethodInfo object for the <see cref="M:System.Reflection.EventInfo.AddEventHandler(System.Object,System.Delegate)" /> method of the event, specifying whether to return non-public methods.</summary>
	/// <returns>A <see cref="T:System.Reflection.MethodInfo" /> object representing the method used to add an event handler delegate to the event source.</returns>
	/// <param name="nonPublic">true if non-public methods can be returned; otherwise, false. </param>
	/// <exception cref="T:System.MethodAccessException">
	///   <paramref name="nonPublic" /> is true, the method used to add an event handler delegate is non-public, and the caller does not have permission to reflect on non-public methods. </exception>
	public abstract MethodInfo GetAddMethod(bool nonPublic);

	/// <summary>Returns the method that is called when the event is raised.</summary>
	/// <returns>The method that is called when the event is raised.</returns>
	public MethodInfo GetRaiseMethod()
	{
		return GetRaiseMethod(nonPublic: false);
	}

	/// <summary>When overridden in a derived class, returns the method that is called when the event is raised, specifying whether to return non-public methods.</summary>
	/// <returns>A MethodInfo object that was called when the event was raised.</returns>
	/// <param name="nonPublic">true if non-public methods can be returned; otherwise, false. </param>
	/// <exception cref="T:System.MethodAccessException">
	///   <paramref name="nonPublic" /> is true, the method used to add an event handler delegate is non-public, and the caller does not have permission to reflect on non-public methods. </exception>
	public abstract MethodInfo GetRaiseMethod(bool nonPublic);

	/// <summary>Returns the method used to remove an event handler delegate from the event source.</summary>
	/// <returns>A <see cref="T:System.Reflection.MethodInfo" /> object representing the method used to remove an event handler delegate from the event source.</returns>
	public MethodInfo GetRemoveMethod()
	{
		return GetRemoveMethod(nonPublic: false);
	}

	/// <summary>When overridden in a derived class, retrieves the MethodInfo object for removing a method of the event, specifying whether to return non-public methods.</summary>
	/// <returns>A <see cref="T:System.Reflection.MethodInfo" /> object representing the method used to remove an event handler delegate from the event source.</returns>
	/// <param name="nonPublic">true if non-public methods can be returned; otherwise, false. </param>
	/// <exception cref="T:System.MethodAccessException">
	///   <paramref name="nonPublic" /> is true, the method used to add an event handler delegate is non-public, and the caller does not have permission to reflect on non-public methods. </exception>
	public abstract MethodInfo GetRemoveMethod(bool nonPublic);

	/// <summary>Returns the methods that have been associated with the event in metadata using the .other directive, specifying whether to include non-public methods.</summary>
	/// <returns>An array of <see cref="T:System.Reflection.EventInfo" /> objects representing methods that have been associated with an event in metadata by using the .other directive. If there are no methods matching the specification, an empty array is returned.</returns>
	/// <param name="nonPublic">true to include non-public methods; otherwise, false.</param>
	/// <exception cref="T:System.NotImplementedException">This method is not implemented.</exception>
	public virtual MethodInfo[] GetOtherMethods(bool nonPublic)
	{
		return EmptyArray<MethodInfo>.Value;
	}

	/// <summary>Returns the public methods that have been associated with an event in metadata using the .other directive.</summary>
	/// <returns>An array of <see cref="T:System.Reflection.EventInfo" /> objects representing the public methods that have been associated with the event in metadata by using the .other directive. If there are no such public methods, an empty array is returned.</returns>
	public MethodInfo[] GetOtherMethods()
	{
		return GetOtherMethods(nonPublic: false);
	}

	/// <summary>Removes an event handler from an event source.</summary>
	/// <param name="target">The event source. </param>
	/// <param name="handler">The delegate to be disassociated from the events raised by target. </param>
	/// <exception cref="T:System.InvalidOperationException">The event does not have a public remove accessor. </exception>
	/// <exception cref="T:System.ArgumentException">The handler that was passed in cannot be used. </exception>
	/// <exception cref="T:System.Reflection.TargetException">NoteIn the .NET for Windows Store apps or the Portable Class Library, catch <see cref="T:System.Exception" /> instead.The <paramref name="target" /> parameter is null and the event is not static.-or- The <see cref="T:System.Reflection.EventInfo" /> is not declared on the target. </exception>
	/// <exception cref="T:System.MethodAccessException">NoteIn the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.MemberAccessException" />, instead.The caller does not have access permission to the member. </exception>
	[DebuggerStepThrough]
	[DebuggerHidden]
	public virtual void RemoveEventHandler(object target, Delegate handler)
	{
		MethodInfo removeMethod = GetRemoveMethod();
		if (removeMethod == null)
		{
			throw new InvalidOperationException("Cannot remove a handler to an event that doesn't have a visible remove method");
		}
		removeMethod.Invoke(target, new object[1] { handler });
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

	/// <summary>Indicates whether two <see cref="T:System.Reflection.EventInfo" /> objects are equal.</summary>
	/// <returns>true if <paramref name="left" /> is equal to <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">The first object to compare.</param>
	/// <param name="right">The second object to compare.</param>
	public static bool operator ==(EventInfo left, EventInfo right)
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

	/// <summary>Indicates whether two <see cref="T:System.Reflection.EventInfo" /> objects are not equal.</summary>
	/// <returns>true if <paramref name="left" /> is not equal to <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">The first object to compare.</param>
	/// <param name="right">The second object to compare.</param>
	public static bool operator !=(EventInfo left, EventInfo right)
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
	void _EventInfo.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
	{
		throw new NotImplementedException();
	}

	/// <summary>Returns a T:System.Type object representing the <see cref="T:System.Reflection.EventInfo" /> type.</summary>
	/// <returns>A T:System.Type object representing the <see cref="T:System.Reflection.EventInfo" /> type.</returns>
	Type _EventInfo.GetType()
	{
		return GetType();
	}

	/// <summary>Retrieves the type information for an object, which can then be used to get the type information for an interface.</summary>
	/// <param name="iTInfo">The type information to return.</param>
	/// <param name="lcid">The locale identifier for the type information.</param>
	/// <param name="ppTInfo">Receives a pointer to the requested type information object.</param>
	/// <exception cref="T:System.NotImplementedException">Late-bound access using the COM IDispatch interface is not supported.</exception>
	void _EventInfo.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
	{
		throw new NotImplementedException();
	}

	/// <summary>Retrieves the number of type information interfaces that an object provides (either 0 or 1).</summary>
	/// <param name="pcTInfo">Points to a location that receives the number of type information interfaces provided by the object.</param>
	/// <exception cref="T:System.NotImplementedException">Late-bound access using the COM IDispatch interface is not supported.</exception>
	void _EventInfo.GetTypeInfoCount(out uint pcTInfo)
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
	void _EventInfo.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
	{
		throw new NotImplementedException();
	}

	private static void AddEventFrame<T, D>(AddEvent<T, D> addEvent, object obj, object dele)
	{
		if (obj == null)
		{
			throw new TargetException("Cannot add a handler to a non static event with a null target");
		}
		if (!(obj is T))
		{
			throw new TargetException("Object doesn't match target");
		}
		addEvent((T)obj, (D)dele);
	}

	private static void StaticAddEventAdapterFrame<D>(StaticAddEvent<D> addEvent, object obj, object dele)
	{
		addEvent((D)dele);
	}

	private static AddEventAdapter CreateAddEventDelegate(MethodInfo method)
	{
		Type[] typeArguments;
		Type typeFromHandle;
		string name;
		if (method.IsStatic)
		{
			typeArguments = new Type[1] { method.GetParametersInternal()[0].ParameterType };
			typeFromHandle = typeof(StaticAddEvent<>);
			name = "StaticAddEventAdapterFrame";
		}
		else
		{
			typeArguments = new Type[2]
			{
				method.DeclaringType,
				method.GetParametersInternal()[0].ParameterType
			};
			typeFromHandle = typeof(AddEvent<, >);
			name = "AddEventFrame";
		}
		object firstArgument = Delegate.CreateDelegate(typeFromHandle.MakeGenericType(typeArguments), method);
		MethodInfo method2 = typeof(EventInfo).GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic);
		method2 = method2.MakeGenericMethod(typeArguments);
		return (AddEventAdapter)Delegate.CreateDelegate(typeof(AddEventAdapter), firstArgument, method2, throwOnBindFailure: true);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern EventInfo internal_from_handle_type(IntPtr event_handle, IntPtr type_handle);

	internal static EventInfo GetEventFromHandle(RuntimeEventHandle handle)
	{
		if (handle.Value == IntPtr.Zero)
		{
			throw new ArgumentException("The handle is invalid.");
		}
		return internal_from_handle_type(handle.Value, IntPtr.Zero);
	}

	internal static EventInfo GetEventFromHandle(RuntimeEventHandle handle, RuntimeTypeHandle reflectedType)
	{
		if (handle.Value == IntPtr.Zero)
		{
			throw new ArgumentException("The handle is invalid.");
		}
		EventInfo eventInfo = internal_from_handle_type(handle.Value, reflectedType.Value);
		if (eventInfo == null)
		{
			throw new ArgumentException("The event handle and the type handle are incompatible.");
		}
		return eventInfo;
	}
}
