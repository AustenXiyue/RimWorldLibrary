using Unity;

namespace System.Diagnostics.Eventing.Reader;

/// <summary>Represents a query for events in an event log and the settings that define how the query is executed and on what computer the query is executed on.</summary>
/// <filterpriority>2</filterpriority>
public class EventLogQuery
{
	/// <summary>Gets or sets the Boolean value that determines whether to read events from the newest event in an event log to the oldest event in the log.</summary>
	/// <returns>Returns true if events are read from the newest event in the log to the oldest event, and returns false if events are read from the oldest event in the log to the newest event.</returns>
	/// <filterpriority>2</filterpriority>
	public bool ReverseDirection
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

	/// <summary>Gets or sets the session that access the Event Log service on the local computer or a remote computer. This object can be set to access a remote event log by creating a <see cref="T:System.Diagnostics.Eventing.Reader.EventLogReader" /> object or an <see cref="T:System.Diagnostics.Eventing.Reader.EventLogWatcher" /> object with this <see cref="T:System.Diagnostics.Eventing.Reader.EventLogQuery" /> object.</summary>
	/// <returns>Returns an <see cref="T:System.Diagnostics.Eventing.Reader.EventLogSession" /> object.</returns>
	/// <filterpriority>2</filterpriority>
	public EventLogSession Session
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
		set
		{
			Unity.ThrowStub.ThrowNotSupportedException();
		}
	}

	/// <summary>Gets or sets a Boolean value that determines whether this query will continue to retrieve events when the query has an error.</summary>
	/// <returns>true indicates that the query will continue to retrieve events even if the query fails for some logs, and false indicates that this query will not continue to retrieve events when the query fails.</returns>
	/// <filterpriority>2</filterpriority>
	public bool TolerateQueryErrors
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

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Eventing.Reader.EventLogQuery" /> class by specifying the target of the query. The target can be an active event log or a log file.</summary>
	/// <param name="path">The name of the event log to query, or the path to the event log file to query.</param>
	/// <param name="pathType">Specifies whether the string used in the path parameter specifies the name of an event log, or the path to an event log file.</param>
	/// <filterpriority>2</filterpriority>
	public EventLogQuery(string path, PathType pathType)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Eventing.Reader.EventLogQuery" /> class by specifying the target of the query and the event query. The target can be an active event log or a log file.</summary>
	/// <param name="path">The name of the event log to query, or the path to the event log file to query.</param>
	/// <param name="pathType">Specifies whether the string used in the path parameter specifies the name of an event log, or the path to an event log file.</param>
	/// <param name="query">The event query used to retrieve events that match the query conditions.</param>
	/// <filterpriority>2</filterpriority>
	public EventLogQuery(string path, PathType pathType, string query)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
