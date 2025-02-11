using System.Collections.Generic;
using Unity;

namespace System.Diagnostics.Tracing;

/// <summary>Provides the arguments for the <see cref="M:System.Diagnostics.Tracing.EventSource.OnEventCommand(System.Diagnostics.Tracing.EventCommandEventArgs)" /> callback.</summary>
public class EventCommandEventArgs : EventArgs
{
	internal EventSource eventSource;

	internal EventDispatcher dispatcher;

	internal EventListener listener;

	internal int perEventSourceSessionId;

	internal int etwSessionId;

	internal bool enable;

	internal EventLevel level;

	internal EventKeywords matchAnyKeyword;

	internal EventCommandEventArgs nextCommand;

	/// <summary>Gets the command for the callback.</summary>
	/// <returns>The callback command.</returns>
	public EventCommand Command { get; internal set; }

	/// <summary>Gets the array of arguments for the callback.</summary>
	/// <returns>An array of callback arguments.</returns>
	public IDictionary<string, string> Arguments { get; internal set; }

	/// <summary>Enables the event that has the specified identifier.</summary>
	/// <returns>true if <paramref name="eventId" /> is in range; otherwise, false.</returns>
	/// <param name="eventId">The identifier of the event to enable.</param>
	public bool EnableEvent(int eventId)
	{
		if (Command != EventCommand.Enable && Command != EventCommand.Disable)
		{
			throw new InvalidOperationException();
		}
		return eventSource.EnableEventForDispatcher(dispatcher, eventId, value: true);
	}

	/// <summary>Disables the event that have the specified identifier.</summary>
	/// <returns>true if <paramref name="eventId" /> is in range; otherwise, false.</returns>
	/// <param name="eventId">The identifier of the event to disable.</param>
	public bool DisableEvent(int eventId)
	{
		if (Command != EventCommand.Enable && Command != EventCommand.Disable)
		{
			throw new InvalidOperationException();
		}
		return eventSource.EnableEventForDispatcher(dispatcher, eventId, value: false);
	}

	internal EventCommandEventArgs(EventCommand command, IDictionary<string, string> arguments, EventSource eventSource, EventListener listener, int perEventSourceSessionId, int etwSessionId, bool enable, EventLevel level, EventKeywords matchAnyKeyword)
	{
		Command = command;
		Arguments = arguments;
		this.eventSource = eventSource;
		this.listener = listener;
		this.perEventSourceSessionId = perEventSourceSessionId;
		this.etwSessionId = etwSessionId;
		this.enable = enable;
		this.level = level;
		this.matchAnyKeyword = matchAnyKeyword;
	}

	internal EventCommandEventArgs()
	{
		ThrowStub.ThrowNotSupportedException();
	}
}
