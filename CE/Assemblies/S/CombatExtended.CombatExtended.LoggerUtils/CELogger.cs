using System.Runtime.CompilerServices;
using Verse;

namespace CombatExtended.CombatExtended.LoggerUtils;

internal class CELogger
{
	private static readonly bool isInDebugMode;

	public static void Message(string message, bool showOutOfDebugMode = false, [CallerMemberName] string memberName = "")
	{
		if (isInDebugMode || showOutOfDebugMode)
		{
			Log.Message($"CE - {memberName}() ({Find.TickManager.TicksGame}) - {message}");
		}
	}

	public static void Warn(string message, bool showOutOfDebugMode = false, [CallerMemberName] string memberName = "")
	{
		if (isInDebugMode || showOutOfDebugMode)
		{
			Log.Warning($"CE - {memberName}() ({Find.TickManager.TicksGame}) - {message}");
		}
	}

	public static void Error(string message, bool showOutOfDebugMode = true, [CallerMemberName] string memberName = "")
	{
		if (isInDebugMode || showOutOfDebugMode)
		{
			Log.Error($"CE - {memberName}() ({Find.TickManager.TicksGame}) - {message}");
		}
	}

	public static void ErrorOnce(string message, bool showOutOfDebugMode = true, [CallerMemberName] string memberName = "", [CallerLineNumber] int sourceLineNumber = 0)
	{
		if (isInDebugMode || showOutOfDebugMode)
		{
			Log.ErrorOnce($"CE - {memberName}() ({Find.TickManager.TicksGame}) - {message}", sourceLineNumber);
		}
	}
}
