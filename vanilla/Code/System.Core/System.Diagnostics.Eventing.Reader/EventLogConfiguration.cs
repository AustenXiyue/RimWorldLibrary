using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using Unity;

namespace System.Diagnostics.Eventing.Reader;

/// <summary>Contains static information and configuration settings for an event log. Many of the configurations settings were defined by the event provider that created the log.</summary>
/// <filterpriority>2</filterpriority>
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public class EventLogConfiguration : IDisposable
{
	/// <summary>Gets the flag that indicates if the event log is a classic event log. A classic event log is one that has its events defined in a .mc file instead of a manifest (.xml file) used by the event provider.</summary>
	/// <returns>Returns true if the event log is a classic log, and returns false if the event log is not a classic log.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsClassicLog
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(bool);
		}
	}

	/// <summary>Gets or sets a Boolean value that determines whether the event log is enabled or disabled. An enabled log is one in which events can be logged, and a disabled log is one in which events cannot be logged.</summary>
	/// <returns>Returns true if the log is enabled, and returns false if the log is disabled.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsEnabled
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

	/// <summary>Gets or sets the file directory path to the location of the file where the events are stored for the log.</summary>
	/// <returns>Returns a string that contains the path to the event log file.</returns>
	/// <filterpriority>2</filterpriority>
	public string LogFilePath
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

	/// <summary>Gets an <see cref="T:System.Diagnostics.Eventing.Reader.EventLogIsolation" /> value that specifies whether the event log is an application, system, or custom event log. </summary>
	/// <returns>Returns an <see cref="T:System.Diagnostics.Eventing.Reader.EventLogIsolation" /> value.</returns>
	/// <filterpriority>2</filterpriority>
	public EventLogIsolation LogIsolation
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(EventLogIsolation);
		}
	}

	/// <summary>Gets or sets an <see cref="T:System.Diagnostics.Eventing.Reader.EventLogMode" /> value that determines how events are handled when the event log becomes full.</summary>
	/// <returns>Returns an <see cref="T:System.Diagnostics.Eventing.Reader.EventLogMode" /> value.</returns>
	/// <filterpriority>2</filterpriority>
	public EventLogMode LogMode
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(EventLogMode);
		}
		set
		{
			Unity.ThrowStub.ThrowNotSupportedException();
		}
	}

	/// <summary>Gets the name of the event log.</summary>
	/// <returns>Returns a string that contains the name of the event log.</returns>
	/// <filterpriority>2</filterpriority>
	public string LogName
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets an <see cref="T:System.Diagnostics.Eventing.Reader.EventLogType" /> value that determines the type of the event log.</summary>
	/// <returns>Returns an <see cref="T:System.Diagnostics.Eventing.Reader.EventLogType" /> value.</returns>
	/// <filterpriority>2</filterpriority>
	public EventLogType LogType
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(EventLogType);
		}
	}

	/// <summary>Gets or sets the maximum size, in bytes, that the event log file is allowed to be. When the file reaches this maximum size, it is considered full.</summary>
	/// <returns>Returns a long value that represents the maximum size, in bytes, that the event log file is allowed to be.</returns>
	/// <filterpriority>2</filterpriority>
	public long MaximumSizeInBytes
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(long);
		}
		set
		{
			Unity.ThrowStub.ThrowNotSupportedException();
		}
	}

	/// <summary>Gets the name of the event provider that created this event log.</summary>
	/// <returns>Returns a string that contains the name of the event provider that created this event log.</returns>
	/// <filterpriority>2</filterpriority>
	public string OwningProviderName
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets the size of the buffer that the event provider uses for publishing events to the log.</summary>
	/// <returns>Returns an integer value that can be null.</returns>
	/// <filterpriority>2</filterpriority>
	public int? ProviderBufferSize
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets the control globally unique identifier (GUID) for the event log if the log is a debug log. If this log is not a debug log, this value will be null. </summary>
	/// <returns>Returns a GUID value or null.</returns>
	/// <filterpriority>2</filterpriority>
	public Guid? ProviderControlGuid
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets or sets keyword mask used by the event provider.</summary>
	/// <returns>Returns a long value that can be null if the event provider did not define any keywords.</returns>
	/// <filterpriority>2</filterpriority>
	public long? ProviderKeywords
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

	/// <summary>Gets the maximum latency time used by the event provider when publishing events to the log.</summary>
	/// <returns>Returns an integer value that can be null if no latency time was specified by the event provider.</returns>
	/// <filterpriority>2</filterpriority>
	public int? ProviderLatency
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets or sets the maximum event level (which defines the severity of the event) that is allowed to be logged in the event log. This value is defined by the event provider.</summary>
	/// <returns>Returns an integer value that can be null if the maximum event level was not defined in the event provider.</returns>
	/// <filterpriority>2</filterpriority>
	public int? ProviderLevel
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

	/// <summary>Gets the maximum number of buffers used by the event provider to publish events to the event log.</summary>
	/// <returns>Returns an integer value that is the maximum number of buffers used by the event provider to publish events to the event log. This value can be null.</returns>
	/// <filterpriority>2</filterpriority>
	public int? ProviderMaximumNumberOfBuffers
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets the minimum number of buffers used by the event provider to publish events to the event log.</summary>
	/// <returns>Returns an integer value that is the minimum number of buffers used by the event provider to publish events to the event log. This value can be null.</returns>
	/// <filterpriority>2</filterpriority>
	public int? ProviderMinimumNumberOfBuffers
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets an enumerable collection of the names of all the event providers that can publish events to this event log.</summary>
	/// <returns>Returns an enumerable collection of strings that contain the event provider names.</returns>
	/// <filterpriority>2</filterpriority>
	public IEnumerable<string> ProviderNames
	{
		get
		{
			//IL_0007: Expected O, but got I4
			Unity.ThrowStub.ThrowNotSupportedException();
			return (IEnumerable<string>)0;
		}
	}

	/// <summary>Gets or sets the security descriptor of the event log. The security descriptor defines the users and groups of users that can read and write to the event log.</summary>
	/// <returns>Returns a string that contains the security descriptor for the event log.</returns>
	/// <filterpriority>2</filterpriority>
	public string SecurityDescriptor
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

	/// <summary>Initializes a new <see cref="T:System.Diagnostics.Eventing.Reader.EventLogConfiguration" /> object by specifying the local event log for which to get information and configuration settings. </summary>
	/// <param name="logName">The name of the local event log for which to get information and configuration settings.</param>
	/// <filterpriority>2</filterpriority>
	public EventLogConfiguration(string logName)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Initializes a new <see cref="T:System.Diagnostics.Eventing.Reader.EventLogConfiguration" /> object by specifying the name of the log for which to get information and configuration settings. The log can be on the local computer or a remote computer, based on the event log session specified.</summary>
	/// <param name="logName">The name of the event log for which to get information and configuration settings.</param>
	/// <param name="session">The event log session used to determine the event log service that the specified log belongs to. The session is either connected to the event log service on the local computer or a remote computer.</param>
	/// <filterpriority>2</filterpriority>
	[SecurityCritical]
	public EventLogConfiguration(string logName, EventLogSession session)
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

	/// <summary>Saves the configuration settings that </summary>
	/// <filterpriority>2</filterpriority>
	public void SaveChanges()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
