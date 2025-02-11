using System.Security.Permissions;
using Unity;

namespace System.Diagnostics.Eventing.Reader;

/// <summary>Allows you to access the run-time properties of active event logs and event log files. These properties include the number of events in the log, the size of the log, a value that determines whether the log is full, and the last time the log was written to or accessed.</summary>
/// <filterpriority>2</filterpriority>
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public sealed class EventLogInformation
{
	/// <summary>Gets the file attributes of the log file associated with the log.</summary>
	/// <returns>Returns an integer value. This value can be null.</returns>
	/// <filterpriority>2</filterpriority>
	public int? Attributes
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets the time that the log file associated with the event log was created.</summary>
	/// <returns>Returns a <see cref="T:System.DateTime" /> object. This value can be null.</returns>
	/// <filterpriority>2</filterpriority>
	public DateTime? CreationTime
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets the size of the file, in bytes, associated with the event log.</summary>
	/// <returns>Returns a long value.</returns>
	/// <filterpriority>2</filterpriority>
	public long? FileSize
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets a Boolean value that determines whether the log file has reached its maximum size (the log is full).</summary>
	/// <returns>Returns true if the log is full, and returns false if the log is not full.</returns>
	/// <filterpriority>2</filterpriority>
	public bool? IsLogFull
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets the last time the log file associated with the event log was accessed.</summary>
	/// <returns>Returns a <see cref="T:System.DateTime" /> object. This value can be null.</returns>
	/// <filterpriority>2</filterpriority>
	public DateTime? LastAccessTime
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets the last time data was written to the log file associated with the event log.</summary>
	/// <returns>Returns a <see cref="T:System.DateTime" /> object. This value can be null.</returns>
	/// <filterpriority>2</filterpriority>
	public DateTime? LastWriteTime
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets the number of the oldest event record in the event log.</summary>
	/// <returns>Returns a long value that represents the number of the oldest event record in the event log. This value can be null.</returns>
	/// <filterpriority>2</filterpriority>
	public long? OldestRecordNumber
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets the number of event records in the event log.</summary>
	/// <returns>Returns a long value that represents the number of event records in the event log. This value can be null.</returns>
	/// <filterpriority>2</filterpriority>
	public long? RecordCount
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	internal EventLogInformation()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
