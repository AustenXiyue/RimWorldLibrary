using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Security;
using Unity;

namespace System.Diagnostics.Tracing;

/// <summary>Provides data for the <see cref="M:System.Diagnostics.Tracing.EventListener.OnEventWritten(System.Diagnostics.Tracing.EventWrittenEventArgs)" /> callback.</summary>
public class EventWrittenEventArgs : EventArgs
{
	private string m_message;

	private string m_eventName;

	private EventSource m_eventSource;

	private ReadOnlyCollection<string> m_payloadNames;

	internal EventTags m_tags;

	internal EventOpcode m_opcode;

	internal EventKeywords m_keywords;

	public string EventName
	{
		get
		{
			if (m_eventName != null || EventId < 0)
			{
				return m_eventName;
			}
			return m_eventSource.m_eventData[EventId].Name;
		}
		internal set
		{
			m_eventName = value;
		}
	}

	/// <summary>Gets the event identifier.</summary>
	/// <returns>The event identifier.</returns>
	public int EventId { get; internal set; }

	/// <summary>Gets the activity ID on the thread that the event was written to. </summary>
	/// <returns>The activity ID on the thread that the event was written to. </returns>
	public Guid ActivityId
	{
		[SecurityCritical]
		get
		{
			return EventSource.CurrentThreadActivityId;
		}
	}

	/// <summary>Gets the identifier of an activity that is related to the activity represented by the current instance. </summary>
	/// <returns>The identifier of the related activity, or <see cref="F:System.Guid.Empty" /> if there is no related activity.</returns>
	public Guid RelatedActivityId
	{
		[SecurityCritical]
		get;
		internal set; }

	/// <summary>Gets the payload for the event.</summary>
	/// <returns>The payload for the event.</returns>
	public ReadOnlyCollection<object> Payload { get; internal set; }

	public ReadOnlyCollection<string> PayloadNames
	{
		get
		{
			if (m_payloadNames == null)
			{
				List<string> list = new List<string>();
				ParameterInfo[] parameters = m_eventSource.m_eventData[EventId].Parameters;
				foreach (ParameterInfo parameterInfo in parameters)
				{
					list.Add(parameterInfo.Name);
				}
				m_payloadNames = new ReadOnlyCollection<string>(list);
			}
			return m_payloadNames;
		}
		internal set
		{
			m_payloadNames = value;
		}
	}

	/// <summary>Gets the event source object.</summary>
	/// <returns>The event source object.</returns>
	public EventSource EventSource => m_eventSource;

	/// <summary>Gets the keywords for the event.</summary>
	/// <returns>The keywords for the event.</returns>
	public EventKeywords Keywords
	{
		get
		{
			if (EventId < 0)
			{
				return m_keywords;
			}
			return (EventKeywords)m_eventSource.m_eventData[EventId].Descriptor.Keywords;
		}
	}

	/// <summary>Gets the operation code for the event.</summary>
	/// <returns>The operation code for the event.</returns>
	public EventOpcode Opcode
	{
		get
		{
			if (EventId < 0)
			{
				return m_opcode;
			}
			return (EventOpcode)m_eventSource.m_eventData[EventId].Descriptor.Opcode;
		}
	}

	/// <summary>Gets the task for the event.</summary>
	/// <returns>The task for the event.</returns>
	public EventTask Task
	{
		get
		{
			if (EventId < 0)
			{
				return EventTask.None;
			}
			return (EventTask)m_eventSource.m_eventData[EventId].Descriptor.Task;
		}
	}

	public EventTags Tags
	{
		get
		{
			if (EventId < 0)
			{
				return m_tags;
			}
			return m_eventSource.m_eventData[EventId].Tags;
		}
	}

	/// <summary>Gets the message for the event.</summary>
	/// <returns>The message for the event.</returns>
	public string Message
	{
		get
		{
			if (EventId < 0)
			{
				return m_message;
			}
			return m_eventSource.m_eventData[EventId].Message;
		}
		internal set
		{
			m_message = value;
		}
	}

	public EventChannel Channel
	{
		get
		{
			if (EventId < 0)
			{
				return EventChannel.None;
			}
			return (EventChannel)m_eventSource.m_eventData[EventId].Descriptor.Channel;
		}
	}

	/// <summary>Gets the version of the event.</summary>
	/// <returns>The version of the event.</returns>
	public byte Version
	{
		get
		{
			if (EventId < 0)
			{
				return 0;
			}
			return m_eventSource.m_eventData[EventId].Descriptor.Version;
		}
	}

	/// <summary>Gets the level of the event.</summary>
	/// <returns>The level of the event.</returns>
	public EventLevel Level
	{
		get
		{
			if (EventId < 0)
			{
				return EventLevel.LogAlways;
			}
			return (EventLevel)m_eventSource.m_eventData[EventId].Descriptor.Level;
		}
	}

	internal EventWrittenEventArgs(EventSource eventSource)
	{
		m_eventSource = eventSource;
	}

	internal EventWrittenEventArgs()
	{
		ThrowStub.ThrowNotSupportedException();
	}
}
