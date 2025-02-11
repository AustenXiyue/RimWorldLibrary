using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Security.Permissions;
using Unity;

namespace System.Diagnostics.Eventing.Reader;

/// <summary>Enables you to read events from an event log based on an event query. The events that are read by this object are returned as <see cref="T:System.Diagnostics.Eventing.Reader.EventRecord" /> objects.</summary>
/// <filterpriority>2</filterpriority>
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public class EventLogReader : IDisposable
{
	/// <summary>Gets or sets the number of events retrieved from the stream of events on every read operation.</summary>
	/// <returns>Returns an integer value.</returns>
	/// <filterpriority>2</filterpriority>
	public int BatchSize
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(int);
		}
		set
		{
			Unity.ThrowStub.ThrowNotSupportedException();
		}
	}

	/// <summary>Gets the status of each event log or log file associated with the event query in this object.</summary>
	/// <returns>Returns a list of <see cref="T:System.Diagnostics.Eventing.Reader.EventLogStatus" /> objects that each contain status information about an event log associated with the event query in this object.</returns>
	/// <filterpriority>2</filterpriority>
	public IList<EventLogStatus> LogStatus
	{
		[SecurityCritical]
		get
		{
			//IL_0007: Expected O, but got I4
			Unity.ThrowStub.ThrowNotSupportedException();
			return (IList<EventLogStatus>)0;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Eventing.Reader.EventLogReader" /> class by specifying an event query.</summary>
	/// <param name="eventQuery">The event query used to retrieve events.</param>
	/// <filterpriority>2</filterpriority>
	public EventLogReader(EventLogQuery eventQuery)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Eventing.Reader.EventLogReader" /> class by specifying an event query and a bookmark that is used as starting position for the query.</summary>
	/// <param name="eventQuery">The event query used to retrieve events.</param>
	/// <param name="bookmark">The bookmark (placeholder) used as a starting position in the event log or stream of events. Only events logged after the bookmark event will be returned by the query.</param>
	/// <filterpriority>2</filterpriority>
	[SecurityCritical]
	public EventLogReader(EventLogQuery eventQuery, EventBookmark bookmark)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Eventing.Reader.EventLogReader" /> class by specifying an active event log to retrieve events from.</summary>
	/// <param name="path">The name of the event log to retrieve events from.</param>
	/// <filterpriority>2</filterpriority>
	public EventLogReader(string path)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Eventing.Reader.EventLogReader" /> class by specifying the name of an event log to retrieve events from or the path to a log file to retrieve events from.</summary>
	/// <param name="path">The name of the event log to retrieve events from, or the path to the event log file to retrieve events from.</param>
	/// <param name="pathType">Specifies whether the string used in the path parameter specifies the name of an event log, or the path to an event log file.</param>
	/// <filterpriority>2</filterpriority>
	public EventLogReader(string path, PathType pathType)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Cancels the current query operation.</summary>
	/// <filterpriority>2</filterpriority>
	public void CancelReading()
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

	/// <summary>Reads the next event that is returned from the event query in this object.</summary>
	/// <returns>Returns an <see cref="T:System.Diagnostics.Eventing.Reader.EventRecord" /> object.</returns>
	/// <filterpriority>2</filterpriority>
	public EventRecord ReadEvent()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return null;
	}

	/// <summary>Reads the next event that is returned from the event query in this object.</summary>
	/// <returns>Returns an <see cref="T:System.Diagnostics.Eventing.Reader.EventRecord" /> object.</returns>
	/// <param name="timeout">The maximum time to allow the read operation to run before canceling the operation.</param>
	/// <filterpriority>2</filterpriority>
	[SecurityCritical]
	public EventRecord ReadEvent(TimeSpan timeout)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return null;
	}

	/// <summary>Changes the position in the event stream where the next event that is read will come from by specifying a bookmark event. No events logged before the bookmark event will be retrieved.</summary>
	/// <param name="bookmark">The bookmark (placeholder) used as a starting position in the event log or stream of events. Only events that have been logged after the bookmark event will be returned by the query.</param>
	/// <filterpriority>2</filterpriority>
	public void Seek(EventBookmark bookmark)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Changes the position in the event stream where the next event that is read will come from by specifying a bookmark event and an offset number of events from the bookmark. No events logged before the bookmark plus the offset will be retrieved.</summary>
	/// <param name="bookmark">The bookmark (placeholder) used as a starting position in the event log or stream of events. Only events that have been logged after the bookmark event will be returned by the query.</param>
	/// <param name="offset">The offset number of events to change the position of the bookmark.</param>
	/// <filterpriority>2</filterpriority>
	[SecurityCritical]
	public void Seek(EventBookmark bookmark, long offset)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Changes the position in the event stream where the next event that is read will come from by specifying a starting position and an offset from the starting position. No events logged before the starting position plus the offset will be retrieved.</summary>
	/// <param name="origin">A value from the <see cref="T:System.IO.SeekOrigin" /> enumeration defines where in the stream of events to start querying for events.</param>
	/// <param name="offset">The offset number of events to add to the origin.</param>
	/// <filterpriority>2</filterpriority>
	[SecurityCritical]
	public void Seek(SeekOrigin origin, long offset)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
