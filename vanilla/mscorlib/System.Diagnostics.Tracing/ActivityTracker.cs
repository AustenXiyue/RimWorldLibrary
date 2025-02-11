using System.Security;
using System.Threading;

namespace System.Diagnostics.Tracing;

internal class ActivityTracker
{
	private class ActivityInfo
	{
		private enum NumberListCodes : byte
		{
			End = 0,
			LastImmediateValue = 10,
			PrefixCode = 11,
			MultiByte1 = 12
		}

		internal readonly string m_name;

		private readonly long m_uniqueId;

		internal readonly Guid m_guid;

		internal readonly int m_activityPathGuidOffset;

		internal readonly int m_level;

		internal readonly EventActivityOptions m_eventOptions;

		internal long m_lastChildID;

		internal int m_stopped;

		internal readonly ActivityInfo m_creator;

		internal readonly Guid m_activityIdToRestore;

		public Guid ActivityId => m_guid;

		public ActivityInfo(string name, long uniqueId, ActivityInfo creator, Guid activityIDToRestore, EventActivityOptions options)
		{
			m_name = name;
			m_eventOptions = options;
			m_creator = creator;
			m_uniqueId = uniqueId;
			m_level = ((creator != null) ? (creator.m_level + 1) : 0);
			m_activityIdToRestore = activityIDToRestore;
			CreateActivityPathGuid(out m_guid, out m_activityPathGuidOffset);
		}

		public static string Path(ActivityInfo activityInfo)
		{
			if (activityInfo == null)
			{
				return "";
			}
			return Path(activityInfo.m_creator) + "/" + activityInfo.m_uniqueId;
		}

		public override string ToString()
		{
			string text = "";
			if (m_stopped != 0)
			{
				text = ",DEAD";
			}
			return m_name + "(" + Path(this) + text + ")";
		}

		public static string LiveActivities(ActivityInfo list)
		{
			if (list == null)
			{
				return "";
			}
			return list.ToString() + ";" + LiveActivities(list.m_creator);
		}

		public bool CanBeOrphan()
		{
			if ((m_eventOptions & EventActivityOptions.Detachable) != 0)
			{
				return true;
			}
			return false;
		}

		[SecuritySafeCritical]
		private unsafe void CreateActivityPathGuid(out Guid idRet, out int activityPathGuidOffset)
		{
			fixed (Guid* outPtr = &idRet)
			{
				int whereToAddId = 0;
				if (m_creator != null)
				{
					whereToAddId = m_creator.m_activityPathGuidOffset;
					idRet = m_creator.m_guid;
				}
				else
				{
					int num = 0;
					num = Thread.GetDomainID();
					whereToAddId = AddIdToGuid(outPtr, whereToAddId, (uint)num);
				}
				activityPathGuidOffset = AddIdToGuid(outPtr, whereToAddId, (uint)m_uniqueId);
				if (12 < activityPathGuidOffset)
				{
					CreateOverflowGuid(outPtr);
				}
			}
		}

		[SecurityCritical]
		private unsafe void CreateOverflowGuid(Guid* outPtr)
		{
			for (ActivityInfo creator = m_creator; creator != null; creator = creator.m_creator)
			{
				if (creator.m_activityPathGuidOffset <= 10)
				{
					uint id = (uint)Interlocked.Increment(ref creator.m_lastChildID);
					*outPtr = creator.m_guid;
					if (AddIdToGuid(outPtr, creator.m_activityPathGuidOffset, id, overflow: true) <= 12)
					{
						break;
					}
				}
			}
		}

		[SecurityCritical]
		private unsafe static int AddIdToGuid(Guid* outPtr, int whereToAddId, uint id, bool overflow = false)
		{
			byte* ptr = (byte*)outPtr;
			byte* ptr2 = ptr + 12;
			ptr += whereToAddId;
			if (ptr2 <= ptr)
			{
				return 13;
			}
			if (0 < id && id <= 10 && !overflow)
			{
				WriteNibble(ref ptr, ptr2, id);
			}
			else
			{
				uint num = 4u;
				if (id <= 255)
				{
					num = 1u;
				}
				else if (id <= 65535)
				{
					num = 2u;
				}
				else if (id <= 16777215)
				{
					num = 3u;
				}
				if (overflow)
				{
					if (ptr2 <= ptr + 2)
					{
						return 13;
					}
					WriteNibble(ref ptr, ptr2, 11u);
				}
				WriteNibble(ref ptr, ptr2, 12 + (num - 1));
				if (ptr < ptr2 && *ptr != 0)
				{
					if (id < 4096)
					{
						*ptr = (byte)(192 + (id >> 8));
						id &= 0xFF;
					}
					ptr++;
				}
				while (0 < num)
				{
					if (ptr2 <= ptr)
					{
						ptr++;
						break;
					}
					*(ptr++) = (byte)id;
					id >>= 8;
					num--;
				}
			}
			*(uint*)((byte*)outPtr + (nint)3 * (nint)4) = *(uint*)outPtr + *(uint*)((byte*)outPtr + 4) + *(uint*)((byte*)outPtr + (nint)2 * (nint)4) + 1503500717;
			return (int)(ptr - (byte*)outPtr);
		}

		[SecurityCritical]
		private unsafe static void WriteNibble(ref byte* ptr, byte* endPtr, uint value)
		{
			if (*ptr != 0)
			{
				byte* intPtr = ptr++;
				*intPtr |= (byte)value;
			}
			else
			{
				*ptr = (byte)(value << 4);
			}
		}
	}

	private AsyncLocal<ActivityInfo> m_current;

	private bool m_checkedForEnable;

	private static ActivityTracker s_activityTrackerInstance = new ActivityTracker();

	private static long m_nextId = 0L;

	private const ushort MAX_ACTIVITY_DEPTH = 100;

	public static ActivityTracker Instance => s_activityTrackerInstance;

	private Guid CurrentActivityId => m_current.Value.ActivityId;

	public void OnStart(string providerName, string activityName, int task, ref Guid activityId, ref Guid relatedActivityId, EventActivityOptions options)
	{
		if (m_current == null)
		{
			if (m_checkedForEnable)
			{
				return;
			}
			m_checkedForEnable = true;
			Enable();
			if (m_current == null)
			{
				return;
			}
		}
		ActivityInfo value = m_current.Value;
		string text = NormalizeActivityName(providerName, activityName, task);
		TplEtwProvider log = TplEtwProvider.Log;
		if (log.Debug)
		{
			log.DebugFacilityMessage("OnStartEnter", text);
			log.DebugFacilityMessage("OnStartEnterActivityState", ActivityInfo.LiveActivities(value));
		}
		if (value != null)
		{
			if (value.m_level >= 100)
			{
				activityId = Guid.Empty;
				relatedActivityId = Guid.Empty;
				if (log.Debug)
				{
					log.DebugFacilityMessage("OnStartRET", "Fail");
				}
				return;
			}
			if ((options & EventActivityOptions.Recursive) == 0 && FindActiveActivity(text, value) != null)
			{
				OnStop(providerName, activityName, task, ref activityId);
				value = m_current.Value;
			}
		}
		long uniqueId = ((value != null) ? Interlocked.Increment(ref value.m_lastChildID) : Interlocked.Increment(ref m_nextId));
		relatedActivityId = EventSource.CurrentThreadActivityId;
		ActivityInfo activityInfo = new ActivityInfo(text, uniqueId, value, relatedActivityId, options);
		m_current.Value = activityInfo;
		activityId = activityInfo.ActivityId;
		if (log.Debug)
		{
			log.DebugFacilityMessage("OnStartRetActivityState", ActivityInfo.LiveActivities(activityInfo));
			log.DebugFacilityMessage1("OnStartRet", activityId.ToString(), relatedActivityId.ToString());
		}
	}

	public void OnStop(string providerName, string activityName, int task, ref Guid activityId)
	{
		if (m_current == null)
		{
			return;
		}
		string text = NormalizeActivityName(providerName, activityName, task);
		TplEtwProvider log = TplEtwProvider.Log;
		if (log.Debug)
		{
			log.DebugFacilityMessage("OnStopEnter", text);
			log.DebugFacilityMessage("OnStopEnterActivityState", ActivityInfo.LiveActivities(m_current.Value));
		}
		ActivityInfo activityInfo;
		ActivityInfo activityInfo2;
		do
		{
			ActivityInfo value = m_current.Value;
			activityInfo = null;
			activityInfo2 = FindActiveActivity(text, value);
			if (activityInfo2 == null)
			{
				activityId = Guid.Empty;
				if (log.Debug)
				{
					log.DebugFacilityMessage("OnStopRET", "Fail");
				}
				return;
			}
			activityId = activityInfo2.ActivityId;
			ActivityInfo activityInfo3 = value;
			while (activityInfo3 != activityInfo2 && activityInfo3 != null)
			{
				if (activityInfo3.m_stopped != 0)
				{
					activityInfo3 = activityInfo3.m_creator;
					continue;
				}
				if (activityInfo3.CanBeOrphan())
				{
					if (activityInfo == null)
					{
						activityInfo = activityInfo3;
					}
				}
				else
				{
					activityInfo3.m_stopped = 1;
				}
				activityInfo3 = activityInfo3.m_creator;
			}
		}
		while (Interlocked.CompareExchange(ref activityInfo2.m_stopped, 1, 0) != 0);
		if (activityInfo == null)
		{
			activityInfo = activityInfo2.m_creator;
		}
		m_current.Value = activityInfo;
		if (log.Debug)
		{
			log.DebugFacilityMessage("OnStopRetActivityState", ActivityInfo.LiveActivities(activityInfo));
			log.DebugFacilityMessage("OnStopRet", activityId.ToString());
		}
	}

	[SecuritySafeCritical]
	public void Enable()
	{
		if (m_current == null)
		{
			m_current = new AsyncLocal<ActivityInfo>(ActivityChanging);
		}
	}

	private ActivityInfo FindActiveActivity(string name, ActivityInfo startLocation)
	{
		for (ActivityInfo activityInfo = startLocation; activityInfo != null; activityInfo = activityInfo.m_creator)
		{
			if (name == activityInfo.m_name && activityInfo.m_stopped == 0)
			{
				return activityInfo;
			}
		}
		return null;
	}

	private string NormalizeActivityName(string providerName, string activityName, int task)
	{
		if (activityName.EndsWith("Start"))
		{
			activityName = activityName.Substring(0, activityName.Length - "Start".Length);
		}
		else if (activityName.EndsWith("Stop"))
		{
			activityName = activityName.Substring(0, activityName.Length - "Stop".Length);
		}
		else if (task != 0)
		{
			activityName = "task" + task;
		}
		return providerName + activityName;
	}

	private void ActivityChanging(AsyncLocalValueChangedArgs<ActivityInfo> args)
	{
		ActivityInfo activityInfo = args.CurrentValue;
		ActivityInfo previousValue = args.PreviousValue;
		if (previousValue != null && previousValue.m_creator == activityInfo && (activityInfo == null || previousValue.m_activityIdToRestore != activityInfo.ActivityId))
		{
			EventSource.SetCurrentThreadActivityId(previousValue.m_activityIdToRestore);
			return;
		}
		while (activityInfo != null)
		{
			if (activityInfo.m_stopped == 0)
			{
				EventSource.SetCurrentThreadActivityId(activityInfo.ActivityId);
				break;
			}
			activityInfo = activityInfo.m_creator;
		}
	}
}
