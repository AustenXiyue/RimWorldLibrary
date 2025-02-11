using System.ComponentModel;

namespace System.Diagnostics;

/// <summary>Represents language-neutral information for an event log entry.</summary>
/// <filterpriority>2</filterpriority>
public class EventInstance
{
	private int _categoryId;

	private EventLogEntryType _entryType;

	private long _instanceId;

	/// <summary>Gets or sets the resource identifier that specifies the application-defined category of the event entry.</summary>
	/// <returns>A numeric category value or resource identifier that corresponds to a string defined in the category resource file of the event source. The default is zero, which signifies that no category will be displayed for the event entry.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The property is set to a negative value or to a value larger than <see cref="F:System.UInt16.MaxValue" />. </exception>
	/// <filterpriority>2</filterpriority>
	public int CategoryId
	{
		get
		{
			return _categoryId;
		}
		set
		{
			if (value < 0 || value > 65535)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			_categoryId = value;
		}
	}

	/// <summary>Gets or sets the event type of the event log entry.</summary>
	/// <returns>An <see cref="T:System.Diagnostics.EventLogEntryType" /> value that indicates the event entry type. The default value is <see cref="F:System.Diagnostics.EventLogEntryType.Information" />.</returns>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">The property is not set to a valid <see cref="T:System.Diagnostics.EventLogEntryType" /> value. </exception>
	/// <filterpriority>2</filterpriority>
	public EventLogEntryType EntryType
	{
		get
		{
			return _entryType;
		}
		set
		{
			if (!Enum.IsDefined(typeof(EventLogEntryType), value))
			{
				throw new InvalidEnumArgumentException("value", (int)value, typeof(EventLogEntryType));
			}
			_entryType = value;
		}
	}

	/// <summary>Gets or sets the resource identifier that designates the message text of the event entry.</summary>
	/// <returns>A resource identifier that corresponds to a string defined in the message resource file of the event source.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The property is set to a negative value or to a value larger than <see cref="F:System.UInt32.MaxValue" />. </exception>
	/// <filterpriority>2</filterpriority>
	public long InstanceId
	{
		get
		{
			return _instanceId;
		}
		set
		{
			if (value < 0 || value > uint.MaxValue)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			_instanceId = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.EventInstance" /> class using the specified resource identifiers for the localized message and category text of the event entry.</summary>
	/// <param name="instanceId">A resource identifier that corresponds to a string defined in the message resource file of the event source.</param>
	/// <param name="categoryId">A resource identifier that corresponds to a string defined in the category resource file of the event source, or zero to specify no category for the event. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="instanceId" /> parameter is a negative value or a value larger than <see cref="F:System.UInt32.MaxValue" />.-or- The <paramref name="categoryId" /> parameter is a negative value or a value larger than <see cref="F:System.UInt16.MaxValue" />. </exception>
	public EventInstance(long instanceId, int categoryId)
		: this(instanceId, categoryId, EventLogEntryType.Information)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.EventInstance" /> class using the specified resource identifiers for the localized message and category text of the event entry and the specified event log entry type.</summary>
	/// <param name="instanceId">A resource identifier that corresponds to a string defined in the message resource file of the event source. </param>
	/// <param name="categoryId">A resource identifier that corresponds to a string defined in the category resource file of the event source, or zero to specify no category for the event. </param>
	/// <param name="entryType">An <see cref="T:System.Diagnostics.EventLogEntryType" /> value that indicates the event type. </param>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
	///   <paramref name="entryType" /> is not a valid <see cref="T:System.Diagnostics.EventLogEntryType" /> value. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="instanceId" /> is a negative value or a value larger than <see cref="F:System.UInt32.MaxValue" />.-or- <paramref name="categoryId" /> is a negative value or a value larger than <see cref="F:System.UInt16.MaxValue" />. </exception>
	public EventInstance(long instanceId, int categoryId, EventLogEntryType entryType)
	{
		InstanceId = instanceId;
		CategoryId = categoryId;
		EntryType = entryType;
	}
}
