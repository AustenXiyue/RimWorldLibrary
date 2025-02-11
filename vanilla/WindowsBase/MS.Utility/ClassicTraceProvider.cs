using System;
using MS.Internal;
using MS.Win32;

namespace MS.Utility;

internal sealed class ClassicTraceProvider : TraceProvider
{
	private ulong _traceHandle;

	private static ClassicEtw.ControlCallback _etwProc;

	internal ClassicTraceProvider()
	{
	}

	internal unsafe override void Register(Guid providerGuid)
	{
		Guid guid = new Guid(3029687280u, 15089, 18240, 180, 117, 153, 5, 93, 63, 233, 170);
		_etwProc = EtwEnableCallback;
		ClassicEtw.TRACE_GUID_REGISTRATION taskGuids = default(ClassicEtw.TRACE_GUID_REGISTRATION);
		taskGuids.Guid = &guid;
		taskGuids.RegHandle = null;
		ClassicEtw.RegisterTraceGuidsW(_etwProc, IntPtr.Zero, ref providerGuid, 1, ref taskGuids, null, null, out var regHandle);
		_registrationHandle.Value = regHandle;
	}

	private unsafe uint EtwEnableCallback(ClassicEtw.WMIDPREQUESTCODE requestCode, nint context, nint bufferSize, ClassicEtw.WNODE_HEADER* buffer)
	{
		try
		{
			switch (requestCode)
			{
			case ClassicEtw.WMIDPREQUESTCODE.EnableEvents:
				_traceHandle = buffer->HistoricalContext;
				_keywords = (EventTrace.Keyword)ClassicEtw.GetTraceEnableFlags(buffer->HistoricalContext);
				_level = (EventTrace.Level)ClassicEtw.GetTraceEnableLevel(buffer->HistoricalContext);
				_enabled = true;
				break;
			case ClassicEtw.WMIDPREQUESTCODE.DisableEvents:
				_enabled = false;
				_traceHandle = 0uL;
				_level = EventTrace.Level.LogAlways;
				_keywords = (EventTrace.Keyword)0;
				break;
			default:
				_enabled = false;
				_traceHandle = 0uL;
				break;
			}
			return 0u;
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalException(ex))
			{
				throw;
			}
			return 0u;
		}
	}

	~ClassicTraceProvider()
	{
		ClassicEtw.UnregisterTraceGuids(_registrationHandle.Value);
	}

	internal unsafe override uint EventWrite(EventTrace.Event eventID, EventTrace.Keyword keywords, EventTrace.Level level, int argc, EventData* argv)
	{
		ClassicEtw.EVENT_HEADER eVENT_HEADER = default(ClassicEtw.EVENT_HEADER);
		eVENT_HEADER.Header.ClientContext = 0u;
		eVENT_HEADER.Header.Flags = 1179648u;
		eVENT_HEADER.Header.Guid = EventTrace.GetGuidForEvent(eventID);
		eVENT_HEADER.Header.Level = (byte)level;
		eVENT_HEADER.Header.Type = EventTrace.GetOpcodeForEvent(eventID);
		eVENT_HEADER.Header.Version = EventTrace.GetVersionForEvent(eventID);
		EventData* ptr = &eVENT_HEADER.Data;
		if (argc > 16)
		{
			argc = 16;
		}
		eVENT_HEADER.Header.Size = (ushort)(argc * sizeof(EventData) + 48);
		for (int i = 0; i < argc; i++)
		{
			ptr[i].Ptr = argv[i].Ptr;
			ptr[i].Size = argv[i].Size;
		}
		return ClassicEtw.TraceEvent(_traceHandle, &eVENT_HEADER);
	}
}
