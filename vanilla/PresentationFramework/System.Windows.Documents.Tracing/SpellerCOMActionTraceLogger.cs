using System.Collections.Generic;
using System.Diagnostics.Tracing;
using MS.Internal;
using MS.Internal.Telemetry.PresentationFramework;

namespace System.Windows.Documents.Tracing;

internal class SpellerCOMActionTraceLogger : IDisposable
{
	public enum Actions
	{
		SpellCheckerCreation,
		RegisterUserDictionary,
		UnregisterUserDictionary,
		ComprehensiveCheck
	}

	private class InstanceInfo
	{
		public Guid Id { get; set; }

		public Dictionary<Actions, long> CumulativeCallTime100Ns { get; set; }

		public Dictionary<Actions, long> NumCallsMeasured { get; set; }
	}

	[EventData]
	private struct SpellerCOMTimingData
	{
		public string TextBoxBaseIdentifier { get; set; }

		public string SpellerCOMAction { get; set; }

		public long CallTimeForCOMCallMs { get; set; }

		public long RunningAverageCallTimeForCOMCallsMs { get; set; }
	}

	private static readonly string SpellerCOMLatencyMeasurement = "SpellerCOMLatencyMeasurement";

	private static readonly Dictionary<Actions, long> _timeLimits100Ns = new Dictionary<Actions, long>
	{
		{
			Actions.SpellCheckerCreation,
			2500000L
		},
		{
			Actions.ComprehensiveCheck,
			500000L
		},
		{
			Actions.RegisterUserDictionary,
			10000000L
		},
		{
			Actions.UnregisterUserDictionary,
			10000000L
		}
	};

	private static WeakDictionary<WinRTSpellerInterop, InstanceInfo> _instanceInfos = new WeakDictionary<WinRTSpellerInterop, InstanceInfo>();

	private static readonly object _lockObject = new object();

	private Actions _action;

	private long _beginTicks;

	private InstanceInfo _instanceInfo;

	private bool _disposed;

	public SpellerCOMActionTraceLogger(WinRTSpellerInterop caller, Actions action)
	{
		_action = action;
		InstanceInfo value = null;
		lock (_lockObject)
		{
			if (!_instanceInfos.TryGetValue(caller, out value))
			{
				value = new InstanceInfo
				{
					Id = Guid.NewGuid(),
					CumulativeCallTime100Ns = new Dictionary<Actions, long>(),
					NumCallsMeasured = new Dictionary<Actions, long>()
				};
				foreach (Actions value2 in Enum.GetValues(typeof(Actions)))
				{
					value.CumulativeCallTime100Ns.Add(value2, 0L);
					value.NumCallsMeasured.Add(value2, 0L);
				}
				_instanceInfos.Add(caller, value);
			}
		}
		_instanceInfo = value;
		_beginTicks = DateTime.Now.Ticks;
	}

	private void UpdateRunningAverageAndLogDebugInfo(long endTicks)
	{
		try
		{
			long num = endTicks - _beginTicks;
			lock (_lockObject)
			{
				_instanceInfo.NumCallsMeasured[_action]++;
				_instanceInfo.CumulativeCallTime100Ns[_action] += num;
			}
			long num2 = (long)Math.Floor(1.0 * (double)_instanceInfo.CumulativeCallTime100Ns[_action] / (double)_instanceInfo.NumCallsMeasured[_action]);
			if (_action == Actions.RegisterUserDictionary || _action == Actions.UnregisterUserDictionary || num > _timeLimits100Ns[_action] || num2 > 2 * _timeLimits100Ns[_action])
			{
				EventSource provider = TraceLoggingProvider.GetProvider();
				EventSourceOptions options = TelemetryEventSource.MeasuresOptions();
				SpellerCOMTimingData spellerCOMTimingData = new SpellerCOMTimingData
				{
					TextBoxBaseIdentifier = _instanceInfo.Id.ToString(),
					SpellerCOMAction = _action.ToString(),
					CallTimeForCOMCallMs = (long)Math.Floor((double)num * 1.0 / 10000.0),
					RunningAverageCallTimeForCOMCallsMs = (long)Math.Floor((double)num2 * 1.0 / 10000.0)
				};
				provider?.Write(data: spellerCOMTimingData, eventName: SpellerCOMLatencyMeasurement, options: options);
			}
		}
		catch
		{
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			if (disposing)
			{
				UpdateRunningAverageAndLogDebugInfo(DateTime.Now.Ticks);
			}
			_disposed = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}
}
