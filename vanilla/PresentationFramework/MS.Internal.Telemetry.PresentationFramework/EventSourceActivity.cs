using System;
using System.Diagnostics.Contracts;
using System.Diagnostics.Tracing;

namespace MS.Internal.Telemetry.PresentationFramework;

internal sealed class EventSourceActivity : IDisposable
{
	private enum State
	{
		Initialized,
		Started,
		Stopped
	}

	[EventData]
	private class EmptyStruct
	{
		private static EmptyStruct _instance;

		internal static EmptyStruct Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new EmptyStruct();
				}
				return _instance;
			}
		}

		private EmptyStruct()
		{
		}
	}

	private static Guid _emptyGuid;

	private readonly EventSource _eventSource;

	private EventSourceOptions _startStopOptions;

	private Guid _parentId;

	private Guid _id = Guid.NewGuid();

	private State _state;

	internal EventSource EventSource => _eventSource;

	internal Guid Id => _id;

	internal EventSourceActivity(EventSource eventSource)
		: this(eventSource, default(EventSourceOptions))
	{
	}

	internal EventSourceActivity(EventSource eventSource, EventSourceOptions startStopOptions)
		: this(eventSource, startStopOptions, Guid.Empty)
	{
	}

	internal EventSourceActivity(EventSource eventSource, EventSourceOptions startStopOptions, Guid parentActivityId)
	{
		Contract.Requires<ArgumentNullException>(eventSource != null, "eventSource");
		_eventSource = eventSource;
		_startStopOptions = startStopOptions;
		_parentId = parentActivityId;
	}

	internal EventSourceActivity(EventSourceActivity parentActivity)
		: this(parentActivity, default(EventSourceOptions))
	{
	}

	internal EventSourceActivity(EventSourceActivity parentActivity, EventSourceOptions startStopOptions)
	{
		Contract.Requires<ArgumentNullException>(parentActivity != null, "parentActivity");
		_eventSource = parentActivity.EventSource;
		_startStopOptions = startStopOptions;
		_parentId = parentActivity.Id;
	}

	internal void Start(string eventName)
	{
		Contract.Requires<ArgumentNullException>(eventName != null, "eventName");
		EmptyStruct data = EmptyStruct.Instance;
		Start(eventName, ref data);
	}

	internal void Start<T>(string eventName, T data)
	{
		Start(eventName, ref data);
	}

	internal void Stop(string eventName)
	{
		Contract.Requires<ArgumentNullException>(eventName != null, "eventName");
		EmptyStruct data = EmptyStruct.Instance;
		Stop(eventName, ref data);
	}

	internal void Stop<T>(string eventName, T data)
	{
		Stop(eventName, ref data);
	}

	internal void Write(string eventName)
	{
		Contract.Requires<ArgumentNullException>(eventName != null, "eventName");
		EventSourceOptions options = default(EventSourceOptions);
		EmptyStruct data = EmptyStruct.Instance;
		Write(eventName, ref options, ref data);
	}

	internal void Write(string eventName, EventSourceOptions options)
	{
		Contract.Requires<ArgumentNullException>(eventName != null, "eventName");
		EmptyStruct data = EmptyStruct.Instance;
		Write(eventName, ref options, ref data);
	}

	internal void Write<T>(string eventName, T data)
	{
		EventSourceOptions options = default(EventSourceOptions);
		Write(eventName, ref options, ref data);
	}

	internal void Write<T>(string eventName, EventSourceOptions options, T data)
	{
		Write(eventName, ref options, ref data);
	}

	public void Dispose()
	{
		if (_state == State.Started)
		{
			_state = State.Stopped;
			EmptyStruct data = EmptyStruct.Instance;
			_eventSource.Write("Dispose", ref _startStopOptions, ref _id, ref _emptyGuid, ref data);
		}
	}

	private void Start<T>(string eventName, ref T data)
	{
		if (_state != 0)
		{
			throw new InvalidOperationException();
		}
		_state = State.Started;
		_startStopOptions.Opcode = EventOpcode.Start;
		_eventSource.Write(eventName, ref _startStopOptions, ref _id, ref _parentId, ref data);
		_startStopOptions.Opcode = EventOpcode.Stop;
	}

	private void Write<T>(string eventName, ref EventSourceOptions options, ref T data)
	{
		if (_state != State.Started)
		{
			throw new InvalidOperationException();
		}
		_eventSource.Write(eventName, ref options, ref _id, ref _emptyGuid, ref data);
	}

	private void Stop<T>(string eventName, ref T data)
	{
		if (_state != State.Started)
		{
			throw new InvalidOperationException();
		}
		_state = State.Stopped;
		_eventSource.Write(eventName, ref _startStopOptions, ref _id, ref _emptyGuid, ref data);
	}
}
