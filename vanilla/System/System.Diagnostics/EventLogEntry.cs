using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Unity;

namespace System.Diagnostics;

/// <summary>Encapsulates a single record in the event log. This class cannot be inherited.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[DesignTimeVisible(false)]
[ToolboxItem(false)]
[PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
public sealed class EventLogEntry : Component, ISerializable
{
	private string category;

	private short categoryNumber;

	private byte[] data;

	private EventLogEntryType entryType;

	private int eventID;

	private int index;

	private string machineName;

	private string message;

	private string[] replacementStrings;

	private string source;

	private DateTime timeGenerated;

	private DateTime timeWritten;

	private string userName;

	private long instanceId;

	/// <summary>Gets the text associated with the <see cref="P:System.Diagnostics.EventLogEntry.CategoryNumber" /> property for this entry.</summary>
	/// <returns>The application-specific category text.</returns>
	/// <exception cref="T:System.Exception">The space could not be allocated for one of the insertion strings associated with the category. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[MonitoringDescription("The category of this event entry.")]
	public string Category => category;

	/// <summary>Gets the category number of the event log entry.</summary>
	/// <returns>The application-specific category number for this entry.</returns>
	/// <filterpriority>1</filterpriority>
	[MonitoringDescription("An ID for the category of this event entry.")]
	public short CategoryNumber => categoryNumber;

	/// <summary>Gets the binary data associated with the entry.</summary>
	/// <returns>An array of bytes that holds the binary data associated with the entry.</returns>
	/// <filterpriority>1</filterpriority>
	[MonitoringDescription("Binary data associated with this event entry.")]
	public byte[] Data => data;

	/// <summary>Gets the event type of this entry.</summary>
	/// <returns>The event type that is associated with the entry in the event log.</returns>
	/// <filterpriority>1</filterpriority>
	[MonitoringDescription("The type of this event entry.")]
	public EventLogEntryType EntryType => entryType;

	/// <summary>Gets the application-specific event identifier for the current event entry.</summary>
	/// <returns>The application-specific identifier for the event message.</returns>
	/// <filterpriority>3</filterpriority>
	[Obsolete("Use InstanceId")]
	[MonitoringDescription("An ID number for this event entry.")]
	public int EventID => eventID;

	/// <summary>Gets the index of this entry in the event log.</summary>
	/// <returns>The index of this entry in the event log.</returns>
	/// <filterpriority>2</filterpriority>
	[MonitoringDescription("Sequence numer of this event entry.")]
	public int Index => index;

	/// <summary>Gets the resource identifier that designates the message text of the event entry.</summary>
	/// <returns>A resource identifier that corresponds to a string definition in the message resource file of the event source.</returns>
	/// <filterpriority>1</filterpriority>
	[MonitoringDescription("The instance ID for this event entry.")]
	[ComVisible(false)]
	public long InstanceId => instanceId;

	/// <summary>Gets the name of the computer on which this entry was generated.</summary>
	/// <returns>The name of the computer that contains the event log.</returns>
	/// <filterpriority>1</filterpriority>
	[MonitoringDescription("The Computer on which this event entry occured.")]
	public string MachineName => machineName;

	/// <summary>Gets the localized message associated with this event entry.</summary>
	/// <returns>The formatted, localized text for the message. This includes associated replacement strings.</returns>
	/// <exception cref="T:System.Exception">The space could not be allocated for one of the insertion strings associated with the message. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[Editor("System.ComponentModel.Design.BinaryEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	[MonitoringDescription("The message of this event entry.")]
	public string Message => message;

	/// <summary>Gets the replacement strings associated with the event log entry.</summary>
	/// <returns>An array that holds the replacement strings stored in the event entry.</returns>
	/// <filterpriority>2</filterpriority>
	[MonitoringDescription("Application strings for this event entry.")]
	public string[] ReplacementStrings => replacementStrings;

	/// <summary>Gets the name of the application that generated this event.</summary>
	/// <returns>The name registered with the event log as the source of this event.</returns>
	/// <filterpriority>1</filterpriority>
	[MonitoringDescription("The source application of this event entry.")]
	public string Source => source;

	/// <summary>Gets the local time at which this event was generated.</summary>
	/// <returns>The local time at which this event was generated.</returns>
	/// <filterpriority>2</filterpriority>
	[MonitoringDescription("Generation time of this event entry.")]
	public DateTime TimeGenerated => timeGenerated;

	/// <summary>Gets the local time at which this event was written to the log.</summary>
	/// <returns>The local time at which this event was written to the log.</returns>
	/// <filterpriority>1</filterpriority>
	[MonitoringDescription("The time at which this event entry was written to the logfile.")]
	public DateTime TimeWritten => timeWritten;

	/// <summary>Gets the name of the user who is responsible for this event.</summary>
	/// <returns>The security identifier (SID) that uniquely identifies a user or group.</returns>
	/// <exception cref="T:System.SystemException">Account information could not be obtained for the user's SID. </exception>
	/// <filterpriority>1</filterpriority>
	[MonitoringDescription("The name of a user associated with this event entry.")]
	public string UserName => userName;

	internal EventLogEntry(string category, short categoryNumber, int index, int eventID, string source, string message, string userName, string machineName, EventLogEntryType entryType, DateTime timeGenerated, DateTime timeWritten, byte[] data, string[] replacementStrings, long instanceId)
	{
		this.category = category;
		this.categoryNumber = categoryNumber;
		this.data = data;
		this.entryType = entryType;
		this.eventID = eventID;
		this.index = index;
		this.machineName = machineName;
		this.message = message;
		this.replacementStrings = replacementStrings;
		this.source = source;
		this.timeGenerated = timeGenerated;
		this.timeWritten = timeWritten;
		this.userName = userName;
		this.instanceId = instanceId;
	}

	[System.MonoTODO]
	private EventLogEntry(SerializationInfo info, StreamingContext context)
	{
	}

	/// <summary>Performs a comparison between two event log entries.</summary>
	/// <returns>true if the <see cref="T:System.Diagnostics.EventLogEntry" /> objects are identical; otherwise, false.</returns>
	/// <param name="otherEntry">The <see cref="T:System.Diagnostics.EventLogEntry" /> to compare. </param>
	/// <filterpriority>1</filterpriority>
	public bool Equals(EventLogEntry otherEntry)
	{
		if (otherEntry == this)
		{
			return true;
		}
		if (otherEntry.Category == category && otherEntry.CategoryNumber == categoryNumber && otherEntry.Data.Equals(data) && otherEntry.EntryType == entryType && otherEntry.InstanceId == instanceId && otherEntry.Index == index && otherEntry.MachineName == machineName && otherEntry.Message == message && otherEntry.ReplacementStrings.Equals(replacementStrings) && otherEntry.Source == source && otherEntry.TimeGenerated.Equals(timeGenerated) && otherEntry.TimeWritten.Equals(timeWritten))
		{
			return otherEntry.UserName == userName;
		}
		return false;
	}

	/// <summary>Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with the data needed to serialize the target object.</summary>
	/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> to populate with data. </param>
	/// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext" />) for this serialization. </param>
	[System.MonoTODO("Needs serialization support")]
	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
		throw new NotImplementedException();
	}

	internal EventLogEntry()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
