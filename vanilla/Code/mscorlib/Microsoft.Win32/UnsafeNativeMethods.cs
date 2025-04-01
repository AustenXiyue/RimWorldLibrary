using System;
using System.Diagnostics.Tracing;
using System.Runtime.InteropServices;
using System.Security;

namespace Microsoft.Win32;

[SuppressUnmanagedCodeSecurity]
[SecurityCritical]
internal static class UnsafeNativeMethods
{
	[SecurityCritical]
	[SuppressUnmanagedCodeSecurity]
	internal static class ManifestEtw
	{
		[SecurityCritical]
		internal unsafe delegate void EtwEnableCallback([In] ref Guid sourceId, [In] int isEnabled, [In] byte level, [In] long matchAnyKeywords, [In] long matchAllKeywords, [In] EVENT_FILTER_DESCRIPTOR* filterData, [In] void* callbackContext);

		internal struct EVENT_FILTER_DESCRIPTOR
		{
			public long Ptr;

			public int Size;

			public int Type;
		}

		internal enum ActivityControl : uint
		{
			EVENT_ACTIVITY_CTRL_GET_ID = 1u,
			EVENT_ACTIVITY_CTRL_SET_ID,
			EVENT_ACTIVITY_CTRL_CREATE_ID,
			EVENT_ACTIVITY_CTRL_GET_SET_ID,
			EVENT_ACTIVITY_CTRL_CREATE_SET_ID
		}

		internal enum EVENT_INFO_CLASS
		{
			BinaryTrackInfo,
			SetEnableAllKeywords,
			SetTraits
		}

		internal enum TRACE_QUERY_INFO_CLASS
		{
			TraceGuidQueryList,
			TraceGuidQueryInfo,
			TraceGuidQueryProcess,
			TraceStackTracingInfo,
			MaxTraceSetInfoClass
		}

		internal struct TRACE_GUID_INFO
		{
			public int InstanceCount;

			public int Reserved;
		}

		internal struct TRACE_PROVIDER_INSTANCE_INFO
		{
			public int NextOffset;

			public int EnableCount;

			public int Pid;

			public int Flags;
		}

		internal struct TRACE_ENABLE_INFO
		{
			public int IsEnabled;

			public byte Level;

			public byte Reserved1;

			public ushort LoggerId;

			public int EnableProperty;

			public int Reserved2;

			public long MatchAnyKeyword;

			public long MatchAllKeyword;
		}

		internal const int ERROR_ARITHMETIC_OVERFLOW = 534;

		internal const int ERROR_NOT_ENOUGH_MEMORY = 8;

		internal const int ERROR_MORE_DATA = 234;

		internal const int ERROR_NOT_SUPPORTED = 50;

		internal const int ERROR_INVALID_PARAMETER = 87;

		internal const int EVENT_CONTROL_CODE_DISABLE_PROVIDER = 0;

		internal const int EVENT_CONTROL_CODE_ENABLE_PROVIDER = 1;

		internal const int EVENT_CONTROL_CODE_CAPTURE_STATE = 2;

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
		[SecurityCritical]
		internal unsafe static extern uint EventRegister([In] ref Guid providerId, [In] EtwEnableCallback enableCallback, [In] void* callbackContext, [In][Out] ref long registrationHandle);

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
		[SecurityCritical]
		internal static extern uint EventUnregister([In] long registrationHandle);

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
		[SecurityCritical]
		internal unsafe static extern int EventWrite([In] long registrationHandle, [In] ref EventDescriptor eventDescriptor, [In] int userDataCount, [In] EventProvider.EventData* userData);

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
		[SecurityCritical]
		internal static extern int EventWriteString([In] long registrationHandle, [In] byte level, [In] long keyword, [In] string msg);

		internal unsafe static int EventWriteTransferWrapper(long registrationHandle, ref EventDescriptor eventDescriptor, Guid* activityId, Guid* relatedActivityId, int userDataCount, EventProvider.EventData* userData)
		{
			int num = EventWriteTransfer(registrationHandle, ref eventDescriptor, activityId, relatedActivityId, userDataCount, userData);
			if (num == 87 && relatedActivityId == null)
			{
				Guid empty = Guid.Empty;
				num = EventWriteTransfer(registrationHandle, ref eventDescriptor, activityId, &empty, userDataCount, userData);
			}
			return num;
		}

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
		[SuppressUnmanagedCodeSecurity]
		private unsafe static extern int EventWriteTransfer([In] long registrationHandle, [In] ref EventDescriptor eventDescriptor, [In] Guid* activityId, [In] Guid* relatedActivityId, [In] int userDataCount, [In] EventProvider.EventData* userData);

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
		[SuppressUnmanagedCodeSecurity]
		internal static extern int EventActivityIdControl([In] ActivityControl ControlCode, [In][Out] ref Guid ActivityId);

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
		[SuppressUnmanagedCodeSecurity]
		internal unsafe static extern int EventSetInformation([In] long registrationHandle, [In] EVENT_INFO_CLASS informationClass, [In] void* eventInformation, [In] int informationLength);

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
		[SuppressUnmanagedCodeSecurity]
		internal unsafe static extern int EnumerateTraceGuidsEx(TRACE_QUERY_INFO_CLASS TraceQueryInfoClass, void* InBuffer, int InBufferSize, void* OutBuffer, int OutBufferSize, ref int ReturnLength);
	}
}
