using System.Security;
using System.Security.Permissions;
using Unity;

namespace System.Diagnostics.Eventing;

/// <summary>Use this class to write events.</summary>
/// <filterpriority>2</filterpriority>
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public class EventProvider : IDisposable
{
	/// <summary>Defines the possible states of the last write operation.</summary>
	/// <filterpriority>2</filterpriority>
	public enum WriteEventErrorCode
	{
		/// <summary>The event is larger than the session buffer size; events cannot span buffers.</summary>
		EventTooBig = 2,
		/// <summary>The write was successful.</summary>
		NoError = 0,
		/// <summary>The session ran out of free buffers to write to. This can occur during high event rates because the disk subsystem is overloaded or the number of buffers is too small. Rather than blocking until more buffers become available, the event is dropped. Consider increasing the number and size of the buffers for the session, or reducing the number of events written or the size of the events.</summary>
		NoFreeBuffers = 1
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Eventing.EventProvider" /> class.</summary>
	/// <param name="providerGuid">Guid that uniquely identifies the provider.</param>
	/// <exception cref="T:System.InsufficientMemoryException">There is not enough memory to complete the operation.</exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The classes in the <see cref="N:System.Diagnostics.Eventing" /> namespace work only on Windows Vista.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="providerGuid" /> parameter cannot be null.</exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">An error returned by the ETW subsystem. </exception>
	/// <filterpriority>2</filterpriority>
	[SecuritySafeCritical]
	[PermissionSet(SecurityAction.Demand, Unrestricted = true)]
	public EventProvider(Guid providerGuid)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Removes the provider's registration from the ETW subsystem and releases all unmanaged resources.</summary>
	/// <exception cref="T:System.ComponentModel.Win32Exception">An error returned by the ETW subsystem. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual void Close()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Creates a unique activity identifier for the provider.</summary>
	/// <returns>A unique Guid that you use when calling the <see cref="M:System.Diagnostics.Eventing.EventProvider.SetActivityId(System.Guid@)" /> method to set the activity identifier for the provider.</returns>
	/// <filterpriority>2</filterpriority>
	[SecurityCritical]
	public static Guid CreateActivityId()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return default(Guid);
	}

	/// <filterpriority>2</filterpriority>
	public void Dispose()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	[SecuritySafeCritical]
	protected virtual void Dispose(bool disposing)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Gets the last error associated with an event write failure.</summary>
	/// <returns>Use the value to determine the cause of an event write failure.</returns>
	/// <filterpriority>2</filterpriority>
	public static WriteEventErrorCode GetLastWriteEventError()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return default(WriteEventErrorCode);
	}

	/// <summary>Determines whether any session enabled the provider, regardless of the level and keyword values used to enable the provider.</summary>
	/// <returns>Is true if the provider is enabled to any session; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsEnabled()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return default(bool);
	}

	/// <summary>Determines whether any session is requesting the specified event from the provider.</summary>
	/// <returns>Is true if any session is requesting the specified event; otherwise, false.</returns>
	/// <param name="level">Level of detail included in the event.</param>
	/// <param name="keywords">Bit mask that specifies the event category. This mask should be the same keyword mask that is defined in the manifest for the event.</param>
	/// <filterpriority>2</filterpriority>
	public bool IsEnabled(byte level, long keywords)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return default(bool);
	}

	/// <summary>Sets the current activity identifier used by the <see cref="Overload:System.Diagnostics.Eventing.EventProvider.WriteEvent" /> methods.</summary>
	/// <param name="id">A unique activity identifier that the <see cref="M:System.Diagnostics.Eventing.EventProvider.CreateActivityId" /> method returns.</param>
	/// <filterpriority>2</filterpriority>
	[SecurityCritical]
	public static void SetActivityId(ref Guid id)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Writes an event. The event data is specified as a block of memory.</summary>
	/// <returns>Is true if the event is written; otherwise, false. If false, call the <see cref="M:System.Diagnostics.Eventing.EventProvider.GetLastWriteEventError" /> method to determine the cause of the failure.</returns>
	/// <param name="eventDescriptor">An instance of <see cref="T:System.Diagnostics.Eventing.EventDescriptor" /> that identifies the event to write.</param>
	/// <param name="dataCount">Size of the event data to which the <paramref name="data" /> parameter points. The maximum event data size is limited to 64 KB minus the size of the event headers. The event size is less if the session's buffer size is less and the session includes extended data items with the event.</param>
	/// <param name="data">Pointer to the event data to write.</param>
	[SecurityCritical]
	protected bool WriteEvent(ref EventDescriptor eventDescriptor, int dataCount, IntPtr data)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return default(bool);
	}

	/// <summary>Writes an event. The event data is specified as an array of objects.</summary>
	/// <returns>Is true if the event is written; otherwise, false. If false, call the <see cref="M:System.Diagnostics.Eventing.EventProvider.GetLastWriteEventError" /> method to determine the cause of the failure.</returns>
	/// <param name="eventDescriptor">An instance of <see cref="T:System.Diagnostics.Eventing.EventDescriptor" /> that identifies the event to write.</param>
	/// <param name="eventPayload">An array of objects that contain the event data to write. The object must be in the order specified in the manifest. The array is limited to 32 objects, of which only eight may be strings. The maximum data size for the event is limited to 64 KB minus the size of the event headers. The event size is less if the session's buffer size is less and the session includes extended data items with the event.This parameter can be null.</param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="eventPayload" /> parameter contains too many objects or strings.</exception>
	/// <filterpriority>2</filterpriority>
	public bool WriteEvent(ref EventDescriptor eventDescriptor, object[] eventPayload)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return default(bool);
	}

	/// <summary>Writes an event. The event data is specified as a string.</summary>
	/// <returns>Is true if the event is written; otherwise, false. If false, call the <see cref="M:System.Diagnostics.Eventing.EventProvider.GetLastWriteEventError" /> method to determine the cause of the failure.</returns>
	/// <param name="eventDescriptor">An instance of <see cref="T:System.Diagnostics.Eventing.EventDescriptor" /> that identifies the event to write.</param>
	/// <param name="data">The string to write as the event data.</param>
	/// <exception cref="T:System.ArgumentException">If <paramref name="data" /> is null.</exception>
	/// <filterpriority>2</filterpriority>
	[SecurityCritical]
	public bool WriteEvent(ref EventDescriptor eventDescriptor, string data)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return default(bool);
	}

	/// <summary>Writes an event that contains a string as its data.</summary>
	/// <returns>Is true if the event is written; otherwise, false. If false, call the <see cref="M:System.Diagnostics.Eventing.EventProvider.GetLastWriteEventError" /> method to determine the cause of the failure.</returns>
	/// <param name="eventMessage">String to write as the event data.</param>
	/// <exception cref="T:System.ArgumentException">If <paramref name="eventMessage" /> is null.</exception>
	/// <filterpriority>2</filterpriority>
	public bool WriteMessageEvent(string eventMessage)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return default(bool);
	}

	/// <summary>Writes an event that contains a string as its data if the level and keyword value match the events requested by the session.</summary>
	/// <returns>Is true if the event is written; otherwise, false. If false, call the <see cref="M:System.Diagnostics.Eventing.EventProvider.GetLastWriteEventError" /> method to determine the cause of the failure.</returns>
	/// <param name="eventMessage">String to write as the event data.</param>
	/// <param name="eventLevel">Level of detail included in the event. If the provider uses a manifest to define the event, set this value to the same level defined in the manifest.</param>
	/// <param name="eventKeywords">Bit mask that specifies the event category. If the provider uses a manifest to define the event, set this value to the same keyword mask defined in the manifest.</param>
	/// <exception cref="T:System.ArgumentException">If <paramref name="eventMessage" /> is null.</exception>
	/// <filterpriority>2</filterpriority>
	[SecurityCritical]
	public bool WriteMessageEvent(string eventMessage, byte eventLevel, long eventKeywords)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return default(bool);
	}

	/// <summary>Links events together when tracing events in an end-to-end scenario. The event data is specified as a block of memory.</summary>
	/// <returns>Is true if the event is written; otherwise, false. If false, call the <see cref="M:System.Diagnostics.Eventing.EventProvider.GetLastWriteEventError" /> method to determine the cause of the failure.</returns>
	/// <param name="eventDescriptor">An instance of <see cref="T:System.Diagnostics.Eventing.EventDescriptor" /> that identifies the event to write.</param>
	/// <param name="relatedActivityId">Activity identifier from the previous component. Use this parameter to link your component's events to the previous component's events.</param>
	/// <param name="dataCount">Size of the event data to which the <paramref name="data" /> parameter points. The maximum event data size is limited to 64 KB minus the size of the event headers. The event size is less if the session's buffer size is less and the session includes extended data items with the event.</param>
	/// <param name="data">Pointer to the event data to write.</param>
	[SecurityCritical]
	protected bool WriteTransferEvent(ref EventDescriptor eventDescriptor, Guid relatedActivityId, int dataCount, IntPtr data)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return default(bool);
	}

	/// <summary>Links events together when tracing events in an end-to-end scenario. The event data is specified as an array of objects.</summary>
	/// <returns>Is true if the event is written; otherwise, false. If false, call the <see cref="M:System.Diagnostics.Eventing.EventProvider.GetLastWriteEventError" /> method to determine the cause of the failure.</returns>
	/// <param name="eventDescriptor">An instance of <see cref="T:System.Diagnostics.Eventing.EventDescriptor" /> that identifies the event to write.</param>
	/// <param name="relatedActivityId">Activity identifier from the previous component. Use this parameter to link your component's events to the previous component's events.</param>
	/// <param name="eventPayload">An array of objects that contain the event data to write. The data must be in the order specified in the manifest. The array is limited to 32 objects, of which only eight may be strings. The maximum data size for the event is limited to 64 KB minus the size of the event headers. The event size is less if the session's buffer size is less and the session includes extended data items with the event.</param>
	/// <exception cref="T:System.ArgumentException">If <paramref name="eventPayload" /> contains too many objects or strings.</exception>
	/// <filterpriority>2</filterpriority>
	[SecurityCritical]
	public bool WriteTransferEvent(ref EventDescriptor eventDescriptor, Guid relatedActivityId, object[] eventPayload)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return default(bool);
	}
}
