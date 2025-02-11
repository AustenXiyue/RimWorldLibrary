using System;
using MS.Win32;

namespace MS.Utility;

internal class ManifestTraceProvider : TraceProvider
{
	private static ManifestEtw.EtwEnableCallback _etwEnabledCallback;

	internal ManifestTraceProvider()
	{
	}

	internal unsafe override void Register(Guid providerGuid)
	{
		_etwEnabledCallback = EtwEnableCallback;
		ulong registrationHandle = 0uL;
		ManifestEtw.EventRegister(ref providerGuid, _etwEnabledCallback, null, ref registrationHandle);
		_registrationHandle.Value = registrationHandle;
	}

	private unsafe void EtwEnableCallback(ref Guid sourceId, int isEnabled, byte level, long matchAnyKeywords, long matchAllKeywords, ManifestEtw.EVENT_FILTER_DESCRIPTOR* filterData, void* callbackContext)
	{
		_enabled = isEnabled > 0;
		_level = (EventTrace.Level)level;
		_keywords = (EventTrace.Keyword)matchAnyKeywords;
		_matchAllKeyword = (EventTrace.Keyword)matchAllKeywords;
	}

	~ManifestTraceProvider()
	{
		if (_registrationHandle.Value != 0L)
		{
			try
			{
				ManifestEtw.EventUnregister(_registrationHandle.Value);
				return;
			}
			finally
			{
				_registrationHandle.Value = 0uL;
			}
		}
	}

	internal unsafe override uint EventWrite(EventTrace.Event eventID, EventTrace.Keyword keywords, EventTrace.Level level, int argc, EventData* argv)
	{
		ManifestEtw.EventDescriptor eventDescriptor = default(ManifestEtw.EventDescriptor);
		eventDescriptor.Id = (ushort)eventID;
		eventDescriptor.Version = EventTrace.GetVersionForEvent(eventID);
		eventDescriptor.Channel = 16;
		eventDescriptor.Level = (byte)level;
		eventDescriptor.Opcode = EventTrace.GetOpcodeForEvent(eventID);
		eventDescriptor.Task = EventTrace.GetTaskForEvent(eventID);
		eventDescriptor.Keywords = (long)keywords;
		if (argc == 0)
		{
			argv = null;
		}
		return ManifestEtw.EventWrite(_registrationHandle.Value, ref eventDescriptor, (uint)argc, argv);
	}
}
