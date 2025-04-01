using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Diagnostics;

/// <summary>Provides a simple listener that directs tracing or debugging output to an <see cref="T:System.Diagnostics.EventLog" />.</summary>
/// <filterpriority>2</filterpriority>
[PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
public sealed class EventLogTraceListener : TraceListener
{
	private EventLog event_log;

	private string name;

	/// <summary>Gets or sets the event log to write to.</summary>
	/// <returns>The event log to write to.</returns>
	/// <filterpriority>2</filterpriority>
	public EventLog EventLog
	{
		get
		{
			return event_log;
		}
		set
		{
			event_log = value;
		}
	}

	/// <summary>Gets or sets the name of this <see cref="T:System.Diagnostics.EventLogTraceListener" />.</summary>
	/// <returns>The name of this trace listener.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override string Name
	{
		get
		{
			if (name == null)
			{
				return event_log.Source;
			}
			return name;
		}
		set
		{
			name = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.EventLogTraceListener" /> class without a trace listener.</summary>
	public EventLogTraceListener()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.EventLogTraceListener" /> class using the specified event log.</summary>
	/// <param name="eventLog">The event log to write to. </param>
	public EventLogTraceListener(EventLog eventLog)
	{
		if (eventLog == null)
		{
			throw new ArgumentNullException("eventLog");
		}
		event_log = eventLog;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.EventLogTraceListener" /> class using the specified source.</summary>
	/// <param name="source">The name of an existing event log source. </param>
	public EventLogTraceListener(string source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		event_log = new EventLog();
		event_log.Source = source;
	}

	/// <summary>Closes the event log so that it no longer receives tracing or debugging output.</summary>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override void Close()
	{
		event_log.Close();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			event_log.Dispose();
		}
	}

	/// <summary>Writes a message to the event log for this instance.</summary>
	/// <param name="message">The message to write. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="message" /> exceeds 32,766 characters.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override void Write(string message)
	{
		TraceData(new TraceEventCache(), event_log.Source, TraceEventType.Information, 0, message);
	}

	/// <summary>Writes a message to the event log for this instance.</summary>
	/// <param name="message">The message to write. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="message" /> exceeds 32,766 characters.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override void WriteLine(string message)
	{
		Write(message);
	}

	/// <summary>Writes trace information, a data object, and event information to the event log.</summary>
	/// <param name="eventCache">An object that contains the current process ID, thread ID, and stack trace information.</param>
	/// <param name="source">A name used to identify the output; typically the name of the application that generated the trace event.</param>
	/// <param name="severity">One of the enumeration values that specifies the type of event that has caused the trace.</param>
	/// <param name="id">A numeric identifier for the event. The combination of <paramref name="source" /> and <paramref name="id" /> uniquely identifies an event.</param>
	/// <param name="data">A data object to write to the output file or stream.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="source" /> is not specified.-or-The log entry string exceeds 32,766 characters.</exception>
	[ComVisible(false)]
	public override void TraceData(TraceEventCache eventCache, string source, TraceEventType severity, int id, object data)
	{
		EventLogEntryType type;
		switch (severity)
		{
		case TraceEventType.Critical:
		case TraceEventType.Error:
			type = EventLogEntryType.Error;
			break;
		case TraceEventType.Warning:
			type = EventLogEntryType.Warning;
			break;
		default:
			type = EventLogEntryType.Information;
			break;
		}
		event_log.WriteEntry((data != null) ? data.ToString() : string.Empty, type, id, 0);
	}

	/// <summary>Writes trace information, an array of data objects, and event information to the event log.</summary>
	/// <param name="eventCache">An object that contains the current process ID, thread ID, and stack trace information.</param>
	/// <param name="source">A name used to identify the output; typically the name of the application that generated the trace event.</param>
	/// <param name="severity">One of the enumeration values that specifies the type of event that has caused the trace.</param>
	/// <param name="id">A numeric identifier for the event. The combination of <paramref name="source" /> and <paramref name="id" /> uniquely identifies an event.</param>
	/// <param name="data">An array of data objects.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="source" /> is not specified.-or-The log entry string exceeds 32,766 characters.</exception>
	[ComVisible(false)]
	public override void TraceData(TraceEventCache eventCache, string source, TraceEventType severity, int id, params object[] data)
	{
		string data2 = string.Empty;
		if (data != null)
		{
			string[] array = new string[data.Length];
			for (int i = 0; i < data.Length; i++)
			{
				array[i] = ((data[i] != null) ? data[i].ToString() : string.Empty);
			}
			data2 = string.Join(", ", array);
		}
		TraceData(eventCache, source, severity, id, data2);
	}

	/// <summary>Writes trace information, a message, and event information to the event log.</summary>
	/// <param name="eventCache">An object that contains the current process ID, thread ID, and stack trace information.</param>
	/// <param name="source">A name used to identify the output; typically the name of the application that generated the trace event.</param>
	/// <param name="severity">One of the enumeration values that specifies the type of event that has caused the trace.</param>
	/// <param name="id">A numeric identifier for the event. The combination of <paramref name="source" /> and <paramref name="id" /> uniquely identifies an event.</param>
	/// <param name="message">The trace message.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="source" /> is not specified.-or-The log entry string exceeds 32,766 characters.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[ComVisible(false)]
	public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType severity, int id, string message)
	{
		TraceData(eventCache, source, severity, id, message);
	}

	/// <summary>Writes trace information, a formatted array of objects, and event information to the event log.</summary>
	/// <param name="eventCache">An object that contains the current process ID, thread ID, and stack trace information.</param>
	/// <param name="source">A name used to identify the output; typically the name of the application that generated the trace event.</param>
	/// <param name="severity">One of the enumeration values that specifies the type of event that has caused the trace.</param>
	/// <param name="id">A numeric identifier for the event. The combination of <paramref name="source" /> and <paramref name="id" /> uniquely identifies an event.</param>
	/// <param name="format">A format string that contains zero or more format items that correspond to objects in the <paramref name="args" /> array.</param>
	/// <param name="args">An object array containing zero or more objects to format.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="source" /> is not specified.-or-The log entry string exceeds 32,766 characters.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[ComVisible(false)]
	public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType severity, int id, string format, params object[] args)
	{
		TraceEvent(eventCache, source, severity, id, (format != null) ? string.Format(format, args) : null);
	}
}
