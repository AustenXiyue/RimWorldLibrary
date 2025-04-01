using System.Security;
using System.Security.Permissions;
using Unity;

namespace System.Diagnostics.Eventing.Reader;

/// <summary>Allows you to subscribe to incoming events. Each time a desired event is published to an event log, the <see cref="E:System.Diagnostics.Eventing.Reader.EventLogWatcher.EventRecordWritten" /> event is raised, and the method that handles this event will be executed. </summary>
/// <filterpriority>2</filterpriority>
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public class EventLogWatcher : IDisposable
{
	/// <summary>Determines whether this object starts delivering events to the event delegate.</summary>
	/// <returns>Returns true when this object can deliver events to the event delegate, and returns false when this object has stopped delivery.</returns>
	/// <filterpriority>2</filterpriority>
	public bool Enabled
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(bool);
		}
		set
		{
			Unity.ThrowStub.ThrowNotSupportedException();
		}
	}

	/// <summary>Allows setting a delegate (event handler method) that gets called every time an event is published that matches the criteria specified in the event query for this object. </summary>
	/// <filterpriority>2</filterpriority>
	public event EventHandler<EventRecordWrittenEventArgs> EventRecordWritten
	{
		add
		{
			Unity.ThrowStub.ThrowNotSupportedException();
		}
		remove
		{
			Unity.ThrowStub.ThrowNotSupportedException();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Eventing.Reader.EventLogWatcher" /> class by specifying an event query.</summary>
	/// <param name="eventQuery">Specifies a query for the event subscription. When an event is logged that matches the criteria expressed in the query, then the <see cref="E:System.Diagnostics.Eventing.Reader.EventLogWatcher.EventRecordWritten" /> event is raised. </param>
	/// <filterpriority>2</filterpriority>
	public EventLogWatcher(EventLogQuery eventQuery)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Eventing.Reader.EventLogWatcher" /> class by specifying an event query and a bookmark that is used as starting position for the query.</summary>
	/// <param name="eventQuery">Specifies a query for the event subscription. When an event is logged that matches the criteria expressed in the query, then the <see cref="E:System.Diagnostics.Eventing.Reader.EventLogWatcher.EventRecordWritten" /> event is raised.</param>
	/// <param name="bookmark">The bookmark (placeholder) used as a starting position in the event log or stream of events. Only events that have been logged after the bookmark event will be returned by the query.</param>
	/// <filterpriority>2</filterpriority>
	public EventLogWatcher(EventLogQuery eventQuery, EventBookmark bookmark)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Eventing.Reader.EventLogWatcher" /> class by specifying an event query, a bookmark that is used as starting position for the query, and a Boolean value that determines whether to read the events that already exist in the event log.</summary>
	/// <param name="eventQuery">Specifies a query for the event subscription. When an event is logged that matches the criteria expressed in the query, then the <see cref="E:System.Diagnostics.Eventing.Reader.EventLogWatcher.EventRecordWritten" /> event is raised.</param>
	/// <param name="bookmark">The bookmark (placeholder) used as a starting position in the event log or stream of events. Only events that have been logged after the bookmark event will be returned by the query.</param>
	/// <param name="readExistingEvents">A Boolean value that determines whether to read the events that already exist in the event log. If this value is true, then the existing events are read and if this value is false, then the existing events are not read.</param>
	/// <filterpriority>2</filterpriority>
	public EventLogWatcher(EventLogQuery eventQuery, EventBookmark bookmark, bool readExistingEvents)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Eventing.Reader.EventLogWatcher" /> class by specifying the name or path to an event log.</summary>
	/// <param name="path">The path or name of the event log monitor for events. If any event is logged in this event log, then the <see cref="E:System.Diagnostics.Eventing.Reader.EventLogWatcher.EventRecordWritten" /> event is raised.</param>
	/// <filterpriority>2</filterpriority>
	public EventLogWatcher(string path)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Releases all the resources used by this object.</summary>
	/// <filterpriority>2</filterpriority>
	public void Dispose()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Releases the unmanaged resources used by this object, and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
	[SecuritySafeCritical]
	protected virtual void Dispose(bool disposing)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
